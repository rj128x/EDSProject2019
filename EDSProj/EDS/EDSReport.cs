using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.EDS
{

	public class EDSReportRequestRecord
	{
		public EDSPointInfo Point { get; set; }
		public EDSReportFunction Function { get; set; }
		public string Id { get; }
		public string Desc { get; }

		public EDSReportRequestRecord(EDSPointInfo point, EDSReportFunction func) {
			this.Point = point;
			this.Function = func;
			this.Id = String.Format("{0} [{1}]", Point.IESS, Function);
			this.Desc = String.Format("{0} [{1}]", Point.Desc, EDSClass.ReportFunctions[func]);
		}

	}

	public delegate void EDSFinish();

	public class EDSReport
	{
		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public bool MSK { get; set; }
		public EDSReportPeriod Period { get; set; }
		public Dictionary<string, EDSReportRequestRecord> RequestData = new Dictionary<string, EDSReportRequestRecord>();
		public Dictionary<DateTime, Dictionary<string, double>> ResultData { get; set; }
		public EDSFinish FinishRead;

		public EDSReportRequestRecord addRequestField(EDSPointInfo point, EDSReportFunction func) {
			try {
				EDSReportRequestRecord rec = new EDSReportRequestRecord(point, func);
				RequestData.Add(rec.Id, rec);
                return rec;
			} catch (Exception e) {
				Logger.Info(e.ToString());
                return null;
			}
		}

		public EDSReport(DateTime dateStart, DateTime dateEnd, EDSReportPeriod period, bool MSK = false) {
			DateStart = dateStart;
			DateEnd = dateEnd;
			Period = period;
			this.MSK = MSK;
			if (MSK) {
				DateStart = DateStart.AddHours(2);
				DateEnd = DateEnd.AddHours(2);
			}
		}

		public async Task<bool> ReadData() {
			ResultData = new Dictionary<DateTime, Dictionary<string, double>>();
			DateTime date = DateStart.AddHours(0);			
			while (date < DateEnd) {
				DateTime de = date.AddHours(0);
				switch (Period) {
					case EDSReportPeriod.sec:
						de = date.AddSeconds(1);
						break;
					case EDSReportPeriod.minute:
						de = date.AddMinutes(1);
						break;
					case EDSReportPeriod.hour:
						de = date.AddHours(1);
						break;
					case EDSReportPeriod.day:
						de = date.AddDays(1);
						break;
					case EDSReportPeriod.month:
						de = date.AddMonths(1);
						break;
				}
				ResultData.Add(date, new Dictionary<string, double>());
				foreach (string id in RequestData.Keys) {
					ResultData[date].Add(id, 0);
				}
				date = de.AddHours(0);
			}


			List<TabularRequestItem> list = new List<TabularRequestItem>();
			foreach (EDSReportRequestRecord rec in RequestData.Values) {
				list.Add(new TabularRequestItem() {
					function = EDSClass.getReportFunctionName(rec.Function),
					pointId = new PointId() { iess = rec.Point.IESS },
					shadePriority = rec.Point.IsShade ? ShadePriority.SHADEOVERREGULAR : ShadePriority.REGULAROVERSHADE
				});
			}



			if (!EDSClass.Connected)
				EDSClass.Connect();
			if (EDSClass.Connected) {
				if (Period != EDSReportPeriod.month) {
					List<DateTime> dates = ResultData.Keys.ToList();
					DateTime ds = DateStart.AddHours(0);
					DateTime de = DateStart.AddHours(1);
					while (ds < DateEnd) {
						if (Period == EDSReportPeriod.minute || Period == EDSReportPeriod.sec) {
							int i0 = dates.IndexOf(ds);
							int i1 = i0 + 15000;
							de = i1 < dates.Count ? dates[i1] : DateEnd;
						} else {
							de = de.AddDays(5);
							try {
								de = dates.First(d => d >= de);
							} catch {
								de = DateEnd;
							}
						}

						EDSClass.Single.GlobalInfo = String.Format("{0}-{1}", ds.ToString("dd.MM.yyyy HH:mm:ss"), de.ToString("dd.MM.yyyy HH:mm:ss"));

						TabularRequest req = new TabularRequest();
						req.period = new TimePeriod() {
							from = new Timestamp() { second = EDSClass.toTS(ds) },
							till = new Timestamp() { second = EDSClass.toTS(de) }
						};

						req.step = new TimeDuration() { seconds = EDSClass.getPeriodSeconds(Period) };
						req.items = list.ToArray();
						uint id = EDSClass.Client.requestTabular(EDSClass.AuthStr, req);
						TabularRow[] rows;
						bool ok = await EDSClass.ProcessQueryAsync(id);
						if (!ok)
							break;
						PointId[] points = EDSClass.Client.getTabular(EDSClass.AuthStr, id, out rows);
						List<string> keys = RequestData.Keys.ToList();

						foreach (TabularRow row in rows) {
							DateTime dt = EDSClass.fromTS(row.ts.second);
							for (int i = 0; i < row.values.Count(); i++) {
								double val = EDSClass.getVal(row.values[i].value);
								PointId point = points[i];
								string resId = keys[i];
								EDSReportRequestRecord request = RequestData[resId];
								if (request.Function == EDSReportFunction.vyrab && Period == EDSReportPeriod.day) {
									val *= 24;
								}
								ResultData[dt][resId] = val;
							}
						}

						ds = de.AddHours(0);
					}
				} else {
					DateTime ds = DateStart.AddHours(0);
					while (ds < DateEnd) {
                        EDSClass.Single.GlobalInfo = String.Format("{0}-{1}", ds.ToString("dd.MM.yyyy"), ds.AddMonths(1).ToString("dd.MM.yyyy"));
                        DateTime de = ds.AddMonths(1);
						TabularRequest req = new TabularRequest();
						req.period = new TimePeriod() {
							from = new Timestamp() { second = EDSClass.toTS(ds) },
							till = new Timestamp() { second = EDSClass.toTS(de) }
						};


						int seconds = (int)(EDSClass.toTS(de) - EDSClass.toTS(ds));
						req.step = new TimeDuration() { seconds = seconds };
						req.items = list.ToArray();
						uint id = EDSClass.Client.requestTabular(EDSClass.AuthStr, req);
						TabularRow[] rows;
						bool ok = await EDSClass.ProcessQueryAsync(id);
						if (!ok)
							break;
						PointId[] points = EDSClass.Client.getTabular(EDSClass.AuthStr, id, out rows);
						List<string> keys = RequestData.Keys.ToList();

						TabularRow row = rows.First();
						DateTime dt = EDSClass.fromTS(row.ts.second);
						for (int i = 0; i < row.values.Count(); i++) {
							double val = EDSClass.getVal(row.values[i].value);
							PointId point = points[i];
							string resId = keys[i];
							EDSReportRequestRecord request = RequestData[resId];
							if (request.Function == EDSReportFunction.vyrab) {
								val *= seconds / 3600.0;
							}
							ResultData[dt][resId] = val;
						}

						ds = de.AddHours(0);
					}
				}
			}

			return true;
		}



	}
}
