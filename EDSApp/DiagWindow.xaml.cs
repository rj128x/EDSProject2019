
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
using ZedGraph;
using EDSProj.Diagnostics;
using EDSProj;
using System.Globalization;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для DiagWindow.xaml
	/// </summary>
	public partial class DiagWindow : Window
	{
        public DiadOilClass CurrentDiag;
        ReportResultWindow win;
        ReportResultWindow winRun;
        public DiagWindow() {
			InitializeComponent();
			clndFrom.SelectedDate = DateTime.Now.Date.AddDays(-1);
			clndTo.SelectedDate = DateTime.Now.Date.AddDays(1);
            grdStatus.DataContext = EDSClass.Single;
            DiadOilClass.init();
            txtDatch.Text = 0.ToString();
            txtLVLcor.Text = 33.ToString();
            txtMinutes.Text = 20.ToString();
            txtAvgUrov.Text = 0.ToString();
            txtV0.Text = (1.4).ToString();
            txtV1mm.Text = (0.02).ToString();
            txtTbaz.Text = (20).ToString();
            txtTkoef.Text = (0.0007).ToString();
        }

		private async void btnCreate_Click(object sender, RoutedEventArgs e) {
            EDSClass.Disconnect();
            EDSClass.Connect();
            string GG = txtGG.Text;
            CurrentDiag = new DiadOilClass();
            CurrentDiag.DateStart = clndFrom.SelectedDate.Value;
            CurrentDiag.DateEnd = clndTo.SelectedDate.Value;

            
            bool ok=await CurrentDiag.ReadData(GG);
            //win = new ReportResultWindow();
            reCalcGP();
            btnCreate.IsEnabled = false;
            btnRecalc.IsEnabled = true;
        }

        private void reCalcGP()
        {
            
            int datch = Int32.Parse(txtDatch.Text);
            double lvlCor = Int32.Parse(txtLVLcor.Text);
            int mins = Int32.Parse(txtMinutes.Text);
            int lmins = Int32.Parse(txtAvgUrov.Text);
            double V0 = Double.Parse(txtV0.Text);
            double V1mm = Double.Parse(txtV1mm.Text);
            double Tkoef = Double.Parse(txtTkoef.Text);
            int Tbaz = Int32.Parse(txtTbaz.Text);

            string caption = String.Format("GG{0}, Датчик {1}, V0={2} V1mm={3} Tkoef={4} Tbaz={5}", txtGG.Text, datch == 0 ? "Avg" : datch.ToString(), V0, V1mm, Tkoef, Tbaz);
            CurrentDiag.recalcData(V0,V1mm,Tkoef, lvlCor, mins,Tbaz,datch,lmins);

            try
            {
                win.Show();
            }
            catch
            {
                win = new ReportResultWindow();
            }

            try
            {
                winRun.Show();
            }
            catch
            {
                winRun = new ReportResultWindow();
            }

            win.chart.initControl();
            win.chart.init(true, "dd.MM HH");
            win.chart.AllYAxisIsVisible = true;


            win.chart.AddSerie("L1", CurrentDiag.serieLvl1, System.Drawing.Color.LightGreen, true, false, true, -1);
            win.chart.AddSerie("L2", CurrentDiag.serieLvl2, System.Drawing.Color.LightSeaGreen, true, false, true, -1);
            win.chart.AddSerie("T up", CurrentDiag.serieTUP, System.Drawing.Color.LightBlue, true, false, true, 0);
            win.chart.AddSerie("T dn", CurrentDiag.serieTDn, System.Drawing.Color.LightBlue, true, false, true, 0);
            win.chart.AddSerie("F", CurrentDiag.serieF, System.Drawing.Color.Red, true, false, true, 1,true,0,200);
            


            win.chart.init(true, "dd.MM HH");
            win.chart.AddSerie("T avg", CurrentDiag.serieTAvg, System.Drawing.Color.LightBlue, true, false, true, 0);
            win.chart.AddSerie("V расч", CurrentDiag.serieV, System.Drawing.Color.Orange, true, false, true, 1);
            win.chart.AddSerie("V ovation", CurrentDiag.serieVOv, System.Drawing.Color.OrangeRed, true, false, true, 1);
            win.chart.AddSerie("L коррекция", CurrentDiag.serieLvlCor, System.Drawing.Color.Green, true, false, true, -1);
            
            win.Title = caption;
            win.Show();

            winRun.chart.initControl();
            winRun.chart.init(true, "0.00",true);
            winRun.chart.AddPointSerie("V", CurrentDiag.serieVFull, System.Drawing.Color.Orange, true, false);
            winRun.chart.AddPointSerie("V run", CurrentDiag.serieVRun, System.Drawing.Color.Green, true, false);
            winRun.chart.AddPointSerie("V stop" , CurrentDiag.serieVStop, System.Drawing.Color.Blue, true, false);
            winRun.Title = "объем на ГГ (убрать пустые поля)";
            winRun.Show();
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
