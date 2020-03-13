using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDSProj.EDS;

namespace EDSProj.Diagnostics
{
    public class DiadOilClass : INotifyPropertyChanged
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
        public SortedList<DateTime, double> serieVOv;
        public SortedList<double, double> serieVRun;
        public SortedList<double, double> serieVStop;
        public SortedList<double, double> serieVFull;

        EDSReportRequestRecord recTUp;
        EDSReportRequestRecord recTDn;
        EDSReportRequestRecord recLvl1;
        EDSReportRequestRecord recLvl2;
        EDSReportRequestRecord recF;
        EDSReportRequestRecord recVOv;

        EDSReport report;

        public static SortedList<string, EDSPointInfo> AllPoints;

        public static async void init()
        {
            AllPoints = new SortedList<string, EDSPointInfo>();
            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr(".*VT_BG.*", AllPoints);
            ok &= await EDSPointsClass.getPointsArr(".*VT_GC.*", AllPoints);

        }

        public async Task<bool> ReadData(string GG)
        {
            report = new EDSReport(DateStart.AddSeconds(0), DateEnd, EDSReportPeriod.minute);

            recTUp = report.addRequestField(AllPoints[GG + "VT_BG01RI-25.MCR@GRARM"], EDSReportFunction.val);
            recTDn = report.addRequestField(AllPoints[GG + "VT_BG01RI-26.MCR@GRARM"], EDSReportFunction.val);
            recLvl1 = report.addRequestField(AllPoints[GG + "VT_BG01AI-01.MCR@GRARM"], EDSReportFunction.val);
            recLvl2 = report.addRequestField(AllPoints[GG + "VT_BG01AI-02.MCR@GRARM"], EDSReportFunction.val);
            recF = report.addRequestField(AllPoints[GG + "VT_GC01A-16.MCR@GRARM"], EDSReportFunction.val);
            if (GG == "05")
            {
                recVOv= report.addRequestField(AllPoints[GG + "VT_BG99A-02.MCR@GRARM"], EDSReportFunction.val);
            }
            else
            {
                recVOv = null;
            }



            bool ok = await report.ReadData();
            if (!ok)
                return false;


            return ok;

        }

        private double _V0;
        private double _V1mm { get; set; }
        private double _Tkoef { get; set; }
        private double _LDiff { get; set; }
        private int _fMinutes { get; set; }
        private int _Tbaz { get; set; }
        private int _datch { get; set; }
        private int _lMinutes { get; set; }

        public double V0 { get { return _V0; } set { _V0 = value; NotifyChanged("V0"); } }
        public double V1mm { get { return _V1mm; } set { _V1mm = value; NotifyChanged("V1mm"); } }
        public double Tkoef { get { return _Tkoef; } set { _Tkoef = value; NotifyChanged("Tkoef"); } }
        public double LDiff { get { return _LDiff; } set { _LDiff = value; NotifyChanged("LDiff"); } }
        public int fMinutes { get { return _fMinutes; } set { _fMinutes = value; NotifyChanged("fMinutes"); } }
        public int Tbaz { get { return _Tbaz; } set { _Tbaz = value; NotifyChanged("Tbaz"); } }
        public int datch { get { return _datch; } set { _datch = value; NotifyChanged("_datch"); } }
        public int lMinutes { get { return _lMinutes; } set { _lMinutes = value; NotifyChanged("_lMinutes"); } }


        public void recalcData()
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
            serieVOv = new SortedList<DateTime, double>();
            SortedList<DateTime, double> prevF = new SortedList<DateTime, double>();
            SortedList<DateTime, double> prevL1 = new SortedList<DateTime, double>();
            SortedList<DateTime, double> prevL2 = new SortedList<DateTime, double>();



            foreach (DateTime date in report.ResultData.Keys)
            {
                double TUp = report.ResultData[date][recTUp.Id];
                double TDn = report.ResultData[date][recTDn.Id];
                double Lvl1 = report.ResultData[date][recLvl1.Id];
                double Lvl2 = report.ResultData[date][recLvl2.Id];
                double vOv = 0;
                if (recVOv != null)
                {
                    vOv= report.ResultData[date][recVOv.Id];
                }
                
                double F = report.ResultData[date][recF.Id];

                double Tavg = (TUp + TDn) / 2;
                prevF.Add(date, F);
                prevL1.Add(date, Lvl1);
                prevL2.Add(date, Lvl2);
                if (date < DateStart.AddMinutes(fMinutes) || date<DateStart.AddMinutes(lMinutes))
                    continue;

                if (lMinutes > 0)
                {
                    Lvl1 = prevL1.Values.Average();
                    Lvl2 = prevL2.Values.Average();
                }

                double Lvl = datch == 0 ? (Lvl1 + Lvl2) / 2.0 : (datch == 1 ? Lvl1 : Lvl2);
                double lvlCor = F > 49 ? Lvl + LDiff : Lvl;

                double v1 = (V0 + lvlCor * V1mm) * (1 + (Tbaz - Tavg) * Tkoef)*1000;



                int index = 0;

                if (TUp != 0 && TDn != 0 && Lvl1 != 0 && Lvl2 != 0)
                {
                    if (recVOv!=null)
                        serieVOv.Add(date, vOv);
                    serieTUP.Add(date, TUp);
                    serieTDn.Add(date, TDn);
                    serieF.Add(date, F);
                    serieTAvg.Add(date, Tavg);
                    serieLvl1.Add(date, Lvl1);
                    serieLvl2.Add(date, Lvl2);

                    double l1Min = prevL1.Values.Min();
                    double l2Min = prevL2.Values.Min();
                    double l1Max = prevL1.Values.Max();
                    double l2Max = prevL2.Values.Max();

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

                while (prevL1.Keys.First().AddMinutes(lMinutes) < prevL1.Keys.Last())
                {                    
                    prevL1.RemoveAt(0);
                    prevL2.RemoveAt(0);
                }

            }


        }
    }
}
