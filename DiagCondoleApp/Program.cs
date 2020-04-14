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
using System.Text;
using System.Threading.Tasks;

namespace DiagCondoleApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            /*DateTime DateStart = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(-5);
            DateTime DateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
            if (args.Count() == 0)
            {
                Console.WriteLine("Введите дату начала выгрузки: ");
                DateStart = DateTime.Parse(Console.ReadLine());
                Console.WriteLine("Введите дату конца выгрузки: ");
                DateEnd = DateTime.Parse(Console.ReadLine());
            }
            */

            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger("C:/diagLog", "diag");
            //process(DateStart, DateEnd);
            //PuskStopReader.CheckCrossData(DateStart, DateEnd);
            //PuskStopReader.RefreshPuskStopData(DateStart, DateEnd);
            CreateReport(DateTime.Parse("01.01.2020"), DateTime.Parse("13.04.2020"));
            //Console.ReadLine();
        }



        public static object getVal(double val)
        {
            if (Double.IsNaN(val) || double.IsInfinity(val))
                return "";
            else
                return val;
        }

        protected static void fillVerticalDouble(IXLWorksheet sheet, int col, string title, List<double> data)
        {
            sheet.Cell(1, col).Value = title;
            int row = 2;
            foreach (double d in data)
            {
                sheet.Cell(row++, col).Value = d;
            }
        }
        protected static void fillHorDouble(IXLWorksheet sheet, int row, string title, List<double> data)
        {
            sheet.Cell(row, 1).Value = title;
            int col = 2;
            foreach (double d in data)
            {
                sheet.Cell(row, col++).Value = d;
            }
        }

        public static async void CreateReport(DateTime dateStart, DateTime dateEnd)
        {
            int nasosCount = 3;
            string type = "MNU";
            string typeRunGG = "GG_UST";

            string FileNameFull = String.Format("D:/wrk/test{0}.xlsx", type);
            XLWorkbook wbFull = new XLWorkbook();


            for (int gg = 1; gg <= 10; gg++)
            {
                string FileNameTem = String.Format("D:/wrk/template3.xlsx", nasosCount);
                string FileName = String.Format("D:/wrk/test{0}_GG{1}.xlsx", type,gg);
                XLWorkbook wb = new XLWorkbook(FileNameTem);
                wb.SaveAs(FileName);

                IXLWorksheet sheetTemp = null;
                bool ok = wb.TryGetWorksheet("GGTemplate", out sheetTemp);

                IXLWorksheet sheet = sheetTemp.CopyTo(String.Format("GG {0}", gg));
                IXLWorksheet sheetSvod = wb.AddWorksheet("GG SVOD");

                int RowCount = (int)((dateEnd - dateStart).TotalDays / 7 + 7);

                sheet.Range(sheet.Cell(1, 1), sheet.Cell(4, 100)).CopyTo(sheet.Cell(RowCount + 1, 1));
                sheet.Range(sheet.Cell(1, 1), sheet.Cell(4, 100)).CopyTo(sheet.Cell(RowCount * 2 + 1, 1));

                sheet.Cell(1, 4).Value = "ГГ в работе или простое";
                sheet.Cell(RowCount + 1, 4).Value = "ГГ в работе";
                sheet.Cell(RowCount * 2 + 1, 4).Value = "ГГ в простое";

                DateTime ds = dateStart.AddSeconds(0);

                int rowIndex = 0;
                int  weekNumber = 0;
                while (ds < dateEnd)
                {
                    Logger.Info(String.Format("{0}: {1}", gg, ds));
                    DateTime de = ds.AddDays(7);
                    DiagNasos diag = new DiagNasos(ds, de, String.Format("{0:00}", gg));

                    diag.ReadData(type, nasosCount, typeRunGG);
                    Logger.Info("read");

                    ok = wb.TryGetWorksheet("GGFullData", out sheetTemp);
                    IXLWorksheet shFull = sheetTemp.CopyTo(String.Format("GG{0}_{1}", gg, ds.ToString("ddMM")));


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
                        if (pd.NextRecord != null)
                            shFull.Cell(row, 7).Value = pd.NextRecord.TimeOn;
                        shFull.Cell(row, 8).Value = pd.Comment;
                    }


                    sheetSvod.Cell(RowCount * 1 - 1,1).Value = "ГГ в работе. время простоя насосов";
                    fillHorDouble(sheetSvod, RowCount * 1 + weekNumber, ds.ToString(), diag.NasosRunGG["SVOD"].StayInfo.sorted);
                    sheetSvod.Cell(RowCount * 2 - 1, 1).Value = "ГГ в работе. время работы насосов";
                    fillHorDouble(sheetSvod, RowCount * 2 + weekNumber, ds.ToString(), diag.NasosRunGG["SVOD"].RunInfo.sorted);
                    sheetSvod.Cell(RowCount * 3 - 1, 1).Value = "ГГ в работе. скорость набора уровня (потери давления)";
                    fillHorDouble(sheetSvod, RowCount * 3 + weekNumber, ds.ToString(), diag.NasosRunGG["SVOD"].VInfo.sorted);

                    sheetSvod.Cell(RowCount * 4 - 1, 1).Value = "ГГ в простое. время простоя насосов";
                    fillHorDouble(sheetSvod, RowCount * 4 + weekNumber, ds.ToString(), diag.NasosStopGG["SVOD"].StayInfo.sorted);
                    sheetSvod.Cell(RowCount * 5 - 1, 1).Value = "ГГ в простое. время простоя насосов";
                    fillHorDouble(sheetSvod, RowCount * 5 + weekNumber, ds.ToString(), diag.NasosStopGG["SVOD"].RunInfo.sorted);
                    sheetSvod.Cell(RowCount * 6 - 1, 1).Value = "ГГ в простое. время простоя насосов";
                    fillHorDouble(sheetSvod, RowCount * 6 + weekNumber, ds.ToString(), diag.NasosStopGG["SVOD"].VInfo.sorted);

                    fillVerticalDouble(shFull, 16, "вр простоя (ГГраб)", diag.NasosStopGG["SVOD"].StayInfo.sorted);
                    fillVerticalDouble(shFull, 17, "вр простоя (ГГраб)", diag.NasosStopGG["SVOD"].StayInfo.filtered);
                    fillVerticalDouble(shFull, 18, "вр раб (ГГраб)", diag.NasosStopGG["SVOD"].StayInfo.sorted);
                    fillVerticalDouble(shFull, 19, "вр раб (ГГраб)", diag.NasosStopGG["SVOD"].StayInfo.filtered);
                    fillVerticalDouble(shFull, 20, "скор (ГГраб)", diag.NasosStopGG["SVOD"].VInfo.sorted);
                    fillVerticalDouble(shFull, 21, "скор (ГГраб)", diag.NasosStopGG["SVOD"].VInfo.filtered);


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
                        sheet.Cell(row, 5).Value = getVal(data["SVOD"].RunInfo.Count/data["SVOD"].sumTime * 3600);
                        sheet.Cell(row, 5).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 6).Value = getVal(data["SVOD"].RunInfo.MathO / 60);
                        sheet.Cell(row, 6).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 7).Value = getVal(data["SVOD"].StayInfo.MathO / 60);
                        sheet.Cell(row, 7).Style.NumberFormat.Format = "0.0";
                        sheet.Cell(row, 8).Value = getVal(data["SVOD"].RunInfo.MathO/data["SVOD"].StayInfo.MathO);
                        sheet.Cell(row, 8).Style.NumberFormat.Format = "0.00";
                        sheet.Cell(row, 9).Value = getVal(data["SVOD"].VInfo.MathO * 3600);
                        sheet.Cell(row, 9).Style.NumberFormat.Format = "0.00";

                        int col = 9;

                        for (int nasos = 1; nasos <= nasosCount; nasos++)
                        {
                            AnalizeNasosData nd = data[String.Format("{0}_{1}", type, nasos)];
                            sheet.Cell(row, col + 1).Value = getVal(nd.RunInfo.Count);
                            sheet.Cell(row, col + 1).Style.NumberFormat.Format = "0";
                            if (data["SVOD"].RunInfo.Count>0)
                            sheet.Cell(row, col + 2).Value = getVal(nd.RunInfo.Count*100.0 / data["SVOD"].RunInfo.Count );
                            sheet.Cell(row, col + 2).Style.NumberFormat.Format = "0.0";

                            sheet.Cell(row, col + 3).Value = getVal(nd.RunInfo.MathO / 60.0);
                            sheet.Cell(row, col + 3).Style.NumberFormat.Format = "0.0";

                            col += 3;
                        }


                    }
                    ds = de.AddSeconds(0);
                    rowIndex++;
                    weekNumber++;
                }
                sheet.CopyTo(wbFull, sheet.Name);
                wb.SaveAs(FileName);
            }
            wbFull.SaveAs(FileNameFull);
            

        }

        public static async void process(DateTime dateStart, DateTime dateEnd)
        {
            DateTime date = dateStart.AddSeconds(0); ;
            DateTime start = DateTime.Now;
            while (date < dateEnd)
            {
                Logger.Info(date.ToString());

                foreach (string gg in new string[] { "04", "05", "07" })
                {
                    Logger.Info(String.Format("ГГ {0} Дата {1}", gg, date));
                    int ggInt = Int32.Parse(gg);
                    List<PuskStopReader.PuskStopReaderRecord> request = new List<PuskStopReader.PuskStopReaderRecord>();
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

                    bool ok = await PuskStopReader.FillPuskStopData(request, date, date.AddHours(3));


                }


                foreach (string gg in new string[] { "01", "02", "06", "08", "09", "10" })
                {
                    Logger.Info(String.Format("ГГ {0} Дата {1}", gg, date));
                    int ggInt = Int32.Parse(gg);
                    string suffix = "";
                    suffix = ggInt <= 2 ? ".UNIT01@BLOCK1" : ggInt <= 6 ? ".UNIT05@BLOCK3" : ggInt <= 8 ? ".UNIT07@BLOCK4" : ".UNIT09@BLOCK5";
                    List<PuskStopReader.PuskStopReaderRecord> request = new List<PuskStopReader.PuskStopReaderRecord>();

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

                    bool ok = await PuskStopReader.FillPuskStopData(request, date, date.AddHours(3));


                }
                date = date.AddHours(3);
            }
            DateTime end = DateTime.Now;
            Logger.Info((end - start).TotalMinutes.ToString());
            Logger.Info("Finish");
        }


    }
}
