using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.ModesCentre
{
	public class MCSettingsRecord
	{
		public int MCCode { get; set; }
		public int PiramidaCode { get; set; }
		public bool WriteIntegratedData { get; set; }
		public bool WriteToEDS { get; set; }
		public bool Autooper { get; set; }
		public string MCName { get; set; }
		public string EDSPoint { get; set; }
        public string EDSPointSmooth { get; set; }
        public string IntegratedEDSPoint { get; set; }
        
    }


	public class MCSettings
	{
		public string MCServer { get; set; }
		public string MCUser { get; set; }
		public string MCPassword { get; set; }
		public List<MCSettingsRecord> MCData { get; set; }
		public static MCSettings Single { get; protected set; }
        

        public static void init(string filename = null) {
			if (filename == null) {
				filename = "Data\\MCSettings.xml";
			}
			MCSettings settings = XMLSer<MCSettings>.fromXML(filename);

			Single = settings;
		}
	}
}
