using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EDSProj
{
	public enum EDSReportPeriod { sec, minute, hour, day, month }
	public enum EDSReportFunction { avg, max, min, val, vyrab }
	public class TechGroupInfo
	{
		public string Name { get; set; }
		public string Desc { get; set; }
		public int Id { get; set; }
		public bool Selected { get; set; }
	}

	public delegate void StateChangeDelegate();

	public class EDSClass : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static EDSClass Single { get; protected set; }


		protected string _authStr { get; set; }
		protected edsPortTypeClient _client { get; set; }

		public static Dictionary<EDSReportPeriod, string> ReportPeriods { get; protected set; }
		public static Dictionary<EDSReportFunction, string> ReportFunctions { get; protected set; }


		protected static Dictionary<int, TechGroupInfo> _techGroups { get; set; }
		public async static Task<Dictionary<int, TechGroupInfo>> GetTechGroups() {

			if (_techGroups == null) {
				bool ok = await loadTechGroupsFromServer();
			}
			return _techGroups;

		}


		public void NotifyChanged(string propName) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		protected string _globalInfo;
		public String GlobalInfo {
			get {
				return _globalInfo;
			}
			set {
				_globalInfo = value;
				NotifyChanged("GlobalInfo");
			}
		}

		protected string _connectInfo;
		public String ConnectInfo {
			get {
				return _connectInfo;
			}
			set {
				_connectInfo = value;
				NotifyChanged("ConnectInfo");
			}
		}

		protected string _processInfo;
		public String ProcessInfo {
			get {
				return _processInfo;
			}
			set {
				_processInfo = value;
				NotifyChanged("ProcessInfo");
			}
		}

		protected bool _ready;
		public bool Ready {
			get {
				return _ready;
			}
			set {
				_ready = value;
				if (_ready) {
					AbortCalc = false;
					GlobalInfo = "...";					
				}
				NotifyChanged("Ready");
			}
		}

		protected bool _processCalc;
		public bool ProcessCalc {
			get {
				return _processCalc;
			}
			set {
				_processCalc = value;
				if (!_processCalc) {
					AbortCalc = false;
					ProcessInfo = "...";
				}
				NotifyChanged("ProcessCalc");
			}
		}

		protected bool _abortCalc;
		public bool AbortCalc {
			get {
				return _abortCalc;
			}
			set {
				_abortCalc = value;
				NotifyChanged("AbortCalc");
			}
		}

		public void Abort() {
			if (!AbortCalc && ProcessCalc) {
				AbortCalc = true;
				GlobalInfo = "Прерывание";
				ProcessInfo = "Прерывание";
			}
		}

		public EDSClass() {

		}

		public static long toTS(DateTime date) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan diff = date.ToUniversalTime() - origin;
			return (long)(diff.TotalSeconds);
		}

		public static DateTime fromTS(long sec) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return origin.AddSeconds(sec).ToLocalTime();
		}

		protected bool _connect() {
			Logger.Info("Подключение к EDS серверу");
			ConnectInfo = "Попытка подключения";
			if (_client == null) {
				_client = new edsPortTypeClient();                
			}
			Logger.Info("Проверка состояния подключения");
			if (_client.State != System.ServiceModel.CommunicationState.Opened) {
				_authStr = _client.login(Settings.Single.EDSUser, Settings.Single.EDSPassword, ClientType.CLIENTTYPEDEFAULT);                
			}
			Logger.Info("Состояние подключения: " + Client.State);
			ConnectInfo = "Состояние подключения: " + Client.State;
			bool connected = _client.State == System.ServiceModel.CommunicationState.Opened;
			Single.Ready = connected;
			return connected;
		}

        protected bool _disconnect()
        {
            try            {
                _client.logout(_authStr);
            }
            catch { }
            _client = null;
            Logger.Info("Проверка состояния подключения");
            _authStr = "";
           
            ConnectInfo = "Состояние подключения: откл" + Client.State;
            bool connected = false;
            Single.Ready = false;
            return false;
        }

        public static bool Connected {
			get {
				bool con = false;
				if (Single._client == null)
					con = false;
				else
					con = Single._client.State == System.ServiceModel.CommunicationState.Opened;
				if (!con)
					Single.Ready = false;
				return con;
			}
		}

		public static edsPortTypeClient Client {
			get {
				return Single._client;
			}
		}

		public static string AuthStr {
			get {
				return Single._authStr;
			}
		}

		public static bool Connect() {
			return Single._connect();
		}

        public static void Disconnect()
        {
            try
            {
                Single._disconnect();
            }
            catch { }

        }

        static EDSClass() {
			Single = new EDSClass();
			ReportPeriods = new Dictionary<EDSReportPeriod, string>();
			ReportPeriods.Add(EDSReportPeriod.sec, "Секунда");
			ReportPeriods.Add(EDSReportPeriod.minute, "Минута");
			ReportPeriods.Add(EDSReportPeriod.hour, "Час");
			ReportPeriods.Add(EDSReportPeriod.day, "Сутки");
			ReportPeriods.Add(EDSReportPeriod.month, "Месяц");

			ReportFunctions = new Dictionary<EDSReportFunction, string>();
			ReportFunctions.Add(EDSReportFunction.avg, "Среднее");
			ReportFunctions.Add(EDSReportFunction.vyrab, "Выработка");
			ReportFunctions.Add(EDSReportFunction.min, "Минимум");
			ReportFunctions.Add(EDSReportFunction.max, "Максимум");
			ReportFunctions.Add(EDSReportFunction.val, "Значение");
		}

		public static string getReportFunctionName(EDSReportFunction func) {
			string name = "AVG";
			switch (func) {
				case EDSReportFunction.avg:
					name = "AVG";
					break;
				case EDSReportFunction.vyrab:
					name = "AVG";
					break;
				case EDSReportFunction.val:
					name = "VALUE";
					break;
				case EDSReportFunction.min:
					name = "MIN_VALUE";
					break;
				case EDSReportFunction.max:
					name = "MAX_VALUE";
					break;
			}
			return name;
		}

		public static long getPeriodSeconds(EDSReportPeriod period) {
			long res = 3600;
			switch (period) {
				case EDSReportPeriod.sec:
					res = 1;
					break;
				case EDSReportPeriod.minute:
					res = 60;
					break;
				case EDSReportPeriod.hour:
					res = 3600;
					break;
				case EDSReportPeriod.day:
					res = 3600 * 24;
					break;
			}
			return res;
		}

		public static bool ProcessQuery(uint id) {
			Single.ProcessCalc = true;
			Single.Ready = false;
			bool finished = false;
			RequestStatus status;
			float progress;
			string msg;
			int i = 0;
			bool ok = false;
			try {
				do {
					Thread.Sleep(200);
					i++;
					Single._client.getRequestStatus(Single._authStr, id, out status, out progress, out msg);
					Logger.Info(String.Format("{3} {0}: {1} ({2})", status, progress * 100, msg, i));
					ok = status == RequestStatus.REQUESTSUCCESS;
					Single.ProcessInfo = String.Format("{0}: {1:0.00}% ({2})", msg, progress * 100, i);
					finished = ok || i >= 1500;
					if (Single.AbortCalc) {
						Single.AbortCalc = false;
						finished = true;
						ok = false;
					}
				} while (!finished);
			} catch (Exception e) {
				Logger.Info(e.ToString());
			}

			Single.ProcessCalc = false;
			Single.Ready = true;

			return ok;
		}

		public static async Task<bool> ProcessQueryAsync(uint id) {
			Single.ProcessCalc = true;
			Single.Ready = false;
			bool finished = false;
			int i = 0;
			bool ok = false;
			getRequestStatusRequest req = new getRequestStatusRequest(Single._authStr, id);
			try {
				do {
					Thread.Sleep(500);
					i++;
					getRequestStatusResponse res = await Single._client.getRequestStatusAsync(req);
					Logger.Info(String.Format("{3} {0}: {1} ({2})", res.status, res.progress * 100, res.message, i));
					ok = res.status == RequestStatus.REQUESTSUCCESS;
					Single.ProcessInfo = String.Format("{0}: {1:0.00}% ({2})", res.message, res.progress * 100, i);
					finished = ok || i >= 100000;
					if (Single.AbortCalc) {
						Single.AbortCalc = false;
						finished = true;
						ok = false;
					}

				} while (!finished);
			} catch (Exception e) {
				Logger.Info(e.ToString());
			}
			Single.ProcessCalc = false;
			Single.Ready = true;
			return ok;
		}

		protected async static Task<bool> loadTechGroupsFromServer() {
			if (!Connected)
				Connect();

			Single.GlobalInfo = "Получение групп точек";
			Single.ProcessCalc = true;
			Single.Ready = false;
			_techGroups = new Dictionary<int, TechGroupInfo>();
			try {
				getTechnologicalGroupsRequest req = new getTechnologicalGroupsRequest();
				req.authString = EDSClass.AuthStr;
				getTechnologicalGroupsResponse resp = await Client.getTechnologicalGroupsAsync(EDSClass.AuthStr);

				foreach (Group gr in resp.groups) {
					if (!String.IsNullOrEmpty(gr.desc)) {
						TechGroupInfo tg = new TechGroupInfo();
						tg.Id = gr.id;
						tg.Name = gr.name;
						tg.Desc = gr.desc;
						_techGroups.Add(gr.id, tg);
					}
				}
			} finally {
				Single.ProcessCalc = false;
				Single.Ready = true;
			}
			return true;
			//_techGroups = Result;
		}

		public static double getVal(PointValue val) {
			double res = 0;
			try {
				if (val.avSpecified)
					res= (double)val.av;
				if (val.bvSpecified)
					res = val.bv.Value ? 1.0 : 0.0;
				if (val.davSpecified)
					res = (double)val.dav;
				if (val.ipvSpecified)
					res = (double)val.ipv;
				if (val.pvSpecified)
					res = (double)val.pv;
			} catch {

			}
			return res;
		}


	}
}
