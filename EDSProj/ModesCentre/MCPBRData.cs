﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using EDSProj.ModesCentre;
using EDSProj.EDSWebService;

namespace EDSProj.ModesCentre
{
    public class MCPBRData
    {
        public SortedList<DateTime, double> Data { get; set; }
        public SortedList<DateTime, double> DataMin { get; set; }
        public SortedList<DateTime, double> DataMax { get; set; }
        public SortedList<DateTime, double> DataNPRCH { get; set; }
        public SortedList<DateTime, double> DataHH { get; set; }
        public SortedList<DateTime, double> DataSmooth { get; set; }
        public int Item { get; set; }
        public string PointName { get; set; }
        public string PointNameMin { get; set; }
        public string PointNameMax { get; set; }

        public string SmoothPointName { get; set; }
        public MCSettingsRecord DataSettings;

        public bool ReadPBR { get; set; }        
        public bool ReadNPRCH { get; set; }


        public MCPBRData(MCSettingsRecord rec)
        {
            Item = rec.PiramidaCode;
            PointName = rec.EDSPoint;
            PointNameMin = rec.EDSPointMin;
            PointNameMax = rec.EDSPointMax;
            SmoothPointName = rec.EDSPointSmooth;
            Logger.Info(rec.EDSPoint);
            DataSettings = rec;

            ReadPBR = rec.ReadPBR;
            ReadNPRCH = rec.ReadNPRCH;

            Data = new SortedList<DateTime, double>();
            DataMin = new SortedList<DateTime, double>();
            DataMax = new SortedList<DateTime, double>();
            DataHH = new SortedList<DateTime, double>();
            DataNPRCH = new SortedList<DateTime, double>();
            DataSmooth = new SortedList<DateTime, double>();
        }

        public static MCPBRData getPBR(int mcCode, string mcName)
        {

            foreach (MCSettingsRecord rec in MCSettings.Single.MCData)
            {
                if (rec.MCName.ToLower() == mcName.ToLower())
                {
                    MCPBRData pbr = new MCPBRData(rec);
                    return pbr;
                }
            }

            Logger.Info("Ошибка при разборе полученного макета. Возможно изменение кодировки MC");

            return null; ;
        }

        public void AddValue(DateTime date, double val)
        {
            try
            {
                Data.Add(date, val);
            }
            catch (Exception e)
            {
                Logger.Info("ошибка при разборе пбр. дубль в исходных днных " + e);
            }
        }

        public void AddMinValue(DateTime date, double val)
        {
            try
            {
                DataMin.Add(date, val);
            }
            catch (Exception e)
            {
                Logger.Info("ошибка при разборе пбр мин. дубль в исходных днных " + e);
            }
        }

        public void AddMaxValue(DateTime date, double val)
        {
            try
            {
                DataMax.Add(date, val);
            }
            catch (Exception e)
            {
                Logger.Info("ошибка при разборе пбр макс. дубль в исходных днных " + e);
            }
        }

        public void AddNPRCHValue(DateTime date, double val)
        {
            try
            {
                DataNPRCH.Add(date, val);
            }
            catch (Exception e)
            {
                Logger.Info("ошибка при разборе  нпрч. дубль в исходных днных " + e);
            }
        }

        protected SortedList<DateTime, double> createInegratedData()
        {
            SortedList<DateTime, double> integr = new SortedList<DateTime, double>();
            int index = 0;

            foreach (KeyValuePair<DateTime, double> de in Data)
            {
                DateTime nextDate = index < Data.Count - 1 ? Data.Keys[index + 1] : Data.Last().Key;
                if (de.Key == nextDate)
                    break;
                DateTime dt = de.Key.AddMinutes(0);
                int i = 0;
                while (dt < nextDate)
                {
                    double sum = de.Value + (Data[nextDate] - de.Value) / 60 * i;
                    integr.Add(dt, sum);
                    dt = dt.AddMinutes(1);
                    i++;
                }
                index++;
            }
            return integr;
        }

        protected SortedList<DateTime, double> createHHData()
        {
            SortedList<DateTime, double> hh = new SortedList<DateTime, double>();
            int index = 0;
            foreach (KeyValuePair<DateTime, double> de in Data)
            {
                hh.Add(de.Key, de.Value);
                DateTime nextDate = index < Data.Count - 1 ? Data.Keys[index + 1] : Data.Last().Key;
                if (de.Key == nextDate)
                    break;
                DateTime dt = de.Key.AddMinutes(30);
                double val = (de.Value + Data[nextDate]) / 2;
                hh.Add(dt, val);
                index++;
            }
            return hh;
        }

        protected SortedList<DateTime, double> createSmoothData()
        {
            SortedList<DateTime, double> hh = new SortedList<DateTime, double>();
            int index = 0;
            foreach (KeyValuePair<DateTime, double> de in Data)
            {
                hh.Add(de.Key, de.Value);
                DateTime nextDate = index < Data.Count - 1 ? Data.Keys[index + 1] : Data.Last().Key;
                if (de.Key == nextDate)
                    break;
                DateTime d1 = de.Key.AddMinutes(0);
                DateTime d2 = nextDate.AddMinutes(0);
                TimeSpan ts = d2 - d1;
                int mins = (int)ts.TotalMinutes;
                double v2 = Data[d2];
                double v1 = Data[d1];
                for (int i = 0; i < mins; i++)
                {
                    DateTime d = d1.AddMinutes(i);
                    double val = v1 + (v2 - v1) / mins * i;
                    if (!hh.ContainsKey(d))
                    {
                        hh.Add(d, val);
                    }
                }
                index++;
            }
            return hh;
        }

        protected SortedList<DateTime, double> createHH15Data()
        {
            SortedList<DateTime, double> hh = new SortedList<DateTime, double>();
            int index = 0;
            foreach (KeyValuePair<DateTime, double> de in Data)
            {
                hh.Add(de.Key.AddMinutes(-15), de.Value);
                DateTime nextDate = index < Data.Count - 1 ? Data.Keys[index + 1] : Data.Last().Key;
                if (de.Key == nextDate)
                    break;
                DateTime dt = de.Key.AddMinutes(30);
                double val = (de.Value + Data[nextDate]) / 2;
                hh.Add(dt.AddMinutes(-15), val);
                index++;
            }
            return hh;
        }

        public bool writeToEDS(string pointName, SortedList<DateTime, double> data, bool data15, bool datamin, bool data60)
        {
            bool ok = false;
            try
            {
                //SortedList<DateTime, double> data15 = createHH15Data();
                if (!EDSClass.Connected)
                    EDSClass.Connect();

                List<ShadeSelector> sel = new List<ShadeSelector>();
                sel.Add(new ShadeSelector()
                {
                    period = new TimePeriod()
                    {
                        from = new Timestamp() { second = EDSClass.toTS(data.Keys.First().AddHours(2)) },
                        till = new Timestamp() { second = EDSClass.toTS(data.Keys.Last().AddHours(2)) }
                    },
                    pointId = new PointId()
                    {
                        iess = pointName
                    }
                });

                Logger.Info("Удаление записей");
                uint id = EDSClass.Client.requestShadesClear(EDSClass.AuthStr, sel.ToArray());
                ok = EDSClass.ProcessQuery(id);

                if (ok)
                {
                    List<Shade> shades = new List<Shade>();
                    List<ShadeValue> vals = new List<ShadeValue>();

                    if (data15)
                    {
                        foreach (KeyValuePair<DateTime, double> de in data)
                        {
                            DateTime d = de.Key.AddMinutes(0);
                            DateTime dEnd = de.Key.AddMinutes(30);
                            //dEnd = dEnd > data.Last().Key ? data.Last().Key : dEnd;
                            while (d < dEnd)
                            {
                                vals.Add(new ShadeValue()
                                {
                                    period = new TimePeriod()
                                    {
                                        from = new Timestamp() { second = EDSClass.toTS(d.AddHours(2)) },
                                        till = new Timestamp() { second = EDSClass.toTS(d.AddHours(2).AddMinutes(1)) }
                                    },
                                    quality = Quality.QUALITYGOOD,
                                    value = new PointValue() { av = (float)de.Value, avSpecified = true }
                                });
                                d = d.AddMinutes(1);
                            }
                        };
                    }
                    else if (datamin)
                    {
                        foreach (KeyValuePair<DateTime, double> de in data)
                        {
                            vals.Add(new ShadeValue()
                            {
                                period = new TimePeriod()
                                {
                                    from = new Timestamp() { second = EDSClass.toTS(de.Key.AddHours(2)) },
                                    till = new Timestamp() { second = EDSClass.toTS(de.Key.AddHours(2).AddMinutes(1)) }
                                },
                                quality = Quality.QUALITYGOOD,
                                value = new PointValue() { av = (float)de.Value, avSpecified = true }
                            });
                        };
                    }
                    else if (data60)
                    {
                        foreach (KeyValuePair<DateTime, double> de in data)
                        {
                            vals.Add(new ShadeValue()
                            {
                                period = new TimePeriod()
                                {
                                    from = new Timestamp() { second = EDSClass.toTS(de.Key.AddHours(2)) },
                                    till = new Timestamp() { second = EDSClass.toTS(de.Key.AddHours(3)) }
                                },
                                quality = Quality.QUALITYGOOD,
                                value = new PointValue() { av = (float)de.Value, avSpecified = true }
                            });
                        };
                    }
                    else
                    {
                        double sum = 0;
                        foreach (KeyValuePair<DateTime, double> de in data)
                        {
                            DateTime d = de.Key.AddMinutes(0);
                            sum += de.Value / 60;
                            vals.Add(new ShadeValue()
                            {
                                period = new TimePeriod()
                                {
                                    from = new Timestamp() { second = EDSClass.toTS(d.AddHours(2)) },
                                    till = new Timestamp() { second = EDSClass.toTS(d.AddHours(2).AddMinutes(1)) - 1 }
                                },
                                quality = Quality.QUALITYGOOD,
                                value = new PointValue() { av = (float)sum, avSpecified = true }
                            });
                            d = d.AddMinutes(1);
                        };
                    }
                    shades.Add(new Shade()
                    {
                        pointId = new PointId() { iess = pointName },
                        values = vals.ToArray()
                    });

                    Logger.Info("Запись данных");
                    id = EDSClass.Client.requestShadesWrite(EDSClass.AuthStr, shades.ToArray());
                    ok = EDSClass.ProcessQuery(id);
                }
            }
            catch (Exception e)
            {
                Logger.Info("Ошибка при записи ПБР в EDS " + e.ToString());
                ok = false;
            }
            return ok;
        }



        public void CreateData()
        {
            DataHH = createHHData();
        }

        public bool ProcessData()
        {
            try
            {
                bool ok = true;
                SortedList<DateTime, double> data15 = createHH15Data();
                SortedList<DateTime, double> smooth = createSmoothData();
                if (!String.IsNullOrEmpty(this.PointName) && DataSettings.WriteToEDS)
                {
                    Logger.Info("Запись в ЕДС данных ПБР");
                    ok = ok && this.writeToEDS(this.PointName, data15, true, false, false);
                }
                if (!String.IsNullOrEmpty(this.SmoothPointName) && DataSettings.WriteToEDS)
                {
                    Logger.Info("Запись в ЕДС данных ПБР (сглаж)");
                    ok = ok && this.writeToEDS(this.SmoothPointName, smooth, false, true, false);
                }
                if (DataSettings.WriteIntegratedData && !String.IsNullOrEmpty(DataSettings.IntegratedEDSPoint))
                {
                    SortedList<DateTime, double> integr = createInegratedData();
                    Logger.Info("Запись в ЕДС интегрированных данных");
                    ok = ok && this.writeToEDS(DataSettings.IntegratedEDSPoint, integr, false, false, false);
                }
                if (DataSettings.WriteToEDSMinMax && !String.IsNullOrEmpty(DataSettings.EDSPointMin) && !String.IsNullOrEmpty(DataSettings.EDSPointMax))
                {
                    Logger.Info("Запись в ЕДС данных ПБР (мин макс)");
                    ok = ok && this.writeToEDS(this.PointNameMin, DataMin, false, false, true);
                    ok = ok && this.writeToEDS(this.PointNameMax, DataMax, false, false, true);
                }

                if (DataSettings.ReadNPRCH && !String.IsNullOrEmpty(DataSettings.EDSPoint) )
                {
                    Logger.Info("Запись в ЕДС данных ПБР (мин макс)");
                    ok = this.writeToEDS(this.PointName, DataNPRCH, false, false, true);
                }
                return ok;
            }
            catch (Exception e)
            {
                Logger.Info("Ошибка при записи ПБР в базу ");
                Logger.Info(e.ToString());
                return false;
            }
        }
    }

}
