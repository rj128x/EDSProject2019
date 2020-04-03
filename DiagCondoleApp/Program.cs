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
            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger("C:/diagLog", "diag");
            process();
            Logger.Info("Finish");
            Console.ReadLine();
        }

        public static async void process()
        {
            DateTime date = DateTime.Parse("21.03.2020 00:00");
            DateTime dateEnd = DateTime.Parse("29.03.2020");

            while (date < dateEnd)
            {
                Logger.Info(date.ToString());
                DateTime start = DateTime.Now;
                foreach (string gg in new string[]{ "04","05","07"}){
                    bool ok = await PuskStopData.FillPuskStopData(gg, "GG_STOP", gg+"VT_GC02D-45.MCR@GRARM", date, date.AddHours(12), false);
                    ok = await PuskStopData.FillPuskStopData(gg, "MNU_1", gg+"VT_PS01DI-01.MCR@GRARM", date, date.AddHours(12), true);
                    ok = await PuskStopData.FillPuskStopData(gg, "MNU_2", gg+"VT_PS02DI-01.MCR@GRARM", date, date.AddHours(12), true);
                    ok = await PuskStopData.FillPuskStopData(gg, "MNU_3", gg+"VT_PS03DI-01.MCR@GRARM", date, date.AddHours(12), true);
                }




                DateTime end = DateTime.Now;
                Logger.Info((end - start).TotalMinutes.ToString());
                date = date.AddHours(12);
            }

        }


    }
}
