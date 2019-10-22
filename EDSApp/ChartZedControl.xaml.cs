using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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
using ZedGraph;

namespace EDSApp
{
    public class ChartZedSerie : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public static List<System.Drawing.Color> Colors;
        static ChartZedSerie()
        {
            Colors = new List<System.Drawing.Color>();
            Colors.Add(System.Drawing.Color.Red);
            Colors.Add(System.Drawing.Color.Green);
            Colors.Add(System.Drawing.Color.Blue);
            Colors.Add(System.Drawing.Color.Purple);
            Colors.Add(System.Drawing.Color.YellowGreen);
            Colors.Add(System.Drawing.Color.Pink);
            Colors.Add(System.Drawing.Color.Orange);
            Colors.Add(System.Drawing.Color.Gray);
        }
        public static int indexColor = 0;

        public static System.Drawing.Color NextColor()
        {
            return Colors[indexColor++ % Colors.Count];
        }
        public string Header { get; set; }
        public LineItem Item { get; set; }

        public int Y2Index { get; set; }

        private bool _isVisible;
        public bool IsVisible {
            get => _isVisible; set {
                _isVisible = value;
                NotifyChanged("IsVisible");
            }
        }
        protected System.Drawing.Color _color;

        public System.Drawing.Color Color {
            get {
                return _color;
            }
            set {
                _color = value;
                FillBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_color.A, _color.R, _color.G, _color.B));
            }
        }

        private double _value;
        public double Value {
            get => _value;
            set {
                _value = value;
                NotifyChanged("Value");
            }
        }


        public System.Windows.Media.Brush FillBrush { get; set; }
        public SortedList<DateTime, double> Data { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для ChartZedControl.xaml
    /// </summary>
    public partial class ChartZedControl : UserControl, INotifyPropertyChanged
    {


        public GraphPane CurrentGraphPane { get; set; }
        public bool AllYAxisIsVisible = false;
        public Dictionary<GraphPane, LineItem> CrossHairItems;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public ObservableCollection<ChartZedSerie> ObsSeries;
        public Dictionary<string, ChartZedSerie> Series;
        private DateTime _minDate;
        private DateTime _maxDate;
        private DateTime _cursorDate;

        public DateTime MinDate { get => _minDate; set { _minDate = value; NotifyChanged("MinDate"); } }
        public DateTime MaxDate { get => _maxDate; set { _maxDate = value; NotifyChanged("MinDate"); } }
        public DateTime CursorDate { get => _cursorDate; set { _cursorDate = value; NotifyChanged("CursorDate"); } }

        public bool ShowCrossHair { get; set; }
        Dictionary<GraphPane, LineObj> xHairOld = new Dictionary<GraphPane, LineObj>();

        public ChartZedControl()
        {
            ObsSeries = new ObservableCollection<ChartZedSerie>();
            
            InitializeComponent();

            CurrentGraphPane = chart.GraphPane;
            chart.MouseMove += Chart_MouseMove;

        }



        private int prevX;
        private void Chart_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!ShowCrossHair && !(e.Button == System.Windows.Forms.MouseButtons.Right))
                return;

            double x, y;
            CurrentGraphPane.ReverseTransform(e.Location, out x, out y);

            XDate dt = new XDate(x);
            CursorDate = dt.DateTime;

            foreach (ChartZedSerie serie in ObsSeries)
            {
                try
                {
                    var d = serie.Data.First(de => de.Key >= CursorDate);
                    serie.Value = serie.Data[d.Key];
                }
                catch { }

            }

            
            
            chart.Invalidate(new Region(new System.Drawing.Rectangle(chart.DisplayRectangle.Left + prevX - 5, chart.DisplayRectangle.Top, 10, chart.DisplayRectangle.Height)), false);
            prevX = e.Location.X;
            foreach (GraphPane pane in chart.MasterPane.PaneList)
            {
                LineItem CrossHairItem = CrossHairItems[pane];
                CrossHairItem.Points[0].X = x;
                CrossHairItem.Points[1].X = x;
                CrossHairItem.Points[0].Y = pane.YAxis.Scale.Min;
                CrossHairItem.Points[1].Y = pane.YAxis.Scale.Max;
            }

            chart.Invalidate(new Region(new System.Drawing.Rectangle(chart.DisplayRectangle.Left + e.Location.X - 5, chart.DisplayRectangle.Top, 10, chart.DisplayRectangle.Height)), false);
        }

        public void initControl()
        {
            pnlButtons.DataContext = this;
            ObsSeries = new ObservableCollection<ChartZedSerie>();
            grdLegend.ItemsSource = ObsSeries;
        }


        public void init(bool newGraphPane = false, string xFormat = "dd.MM")
        {

            GraphPane graphPane = CurrentGraphPane;
            if (newGraphPane)
            {
                graphPane = new GraphPane();
                CurrentGraphPane = graphPane;
                chart.MasterPane.Add(graphPane);

            }
            chart.MasterPane.SetLayout(chart.CreateGraphics(), PaneLayout.SingleColumn);

            graphPane.CurveList.Clear();
            graphPane.XAxis.Type = AxisType.Date;
            graphPane.XAxis.Scale.Format = xFormat;
            graphPane.XAxis.Title.IsVisible = false;
            graphPane.YAxis.Title.IsVisible = false;
            graphPane.YAxis.Scale.FontSpec.Size = 5;
            graphPane.YAxis.Scale.IsUseTenPower = false;
            graphPane.YAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MinorTic.IsOpposite = false;
            graphPane.YAxis.MajorTic.IsOpposite = false;
            xHairOld.Add(graphPane, new LineObj());

            graphPane.Title.IsVisible = false;
            graphPane.Legend.IsVisible = false;
            chart.IsZoomOnMouseCenter = false;


            graphPane.XAxis.Scale.FontSpec.Size = 10;
            graphPane.XAxis.MajorGrid.IsVisible = true;

            chart.IsSynchronizeXAxes = true;

            chart.IsEnableWheelZoom = false;


            ChartZedSerie.indexColor = 0;


        }

        public void refreshCrossHair()
        {
            if (CrossHairItems == null)
                CrossHairItems = new Dictionary<GraphPane, LineItem>();
            LineItem CrossHairItem;
            foreach (GraphPane pane in chart.MasterPane.PaneList)
            {
                if (!CrossHairItems.ContainsKey(pane))
                {
                    CrossHairItem = pane.AddCurve("ch", new double[] { 0, 0 }, new double[] { 0, 1 }, System.Drawing.Color.Black, SymbolType.None);
                    CrossHairItems.Add(pane, CrossHairItem);
                }

                CrossHairItem = CrossHairItems[pane];
                CrossHairItem.Points[0].X = pane.XAxis.Scale.Min;
                CrossHairItem.Points[1].X = pane.XAxis.Scale.Min;
                CrossHairItem.Points[0].Y = pane.YAxis.Scale.Min;
                CrossHairItem.Points[1].Y = pane.YAxis.Scale.Max;

            }

        }


        public void refreshDates()
        {
            DateTime min = DateTime.MaxValue;
            DateTime max = DateTime.MinValue;
            foreach (ChartZedSerie ser in ObsSeries)
            {
                DateTime minD = ser.Data.Keys.Min();
                DateTime maxD = ser.Data.Keys.Max();
                min = min > minD ? minD : min;
                max = max < maxD ? maxD : max;
            }
            foreach (GraphPane pane in chart.MasterPane.PaneList)
            {
                pane.XAxis.Scale.Min = XDate.DateTimeToXLDate(min);
                pane.XAxis.Scale.Max = XDate.DateTimeToXLDate(max);
            }
        }

        public ChartZedSerie AddSerie(String header, SortedList<DateTime, double> values, System.Drawing.Color color,
            bool line, bool symbol, bool dash = false, int y2axisIndex = -1, bool isVisible = true, double min = double.MinValue, double max = double.MaxValue)
        {
            if (values.Count == 0)
                return null;
            GraphPane graphPane = CurrentGraphPane;
            PointPairList points = new PointPairList();
            foreach (KeyValuePair<DateTime, double> de in values)
            {
                points.Add(new PointPair(new XDate(de.Key), de.Value));
            }
            ChartZedSerie serie = new ChartZedSerie();
            serie.Header = header;
            serie.Data = values;
            serie.Color = color;
            serie.IsVisible = true;
            LineItem lineItem = graphPane.AddCurve(header, points, color, symbol ? SymbolType.Circle : SymbolType.None);
            serie.Item = lineItem;

            lineItem.Line.Width = 2;

            lineItem.Line.IsVisible = line;
            if (symbol)
            {
                lineItem.Symbol.Size = 3f;
                lineItem.Symbol.Fill = new Fill(color);

            }
            ObsSeries.Add(serie);
            serie.IsVisible = isVisible;
            serie.Item.IsVisible = isVisible;
            serie.Y2Index = y2axisIndex;
            serie.Item.Line.DashOn = dash ? 50 : 0;
            serie.Item.Line.DashOff = dash ? 100 : 0;
            if (dash)
                lineItem.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;

            if (y2axisIndex > -1)
            {
                Y2Axis newAx;
                while (graphPane.Y2AxisList.Count() < y2axisIndex + 1)
                {
                    newAx = new Y2Axis();
                    newAx.Title.IsVisible = false;
                    newAx.Scale.FontSpec.Size = 5;
                    newAx.Scale.IsLabelsInside = true;
                    newAx.Scale.IsUseTenPower = false;
                    newAx.IsVisible = true;
                    newAx.Scale.FontSpec.Angle = (float)(-Math.PI / 2.0);
                    newAx.MajorTic.IsOpposite = false;
                    newAx.MinorTic.IsOpposite = false;
                    newAx.Scale.FontSpec.FontColor = System.Drawing.Color.White;
                    newAx.MajorTic.Color = System.Drawing.Color.White;
                    newAx.MinorTic.Color = System.Drawing.Color.White;
                    newAx.Color = System.Drawing.Color.White;
                    graphPane.Y2AxisList.Add(newAx);
                }
                newAx = graphPane.Y2AxisList[y2axisIndex];
                newAx.Color = color;
                newAx.Scale.FontSpec.FontColor = color;
                newAx.MajorTic.Color = color;
                newAx.MinorTic.Color = color;
                if (min > double.MinValue)
                    newAx.Scale.Min = min;
                if (max < double.MaxValue)
                    newAx.Scale.Max = max;

                lineItem.IsY2Axis = true;
                lineItem.YAxisIndex = y2axisIndex;

            }
            refreshDates();
            refreshCrossHair();
            chart.AxisChange();
            chart.Invalidate();

            return serie;
        }



        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chb = sender as CheckBox;
                ChartZedSerie ser = grdLegend.SelectedItem as ChartZedSerie;
                ser.Item.IsVisible = chb.IsChecked.Value;
                ser.IsVisible = chb.IsChecked.Value;
                foreach (GraphPane pane in chart.MasterPane.PaneList)
                    refresh(pane);
            }
            catch { }
        }

        public void refresh(GraphPane graphPane)
        {
            if (!AllYAxisIsVisible)
            {
                for (int i = 0; i < graphPane.Y2AxisList.Count; i++)
                {
                    bool vis = false;
                    foreach (ChartZedSerie ser in ObsSeries)
                    {
                        if (ser.IsVisible && (ser.Y2Index == i))
                        {
                            vis = true;
                        }
                    }
                    graphPane.Y2AxisList[i].IsVisible = vis;
                }
            }
            chart.AxisChange();
            chart.Invalidate();
        }

        private void SetAll(bool visible)
        {
            foreach (ChartZedSerie ser in ObsSeries)
            {
                ser.Item.IsVisible = visible;
                ser.IsVisible = visible;

            }
            foreach (GraphPane pane in chart.MasterPane.PaneList)
                refresh(pane);

        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SetAll(true);
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            SetAll(false);
        }



    }
}
