using EDSProj;
using EDSProj.EDS;
using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для ReportWindow.xaml
	/// </summary>
	public partial class AVRCHMReportWindow : Window
	{
        public SortedList<string, EDSPointInfo> AllPoints;
        public AVRCHMReportWindow() {
			InitializeComponent();			

			grdStatus.DataContext = EDSClass.Single;
            clndDate.SelectedDate = DateTime.Now.Date.AddDays(-1);
		}

        public async Task<bool> init()
        {

            AllPoints = await EDSPointsClass.GetAllAnalogPoints();
            AVRCHMReport.init(AllPoints);
            return true;
        }



        private async void  btnCreate_Click(object sender, RoutedEventArgs e) {
			if (!EDSClass.Single.Ready) {
				MessageBox.Show("ЕДС сервер не готов");
				return;
			}

            DateTime dt = clndDate.SelectedDate.Value;
            AVRCHMReport report = new AVRCHMReport();
            DateTime ds = dt;
            DateTime de = dt.AddHours(5);
            if (de > DateTime.Now.AddMinutes(-10))
                de = DateTime.Now.AddMinutes(-10);
            bool ok=await report.ReadData(ds, de);
            if (ok)
            {
                report.ProcessData();
                grdEvents.ItemsSource = report.Events.Values.ToList();
                ReportResultWindow win = new ReportResultWindow();
                win.chart.init();
                win.chart.chart.GraphPane.XAxis.Scale.Format = "HH:mm:ss";

                win.chart.AddSerie("P план", report.getSerieData("PPlan",ds,de), System.Drawing.Color.Pink, true, false, 0, false);
                win.chart.AddSerie("P факт", report.getSerieData("PFakt", ds, de), System.Drawing.Color.Blue, true, false, 0, true);
                win.chart.AddSerie("P зад сум", report.getSerieData("PPlanFull", ds, de), System.Drawing.Color.Coral, true, false, 0, true);
                win.chart.AddSerie("P зад ГРАРМ", report.getSerieData("SumGroupZad", ds, de), System.Drawing.Color.Green, true, false, 0, false);
                win.chart.AddSerie("P нг", report.getSerieData("PMin", ds, de), System.Drawing.Color.Black, true, false, 0, true);
                win.chart.AddSerie("P вг", report.getSerieData("PMax", ds, de), System.Drawing.Color.Black, true, false, 0, true);
                win.chart.AddSerie("ГГ кол", report.getSerieData("GGCount", ds, de), System.Drawing.Color.LightBlue, true, false, 1, true);                
                win.chart.AddSerie("нарушение", report.getSerieData("ErrorLimits15", ds, de,false), System.Drawing.Color.Red, false, true, 0, true);
                win.chart.AddSerie("P звн", report.getSerieData("PZVN", ds, de), System.Drawing.Color.Orange, true, false, 2, false);
                win.chart.AddSerie("P перв", report.getSerieData("PPerv", ds, de), System.Drawing.Color.Purple, true, false, 2, false);


                win.Show();

            }


            EDSClass.Disconnect();
            EDSClass.Connect();
			
		

		}

		private void btnAbort_Click(object sender, RoutedEventArgs e) {
			EDSClass.Single.Abort();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e) {
            bool ok = await init();
            
        }

        private void grdEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                AVRCHMReportRecord ev = grdEvents.SelectedItem as AVRCHMReportRecord;
                MessageBox.Show(ev.HasError);
            }
            catch { }
        }
    }
}

