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
using ZedGraph;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для ReportWindow.xaml
	/// </summary>
	public partial class AVRCHMReportWindow : Window
	{
        public SortedList<string, EDSPointInfo> AllPoints;
        public AVRCHMReport report;
        public DateTime currentDate;
        public AVRCHMReportWindow() {
			InitializeComponent();			

			grdStatus.DataContext = EDSClass.Single;
            clndDate.SelectedDate = DateTime.Now.Date.AddDays(-1);
		}

        public async Task<bool> init()
        {
            AllPoints = new SortedList<string, EDSPointInfo>();
            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr("11VT.*", AllPoints);
            ok &= await EDSPointsClass.getPointsArr("PBR.*", AllPoints);
            
            AVRCHMReport.init(AllPoints);
            return true;
        }



        private async void  btnCreate_Click(object sender, RoutedEventArgs e) {
			if (!EDSClass.Single.Ready) {
				MessageBox.Show("ЕДС сервер не готов");
				return;
			}

            EDSClass.Disconnect();
            EDSClass.Connect();

            DateTime dt = clndDate.SelectedDate.Value;
            report = new AVRCHMReport();
            DateTime ds = dt.AddHours(0);
            DateTime de = dt.AddHours(24);
            if (de > DateTime.Now.AddMinutes(-5).AddHours(-2))
                de = DateTime.Now.AddMinutes(-5).AddHours(-2);
            int winSize = Int32.Parse(txtTWin.Text);
            int tPlanMax = Int32.Parse(txtTPlan.Text);
            int zvn = Int32.Parse(txtZVN.Text);
            bool ok=await report.ReadData(ds, de,zvn,tPlanMax,winSize);
            if (ok)
            {
                grdEvents.ItemsSource = report.Events.ToList();
                currentDate = dt;
            }                    
		

		}

        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            int winSize = Int32.Parse(txtTWin.Text);
            int tPlanMax = Int32.Parse(txtTPlan.Text);
            int zvn = Int32.Parse(txtZVN.Text);
            report.checkErrors(zvn, tPlanMax, winSize);
            grdEvents.ItemsSource = report.Events.ToList();
                        
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e) {
			EDSClass.Single.Abort();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e) {
            bool ok = await init();
            
        }

        private async void grdEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!EDSClass.Single.Ready)
            {
                MessageBox.Show("ЕДС сервер не готов");
                return;
            }
            try
            {
                EDSClass.Disconnect();
                EDSClass.Connect();

                AVRCHMReportRecord ev = grdEvents.SelectedItem as AVRCHMReportRecord;

                ReportResultWindow win = new ReportResultWindow();
                win.Title = ev.Date;
                win.chart.initControl();
                win.chart.init(true, "HH:mm:ss");
                win.chart.AllYAxisIsVisible = true;
                DateTime ds = ev.DateStart;
                DateTime de = ev.DateEnd;

                SortedList<String, SortedList<DateTime, double>> errors = report.getSeriesErrorsData("ErrorLimits15", ds, de);
                foreach (KeyValuePair<String, SortedList<DateTime, double>> rec in errors)
                {
                    win.chart.AddSerie("План "+rec.Key, rec.Value, System.Drawing.Color.Red, true, false, false, 0, true);
                }

                bool HasErrorReserv = false;
                errors = report.getSeriesErrorsData("ErrorRezervGraph", ds, de);
                foreach (KeyValuePair<String, SortedList<DateTime, double>> rec in errors)
                {
                    HasErrorReserv = true;
                    win.chart.AddSerie("Резерв "+rec.Key, rec.Value, System.Drawing.Color.OrangeRed, true, false, false, 3, true);
                }


                win.chart.AddSerie("P план", report.getSerieData("PPlan", ds, de), System.Drawing.Color.Pink, true, false, true, 0, false);
                win.chart.AddSerie("P факт", report.getSerieData("PFakt", ds, de), System.Drawing.Color.Blue, true, false, false, 0, true);
                win.chart.AddSerie("P зад сум", report.getSerieData("PPlanFull", ds, de), System.Drawing.Color.Coral, true, false, false, 0, true);
                win.chart.AddSerie("P зад ГРАРМ", report.getSerieData("SumGroupZad", ds, de), System.Drawing.Color.Green, true, false, true, 0, false);
                win.chart.AddSerie("P нг", report.getSerieData("PMin", ds, de), System.Drawing.Color.Gray, true, false, false, 0, true);
                win.chart.AddSerie("P вг", report.getSerieData("PMax", ds, de), System.Drawing.Color.Gray, true, false, false, 0, true);
                win.chart.AddSerie("ГГ кол", report.getSerieData("GGCount", ds, de), System.Drawing.Color.LightBlue, true, false, false, 1, true, 0, 20);
                //win.chart.AddSerie("нарушение", report.getSerieData("ErrorLimits15", ds, de, false), System.Drawing.Color.Red, false, true, false, 0, true);

                win.chart.AddSerie("P перв", report.getSerieData("PPerv", ds, de), System.Drawing.Color.Purple, true, false, false, 2, false);
                win.chart.AddSerie("P звн", report.getSerieData("PZVN", ds, de), System.Drawing.Color.Orange, true, false, false, 2, false);
                win.chart.AddSerie("ресурс+", report.getSerieData("ResursZagr", ds, de), System.Drawing.Color.GreenYellow, true, false, false, 3, HasErrorReserv);
                win.chart.AddSerie("ресурс-", report.getSerieData("ResursRazgr", ds, de), System.Drawing.Color.YellowGreen, true, false, false, 3, HasErrorReserv);
                //win.chart.AddSerie("Нарушение рез", report.getSerieData("ErrorRezervGraph", ds, de, false), System.Drawing.Color.Red, false, true, false, 3, false);


                

                if (!ev.Date.ToLower().Contains("сутки"))
                {
                    /*win.chart2.Visibility = Visibility.Visible;
                    win.mainGrid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);*/
                    win.chart.init(true, "HH:mm:ss");

                    Dictionary<string, SortedList<DateTime, double>> series = await report.getGaData(ds, de);
                    System.Drawing.Color color = ChartZedSerie.NextColor();
                    foreach (string key in series.Keys)
                    {
                        bool isVG = key.Contains("ВГ");
                        SortedList<DateTime, double> data = series[key];
                        if (!isVG && data.Values.Max() > 10 || isVG && data.Values.Min() != data.Values.Max())
                            win.chart.AddSerie(key, data, color, true, false, key.Contains("Задание"), isVG ? 0 : 3, !isVG, isVG ? 0 : double.MinValue, isVG ? 20 : double.MaxValue);

                        color = isVG ? ChartZedSerie.NextColor() : color;
                    }

                }


                win.Show();
            }
            catch {
                Logger.Info(e.ToString());
            }
        }

       
    }
}

