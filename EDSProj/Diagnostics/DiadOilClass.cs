using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EDSProj.EDS;

namespace EDSProj.Diagnostics
{
    public abstract class DiadOilClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string GG { get; set; }

        public SortedList<DateTime, double> serieTUP;
        public SortedList<DateTime, double> serieTDn;
        public SortedList<DateTime, double> serieF;
        public SortedList<DateTime, double> serieLvl1;
        public SortedList<DateTime, double> serieLvl2;
        public SortedList<DateTime, double> serieTAvg;
        public SortedList<DateTime, double> serieLvlCor;
        public SortedList<DateTime, double> serieV;
        public SortedList<double, double> serieVRun;
        public SortedList<double, double> serieVStop;
        public SortedList<double, double> serieVFull;

        protected EDSReportRequestRecord recTUp;
        protected EDSReportRequestRecord recTDn;
        protected EDSReportRequestRecord recLvl1;
        protected EDSReportRequestRecord recLvl2;
        protected EDSReportRequestRecord recF;

        protected EDSReport report;

        public static SortedList<string, EDSPointInfo> AllPoints;

        public static async void init()
        {
            AllPoints = new SortedList<string, EDSPointInfo>();
            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr(".*VT_BG.*", AllPoints);
            ok &= await EDSPointsClass.getPointsArr(".*VT_GC.*", AllPoints);

        }

        public abstract Task<bool> ReadData(string GG);


        private double _V0;
        private double _V1mm { get; set; }
        private double _Tkoef { get; set; }
        private double _LDiff { get; set; }
        private int _fMinutes { get; set; }
        private int _Tbaz { get; set; }
        private int _datch { get; set; }
        private int _lMinutes { get; set; }
        private int _vMinutes { get; set; }

        public double V0 { get { return _V0; } set { _V0 = value; NotifyChanged("V0"); } }
        public double V1mm { get { return _V1mm; } set { _V1mm = value; NotifyChanged("V1mm"); } }
        public double Tkoef { get { return _Tkoef; } set { _Tkoef = value; NotifyChanged("Tkoef"); } }
        public double LDiff { get { return _LDiff; } set { _LDiff = value; NotifyChanged("LDiff"); } }
        public int fMinutes { get { return _fMinutes; } set { _fMinutes = value; NotifyChanged("fMinutes"); } }
        public int Tbaz { get { return _Tbaz; } set { _Tbaz = value; NotifyChanged("Tbaz"); } }
        public int datch { get { return _datch; } set { _datch = value; NotifyChanged("_datch"); } }
        public int lMinutes { get { return _lMinutes; } set { _lMinutes = value; NotifyChanged("_lMinutes"); } }
        public int vMinutes { get { return _vMinutes; } set { _vMinutes = value; NotifyChanged("_vMinutes"); } }



        public virtual void recalcData()
        {
            serieTUP = new SortedList<DateTime, double>();
            serieTDn = new SortedList<DateTime, double>();
            serieLvl1 = new SortedList<DateTime, double>();
            serieLvl2 = new SortedList<DateTime, double>();

            serieLvlCor = new SortedList<DateTime, double>();
            serieTAvg = new SortedList<DateTime, double>();
            serieF = new SortedList<DateTime, double>();
            serieV = new SortedList<DateTime, double>();
            serieVRun = new SortedList<double, double>();
            serieVStop = new SortedList<double, double>();
            serieVFull = new SortedList<double, double>();
            SortedList<DateTime, double> prevF = new SortedList<DateTime, double>();
            SortedList<DateTime, double> prevL1 = new SortedList<DateTime, double>();
            SortedList<DateTime, double> prevL2 = new SortedList<DateTime, double>();


            foreach (DateTime date in report.ResultData.Keys)
            {
                double Lvl1 = report.ResultData[date][recLvl1.Id];
                double Lvl2 = report.ResultData[date][recLvl2.Id];
                double TUp = report.ResultData[date][recTUp.Id];
                double TDn = report.ResultData[date][recTDn.Id];
                double F = report.ResultData[date][recF.Id];

                double vOv = 0;

                double Tavg = (TUp + TDn) / 2;



                if (serieF.Count > 0 && Math.Abs(F - serieF.Values.Last()) > 40)
                {
                    prevL1.Clear();
                    prevL2.Clear();
                }

                prevF.Add(date, F);
                prevL1.Add(date, Lvl1);
                prevL2.Add(date, Lvl2);

                if (lMinutes > 0 && prevL1.Count > 0 && prevL2.Count > 0)
                {
                    double l1Min = prevL1.Values.Min();
                    double l2Min = prevL2.Values.Min();
                    double l1Max = prevL1.Values.Max();
                    double l2Max = prevL2.Values.Max();
                    Lvl1 = prevL1.Values.Average();
                    Lvl2 = prevL2.Values.Average();
                    /*Lvl1 = (l1Min + l1Max) / 2;
                    Lvl2 = (l2Min + l2Max) / 2;*/
                }




                double Lvl = datch == 0 ? (Lvl1 + Lvl2) / 2.0 : (datch == 1 ? Lvl1 : Lvl2);
                double lvlCor = F > 49 ? Lvl + LDiff : Lvl;

                double v1 = (V0 + lvlCor * V1mm) * (1 + (Tbaz - Tavg) * Tkoef) * 1000;


                if (TUp != 0 && TDn != 0 && Lvl1 != 0 && Lvl2 != 0)
                {
                    serieTUP.Add(date, TUp);
                    serieTDn.Add(date, TDn);
                    serieF.Add(date, F);
                    serieTAvg.Add(date, Tavg);
                    serieLvl1.Add(date, Lvl1);
                    serieLvl2.Add(date, Lvl2);



                    if ((prevF.Values.Max() < 1 || prevF.Values.Min() > 49 || fMinutes == 0) /*&& (l1Min / l1Max > 0.95) && (l2Min / l2Max > 0.95)*/)
                    {
                        serieLvlCor.Add(date, lvlCor);
                        serieV.Add(date, v1);
                        if (F > 49)
                            serieVRun.Add(serieVRun.Count(), v1);
                        else
                            serieVStop.Add(serieVStop.Count(), v1);
                        serieVFull.Add(serieVFull.Count(), v1);
                    }
                    else
                    {
                        serieLvlCor.Add(date, double.NegativeInfinity);
                        serieV.Add(date, double.NegativeInfinity);
                    }
                }


                while (prevF.Keys.First().AddMinutes(fMinutes) < prevF.Keys.Last())
                {
                    prevF.RemoveAt(0);
                }

                if (prevL1.Count > 0)
                    while (prevL1.Keys.First().AddMinutes(lMinutes) < prevL1.Keys.Last())
                    {
                        prevL1.RemoveAt(0);
                        prevL2.RemoveAt(0);
                    }

            }

            if (vMinutes > 1)
            {
                serieVFull = new SortedList<double, double>();
                serieVRun = new SortedList<double, double>();
                serieVStop = new SortedList<double, double>();

                SortedList<DateTime, double> otr = new SortedList<DateTime, double>();
                List<DateTime> keys = serieV.Keys.ToList();
                foreach (DateTime date in keys)
                {
                    double val = serieV[date];
                    if (val != double.NegativeInfinity)
                        otr.Add(date, val);

                    if (otr.Count == vMinutes && val != double.NegativeInfinity)
                    {
                        otr.RemoveAt(0);
                    }
                    if (val == double.NegativeInfinity)
                        otr.Clear();
                    else
                    {
                        serieV[date] = otr.Values.Average();
                    }
                    if (val != double.NegativeInfinity)
                    {
                        serieVFull.Add(serieVFull.Count, serieV[date]);
                        if (serieF[date] > 49)
                            serieVRun.Add(serieVRun.Count(), serieV[date]);
                        else
                            serieVStop.Add(serieVStop.Count(), serieV[date]);
                    }
                }

            }

        }

        


        protected string getValFromSerie(SortedList<DateTime, double> ser, DateTime date, string format)
        {
            if (ser.ContainsKey(date))
            {
                double val = ser[date];
                string valStr = val > double.NegativeInfinity ? String.Format("{0:" + format + "}", ser[date]) : "";
                return valStr;
            }
            else
            {
                return "";
            }

        }
        public void createText()
        {
            try
            {
                List<string> headers = new List<string>();
                headers.Add("L1");
                headers.Add("L2");
                headers.Add("T up");
                headers.Add("T dn");
                headers.Add("F");
                headers.Add("T avg");
                headers.Add("L кор");
                headers.Add("V calc");

                string header = "";
                foreach (string hdr in headers)
                {
                    header += String.Format("<th width='100'>{0}</th>", hdr);
                }

                string fn = Path.GetTempPath() + "/out.html";
                System.IO.TextWriter tW = new StreamWriter(fn);

                String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>точка</th>{0}</tr>", header);
                tW.WriteLine(txt);

                List<DateTime> keys = serieV.Keys.ToList();
                foreach (DateTime date in keys)
                {
                    string ValuesStr = "";
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieLvl1, date, "0.00"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieLvl2, date, "0.00"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieTUP, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieTDn, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieF, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieTAvg, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieLvlCor, date, "0.00"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieV, date, "0"));

                    tW.WriteLine(String.Format("<tr><th >{0}</th>{1}</tr>", date.ToString("dd.MM.yyyy HH:mm"), ValuesStr));
                }
                tW.WriteLine("</table></html>");
                tW.Close();

                Process.Start(fn);
            }
            catch
            {
                MessageBox.Show("Доступ к папке?");
            }
        }
    }
}
