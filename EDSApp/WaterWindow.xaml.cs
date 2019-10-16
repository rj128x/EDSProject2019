using EDSProj;
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

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для WaterWin.xaml
	/// </summary>
	public partial class WaterWindow : Window
	{
		public WaterWindow() {
			InitializeComponent();
		}

		protected static double getDouble(string str) {
			try {
				return double.Parse(str.Replace(".", ","));
			} catch {
				try {
					return double.Parse(str.Replace(",", "."));
				} catch {
					return 0;
				}
			}
		}

		private void btnCalcGGP_Click(object sender, RoutedEventArgs e) {
			double q = getDouble(txtQGG.Text);
			double h = getDouble(txtHGG.Text);
			int gg = int.Parse(txtGG.Text);
			double p = InerpolationRashod.getPower(gg, q, h);
			txtPGG.Text = p.ToString("0.00");

		}

		private void btnCalcGGQ_Click(object sender, RoutedEventArgs e) {
			double p = getDouble(txtPGG.Text);
			double h = getDouble(txtHGG.Text);
			int gg = int.Parse(txtGG.Text);
			double q = InerpolationRashod.getRashodGA(gg, p, h);
			txtQGG.Text = q.ToString("0.00");
			
		}

		private void btnCalcGESP_Click(object sender, RoutedEventArgs e) {
			double q = getDouble(txtQGES.Text);
			double h = getDouble(txtHGES.Text);			
			double p = InerpolationRashod.getPowerGES(q, h);
			txtPGES.Text = p.ToString("0.00");
		}

		private void btnCalcGESQ_Click(object sender, RoutedEventArgs e) {
			double p = getDouble(txtPGES.Text);
			double h = getDouble(txtHGES.Text);
			double q = OptimRashod.getOptimRashod(p,h);
			txtQGES.Text = q.ToString("0.00");
		}

		private void btnRUSA_Click(object sender, RoutedEventArgs e) {
			double p = getDouble(txtPGES_RUSA.Text);
			double h = getDouble(txtHGES_RUSA.Text);
			String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>Кол</th><th>Расход</th><th>КПД</th><th>Мощность</th><th>Состав</th></tr>");

			SortedList<double,List<int>> result= OptimRashod.getOptimRashodFull(p, h);
			foreach (KeyValuePair<double,List<int>> de in result) {
				double kpd = InerpolationRashod.KPD(p, h, de.Key)*100;
				double divP = p / de.Value.Count;				
				string tr = String.Format("<tr><td>{0}</td><td>{1:0.00}</td><td>{2:0.00}</td><td>{3:0.00}</td><td>{4}</td></tr>", de.Value.Count, de.Key, kpd, divP, string.Join(" ", de.Value));
				txt += tr;
			}
			txt += "</table>";
			wbResult.NavigateToString(txt);
		}

		private void btn8H_Click(object sender, RoutedEventArgs e) {
			double napor = getDouble(txtHGES_8H.Text);
			double pRaspGTP1 = getDouble(txtPGTP1_8H.Text);
			double pRaspGTP2 = getDouble(txtPGTP2_8H.Text);
			double qFAVR = getDouble(txtQ_FAVR_8H.Text);
			double needTime = getDouble(txt_TIME_8H.Text);
			double needQ0 = getDouble(txtQ0_8H.Text);

			double rashod8 = (qFAVR * 24 - needQ0 * (24 - needTime)) / needTime;
			double p8h_GES = InerpolationRashod.getPowerGES(rashod8, napor);
			double p8h_GTP1 = p8h_GES / (pRaspGTP1+pRaspGTP2) * pRaspGTP1;
			if (p8h_GES - p8h_GTP1 > pRaspGTP2)
				p8h_GTP1 = pRaspGTP1;
			double p8h_GTP2 = p8h_GES - p8h_GTP1;

			double pikGTP1 = pRaspGTP1 - p8h_GTP1;
			double pikGTP2 = pRaspGTP2 - p8h_GTP2;
			double pikGES = pRaspGTP1 + pRaspGTP2 - p8h_GES;

			double q_gtp1 = OptimRashod.getOptimRashod(p8h_GTP1, napor);
			double q_gtp2 = OptimRashod.getOptimRashod(p8h_GTP2, napor);
			double q_ges = q_gtp1+q_gtp2;

			txt_pRasp_GTP1.Text = pRaspGTP1.ToString("0.00");
			txt_pRasp_GTP2.Text = pRaspGTP2.ToString("0.00");
			txt_pRasp_GES.Text = (pRaspGTP1+pRaspGTP2).ToString("0.00");

			txt_p8h_GTP1.Text = p8h_GTP1.ToString("0.00");
			txt_p8h_GTP2.Text = p8h_GTP2.ToString("0.00");
			txt_p8h_GES.Text = p8h_GES.ToString("0.00");

			txt_pPik_GTP1.Text = pikGTP1.ToString("0.00");
			txt_pPik_GTP2.Text = pikGTP2.ToString("0.00");
			txt_pPik_GES.Text = pikGES.ToString("0.00");

			txt_q_GTP1.Text = q_gtp1.ToString("0.00");
			txt_q_GTP2.Text = q_gtp2.ToString("0.00");
			txt_q_GES.Text = q_ges.ToString("0.00");

			txt_q8h_GES_check.Text = rashod8.ToString("0.00");
			//txt_p8h_GES_check.Text = p8h_GES.ToString("0.00");

		}
	}
}
