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
                MathO = double.NaN;
                if (data.Count == 0)
                    return;
                sorted = data.Values.ToList();
                sorted.Sort();
                filtered = (from d in sorted select d).ToList();
                if (data.Count <= 10)
                {
                    MathO = sorted.Average();
                    return;
                }


                double avg = sorted.Average();
                double mid = sorted[sorted.Count / 2];
                double x25 = sorted[sorted.Count / 4];
                double x75 = sorted[sorted.Count * 3 / 4];


                /*double min = x25 - 1.5 * (x75 - x25);
                double max = x75 + 1.5 * (x75 - x25);*/
                double min = x25;
                double max = x75;

                filtered = (from d in sorted where d >= min && d <= max select d).ToList();
                filtered = (from d in filtered where d * mid > 0 select d).ToList();

                if (filtered.Count > 0)
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
            sumTime = StayInfo.filtered.Sum() + RunInfo.filtered.Sum(); ;

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
        public int GG { get; set; }

        public string NasosType { get; set; }



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


        public DiagNasos(DateTime dateStart, DateTime dateEnd, int GG)
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
            IEnumerable<PuskStopInfo> req =
                    (from pi in diagDB.PuskStopInfoes
                     where
                         pi.GG == GG && pi.TypeData.Contains(type_data) &&
                         pi.TimeOff > DateStart && pi.TimeOn < DateEnd
                     orderby pi.TimeOn
                     select pi);



            PuskStopData prevData = null;
            PuskStopData last = null;
            bool first = true;
            foreach (PuskStopInfo pi in req)
            {
                if (first && pi.TimeOn > DateStart)
                {
                    try
                    {
                        PuskStopInfo rf =
                            (from p in diagDB.PuskStopInfoes
                             where
                                     p.GG == GG && p.TypeData.Contains(type_data) &&
                                     p.TimeOff < DateStart
                             orderby p.TimeOff descending
                             select p).First();

                        prevData = new PuskStopData();
                        prevData.TimeOff = rf.TimeOff;
                        prevData.ValueStart = rf.ValueStart;
                        prevData.ValueEnd = rf.ValueEnd;
                    }
                    catch { }
                }

                first = false;
                PuskStopData dat = new PuskStopData();
                dat.Length = pi.Length;
                dat.TimeOn = pi.TimeOn;
                dat.TimeOff = pi.TimeOff;
                dat.ValueEnd = pi.ValueEnd;
                dat.ValueStart = pi.ValueStart;
                dat.TypeData = pi.TypeData;
                dat.Comment = "";
                dat.PrevRecord = prevData;

                prevData = dat;

                result.Add(dat);

                if (DiffData != null && !DiffData.ContainsKey(pi.TypeData))
                    DiffData.Add(pi.TypeData, new List<PuskStopData>());
                DiffData[pi.TypeData].Add(dat);
                last = dat;
            }




            return result;
        }

        protected void CreateComments()
        {
            foreach (PuskStopData nd in FullNasosData)
            {
                if (nd.PrevRecord == null)
                    continue;
                Dictionary<string, double> onInf = new Dictionary<string, double>();
                IEnumerable<PuskStopData> req = from g in FullGGData
                                                where
                                                g.TimeOff > nd.PrevRecord.TimeOff && g.TimeOn < nd.TimeOff
                                                select g;
                foreach (PuskStopData ggRec in req)
                {
                    DateTime d1 = ggRec.TimeOn < nd.PrevRecord.TimeOff ? nd.PrevRecord.TimeOff : ggRec.TimeOn;
                    DateTime d2 = ggRec.TimeOff > nd.TimeOff ? nd.TimeOff : ggRec.TimeOff;
                    double len = (d2 - d1).TotalSeconds;
                    if (len > 0)
                    {
                        if (!onInf.ContainsKey(ggRec.TypeData))
                        {
                            onInf.Add(ggRec.TypeData, 0);
                        }
                        onInf[ggRec.TypeData] += len;
                    }
                }
                foreach (string type in onInf.Keys)
                {
                    double lenOn = onInf[type];
                    double sumLen = (nd.TimeOff - nd.PrevRecord.TimeOff).TotalSeconds;
                    if (lenOn / sumLen > 0.999)
                        nd.Comment += type + ";";
                }
            }

            /*foreach (PuskStopData ggRec in FullGGData)
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
            }*/
        }

        protected void UpdateGGData(string typeGG, AnalizeNasosData nasosData)
        {
            DiagDBEntities diagDB = new DiagDBEntities();
            IEnumerable<PuskStopData> ggData = from g in FullGGData where g.TypeData.Contains(typeGG) select g;
            int sign = NasosType == "MNU" ? -1 : 1;
            foreach (PuskStopData ggRec in ggData)
            {
                if ((ggRec.TimeOff - ggRec.TimeOn).TotalHours < 1)
                    continue;
                SortedList<DateTime, double> vals = new SortedList<DateTime, double>();

                IEnumerable<AnalogData> innerData = (from a in diagDB.AnalogDatas
                                                     where
                        a.Date >= ggRec.TimeOn && a.Date <= ggRec.TimeOff &&
                         a.pointType == NasosType && a.gg == GG
                                                     orderby a.Date
                                                     select a);
                if (innerData.Count() > 0)
                {
                    foreach (AnalogData ad in innerData)
                    {
                        if (!vals.ContainsKey(ad.Date))
                        {
                            vals.Add(ad.Date, ad.value);
                        }
                    }

                }

                IEnumerable<PuskStopData> req = from nd in FullNasosData
                                                where
                       (nd.TimeOn > ggRec.TimeOn && nd.TimeOn < ggRec.TimeOff) ||
                       (nd.TimeOff > ggRec.TimeOn && nd.TimeOff < ggRec.TimeOff)
                                                orderby nd.TimeOn
                                                select nd;
                if (req.Count() == 0)
                {
                    if (vals.Count >= 2)
                    {
                        double v1 = vals.First().Value;
                        double v2 = vals.Last().Value;
                        DateTime d1 = vals.First().Key;
                        DateTime d2 = vals.Last().Key;
                        double v = v2 - v1;
                        if (v * sign > 0)
                        {
                            nasosData.VInfo.Add(d1, v / (d2 - d1).TotalSeconds);
                        }
                    }
                }
                else
                {
                    List<PuskStopData> list = req.ToList();
                    if (list.Count() > 0 && innerData.Count() > 0)
                    {
                        PuskStopData first = list.First();
                        if (first.TimeOn > vals.First().Key)
                        {
                            double v1 = vals.First().Value;
                            double v2 = first.ValueStart;
                            DateTime d1 = vals.First().Key;
                            DateTime d2 = first.TimeOn;
                            double v = v2 - v1;
                            if (v * sign > 0 && (d2 - d1).TotalHours > 1)
                            {
                                nasosData.VInfo.Add(d1, v / (d2 - d1).TotalSeconds);
                            }
                        }
                        PuskStopData last = list.Last();
                        if (last.TimeOff < vals.Last().Key)
                        {
                            double v1 = last.ValueEnd;
                            double v2 = vals.Last().Value;
                            DateTime d1 = last.TimeOff;
                            DateTime d2 = vals.Last().Key;
                            double v = v2 - v1;
                            if (v * sign > 0 && (d2 - d1).TotalHours > 1)
                            {
                                nasosData.VInfo.Add(d1, v / (d2 - d1).TotalSeconds);
                            }
                        }
                    }
                }
            }

        }



        protected Dictionary<string, AnalizeNasosData> processNasosData(string typeGG)
        {
            Dictionary<string, AnalizeNasosData> result = new Dictionary<string, AnalizeNasosData>();
            result.Add("SVOD", new AnalizeNasosData());
            int sign = NasosType == "MNU" ? -1 : 1;

            foreach (string key in DataNasos.Keys)
            {
                result.Add(key, new AnalizeNasosData());
            }
            IEnumerable<PuskStopData> req = from nd in FullNasosData
                                            where
                                            nd.Comment.Contains(typeGG)
                                            select nd;
            foreach (PuskStopData nr in req)
            {
                DateTime d1 = (nr.PrevRecord != null) ?
                    nr.PrevRecord.TimeOff : DateStart;
                DateTime d2 = nr.TimeOn;
                DateTime d3 = nr.TimeOff;


                double sumTime = (d3 - d1).TotalSeconds;
                double sumWork = (d3 - d2).TotalSeconds;
                double sumStay = (d2 - d1).TotalSeconds;

                if (sumStay > 0)
                {
                    result["SVOD"].StayInfo.Add(d1, sumStay);
                }


                if (sumTime > 0)
                {

                    result["SVOD"].RunInfo.Add(d2, sumWork);
                    result[nr.TypeData].RunInfo.Add(d2, sumWork);
                }



                if (nr.PrevRecord != null && nr.TimeOn > nr.PrevRecord.TimeOff )
                {
                    double l1 = nr.PrevRecord.ValueEnd;
                    double l2 = nr.ValueStart;
                    double tim = (nr.TimeOn - nr.PrevRecord.TimeOff).TotalSeconds;
                    double v = l2 - l1;

                    double avgV = (l2 - l1) / tim;
                    if (v*sign>0)
                        result["SVOD"].VInfo.Add(nr.PrevRecord.TimeOff, avgV);
                }
            }


            UpdateGGData(typeGG, result["SVOD"]);
            foreach (string key in result.Keys)
            {
                result[key].calcAvg();
            }

            return result;
        }


        public bool ReadData(String type, int nasosCount, string typeCalcRun = "GG_UST")
        {
            NasosType = type;
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
