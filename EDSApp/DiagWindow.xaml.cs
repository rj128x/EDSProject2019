
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using EDSProj.Diagnostics;
using EDSProj;
using System.Globalization;
using System.ComponentModel;

namespace EDSApp
{
    /// <summary>
    /// Логика взаимодействия для DiagWindow.xaml
    /// </summary>


    public partial class DiagWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private DateTime _DateStart;
        private DateTime _DateEnd;
        public String DateStart {
            get { return _DateStart.ToString("dd.MM.yyyy HH:mm"); }
            set {
                try
                {
                    _DateStart = DateTime.Parse(value);
                }
                catch
                {
                }
                changeInitData();
                NotifyChanged("DateStart");
            }
        }
        public String DateEnd {
            get { return _DateEnd.ToString("dd.MM.yyyy HH:mm"); }
            set {
                try
                {
                    _DateEnd = DateTime.Parse(value);
                }
                catch
                {
                }
                changeInitData();
                NotifyChanged("DateEnd");
            }
        }

        public DiadOilClass CurrentDiag;
        ReportResultWindow win;
        ReportResultWindow winRun;
        public DiagWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            DateStart = DateTime.Now.Date.ToString();
            DateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour).ToString();
            grdStatus.DataContext = EDSClass.Single;
            DiadOilClass.init();
            CurrentDiag = new DiadOilClass();
            brdSettings.DataContext = CurrentDiag;
            CurrentDiag.datch = 0;
            CurrentDiag.LDiff = 33;
            CurrentDiag.fMinutes = 20;
            CurrentDiag.lMinutes = 0;
            CurrentDiag.V0 = (1.4);
            CurrentDiag.V1mm = (0.02);
            CurrentDiag.Tbaz = (20);
            CurrentDiag.Tkoef = (0.0007);
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            EDSClass.Disconnect();
            EDSClass.Connect();
            CurrentDiag.GG = txtGG.Text;


            CurrentDiag.DateStart = _DateStart;



            CurrentDiag.DateEnd = _DateEnd;


            bool ok = await CurrentDiag.ReadData(CurrentDiag.GG);
            //win = new ReportResultWindow();
            reCalcGP();
            btnCreate.IsEnabled = false;
            btnRecalc.IsEnabled = true;
        }

        private void reCalcGP()
        {



            string caption = String.Format("GG{0}, Датчик {1}, V0={2} V1mm={3} Tkoef={4} Tbaz={5}",
                txtGG.Text, CurrentDiag.datch == 0 ? "Avg" : CurrentDiag.datch.ToString(), CurrentDiag.V0, CurrentDiag.V1mm, CurrentDiag.Tkoef, CurrentDiag.Tbaz);
            CurrentDiag.recalcData();

            try
            {
                win.Show();
                win.Title = caption;
                win.chart.UpdateSerieData("L1", CurrentDiag.serieLvl1);
                win.chart.UpdateSerieData("L2", CurrentDiag.serieLvl2);
                win.chart.UpdateSerieData("T up", CurrentDiag.serieTUP);
                win.chart.UpdateSerieData("T dn", CurrentDiag.serieTDn);
                win.chart.UpdateSerieData("F", CurrentDiag.serieF);


                win.chart.UpdateSerieData("T avg", CurrentDiag.serieTAvg);
                win.chart.UpdateSerieData("V расч", CurrentDiag.serieV);
                win.chart.UpdateSerieData("V ovation", CurrentDiag.serieVOv);
                win.chart.UpdateSerieData("L коррекция", CurrentDiag.serieLvlCor);
                win.chart.updateSeries();

            }
            catch { 
                win = new ReportResultWindow();
                win.chart.initControl();
                win.chart.init(true, "dd.MM HH");
                win.chart.AllYAxisIsVisible = true;


                win.chart.AddSerie("L1", CurrentDiag.serieLvl1, System.Drawing.Color.LightGreen, true, false, true, -1);
                win.chart.AddSerie("L2", CurrentDiag.serieLvl2, System.Drawing.Color.LightSeaGreen, true, false, true, -1);
                win.chart.AddSerie("T up", CurrentDiag.serieTUP, System.Drawing.Color.LightBlue, true, false, true, 0);
                win.chart.AddSerie("T dn", CurrentDiag.serieTDn, System.Drawing.Color.LightBlue, true, false, true, 0);
                win.chart.AddSerie("F", CurrentDiag.serieF, System.Drawing.Color.Red, true, false, true, 1, true, 0, 200);



                win.chart.init(true, "dd.MM HH");
                win.chart.AddSerie("T avg", CurrentDiag.serieTAvg, System.Drawing.Color.LightBlue, true, false, true, 0);
                win.chart.AddSerie("V расч", CurrentDiag.serieV, System.Drawing.Color.Orange, true, false, true, 1);
                win.chart.AddSerie("V ovation", CurrentDiag.serieVOv, System.Drawing.Color.OrangeRed, true, false, true, 1);
                win.chart.AddSerie("L коррекция", CurrentDiag.serieLvlCor, System.Drawing.Color.Green, true, false, true, -1);
                win.Title = caption;
                win.Show();
            }

            try
            {
                winRun.Show();
                winRun.chart.UpdatePointSerieData("V", CurrentDiag.serieVFull);
                winRun.chart.UpdatePointSerieData("V run", CurrentDiag.serieVRun);
                winRun.chart.UpdatePointSerieData("V stop", CurrentDiag.serieVStop);
                winRun.chart.updateSeries();
            }
            catch
            {
                winRun = new ReportResultWindow();
                winRun.chart.initControl();
                winRun.chart.init(true, "0.00", true);
                winRun.chart.AddPointSerie("V", CurrentDiag.serieVFull, System.Drawing.Color.Orange, true, false);
                winRun.chart.AddPointSerie("V run", CurrentDiag.serieVRun, System.Drawing.Color.Green, true, false);
                winRun.chart.AddPointSerie("V stop", CurrentDiag.serieVStop, System.Drawing.Color.Blue, true, false);
                winRun.Title = "объем на ГГ (убрать пустые поля)";
                winRun.Show();
            }




            
            

            

        }





        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            reCalcGP();
        }

        private void changeInitData()
        {
            btnCreate.IsEnabled = true;
            btnRecalc.IsEnabled = false;

        }
        private void clndFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            changeInitData();
        }

        private void txtGG_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeInitData();
        }
    }
}
