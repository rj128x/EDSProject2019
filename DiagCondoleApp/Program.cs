using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EDSProj;
using EDSProj.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DiagCondoleApp
{

    class Program
    {
        protected static DateTime GetDate(string arg)
        {
            int val = 0;
            bool isInt = int.TryParse(arg, out val);
            if (isInt)
            {
                DateTime now = DateTime.Now;

                DateTime date = now.Date;

                date = date.AddHours(val);
                return date;
            }
            else
            {
                bool isDate;
                DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                isDate = DateTime.TryParse(arg, out date);
                if (isDate)
                {
                    return date;
                }
                else
                {
                    date = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                    if (DateTime.Now.Minute > 30)
                        date = date.AddMinutes(30);
                    return date;
                }
            }
        }
        public static void Main(string[] args)
        {
            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger("C:/diagLog", "diag");

            DateTime DateStart = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(-5);
            DateTime DateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
            int type = 0;
            if (args.Count() == 0)
            {
                Console.WriteLine("введите тип операции  : ");
                Console.WriteLine("считать данные-0:");
                Console.WriteLine("исправить перескающиеся данные-1:");
                Console.WriteLine("заполнить дополнительные данные-2:");
                Console.WriteLine("1-2-3 - 100:");
                Console.WriteLine("отчет по МНУ - 201:");
                Console.WriteLine("отчет по ДН - 202:");
                Console.WriteLine("отчет по ДН - 203:");
                Console.WriteLine("обновить точки БД - 300:");
                type = Int32.Parse(Console.ReadLine());
                if (type == 300)
                {
                    fillDBPoints();
                    return;
                }

                Console.WriteLine("Введите дату начала выгрузки: ");
                DateStart = DateTime.Parse(Console.ReadLine());
                Console.WriteLine("Введите дату конца выгрузки: ");
                DateEnd = DateTime.Parse(Console.ReadLine());
                
            }
            if (args.Count() == 3)
            {
                DateStart = GetDate(args[0]);
                DateEnd = GetDate(args[1]);
                type = Int32.Parse(args[2]);
                if (type == 300)
                {
                    fillDBPoints();
                    return;
                }
            }
             


            Task<bool> res = run(DateStart, DateEnd, type);
            res.Wait();

            

            /*fillDBPoints();
            Console.ReadLine();*/

        }

        public static async Task<bool> run(DateTime DateStart, DateTime DateEnd, int type)
        {
            bool ok = true;
            switch (type)
            {
                case 0:
                    ok = await process(DateStart, DateEnd);
                    Logger.Info(ok.ToString());
                    break;
                case 1:
                    ok = PuskStopReader.CheckCrossData(DateStart, DateEnd);
                    Logger.Info(ok.ToString());
                    break;
                case 2:
                    ok = await PuskStopReader.FillAnalogData(DateStart, DateEnd, (new string[] { "DN", "LN" }).ToList());
                    Logger.Info(ok.ToString());
                    break;
                case 100:
                    ok = await process(DateStart, DateEnd);
                    Logger.Info(ok.ToString());
                    PuskStopReader.CheckCrossData(DateStart, DateEnd);
                    ok = await PuskStopReader.FillAnalogData(DateStart, DateEnd, (new string[] { "DN", "LN" }).ToList());
                    Logger.Info(ok.ToString());
                    break;
                case 201:
                    CreateReport(DateStart, DateEnd, "MNU", "Насосы МНУ");
                    break;
                case 202:
                    CreateReport(DateStart, DateEnd, "DN", "Дренажные насосы");
                    break;
                case 203:
                    CreateReport(DateStart, DateEnd, "LN", "Лекажные насосы");
                    break;
            }
            return ok;

        }




        public static object getVal(double val)
        {
            if (Double.IsNaN(val) || double.IsInfinity(val))
                return "";
            else
                return val;
        }

        protected static void fillVerticalDouble(IXLWorksheet sheet, int col, string title, List<double> data, double div = 1)
        {
            sheet.Cell(1, col).Value = title;
            int row = 2;
            foreach (double d in data)
            {
                sheet.Cell(row++, col).Value = d / div;
            }
        }
        protected static void fillHorDouble(IXLWorksheet sheet, int row, string title, List<double> data, double div = 1)
        {
            sheet.Cell(row, 1).Value = title;
            int col = 2;
            foreach (double d in data)
            {
                sheet.Cell(row, col).Value = d / div;
                col++;
            }
        }

        public static async void CreateReport(DateTime dateStart, DateTime dateEnd, string type, string Header)
        {
            int nasosCount = type == "MNU" ? 3 : 2;
            string typeRunGG = "GG_RUN";


            
            string FileNameFull = String.Format("{0}/test{1}.xlsx", Settings.Single.DiagFolder,type);
            XLWorkbook wbFull = new XLWorkbook();
            IXLWorksheet sheetSvodGG = null;
            int stepDays = 7;
            
            string[] GGArrStr = (Settings.Single.DiagNewGG + "~" + Settings.Single.DiagOldGG).Split('~');            
            //int[] GGArr = new int[] { 1, 3, 4, 5, 7, 8, 10,  2, 6,  9 };            
            for (int ggInd = 1; ggInd <= 10; ggInd++)
            {
                int ggNum = Int32.Parse(GGArrStr[ggInd - 1]);
                if (!Settings.Single.DiagSettings[ggNum - 1].activeReport)
                    continue;
                string FileNameTem = String.Format("{0}/template3.xlsx", Settings.Single.DiagFolder);
                string FileName = String.Format("{0}/test{1}_GG{2}.xlsx",Settings.Single.DiagFolder, type, ggNum);

                try
                {
                    File.Delete(FileName);
                    File.Copy(FileNameTem, FileName);
                }
                catch { }

                XLWorkbook wb = new XLWorkbook(FileName);

                IXLWorksheet sheetTemp = null;
                bool ok = wb.TryGetWorksheet("GGTemplate", out sheetTemp);
                if (nasosCount == 2)
                {
                    sheetTemp.Range(1, 18, 20, 30).Clear();
                    sheetTemp.Range(1, 4, 1, 17).Merge();
                }
                IXLWorksheet sheet = sheetTemp.CopyTo(String.Format("GG {0}", ggNum));
                
                if (ggInd == 1)
                {
                    sheetSvodGG = sheetTemp.CopyTo(wbFull, "Свод");
                    sheetSvodGG.Cell(1, 1).Value = "ГГ";
                    sheetSvodGG.Cell(1, 4).Value = "ГГ в работе или простое";
                    sheetSvodGG.Range(sheetSvodGG.Cell(1, 1), sheetSvodGG.Cell(4, 100)).CopyTo(sheetSvodGG.Cell(4 + 13, 1));
                    sheetSvodGG.Range(sheetSvodGG.Cell(1, 1), sheetSvodGG.Cell(4, 100)).CopyTo(sheetSvodGG.Cell(8 + 25, 1));
                    sheetSvodGG.Cell(4 + 13, 4).Value = "ГГ в работе";
                    sheetSvodGG.Cell(8 + 25, 4).Value = "ГГ в простое";
                }
                sheetTemp.Delete();
                IXLWorksheet sheetSvod = wb.AddWorksheet("GG SVOD");

                int RowCount = (int)((dateEnd - dateStart).TotalDays / stepDays + 7);

                sheet.Range(sheet.Cell(1, 1), sheet.Cell(4, 100)).CopyTo(sheet.Cell(RowCount + 1, 1));
                sheet.Range(sheet.Cell(1, 1), sheet.Cell(4, 100)).CopyTo(sheet.Cell(RowCount * 2 + 1, 1));

                sheet.Cell(1, 4).Value = "ГГ в работе или простое";
                sheet.Cell(RowCount + 1, 4).Value = "ГГ в работе";
                sheet.Cell(RowCount * 2 + 1, 4).Value = "ГГ в простое";

                DateTime ds = dateStart.AddSeconds(0);

                int rowIndex = 0;
                int weekNumber = 0;
                while (ds < dateEnd)
                {


                    DateTime de = ds.AddDays(7);
                    if (type == "MNU" && ds.Year >= 2020)
                        typeRunGG = "GG_UST";
                    else
                        typeRunGG = "GG_RUN";

                    Logger.Info(String.Format("{0}: {1}", ggNum, de));
                    DiagNasos diag = new DiagNasos(ds, de, ggNum);

                    diag.ReadData(type, nasosCount, typeRunGG);
                    Logger.Info("read");

                    ok = wb.TryGetWorksheet("GGFullData", out sheetTemp);
                    IXLWorksheet shFull = sheetTemp.CopyTo(String.Format("GG{0}_{1}", ggNum, ds.ToString("ddMM")));


                    int row = 2;
                    foreach (PuskStopData pd in diag.FullNasosData)
                    {
                        row++;
                        shFull.Cell(row, 1).Value = pd.TimeOn;
                        shFull.Cell(row, 2).Value = pd.TimeOff;
                        shFull.Cell(row, 3).Value = pd.TypeData;
                        shFull.Cell(row, 4).Value = pd.ValueStart;
                        shFull.Cell(row, 5).Value = pd.ValueEnd;
                        if (pd.PrevRecord != null)
                            shFull.Cell(row, 6).Value = pd.PrevRecord.TimeOff;
                        /*if (pd.NextRecord != null)
                            shFull.Cell(row, 7).Value = pd.NextRecord.TimeOn;*/
                        shFull.Cell(row, 8).Value = pd.Comment;
                        shFull.Cell(row, 9).Value = (pd.TimeOff - pd.TimeOn).TotalSeconds / 60;
                        if (pd.PrevRecord != null)
                            shFull.Cell(row, 10).Value = -(pd.PrevRecord.TimeOff - pd.TimeOn).TotalSeconds / 60;
                    }


                    sheetSvod.Cell(RowCount * 1 - 1, 1).Value = "ГГ в работе. время простоя насосов";
                    fillHorDouble(sheetSvod, RowCount * 1 + weekNumber, "неделя " + ds.ToString(), diag.NasosRunGG["SVOD"].StayInfo.sorted, 60);
                    sheetSvod.Cell(RowCount * 2 - 1, 1).Value = "ГГ в работе. время работы насосов";
                    fillHorDouble(sheetSvod, RowCount * 2 + weekNumber, "неделя " + ds.ToString(), diag.NasosRunGG["SVOD"].RunInfo.sorted, 60);
                    sheetSvod.Cell(RowCount * 3 - 1, 1).Value = "ГГ в работе. скорость набора уровня (потери давления)";
                    fillHorDouble(sheetSvod, RowCount * 3 + weekNumber, "неделя " + ds.ToString(), diag.NasosRunGG["SVOD"].VInfo.sorted);

                    sheetSvod.Cell(RowCount * 4 - 1, 1).Value = "ГГ в простое. время простоя насосов";
                    fillHorDouble(sheetSvod, RowCount * 4 + weekNumber, "неделя " + ds.ToString(), diag.NasosStopGG["SVOD"].StayInfo.sorted, 60);
                    sheetSvod.Cell(RowCount * 5 - 1, 1).Value = "ГГ в простое. время работы насосов";
                    fillHorDouble(sheetSvod, RowCount * 5 + weekNumber, "неделя " + ds.ToString(), diag.NasosStopGG["SVOD"].RunInfo.sorted, 60);
                    sheetSvod.Cell(RowCount * 6 - 1, 1).Value = "ГГ в простое. скорость набора уровня (потери давления)";
                    fillHorDouble(sheetSvod, RowCount * 6 + weekNumber, "неделя " + ds.ToString(), diag.NasosStopGG["SVOD"].VInfo.sorted);




                    for (int i = 0; i < 3; i++)
                    {
                        Dictionary<string, AnalizeNasosData> data = i == 0 ? diag.NasosGG : i == 1 ? diag.NasosRunGG : diag.NasosStopGG;

                        row = 5 + RowCount * i + rowIndex;

                        sheet.Cell(row, 1).Value = ds;
                        sheet.Cell(row, 2).Value = diag.timeGGRun / 3600;
                        sheet.Cell(row, 2).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 3).Value = diag.timeGGStop / 3600;
                        sheet.Cell(row, 3).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 4).Value = getVal(data["SVOD"].RunInfo.Count);
                        sheet.Cell(row, 4).Style.NumberFormat.Format = "0";
                        sheet.Cell(row, 5).Value = getVal(data["SVOD"].RunInfo.Count / data["SVOD"].sumTime * 3600);
                        sheet.Cell(row, 5).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 6).Value = getVal(data["SVOD"].RunInfo.MathO / 60);
                        sheet.Cell(row, 6).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 7).Value = getVal(data["SVOD"].StayInfo.MathO / 60);
                        sheet.Cell(row, 7).Style.NumberFormat.Format = "0.0";
                        double sumRun = data["SVOD"].RunInfo.filtered.Sum();
                        double sumStay = data["SVOD"].StayInfo.filtered.Sum();
                        if (sumStay > 0)
                        {
                            sheet.Cell(row, 8).Value = getVal(sumRun / sumStay);
                            sheet.Cell(row, 8).Style.NumberFormat.Format = "0.00";
                        }
                        sheet.Cell(row, 9).Value = getVal(data["SVOD"].VInfo.MathO * 3600);
                        sheet.Cell(row, 9).Style.NumberFormat.Format = "0.00";

                        int col = 9;

                        for (int nasos = 1; nasos <= nasosCount; nasos++)
                        {
                            AnalizeNasosData nd = data[String.Format("{0}_{1}", type, nasos)];
                            sheet.Cell(row, col + 1).Value = getVal(nd.RunInfo.Count);
                            sheet.Cell(row, col + 1).Style.NumberFormat.Format = "0";
                            if (data["SVOD"].RunInfo.Count > 0)
                                sheet.Cell(row, col + 2).Value = getVal(nd.RunInfo.Count * 100.0 / data["SVOD"].RunInfo.Count);
                            sheet.Cell(row, col + 2).Style.NumberFormat.Format = "0.0";

                            sheet.Cell(row, col + 3).Value = getVal(nd.RunInfo.MathO / 60.0);
                            sheet.Cell(row, col + 3).Style.NumberFormat.Format = "0.0";

                            sumRun = nd.RunInfo.filtered.Sum();
                            sumStay = data["SVOD"].StayInfo.filtered.Sum() - sumRun;
                            if (sumStay > 0)
                            {
                                sheet.Cell(row, col + 4).Value = getVal(sumRun / sumStay);
                                sheet.Cell(row, col + 4).Style.NumberFormat.Format = "0.00";
                            }

                            col += 4;
                        }
                        if (de >= dateEnd)
                        {
                            int rowdesc = (i + 1) * 4 + i * 12 + ggInd;
                            sheet.Range(row, 2, row, 100).CopyTo(sheetSvodGG.Cell(rowdesc , 2));
                            sheetSvodGG.Cell(rowdesc, 1).Value = String.Format("ГГ-{0}", ggNum);
                        }

                    }
                    ds = de.AddSeconds(0);
                    rowIndex++;
                    weekNumber++;
                }
                sheetTemp.Delete();
                sheet.CopyTo(wbFull, sheet.Name);
                wb.SaveAs(FileName);

            }
            wbFull.SaveAs(FileNameFull);
            //sendDiagData(FileNameFull,Header);
        }

        public static async Task<bool> process(DateTime dateStart, DateTime dateEnd)
        {
            DateTime date = dateStart.AddSeconds(0); ;
            DateTime start = DateTime.Now;
            DiagDBEntities diagDB = new DiagDBEntities();
            Dictionary<int, List<PuskStopReader.PuskStopReaderRecord>> requestsDict = new Dictionary<int, List<PuskStopReader.PuskStopReaderRecord>>();
            for (int gg = 1; gg <= 10; gg++)
            {
                if (!Settings.Single.DiagSettings[gg - 1].activeReport)
                    continue;
                List<PuskStopReader.PuskStopReaderRecord> request = new List<PuskStopReader.PuskStopReaderRecord>();
                IEnumerable<PuskStopPoint> req = from p in diagDB.PuskStopPoints where p.gg == gg && p.analog == false select p;
                foreach (PuskStopPoint pt in req)
                {
                    PuskStopReader.PuskStopReaderRecord rec = new PuskStopReader.PuskStopReaderRecord();
                    rec.DBRecord = pt.pointType;
                    rec.iess = pt.point;
                    rec.inverted = pt.inverted;
                    rec.gg = pt.gg;
                    IEnumerable<PuskStopPoint> req2 = from p in diagDB.PuskStopPoints where p.gg == gg && p.analog == true && pt.pointType.Contains(p.pointType) select p;
                    if (req2.Count() > 0)
                    {
                        PuskStopPoint inner = req2.First();
                        rec.ValueIess = inner.point;
                    }
                    request.Add(rec);
                }
                requestsDict.Add(gg, request);
            }

            while (date < dateEnd)
            {
                Logger.Info(date.ToString());
                for (int gg = 1; gg <= 10; gg++)
                {
                    if (!Settings.Single.DiagSettings[gg - 1].activeReport)
                        continue;
                    if (date < DateTime.Parse(Settings.Single.DiagSettings[gg - 1].dateStartRead))
                        continue;
                    /*if (gg == 10) continue;
                    if (gg == 8 && date < DateTime.Parse("01.07.2022")) continue;
                    if (gg == 3 && date < DateTime.Parse("07.05.2020")) continue;
                    if (gg == 5 && date < DateTime.Parse("01.06.2019"))
                        continue;
                    if (gg == 1 && date < DateTime.Parse("21.04.2021"))
                        continue;*/
                    Logger.Info(String.Format("ГГ {0} Дата {1}", gg, date));
                    List<PuskStopReader.PuskStopReaderRecord> request = requestsDict[gg];

                    bool ok = await PuskStopReader.FillPuskStopData(request, date, date.AddHours(3));

                }
                date = date.AddHours(3);
            }
            DateTime end = DateTime.Now;
            Logger.Info((end - start).TotalMinutes.ToString());
            Logger.Info("Finish");
            return true;
        }

        public static async void fillDBPoints()
        {

            List<PuskStopReader.PuskStopReaderRecord> request = new List<PuskStopReader.PuskStopReaderRecord>();
            string[] newGG=Settings.Single.DiagNewGG.Split('~');
            foreach (string gg in newGG)
            {
                int ggInt = Int32.Parse(gg);
                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "GG_RUN",
                    iess = gg + "VT_GC02D-45.MCR@GRARM",
                    inverted = true,
                    gg = ggInt
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "GG_STOP",
                    iess = gg + "VT_GC02D-45.MCR@GRARM",
                    inverted = false,
                    gg = ggInt
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "GG_UST",
                    iess = gg + "VT_GC03D-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "MNU_1",
                    iess = gg + "VT_PS01DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00A-01.MCR@GRARM"
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "MNU_2",
                    iess = gg + "VT_PS02DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00A-01.MCR@GRARM"

                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "MNU_3",
                    iess = gg + "VT_PS03DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00A-01.MCR@GRARM"
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "LN_1",
                    iess = gg + "VT_PS04DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00AI-07.MCR@GRARM"
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "LN_2",
                    iess = gg + "VT_PS05DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00AI-07.MCR@GRARM"

                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "DN_1",
                    iess = gg + "VT_DS01DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_DS00AI-01.MCR@GRARM"
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "DN_2",
                    iess = gg + "VT_DS02DI-01.MCR@GRARM",
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_DS00AI-01.MCR@GRARM"
                });

            }

            string[] oldGG = Settings.Single.DiagOldGG.Split('~');
            foreach (string gg in oldGG)
            {
                int ggInt = Int32.Parse(gg);
                string suffix = "";
                suffix = ggInt <= 2 ? ".UNIT01@BLOCK1" : ggInt <= 6 ? ".UNIT05@BLOCK3" : ggInt <= 8 ? ".UNIT07@BLOCK4" : ".UNIT09@BLOCK5";


                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "GG_RUN",
                    iess = gg + "VT_GC00D-115" + suffix,
                    inverted = true,
                    gg = ggInt
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "GG_STOP",
                    iess = gg + "VT_GC00D-115" + suffix,
                    inverted = false,
                    gg = ggInt
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "GG_UST",
                    iess = gg + "VT_GC05D-01" + suffix,
                    inverted = false,
                    gg = ggInt
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "MNU_1",
                    iess = gg + "VT_PS01DI-03" + suffix,
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00A-11" + suffix
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "MNU_2",
                    iess = gg + "VT_PS02DI-03" + suffix,
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00A-11" + suffix
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "MNU_3",
                    iess = gg + "VT_PS03DI-03" + suffix,
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS00A-11" + suffix
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "LN_1",
                    iess = gg + "VT_PS04DI-03" + suffix,
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_PS04AI-01" + suffix
                });


                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "DN_1",
                    iess = gg + "VT_DS01DI-03" + suffix,
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_DS00AI-01" + suffix
                });

                request.Add(new PuskStopReader.PuskStopReaderRecord()
                {
                    DBRecord = "DN_2",
                    iess = gg + "VT_DS02DI-03" + suffix,
                    inverted = false,
                    gg = ggInt,
                    ValueIess = gg + "VT_DS00AI-01" + suffix
                });

            }
            DiagDBEntities diagDB = new DiagDBEntities();
            foreach (PuskStopReader.PuskStopReaderRecord rec in request)
            {
                string ggStr = rec.iess.Substring(0, 2);
                int gg = Int32.Parse(ggStr);
                string typeAnalog = "---";
                if (!string.IsNullOrEmpty(rec.ValueIess))
                {
                    int pos = rec.DBRecord.IndexOf("_");
                    typeAnalog = rec.DBRecord.Substring(0, pos);
                }
                IEnumerable<PuskStopPoint> req = from p in diagDB.PuskStopPoints where p.gg == gg && (p.pointType == rec.DBRecord || p.pointType == typeAnalog) select p;
                if (req.Count() > 0)
                {
                    foreach (PuskStopPoint p in req)
                    {
                        diagDB.PuskStopPoints.Remove(p);
                    }
                    diagDB.SaveChanges();
                }

                PuskStopPoint pt = new PuskStopPoint();
                pt.gg = gg;
                pt.inverted = rec.inverted;
                pt.pointType = rec.DBRecord;
                pt.point = rec.iess;
                pt.analog = false;
                diagDB.PuskStopPoints.Add(pt);

                if (!string.IsNullOrEmpty(rec.ValueIess))
                {
                    int pos = rec.DBRecord.IndexOf("_");
                    typeAnalog = rec.DBRecord.Substring(0, pos);
                    pt = new PuskStopPoint();
                    pt.gg = gg;
                    pt.inverted = false;
                    pt.analog = true;
                    pt.pointType = typeAnalog;
                    pt.point = rec.ValueIess;
                    diagDB.PuskStopPoints.Add(pt);
                }
                diagDB.SaveChanges();

            }

        }

        public static void sendDiagData(string FileName,string Header)
        {
            try
            {



                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();

                mess.From = new MailAddress(Settings.Single.SMTPFrom);

                mess.Subject = "Диагностика. "+Header;
                mess.Body = "диагностика работы насосов";
                string[] messTo = Settings.Single.DiagMail.Split(new char[] { ';' });
                foreach (string mt in messTo)
                    mess.To.Add(mt);
                mess.Attachments.Add(new Attachment(FileName));

                mess.SubjectEncoding = System.Text.Encoding.UTF8;
                mess.BodyEncoding = System.Text.Encoding.UTF8;
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
                Logger.Info("Данные в  отправлены успешно");

            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при отправке почты: {0}", e.ToString()), Logger.LoggerSource.server);
                Logger.Info("Данные в  не отправлены");
            }
        }


    }
}
