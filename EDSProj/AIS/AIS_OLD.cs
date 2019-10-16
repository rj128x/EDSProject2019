using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.AIS
{
	public class AISPointInfo
	{
		public int Obj { get; set; }
		public int ObjType { get; set; }
		public int Item { get; set; }
		public string DBName { get; set; }
		public string EDSPoint { get; set; }
		public string Desc { get; set; }
	}

	public class AIS_OLD
	{
		public List<AISPointInfo> AISPoints { get; set; }

		public void readPoints() {
			AISPoints = new List<AISPointInfo>();
			string[] lines = System.IO.File.ReadAllLines("Data/ais_old_NEWPOINTS.txt");
			foreach (string line in lines) {
				try {
					string[] parts = line.Split(new char[] { '\t' });
					string iess = parts[0];
					string desc = parts[1];
					string code = parts[2];
					string[] codeParts = code.Split('-');
					int obj = Int32.Parse(codeParts[0]);
					int item = Int32.Parse(codeParts[1]);

					if (obj >= 0 && item > 0 && !String.IsNullOrEmpty(iess) && !(desc.Contains("1 мин"))) {
						AISPointInfo point = new AISPointInfo();
						point.EDSPoint = iess;
						point.Obj = obj;
						point.Item = item;
						point.Desc = desc;
						point.ObjType = 0;
						if (obj == 8737 || obj == 8738) {
							point.DBName = "Piramida3000";
						}
						if (obj == 8739 || obj == 8740) {
							point.DBName = "PiramidaTU";
						}
						if (obj == 1) {
							point.DBName = "Piramida3000";
							point.ObjType = 2;
						}
						if (obj == 0) {
							point.DBName = "Piramida3000";
							point.ObjType = 2;
						}
						AISPoints.Add(point);
					}
				} catch { }
			}
		}

		public static string DateFormat = "yyyy-MM-dd HH:mm:ss";

		public static SqlConnection getConnection(string dbName) {
			String str = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3};Trusted_Connection=False;",
				@"sr-votges-015\sqlexpress", dbName, "sa", "psWD!159!");
			return new SqlConnection(str);
		}

		public void readDataFromDB(DateTime dateStart, DateTime dateEnd) {
			List<ShadeSelector> sel = new List<ShadeSelector>();
			List<Shade> shades = new List<Shade>();
			List<ShadeValue> vals = new List<ShadeValue>();

			List<DateTime> dates = new List<DateTime>();
			DateTime dt = dateStart.AddMinutes(30);
			while (dt <= dateEnd) {
				dates.Add(dt);
				dt = dt.AddMinutes(30);
			}

			try {
				foreach (AISPointInfo point in AISPoints) {
					SqlConnection con = getConnection(point.DBName);
					con.Open();
					string comSTR = "";
					comSTR = String.Format("SELECT DATA_DATE,VALUE0 FROM DATA WHERE OBJECT={0} AND OBJTYPE={1} AND ITEM={2} AND PARNUMBER=12 AND DATA_DATE>'{3}' AND DATA_DATE<='{4}'",
						point.Obj, point.ObjType, point.Item, dateStart.ToString(DateFormat), dateEnd.ToString(DateFormat));
					SqlCommand command = new SqlCommand(comSTR, con);
					SqlDataReader reader = command.ExecuteReader();


					List<DateTime> fillDates = new List<DateTime>();
					while (reader.Read()) {
						DateTime date = reader.GetDateTime(0);
						double val = reader.GetDouble(1);

						try {
							vals.Add(new ShadeValue() {
								period = new TimePeriod() {
									from = new Timestamp() { second = EDSClass.toTS(date.AddHours(2).AddMinutes(-30)) },
									till = new Timestamp() { second = EDSClass.toTS(date.AddHours(2)) }
								},
								quality = Quality.QUALITYGOOD,
								value = new PointValue() { av = (long)val, avSpecified = true }
							});
							fillDates.Add(date);
						} catch { }
					}

					foreach (DateTime emptyDT in dates) {
						if (!fillDates.Contains(emptyDT)) {
							try {
								vals.Add(new ShadeValue() {
									period = new TimePeriod() {
										from = new Timestamp() { second = EDSClass.toTS(emptyDT.AddHours(2).AddMinutes(-30)) },
										till = new Timestamp() { second = EDSClass.toTS(emptyDT.AddHours(2)) }
									},
									quality = Quality.QUALITYBAD,
									value = new PointValue() { av = (long)0, avSpecified = true }
								});
							} catch { }
						}
					}

					reader.Close();
					con.Close();

					if (vals.Count == 0)
						continue;


					sel.Add(new ShadeSelector() {
						period = new TimePeriod() {
							from = new Timestamp() { second = EDSClass.toTS(dateStart.AddHours(2).AddMinutes(30)) },
							till = new Timestamp() { second = EDSClass.toTS(dateEnd.AddHours(2).AddMinutes(-30)) }
						},
						pointId = new PointId() {
							iess = point.EDSPoint
						}
					});


					shades.Add(new Shade() {
						pointId = new PointId() { iess = point.EDSPoint },
						values = vals.ToArray()
					});

				}

				if (!EDSClass.Connected) {
					EDSClass.Connect();
				}

				Logger.Info(String.Format("Удаление записей по точке {0} - {1} ", dateStart.ToString("dd.MM.yyyy HH:mm"), dateEnd.ToString("dd.MM.yyyy HH:mm")));
			    uint id = EDSClass.Client.requestShadesClear(EDSClass.AuthStr, sel.ToArray());
				bool ok = EDSClass.ProcessQuery(id);
				Logger.Info(String.Format("Запись данных по точке {0} - {1} ", dateStart.ToString("dd.MM.yyyy HH:mm"), dateEnd.ToString("dd.MM.yyyy HH:mm")));
				id = EDSClass.Client.requestShadesWrite(EDSClass.AuthStr, shades.ToArray());
				ok = EDSClass.ProcessQuery(id);
			}catch (Exception e) {
				Logger.Info("ошибка обработки: " + e.ToString());
			}
		}

		public void refreshDataInEDS(DateTime dateStart, DateTime dateEnd) {

			List<DateTime> dates = new List<DateTime>();
			DateTime dt = dateStart.AddMinutes(30);
			while (dt <= dateEnd) {
				dates.Add(dt);
				dt = dt.AddMinutes(30);
			}
			try {
				foreach (AISPointInfo point in AISPoints) {
					SqlConnection con = getConnection(point.DBName);
					con.Open();
					string comSTR = "";
					comSTR = String.Format("SELECT DATA_DATE,VALUE0 FROM DATA WHERE OBJECT={0} AND OBJTYPE={1} AND ITEM={2} AND PARNUMBER=12 AND DATA_DATE>'{3}' AND DATA_DATE<='{4}'",
						point.Obj, point.ObjType, point.Item, dateStart.ToString(DateFormat), dateEnd.ToString(DateFormat));
					SqlCommand command = new SqlCommand(comSTR, con);
					SqlDataReader reader = command.ExecuteReader();


					List<DateTime> fillDates = new List<DateTime>();
					while (reader.Read()) {
						DateTime date = reader.GetDateTime(0);
						double val = reader.GetDouble(1);

						try {
							fillDates.Add(date);
						} catch { }
					}

					List<ShadeSelector> sel = new List<ShadeSelector>();
					List<Shade> shades = new List<Shade>();
					List<ShadeValue> vals = new List<ShadeValue>();
					foreach (DateTime emptyDT in dates) {
						if (!fillDates.Contains(emptyDT)) {
							try {
								

								sel.Add(new ShadeSelector() {
									period = new TimePeriod() {
										from = new Timestamp() { second = EDSClass.toTS(emptyDT.AddHours(2).AddMinutes(-30)) },
										till = new Timestamp() { second = EDSClass.toTS(emptyDT.AddHours(2)) }
									},
									pointId = new PointId() {
										iess = point.EDSPoint
									}
								});

								vals.Add(new ShadeValue() {
									period = new TimePeriod() {
										from = new Timestamp() { second = EDSClass.toTS(emptyDT.AddHours(2).AddMinutes(-30)) },
										till = new Timestamp() { second = EDSClass.toTS(emptyDT.AddHours(2)) }
									},
									quality = Quality.QUALITYBAD,
									value = new PointValue() { av = (float)0, avSpecified = true }
								});

								

							} catch (Exception e) {
								Logger.Info(e.ToString());
							}

						}
					}
									

					reader.Close();
					con.Close();
					if (vals.Count == 0)
						continue;

					try {
						shades.Add(new Shade() {
							pointId = new PointId() { iess = point.EDSPoint },
							values = vals.ToArray()
						});

						if (!EDSClass.Connected) {
							EDSClass.Connect();
						}
                        Logger.Info(String.Format("Удаление записей по точке {2} {0} - {1} ", dateStart.ToString("dd.MM.yyyy HH:mm"), dateEnd.ToString("dd.MM.yyyy HH:mm"), point.Desc));
                        uint id = EDSClass.Client.requestShadesClear(EDSClass.AuthStr, sel.ToArray());
						bool ok = EDSClass.ProcessQuery(id);
						Logger.Info(String.Format("Запись данных по точке {2} {0} - {1} ", dateStart.ToString("dd.MM.yyyy HH:mm"), dateEnd.ToString("dd.MM.yyyy HH:mm"), point.Desc));
						 id = EDSClass.Client.requestShadesWrite(EDSClass.AuthStr, shades.ToArray());
						 ok = EDSClass.ProcessQuery(id);
					}catch(Exception e) {
						Logger.Info(e.ToString());
					}

				}

				
			} catch (Exception e) {
				Logger.Info("ошибка обработки: " + e.ToString());
			}
		}

	}
}
