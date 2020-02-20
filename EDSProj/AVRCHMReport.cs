using EDSProj.EDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
    public class AVRCHMRecord
    {
        public SortedList<int, double> P_GA { get; set; }
        public SortedList<int, double> NA_GA { get; set; }
        public double SumGroupZad { get; set; }
        public int GGCount { get; set; }
        public double PPlan { get; set; }
        public double PPerv { get; set; }
        public double PZVN { get; set; }
        public double PFakt { get; set; }
        public double PPlanFull { get; set; }
        public double PMax { get; set; }
        public double PMin { get; set; }
        public double PMinMaket { get; set; }
        public double PMaxMaket { get; set; }
        public int ErrorLimits { get; set; }
        public double ErrorLimits15 { get; set; }
        public double ResursZagr { get; set; }
        public double ResursRazgr { get; set; }
        public double ErrorRezerv { get; set; }
        public double ErrorRezervGraph { get; set; }
        public AVRCHMRecord()
        {
            P_GA = new SortedList<int, double>();
            NA_GA = new SortedList<int, double>();
        }
        public double getInfo(string desc)
        {
            switch (desc)
            {
                case "PPlan": return PPlan;
                case "PPerv": return PPerv;
                case "PZVN": return PZVN;
                case "PFakt": return PFakt;
                case "PMin": return PMin;
                case "PMax": return PMax;
                case "PMinMaket": return PMinMaket;
                case "PMaxMaket": return PMaxMaket;
                case "PPlanFull": return PPlanFull;
                case "GGCount": return GGCount;
                case "SumGroupZad": return SumGroupZad;
                case "ErrorLimits": return ErrorLimits;
                case "ErrorLimits15": return ErrorLimits15;
                case "ErrorRezervGraph": return ErrorRezervGraph;


                case "ResursRazgr": return ResursRazgr;
                case "ResursZagr": return ResursZagr;

                default: return 0;
            }
        }
    }

    public class AVRCHMReportRecord
    {
        public string TypeRecord { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string HasError { get; set; }
        public string Date { get; set; }
    }

    public class AVRCHMReport
    {
        protected static Dictionary<string, EDSPointInfo> PointsRef;
        protected static SortedList<String, EDSPointInfo> AllPoints;
        public static void init(SortedList<String, EDSPointInfo> allPoints)
        {
            AllPoints = allPoints;
            PointsRef = new Dictionary<string, EDSPointInfo>();

            PointsRef.Add("PZad", AllPoints["11VT_GP00A-003.MCR@GRARM"]);
            PointsRef.Add("PPlan", AllPoints["11VT_GP00A-043.MCR@GRARM"]);
            PointsRef.Add("PZVN", AllPoints["11VT_GP00AP-125.MCR@GRARM"]);
            PointsRef.Add("PPerv", AllPoints["11VT_GP00AP-119.MCR@GRARM"]);
            PointsRef.Add("PFakt", AllPoints["11VT_GP00A-136.MCR@GRARM"]);

            PointsRef.Add("PMinMaket", AllPoints["PBR_GES_MIN.EDS@CALC"]);
            PointsRef.Add("PMaxMaket", AllPoints["PBR_GES_MAX.EDS@CALC"]);

            PointsRef.Add("ResursZagr", AllPoints["11VT_GP00AP-131.MCR@GRARM"]);
            PointsRef.Add("ResursRazgr", AllPoints["11VT_GP00AP-132.MCR@GRARM"]);

            PointsRef.Add("GGCount", AllPoints["11VT_GP00AP-151.MCR@GRARM"]);

        }
        public Dictionary<DateTime, AVRCHMRecord> Data { get; set; }
        public List<AVRCHMReportRecord> Events { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int ZVNMax { get; set; }
        public int TPlanMax { get; set; }
        public int WinSize { get; set; }

        public void checkErrors(int zvnMax, int tPlanMax, int winSize)
        {
            ZVNMax = zvnMax;
            TPlanMax = tPlanMax;
            WinSize = winSize;

            List<double> prevPPlan = new List<double>();
            int errorLimits = 0;
            int errorRezerv = 0;
            AVRCHMRecord prevRecord = null;

            List<DateTime> errorDates = new List<DateTime>();
            List<DateTime> eventDates = new List<DateTime>();


            foreach (KeyValuePair<DateTime,AVRCHMRecord> de in Data)
            {
                AVRCHMRecord rec = de.Value;
                DateTime dtMSK = de.Key;
                rec.ErrorLimits15 = 0;
                rec.ErrorLimits = 0;
                rec.ErrorRezerv = 0;
                rec.ErrorRezervGraph = 0;

                rec.PPlan = rec.PPlan;

                if (rec.PMaxMaket < 100)
                    rec.PMaxMaket = 1000;

                rec.PPlanFull = rec.PPlan + rec.PPerv + rec.PZVN;
                prevPPlan.Add(rec.PPlanFull);
                rec.PMax = prevPPlan.Max() + rec.PMaxMaket * 0.01;
                rec.PMin = prevPPlan.Min() - rec.PMaxMaket * 0.01;
                if (prevPPlan.Count > WinSize)
                    prevPPlan.RemoveAt(0);

                if (dtMSK > DateStart && dtMSK < DateEnd)
                {
                    if (rec.PFakt > rec.PMax || rec.PFakt < rec.PMin)
                    {
                        rec.ErrorLimits = errorLimits + 1;
                        if (rec.ErrorLimits > TPlanMax)
                            rec.ErrorLimits15 = rec.PFakt;
                        errorLimits++;
                        if (errorLimits >= TPlanMax)
                        {
                            errorDates.Add(dtMSK);
                        }
                    }
                    else
                    {
                        errorLimits = 0;
                    }


                    bool errZagr = (rec.PZVN + rec.ResursZagr) < ZVNMax - 0.01 * rec.PMaxMaket;
                    bool errRazgr = (rec.ResursRazgr - rec.PZVN) < ZVNMax - 0.01 * rec.PMaxMaket;

                    if (errRazgr || errZagr)
                    {
                        errorRezerv++;
                        rec.ErrorRezerv = errorRezerv;                        
                        rec.ErrorRezervGraph = errRazgr ? rec.ResursRazgr+0.001 : rec.ResursZagr+0.001;
                        errorDates.Add(dtMSK);
                    }
                    else
                    {
                        errorRezerv = 0;
                    }



                    if (rec.GGCount != prevRecord.GGCount)
                    {
                        errorDates.Add(dtMSK);
                    }

                    if (Math.Abs(rec.SumGroupZad - prevRecord.SumGroupZad) > 10)
                    {
                        errorDates.Add(dtMSK);
                    }
                }

                prevRecord = rec;

            }
            ProcessData(errorDates);



            
        }
        public async Task<bool> ReadData(DateTime dateStart, DateTime dateEnd,int zvnMax,int tPlanMax,int winSize)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;
            EDSReportPeriod period = EDSReportPeriod.sec;

            EDSReport report = new EDSReport(dateStart.AddMinutes(-2), dateEnd.AddMinutes(2), period, true);

            Dictionary<string, EDSReportRequestRecord> records = new Dictionary<string, EDSReportRequestRecord>();
            foreach (KeyValuePair<string, EDSPointInfo> de in PointsRef)
            {
                EDSReportRequestRecord rec = report.addRequestField(de.Value, EDSReportFunction.val);
                records.Add(de.Key, rec);
            }

            bool ok = await report.ReadData();

            if (!ok)
                return false;

            Data = new Dictionary<DateTime, AVRCHMRecord>();

            report.ResultData.Remove(report.ResultData.Keys.Last());
            DateTime dtMSK = report.ResultData.Keys.First().AddHours(-2);
            foreach (DateTime date in report.ResultData.Keys)
            {
                dtMSK = date.AddHours(-2);
                AVRCHMRecord rec = new AVRCHMRecord();
                Data.Add(dtMSK, rec);

                Dictionary<string, double> dataRec = report.ResultData[date];
                rec.PFakt = dataRec[records["PFakt"].Id];
                rec.PPlan = dataRec[records["PPlan"].Id];
                rec.PZVN = dataRec[records["PZVN"].Id];
                rec.PPerv = dataRec[records["PPerv"].Id];
                rec.GGCount = (int)dataRec[records["GGCount"].Id];
                rec.SumGroupZad = dataRec[records["PZad"].Id] - rec.PZVN;
                rec.ResursZagr = dataRec[records["ResursZagr"].Id];
                rec.ResursRazgr = dataRec[records["ResursRazgr"].Id];
                rec.PMinMaket = dataRec[records["PMinMaket"].Id];
                rec.PMaxMaket = dataRec[records["PMaxMaket"].Id];

            }
            checkErrors(zvnMax, tPlanMax, winSize);
            return true;
        }

        public void ProcessData(List<DateTime> errorDates)
        {
            Events = new List<AVRCHMReportRecord>();
            
            AVRCHMReportRecord ev = new AVRCHMReportRecord();
            if (errorDates.Count > 0)
            {
                DateTime date = errorDates.First();
                ev.DateStart = date.AddMinutes(-2);
                ev.DateEnd = date.AddMinutes(2);
                Events.Add(ev);
                foreach (DateTime dt in errorDates)
                {
                    if (ev.DateStart.AddMinutes(6) >= dt.AddMinutes(2))
                    {
                        ev.DateEnd = dt.AddMinutes(2);
                    }
                    else
                    {
                        ev = new AVRCHMReportRecord();
                        Events.Add(ev);
                        ev.DateStart = dt.AddMinutes(-2);
                        ev.DateEnd = dt.AddMinutes(2);
                    }
                }

                foreach (AVRCHMReportRecord evn in Events)
                {
                    evn.Date = String.Format("{0} {1}-{2}", evn.DateStart.ToString("dd.MM"), evn.DateStart.ToString("HH:mm:ss"), evn.DateEnd.ToString("HH:mm:ss"));
                    bool hasError = false;
                    double errorLimits = 0;
                    double errorRezerv = 0;
                    AVRCHMRecord prevRecord = null;
                    foreach (DateTime dt in Data.Keys)
                    {

                        if (dt > evn.DateStart && dt <= evn.DateEnd)
                        {
                            AVRCHMRecord rec = Data[dt];
                            if (rec.ErrorLimits == 0 && errorLimits > 0 || dt == evn.DateEnd&&rec.ErrorLimits>0)
                            {
                                evn.HasError += String.Format("План {0}сек\r\n", errorLimits);

                            }
                            if (rec.ErrorRezerv == 0 && errorRezerv > 0 ||dt==evn.DateEnd&&rec.ErrorRezerv>0)
                            {
                                evn.HasError += String.Format("Резерв {0}сек\r\n", errorRezerv);
                            }
                            if (prevRecord != null)
                            {

                                if (rec.GGCount != prevRecord.GGCount)
                                {
                                    evn.TypeRecord += String.Format("[{0}] Смена состава \r\n", dt.ToString("HH:mm:ss"));
                                }

                                if (Math.Abs(rec.SumGroupZad - prevRecord.SumGroupZad) > 10)
                                {
                                    evn.TypeRecord += String.Format("[{2}] Смена нагрузки {1:0} - {0:0} \r\n", rec.SumGroupZad, prevRecord.SumGroupZad, dt.ToString("HH:mm:ss"));
                                }
                            }
                            prevRecord = rec;
                            errorLimits = rec.ErrorLimits;
                            errorRezerv = rec.ErrorRezerv;
                        }
                    }

                }
            }
            ev = new AVRCHMReportRecord();
            ev.DateStart = DateStart;
            ev.DateEnd = DateEnd;
            ev.Date = String.Format("{0}: сутки", ev.DateStart.ToString("dd.MM"));
            ev.HasError = "----------";
            ev.TypeRecord = "----------";
            Events.Add(ev);

        }


        public SortedList<DateTime, double> getSerieData(string desc, DateTime dateStart, DateTime dateEnd, bool add0 = true)
        {
            SortedList<DateTime, double> data = new SortedList<DateTime, double>();

            foreach (DateTime date in Data.Keys)
            {
                if (date >= dateStart && date <= dateEnd)
                {
                    double val = Data[date].getInfo(desc);

                    if (add0)
                        data.Add(date, val);
                    else
                    {
                        if (val != 0)
                            data.Add(date, val);
                    }
                }

            }

            return data;
        }


        public SortedList<String,SortedList<DateTime, double>> getSeriesErrorsData(string desc, DateTime dateStart, DateTime dateEnd)
        {
            SortedList<String, SortedList<DateTime, double>> result = new SortedList<string, SortedList<DateTime, double>>();
            
            
            SortedList<DateTime, double> data = new SortedList<DateTime, double>();
            
            foreach (DateTime date in Data.Keys)
            {
                if (date >= dateStart && date <= dateEnd)
                {
                    double val = Data[date].getInfo(desc);

                    if (val != 0)
                       data.Add(date, val);
                    else
                    {
                        if (data.Count > 0)
                        {
                            result.Add("Error "+data.First().Key.ToString("HH:mm:ss"), data);
                            data = new SortedList<DateTime, double>();
                        }
                    }                       
                    
                }

            }
            if (data.Count > 0)
            {
                result.Add("Error", data);
                data = new SortedList<DateTime, double>();
            }
            return result;
        }


        public async Task<Dictionary<string, SortedList<DateTime, double>>> getGaData(DateTime dateStart, DateTime dateEnd)
        {
            Dictionary<string, SortedList<DateTime, double>> result = new Dictionary<string, SortedList<DateTime, double>>();
            EDSReportPeriod period = EDSReportPeriod.sec;
            EDSReport report = new EDSReport(dateStart, dateEnd, period, true);
            for (int gg = 1; gg <= 10; gg++)
            {

                string name = String.Format("11VT_GG{0}{1}AP-031.MCR@GRARM", gg < 10 ? "0" : "", gg);
                report.addRequestField(AllPoints[name], EDSReportFunction.val);
                name = String.Format("11VT_GG{0}{1}AP-040.MCR@GRARM", gg < 10 ? "0" : "", gg);
                report.addRequestField(AllPoints[name], EDSReportFunction.val);

                name = String.Format("11VT_GG{0}{1}AP-043.MCR@GRARM", gg < 10 ? "0" : "", gg);
                report.addRequestField(AllPoints[name], EDSReportFunction.val);

                /*name = String.Format("11VT_GG{0}{1}AP-502.MCR@GRARM", gg < 10 ? "0" : "", gg);
                report.addRequestField(AllPoints[name], EDSReportFunction.val);
                name = String.Format("11VT_GG{0}{1}AP-501.MCR@GRARM", gg < 10 ? "0" : "", gg);
                report.addRequestField(AllPoints[name], EDSReportFunction.val);*/

                name = String.Format("11VT_BS{0}{1}D-001.MCR@GRARM", gg < 10 ? "0" : "", gg);
                report.addRequestField(AllPoints[name], EDSReportFunction.val);

            }

            bool ok = await report.ReadData();
            if (!ok)
                return new Dictionary<string, SortedList<DateTime, double>>();

            int indexVG = 0;
            foreach (string key in report.RequestData.Keys)
            {
                bool isVG = report.RequestData[key].Desc.Contains("ВГ");
                if (isVG)
                    indexVG++;
                SortedList<DateTime, double> data = new SortedList<DateTime, double>();
                foreach (DateTime date in report.ResultData.Keys)
                {
                    if (isVG)
                    {
                        data.Add(date.AddHours(-2), report.ResultData[date][key] + (indexVG));
                    }
                    else
                    {
                        data.Add(date.AddHours(-2), report.ResultData[date][key]);
                    }
                }

                result.Add(report.RequestData[key].Desc, data);

            }
            return result;
        }
    }
}
