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
    public class DiadOilRegulClass : INotifyPropertyChanged
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

        public SortedList<DateTime, double> serieV;
        public SortedList<double, double> serieVRun;
        public SortedList<double, double> serieVStop;
        public SortedList<double, double> serieVFull;
        public SortedList<DateTime, double> serieF;
        public SortedList<DateTime, double> serieL_OGA;
        public SortedList<DateTime, double> serieL_AGA;
        public SortedList<DateTime, double> serieL_SB;
        public SortedList<DateTime, double> serieL_LB;
        public SortedList<DateTime, double> serieL_NA;
        public SortedList<DateTime, double> serieL_RK;
        public SortedList<DateTime, double> serieD_MNU1;
        public SortedList<DateTime, double> serieD_MNU2;
        public SortedList<DateTime, double> serieD_MNU3;
        public SortedList<DateTime, double> serieD_LA1;
        public SortedList<DateTime, double> serieD_LA2;

        public SortedList<DateTime, double> serieD_MNU;
        public SortedList<DateTime, double> serieD_LA;
        public SortedList<DateTime, double> serieT_SB;

        public SortedList<DateTime, double> serieV_OGA;
        public SortedList<DateTime, double> serieV_AGA;
        public SortedList<DateTime, double> serieV_SB;
        public SortedList<DateTime, double> serieV_LB;
        public SortedList<DateTime, double> serieV_NA;
        public SortedList<DateTime, double> serieV_RK;


        protected EDSReportRequestRecord recF;
        protected EDSReportRequestRecord recL_OGA;
        protected EDSReportRequestRecord recL_AGA;
        protected EDSReportRequestRecord recL_SB;
        protected EDSReportRequestRecord recL_LB;
        protected EDSReportRequestRecord recL_NA;
        protected EDSReportRequestRecord recL_RK;

        protected EDSReportRequestRecord recD_MNU1;
        protected EDSReportRequestRecord recD_MNU2;
        protected EDSReportRequestRecord recD_MNU3;
        protected EDSReportRequestRecord recD_LA1;
        protected EDSReportRequestRecord recD_LA2;

        protected EDSReportRequestRecord recT_SB;

        protected EDSReport report;

        public static SortedList<string, EDSPointInfo> AllPoints;

        public static async void init()
        {
            AllPoints = new SortedList<string, EDSPointInfo>();
            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr(".*VT_PS.*", AllPoints);
            ok &= await EDSPointsClass.getPointsArr(".*VT_GC.*", AllPoints);

        }

        public async Task<bool> ReadData(string GG)
        {

            report = new EDSReport(DateStart.AddSeconds(0), DateEnd, EDSReportPeriod.minute);
            recF = report.addRequestField(AllPoints[GG + "VT_GC01A-16.MCR@GRARM"], EDSReportFunction.val);
            recL_OGA = report.addRequestField(AllPoints[GG + "VT_PS00A-03.MCR@GRARM"], EDSReportFunction.val);
            recL_AGA = report.addRequestField(AllPoints[GG + "VT_PS00A-11.MCR@GRARM"], EDSReportFunction.val);
            recL_SB = report.addRequestField(AllPoints[GG + "VT_PS00A-05.MCR@GRARM"], EDSReportFunction.val);
            recL_LB = report.addRequestField(AllPoints[GG + "VT_PS00AI-07.MCR@GRARM"], EDSReportFunction.val);
            recL_NA = report.addRequestField(AllPoints[GG + "VT_GC02A-86.MCR@GRARM"], EDSReportFunction.val);
            recL_RK = report.addRequestField(AllPoints[GG + "VT_GC02A-68.MCR@GRARM"], EDSReportFunction.val);

            recD_MNU1 = report.addRequestField(AllPoints[GG + "VT_PS01DI-01.MCR@GRARM"], EDSReportFunction.max);
            recD_MNU2 = report.addRequestField(AllPoints[GG + "VT_PS02DI-01.MCR@GRARM"], EDSReportFunction.max);
            recD_MNU3 = report.addRequestField(AllPoints[GG + "VT_PS03DI-01.MCR@GRARM"], EDSReportFunction.max);

            recD_LA1 = report.addRequestField(AllPoints[GG + "VT_PS04DI-01.MCR@GRARM"], EDSReportFunction.max);
            recD_LA2 = report.addRequestField(AllPoints[GG + "VT_PS05DI-01.MCR@GRARM"], EDSReportFunction.max);

            recT_SB = report.addRequestField(AllPoints[GG + "VT_PS00A-21.MCR@GRARM"], EDSReportFunction.val);




            bool ok = await report.ReadData();
            if (!ok)
                return false;


            return ok;


        }


        private double _V0_OGA;
        private double _V1mm_OGA;
        private double _V0_AGA;
        private double _V1mm_AGA;
        private double _V0_SB;
        private double _V1mm_SB;
        private double _V0_LB;
        private double _V1mm_LB;
        private double _L_NA;
        private double _V0_Opn_NA;
        private double _V0_Cls_NA;
        private double _V0_Opn_RK;
        private double _V0_Cls_RK;
        private double _L_RK;
        private double _V_TB;
        private double _Tkoef;
        private double _TMZ;
        private double _Tbaz;
        private double _vMinutes;
        private double _fMinutes;
        private double _HodRK;
        private double _HodNA;


        public double V0_OGA { get { return _V0_OGA; } set { _V0_OGA = value; NotifyChanged("V0_OGA"); } }
        public double V1mm_OGA { get { return _V1mm_OGA; } set { _V1mm_OGA = value; NotifyChanged("V1mm_OGA"); } }
        public double V0_AGA { get { return _V0_AGA; } set { _V0_AGA = value; NotifyChanged("V0_AGA"); } }
        public double V1mm_AGA { get { return _V1mm_AGA; } set { _V1mm_AGA = value; NotifyChanged("V1mm_AGA"); } }
        public double V0_SB { get { return _V0_SB; } set { _V0_SB = value; NotifyChanged("V0_SB"); } }
        public double V1mm_SB { get { return _V1mm_SB; } set { _V1mm_SB = value; NotifyChanged("V1mm_SB"); } }
        public double V0_LB { get { return _V0_LB; } set { _V0_LB = value; NotifyChanged("V0_LB"); } }
        public double V1mm_LB { get { return _V1mm_LB; } set { _V1mm_LB = value; NotifyChanged("V1mm_LB"); } }
        public double L_NA { get { return _L_NA; } set { _L_NA = value; NotifyChanged("L_NA"); } }

        public double V0_Opn_NA { get { return _V0_Opn_NA; } set { _V0_Opn_NA = value; NotifyChanged("V0_Opn_NA"); } }
        public double V0_Cls_NA { get { return _V0_Cls_NA; } set { _V0_Cls_NA = value; NotifyChanged("V0_Cls_NA"); } }
        public double L_RK { get { return _L_RK; } set { _L_RK = value; NotifyChanged("L_RK"); } }
        public double V0_Opn_RK { get { return _V0_Opn_RK; } set { _V0_Opn_RK = value; NotifyChanged("V0_Opn_RK"); } }
        public double V0_Cls_RK { get { return _V0_Cls_RK; } set { _V0_Cls_RK = value; NotifyChanged("V0_Cls_RK"); } }
        public double V_TB { get { return _V_TB; } set { _V_TB = value; NotifyChanged("V_TB"); } }
        public double Tkoef { get { return _Tkoef; } set { _Tkoef = value; NotifyChanged("Tkoef"); } }
        public double TMZ { get { return _TMZ; } set { _TMZ = value; NotifyChanged("TMZ"); } }
        public double Tbaz { get { return _Tbaz; } set { _Tbaz = value; NotifyChanged("Tbaz"); } }
        public double vMinutes { get { return _vMinutes; } set { _vMinutes = value; NotifyChanged("vMinutes"); } }
        public double fMinutes { get { return _fMinutes; } set { _fMinutes = value; NotifyChanged("fMinutes"); } }

        public double HodRK { get { return _HodRK; } set { _HodRK = value; NotifyChanged("HodRK"); } }
        public double HodNA { get { return _HodNA; } set { _HodNA = value; NotifyChanged("HodNA"); } }





        public virtual void recalcData()
        {

            serieF = new SortedList<DateTime, double>();
            serieV = new SortedList<DateTime, double>();
            serieVRun = new SortedList<double, double>();
            serieVStop = new SortedList<double, double>();
            serieVFull = new SortedList<double, double>();
            serieF= new SortedList<DateTime, double>();
            serieL_OGA= new SortedList<DateTime, double>();
            serieL_AGA= new SortedList<DateTime, double>();
            serieL_SB= new SortedList<DateTime, double>();
            serieL_LB= new SortedList<DateTime, double>();
            serieL_NA= new SortedList<DateTime, double>();
            serieL_RK= new SortedList<DateTime, double>();
            serieD_MNU1= new SortedList<DateTime, double>();
            serieD_MNU2= new SortedList<DateTime, double>();
            serieD_MNU3= new SortedList<DateTime, double>();
            serieD_LA1= new SortedList<DateTime, double>();
            serieD_LA2= new SortedList<DateTime, double>();

            serieD_MNU= new SortedList<DateTime, double>();
            serieD_LA= new SortedList<DateTime, double>();
            serieT_SB= new SortedList<DateTime, double>();

            serieV_OGA= new SortedList<DateTime, double>();
            serieV_AGA= new SortedList<DateTime, double>();
            serieV_SB= new SortedList<DateTime, double>();
            serieV_LB= new SortedList<DateTime, double>();
            serieV_NA = new SortedList<DateTime, double>();
            serieV_RK = new SortedList<DateTime, double>();

            SortedList<DateTime, double> prevF = new SortedList<DateTime, double>();
            SortedList<DateTime, double> prevDNasos = new SortedList<DateTime, double>();

            foreach (DateTime date in report.ResultData.Keys)
            {
                double T_SB = report.ResultData[date][recT_SB.Id];
                double F = report.ResultData[date][recF.Id];

                double L_OGA = report.ResultData[date][recL_OGA.Id];
                double L_AGA = report.ResultData[date][recL_AGA.Id];
                double L_LB = report.ResultData[date][recL_LB.Id];
                double L_SB = report.ResultData[date][recL_SB.Id];
                double L_NA = report.ResultData[date][recL_NA.Id];
                double L_RK = report.ResultData[date][recL_RK.Id];

                double D_MNU1 = report.ResultData[date][recD_MNU1.Id];
                double D_MNU2 = report.ResultData[date][recD_MNU2.Id];
                double D_MNU3 = report.ResultData[date][recD_MNU3.Id];
                double D_LA1 = report.ResultData[date][recD_LA1.Id];
                double D_LA2 = report.ResultData[date][recD_LA2.Id];

                prevF.Add(date,F);

                double D_MNU = (D_MNU1 > 0 || D_MNU2 > 0 || D_MNU3 > 0)?1:0;
                double D_LA = (D_LA1 > 0 || D_LA2 > 0) ? 1: 0;
                double D_LA_MNU = D_MNU > 0 || D_LA > 0?1:0;
                prevDNasos.Add(date, D_LA_MNU);

                serieT_SB.Add(date, T_SB);
                serieF.Add(date, F);

                /*serieL_OGA.Add(date, L_OGA);
                serieL_AGA.Add(date, L_AGA);
                serieL_LB.Add(date, L_LB);
                serieL_SB.Add(date, L_SB);*/
                serieL_NA.Add(date, L_NA);
                serieL_RK.Add(date, L_RK);

                /*serieD_MNU1.Add(date, D_MNU1);
                serieD_MNU2.Add(date, D_MNU2);
                serieD_MNU3.Add(date, D_MNU3);*/
                serieD_MNU.Add(date, D_MNU);

                /*serieD_LA1.Add(date, D_LA1);
                serieD_LA2.Add(date, D_LA2);*/
                serieD_LA.Add(date, D_LA);

                double V_OGA = double.NegativeInfinity;
                double V_AGA = double.NegativeInfinity;
                double V_SB = double.NegativeInfinity;
                double V_LB = double.NegativeInfinity;
                double V_NA = double.NegativeInfinity;
                double V_RK = double.NegativeInfinity;

                if ((prevF.Values.Max() < 1 || prevF.Values.Min() > 49 || fMinutes == 0)  )
                {
                    V_NA = L_NA * V0_Opn_NA / HodNA + (HodNA - L_NA) * V0_Cls_NA / HodNA;
                    V_RK = L_RK * V0_Opn_RK / HodRK + (HodRK - L_RK) * V0_Cls_RK / HodRK;
                }
                
                if (D_MNU==0 && D_LA == 0 && prevDNasos.Values.Max() < 1)
                {
                    V_OGA = V0_OGA + V1mm_OGA * L_OGA;
                    V_AGA = V0_AGA + V1mm_AGA * L_AGA;
                    V_SB = V0_SB + V1mm_SB * L_SB;
                    V_LB = V0_LB + V1mm_LB * L_LB;
                }

                double V = V_NA + V_RK + V_OGA + V_SB + V_LB+V_AGA;
                V = V > 0 ? V : Double.NegativeInfinity;
                if (V != double.NegativeInfinity)
                {
                    serieVFull.Add(serieVFull.Count, V);
                    if (serieF[date] > 49)
                        serieVRun.Add(serieVRun.Count(), V);
                    else
                        serieVStop.Add(serieVStop.Count(), V);
                }


                serieV_OGA.Add(date, V_OGA);
                serieV_AGA.Add(date, V_AGA);
                serieV_SB.Add(date, V_SB);
                serieV_LB.Add(date, V_LB);
                serieV_NA.Add(date, V_NA);
                serieV_RK.Add(date, V_RK);
                serieV.Add(date, V);
                while (prevF.Keys.First().AddMinutes(fMinutes) < prevF.Keys.Last())
                {
                    prevF.RemoveAt(0);
                }
                while (prevDNasos.Keys.First().AddMinutes(2) < prevDNasos.Keys.Last())
                {
                    prevDNasos.RemoveAt(0);
                }
            }

            if (vMinutes > 1)
            {
                serieVFull = new SortedList<double, double>();
                serieVRun = new SortedList<double, double>();
                serieVStop = new SortedList<double, double>();

                SortedList<DateTime, double> otr = new SortedList<DateTime, double>();
                SortedList<DateTime, double> otrF = new SortedList<DateTime, double>();
                List<DateTime> keys = serieV.Keys.ToList();
                foreach (DateTime date in keys)
                {
                    double val = serieV[date];
                    if (val != double.NegativeInfinity)
                    {
                        otr.Add(date, val);
                    }

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
                MathFunc.CreateAvgSerie(serieVFull, (int)vMinutes);
                MathFunc.CreateAvgSerie(serieVRun, (int)vMinutes);
                MathFunc.CreateAvgSerie(serieVStop, (int)vMinutes);
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
                    /*ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieLvl1, date, "0.00"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieLvl2, date, "0.00"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieTUP, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieTDn, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieF, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieTAvg, date, "0"));
                    ValuesStr += String.Format("<td align='right'>{0}</td>", getValFromSerie(serieLvlCor, date, "0.00"));*/
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
