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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow() {
			InitializeComponent(); 
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "EDSApp");
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			ReportWindow win = new EDSApp.ReportWindow();
            win.Show();
            this.Close();
        }

		private void buttonWater_Click(object sender, RoutedEventArgs e) {
			WaterWindow win = new WaterWindow();
            win.Show();
            this.Close();
        }

		private void buttonDiag_Click(object sender, RoutedEventArgs e) {
			DiagWindow win = new DiagWindow();
            win.Show();
            this.Close();
        }

        private void buttonAVRCHM_Click(object sender, RoutedEventArgs e)
        {            
            AVRCHMReportWindow win = new AVRCHMReportWindow();
            win.Show();
            this.Close();
        }

        private void buttonDiad_Click(object sender, RoutedEventArgs e)
        {
            DiagWindow win = new DiagWindow();
            win.Show();
            this.Close();
        }
    }
}
