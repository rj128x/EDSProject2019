

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
        public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public double Length { get; set; }


    }
    public class PuskStopReader
    {
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

        public static async Task<List<PuskStopData>> AnalizePuskStopDataFull(string pointName, DateTime dateStart, DateTime dateEnd, bool inverted)
        {
            EDSReport report = new EDSReport(dateStart, dateEnd, EDSReportPeriod.sec);
            EDSPointInfo point = await EDSPointInfo.GetPointInfo(pointName);
            EDSReportRequestRecord pointRec = report.addRequestField(point, EDSReportFunction.val);
            bool ok = await report.ReadData();
            List<DateTime> keys = report.ResultData.Keys.ToList();
            SortedList<DateTime, double> data = new SortedList<DateTime, double>();
            foreach (DateTime dt in keys)
            {
                data.Add(dt, report.ResultData[dt][pointRec.Id]);
            }
            double val0 = data.Values.First();

            List<PuskStopData> result = new List<PuskStopData>();
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
                    return result;
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
                        return result;
                    }
                }
                catch
                {
                    return result;
                }
            }

            return result;



        }


        public async static Task<bool> FillPuskStopData(string GG, string typeData, string iess, DateTime dateStart, DateTime dateEnd, bool full)
        {
            try
            {
                EDSClass.Disconnect();
            }
            catch { }
            EDSClass.Connect();
            if (!EDSClass.Connected)
                return false;

            List<PuskStopData> data = new List<PuskStopData>(); ;
            if (full)
            {
                data = await AnalizePuskStopDataFull(iess, dateStart, dateEnd, true);
            }
            else
            {
                data = await AnalizePuskStopData(iess, dateStart, dateEnd, false);
            }

            if (data.Count > 0)
            {
                int gg = Int32.Parse(GG);
                DiagDBEntities diagDB = new DiagDBEntities();

                PuskStopData first = data.First();
                PuskStopData last = data.Last();
                IQueryable<PuskStopInfo> reqFirst = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOff == first.TimeOn select pi;

                if (reqFirst.Count() > 0)
                {
                    PuskStopInfo firstDB = reqFirst.First();
                    firstDB.TimeOff = first.TimeOff;
                    firstDB.Length = (firstDB.TimeOff - firstDB.TimeOn).TotalSeconds;

                    diagDB.SaveChanges();

                    if (data.Count > 1)
                    {
                        data.RemoveAt(0);
                    }
                    else
                    {
                        return true;
                    }

                }

                IQueryable<PuskStopInfo> reqLast = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOn == last.TimeOff select pi;
                if (reqLast.Count() > 0)
                {
                    PuskStopInfo lastDB = reqLast.First();
                    lastDB.TimeOn = last.TimeOn;
                    lastDB.Length = (lastDB.TimeOff - lastDB.TimeOn).TotalSeconds;
                    diagDB.SaveChanges();
                    if (data.Count > 0)
                    {
                        data.Remove(last);
                    }
                    else
                    {
                        return true;
                    }

                }



                IQueryable<PuskStopInfo> req = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOn > dateStart && pi.TimeOn <= dateEnd select pi;
                SortedList<DateTime, PuskStopInfo> dataDB = new SortedList<DateTime, PuskStopInfo>();
                foreach (PuskStopInfo pi in req)
                {
                    dataDB.Add(pi.TimeOn, pi);
                }

                foreach (PuskStopData rec in data)
                {
                    PuskStopInfo recDB = new PuskStopInfo();
                    if (dataDB.ContainsKey(rec.TimeOn))
                        recDB = dataDB[rec.TimeOn];

                    recDB.GG = Int32.Parse(GG);
                    recDB.TypeData = typeData;
                    recDB.TimeOn = rec.TimeOn;
                    recDB.TimeOff = rec.TimeOff;
                    recDB.Length = rec.Length;

                    if (!dataDB.ContainsKey(rec.TimeOn))
                        diagDB.PuskStopInfoes.Add(recDB);

                }
                diagDB.SaveChanges();


                return true;
            }
            return false;
        }
    }
}
