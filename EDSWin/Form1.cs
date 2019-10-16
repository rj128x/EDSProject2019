using EDSProj;
using EDSProj.ModesCentre;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDSWin
{
	public partial class Form1 : Form
	{
		public Form1() {
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) {

		}

		private void button1_Click(object sender, EventArgs e) {
			Logger.InitFileLogger("c:/edsLogs/", "eds");
			Settings.init("DATA/Settings.xml");
			Logger.Info(Settings.Single.EDSUser);
			MCSettings.init("DATA/MCSettings.xml");
			Logger.Info(MCSettings.Single.MCServer);
			Logger.Info("hello");
			MCServerReader reader = new MCServerReader(DateTime.Now.Date);
		}
	}
}
