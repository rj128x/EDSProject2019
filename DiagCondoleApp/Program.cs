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
                bool ok = await PuskStopData.FillPuskStopData("05", "GG_STOP", "05VT_GC02D-45.MCR@GRARM", date, date.AddHours(12));
                ok = await PuskStopData.FillPuskStopData("05", "MNU_1", "05VT_PS01DI-01.MCR@GRARM", date, date.AddHours(12));
                ok = await PuskStopData.FillPuskStopData("05", "MNU_2", "05VT_PS02DI-01.MCR@GRARM", date, date.AddHours(12));
                ok = await PuskStopData.FillPuskStopData("05", "MNU_3", "05VT_PS03DI-01.MCR@GRARM", date, date.AddHours(12));
                Logger.Info(ok.ToString());
                date = date.AddHours(12);
            }

        }


    }
}
