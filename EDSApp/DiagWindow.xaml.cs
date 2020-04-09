
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
using static EDSProj.Diagnostics.DiagNasos;
using System.Collections.ObjectModel;

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
        public String DateStart
        {
            get { return _DateStart.ToString("dd.MM.yyyy HH:mm"); }
            set
            {
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
        public String DateEnd
        {
            get { return _DateEnd.ToString("dd.MM.yyyy HH:mm"); }
            set
            {
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
        public ObservableCollection<DiagNasos> CurrentMNU;
        ReportResultWindow win;
        ReportResultWindow winRun;

        ReportResultWindow winRegul;
        ReportResultWindow winRunRegul;
        ReportResultWindow winMNU;
        public DiagWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            /*DateStart = DateTime.Now.Date.ToString();
            DateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour).ToString();*/
            DateStart = "23.03.2020 00:00:00";
            DateEnd = "24.03.2020 00:00:00";
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
            CurrentDiagOilRegul.V0_AGA = 1040;
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
            CurrentDiagOilRegul.V_TB = 2000;



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
                win.chart.CurrentFormatY = "0";
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
                winRun.chart.init(true, "0", true);
                winRun.chart.CurrentFormatY = "0";
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
            btnCreateRegul.IsEnabled = true;
            btnRecalcRegul.IsEnabled = false;

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

            try
            //if (winRegul!=null)
            {
                winRegul.Show();
                winRegul.chart.UpdateSerieData("V СБ", CurrentDiagOilRegul.serieV_SB);
                winRegul.chart.UpdateSerieData("V ОГА", CurrentDiagOilRegul.serieV_OGA);
                winRegul.chart.UpdateSerieData("V АГА", CurrentDiagOilRegul.serieV_AGA);
                winRegul.chart.UpdateSerieData("V РК", CurrentDiagOilRegul.serieV_RK);
                winRegul.chart.UpdateSerieData("V НА", CurrentDiagOilRegul.serieV_NA);
                winRegul.chart.UpdateSerieData("V ЛБ", CurrentDiagOilRegul.serieV_LB);


                winRegul.chart.UpdateSerieData("МНУ", CurrentDiagOilRegul.serieD_MNU);
                winRegul.chart.UpdateSerieData("ЛА", CurrentDiagOilRegul.serieD_LA);
                winRegul.chart.UpdateSerieData("T", CurrentDiagOilRegul.serieT_SB);


                winRegul.chart.UpdateSerieData("F", CurrentDiagOilRegul.serieF);
                winRegul.chart.UpdateSerieData("НА", CurrentDiagOilRegul.serieL_NA);
                winRegul.chart.UpdateSerieData("РК", CurrentDiagOilRegul.serieL_RK);
                winRegul.chart.UpdateSerieData("V", CurrentDiagOilRegul.serieV);
                winRegul.chart.updateSeries();

            }
            //else
            catch
            {
                winRegul = new ReportResultWindow();
                winRegul.chart.initControl();
                winRegul.chart.init(true, "dd.MM HH");
                winRegul.chart.AllYAxisIsVisible = true;
                winRegul.chart.CurrentFormatY = "0";
                winRegul.chart.setY2AxisCount(winRegul.chart.CurrentGraphPane, 2);
                winRegul.chart.AddSerie("V СБ", CurrentDiagOilRegul.serieV_SB, System.Drawing.Color.LightBlue, true, false, true, -1, true);
                winRegul.chart.AddSerie("V ОГА", CurrentDiagOilRegul.serieV_OGA, System.Drawing.Color.LightGreen, true, false, true, -1, true);
                winRegul.chart.AddSerie("V АГА", CurrentDiagOilRegul.serieV_AGA, System.Drawing.Color.LightCoral, true, false, true, 0, false);
                winRegul.chart.AddSerie("V РК", CurrentDiagOilRegul.serieV_RK, System.Drawing.Color.Pink, true, false, true, 0, true);
                winRegul.chart.AddSerie("V НА", CurrentDiagOilRegul.serieV_NA, System.Drawing.Color.Red, true, false, true, 0, true);
                winRegul.chart.AddSerie("V ЛБ", CurrentDiagOilRegul.serieV_LB, System.Drawing.Color.Orange, true, false, true, 1, false);
                //winRegul.chart.CurrentGraphPane.LineType = ZedGraph.LineType.Stack;

                winRegul.chart.init(true, "dd.MM HH");
                winRegul.chart.setY2AxisCount(winRegul.chart.CurrentGraphPane, 2);
                winRegul.chart.AddSerie("МНУ", CurrentDiagOilRegul.serieD_MNU, System.Drawing.Color.Orange, true, false, true, 0, true, -1, 5);
                winRegul.chart.AddSerie("ЛА", CurrentDiagOilRegul.serieD_LA, System.Drawing.Color.DeepPink, true, false, true, 1, true, -2, 4);
                winRegul.chart.AddSerie("T", CurrentDiagOilRegul.serieT_SB, System.Drawing.Color.LightGreen, true, false, true, -1, true);

                winRegul.chart.init(true, "dd.MM HH");
                winRegul.chart.setY2AxisCount(winRegul.chart.CurrentGraphPane, 2);
                winRegul.chart.AddSerie("F", CurrentDiagOilRegul.serieF, System.Drawing.Color.Red, true, false, true, 0, true, 0, 200);
                winRegul.chart.AddSerie("НА", CurrentDiagOilRegul.serieL_NA, System.Drawing.Color.Yellow, true, false, true, 1, true);
                winRegul.chart.AddSerie("РК", CurrentDiagOilRegul.serieL_RK, System.Drawing.Color.Gray, true, false, true, 1, true);
                winRegul.chart.AddSerie("V", CurrentDiagOilRegul.serieV, System.Drawing.Color.LightBlue, true, false, false, -1, true);

                winRegul.Title = caption;
                winRegul.Show();
            }

            //if (winRun!=null)
            try
            {
                winRunRegul.Show();
                winRunRegul.chart.UpdatePointSerieData("V", CurrentDiagOilRegul.serieVFull);
                winRunRegul.chart.UpdatePointSerieData("V run", CurrentDiagOilRegul.serieVRun);
                winRunRegul.chart.UpdatePointSerieData("V stop", CurrentDiagOilRegul.serieVStop);
                winRunRegul.chart.updateSeries();
            }
            //else
            catch
            {
                winRunRegul = new ReportResultWindow();
                winRunRegul.chart.initControl();
                winRunRegul.chart.init(true, "0.00", true);
                winRunRegul.chart.CurrentFormatY = "0";
                winRunRegul.chart.AddPointSerie("V", CurrentDiagOilRegul.serieVFull, System.Drawing.Color.Orange, true, false);
                winRunRegul.chart.AddPointSerie("V run", CurrentDiagOilRegul.serieVRun, System.Drawing.Color.LightGreen, true, false);
                winRunRegul.chart.AddPointSerie("V stop", CurrentDiagOilRegul.serieVStop, System.Drawing.Color.LightBlue, true, false);
                winRunRegul.Title = "объем на ГГ (убрать пустые поля)";
                winRunRegul.Show();
            }
            if (chbCreateReport.IsChecked.Value)
                CurrentDiagOilRegul.createText();


        }

        private async void MNUBUtttonCreate_Click(object sender, RoutedEventArgs e)
        {
            /* if (!EDSClass.Single.Ready)
             {
                 MessageBox.Show("ЕДС сервер не готов");
                 return;
             }
             EDSClass.Disconnect();
             EDSClass.Connect();*/

            DateTime dt = _DateStart;
            ObservableCollection<DiagNasos> CurrentMNU = new ObservableCollection<DiagNasos>();
            
            while (dt < _DateEnd)
            {

                DiagNasos diag = new DiagNasos(dt, dt.AddDays(1), txtGG.Text);
                bool ok = await diag.ReadData("MNU",3);
                CurrentMNU.Add(diag);
                dt = dt.AddDays(1);
            }
            MNUGrid.ItemsSource = CurrentMNU;


        }

        private async void MNUGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DiagNasos record = MNUGrid.SelectedItem as DiagNasos;
            winMNU = new ReportResultWindow();
            winMNU.chart.initControl();
            winMNU.chart.AllYAxisIsVisible = true;
            winMNU.chart.CurrentFormatY = "0";

            winMNU.chart.init(true, "dd.MM HH");
            winMNU.chart.setY2AxisCount(winMNU.chart.CurrentGraphPane, 3);
            winMNU.chart.AddSerie("RUN", record.GetSerieData(record.DateStart, record.DateEnd, record.GGRunData),
                System.Drawing.Color.Red, true, false, true, -1, true);
            winMNU.chart.AddSerie("Ust", record.GetSerieData(record.DateStart, record.DateEnd, record.GGUstData),
                System.Drawing.Color.LightBlue, true, false, true, -1, true);
            winMNU.chart.AddSerie("Одновр раб", record.GetSerieData(record.DateStart, record.DateEnd, record.OneTimeWorkInfo.Values.ToList()),
                System.Drawing.Color.Yellow, true, false, true, 0, true,0,5);
            /*bool ok = await record.GetSerieLEDSData();            
            winMNU.chart.AddSerie("L", record.dataLvl,   System.Drawing.Color.Orange, true, false, true, 1, true);
            winMNU.chart.AddSerie("P", record.dataP, System.Drawing.Color.Yellow, true, false, true, 2, true);*/

            


            winMNU.chart.init(true, "dd.MM HH");
            winMNU.chart.setY2AxisCount(winMNU.chart.CurrentGraphPane, 3);
            winMNU.chart.AddSerie("MNU A", record.GetSerieData(record.DateStart, record.DateEnd, record.NasosData[1]),
                System.Drawing.Color.Red, true, false, true, -1, true, 0, 3);
            winMNU.chart.AddSerie("MNU B", record.GetSerieData(record.DateStart, record.DateEnd, record.NasosData[2]),
                System.Drawing.Color.Green, true, false, true, 0, true, -1, 2);
            winMNU.chart.AddSerie("MNU C", record.GetSerieData(record.DateStart, record.DateEnd, record.NasosData[3]),
                System.Drawing.Color.Yellow, true, false, true, 1, true, -2, 1);




            winMNU.Show();
        }
    }
}
