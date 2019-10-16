using EDSProj;
using EDSProj.EDS;
using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestCNSL
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger(Settings.Single.LogPath, "pbrExport");

            /*DateTime date = DateTime.Now.Date;
			if (args.Length == 1) {
				int day = Int32.Parse(args[0]);
				runReports(date.AddDays(-day));
			}
			if (args.Length == 2) {
				int day = Int32.Parse(args[0]);
				int day2 = Int32.Parse(args[1]);
				for (int d = day; d <= day2; d++) {
					runReports(date.AddDays(-d));
					Thread.Sleep(10000);
				}
			}

            EDSClass.Disconnect();*/
            run();
            Console.ReadLine();
        }

        public static void runReports(DateTime date)
        {
            Logger.Info("статистика за " + date);
            uint cnt;
            uint total;
            if (!EDSClass.Connected)
                EDSClass.Connect();
            ReportConfig[] reports = EDSClass.Client.getReportsConfigs(EDSClass.AuthStr, null, 0, 1000, out cnt, out total);
            Console.WriteLine(reports.Count().ToString());
            List<int> idsForRun = new List<int>();
            foreach (ReportConfig report in reports)
            {
                /*Console.WriteLine(report.id);
				Console.WriteLine(report.reportDefinitionFile);
				Console.WriteLine(report.inputValues);*/
                string file = report.reportDefinitionFile.ToLower();
                if (file.Contains("ou17"))
                {
                    idsForRun.Add((int)report.id);
                }
            }

            foreach (int id in idsForRun)
            {
                Logger.Info(String.Format("Выполнение {0}", id));
                GlobalReportRequest req = new GlobalReportRequest();
                req.reportConfigId = (uint)id;
                req.dtRef = new Timestamp() { second = EDSClass.toTS(date) };

                uint reqId = EDSClass.Client.requestGlobalReport(EDSClass.AuthStr, req);
                bool ok = EDSClass.ProcessQuery(reqId);
                Logger.Info(ok.ToString());
            }


        }



        public async static void run()
        {
            uint cnt;
            uint total;
            if (!EDSClass.Connected)
                EDSClass.Connect();

            /*TimeDuration zone;
            TimeDuration offset = new TimeDuration();
            Timestamp s= EDSClass.Client.getServerTime(EDSClass.AuthStr, out zone, out offset);

            Console.WriteLine(EDSClass.fromTS(zone.seconds));
            Console.WriteLine(EDSClass.fromTS(offset.seconds));
            Console.Write(EDSClass.fromTS(s.second));
            return;*/

            Dictionary<DateTime, int> data = new Dictionary<DateTime, int>();

            DateTime dateStart = DateTime.Parse("01.06.2018 00:00:00");
            DateTime dateEnd = DateTime.Parse("01.09.2018 00:00:00");
            DateTime date = dateStart.AddHours(0);
            DateTime dateSet = DateTime.MinValue;
            bool prevVal = false;
                        
            while (date <= dateEnd)
            {
                EDSClass.Connect();
                TabularRequest req = new TabularRequest();
                List<TabularRequestItem> items = new List<TabularRequestItem>();
                items.Add(new TabularRequestItem()
                {
                    function = "VALUE",
                    pointId = new PointId()
                    {
                        iess = "04VT_AM01P-47.MCR@GRARM"
                    }
                });





                req.items = items.ToArray();

                req.period = new TimePeriod()
                {
                    from = new Timestamp()
                    {
                        second = EDSClass.toTS(date)
                    },
                    till = new Timestamp()
                    {
                        second = EDSClass.toTS(date.AddHours(24))
                    }
                };

                req.step = new TimeDuration()
                {
                    seconds = 1
                };

                uint id = EDSClass.Client.requestTabular(EDSClass.AuthStr, req);

                EDSClass.ProcessQuery(id);

                TabularRow[] rows;

                getTabularRequest request = new getTabularRequest()
                {
                    authString = EDSClass.AuthStr,
                    requestId = id
                };

                getTabularResponse resp = await EDSClass.Client.getTabularAsync(request);

                foreach (TabularRow row in resp.rows)
                {
                    DateTime dt = EDSClass.fromTS(row.ts.second);

                    TabularValue tv = row.values[0];
                    if (tv.quality != Quality.QUALITYGOOD)
                        continue;

                    int val = 0;
                    if (tv.value.ipvSpecified)
                        val = (int)tv.value.ipv.Value;
                    if (tv.value.pvSpecified)
                        val = (int)tv.value.pv.Value;

                    string bin = Convert.ToString(val, 2);
                    bool on = false;
                    if (bin.Length >= 3 && bin[bin.Length - 3] == '1')
                        on = true;

                    if (!prevVal && on)
                    {
                        dateSet = dt.AddHours(0);
                        data.Add(dt, 0);
                    }
                    if (on)
                    {
                        data[dateSet]++;
                    }
                    prevVal = on;

                    /*string str = string.Format("{0}: {1} {2}", date, bin, on);                    
                    Console.WriteLine(str);*/
                }

                date = date.AddHours(24);
                Console.WriteLine(date);
                EDSClass.Disconnect();
            }

            foreach (KeyValuePair<DateTime, int> de in data)
            {
                Console.WriteLine(string.Format("{0}:\t{1}:{2}", de.Key, de.Value / 60,de.Value%60));
            }

        }
    }
}
