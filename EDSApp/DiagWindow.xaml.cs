
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

        public DiadOilGPClass CurrentDiagGP;
        public DiadOilPPClass CurrentDiagPP;
        public DiadOilRegulClass CurrentDiagOilRegul;
        ReportResultWindow win;
        ReportResultWindow winRun;

        ReportResultWindow winRegul;
        ReportResultWindow winRunRegul;
        public DiagWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            DateStart = DateTime.Now.Date.ToString();
            DateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour).ToString();
            grdStatus.DataContext = EDSClass.Single;
            DiadOilClass.init();

            CurrentDiagGP = new DiadOilGPClass();
            brdGPSettings.DataContext = CurrentDiagGP;
            CurrentDiagGP.datch = 0;
            CurrentDiagGP.LDiff = 33;
            CurrentDiagGP.fMinutes = 20;
            CurrentDiagGP.lMinutes = 0;
            CurrentDiagGP.V0 = (1.4);
            CurrentDiagGP.V1mm = (0.02);
            CurrentDiagGP.Tbaz = (20);
            CurrentDiagGP.Tkoef = (0.0007);

            CurrentDiagPP = new DiadOilPPClass();
            brdPPSettings.DataContext = CurrentDiagPP;
            CurrentDiagPP.datch = 0;
            CurrentDiagPP.LDiff = 17.2;
            CurrentDiagPP.fMinutes = 20;
            CurrentDiagPP.lMinutes = 0;
            CurrentDiagPP.V0 = 16;
            CurrentDiagPP.V1mm = 0.1;
            CurrentDiagPP.Tbaz = 20;
            CurrentDiagPP.Tkoef = 0.0007;

            DiadOilRegulClass.init();
            CurrentDiagOilRegul = new DiadOilRegulClass();
            brdRegulSettings.DataContext = CurrentDiagOilRegul;
            CurrentDiagOilRegul.V0_OGA = 7000;
            CurrentDiagOilRegul.V1mm_OGA = 3.1415;
            CurrentDiagOilRegul.V0_AGA = 1600;
            CurrentDiagOilRegul.V1mm_AGA = 0.8655;
            CurrentDiagOilRegul.V0_SB = 8800;
            CurrentDiagOilRegul.V1mm_SB = 11.61512;
            CurrentDiagOilRegul.V0_LB = 0;
            CurrentDiagOilRegul.V1mm_LB = 0.18;

            CurrentDiagOilRegul.V0_Opn_NA = 588.08;
            CurrentDiagOilRegul.V0_Cls_NA = 508.258;
            CurrentDiagOilRegul.HodNA = 1297;


            CurrentDiagOilRegul.V0_Opn_RK = 1067.07;
            CurrentDiagOilRegul.V0_Cls_RK = 970.96;
            CurrentDiagOilRegul.HodRK = 333;

            CurrentDiagOilRegul.Tbaz = 20;
            CurrentDiagOilRegul.TMZ = 22;
            CurrentDiagOilRegul.Tkoef = 0.0007;
            CurrentDiagOilRegul.fMinutes = 10;



        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!EDSClass.Single.Ready)
            {
                MessageBox.Show("ЕДС сервер не готов");
                return;
            }
            EDSClass.Disconnect();
            EDSClass.Connect();
            CurrentDiagGP.GG = txtGG.Text;


            CurrentDiagGP.DateStart = _DateStart;



            CurrentDiagGP.DateEnd = _DateEnd;


            bool ok = await CurrentDiagGP.ReadData(CurrentDiagGP.GG);
            //win = new ReportResultWindow();
            reCalcGP();
            btnCreate.IsEnabled = false;
            btnRecalc.IsEnabled = true;
        }

        public void reCreateCharts(string caption, DiadOilClass diag)
        {
            try
            //if (win!=null)
            {
                win.Show();
                win.Title = caption;
                win.chart.UpdateSerieData("L1", diag.serieLvl1);
                win.chart.UpdateSerieData("L2", diag.serieLvl2);
                win.chart.UpdateSerieData("T up", diag.serieTUP);
                win.chart.UpdateSerieData("T dn", diag.serieTDn);
                win.chart.UpdateSerieData("F", diag.serieF);


                win.chart.UpdateSerieData("T avg", diag.serieTAvg);
                win.chart.UpdateSerieData("V расч", diag.serieV);
                //win.chart.UpdateSerieData("V ovation", diag.serieVOv);
                win.chart.UpdateSerieData("L коррекция", diag.serieLvlCor);
                win.chart.updateSeries();

            }
            //else
            catch
            {
                win = new ReportResultWindow();
                win.chart.initControl();
                win.chart.init(true, "dd.MM HH");
                win.chart.AllYAxisIsVisible = true;


                win.chart.AddSerie("L1", diag.serieLvl1, System.Drawing.Color.LightGreen, true, false, true, -1);
                win.chart.AddSerie("L2", diag.serieLvl2, System.Drawing.Color.LightSeaGreen, true, false, true, -1);
                win.chart.AddSerie("T up", diag.serieTUP, System.Drawing.Color.LightBlue, true, false, true, 0);
                win.chart.AddSerie("T dn", diag.serieTDn, System.Drawing.Color.LightBlue, true, false, true, 0);
                win.chart.AddSerie("F", diag.serieF, System.Drawing.Color.Red, true, false, true, 1, true, 0, 200);



                win.chart.init(true, "dd.MM HH");
                win.chart.AddSerie("T avg", diag.serieTAvg, System.Drawing.Color.LightBlue, true, false, true, 0);
                win.chart.AddSerie("V расч", diag.serieV, System.Drawing.Color.Orange, true, false, true, 1);
                //win.chart.AddSerie("V ovation", diag.serieVOv, System.Drawing.Color.OrangeRed, true, false, true, 1);
                win.chart.AddSerie("L коррекция", diag.serieLvlCor, System.Drawing.Color.Green, true, false, true, -1);
                win.Title = caption;
                win.Show();
            }

            //if (winRun!=null)
            try
            {
                winRun.Show();
                winRun.chart.UpdatePointSerieData("V", diag.serieVFull);
                winRun.chart.UpdatePointSerieData("V run", diag.serieVRun);
                winRun.chart.UpdatePointSerieData("V stop", diag.serieVStop);
                winRun.chart.updateSeries();
            }
            //else
            catch
            {
                winRun = new ReportResultWindow();
                winRun.chart.initControl();
                winRun.chart.init(true, "0.00", true);
                winRun.chart.AddPointSerie("V", diag.serieVFull, System.Drawing.Color.Orange, true, false);
                winRun.chart.AddPointSerie("V run", diag.serieVRun, System.Drawing.Color.LightGreen, true, false);
                winRun.chart.AddPointSerie("V stop", diag.serieVStop, System.Drawing.Color.LightBlue, true, false);
                winRun.Title = "объем на ГГ (убрать пустые поля)";
                winRun.Show();
            }

            if (chbCreateReport.IsChecked.Value)
                diag.createText();
        }

        private void reCalcGP()
        {

            string caption = String.Format("ГП GG{0}, Датчик {1}, V0={2} V1mm={3} Tkoef={4} Tbaz={5}",
                txtGG.Text, CurrentDiagGP.datch == 0 ? "Avg" : CurrentDiagGP.datch.ToString(), CurrentDiagGP.V0, CurrentDiagGP.V1mm, CurrentDiagGP.Tkoef, CurrentDiagGP.Tbaz);
            CurrentDiagGP.recalcData();

            reCreateCharts(caption, CurrentDiagGP);

        }


        private void reCalcPP()
        {

            string caption = String.Format("ПП: GG{0}, Датчик {1}, V0={2} V1mm={3} Tkoef={4} Tbaz={5}",
                txtGG.Text, CurrentDiagPP.datch == 0 ? "Avg" : CurrentDiagPP.datch.ToString(), CurrentDiagPP.V0, CurrentDiagPP.V1mm, CurrentDiagPP.Tkoef, CurrentDiagPP.Tbaz);
            CurrentDiagPP.recalcData();
            reCreateCharts(caption, CurrentDiagPP);
        }





        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            reCalcGP();
        }

        private void changeInitData()
        {
            btnCreate.IsEnabled = true;
            btnRecalc.IsEnabled = false;
            btnCreatePP.IsEnabled = true;
            btnRecalcPP.IsEnabled = false;

        }
        private void clndFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            changeInitData();
        }

        private void txtGG_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeInitData();
        }

        private async void btnCreatePP_Click(object sender, RoutedEventArgs e)
        {
            if (!EDSClass.Single.Ready)
            {
                MessageBox.Show("ЕДС сервер не готов");
                return;
            }
            EDSClass.Disconnect();
            EDSClass.Connect();
            CurrentDiagPP.GG = txtGG.Text;


            CurrentDiagPP.DateStart = _DateStart;



            CurrentDiagPP.DateEnd = _DateEnd;


            bool ok = await CurrentDiagPP.ReadData(CurrentDiagPP.GG);
            //win = new ReportResultWindow();
            reCalcPP();
            btnCreatePP.IsEnabled = false;
            btnRecalcPP.IsEnabled = true;
        }

        private void btnRecalcPP_Click(object sender, RoutedEventArgs e)
        {
            reCalcPP();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                win.Close();
            }
            catch { }
            try
            {
                winRun.Close();

            }
            catch { }

        }

        private void btnRecalcRegul_Click(object sender, RoutedEventArgs e)
        {
            reCalcRegul();
        }

        private async void btnCreateRegul_Click(object sender, RoutedEventArgs e)
        {
            if (!EDSClass.Single.Ready)
            {
                MessageBox.Show("ЕДС сервер не готов");
                return;
            }
            EDSClass.Disconnect();
            EDSClass.Connect();
            CurrentDiagOilRegul.GG = txtGG.Text;

            CurrentDiagOilRegul.DateStart = _DateStart;


            CurrentDiagOilRegul.DateEnd = _DateEnd;


            bool ok = await CurrentDiagOilRegul.ReadData(CurrentDiagOilRegul.GG);
            //win = new ReportResultWindow();
            reCalcRegul();
            btnCreateRegul.IsEnabled = false;
            btnRecalcRegul.IsEnabled = true;
        }


        private void reCalcRegul()
        {

            string caption = String.Format("Объем масла");
            CurrentDiagOilRegul.recalcData();



            winRegul = new ReportResultWindow();
            winRegul.chart.initControl();
            winRegul.chart.init(true, "dd.MM HH");
            winRegul.chart.AllYAxisIsVisible = true;
            winRegul.chart.AddSerie("V SB", CurrentDiagOilRegul.serieV_SB, System.Drawing.Color.Lime, true, false, true, -1, true);
            winRegul.chart.AddSerie("V OGA", CurrentDiagOilRegul.serieV_OGA, System.Drawing.Color.LightGreen, true, false, true, -1, true);
            winRegul.chart.AddSerie("V AGA", CurrentDiagOilRegul.serieV_AGA, System.Drawing.Color.LightBlue, true, false, true, -1, true);
            winRegul.chart.AddSerie("V NA", CurrentDiagOilRegul.serieV_NA, System.Drawing.Color.GreenYellow, true, false, true, -1, true);
            winRegul.chart.AddSerie("V RK", CurrentDiagOilRegul.serieV_RK, System.Drawing.Color.Pink, true, false, true, -1, true);
            winRegul.chart.AddSerie("V LB", CurrentDiagOilRegul.serieV_LB, System.Drawing.Color.Orange, true, false, true, -1, true);
            

            /*winRegul.chart.AddSerie("МНУ", CurrentDiagOilRegul.serieD_MNU, System.Drawing.Color.Orange, true, false, true, 0, false);
            winRegul.chart.AddSerie("ЛА", CurrentDiagOilRegul.serieD_LA, System.Drawing.Color.DeepPink, true, false, true, 0, false);

            winRegul.chart.AddSerie("L AGA", CurrentDiagOilRegul.serieL_AGA, System.Drawing.Color.LightBlue, true, false, true, 1, true);
            winRegul.chart.AddSerie("L OGA", CurrentDiagOilRegul.serieL_OGA, System.Drawing.Color.LightGreen, true, false, true, 1, true);
            winRegul.chart.AddSerie("L NA", CurrentDiagOilRegul.serieL_NA, System.Drawing.Color.GreenYellow, true, false, true, 2, true);
            winRegul.chart.AddSerie("L RK", CurrentDiagOilRegul.serieL_RK, System.Drawing.Color.Indigo, true, false, true, 2, true);

            winRegul.chart.init(true, "dd.MM HH");
            winRegul.chart.AddSerie("L SB", CurrentDiagOilRegul.serieL_SB, System.Drawing.Color.LightPink, true, false, true, 2, true);
            winRegul.chart.AddSerie("L LB", CurrentDiagOilRegul.serieL_LB, System.Drawing.Color.LightGray, true, false, true, 2, true);*/



            winRegul.chart.init(true, "dd.MM HH");
            winRegul.chart.AddSerie("V", CurrentDiagOilRegul.serieV, System.Drawing.Color.LightBlue, true, false, true, -1, true);
            winRegul.chart.AddSerie("F", CurrentDiagOilRegul.serieF, System.Drawing.Color.Red, true, false, true, 0, true, 0, 200);
            winRegul.chart.AddSerie("T", CurrentDiagOilRegul.serieT_SB, System.Drawing.Color.LightGreen, true, false, true, 1, true);

            winRegul.Title = caption;
            winRegul.Show();




        }
    }
}
