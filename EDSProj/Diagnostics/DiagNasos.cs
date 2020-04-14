using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDSProj.EDS;
using EDSProj.EDSWebService;

namespace EDSProj.Diagnostics
{

    public class AnalizeNasosData
    {
        public class AvgInfo
        {
            protected SortedList<DateTime, double> data;
            public List<double> sorted;
            public List<double> filtered;

            public double MathO;
            public int Count { get { return data.Count; } }
            public AvgInfo()
            {
                data = new SortedList<DateTime, double>();
                sorted = new List<double>();
                filtered = new List<double>();

            }
            public void Add(DateTime dt, double val)
            {
                while (data.ContainsKey(dt))
                {
                    dt = dt.AddMilliseconds(1);


                }
                data.Add(dt, val);
            }

            public void calcAVG()
            {
                if (data.Count == 0)
                    return;
                if (data.Count <= 3)
                {
                    MathO = data.Values.Average();
                    return;
                }

                sorted = data.Values.ToList();
                sorted.Sort();
                double avg = sorted.Average();
                double mid = sorted[sorted.Count / 2];
                double x25 = sorted[sorted.Count / 4];
                double x75 = sorted[sorted.Count * 3 / 4];

                double min = x25 - 1.5 * (x75 - x25);
                double max = x75 + 1.5 * (x75 - x25);

                filtered = (from d in sorted where d >= min && d <= max select d).ToList();


                MathO = filtered.Average();

            }

        }

        public double sumTime { get; set; }
        public double workRel { get; set; }

        public AvgInfo StayInfo { get; set; }
        public AvgInfo RunInfo { get; set; }
        public AvgInfo VInfo { get; set; }


        public AnalizeNasosData()
        {
            StayInfo = new AvgInfo();
            RunInfo = new AvgInfo();
            VInfo = new AvgInfo();
        }

        public void calcAvg()
        {
            StayInfo.calcAVG();
            RunInfo.calcAVG();
            VInfo.calcAVG();
        }
    }


    public class DiagNasos : INotifyPropertyChanged
    {
        public string Date { get; set; }
        public double timeGGRun { get; set; }
        public double timeGGStop { get; set; }


        public SortedList<DateTime, PuskStopData> OneTimeWorkInfo;
        public String OneTimeWork { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string GG { get; set; }



        public Dictionary<string, List<PuskStopData>> DataGG = new Dictionary<string, List<PuskStopData>>();
        public Dictionary<string, List<PuskStopData>> DataNasos = new Dictionary<string, List<PuskStopData>>();
        public Dictionary<string, AnalizeNasosData> NasosRunGG = new Dictionary<string, AnalizeNasosData>();
        public Dictionary<string, AnalizeNasosData> NasosStopGG = new Dictionary<string, AnalizeNasosData>();
        public Dictionary<string, AnalizeNasosData> NasosGG = new Dictionary<string, AnalizeNasosData>();
        public List<PuskStopData> FullNasosData = new List<PuskStopData>();
        public List<PuskStopData> FullGGData = new List<PuskStopData>();

        public AnalizeNasosData NasosRunGGView
        {
            get { return NasosRunGG.ContainsKey("SVOD") ? NasosRunGG["SVOD"] : new AnalizeNasosData(); }
        }

        public AnalizeNasosData NasosStopGGView
        {
            get { return NasosStopGG.ContainsKey("SVOD") ? NasosStopGG["SVOD"] : new AnalizeNasosData(); }
        }


        public DiagNasos(DateTime dateStart, DateTime dateEnd, string GG)
        {
            this.DateStart = dateStart;
            this.DateEnd = dateEnd;
            this.GG = GG;

        }


        public SortedList<DateTime, double> GetSerieData(DateTime dateStart, DateTime dateEnd, List<PuskStopData> data)
        {
            SortedList<DateTime, double> result = new SortedList<DateTime, double>();
            IEnumerable<PuskStopData> req = from rec in data where rec.TimeOn < dateEnd && rec.TimeOff > dateStart select rec;
            foreach (PuskStopData rec in req)
            {
                DateTime ds = rec.TimeOn > dateStart ? rec.TimeOn : dateStart;
                DateTime de = rec.TimeOff < dateEnd ? rec.TimeOff : dateEnd;
                result.Add(ds, 0);
                result.Add(ds.AddMilliseconds(1), 1);
                result.Add(de, 1);
                result.Add(de.AddMilliseconds(1), 0);
            }
            return result;
        }


        public SortedList<DateTime, double> dataLvl = new SortedList<DateTime, double>();
        public SortedList<DateTime, double> dataP = new SortedList<DateTime, double>();
        public async Task<bool> GetSerieLEDSData()
        {


            try
            {
                EDSClass.Disconnect();
            }
            catch
            {

            }
            EDSClass.Connect();

            EDSReport report = new EDSReport(DateStart, DateEnd, EDSReportPeriod.sec5);
            EDSPointInfo pointInfoLvl = await EDSPointInfo.GetPointInfo(GG + "VT_PS00A-01.MCR@GRARM");
            EDSPointInfo pointInfoP = await EDSPointInfo.GetPointInfo(GG + "VT_LP01AO-03.MCR@GRARM");
            EDSReportRequestRecord recLvl = report.addRequestField(pointInfoLvl, EDSReportFunction.val);
            EDSReportRequestRecord recP = report.addRequestField(pointInfoP, EDSReportFunction.val);
            bool ok = await report.ReadData();
            dataLvl = new SortedList<DateTime, double>();
            dataP = new SortedList<DateTime, double>();
            foreach (DateTime dt in report.ResultData.Keys)
            {
                dataLvl.Add(dt, report.ResultData[dt][recLvl.Id]);
                dataP.Add(dt, report.ResultData[dt][recP.Id]);
            }
            return true;
        }

        protected List<PuskStopData> createPuskStopData(string type_data, Dictionary<string, List<PuskStopData>> DiffData)
        {
            List<PuskStopData> result = new List<PuskStopData>();
            DiagDBEntities diagDB = new DiagDBEntities();
            int gg = Int32.Parse(GG);
            IQueryable<PuskStopInfo> req =
                    from pi in diagDB.PuskStopInfoes
                    where
                        pi.GG == gg && pi.TypeData.Contains(type_data) &&
                        pi.TimeOff > DateStart && pi.TimeOn < DateEnd
                    orderby pi.TimeOn
                    select pi;

            PuskStopData prevDataFirst = null;
            try
            {
                PuskStopInfo rf =
                    (from pi in diagDB.PuskStopInfoes
                     where
                             pi.GG == gg && pi.TypeData.Contains(type_data) &&
                             pi.TimeOff < DateStart
                     orderby pi.TimeOff descending
                     select pi).First();

                prevDataFirst = new PuskStopData();
                prevDataFirst.TimeOff = rf.TimeOff;
                prevDataFirst.ValueStart = rf.ValueStart;
                prevDataFirst.ValueEnd = rf.ValueEnd;
            }
            catch { }

            PuskStopData nextDataLast = null;
            try
            {
                PuskStopInfo rl =
                    (from pi in diagDB.PuskStopInfoes
                     where
                             pi.GG == gg && pi.TypeData.Contains(type_data) &&
                             pi.TimeOn > DateEnd
                     orderby pi.TimeOn
                     select pi).First();

                nextDataLast = new PuskStopData();
                nextDataLast.TimeOn = rl.TimeOn;
                nextDataLast.ValueStart = rl.ValueStart;
                nextDataLast.ValueEnd = rl.ValueEnd;

            }
            catch { }
            //List<PuskStopData> forPrevNext = new List<PuskStopData>();

            //forPrevNext.Add(prevDataFirst);
            PuskStopData prevData = prevDataFirst;
            foreach (PuskStopInfo pi in req)
            {
                PuskStopData dat = new PuskStopData();
                dat.Length = pi.Length;
                dat.TimeOn = pi.TimeOn;
                dat.TimeOff = pi.TimeOff;
                dat.ValueEnd = pi.ValueEnd;
                dat.ValueStart = pi.ValueStart;
                dat.TypeData = pi.TypeData;
                dat.Comment = "";
                dat.PrevRecord = prevData;
                if (dat.PrevRecord != null)
                    dat.PrevRecord.NextRecord = dat;
                prevData = dat;

                /*try
                {
                    dat.PrevRecord= (from d in forPrevNext where d.TimeOff < pi.TimeOn orderby d.TimeOff descending select d).First();
                    if (dat.PrevRecord != null)
                    {
                        dat.PrevRecord.NextRecord = dat;
                    }
                }
                catch { }
                forPrevNext.Add(dat);
                */

                result.Add(dat);

                if (DiffData != null && !DiffData.ContainsKey(pi.TypeData))
                    DiffData.Add(pi.TypeData, new List<PuskStopData>());
                DiffData[pi.TypeData].Add(dat);

            }

            if (result.Count > 0)
                result.Last().NextRecord = nextDataLast;

            return result;
        }

        protected void CreateComments()
        {
            foreach (PuskStopData ggRec in FullGGData)
            {
                IEnumerable<PuskStopData> req = from nd in FullNasosData
                                                where
                       nd.PrevRecord != null &&
                       nd.PrevRecord.TimeOff > ggRec.TimeOn &&
                       nd.TimeOff < ggRec.TimeOff
                                                select nd;
                foreach (PuskStopData pi in req)
                {
                    pi.Comment += ggRec.TypeData + ";";
                }
            }

        }



        protected Dictionary<string, AnalizeNasosData> processNasosData(string typeGG)
        {
            Dictionary<string, AnalizeNasosData> result = new Dictionary<string, AnalizeNasosData>();
            result.Add("SVOD", new AnalizeNasosData());


            foreach (string key in DataNasos.Keys)
            {
                result.Add(key, new AnalizeNasosData());
            }
            PuskStopData last = null;
            IEnumerable<PuskStopData> req = from nd in FullNasosData
                                            where
                                            nd.Comment.Contains(typeGG)
                                            select nd;
            foreach (PuskStopData nr in req)
            {
                DateTime d1 = nr.PrevRecord != null && nr.PrevRecord.TimeOff > DateStart ?
                    nr.PrevRecord.TimeOff : DateStart;
                DateTime d2 = nr.TimeOn > DateStart ? nr.TimeOn : DateStart;
                DateTime d3 = nr.TimeOff < DateEnd ? nr.TimeOff : DateEnd;


                double sumTime = (d3 - d1).TotalSeconds;
                double sumWork = (d3 - d2).TotalSeconds;
                double sumStay = (d2 - d1).TotalSeconds;

                if (sumStay > 0)
                {
                    result["SVOD"].StayInfo.Add(d1, sumStay);
                }
                result["SVOD"].RunInfo.Add(d2, sumWork);

                if (sumTime > 0)
                {
                    result["SVOD"].sumTime += sumTime;
                }



                result[nr.TypeData].RunInfo.Add(d2, sumWork);

                if (nr.PrevRecord != null && nr.TimeOn > nr.PrevRecord.TimeOff)
                {
                    double l1 = nr.PrevRecord.ValueEnd;
                    double l2 = nr.ValueStart;
                    double tim = (nr.TimeOn - nr.PrevRecord.TimeOff).TotalSeconds;
                    double avgV = (l2 - l1) / tim;

                    result["SVOD"].VInfo.Add(nr.PrevRecord.TimeOff, avgV);
                }
                last = nr;
            }
            if (last != null && last.NextRecord != null && last.NextRecord.TimeOn > last.TimeOff)
            {
                double l1 = last.ValueEnd;
                double l2 = last.NextRecord.ValueStart;
                double tim = (last.NextRecord.TimeOn - last.TimeOff).TotalSeconds;
                double avgV = (l2 - l1) / tim;

                result["SVOD"].VInfo.Add(last.TimeOff, avgV);

            }

            foreach (string key in result.Keys)
            {
                result[key].calcAvg();
            }

            return result;
        }


        public bool ReadData(String type, int nasosCount, string typeCalcRun = "GG_UST")
        {
            DataGG = new Dictionary<string, List<PuskStopData>>();
            DataGG.Add("GG_RUN", new List<PuskStopData>());
            DataGG.Add("GG_STOP", new List<PuskStopData>());
            DataGG.Add("GG_UST", new List<PuskStopData>());

            DataNasos = new Dictionary<string, List<PuskStopData>>();
            Date = DateStart.ToString("dd.MM");

            for (int nasos = 1; nasos <= nasosCount; nasos++)
            {
                DataNasos.Add(String.Format("{0}_{1}", type, nasos), new List<PuskStopData>());
            }

            FullGGData = createPuskStopData("GG", DataGG);
            FullNasosData = createPuskStopData(type, DataNasos);
            CreateComments();

            timeGGRun = 0;
            timeGGStop = 0;
            OneTimeWorkInfo = new SortedList<DateTime, PuskStopData>();

            IEnumerable<PuskStopData> req = from gg in DataGG[typeCalcRun] where gg.TimeOn <= DateEnd && gg.TimeOff >= DateStart select gg;
            foreach (PuskStopData rec in req)
            {
                DateTime start = rec.TimeOn > DateStart ? rec.TimeOn : DateStart;
                DateTime end = rec.TimeOff < DateEnd ? rec.TimeOff : DateEnd;
                timeGGRun += (end - start).TotalSeconds;
            }

            req = from gg in DataGG["GG_STOP"] where gg.TimeOn <= DateEnd && gg.TimeOff >= DateStart select gg;
            foreach (PuskStopData rec in req)
            {
                DateTime start = rec.TimeOn > DateStart ? rec.TimeOn : DateStart;
                DateTime end = rec.TimeOff < DateEnd ? rec.TimeOff : DateEnd;
                timeGGStop += (end - start).TotalSeconds;
            }

            NasosRunGG = processNasosData(typeCalcRun);
            NasosStopGG = processNasosData("GG_STOP");
            NasosGG = processNasosData("");




            /*NasosRunGG.cntPuskRel = NasosRunGG.cntPusk / timeGGRun;
            NasosStopGG.cntPuskRel = NasosStopGG.cntPusk / (timeGGStop);*/


            return true;
        }
    }
}
