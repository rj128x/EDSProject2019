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
	public partial class ReportWindow : Window
	{
		public ReportWindow() {
			InitializeComponent();			
			cmbPeriod.ItemsSource = EDSClass.ReportPeriods;

			clndFrom.SelectedDate = DateTime.Now.Date.AddDays(-1);
			clndTo.SelectedDate = DateTime.Now.Date;
			cmbPeriod.SelectedValue = EDSReportPeriod.hour;
			grdStatus.DataContext = EDSClass.Single;
		}




		private async void  btnCreate_Click(object sender, RoutedEventArgs e) {
			if (!EDSClass.Single.Ready) {
				MessageBox.Show("ЕДС сервер не готов");
				return;
			}
			if (!clndFrom.SelectedDate.HasValue) {
				MessageBox.Show("Выберите дату начала");
				return;
			}
			if (!clndFrom.SelectedDate.HasValue) {
				MessageBox.Show("Выберите дату конца");
				return;
			}
			/*if (cmbFunction.SelectedItem == null) {
				MessageBox.Show("Выберите функцию");
				return;
			}*/
			if (cmbPeriod.SelectedItem == null) {
				MessageBox.Show("Выберите период");
				return;
			}

			if (cntrlSelectPoints.SelectedPoints.Count() == 0) {
				MessageBox.Show("Выберите точки");
				return;
			}


			DateTime dtStart = clndFrom.SelectedDate.Value;
			DateTime dtEnd = clndTo.SelectedDate.Value;
			//EDSRdeportFunction func = (EDSReportFunction)cmbFunction.SelectedValue;

			EDSReportPeriod period = (EDSReportPeriod)cmbPeriod.SelectedValue;

			EDSReport report = new EDSReport(dtStart, dtEnd, period, chbMsk.IsChecked.Value);

			foreach (EDSReportRequestRecord rec in cntrlSelectPoints.SelectedPoints) {
				report.addRequestField(rec.Point, rec.Function);
			}

            //System.Windows.Application.Current.Dispatcher.Invoke( System.Windows.Threading.DispatcherPriority.Background, new System.Action(delegate { report.ReadData(); }));


            EDSClass.Disconnect();
            EDSClass.Connect();
			
			bool ready=await report.ReadData();

			String header = "";
			foreach (EDSReportRequestRecord rec in report.RequestData.Values) {
				header += String.Format("<th width='100'>{0}</th>", rec.Desc);
			}

			TextWriter tW = new StreamWriter("out.html");

			String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>точка</th>{0}</tr>", header);
			tW.WriteLine(txt);

			foreach (KeyValuePair<DateTime, Dictionary<string, double>> de in report.ResultData) {
				DateTime dt = de.Key;
				string ValuesStr = "";
				foreach (double val in de.Value.Values) {
					ValuesStr += String.Format("<td align='right'>{0:0.00}</td>", val);
				}
				tW.WriteLine(String.Format("<tr><th >{0}</th>{1}</tr>", dt.ToString("dd.MM.yyyy HH:mm:ss"), ValuesStr));				
			}			
			tW.WriteLine("</table></html>");
			tW.Close();

			Process.Start("out.html");


			ReportResultWindow win = new ReportResultWindow();
            win.chart.initControl();
			win.chart.init(true,"dd.MM HH:mm:ss");
			SortedList<DateTime, double> data = new SortedList<DateTime, double>();
			int index = -1;
			foreach (KeyValuePair<string, EDSReportRequestRecord> de in report.RequestData) {

				string id = de.Key;
				EDSReportRequestRecord request = de.Value;
				data.Clear();
				foreach (DateTime dt in report.ResultData.Keys) {
					data.Add(dt, report.ResultData[dt][id]);
				}
				win.chart.AddSerie(request.Desc, data, ChartZedSerie.NextColor(), true, true, false, index++);
			}
			win.Show();

		}

		private void btnAbort_Click(object sender, RoutedEventArgs e) {
			EDSClass.Single.Abort();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e) {
			bool ok=await cntrlSelectPoints.init();
		}
	}
}

