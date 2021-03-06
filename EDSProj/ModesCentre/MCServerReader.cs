﻿using EDSProj.EDSWebService;
using Modes;
using Modes.BusinessLogic;
using Modes.Sdc;
using ModesApiExternal;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Tools;

namespace EDSProj.ModesCentre
{
    public class MCServerReader
    {
        //public Dictionary<int, MCPBRData> Data { get; set; }
        public DateTime Date { get; set; }
        public List<string> AutooperData { get; set; }
        public int NPBR { get; set; }
        protected IApiExternal api;
        public Dictionary<int, MCPBRData> ProcessedPBRS;

        public void modesConnect()
        {
            int index = 0;
            while (!ModesApiFactory.IsInitilized && index <= 10)
            {
                try
                {
                    using (Impersonator imp = new Impersonator(MCSettings.Single.MCUser, "corp", MCSettings.Single.MCPassword))
                    {
                        Logger.Info(String.Format("Подключение к MC. Попытка {0}", index));
                        //ModesApiFactory.Initialize(MCSettings.Single.MCServer, MCSettings.Single.MCUser, MCSettings.Single.MCPassword);
                        ModesApiFactory.Initialize(MCSettings.Single.MCServer);
                        api = ModesApiFactory.GetModesApi();
                    }
                    /*Logger.Info(String.Format("Подключение к MC. Попытка {0}", index));
					ModesApiFactory.Initialize(MCSettings.Single.MCServer, "mc_votges", "mcV0tges");					
					api = ModesApiFactory.GetModesApi();*/
                }
                catch (Exception e)
                {
                    Logger.Info(e.ToString());
                }
                index++;
            }
        }

        public MCServerReader(DateTime date, bool writeNPBR = true)
        {
            Logger.Info("Чтение ПБР за " + date.ToString());
            Date = date;
            AutooperData = new List<string>();
            Logger.Info("Connect MC");
            ProcessedPBRS = new Dictionary<int, MCPBRData>();

            try
            {
                modesConnect();
                SyncZone zone = SyncZone.First;

                IModesTimeSlice ts = null;
                ts = api.GetModesTimeSlice(date.Date.LocalHqToSystemEx(), zone
                            , TreeContent.AllObjects /*только оборудование, по которому СО публикует ПГ(включая родителей)*/
                            , false);



                bool ok = true;
                foreach (IGenObject obj in ts.GenTree)
                {
                    ok = ok && getPlan(obj);
                }

                /*if (ok) {
					if (ProcessedPBRS.Count != 8) {
						Logger.Info("Количество ПБР !=7. Отправка в автооператор не производится");
					} else {
						foreach (MCPBRData pbr in ProcessedPBRS.Values) {
							pbr.CreateData();
							pbr.addAutooperData(AutooperData);
						}
						Logger.Info("АО кол.строк " + AutooperData.Count);
						if (AutooperData.Count >=149) {
							Logger.Info("Отправка в АО кол.строк ");
							try {
								sendAutooperData();
								Logger.Info("Данный отправлены  в АО");
							} catch (Exception e) {
								Logger.Info("Ошибка при отправке данных в АО");
								Logger.Info(e.ToString());
							}
						} else {
							Logger.Info("Неверное количество данных в АО");
						}
					}
				}*/
                //return;

                if (ok)
                {
                    Logger.Info("Зaпись ПБР в Базу");
                    foreach (MCPBRData pbr in ProcessedPBRS.Values)
                    {
                        try
                        {
                            pbr.CreateData();
                            bool okWrite = false;
                            int i = 0;
                            while (!okWrite && i < 3)
                            {
                                Logger.Info(String.Format("Запись {0} ПБР. Попытка {1}", pbr.Item, i));
                                okWrite = pbr.ProcessData();
                                i++;
                            }
                        }
                        catch
                        {
                            Logger.Info("Ошибка при записи ПБР в базу");
                        }
                    }
                    Logger.Info("Запись номера ПБР");

                    DateTime ds = DateTime.Now;
                    DateTime de = this.Date.AddDays(1);
                    List<ShadeSelector> sel = new List<ShadeSelector>();
                    sel.Add(new ShadeSelector()
                    {
                        period = new TimePeriod()
                        {
                            from = new Timestamp() { second = EDSClass.toTS(ds) },
                            till = new Timestamp() { second = EDSClass.toTS(de) }
                        },
                        pointId = new PointId()
                        {
                            iess = "NPBR.EDS@CALC"
                        }
                    });

                    Logger.Info("Удаление номера ПБР");
                    uint id = EDSClass.Client.requestShadesClear(EDSClass.AuthStr, sel.ToArray());
                    ok = EDSClass.ProcessQuery(id);

                    if (ok)
                    {
                        List<Shade> shades = new List<Shade>();
                        List<ShadeValue> vals = new List<ShadeValue>();
                        vals.Add(new ShadeValue()
                        {
                            period = new TimePeriod()
                            {
                                from = new Timestamp() { second = EDSClass.toTS(ds) },
                                till = new Timestamp() { second = EDSClass.toTS(de) }
                            },
                            quality = Quality.QUALITYGOOD,
                            value = new PointValue() { av = NPBR, avSpecified = true }
                        });
                        shades.Add(new Shade()
                        {
                            pointId = new PointId() { iess = "NPBR.EDS@CALC" },
                            values = vals.ToArray()
                        });
                        Logger.Info("Запись данных");
                        id = EDSClass.Client.requestShadesWrite(EDSClass.AuthStr, shades.ToArray());
                        ok = EDSClass.ProcessQuery(id);
                    }



                }
            }
            catch (Exception e)
            {
                Logger.Info("Ошибка при получении ПБР с сервера MC " + e);
            }
            finally
            {
                ModesApiFactory.CloseConnection();
            }
        }

        public bool getPlan(IGenObject obj)
        {
            DateTime dt1 = Date.Date.LocalHqToSystemEx();
            DateTime dt0 = Date.Date.AddDays(1).LocalHqToSystemEx();
            modesConnect();


            MCPBRData pbr = MCPBRData.getPBR(obj.Id, obj.Name);

            bool ok = true;

            foreach (IGenObject ch in obj.Children)
            {
                bool ok2 = getPlan(ch);
                ok = ok && ok2;
            }

            if (pbr == null)
            {                
                return ok;
            }
            
            if (pbr.ReadNPRCH)
            {
                IList<IGenObject> objs = new List<IGenObject>();
                objs.Add(obj);
                api.RefreshGenObjects(objs, dt1, SyncZone.First);
                IVarParam vp = obj.GetVarParam("НПРЧ_уч");
                for (int i = 0; i < 24; i++)
                {
                    object val = vp.GetValue(i);
                    pbr.AddNPRCHValue(dt1.SystemToLocalHqEx().AddHours(i), (bool)val?1:0);                    
                }
                ProcessedPBRS.Add(pbr.Item, pbr);

            }


            


            if (pbr.ReadPBR)
            {
                IList<PlanValueItem> data = api.GetPlanValuesActual(dt1, dt0, obj);
                Logger.Info(String.Format("Обработка ПБР для {0}({1}) [{2}]", obj.Description, obj.Id, obj.Name));


                if (pbr.DataSettings != null)
                {
                    foreach (PlanValueItem item in data)
                    {
                        if (item.ObjFactor == 0)
                        {
                            pbr.AddValue(item.DT.SystemToLocalHqEx(), item.Value);
                            string pt = item.Type.ToString().Replace("ПБР", "");
                            int num = 0;
                            try
                            {
                                num = Int32.Parse(pt);
                            }
                            catch { }
                            NPBR = num;
                        }
                        else if (item.ObjFactor == 1 && pbr.DataSettings.WriteToEDSMinMax)
                        {
                            pbr.AddMinValue(item.DT.SystemToLocalHqEx().AddHours(-1), item.Value);
                        }
                        else if (item.ObjFactor == 2 && pbr.DataSettings.WriteToEDSMinMax)
                        {
                            pbr.AddMaxValue(item.DT.SystemToLocalHqEx().AddHours(-1), item.Value);
                        }
                    }
                    Logger.Info(String.Format("Получено {0} записей с {1} по {2} по объекту {3}", pbr.Data.Count, dt1.SystemToLocalHqEx(), dt0.SystemToLocalHqEx(), obj.Description));
                    if (pbr.Data.Count > 10)
                    {
                        if (!ProcessedPBRS.ContainsKey(pbr.Item))
                            ProcessedPBRS.Add(pbr.Item, pbr);
                    }
                    else
                    {
                        Logger.Info("Недостаточно данных");
                        ok = false;
                    }
                    Logger.Info("===Данные считаны: " + (ok ? "Успешно" : "Ошибка"));
                }
                else
                {
                    Logger.Info("===Ошибка при разборе полученного макета. Возможно изменение кодировки MC");
                }


            }
            return ok;
        }



        /*protected void sendAutooperData1() {
			string fn = "pbr-0000" + (NPBR < 10 ? "0" : "") + NPBR.ToString() + "-" + Date.ToString("yyyyMMdd") + ".csv";
			string body = String.Join("\r\n", AutooperData);
			try {
				StreamWriter sw = new StreamWriter(fn, false);
				sw.WriteLine(body);
				sw.Close();
				FTPClass.SendFile(fn);
			} catch { }

			try {
				FileInfo file = new FileInfo(fn);
				file.Delete();
			} catch { }

		}*/

        

    }
}
