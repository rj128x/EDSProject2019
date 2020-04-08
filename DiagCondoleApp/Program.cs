using EDSProj;
using EDSProj.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagCondoleApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            DateTime DateStart = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(-5);
            DateTime DateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
            if (args.Count() == 0)
            {
                Console.WriteLine("Введите дату начала выгрузки: ");
                DateStart = DateTime.Parse(Console.ReadLine());
                Console.WriteLine("Введите дату конца выгрузки: ");
                DateEnd = DateTime.Parse(Console.ReadLine());
            }

            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger("C:/diagLog", "diag");
            //process(DateStart, DateEnd);
            PuskStopReader.RefreshPuskStopData(DateStart, DateEnd);

            //Console.ReadLine();
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
                        gg = ggInt
                    });

                    request.Add(new PuskStopReader.PuskStopReaderRecord()
                    {
                        DBRecord = "MNU_2",
                        iess = gg + "VT_PS02DI-01.MCR@GRARM",
                        inverted = false,
                        gg = ggInt
                    });

                    request.Add(new PuskStopReader.PuskStopReaderRecord()
                    {
                        DBRecord = "MNU_3",
                        iess = gg + "VT_PS03DI-01.MCR@GRARM",
                        inverted = false,
                        gg = ggInt
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
                        gg = ggInt
                    });

                    request.Add(new PuskStopReader.PuskStopReaderRecord()
                    {
                        DBRecord = "MNU_2",
                        iess = gg + "VT_PS02DI-03" + suffix,
                        inverted = false,
                        gg = ggInt
                    });

                    request.Add(new PuskStopReader.PuskStopReaderRecord()
                    {
                        DBRecord = "MNU_3",
                        iess = gg + "VT_PS03DI-03" + suffix,
                        inverted = false,
                        gg = ggInt
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
