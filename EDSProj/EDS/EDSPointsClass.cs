using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDSProj
{
	public class EDSPointInfo
	{
		public string IESS { get; set; }
		public string Desc { get; set; }
		public string TG { get; set; }
		public List<int> Groups { get; set; }
		public string FullName { get; set; }
		public string AC { get; set; }
		public bool IsShade { get; set; }

		public EDSPointInfo(string iess, string desc, string tg) {
			this.IESS = iess;
			this.Desc = desc;
			this.TG = tg;
			//this.AC = ac;
			this.IsShade = iess.Contains("30VT");
			Groups = new List<int>();
			try {
				string[] groups = tg.Split(new char[] { ';' });
				foreach (string group in groups) {
					try {
						Groups.Add(Int32.Parse(group));
					} catch { }
				}
			} catch { }
			this.FullName = String.Format("{0,-40}{1}", iess, desc);
		}
	}

	public class EDSPointsClass
	{
		protected static SortedList<string, EDSPointInfo> _allAnalogPoints { get; set; }

		public async static Task<SortedList<string, EDSPointInfo>> GetAllAnalogPoints() {

			if (_allAnalogPoints == null) {
				bool ok=await LoadAnalogPointsFromServer();
			}
			return _allAnalogPoints;

		}

		protected static void GetAllPoints1() {
			_allAnalogPoints = new SortedList<string, EDSPointInfo>();
			try {
				string[] lines = System.IO.File.ReadAllLines("Data/allPoints.txt");
				foreach (string line in lines) {
					if (!line.Contains("POINT"))
						continue;
					try {
						Regex regex = new Regex(@"RT=(\w+).+IESS='([^']+).+DESC='([^']+).+AC='([^']+).+TG='([^']+)");
						Match match = regex.Match(line);
						if (match.Success) {
							string type = match.Groups[1].Value.ToLower();
							string iess = match.Groups[2].Value;
							string desc = match.Groups[3].Value;
							string ac = match.Groups[4].Value;
							string tg = match.Groups[5].Value;
							if (type == "analog" || type == "double") {
								_allAnalogPoints.Add(iess, new EDSPointInfo(iess, desc, tg));
							}
						}
					} catch (Exception e) {
						Logger.Info(String.Format("Ошибка при разборе строки {0}: {1}", line, e));
					}
				}
			} catch (Exception e) {
				Logger.Info(("Ошибка при получении списка точек: " + e.ToString()));
			}
		}

		protected async static Task<bool> LoadAnalogPointsFromServer() {
			_allAnalogPoints = new SortedList<string, EDSPointInfo>();
			try {
				if (!EDSClass.Connected)
					EDSClass.Connect();
				PointFilter filter = new PointFilter();
				List<PointType> types = new List<PointType>();
				types.Add(PointType.POINTTYPEANALOG);
                types.Add(PointType.POINTTYPEPACKED);
				types.Add(PointType.POINTTYPEDOUBLE);
                types.Add(PointType.POINTTYPEINT64);
                filter.rt = types.ToArray();



				bool finish = false;
				uint index = 0;
				getPointsRequest req = new getPointsRequest();
				req.authString = EDSClass.AuthStr;
				req.filter = filter;
				req.order = "";

				EDSClass.Single.GlobalInfo = "Получение списка точек";
				EDSClass.Single.Ready = false;
				EDSClass.Single.ProcessCalc = true;
				uint match = 0;
				while (!finish) {
					req.maxCount = 1000;
					req.startIdx = index;
					EDSClass.Single.ProcessInfo = String.Format("Точки {0} - {1} из {2}", req.startIdx, req.startIdx + req.maxCount, (match == 0 ? "?" : match.ToString()));
					getPointsResponse resp = await EDSClass.Client.getPointsAsync(req);

					//Point[] points = EDSClass.Client.getPoints(EDSClass.AuthStr, filter, "", index, 1000, out cnt, out total);
					foreach (Point point in resp.points) {
						try {
							string tg = string.Join(";", point.tg);
							_allAnalogPoints.Add(point.id.iess, new EDSPointInfo(point.id.iess, point.desc, tg));
						} catch { }
					}
					index += (uint)resp.points.Count();
					finish = index >= resp.matchCount;
					match = resp.matchCount;
				}

			} catch (Exception e) {
				Logger.Info(("Ошибка при получении списка точек: " + e.ToString()));

			} finally {
				EDSClass.Single.Ready = true;
				EDSClass.Single.ProcessCalc = false;
			}
			return true;
		}


        public static async  Task<bool> getPointsArr(string iess, SortedList<string, EDSPointInfo> points)
        {
            try
            {
                if (!EDSClass.Connected)
                    EDSClass.Connect();
                PointFilter filter = new PointFilter();
                List<PointType> types = new List<PointType>();
                types.Add(PointType.POINTTYPEANALOG);
                types.Add(PointType.POINTTYPEPACKED);
                types.Add(PointType.POINTTYPEDOUBLE);
                types.Add(PointType.POINTTYPEINT64);
                types.Add(PointType.POINTTYPEBINARY);
                filter.rt = types.ToArray();
                filter.iessRe = iess;
                


                bool finish = false;
                uint index = 0;
                getPointsRequest req = new getPointsRequest();
                req.authString = EDSClass.AuthStr;
                req.filter = filter;
                req.order = "";

                EDSClass.Single.GlobalInfo = "Получение списка точек";
                EDSClass.Single.Ready = false;
                EDSClass.Single.ProcessCalc = true;
                uint match = 0;
                while (!finish)
                {
                    req.maxCount = 1000;
                    req.startIdx = index;
                    EDSClass.Single.ProcessInfo = String.Format("Точки {0} - {1} из {2}", req.startIdx, req.startIdx + req.maxCount, (match == 0 ? "?" : match.ToString()));
                    getPointsResponse resp = await EDSClass.Client.getPointsAsync(req);

                    //Point[] points = EDSClass.Client.getPoints(EDSClass.AuthStr, filter, "", index, 1000, out cnt, out total);
                    foreach (Point point in resp.points)
                    {
                        try
                        {
                            string tg = string.Join(";", point.tg);
                            points.Add(point.id.iess, new EDSPointInfo(point.id.iess, point.desc, tg));
                        }
                        catch { }
                    }
                    index += (uint)resp.points.Count();
                    finish = index >= resp.matchCount;
                    match = resp.matchCount;
                }

            }
            catch (Exception e)
            {
                Logger.Info(("Ошибка при получении списка точек: " + e.ToString()));

            }
            finally
            {
                EDSClass.Single.Ready = true;
                EDSClass.Single.ProcessCalc = false;
            }
            return true;
        }

    }
}
