using EDSProj;
using EDSProj.EDSWebService;
using EDSProj.ModesCentre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSPBR
{
	class Program
	{
		static void Main(string[] args) {
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "pbrExport");
			//Logger.Info(string.Format("Считано аналоговых точек: {0}", EDSPointsClass.AllAnalogPoints.Count));

			/*uint cnt;
			uint total;
			if (!EDSClass.Connected)
				EDSClass.Connect();
			ReportConfig[] reports = EDSClass.Client.getReportsConfigs(EDSClass.AuthStr, null, 0, 1000, out cnt, out total);
			Console.WriteLine(reports.Count().ToString());
			foreach (ReportConfig report in reports) {
				Console.WriteLine(report.reportDefinitionFile);
				Console.WriteLine(report.id);				
			}*/

			/*if (!EDSClass.Connected)
				EDSClass.Connect();
			GlobalReportRequest req = new GlobalReportRequest();
			req.reportConfigId = 16;			
			req.dtRef = new Timestamp() { second = EDSClass.toTS(DateTime.Now) };

			uint reqId=EDSClass.Client.requestGlobalReport(EDSClass.AuthStr, req);
			bool ok=EDSClass.ProcessQuery(reqId);
			Console.WriteLine(ok.ToString());*/
			
			MCSettings.init("Data/MCSettings.xml");
			MCServerReader reader = new MCServerReader(DateTime.Now.Date);
            EDSClass.Disconnect();
            //Console.ReadLine();
		}
	}
}
