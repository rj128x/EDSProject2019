﻿using EDSProj;
using EDSProj.EDS;
using EDSProj.EDSWebService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace NPRCHApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

        public class BlockData
    {
        public string BlockNumber { get; set; }
        public double PNom { get; set; }
        public double FNom { get; set; }
        public double PMin { get; set; }
        public double PMax { get; set; }
        public BlockData(string num,double pnom,double pmin,double pmax,double Fnom)
        {
            BlockNumber = num;
            PMax = pmax;
            PMin = pmin;
            PNom = pnom;
            FNom = Fnom;
        }

    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //public RecordDay RecDay;
        public void NotifyPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(PropertyName));
            }
        }

        private static Action EmptyDelegate = delegate () { };
        public int DiffHour = 0;
        protected string statusText;
        Dictionary<string, DateTime> FTPData = new Dictionary<string, DateTime>();
        public string StatusText {
            get { return statusText; }
            set {
                statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }

        public  MainWindow()
        {            
            InitializeComponent();
            Settings.init(System.AppDomain.CurrentDomain.BaseDirectory + "/Data/settings.xml");
            Logger.InitFileLogger(System.AppDomain.CurrentDomain.BaseDirectory + "/logs/", "nprch");
            SettingsNPRCH.init(System.AppDomain.CurrentDomain.BaseDirectory + "/Data/SettingsNPRCH.xml");
            grdStatus.DataContext = EDSClass.Single;
            init();
            


            Logger.Info("start");
            txtDiffHour.Text = "3";
            //chart.init();
            clndDate.SelectedDate = DateTime.Now.Date.AddDays(-1);
            statusBar.DataContext = this;
        }

        /*public void prepareChart(ZedGraphControl chart)
        {
            chart.GraphPane.CurveList.Clear();
            chart.GraphPane.XAxis.Type = AxisType.Date;
            chart.GraphPane.XAxis.Scale.Format = "mm:ss";
            chart.GraphPane.XAxis.Title.IsVisible = false;
            chart.GraphPane.YAxis.Title.IsVisible = true;
            chart.GraphPane.YAxis.Title.FontSpec.Size = 10;            
            chart.GraphPane.Title.IsVisible = false;

        }

        public void prepareChartDouble(ZedGraphControl chart)
        {
            chart.GraphPane.CurveList.Clear();
            chart.GraphPane.XAxis.Type = AxisType.Linear;
            chart.GraphPane.XAxis.Title.IsVisible = false;
            chart.GraphPane.YAxis.Title.IsVisible = true;
            chart.GraphPane.YAxis.Title.FontSpec.Size = 10;
            chart.GraphPane.Title.IsVisible = false;
            chart.GraphPane.XAxis.Scale.Format = "0.00";

        }*/

         public  async void init()
        {
            bool ok=await RecordHour.initEDS();
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLoadFiles_Click(object sender, RoutedEventArgs e)
        {
            StatusText = "Загрузка";
            string FN = chbReserv.IsChecked.Value ? "/Data/settingsNPRCHReserv.xml" : "/Data/settingsNPRCH.xml";
            SettingsNPRCH.init(System.AppDomain.CurrentDomain.BaseDirectory + FN);
            btnLoadFiles.IsEnabled = false;
            Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(
                        delegate { LoadFiles(); }
                     )
             );
            btnLoadFiles.IsEnabled = true;

            // btnLoadFiles.Visibility = Visibility.Visible;
        }


        private async Task<bool> LoadFiles()
        {
            DateTime date = clndDate.SelectedDate.Value;
            FTPData = new Dictionary<string, DateTime>();
            DiffHour = Int32.Parse(txtDiffHour.Text);

            List<RecordHour> hoursData = new List<RecordHour>();
            RecordHour.DataSDay = new Dictionary<string, List<Record>>();
            RecordHour prev = null;
            SortedList<string, EDSReportRequestRecord> EDSData = new SortedList<string, EDSReportRequestRecord>();
            EDSReport report = null;
            if (chbNPRCHMaket.IsChecked.Value)
            {
                try
                {
                    EDSClass.Disconnect();
                }
                catch { }
                EDSClass.Connect();
                report = new EDSReport(date.AddHours(-DiffHour + 5), date.AddHours(-DiffHour + 5 + 24),EDSReportPeriod.hour);

                foreach (KeyValuePair<string, BlockData> de in SettingsNPRCH.BlocksDict)
                {
                    int gg = Int32.Parse(de.Key);
                    string iess = String.Format("MC_NPRCH_GG{0}.EDS@CALC", gg);
                    EDSReportRequestRecord rec=report.addRequestField(RecordHour.EDSPoints[iess], EDSReportFunction.val);
                    EDSData.Add(de.Key, rec);
                }
                bool ok=await report.ReadData();
            }
            for (int hour = 0; hour <= 23; hour++)
            {
                foreach (KeyValuePair<string, BlockData> de in SettingsNPRCH.BlocksDict)
                {
                    StatusText = "Загрузка " + hour.ToString();
                    DateTime dt = date.AddHours(hour);
                    bool ok = FTPClass.CheckFile(dt.AddHours(-DiffHour), de.Key);
                    string header = String.Format("ГГ{3} {0} [{1} UTC] {2}", dt.ToString("dd.MM HH"), dt.AddHours(-DiffHour).ToString("dd.MM HH"), ok, de.Key);
                    FTPData.Add(header,dt);
                    if (ok)
                    {
                        StatusText = "Обработка " + hour.ToString();
                        RecordHour rh = new RecordHour(de.Value);
                        rh.Header = header;
                        rh.processFile(dt, DiffHour, false);
                        if (chbNPRCHMaket.IsChecked.Value)
                        {
                            double val=report.ResultData[report.DateStart.AddHours(hour)][EDSData[de.Key].Id];
                            rh.NPRCHMaket = val > 0 ? "+" : " ";                            
                        }
                        hoursData.Add(rh);

                        prev = rh;
                    }
                }
                System.Threading.Thread.Sleep(10);
            }
            //hoursData.Last().NoReactComment = "";
            //hoursData.Last().calcReact(RecordHour.DataSDay["10"],false);
            //RecDay.calcRHO();
            // StatusText = RecDay.RHO.ToString();
            lbHours.ItemsSource = FTPData;
            lbHours.DisplayMemberPath = "Key";
            grdDayData.ItemsSource = hoursData;
            tabDay.IsSelected = true;

            btnLoadFiles.IsEnabled = true;
            return true;
        }

        private void lbHours_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                KeyValuePair<string, DateTime> de = (KeyValuePair<string, DateTime>)lbHours.SelectedItem;
                string key = de.Key.Replace("ГГ", "");
                string[] arr = key.Split(new char[] { ' ' });
                string block = arr[0];
                loadDayData(de.Value, block);

            }

            catch { }
        }

        private void grdDayData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                RecordHour rh = (RecordHour)grdDayData.SelectedValue;
                lbHours.Focus();                
                lbHours.SelectedIndex = FTPData.Keys.ToList().IndexOf(rh.Header);
                lbHours_MouseDoubleClick(lbHours, e);
                e.Handled = true;
            }
            catch { }
        }


        private  void  loadDayData(DateTime date,string block)
        {
            /* try
             {           */
            Dictionary<DateTime, Record> Data;

            RecordHour recHour = new RecordHour(SettingsNPRCH.BlocksDict[block]);
             recHour.processFile(date, DiffHour, true);
            txtFile.Text = recHour.getText();
            Data = recHour.getData();

            SortedList<DateTime, double> list_P = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_F = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Fmin = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Fmax = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Pmin = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Pmax = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Pzad = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Pperv = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Pzvn = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_X = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Y = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Xdx = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_Ydy = new SortedList<DateTime, double>();
            SortedList<DateTime, double> list_NR = new SortedList<DateTime, double>();
            list_Fmin.Add(date, 49.98);
            list_Fmin.Add(date.AddHours(1), 49.98);
            list_Fmax.Add(date, 50.02);
            list_Fmax.Add(date.AddHours(1), 50.02);

            foreach (Record rec in Data.Values)
            {
                list_P.Add(rec.Date, rec.P_fakt);
                list_Pmin.Add(rec.Date, rec.P_min);
                list_Pmax.Add(rec.Date, rec.P_max);
                list_Pzad.Add(rec.Date, rec.P_plan);
                list_F.Add(rec.Date, rec.F_gc);
                list_Pperv.Add(rec.Date, rec.P_planFull);
                list_Pzvn.Add(rec.Date, rec.P_zvn);
                list_X.Add(rec.Date, rec.X_avg);
                list_Y.Add(rec.Date, rec.Y_avg);
                list_Xdx.Add(rec.Date, rec.Xdx_avg);
                list_Ydy.Add(rec.Date, rec.Ydy_avg);
                list_NR.Add(rec.Date, rec.NoReact);

            }

            grdData.ItemsSource = Data.Values;

            chart.initControl();
            chart.init(true, "HH:mm:ss");
            //chart.ShowCrossHair = true;
            chart.AllYAxisIsVisible = true;

            //prepareChart(chart.chart);
            chart.AddSerie("P факт", list_P, System.Drawing.Color.Red, true, false,false,0);
            chart.AddSerie("P плн", list_Pzad, System.Drawing.Color.Orange, true, false, false, 0);
            chart.AddSerie("P мин", list_Pmin, System.Drawing.Color.Pink, true, false, true, 0);
            chart.AddSerie("P макс", list_Pmax, System.Drawing.Color.Pink, true, false, true, 0);
            chart.AddSerie("F мин", list_Fmin, System.Drawing.Color.LightBlue, true, false,true, 1);
            chart.AddSerie("F макс", list_Fmax, System.Drawing.Color.LightBlue, true, false,true, 1);

            chart.AddSerie("F", list_F, System.Drawing.Color.Blue, true, false,false, 1);

            chart.AddSerie("P плн сум", list_Pperv, System.Drawing.Color.Purple, true,false, false,0);

            chart.AddSerie("P звн", list_Pzvn, System.Drawing.Color.Orange, true, false, false, 2, true);

            chart.init(true, "HH:mm:ss");            
            chart.AddSerie("X", list_X, System.Drawing.Color.LightGreen, true, false,true, 0, true);
            chart.AddSerie("Y", list_Y, System.Drawing.Color.LightYellow, true, false,true, 0, true);
            chart.AddSerie("Xdx", list_Xdx, System.Drawing.Color.Green, true, false, false, 1, true);
            chart.AddSerie("Ydy", list_Ydy, System.Drawing.Color.Yellow, true, false, false, 1, true);
            chart.AddSerie("NR", list_NR, System.Drawing.Color.IndianRed, true, false,false, 2, true);

            

            tabHour.IsSelected = true;





            tabHour.Header = string.Format("{0}-{1} [GMT {2}]", date.ToString("dd.MM HH:00"), date.AddHours(1).ToString("dd.MM HH:00"), DiffHour);
            /*}
            catch (Exception e1)
            {
                MessageBox.Show("Ошибка при загрузке данных");
            }*/
        }

        private void btnCalcStatizm_Click(object sender, RoutedEventArgs e)
        {

            double statizm = 0;
            double mp = 0;

            double statizm2 = 0;
            double mp2 = 0;
            double pSgl = 0;
            double dv1 = 0;
            double dv2 = 0;
            bool calcSecond = chkCalcSecond.IsChecked.Value;
            string block = txtBlock.Text;
            double RHO = RecordHour.calcRHO(RecordHour.DataSDay[block]);
            txtRHO.Text= String.Format("RHO={0:0.000}",  RHO); ;
            //bool calcSecond = false;
            int step = 0;
            //RecordHour.calcSTATIZM(RecordHour.DataSDay, ref statizm, ref mp, ref pSgl,ref step);


            RecordHour.calcSTATIZMFast(RecordHour.DataSDay[block], ref statizm, ref mp, ref pSgl,SettingsNPRCH.BlocksDict[block]);
            if (calcSecond)
                RecordHour.calcSTATIZMRegr(RecordHour.DataSDay[block], ref statizm2, ref mp2, ref pSgl, SettingsNPRCH.BlocksDict[block]);


            txtStatizm.Text = String.Format("S={0:0.00}, MP={1:0.000}, P={2:0.000}", statizm, mp, pSgl);
            if (calcSecond)
                txtStatizm2.Text = String.Format("S2={0:0.00}, MP2={1:0.000}", statizm2, mp2);
            else
                txtStatizm2.Text = "";


            chartStatizm.initControl();
            chartStatizm.init(true,"0.00",true);
            //prepareChartDouble(chartStatizm.chart);


            List<double> xx = new List<double>();
            List<double> xy = new List<double>();
            SortedList<double, double> list_Y = new SortedList<double, double>();
            SortedList<double, double> list_Y2 = new SortedList<double, double>();


            double minF = double.MaxValue;
            double maxF = double.MinValue;
            foreach (Record rec in RecordHour.DataSDay[block])
            {
                double d = rec.F_gc - 50;

                xx.Add(rec.F_gc - 50);
                 xy.Add( rec.P_fakt - rec.P_zvn - rec.P_plan);


                if (d < minF)
                    minF = d;
                if (d > maxF)
                    maxF = d;
            }

            double stepF =(maxF - minF)/1000;
            double f = minF;
            while (f < maxF)
            {
                double xi = f;
                double sgnX = Math.Sign(xi);
                double absX = Math.Abs(xi);
                double regr = 0;
                if (absX > (mp + pSgl))
                {
                    regr = -200/statizm * (xi - sgnX * mp);
                }
                else if (absX < (mp - pSgl))
                {
                    regr = 0;
                }
                else
                {
                    regr = -sgnX * 200/statizm / pSgl / 4 * Math.Pow(xi - sgnX * (mp - pSgl), 2);
                }
                list_Y.Add(f, regr);

                regr = 0;
                if (absX > mp2)
                {
                    double diffF = absX - mp2;
                    double d = sgnX * diffF;
                    regr = -200 / statizm2 * d ;
                }

                list_Y2.Add(f, regr);

                f += stepF;
            }


            /*
            chart.CurrentGraphPane.XAxis.Scale.Min = minF;
            chart.CurrentGraphPane.XAxis.Scale.Max = maxF;
            chart.CurrentGraphPane.XAxis.Scale.MinAuto = false;
            chart.CurrentGraphPane.XAxis.Scale.MaxAuto = false;*/

            chartStatizm.AddPointSerie("Calc", list_Y, System.Drawing.Color.Red, true, true);

            if (calcSecond)
                chartStatizm.AddPointSerie("Calc2", list_Y2, System.Drawing.Color.Pink, true, true);
            chartStatizm.AddMixPointSerie("Fakt",  xx,xy, System.Drawing.Color.LightBlue, false, true);
            





        }


    }
}
