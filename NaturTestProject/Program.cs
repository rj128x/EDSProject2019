using EDSProj;
using EDSProj.EDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturTestProject
{
    class Program
    {
        public static SortedList<string, EDSPointInfo> AllPoints;
        static void Main(string[] args)
        {
            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger("C:/naturTest", "dfghdfgh");
            Task<bool> res = Program.readDataHHT();
            res.Wait();
            Console.ReadLine();
        }

        public static async Task<bool> readData()
        {
            Logger.Info("Start");
            EDSClass.Connect();
            AllPoints = new SortedList<string, EDSPointInfo>();

            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr(".*VT.*", AllPoints);
            Logger.Info(String.Format("Points {0}", AllPoints.Count));

            DateTime DateStart = new DateTime(2019, 1, 1);
            DateTime DateEnd = new DateTime(2021, 5, 17);

            Dictionary<int, EDSPointInfo> pointsH = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, EDSPointInfo> pointsNA = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, EDSPointInfo> pointsP = new Dictionary<int, EDSPointInfo>();
            
            Dictionary<int, string> iessP = new Dictionary<int, string>();
            Dictionary<int, string> iessNA = new Dictionary<int, string>();
            Dictionary<int, string> iessH = new Dictionary<int, string>();

            Dictionary<int, EDSReportRequestRecord> recPavg = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recPmin = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recPmax = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recH = new Dictionary<int, EDSReportRequestRecord>();
            //Dictionary<int, EDSReportRequestRecord> recNA = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recNAavg = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recNAmin = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recNAmax = new Dictionary<int, EDSReportRequestRecord>();


            Dictionary<int, List<int>> napors = new Dictionary<int, List<int>>(); ;

            EDSReportRequestRecord recHAvg;
            EDSPointInfo pointHAvg;
            pointHAvg = await EDSPointInfo.GetPointInfo("11VT_ST00A-034.MCR@GRARM");


            foreach (int gg in new int[] { 2,6,8,9,10})
            {
                Logger.Info("Get points " + gg);

                //OutputData.InitOutput(String.Format("gg_{0}_{1}", gg,napor));
                napors.Add(gg, new List<int>());
                string ggS = gg < 10 ? "0" + gg.ToString() : gg.ToString();

                if ((new int[] { 3, 4, 5, 7 }).Contains(gg))
                {
                    iessP.Add(gg, String.Format("0{0}VT_LP01AO-03.MCR@GRARM", gg));
                    iessH.Add(gg, String.Format("0{0}VT_TC01A-11.MCR@GRARM", gg));
                    iessNA.Add(gg, String.Format("0{0}VT_GC01A-08.MCR@GRARM", gg));
                }
                else
                {
                    string suf = "";
                    if (gg <= 2)
                        suf = ".UNIT01@BLOCK1";
                    else if (gg <= 6)
                        suf = ".UNIT05@BLOCK3";
                    else if (gg <= 8)
                        suf = ".UNIT07@BLOCK4";
                    else if (gg <= 10)
                        suf = ".UNIT09@BLOCK5";
                    iessP.Add(gg, String.Format("{0}VT_LP02AO-03", ggS) + suf);
                    iessH.Add(gg, String.Format("{0}VT_GC00A-111", ggS) + suf);
                    //iessH.Add(gg, "11VT_ST00A - 034.MCR@GRARM");
                    
                    iessNA.Add(gg, String.Format("{0}VT_GC03AI-04", ggS) + suf);
                }
                EDSPointInfo p = await EDSPointInfo.GetPointInfo(iessP[gg]);
                pointsP.Add(gg, p);

                p = await EDSPointInfo.GetPointInfo(iessH[gg]);
                pointsH.Add(gg, p);

                p = await EDSPointInfo.GetPointInfo(iessNA[gg]);
                pointsNA.Add(gg, p);

                

            }


            DateTime date = DateStart.AddMinutes(0);
            while (date < DateEnd)
            {
                DateTime ds = date.AddMinutes(0);
                DateTime de = date.AddHours(24);
                Logger.Info(string.Format("{0}-{1}", ds, de));
                recPavg.Clear();
                recPmin.Clear();
                recPmax.Clear();
                recH.Clear();
                //recNA.Clear();
                recNAavg.Clear();
                recNAmin.Clear();
                recNAmax.Clear();


                EDSReport report = new EDSReport(ds, de, EDSReportPeriod.minute5);
                foreach (int gg in new int[] { 2, 6, 8, 9, 10 })
                {
                    recPavg.Add(gg, report.addRequestField(pointsP[gg], EDSReportFunction.avg));
                    recPmin.Add(gg, report.addRequestField(pointsP[gg], EDSReportFunction.min));
                    recPmax.Add(gg, report.addRequestField(pointsP[gg], EDSReportFunction.max));
                    recH.Add(gg, report.addRequestField(pointsH[gg], EDSReportFunction.avg));
                    //recNA.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.avg));

                    recNAavg.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.avg));
                    recNAmin.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.min));
                    recNAmax.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.max));

                }
                recHAvg = report.addRequestField(pointHAvg, EDSReportFunction.avg);


                await report.ReadData();
                List<DateTime> resultDates = report.ResultData.Keys.ToList();
                foreach (DateTime dt in resultDates)
                {
                    foreach (int gg in new int[] { 2, 6, 8, 9, 10 })
                    {
                        double pAvg = report.ResultData[dt][recPavg[gg].Id];
                        double pMin = report.ResultData[dt][recPmin[gg].Id];
                        double pMax = report.ResultData[dt][recPmax[gg].Id];
                        double h = report.ResultData[dt][recH[gg].Id];
                        double hAvg = report.ResultData[dt][recHAvg.Id];
                        //double na = report.ResultData[dt][recNA[gg].Id];

                        double naAvg = report.ResultData[dt][recNAavg[gg].Id];
                        double naMin = report.ResultData[dt][recNAmin[gg].Id];
                        double naMax = report.ResultData[dt][recNAmax[gg].Id];
                        if ((pMax-pMin>2) || pMin < 5||(naMax-naMin>2))
                            continue;

                        int napor = 140;
                        while (napor < 240)
                        {
                            if (Math.Abs(napor / 10.0 - h) < 0.1)
                            {
                                if (!napors[gg].Contains(napor)) {
                                    napors[gg].Add(napor);
                                    OutputData.InitOutput(String.Format("gg_{0}_{1}", gg, napor));
                                }

                                OutputData.writeToOutput(String.Format("gg_{0}_{1}", gg, napor), string.Format("{0};{1};{2};{3};{4}", dt, h,hAvg, pAvg, naAvg));
                            }
                            napor += 5;
                        }


                         napor = 140;
                        while (napor < 240)
                        {
                            if (Math.Abs(napor / 10.0 - hAvg) < 0.1)
                            {
                                if (!napors[gg].Contains(napor))
                                {
                                    napors[gg].Add(napor);
                                    OutputData.InitOutput(String.Format("gg_hAVG_{0}_{1}", gg, napor));
                                }

                                OutputData.writeToOutput(String.Format("gg_hAVG_{0}_{1}", gg, napor), string.Format("{0};{1};{2};{3};{4}", dt, h, hAvg, pAvg, naAvg));
                            }
                            napor += 5;
                        }

                    }
                }
                date = de.AddMinutes(0);
            }
            return true;
        }

        public static async Task<bool> readDataHHT()
        {
            Logger.Info("Start");
            EDSClass.Connect();
            AllPoints = new SortedList<string, EDSPointInfo>();

            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr(".*VT.*MCR@GRARM", AllPoints);
            Logger.Info(String.Format("Points {0}", AllPoints.Count));

            DateTime DateStart = new DateTime(2019, 1, 1);
            DateTime DateEnd = new DateTime(2021, 5, 17);

            Dictionary<int, EDSPointInfo> pointsH = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, EDSPointInfo> pointsNA = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, EDSPointInfo> pointsP = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, EDSPointInfo> pointsF = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, EDSPointInfo> pointsR = new Dictionary<int, EDSPointInfo>();
            Dictionary<int, string> iessP = new Dictionary<int, string>();
            Dictionary<int, string> iessNA = new Dictionary<int, string>();
            Dictionary<int, string> iessH = new Dictionary<int, string>();
            Dictionary<int, string> iessF = new Dictionary<int, string>();
            Dictionary<int, string> iessR = new Dictionary<int, string>();


            Dictionary<int, EDSReportRequestRecord> recPmax = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recH = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recNAmin = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recNAavg = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recNAmax = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recF = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recRmin = new Dictionary<int, EDSReportRequestRecord>();
            Dictionary<int, EDSReportRequestRecord> recRmax = new Dictionary<int, EDSReportRequestRecord>();


            EDSReportRequestRecord recHAvg;
            EDSPointInfo pointHAvg;
            pointHAvg = await EDSPointInfo.GetPointInfo("11VT_ST00A-034.MCR@GRARM");

            Dictionary<int, List<int>> napors = new Dictionary<int, List<int>>(); ;

            foreach (int gg in new int[] { 2,6,8,9,10 })
            {
                Logger.Info("Get points " + gg);
                OutputData.InitOutput(String.Format("hht_gg_{0}", gg));
                //OutputData.InitOutput(String.Format("gg_{0}_{1}", gg,napor));
                napors.Add(gg, new List<int>());
                string ggS = gg < 10 ? "0" + gg.ToString() : gg.ToString();
                if ( (new int[] { 3, 4, 5, 7 }).Contains(gg)){

                    iessP.Add(gg, String.Format("0{0}VT_LP01AO-03.MCR@GRARM", gg));
                    iessH.Add(gg, String.Format("0{0}VT_TC01A-11.MCR@GRARM", gg));
                    iessNA.Add(gg, String.Format("0{0}VT_GC01A-08.MCR@GRARM", gg));
                    iessF.Add(gg, String.Format("0{0}VT_GC01A-16.MCR@GRARM", gg));
                    iessR.Add(gg, String.Format("0{0}VT_AM03P-01.MCR@GRARM", gg));
                }
                else
                {

                    string suf = "";
                    if (gg <= 2)
                        suf = ".UNIT01@BLOCK1";
                    else if (gg <= 6)
                        suf = ".UNIT05@BLOCK3";
                    else if (gg <= 8)
                        suf = ".UNIT07@BLOCK4";
                    else if (gg <= 10)
                        suf = ".UNIT09@BLOCK5";
                    
                    iessP.Add(gg, String.Format("{0}VT_LP02AO-03", ggS)+suf);
                    iessH.Add(gg, String.Format("{0}VT_GC00A-111", ggS) + suf);
                    iessNA.Add(gg, String.Format("{0}VT_GC03AI-04", ggS) + suf);
                    iessF.Add(gg, String.Format("{0}VT_GC00A-105", ggS) + suf);
                    iessR.Add(gg, String.Format("{0}VT_AM00P-60", ggS) + suf);


                }

                EDSPointInfo p = await EDSPointInfo.GetPointInfo(iessP[gg]);
                pointsP.Add(gg, p);

                p = await EDSPointInfo.GetPointInfo(iessH[gg]);
                pointsH.Add(gg, p);

                p = await EDSPointInfo.GetPointInfo(iessNA[gg]);
                pointsNA.Add(gg, p);

                p = await EDSPointInfo.GetPointInfo(iessF[gg]);
                pointsF.Add(gg, p);

                p = await EDSPointInfo.GetPointInfo(iessR[gg]);
                pointsR.Add(gg, p);

            }


            DateTime date = DateStart.AddMinutes(0);
            while (date < DateEnd)
            {
                DateTime ds = date.AddMinutes(0);
                DateTime de = date.AddHours(24);
                Logger.Info(string.Format("{0}-{1}", ds, de));
                
                recPmax.Clear();
                recH.Clear();
                recNAavg.Clear();
                recNAmin.Clear();
                recNAmax.Clear();
                recF.Clear();
                recRmin.Clear();
                recRmax.Clear();

                EDSReport report = new EDSReport(ds, de, EDSReportPeriod.sec30);
                foreach (int gg in new int[] { 2, 6, 8, 9, 10 })
                {                    
                    recPmax.Add(gg, report.addRequestField(pointsP[gg], EDSReportFunction.max));
                    recH.Add(gg, report.addRequestField(pointsH[gg], EDSReportFunction.avg));
                    recNAmin.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.min));
                    recNAmax.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.max));
                    recNAavg.Add(gg, report.addRequestField(pointsNA[gg], EDSReportFunction.avg));
                    recF.Add(gg, report.addRequestField(pointsF[gg], EDSReportFunction.avg));
                    recRmin.Add(gg, report.addRequestField(pointsR[gg], EDSReportFunction.min));
                    recRmax.Add(gg, report.addRequestField(pointsR[gg], EDSReportFunction.max));

                }
                recHAvg = report.addRequestField(pointHAvg, EDSReportFunction.avg);
                await report.ReadData();
                List<DateTime> resultDates = report.ResultData.Keys.ToList();
                foreach (DateTime dt in resultDates)
                {
                    foreach (int gg in new int[] { 2, 6, 8, 9, 10 })
                    {

                        double pMax = report.ResultData[dt][recPmax[gg].Id];
                        double naAvg = report.ResultData[dt][recNAavg[gg].Id];
                        double naMin = report.ResultData[dt][recNAmin[gg].Id];
                        double naMax = report.ResultData[dt][recNAmax[gg].Id];
                        double f = report.ResultData[dt][recF[gg].Id];
                        double h = report.ResultData[dt][recH[gg].Id];
                        double hAvg = report.ResultData[dt][recHAvg.Id];

                        double rMin = report.ResultData[dt][recRmin[gg].Id];
                        double rMax = report.ResultData[dt][recRmax[gg].Id];

                        long mask = (new int[] { 3, 4, 5, 7 }).Contains(gg) ? (long)Math.Pow(2, 4) : (long)Math.Pow(2, 15);
                        
                        long rAbsMax = (long)rMax;
                        long maskMax = rAbsMax & mask;

                        long rAbsMin = (long)rMin;
                        long maskMin = rAbsMin & mask;



                        if (naMax-naMin>1 ||rAbsMin!=rAbsMax||maskMin==0||naAvg<6||naAvg>16)
                            continue;
                                               

                        OutputData.writeToOutput(String.Format("hht_gg_{0}", gg), string.Format("{0};{1};{2};{3}", dt, h,hAvg, naAvg));



                    }
                }
                date = de.AddMinutes(0);
            }
            return true;
        }


    }
}
