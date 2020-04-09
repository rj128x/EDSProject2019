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
        public double sumLen { get; set; }
        public double sumStay { get; set; }

        public double workRel { get; set; }
        public double cntPusk { get; set; }        
        public double cntPuskRel { get; set; }


        public double maxLen { get; set; }
        public double minLen { get; set; }

        public DateTime? minTime { get; set; }
        public DateTime? maxTime { get; set; }


        public double avgLen { get; set; }
        public double avgStay { get; set; }


    }


    public class DiagNasos : INotifyPropertyChanged
    {
        public string Date { get; set; }
        public double timeGGRun { get; set; }
        public double timeGGStop { get; set; }

        public List<AnalizeNasosData> NasosRunGG=new List<AnalizeNasosData>();
        public List<AnalizeNasosData> NasosStopGG=new List<AnalizeNasosData>();
        public List<AnalizeNasosData> NasosGG = new List<AnalizeNasosData>();

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



        public List<PuskStopData> DataFull = new List<PuskStopData>();
        public List<List<PuskStopData>> NasosData = new List<List<PuskStopData>>();        
        public List<PuskStopData> GGRunData = new List<PuskStopData>();
        public List<PuskStopData> GGUstData = new List<PuskStopData>();
        public List<PuskStopData> GGStopData = new List<PuskStopData>();



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

        protected List<PuskStopData> createPuskStopData(string type_data, List<PuskStopData> FullData = null)
        {
            List<PuskStopData> result = new List<PuskStopData>();
            DiagDBEntities diagDB = new DiagDBEntities();
            int gg = Int32.Parse(GG);
            IQueryable<PuskStopInfo> req =
                    from pi in diagDB.PuskStopInfoes
                    where
                        pi.GG == gg && pi.TypeData == type_data &&
                        pi.TimeOff > DateStart && pi.TimeOn < DateEnd
                    select pi;
            foreach (PuskStopInfo pi in req)
            {
                PuskStopData dat = new PuskStopData();
                dat.Length = pi.Length;
                dat.TimeOn = pi.TimeOn;
                dat.TimeOff = pi.TimeOff;
                dat.Comment = pi.Comment;
                result.Add(dat);
                if (FullData != null)
                {
                    DateTime dt = dat.TimeOn;
                    dt = dt.AddMilliseconds(1);
                    FullData.Add(dat);
                }
            }
            return result;
        }


        protected AnalizeNasosData processNasosData(string type,
            List<PuskStopData> NasosData, bool calcOneTime)
        {
            AnalizeNasosData result = new AnalizeNasosData();
            result.maxLen = double.NaN;
            result.minLen = double.NaN;

            IEnumerable<PuskStopData> req = from nd in NasosData
                                    where                       
                                    nd.TimeOn > DateStart && nd.TimeOff < DateEnd && 
                                    nd.Comment.Contains(type) orderby nd.TimeOn
                                            select nd;

            DateTime prevDateOn = DateStart;
            foreach (PuskStopData rec in req)
            {
                result.sumLen += rec.Length;


                result.cntPusk++;
                if (double.IsNaN(result.minLen) || rec.Length < result.minLen)
                {
                    result.minLen = rec.Length;
                    result.minTime = rec.TimeOn;
                }

                if (double.IsNaN(result.maxLen) || rec.Length > result.maxLen)
                {
                    result.maxLen = rec.Length;
                    result.maxTime = rec.TimeOn;
                }



                if (calcOneTime)
                {
                    IEnumerable<PuskStopData> req2 = from nd in req
                                                     where
                                      rec.TimeOn < nd.TimeOff && rec.TimeOff > nd.TimeOn &&
                                      nd.TimeOn > DateStart && nd.TimeOff < DateEnd  
                                                     select nd;
                    if (req2.Count() > 1)
                    {
                        PuskStopData one = new PuskStopData();
                        one.TimeOn = DateTime.MinValue;
                        one.TimeOff = DateTime.MaxValue;
                        foreach (PuskStopData dt in req2)
                        {
                            one.TimeOn = dt.TimeOn > one.TimeOn ? dt.TimeOn : one.TimeOn;
                            one.TimeOff = dt.TimeOff < one.TimeOff ? dt.TimeOff : one.TimeOff;
                        }
                        if (!OneTimeWorkInfo.ContainsKey(one.TimeOn))
                        {
                            one.Length = (one.TimeOff - one.TimeOn).TotalSeconds;
                            OneTimeWorkInfo.Add(one.TimeOn, one);

                        }
                    }
                }
            }
            double timeSum = (type == "STOP" ? timeGGStop : timeGGRun);


            result.sumStay = timeSum-result.sumLen;
            result.workRel = result.sumLen / result.sumStay;
            result.avgLen = result.sumLen / result.cntPusk;
            result.avgStay = result.sumStay/(result.cntPusk);
            result.cntPuskRel = result.cntPusk / timeSum;
            return result;
        }



        public async Task<bool> ReadData(String type, int nasosCount,string typeCalcRun="GG_UST")
        {
            Date = DateStart.ToString("dd.MM");
            GGRunData = createPuskStopData("GG_RUN");
            GGStopData = createPuskStopData("GG_STOP");
            GGUstData = createPuskStopData(typeCalcRun);

            NasosData.Add(DataFull);
            for (int nasos = 1; nasos <= nasosCount;nasos++) {
                NasosData.Add(createPuskStopData(String.Format("{0}_{1}",type,nasos), DataFull));                
            }

            timeGGRun = 0;
            timeGGStop = 0;
            OneTimeWorkInfo = new SortedList<DateTime, PuskStopData>();

            IEnumerable<PuskStopData> req = from gg in GGUstData where gg.TimeOn <= DateEnd && gg.TimeOff >= DateStart select gg;
            foreach (PuskStopData rec in req)
            {
                DateTime start = rec.TimeOn > DateStart ? rec.TimeOn : DateStart;
                DateTime end = rec.TimeOff < DateEnd ? rec.TimeOff : DateEnd;
                timeGGRun += (end - start).TotalSeconds;
            }

            req = from gg in GGStopData where gg.TimeOn <= DateEnd && gg.TimeOff >= DateStart select gg;
            foreach (PuskStopData rec in req)
            {
                DateTime start = rec.TimeOn > DateStart ? rec.TimeOn : DateStart;
                DateTime end = rec.TimeOff < DateEnd ? rec.TimeOff : DateEnd;
                timeGGStop += (end - start).TotalSeconds;
            }



            NasosRunGG.Add(processNasosData(typeCalcRun,  DataFull, true));
            NasosStopGG.Add(processNasosData("STOP", DataFull, true));
            NasosGG.Add(processNasosData("GG",  DataFull, true));
            for (int nasos = 1; nasos <= nasosCount; nasos++)
            {
                NasosRunGG.Add(processNasosData(typeCalcRun, NasosData[nasos], false));
                NasosStopGG.Add(processNasosData("STOP",NasosData[nasos], false));
                NasosGG.Add(processNasosData("GG", NasosData[nasos], false));
            }


            OneTimeWork = String.Format("Одновр пуск насосов: {0} раз", OneTimeWorkInfo.Count());


            /*NasosRunGG.cntPuskRel = NasosRunGG.cntPusk / timeGGRun;
            NasosStopGG.cntPuskRel = NasosStopGG.cntPusk / (timeGGStop);*/


            return true;
        }
    }
}
