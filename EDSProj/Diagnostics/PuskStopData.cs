

using EDSProj.EDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{

    public class PuskStopData
    {
        public PuskStopData()
        {
            Comment = "";
            TypeData = "";
        }
        public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public double Length { get; set; }

        public double ValueStart { get; set; }
        public double ValueEnd { get; set; }

        public string Comment { get; set; }

        public PuskStopData PrevRecord { get; set; }


        public string TypeData { get; set; }
    }
    public class PuskStopReader
    {
        public class PuskStopReaderRecord
        {
            public string iess { get; set; }

            public string ValueIess { get; set; }
            public string DBRecord { get; set; }
            public bool inverted { get; set; }
            public int gg { get; set; }
            public EDSReportRequestRecord rec { get; set; }


        }

        protected static async Task<bool> UpdateValue(PuskStopData dataRecord, PuskStopReaderRecord readerRecord)
        {
            if (String.IsNullOrEmpty(readerRecord.ValueIess))
                return false;
            double val0 = await EDSClass.getValFromServer(readerRecord.ValueIess, dataRecord.TimeOn);
            double val1 = await EDSClass.getValFromServer(readerRecord.ValueIess, dataRecord.TimeOff);
            dataRecord.ValueStart = val0;
            dataRecord.ValueEnd = val1;
            return true;
        }


        public static async Task<List<PuskStopData>> AnalizePuskStopData(string pointName, DateTime dateStart, DateTime dateEnd, bool inverted)
        {
            List<PuskStopData> result = new List<PuskStopData>();
            try
            {
                DateTime ds = dateStart.AddSeconds(0);
                DateTime de = dateEnd.AddSeconds(0);

                double val0 = await EDSClass.getValFromServer(pointName, ds);
                if ((inverted && val0 < 0.5) || (!inverted && val0 > 0.9))
                {
                    PuskStopData record = new PuskStopData();
                    DateTime dt = await EDSClass.getValNextDate(pointName, ds, de, inverted ? "F_INTOOVER_DT" : "F_INTOUNDER_DT", 0.5);
                    if (dt > ds && dt < de)
                    {
                        ds = dt;
                        record.TimeOn = dateStart;
                        record.TimeOff = dt;
                        result.Add(record);
                    }
                    else
                    {
                        record.TimeOn = dateStart;
                        record.TimeOff = dateEnd;
                        result.Add(record);
                        return result;
                    }
                }
                while (ds < de)
                {
                    PuskStopData record = new PuskStopData();
                    DateTime dt = await EDSClass.getValNextDate(pointName, ds.AddSeconds(1), de, inverted ? "F_INTOUNDER_DT" : "F_INTOOVER_DT", 0.5);
                    Logger.Info(String.Format("Pusk {0}", dt));
                    if (dt > ds && dt < de)
                    {
                        record.TimeOn = dt;
                        DateTime dt1 = await EDSClass.getValNextDate(pointName, dt.AddSeconds(1), de, inverted ? "F_INTOOVER_DT" : "F_INTOUNDER_DT", 0.5);
                        Logger.Info(String.Format("Stop {0}", dt1));
                        if (dt1 > dt && dt1 < de)
                        {
                            record.TimeOff = dt1;
                            record.Length = (int)(record.TimeOff - record.TimeOn).TotalSeconds;
                            result.Add(record);
                            record = new PuskStopData();
                            ds = dt1.AddSeconds(1);
                        }
                        else
                        {
                            record.TimeOff = dateEnd;
                            record.Length = (int)(record.TimeOff - record.TimeOn).TotalSeconds;
                            result.Add(record);
                            return result;
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                return result;
            }
            catch
            {
                return result;
            }

        }

        public static async Task<SortedList<int, List<PuskStopData>>> AnalizePuskStopDataFull(List<PuskStopReaderRecord> req, DateTime dateStart, DateTime dateEnd)
        {
            EDSReport report = new EDSReport(dateStart, dateEnd, EDSReportPeriod.sec);
            Dictionary<string, EDSReportRequestRecord> records = new Dictionary<string, EDSReportRequestRecord>();

            foreach (PuskStopReaderRecord de in req)
            {
                EDSReportRequestRecord pointRec = null;
                if (!records.ContainsKey(de.iess))
                {
                    EDSPointInfo point = await EDSPointInfo.GetPointInfo(de.iess);
                    pointRec = report.addRequestField(point, EDSReportFunction.val);
                    records.Add(de.iess, pointRec);
                }
                else
                {
                    pointRec = records[de.iess];
                }
                de.rec = pointRec;

                if (!String.IsNullOrEmpty(de.ValueIess) && !records.ContainsKey(de.ValueIess))
                {
                    EDSPointInfo point = await EDSPointInfo.GetPointInfo(de.ValueIess);
                    pointRec = report.addRequestField(point, EDSReportFunction.val);
                    records.Add(de.ValueIess, pointRec);
                }



            }
            bool ok = await report.ReadData();
            List<DateTime> keys = report.ResultData.Keys.ToList();

            SortedList<int, List<PuskStopData>> FullResult = new SortedList<int, List<PuskStopData>>();

            foreach (PuskStopReaderRecord readField in req)
            {
                bool inverted = readField.inverted;
                List<PuskStopData> result = new List<PuskStopData>();
                FullResult.Add(FullResult.Count, result);
                SortedList<DateTime, double> data = new SortedList<DateTime, double>();
                foreach (DateTime dt in keys)
                {
                    data.Add(dt, report.ResultData[dt][readField.rec.Id]);

                }
                double val0 = data.Values.First();

                PuskStopData record = new PuskStopData();

                DateTime ds = dateStart.AddSeconds(0);
                if ((inverted && val0 < 0.5) || (!inverted && val0 > 0.9))
                {

                    try
                    {
                        DateTime dt = inverted ? data.First(de => de.Value > 0.5).Key : data.First(de => de.Value < 0.5).Key;
                        record.TimeOff = dt;
                        record.TimeOn = dateStart;
                        record.Length = (dt - dateStart).TotalSeconds;
                        ds = dt.AddSeconds(0);
                        result.Add(record);

                    }
                    catch
                    {
                        record.TimeOn = dateStart;
                        record.TimeOff = dateEnd;
                        record.Length = (dateEnd - dateStart).TotalSeconds;
                        result.Add(record);
                        continue;
                    }
                }

                while (ds <= dateEnd)
                {
                    try
                    {
                        DateTime dt = inverted ? data.First(de => de.Key > ds && de.Value < 0.5).Key :
                            data.First(de => de.Key > ds && de.Value > 0.5).Key;
                        record = new PuskStopData();
                        record.TimeOn = dt;
                        try
                        {
                            DateTime dt1 = inverted ? data.First(de => de.Key > dt && de.Value > 0.5).Key :
                                data.First(de => de.Key > dt && de.Value < 0.5).Key;
                            record.TimeOff = dt1;
                            record.Length = (record.TimeOff - record.TimeOn).TotalSeconds;
                            result.Add(record);
                            ds = dt1.AddSeconds(0);
                        }
                        catch
                        {
                            record.TimeOff = dateEnd;
                            record.Length = (record.TimeOff - record.TimeOn).TotalSeconds;
                            result.Add(record);
                            break;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            for (int i = 0; i < FullResult.Keys.Count; i++)
            {
                List<PuskStopData> data = FullResult[i];
                PuskStopReaderRecord reqRec = req[i];
                if (String.IsNullOrEmpty(reqRec.ValueIess))
                    continue;
                EDSReportRequestRecord requestRecord = records[reqRec.ValueIess];
                foreach (PuskStopData pi in data)
                {
                    if (report.ResultData.Keys.Contains(pi.TimeOn))
                        pi.ValueStart = report.ResultData[pi.TimeOn][requestRecord.Id];
                    else
                        pi.ValueStart = await EDSClass.getValFromServer(reqRec.ValueIess, pi.TimeOn);

                    if (report.ResultData.Keys.Contains(pi.TimeOff))
                        pi.ValueEnd = report.ResultData[pi.TimeOff][requestRecord.Id];
                    else
                        pi.ValueEnd = await EDSClass.getValFromServer(reqRec.ValueIess, pi.TimeOff);
                }
            }

            return FullResult;

        }


        public async static Task<bool> FillPuskStopData(List<PuskStopReaderRecord> reqList, DateTime dateStart, DateTime dateEnd)
        {
            try
            {
                EDSClass.Disconnect();
            }
            catch { }
            EDSClass.Connect();
            if (!EDSClass.Connected)
                return false;

            /*bool ok = await RefreshLevelsMNU(reqList, dateStart, dateEnd);
            return ok;*/

            SortedList<int, List<PuskStopData>> FullResult = await AnalizePuskStopDataFull(reqList, dateStart, dateEnd);


            for (int index = 0; index < reqList.Count; index++)
            {
                List<PuskStopData> data = FullResult[index];
                PuskStopReaderRecord reqRecord = reqList[index];
                int gg = reqRecord.gg;
                string typeData = reqRecord.DBRecord;

                if (data.Count > 0)
                {

                    DiagDBEntities diagDB = new DiagDBEntities();

                    PuskStopData first = data.First();
                    PuskStopData last = data.Last();
                    IQueryable<PuskStopInfo> reqFirst = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOff == first.TimeOn select pi;

                    if (reqFirst.Count() > 0)
                    {
                        PuskStopInfo firstDB = reqFirst.First();
                        firstDB.TimeOff = first.TimeOff;
                        firstDB.Length = (firstDB.TimeOff - firstDB.TimeOn).TotalSeconds;
                        firstDB.ValueEnd = first.ValueEnd;

                        diagDB.SaveChanges();

                        if (data.Count > 1)
                        {
                            data.RemoveAt(0);
                        }
                        else
                        {
                            continue;
                        }

                    }

                    IQueryable<PuskStopInfo> reqLast = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOn == last.TimeOff select pi;
                    if (reqLast.Count() > 0)
                    {
                        PuskStopInfo lastDB = reqLast.First();
                        lastDB.TimeOn = last.TimeOn;
                        lastDB.Length = (lastDB.TimeOff - lastDB.TimeOn).TotalSeconds;
                        lastDB.ValueStart = last.ValueStart;
                        diagDB.SaveChanges();
                        if (data.Count > 0)
                        {
                            data.Remove(last);
                        }
                        else
                        {
                            continue;
                        }

                    }



                    IQueryable<PuskStopInfo> req = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOn > dateStart && pi.TimeOn <= dateEnd select pi;
                    SortedList<DateTime, PuskStopInfo> dataDB = new SortedList<DateTime, PuskStopInfo>();
                    foreach (PuskStopInfo pi in req)
                    {
                        if (!dataDB.ContainsKey(pi.TimeOn))
                            dataDB.Add(pi.TimeOn, pi);
                    }

                    foreach (PuskStopData rec in data)
                    {
                        PuskStopInfo recDB = new PuskStopInfo();
                        if (dataDB.ContainsKey(rec.TimeOn))
                            recDB = dataDB[rec.TimeOn];

                        recDB.GG = gg;
                        recDB.TypeData = typeData;
                        recDB.TimeOn = rec.TimeOn;
                        recDB.TimeOff = rec.TimeOff;
                        recDB.Length = rec.Length;
                        recDB.ValueEnd = rec.ValueEnd;
                        recDB.ValueStart = rec.ValueStart;
                        recDB.Comment = "";

                        if (!dataDB.ContainsKey(rec.TimeOn))
                            diagDB.PuskStopInfoes.Add(recDB);

                    }
                    diagDB.SaveChanges();

                }
            }

            return true;
        }

        public static void RefreshPuskStopData(DateTime DateStart, DateTime DateEnd)
        {
            DiagDBEntities diagDB = new DiagDBEntities();
            for (int gg = 1; gg <= 10; gg++)
            {
                if (!Settings.Single.DiagSettings[gg - 1].activeReport)
                    continue;
                IEnumerable<PuskStopInfo> reqGG = from r in diagDB.PuskStopInfoes
                                                  where r.GG == gg && r.TypeData.StartsWith("GG_") &&
                                                  r.TimeOn < DateEnd && r.TimeOff > DateStart
                                                  select r;
                Logger.Info(String.Format("GG {0} - {1}", gg, reqGG.Count()));
                foreach (PuskStopInfo rec in reqGG)
                {

                    IEnumerable<PuskStopInfo> reqPI = from r in diagDB.PuskStopInfoes
                                                      where r.GG == gg && (!r.TypeData.StartsWith("GG")) &&

                                                      r.TimeOn >= rec.TimeOn && r.TimeOff <= rec.TimeOff
                                                      select r;
                    Logger.Info(String.Format("GG {0} {1} - {2}", rec.TypeData, rec.TimeOn, reqPI.Count()));
                    foreach (PuskStopInfo pi in reqPI)
                    {
                        if (!pi.Comment.Contains(rec.TypeData))
                            pi.Comment += String.Format("{0}; ", rec.TypeData, rec.ID);
                    }

                }
                diagDB.SaveChanges();
            }
        }

        public static bool CheckCrossData(DateTime DateStart, DateTime DateEnd)
        {
            DiagDBEntities diagDB = new DiagDBEntities();
            for (int gg = 1; gg <= 10; gg++)
            {
                if (!Settings.Single.DiagSettings[gg - 1].activeReport)
                    continue;
                Logger.Info(String.Format("GG {0}", gg));
                List<PuskStopInfo> data = (
                    from d in diagDB.PuskStopInfoes
                    where d.TimeOn <= DateEnd && d.TimeOff >= DateStart &&
                        d.GG == gg && d.TypeData.Contains("GG")
                    select d).ToList();
                List<int> removedItems = new List<int>();
                foreach (PuskStopInfo pi in data)
                {
                    if (removedItems.Contains(pi.ID))
                        continue;
                    IEnumerable<PuskStopInfo> crossData = from d in data
                                                          where
                          d.GG == pi.GG && d.TypeData == pi.TypeData && d.ID != pi.ID &&
                          (d.TimeOff >= pi.TimeOn && d.TimeOff <= pi.TimeOff ||
                          d.TimeOn >= pi.TimeOn && d.TimeOn <= pi.TimeOff)
                                                          select d;
                    if (crossData.Count() > 0)
                    {
                        Logger.Info(String.Format("Delete {0}", crossData.Count()));
                        foreach (PuskStopInfo d in crossData)
                        {
                            removedItems.Add(d.ID);
                            pi.TimeOn = d.TimeOn < pi.TimeOn ? d.TimeOn : pi.TimeOn;
                            pi.TimeOff = d.TimeOff > pi.TimeOff ? d.TimeOff : pi.TimeOff;
                            pi.Length = (pi.TimeOff - pi.TimeOn).TotalSeconds;
                            diagDB.PuskStopInfoes.Remove(d);
                        }
                    }
                    diagDB.SaveChanges();
                }

            }
            return true;

        }

        public static async Task<bool> FillAnalogData(DateTime DateStart, DateTime DateEnd, List<string> Types)
        {
            DiagDBEntities diagDB = new DiagDBEntities();
            for (int gg = 1; gg <= 10; gg++)
            {
                if (!Settings.Single.DiagSettings[gg-1].activeReport)
                    continue;
                try
                {
                    EDSClass.Disconnect();
                }
                catch { }
                EDSClass.Connect();

                Logger.Info(string.Format("GG{0}", gg));
                List<PuskStopPoint> points = (from p in diagDB.PuskStopPoints
                                              where p.gg == gg && p.analog == true && Types.Contains(p.pointType)
                                              select p).ToList();
                List<AnalogData> existData = (from a in diagDB.AnalogDatas
                                              where
                 a.gg == gg && Types.Contains(a.pointType) &&
                 a.Date >= DateStart && a.Date <= DateEnd
                                              select a).ToList();
                Dictionary<string, string> pointsDict = new Dictionary<string, string>();
                foreach (String type in Types)
                {
                    try
                    {
                        PuskStopPoint pt = (from p in diagDB.PuskStopPoints where p.gg == gg && p.pointType == type select p).First();
                        pointsDict.Add(type, pt.point);
                    }
                    catch
                    {
                        pointsDict.Add(type, "");
                    }

                }

                List<PuskStopInfo> data = (
                    from d in diagDB.PuskStopInfoes
                    where d.TimeOn <= DateEnd && d.TimeOff >= DateStart &&
                        d.GG == gg && d.TypeData.Contains("GG_RUN")
                    select d).ToList();


                foreach (PuskStopInfo ggRec in data)
                {

                    Logger.Info(String.Format("GG {0} {1} -{2}", gg, ggRec.TimeOn, ggRec.TimeOff));
                    foreach (string type in Types)
                    {
                        if (string.IsNullOrEmpty(pointsDict[type]))
                            continue;
                        foreach (DateTime dt in new DateTime[] { ggRec.TimeOn, ggRec.TimeOff })
                        {
                            IEnumerable<AnalogData> datas = (from a in existData where a.Date == dt && a.pointType == type select a);
                            AnalogData dat = null;
                            if (datas.Count() == 0)
                            {
                                dat = new AnalogData();
                                diagDB.AnalogDatas.Add(dat);
                                dat.pointType = type;
                                dat.gg = gg;
                                dat.Date = dt;
                                dat.value = await EDSClass.getValFromServer(pointsDict[type], dt);
                            }
                            else
                            {
                                dat = datas.First();
                            }

                        }
                    }
                    diagDB.SaveChanges();
                }


                DateTime date = DateTime.Parse(DateStart.ToString("dd.MM.yyyy HH:00"));
                while (date <= DateEnd)
                {
                    
                    Logger.Info(String.Format("GG {0} {1} ", gg, date));
                    /* if (gg == 5 && date < DateTime.Parse("01.06.2019"))
                    {
                        date = DateTime.Parse("01.06.2019");
                        continue;
                    }
                    if (gg == 3 && date < DateTime.Parse("07.05.2020"))
                    {
                        date = DateTime.Parse("07.05.2020");
                        continue;
                    }
                    if (gg == 1 && date < DateTime.Parse("21.04.2021"))
                    {
                        date = DateTime.Parse("21.04.2020");
                        continue;
                    }
                    if (gg == 8 && date < DateTime.Parse("01.07.2022"))
                    {
                        date = DateTime.Parse("01.07.2022");
                        continue;
                    }*/

                    if ( date < DateTime.Parse(Settings.Single.DiagSettings[gg - 1].dateStartRead))
                    {
                        date= DateTime.Parse(Settings.Single.DiagSettings[gg - 1].dateStartRead);
                        continue;
                    }

                    foreach (string type in Types)
                    {
                        if (string.IsNullOrEmpty(pointsDict[type]))
                            continue;

                        IEnumerable<AnalogData> datas = (from a in existData where a.Date == date && a.pointType == type select a);
                        AnalogData dat = null;
                        if (datas.Count() == 0)
                        {
                            dat = new AnalogData();
                            diagDB.AnalogDatas.Add(dat);
                            dat.pointType = type;
                            dat.gg = gg;
                            dat.Date = date;
                            dat.value = await EDSClass.getValFromServer(pointsDict[type], date);
                        }
                        else
                        {
                            dat = datas.First();
                        }
                    }
                    date = date.AddHours(3);

                }

            }
            return true;

        }




    }
}
