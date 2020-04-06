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
            DateTime start = DateTime.Now;
            while (date < dateEnd)
            {
                Logger.Info(date.ToString());
                
                foreach (string gg in new string[]{ "04","05","07"}){
                    //bool ok = await PuskStopReader.FillPuskStopData(gg, "GG_RUN", gg+"VT_GC02D-45.MCR@GRARM", date, date.AddHours(12), true);
                     bool ok = await PuskStopReader.FillPuskStopData(gg, "GG_STOP", gg + "VT_GC02D-45.MCR@GRARM", date, date.AddHours(12), false);
                    /*ok = await PuskStopReader.FillPuskStopData(gg, "MNU_1", gg+"VT_PS01DI-01.MCR@GRARM", date, date.AddHours(12), false);
                    ok = await PuskStopReader.FillPuskStopData(gg, "MNU_2", gg+"VT_PS02DI-01.MCR@GRARM", date, date.AddHours(12), false);
                    ok = await PuskStopReader.FillPuskStopData(gg, "MNU_3", gg+"VT_PS03DI-01.MCR@GRARM", date, date.AddHours(12), false);*/
                }




                
                
                date = date.AddHours(12);
            }
            DateTime end = DateTime.Now;
            Logger.Info((end - start).TotalMinutes.ToString());

        }


    }
}
