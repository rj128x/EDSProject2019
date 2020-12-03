using EDSProj;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для SDPMDKWindow.xaml
    /// </summary>
    public partial class SDPMDKWindow : Window
    {
        public SortedList<string, EDSPointInfo> AllPoints;
        public SDPMDKReport report;
        public DateTime currentDate;

        public SDPMDKWindow()
        {
            InitializeComponent();
             init();
        }

        public async Task<bool> init()
        {
            grdStatus.DataContext = EDSClass.Single;
            AllPoints = new SortedList<string, EDSPointInfo>();
            bool ok = true;
            ok &= await EDSPointsClass.getPointsArr("11VT.*", AllPoints);

            SDPMDKReport.init(AllPoints);
            return true;
        }

        private async void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            SDPMDKReport report = new SDPMDKReport();
            DateTime date = clndDate.SelectedDate.Value;
            DateTime dateEnd = date.AddDays(1);
            dateEnd = dateEnd > DateTime.Now ? DateTime.Now.AddMinutes(-10) : dateEnd;
            bool ok=await report.ReadData(date, dateEnd);
            
            grdEvents.ItemsSource=report.DKDataFull.Values;
        }
    }
}
