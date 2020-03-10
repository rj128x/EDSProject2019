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
        public int curX = 0;
        public int curY = 0;
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
            if (e.Button == MouseButtons.None && Math.Abs(curX - e.X) > 2)
            {
                pictureBox1.Left = e.X;
                pictureBox2.Left = e.X;
                
                
                curX = e.X;
            }
            if (e.Button == MouseButtons.None && Math.Abs(curY - e.Y) > 10) {
                pictureBox1.Top = 0;
                pictureBox1.Height = e.Y - 10;

                pictureBox2.Top = e.Y + 10;
                pictureBox2.Height = Height - e.Y - 10;
                
                curY = e.Y;
            }

        }

    }
}
