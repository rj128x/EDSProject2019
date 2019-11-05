using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;


namespace ZedControl
{
    public partial class ZedControl: UserControl
    {
        public ZedControl()
        {
            InitializeComponent();
            init();
        }



        public ZedGraph.ZedGraphControl Chart {
            get {
                return zedGraphControl1;
            }
        }



        public void init()
        {
            Chart.MouseMove += Chart_MouseMove;
            zedGraphControl1.Invalidate();
        }

        Graphics gr;
        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                button1.Top = 0;
                button1.Height = e.Y - 10;
                button1.Left = e.X;


                button2.Top = e.Y + 10;
                button2.Height = Height - e.Y - 10;
                button2.Left = e.X;
            }

        }

    }
}
