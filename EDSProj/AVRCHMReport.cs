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
        public SortedList<int,double> P_GA { get; set; }
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
        public int ErrorLimits { get; set; }
        public double ErrorLimits15 { get; set; }
        public AVRCHMRecord()
        {
            P_GA = new SortedList<int, double>();
            NA_GA = new SortedList<int, double>();
        }
        public double getInfo(string desc)
        {
            switch (desc)
            {
                case "PPlan": return PPlan ;
                case "PPerv": return PPerv ;
                case "PZVN": return PZVN ;
                case "PFakt": return PFakt ;
                case "PMin": return PMin;
                case "PMax": return PMax;
                case "PPlanFull": return PPlanFull;
                case "GGCount": return GGCount ;
                case "SumGroupZad": return PPerv;
                case "ErrorLimits": return ErrorLimits;
                case "ErrorLimits15": return ErrorLimits15;
                default: return  0 ;
            }
        }
    }

    public class AVRCHMReportRecord
    {
        public string TypeRecord { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string HasError { get; set; }
        public DateTime Date { get; set; }
    }

    public class AVRCHMReport
    {
        protected static Dictionary<string, EDSPointInfo> PointsRef;
        protected static SortedList<String, EDSPointInfo> AllPoints;
        public static void init(SortedList<String, EDSPointInfo> allPoints)
        {
            AllPoints = allPoints;
            PointsRef = new Dictionary<string, EDSPointInfo>();
            /*PointsRef.Add("P_GG1", "11VT_GG01AP-031.MCR@GRARM");
            PointsRef.Add("P_GG2", "11VT_GG02AP-031.MCR@GRARM");
            PointsRef.Add("P_GG3", "11VT_GG03AP-031.MCR@GRARM");
            PointsRef.Add("P_GG4", "11VT_GG04AP-031.MCR@GRARM");
            PointsRef.Add("P_GG5", "11VT_GG05AP-031.MCR@GRARM");
            PointsRef.Add("P_GG6", "11VT_GG06AP-031.MCR@GRARM");
            PointsRef.Add("P_GG7", "11VT_GG07AP-031.MCR@GRARM");
            PointsRef.Add("P_GG8", "11VT_GG08AP-031.MCR@GRARM");
            PointsRef.Add("P_GG9", "11VT_GG09AP-031.MCR@GRARM");
            PointsRef.Add("P_GG10", "11VT_GG10AP-031.MCR@GRARM");*/


            /*PointsRef.Add("NA_GG1", "11VT_GG01A-024.MCR@GRARM");
            PointsRef.Add("NA_GG2", "11VT_GG02A-024.MCR@GRARM");
            PointsRef.Add("NA_GG3", "11VT_GG03A-024.MCR@GRARM");
            PointsRef.Add("NA_GG4", "11VT_GG04A-024.MCR@GRARM");
            PointsRef.Add("NA_GG5", "11VT_GG05A-024.MCR@GRARM");
            PointsRef.Add("NA_GG6", "11VT_GG06A-024.MCR@GRARM");
            PointsRef.Add("NA_GG7", "11VT_GG07A-024.MCR@GRARM");
            PointsRef.Add("NA_GG8", "11VT_GG08A-024.MCR@GRARM");
            PointsRef.Add("NA_GG9", "11VT_GG09A-024.MCR@GRARM");
            PointsRef.Add("NA_GG10", "11VT_GG10A-024.MCR@GRARM");*/

            /*PointsRef.Add("PZ_GP1", AllPoints["11VT_GP01AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP2", AllPoints["11VT_GP02AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP3", AllPoints["11VT_GP03AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP4", AllPoints["11VT_GP04AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP5", AllPoints["11VT_GP05AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP6", AllPoints["11VT_GP06AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP7", AllPoints["11VT_GP07AP-106.MCR@GRARM"]);
            PointsRef.Add("PZ_GP8", AllPoints["11VT_GP08AP-106.MCR@GRARM"]);*/

            PointsRef.Add("PZad", AllPoints["11VT_GP00A-003.MCR@GRARM"]);
            PointsRef.Add("PPlan", AllPoints["11VT_GP00A-043.MCR@GRARM"]);
            PointsRef.Add("PZVN", AllPoints["11VT_GP00AP-125.MCR@GRARM"]);
            PointsRef.Add("PPerv", AllPoints["11VT_GP00AP-119.MCR@GRARM"]);
            PointsRef.Add("PFakt", AllPoints["11VT_GP00A-136.MCR@GRARM"]);

            PointsRef.Add("GGCount", AllPoints["11VT_GP00AP-151.MCR@GRARM"]);

        }
        public Dictionary<DateTime, AVRCHMRecord> Data { get; set; }
        public Dictionary<DateTime, AVRCHMReportRecord> Events { get; set; }
        public async Task<bool> ReadData(DateTime dateStart,DateTime dateEnd)
        {

            EDSReportPeriod period = EDSReportPeriod.sec;

            EDSReport report = new EDSReport(dateStart, dateEnd, period);

            Dictionary<string, EDSReportRequestRecord> records = new Dictionary<string, EDSReportRequestRecord>();
            foreach (KeyValuePair<string,EDSPointInfo> de in PointsRef)
            {
                EDSReportRequestRecord rec=report.addRequestField(de.Value, EDSReportFunction.val);
                records.Add(de.Key, rec);
            }

            bool ok =await report.ReadData();

            if (!ok)
                return false;

            Data = new Dictionary<DateTime, AVRCHMRecord>();
            List<double> prevPPlan = new List<double>();
            int errorLimits = 0;
            Events = new Dictionary<DateTime, AVRCHMReportRecord>();
            AVRCHMRecord prevRecord = null;

            foreach (DateTime date in report.ResultData.Keys)
            {
                AVRCHMRecord rec = new AVRCHMRecord();
                Dictionary<string, double> dataRec = report.ResultData[date];
                rec.PFakt = dataRec[records["PFakt"].Id];
                rec.PPlan = dataRec[records["PPlan"].Id];
                rec.PZVN = dataRec[records["PZVN"].Id];
                rec.PPerv = dataRec[records["PPerv"].Id];
                rec.GGCount = (int)dataRec[records["GGCount"].Id];
                rec.SumGroupZad = dataRec[records["PZad"].Id] - rec.PZVN;

                rec.PPlanFull = rec.PPlan + rec.PPerv + rec.PZVN;
                prevPPlan.Add(rec.PPlanFull);
                rec.PMax = prevPPlan.Max() + rec.PPlanFull * 0.01;
                rec.PMin = prevPPlan.Min() - rec.PPlanFull * 0.01;

                if (rec.PFakt>rec.PMax || rec.PFakt < rec.PMin)
                {
                    rec.ErrorLimits = errorLimits + 1;
                    if (rec.ErrorLimits > 15)
                        rec.ErrorLimits15 = rec.PFakt;
                    errorLimits++;
                }
                else
                {
                    rec.ErrorLimits = 0;
                    errorLimits = 0;
                }


                if (prevPPlan.Count > 15)
                    prevPPlan.RemoveAt(0);
                Data.Add(date, rec);


                if (prevRecord == null)
                {
                    prevRecord = rec;
                    continue;
                }
                if (rec.GGCount != prevRecord.GGCount)
                {
                    AVRCHMReportRecord repRec = new AVRCHMReportRecord();
                    repRec.TypeRecord = "Смена состава";
                    repRec.DateStart = date.AddMinutes(-3);
                    repRec.DateEnd = date.AddMinutes(+3);
                    repRec.Date = date;                    
                    Events.Add(date,repRec);
                }

                if (Math.Abs(rec.SumGroupZad - prevRecord.SumGroupZad) > 10)
                {
                    AVRCHMReportRecord repRec = new AVRCHMReportRecord();
                    repRec.TypeRecord = "Смена нагрузки";
                    repRec.DateStart = date.AddMinutes(-3);
                    repRec.DateEnd = date.AddMinutes(+3);
                    repRec.Date = date;
                    Events.Add(date,repRec);
                }

                prevRecord = rec;
            }

            return true;
        }

        public void ProcessData()
        {
            foreach (DateTime evDate in Events.Keys)
            {
                AVRCHMReportRecord ev = Events[evDate];
                bool hasError = false;
                double maxLimitError = 0;
                foreach (DateTime dt in Data.Keys)
                {
                    if (dt>ev.DateStart && dt < ev.DateEnd)
                    {
                        if (Data[dt].ErrorLimits > maxLimitError)
                        {
                            maxLimitError = Data[dt].ErrorLimits;
                            hasError = true;
                        }
                    }
                }
                if (hasError)
                {
                    ev.HasError = String.Format("Длительность превышения {0:0}", maxLimitError);
                }
                else
                {
                    ev.HasError = "Нет ошибок";
                }

            }
        }


        public SortedList<DateTime,double> getSerieData(string desc, DateTime dateStart,DateTime dateEnd,bool add0=true)
        {
            SortedList<DateTime, double> data = new SortedList<DateTime, double>();

            foreach (DateTime date in Data.Keys)
            {
                if (date>=dateStart && date <= dateEnd)
                {
                    double val = Data[date].getInfo(desc);

                    if (add0 )
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
    }
}
