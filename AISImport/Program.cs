using EDSProj;
using EDSProj.AIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISImport
{
	class Program
	{
		protected static DateTime GetDate(string arg,bool min=false) {
			int val = 0;
			bool isInt = int.TryParse(arg, out val);			
			if (isInt) {
                DateTime now = DateTime.Now;
                if (!min)
                {
                    DateTime date = now.Date.AddHours(now.Hour);
                    if (now.Minute > 30)
                        date = date.AddMinutes(30);
                    date = date.AddHours(-val);
                    return date;
                }
                else
                {
                    DateTime date = now.Date.AddHours(now.Hour).AddMinutes(now.Minute).AddMinutes(-val);
                    return date;
                }
			} else {
				bool isDate;
				DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour);                
				isDate = DateTime.TryParse(arg, out date);
				if (isDate) {
					return date;
				} else {
                    date  = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                    if (DateTime.Now.Minute > 30)
                        date = date.AddMinutes(30);
                    return date;
                }
			}
		}

		static void Main(string[] args) {
            
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "aisImport");

            
            		

            bool min = false;
            if (args.Length > 0)
            {
                min = args[0].ToLower().Contains("min");
            }

            //DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour-2 - (min?1:12));
            //DateTime dateEnd = !min?DateTime.Now.Date.AddHours(DateTime.Now.Hour):DateTime.Now.AddHours(-2);

            DateTime date = DateTime.Parse("01.01.2017");
            DateTime dateEnd = DateTime.Parse("01.01.2018");

            if (args.Length == 3) {	
				date = GetDate(args[1],min);
				dateEnd = GetDate(args[2],min);
			}

            Logger.Info(String.Format("{0} {1} min:{2}", date, dateEnd, min));

			if (min) {
                AIS_1MIN ais = new AIS_1MIN();
                ais.readPoints();
                while (date < dateEnd)
                {
                    DateTime de = date.AddMinutes(10);
                    de = de > dateEnd ? dateEnd : de;
                    ais.readDataFromDB(date, de);
                    date = de.AddHours(0);
                }
            }
            else {
                AIS_OLD ais = new AIS_OLD();
                ais.readPoints();
                while (date < dateEnd)
                {
                    int diff = 24 * 10;
                    if (date.Month == 3 && date.Day>15)
                        diff = 12;
                    else
                        diff = 24*15;
                    
                    DateTime de = date.AddHours(diff);
                    de = de > dateEnd ? dateEnd : de;
                    ais.readDataFromDB(date, de);
                    date = de.AddHours(0);
                }
            }
            EDSClass.Disconnect();
            Console.ReadLine();
		}
        

	}
}
