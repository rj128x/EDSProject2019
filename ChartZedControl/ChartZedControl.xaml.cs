using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZedGraph;

namespace ChartZedControl
{
    public class ChartZedYAxis : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public Axis item { get; set; }
        public ChartZedControl Parent { get; set; }
        private bool _isNotAuto;
        public bool IsNotAuto {
            get => _isNotAuto; set {
                _isNotAuto = value;
                item.Scale.MinAuto = !value;
                item.Scale.MaxAuto = !value;
                Parent.RefreshAll();
                NotifyChanged("IsNotAuto");
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

        private double _minVal;
        public double MinVal {
            get => _minVal;
            set {
                _minVal = value;
                if (IsNotAuto)
                {
                    item.Scale.Min = value;
                    Parent.RefreshAll();
                    NotifyChanged("MinVal");
                }
            }
        }

        private double _maxVal;
        public double MaxVal {
            get => _maxVal;
            set {
                _maxVal = value;
                if (IsNotAuto)
                {
                    item.Scale.Max = value;
                    Parent.RefreshAll();
                    NotifyChanged("MaxVal");
                }
            }
        }

        public string Header { get; set; }


        public System.Windows.Media.Brush FillBrush { get; set; }


    }
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
        public bool IsSymbol { get; set; }
        public bool IsDash { get; set; }

        public static System.Drawing.Color NextColor()
        {
            return Colors[indexColor++ % Colors.Count];
        }
        public string Header { get; set; }

        protected string _yAxHeader;
        public string YAxHeader {
            get => _yAxHeader; set {
                _yAxHeader = value;
                NotifyChanged("YAxHeader");
            }
        }

        protected System.Windows.Media.Brush _yAxColor;
        public System.Windows.Media.Brush YAxColor {
            get => _yAxColor; set {
                _yAxColor = value;
                NotifyChanged("YAxColor");
            }
        }


        public LineItem Item { get; set; }
        //public List<LineItem> AllItems { get; set; }
        public GraphPane Pane { get; set; }

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
        public SortedList<double, double> DataPoints { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для ChartZedControl.xaml
    /// </summary>
    public partial class ChartZedControl : UserControl, INotifyPropertyChanged
    {
        public ZedGraphControl chart;
        public System.Drawing.Color BGColor { get; set; }
        public System.Drawing.Color FontColor { get; set; }

        public GraphPane CurrentGraphPane { get; set; }
        public bool AllYAxisIsVisible = false;
        public bool isDoubleXAxis = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public ObservableCollection<ChartZedSerie> ObsSeries;

        public ObservableCollection<ChartZedYAxis> ObsYAxis;

        private DateTime _cursorDate;
        private Double _cursorX;


        public DateTime CursorDate { get => _cursorDate; set { _cursorDate = value; NotifyChanged("CursorDate"); } }
        public Double CursorX { get => _cursorX; set { _cursorX = value; NotifyChanged("CursorX"); } }



        public ChartZedControl()
        {

            ObsSeries = new ObservableCollection<ChartZedSerie>();
            ObsYAxis = new ObservableCollection<ChartZedYAxis>();
            BGColor = System.Drawing.Color.Black;
            FontColor = System.Drawing.Color.Orange;

            InitializeComponent();


        }



        protected void ChartMouseMoveFunction(System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                double x, y;

                CurrentGraphPane.ReverseTransform(e.Location, out x, out y);


                if (!isDoubleXAxis)
                {
                    XDate dt = new XDate(x);
                    CursorDate = dt.DateTime;

                    foreach (ChartZedSerie serie in ObsSeries)
                    {
                        if (serie.Data.Count > 0 && serie.Data.Keys.Min() < CursorDate && CursorDate < serie.Data.Keys.Max())
                        {
                            var d = serie.Data.First(de => de.Key >= CursorDate);
                            serie.Value = serie.Data[d.Key];
                        }
                        else
                        {
                            serie.Value = Double.PositiveInfinity;
                        }

                    }
                }
                else
                {
                    CursorX = x;

                    foreach (ChartZedSerie serie in ObsSeries)
                    {
                        if (serie.DataPoints.Count > 0 && serie.DataPoints.Keys.Min() < CursorX && CursorX < serie.DataPoints.Keys.Max())
                        {
                            var d = serie.DataPoints.First(de => de.Key >= CursorX);
                            serie.Value = serie.DataPoints[d.Key];
                        }
                        else
                        {
                            serie.Value = Double.PositiveInfinity;
                        }

                    }
                }
            }
            catch { }
        }

        public void initControl()
        {
            ObsSeries.Clear();
            ObsYAxis.Clear();

            chart = chartControl.Chart;


            chart.MasterPane.PaneList.Clear();
            pnlButtons.DataContext = this;
            grdLegend.ItemsSource = ObsSeries;
            grdYAxis.ItemsSource = ObsYAxis;


            CurrentGraphPane = chart.GraphPane;

            //chart.MouseMove += Chart_MouseMove;
            chartControl.ChartMouseMoveFunction = ChartMouseMoveFunction;

        }


        public void init(bool newGraphPane = false, string xFormat = "dd.MM", bool pointChart = false)
        {
            if (chart == null)
            {
                initControl();

            }
            GraphPane graphPane = CurrentGraphPane;
            if (graphPane == null)
                graphPane = chart.GraphPane;

            if (newGraphPane)
            {
                graphPane = new GraphPane();
                CurrentGraphPane = graphPane;
                chart.MasterPane.Add(graphPane);

            }
            graphPane.IsIgnoreMissing = false;
            isDoubleXAxis = pointChart;
            chart.MasterPane.SetLayout(chart.CreateGraphics(), PaneLayout.SingleColumn);

            graphPane.CurveList.Clear();
            graphPane.Chart.Fill.Color = BGColor;
            graphPane.Fill.Color = BGColor;
            graphPane.Chart.Fill.Type = FillType.Solid;
            graphPane.Chart.Border.Color = System.Drawing.Color.Gray;
            graphPane.XAxis.Type = isDoubleXAxis ? AxisType.Linear : AxisType.Date;
            graphPane.XAxis.Scale.Format = xFormat;
            graphPane.XAxis.Title.IsVisible = false;
            graphPane.XAxis.Scale.FontSpec.FontColor = FontColor;
            //graphPane.YAxis.Title.IsVisible = false;
            /*graphPane.YAxis.Scale.FontSpec.Size = 9;
            graphPane.YAxis.Scale.FontSpec.IsBold = true;
            graphPane.YAxis.Scale.IsUseTenPower = false;
            graphPane.YAxis.MajorGrid.IsVisible = true;
            graphPane.YAxis.MinorTic.IsOpposite = false;
            graphPane.YAxis.MajorTic.IsOpposite = false;*/
            initAxis(graphPane.YAxis);
            graphPane.YAxis.MajorGrid.Color = System.Drawing.Color.DarkGray;
            graphPane.YAxis.MajorGrid.IsZeroLine = false;



            graphPane.Title.IsVisible = false;
            graphPane.Legend.IsVisible = false;
            chart.IsZoomOnMouseCenter = false;


            graphPane.XAxis.Scale.FontSpec.Size = 10;
            graphPane.XAxis.MajorGrid.IsVisible = true;
            graphPane.XAxis.MajorGrid.Color = System.Drawing.Color.Gray;

            chart.IsSynchronizeXAxes = true;

            chart.IsEnableWheelZoom = false;
            chart.IsEnableVZoom = false;
            chart.IsEnableHZoom = true;


            ChartZedSerie.indexColor = 0;


        }




        public void refreshXScale()
        {
            if (!isDoubleXAxis)
            {
                DateTime min = DateTime.MaxValue;
                DateTime max = DateTime.MinValue;
                foreach (ChartZedSerie ser in ObsSeries)
                {
                    try
                    {
                        DateTime minD = ser.Data.Keys.Min();
                        DateTime maxD = ser.Data.Keys.Max();
                        min = min > minD ? minD : min;
                        max = max < maxD ? maxD : max;
                    }
                    catch { }
                }
                foreach (GraphPane pane in chart.MasterPane.PaneList)
                {
                    pane.XAxis.Scale.Min = XDate.DateTimeToXLDate(min);
                    pane.XAxis.Scale.Max = XDate.DateTimeToXLDate(max);
                }
            }
            else
            {
                double min = Double.MaxValue;
                Double max = Double.MinValue;
                foreach (ChartZedSerie ser in ObsSeries)
                {
                    try
                    {
                        Double minD = ser.DataPoints.Keys.Min();
                        Double maxD = ser.DataPoints.Keys.Max();
                        min = min > minD ? minD : min;
                        max = max < maxD ? maxD : max;
                    }
                    catch { }
                }
                foreach (GraphPane pane in chart.MasterPane.PaneList)
                {
                    pane.XAxis.Scale.Min = min;
                    pane.XAxis.Scale.Max = max;
                }
            }
        }

        protected bool IsVisibleYAxis(GraphPane pane, int yIndex)
        {
            bool vis = false;
            foreach (ChartZedSerie ser in ObsSeries)
            {
                if (ser.IsVisible && (ser.Y2Index == yIndex) && (ser.Pane == pane))
                {
                    vis = true;
                }
            }
            return vis;
        }



        public void refreshYAxisGrid()
        {
            ObsYAxis.Clear();
            int paneIndex = 0;
            foreach (GraphPane pane in chart.MasterPane.PaneList)
            {
                paneIndex++;
                for (int y2 = -1; y2 < pane.Y2AxisList.Count; y2++)
                {
                    Axis yAx = y2 == -1 ? pane.YAxis as Axis : pane.Y2AxisList[y2];
                    bool vis = IsVisibleYAxis(pane, y2);
                    if (vis)
                    {
                        ChartZedYAxis ax = new ChartZedYAxis();
                        ax.Parent = this;
                        ax.item = yAx;
                        ax.Color = yAx.Color;
                        ax.IsNotAuto = !(yAx.Scale.MinAuto && yAx.Scale.MaxAuto);
                        ax.MinVal = yAx.Scale.Min;
                        ax.MaxVal = yAx.Scale.Max;
                        ax.Header = String.Format("{0}-{1}", paneIndex, y2 + 2);
                        yAx.Title.Text = ax.Header;

                        yAx.Title.IsVisible = true;
                        yAx.Title.FontSpec.Size = 10;
                        //yAx.Scale.FontSpec.Angle = (float)(-Math.PI / 2.0); ;


                        ObsYAxis.Add(ax);

                    }
                    foreach (ChartZedSerie ser in ObsSeries)
                    {
                        if ((ser.Y2Index == y2) && (ser.Pane == pane))
                        {

                            ser.YAxColor = ser.IsVisible ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(yAx.Color.A, yAx.Color.R, yAx.Color.G, yAx.Color.B)) :
                                new SolidColorBrush(System.Windows.Media.Colors.White);
                            ser.YAxHeader = ser.IsVisible ? String.Format("{0}-{1}", paneIndex, y2 + 2) : "-";

                        }
                    }
                    yAx.Title.FontSpec.FontColor = vis ? yAx.Color : BGColor;
                }
            }


        }


        protected void initAxis(Axis newAx)
        {
            //newAx.Title.IsVisible = false;
            newAx.Scale.FontSpec.Size = 9;
            newAx.Scale.FontSpec.IsBold = true;
            newAx.Scale.IsLabelsInside = newAx is Y2Axis;
            newAx.Scale.IsUseTenPower = false;
            newAx.IsVisible = true;
            newAx.Scale.FontSpec.Angle = newAx is Y2Axis ? (float)(-Math.PI / 2.0) : 0;
            newAx.MajorTic.IsOpposite = false;
            newAx.MinorTic.IsOpposite = false;
            newAx.MajorGrid.IsZeroLine = false;
            newAx.Scale.FontSpec.FontColor = BGColor;
            newAx.MajorTic.Color = BGColor;
            newAx.MinorTic.Color = BGColor;
            newAx.Color = BGColor;

        }

        public void setY2AxisCount(GraphPane graphPane, int cnt)
        {

                initAxis(graphPane.YAxis);


            if (cnt > -1)
            {
                Y2Axis newAx;
                while (graphPane.Y2AxisList.Count() < cnt)
                {
                    newAx = new Y2Axis();
                    initAxis(newAx);
                    graphPane.Y2AxisList.Add(newAx);
                }       
            }
        }

        public void updateSeries()
        {
            foreach (ChartZedSerie serie in ObsSeries)
            {

                if (serie.Y2Index > -1)
                {
                    serie.Item.YAxisIndex = serie.Y2Index;
                    serie.Item.IsY2Axis = true;
                }
            }
            refreshXScale();
            refreshYAxisGrid();
            foreach (GraphPane pane in chart.MasterPane.PaneList)
            {
                refresh(pane);
            }
        }

        public void UpdateSerieData(String header, SortedList<DateTime, double> values)
        {
            ChartZedSerie serie = ObsSeries.First(ser => ser.Header == header);
            GraphPane graphPane = serie.Pane;
            PointPairList points = new PointPairList();
            if (serie.Item != null)
                graphPane.CurveList.Remove(serie.Item);

            foreach (KeyValuePair<DateTime, double> de in values)
            {
                if (de.Value != double.PositiveInfinity && de.Value != double.NegativeInfinity)
                    points.Add(new PointPair(new XDate(de.Key), de.Value));
                else

                    points.Add(new PointPair(new XDate(de.Key), PointPair.Missing));

            }
            LineItem item = graphPane.AddCurve(header, points, serie.Color, serie.IsSymbol ? SymbolType.Circle : SymbolType.None);
            item.Line.Width = 2;
            
            if (serie.IsSymbol)
            {
                item.Symbol.Size = 3f;
                item.Symbol.Fill = new Fill(serie.Color);
            }

            item.IsVisible = serie.IsVisible;
            item.Line.DashOn = serie.IsDash ? 50 : 0;
            item.Line.DashOff = serie.IsDash ? 100 : 0;
            if (serie.IsDash)
                item.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;

            serie.Item = item;
            serie.Data = values;

            
        }

        


        public ChartZedSerie AddSerie(String header, SortedList<DateTime, double> values, System.Drawing.Color color,
            bool line, bool symbol, bool dash = false, int y2axisIndex = -1, bool isVisible = true, double min = double.MinValue, double max = double.MaxValue)
        {
            if (isDoubleXAxis)
                return null;
            //y2axisIndex += 1;
            if (values.Count == 0)
                return null;
            GraphPane graphPane = CurrentGraphPane;

            ChartZedSerie serie = new ChartZedSerie();
            serie.Color = color;
            serie.IsVisible = isVisible;
            serie.IsDash = dash;
            serie.IsSymbol = symbol;
            serie.Pane = graphPane;
            serie.Header = header;
            serie.Y2Index = y2axisIndex;

            ObsSeries.Add(serie);

            UpdateSerieData(header, values);



            if (y2axisIndex == -1)
            {
                initAxis(graphPane.YAxis);
                graphPane.YAxis.Color = color;

                graphPane.YAxis.MajorTic.Color = color;

                graphPane.YAxis.MinorTic.Color = color;


            }
            if (y2axisIndex > -1)
            {
                Y2Axis newAx;
                while (graphPane.Y2AxisList.Count() < y2axisIndex + 1)
                {
                    newAx = new Y2Axis();
                    initAxis(newAx);
                    graphPane.Y2AxisList.Add(newAx);
                }
                newAx = graphPane.Y2AxisList[y2axisIndex];
                initAxis(newAx);

                newAx.Color = color;
                newAx.Scale.FontSpec.FontColor = color;
                newAx.MajorTic.Color = color;
                newAx.MinorTic.Color = color;

                if (min > double.MinValue)
                    newAx.Scale.Min = min;
                if (max < double.MaxValue)
                    newAx.Scale.Max = max;

                serie.Item.IsY2Axis = true;
                serie.Item.YAxisIndex = y2axisIndex;




            }
            refreshXScale();
            refreshYAxisGrid();
            refresh(graphPane);
            return serie;
        }

        public void UpdatePointSerieData(String header, SortedList<double, double> data)
        {
            ChartZedSerie serie = ObsSeries.First(ser => ser.Header == header);
            GraphPane graphPane = serie.Pane;
            PointPairList points = new PointPairList();

            if (serie.Item != null)
                graphPane.CurveList.Remove(serie.Item);

            int i = 0;
            foreach (double x in data.Keys)
            {
                points.Add(new PointPair(x, data[x]));

                i++;
            }

            LineItem lineItem = graphPane.AddCurve(header, points, serie.Color, serie.IsSymbol ? SymbolType.Circle : SymbolType.None);
            serie.Item = lineItem;

            //lineItem.Line.IsVisible = serie.IsVisible;
            if (serie.IsSymbol)
            {
                lineItem.Symbol.Size = 1.5f;
                lineItem.Symbol.Fill = new Fill(serie.Color);
            }
            serie.Item.IsVisible = serie.IsVisible;

            serie.DataPoints = data;


        }

        public ChartZedSerie AddPointSerie(String header, SortedList<double, double> data,
            System.Drawing.Color color, bool line, bool symbol, int y2axisIndex = -1, bool isVisible = true)
        {
            if (!isDoubleXAxis)
                return null;
            GraphPane graphPane = CurrentGraphPane;

            ChartZedSerie serie = new ChartZedSerie();
            ObsSeries.Add(serie);
            serie.Header = header;
            serie.Color = color;
            serie.IsVisible = isVisible;
            serie.Pane = graphPane;
            serie.Y2Index = y2axisIndex;
            UpdatePointSerieData(header, data);

            if (y2axisIndex == -1)
            {
                initAxis(graphPane.YAxis);
                graphPane.YAxis.Color = color;

            }
            if (y2axisIndex > -1)
            {
                Y2Axis newAx;
                while (graphPane.Y2AxisList.Count() < y2axisIndex + 1)
                {
                    newAx = new Y2Axis();
                    initAxis(newAx);
                    graphPane.Y2AxisList.Add(newAx);
                }
                newAx = graphPane.Y2AxisList[y2axisIndex];
                initAxis(newAx);

                newAx.Color = color;
                newAx.Scale.FontSpec.FontColor = color;
                newAx.MajorTic.Color = color;
                newAx.MinorTic.Color = color;



                serie.Item.IsY2Axis = true;
                serie.Item.YAxisIndex = y2axisIndex;



            }
            refreshXScale();

            refreshYAxisGrid();
            refresh(graphPane);

            return serie;

        }


        public ChartZedSerie AddMixPointSerie(String header, List<double> xx, List<double> xy,
           System.Drawing.Color color, bool line, bool symbol, int y2axisIndex = -1, bool isVisible = true)
        {
            if (!isDoubleXAxis)
                return null;
            GraphPane graphPane = CurrentGraphPane;

            ChartZedSerie serie = new ChartZedSerie();
            ObsSeries.Add(serie);
            serie.Header = header;
            serie.Color = color;
            serie.IsVisible = isVisible;
            serie.Pane = graphPane;
            serie.Y2Index = y2axisIndex;
            serie.IsSymbol = symbol;


            PointPairList points = new PointPairList();



            int i = 0;
            foreach (double x in xx)
            {
                points.Add(new PointPair(x, xy[i]));

                i++;
            }

            LineItem lineItem = graphPane.AddCurve(header, points, serie.Color, serie.IsSymbol ? SymbolType.Circle : SymbolType.None);
            serie.Item = lineItem;

            lineItem.Line.IsVisible = line;
            if (serie.IsSymbol)
            {
                lineItem.Symbol.Size = 1.5f;
                lineItem.Symbol.Fill = new Fill(serie.Color);
            }
            serie.Item.IsVisible = serie.IsVisible;

            serie.DataPoints = new SortedList<double, double>();





            if (y2axisIndex == -1)
            {
                initAxis(graphPane.YAxis);
                graphPane.YAxis.Color = color;

            }
            if (y2axisIndex > -1)
            {
                Y2Axis newAx;
                while (graphPane.Y2AxisList.Count() < y2axisIndex + 1)
                {
                    newAx = new Y2Axis();
                    initAxis(newAx);
                    graphPane.Y2AxisList.Add(newAx);
                }
                newAx = graphPane.Y2AxisList[y2axisIndex];
                initAxis(newAx);

                newAx.Color = color;
                newAx.Scale.FontSpec.FontColor = color;
                newAx.MajorTic.Color = color;
                newAx.MinorTic.Color = color;



                serie.Item.IsY2Axis = true;
                serie.Item.YAxisIndex = y2axisIndex;


            }
            refreshXScale();

            refreshYAxisGrid();
            refresh(graphPane);

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

        public void RefreshAll()
        {
            foreach (GraphPane graphPane in chart.MasterPane.PaneList)
            {
                graphPane.AxisChange();

            }
            chart.Invalidate();
        }


        public void refresh(GraphPane graphPane)
        {
            bool vis = false;
            for (int i = -1; i < graphPane.Y2AxisList.Count; i++)
            {
                vis = IsVisibleYAxis(graphPane, i);


                if (!AllYAxisIsVisible)
                {
                    if (i >= 0)
                        graphPane.Y2AxisList[i].IsVisible = vis;
                    else
                        graphPane.YAxis.IsVisible = vis;
                }
                if (i >= 0)
                    graphPane.Y2AxisList[i].Scale.FontSpec.FontColor = vis ? graphPane.Y2AxisList[i].Color : BGColor;
                else
                    graphPane.YAxis.Scale.FontSpec.FontColor = vis ? graphPane.YAxis.Color : BGColor;

            }


            refreshYAxisGrid();

            graphPane.AxisChange();
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
