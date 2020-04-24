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
using ChartZedControl;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для ReportResultwindow.xaml
	/// </summary>
	/// 
	public partial class ReportResultWindow : Window
	{
		public ChartZedControl.ChartZedControl chart => i_chart;
		public ReportResultWindow() {
			InitializeComponent();
		}

	}
}
