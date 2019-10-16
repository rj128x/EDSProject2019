using EDSProj.EDSWebService;
using Modes;
using Modes.BusinessLogic;
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

		public void modesConnect() {
			int index = 0;
			while (!ModesApiFactory.IsInitilized && index <= 10) {
				try {
					using (Impersonator imp = new Impersonator(MCSettings.Single.MCUser, "corp", MCSettings.Single.MCPassword)) {
						Logger.Info(String.Format("Подключение к MC. Попытка {0}", index));
						//ModesApiFactory.Initialize(MCSettings.Single.MCServer, MCSettings.Single.MCUser, MCSettings.Single.MCPassword);
						ModesApiFactory.Initialize(MCSettings.Single.MCServer);
						api = ModesApiFactory.GetModesApi();
					}
					/*Logger.Info(String.Format("Подключение к MC. Попытка {0}", index));
					ModesApiFactory.Initialize(MCSettings.Single.MCServer, "mc_votges", "mcV0tges");					
					api = ModesApiFactory.GetModesApi();*/
				} catch (Exception e) {
					Logger.Info(e.ToString());
				}
				index++;
			}
		}

		public MCServerReader(DateTime date, bool writeNPBR = true) {
			Logger.Info("Чтение ПБР за " + date.ToString());
			Date = date;
			AutooperData = new List<string>();
			Logger.Info("Connect MC");
			ProcessedPBRS = new Dictionary<int, MCPBRData>();

			try {
				modesConnect();
				SyncZone zone = SyncZone.First;

				IModesTimeSlice ts = null;
				ts = api.GetModesTimeSlice(date.Date.LocalHqToSystemEx(), zone
							, TreeContent.PGObjects /*только оборудование, по которому СО публикует ПГ(включая родителей)*/
							, false);


				bool ok = true;
				foreach (IGenObject obj in ts.GenTree) {
					ok = ok && getPlan(obj);
				}

				if (ok) {
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
				}
				//return;

				if (ok) {
					Logger.Info("Зaпись ПБР в Базу");
					foreach (MCPBRData pbr in ProcessedPBRS.Values) {
						try {
							pbr.CreateData();
							bool okWrite = false;
							int i = 0;
							while (!okWrite && i < 3) {
								Logger.Info(String.Format("Запись {0} ПБР. Попытка {1}", pbr.Item, i));
								okWrite = pbr.ProcessData();
								i++;
							}
						} catch {
							Logger.Info("Ошибка при записи ПБР в базу");
						}
					}
					Logger.Info("Запись номера ПБР");

					DateTime ds = DateTime.Now;
					DateTime de = this.Date.AddDays(1);
					List<ShadeSelector> sel = new List<ShadeSelector>();
					sel.Add(new ShadeSelector() {
						period = new TimePeriod() {
							from = new Timestamp() { second = EDSClass.toTS(ds) },
							till = new Timestamp() { second = EDSClass.toTS(de) }
						},
						pointId = new PointId() {
							iess = "NPBR.EDS@CALC"
						}
					});

					Logger.Info("Удаление номера ПБР");
					uint id = EDSClass.Client.requestShadesClear(EDSClass.AuthStr, sel.ToArray());
					ok =  EDSClass.ProcessQuery(id);

					if (ok) {
						List<Shade> shades = new List<Shade>();
						List<ShadeValue> vals = new List<ShadeValue>();
						vals.Add(new ShadeValue() {
							period = new TimePeriod() {
								from = new Timestamp() { second = EDSClass.toTS(ds) },
								till = new Timestamp() { second = EDSClass.toTS(de) }
							},
							quality = Quality.QUALITYGOOD,
							value = new PointValue() { av = NPBR, avSpecified = true }
						});
						shades.Add(new Shade() {
							pointId = new PointId() { iess = "NPBR.EDS@CALC" },
							values = vals.ToArray()
						});
						Logger.Info("Запись данных");
						id = EDSClass.Client.requestShadesWrite(EDSClass.AuthStr, shades.ToArray());
						ok = EDSClass.ProcessQuery(id);
					}
					

					
				}
			} catch (Exception e) {
				Logger.Info("Ошибка при получении ПБР с сервера MC " + e);
			} finally {
				ModesApiFactory.CloseConnection();
			}
		}

		public bool getPlan(IGenObject obj) {
			DateTime dt1 = Date.Date.LocalHqToSystemEx();
			DateTime dt0 = Date.Date.AddDays(1).LocalHqToSystemEx();
			modesConnect();
			IList<PlanValueItem> data = api.GetPlanValuesActual(dt1, dt0, obj);
			bool ok = true;
			if (data.Count > 0) {
				Logger.Info(String.Format("Обработка ПБР для {0}({1}) [{2}]", obj.Description, obj.Id, obj.Name));

				List<MCPBRData> pbrs = MCPBRData.getPBRS(obj.Id, obj.Name);

				foreach (MCPBRData pbr in pbrs) {
					if (ProcessedPBRS.ContainsKey(pbr.Item)) {
						Logger.Info(string.Format("Данные по коду {0} уже были считаны ", pbr.Item));
						continue;
					}
					if (pbr.DataSettings != null) {
						foreach (PlanValueItem item in data) {
							if (item.ObjFactor == 0) {
								pbr.AddValue(item.DT.SystemToLocalHqEx(), item.Value);
								string pt = item.Type.ToString().Replace("ПБР", "");
								int num = 0;
								try {
									num = Int32.Parse(pt);
								} catch { }
								NPBR = num;
							}
						}
						Logger.Info(String.Format("Получено {0} записей с {1} по {2} по объекту {3}", pbr.Data.Count, dt1.SystemToLocalHqEx(), dt0.SystemToLocalHqEx(), obj.Description));
						if (pbr.Data.Count > 10) {
							if (!ProcessedPBRS.ContainsKey(pbr.Item))
								ProcessedPBRS.Add(pbr.Item, pbr);
						} else {
							Logger.Info("Недостаточно данных");
							ok = false;
						}
						Logger.Info("===Данные считаны: " + (ok ? "Успешно" : "Ошибка"));
					} else {
						Logger.Info("===Ошибка при разборе полученного макета. Возможно изменение кодировки MC");
					}

				}

			}
			foreach (IGenObject ch in obj.Children) {
				bool ok2 = getPlan(ch);
				ok = ok && ok2;
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

        public void sendAutooperData()
        {
            try
            {
                string fn = "pbr-0000" + (NPBR < 10 ? "0" : "") + NPBR.ToString() + "-" + Date.ToString("yyyyMMdd") + ".csv";

                string body = String.Join("\r\n", AutooperData);
                /*TextWriter writer = new StreamWriter(fn, false, Encoding.ASCII);				
				foreach (string str in AutooperData) {
					writer.WriteLine(str);					
				}
				writer.Close();*/

                StreamWriter sw = new StreamWriter("c:/int/ftpfolder/"+fn, false);
                sw.WriteLine(body);
                sw.Close();

                //FileInfo file = new FileInfo("c:/int/ftpfolder/"+fn);
                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();

                mess.From = new MailAddress(Settings.Single.SMTPFrom);

                mess.Subject = "pbr-0000" + (NPBR < 10 ? "0" : "") + NPBR.ToString() + "-" + Date.ToString("yyyyMMdd");
                mess.Body = body;
                mess.To.Add(Settings.Single.AOMail);
                Logger.Info(Settings.Single.AOMail);
                //mess.Attachments.Add(new Attachment(fn));

                mess.SubjectEncoding = System.Text.Encoding.Default;
                mess.BodyEncoding = System.Text.Encoding.Default;
                mess.IsBodyHtml = false;
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Settings.Single.SMTPServer, Settings.Single.SMTPPort);
                client.EnableSsl = true;
                if (string.IsNullOrEmpty(Settings.Single.SMTPUser))
                {
                    client.UseDefaultCredentials = true;
                }
                else
                {
                    client.Credentials = new System.Net.NetworkCredential(Settings.Single.SMTPUser, Settings.Single.SMTPPassword, Settings.Single.SMTPDomain);
                }
                // Отправляем письмо
                client.Send(mess);
                Logger.Info("Данные в автооператор отправлены успешно");
                /*try
                {
                    file.Delete();
                }
                catch { };*/
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при отправке почты: {0}", e.ToString()), Logger.LoggerSource.server);
                Logger.Info("Данные в автооператор не отправлены");
            }
        }

    }
}
