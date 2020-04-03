

using EDSProj.EDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{
    public class PuskStopDataRecord
    {
        public DateTime TimeOn { get; set; }
        public DateTime TimeOff { get; set; }
        public double Length { get; set; }


    }
    public class PuskStopData
    {
        public static async Task<List<PuskStopDataRecord>> AnalizePuskStopData(string pointName, DateTime dateStart, DateTime dateEnd)
        {
            List<PuskStopDataRecord> result = new List<PuskStopDataRecord>();
            try
            {
                DateTime ds = dateStart.AddSeconds(0);
                DateTime de = dateEnd.AddSeconds(0);

                double val0 = await EDSClass.getValFromServer(pointName, ds);
                if (val0 > 0.9)
                {
                    PuskStopDataRecord record = new PuskStopDataRecord();
                    DateTime dt = await EDSClass.getValNextDate(pointName, ds, de, "F_INTOUNDER_DT", 0.5);
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
                    PuskStopDataRecord record = new PuskStopDataRecord();
                    DateTime dt = await EDSClass.getValNextDate(pointName, ds.AddSeconds(1), de, "F_INTOOVER_DT", 0.5);
                    Logger.Info(String.Format("Pusk {0}", dt));
                    if (dt > ds && dt < de)
                    {
                        record.TimeOn = dt;
                        DateTime dt1 = await EDSClass.getValNextDate(pointName, dt.AddSeconds(1), de, "F_INTOUNDER_DT", 0.5);
                        Logger.Info(String.Format("Stop {0}", dt1));
                        if (dt1 > dt && dt1 < de)
                        {
                            record.TimeOff = dt1;
                            record.Length = (int)(record.TimeOff - record.TimeOn).TotalSeconds;
                            result.Add(record);
                            record = new PuskStopDataRecord();
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

        public static async Task<List<PuskStopDataRecord>> AnalizePuskStopDataFull(string pointName, DateTime dateStart, DateTime dateEnd)
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

            List<PuskStopDataRecord> result = new List<PuskStopDataRecord>();
            PuskStopDataRecord record = new PuskStopDataRecord();

            DateTime ds = dateStart.AddSeconds(0);
            if (val0 > 0.9)            {
                
                try
                {
                    DateTime dt = data.First(de => de.Value < 0.5).Key;
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
                    DateTime dt = data.First(de => de.Key > ds && de.Value > 0.5).Key;
                    record = new PuskStopDataRecord();
                    record.TimeOn = dt;
                    try
                    {
                        DateTime dt1 = data.First(de => de.Key > dt && de.Value < 0.5).Key;
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

            List<PuskStopDataRecord> data = new List<PuskStopDataRecord>(); ;
            if (full)
            {
                data=await AnalizePuskStopDataFull(iess, dateStart, dateEnd);
            }
            else
            {
                data = await AnalizePuskStopData(iess, dateStart, dateEnd);
            }

            if (data.Count > 0)
            {
                int gg = Int32.Parse(GG);
                DiagDBEntities diagDB = new DiagDBEntities();

                PuskStopDataRecord first = data.First();
                IQueryable<PuskStopInfo> reqFirst = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOff == first.TimeOn select pi;
                if (reqFirst.Count() > 0)
                {
                    PuskStopInfo firstDB = reqFirst.First();
                    firstDB.TimeOff = first.TimeOff;
                    firstDB.Length = (firstDB.TimeOff - firstDB.TimeOn).TotalSeconds;
                    data.RemoveAt(0);
                    if (data.Count == 0)
                    {
                        diagDB.SaveChanges();
                        return true;
                    }
                }

                IQueryable<PuskStopInfo> req = from pi in diagDB.PuskStopInfoes where pi.GG == gg && pi.TypeData == typeData && pi.TimeOn > dateStart && pi.TimeOn <= dateEnd select pi;
                SortedList<DateTime, PuskStopInfo> dataDB = new SortedList<DateTime, PuskStopInfo>();
                foreach (PuskStopInfo pi in req)
                {
                    dataDB.Add(pi.TimeOn, pi);
                }

                foreach (PuskStopDataRecord rec in data)
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
