using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{
	public enum PumpTypeEnum { Drenage, Leakage, MNU }

	public class PumpDataRecord
	{

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public PumpTypeEnum PumpType { get; set; }
		public int PumpNum { get; set; }
		public double RunTime { get; set; }
		public double LevelStart { get; set; }
		public double LevelStop { get; set; }
		public double PAvg { get; set; }
		public double PMin { get; set; }
		public double PMax { get; set; }
		public bool IsUst { get; set; }
	}

	public class SvodDataRecord
	{

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public double PAvg { get; set; }
		public double PMin { get; set; }
		public double PMax { get; set; }
		public bool IsUst { get; set; }
		public bool IsUstGP { get; set; }
		public bool IsUstPP { get; set; }
		public bool IsUstGPOhl { get; set; }
		public bool IsUstPPOhl { get; set; }


		public double LN1Time { get; set; }
		public double LN2Time { get; set; }
		public double DN1Time { get; set; }
		public double DN2Time { get; set; }
		public double MNU1Time { get; set; }
		public double MNU2Time { get; set; }
		public double MNU3Time { get; set; }

		public int LN1Pusk { get; set; }
		public int LN2Pusk { get; set; }
		public int DN1Pusk { get; set; }
		public int DN2Pusk { get; set; }
		public int MNU1Pusk { get; set; }
		public int MNU2Pusk { get; set; }
		public int MNU3Pusk { get; set; }

		public double GPHot { get; set; }
		public double GPCold { get; set; }
		public double GPLevel { get; set; }

		public double PPHot { get; set; }
		public double PPCold { get; set; }
		public double PPLevel { get; set; }

		public double GPOhlRashod { get; set; }
		public double PPOhlRashod { get; set; }



	}



	public class PumpDiagnostics
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public static string DateFormat = "yyyy-MM-dd HH:mm:ss";

		public PumpDiagnostics(DateTime dateStart, DateTime dateEnd) {
			this.StartDate = dateStart;
			this.EndDate = dateEnd;
		}

		public SortedList<DateTime, PumpDataRecord> ReadDataPump(PumpTypeEnum type, int powerStart, int powerStop) {
			SqlConnection con = ReportOutputFile.getConnection();

			string query = String.Format("Select * from pumpTable where dateStart>='{0}' and dateEnd<='{1}' and isUst=1 and pAvg>={2} and pAvg<={3} and PumpType='{4}' order by dateStart",
				StartDate.ToString(DateFormat), EndDate.ToString(DateFormat), powerStart, powerStop, type.ToString());

			SqlCommand com = con.CreateCommand();
			com.CommandText = query;

			SortedList<DateTime, PumpDataRecord> Data = new SortedList<DateTime, PumpDataRecord>();
			SqlDataReader reader = com.ExecuteReader();
			while (reader.Read()) {
				PumpDataRecord rec = new PumpDataRecord();

				rec.DateStart = reader.GetDateTime(reader.GetOrdinal("DateStart"));
				rec.DateEnd = reader.GetDateTime(reader.GetOrdinal("DateEnd"));
				rec.IsUst = reader.GetBoolean(reader.GetOrdinal("IsUst"));
				rec.PumpNum = reader.GetInt32(reader.GetOrdinal("PumpNum"));
				rec.RunTime = reader.GetDouble(reader.GetOrdinal("RunTime"));
				rec.LevelStart = reader.GetDouble(reader.GetOrdinal("LevelStart"));
				rec.LevelStop = reader.GetDouble(reader.GetOrdinal("LevelStop"));
				rec.PAvg = reader.GetDouble(reader.GetOrdinal("PAvg"));
				rec.PMin = reader.GetDouble(reader.GetOrdinal("PMin"));
				rec.PMax = reader.GetDouble(reader.GetOrdinal("PMax"));
				string tp = reader.GetString(reader.GetOrdinal("PumpType"));
				rec.PumpType = (PumpTypeEnum)Enum.Parse(typeof(PumpTypeEnum), tp);
				Data.Add(rec.DateStart, rec);

			}
			return Data;
		}

		public Dictionary<DateTime, SvodDataRecord> ReadSvod(string groupName, double start, double stop, string IsUstGroup) {
			SqlConnection con = ReportOutputFile.getConnection();
			string query = String.Format("Select * from svodTable where dateStart>='{0}' and dateEnd<='{1}' and {2}>={3} and {2}<={4} and {5}>0 order by dateStart",
				StartDate.ToString(DateFormat), EndDate.ToString(DateFormat), groupName, start.ToString().Replace(",", "."), stop.ToString().Replace(",", "."), IsUstGroup);

			SqlCommand com = con.CreateCommand();
			com.CommandText = query;

			Dictionary<DateTime, SvodDataRecord> Data = new Dictionary<DateTime, SvodDataRecord>();
			SqlDataReader reader = com.ExecuteReader();
			while (reader.Read()) {
				SvodDataRecord rec = new SvodDataRecord();
				rec.DateStart = reader.GetDateTime(reader.GetOrdinal("DateStart"));
				rec.DateEnd = reader.GetDateTime(reader.GetOrdinal("DateEnd"));
				rec.IsUst = reader.GetBoolean(reader.GetOrdinal("IsUst"));
				rec.IsUstGP = reader.GetBoolean(reader.GetOrdinal("IsUstGP"));
				rec.IsUstPP = reader.GetBoolean(reader.GetOrdinal("IsUstPP"));
				rec.IsUstGPOhl = reader.GetBoolean(reader.GetOrdinal("IsUstGPOhl"));
				rec.IsUstPPOhl = reader.GetBoolean(reader.GetOrdinal("IsUstPPOhl"));

				rec.PAvg = reader.GetDouble(reader.GetOrdinal("P_Avg"));
				rec.PMin = reader.GetDouble(reader.GetOrdinal("P_Min"));
				rec.PMax = reader.GetDouble(reader.GetOrdinal("P_Max"));

				rec.LN1Time = reader.GetDouble(reader.GetOrdinal("LN1_Time"));
				rec.LN2Time = reader.GetDouble(reader.GetOrdinal("LN2_Time"));
				rec.DN1Time = reader.GetDouble(reader.GetOrdinal("DN1_Time"));
				rec.DN2Time = reader.GetDouble(reader.GetOrdinal("DN2_Time"));
				rec.MNU1Time = reader.GetDouble(reader.GetOrdinal("MNU1_Time"));
				rec.MNU2Time = reader.GetDouble(reader.GetOrdinal("MNU2_Time"));
				rec.MNU3Time = reader.GetDouble(reader.GetOrdinal("MNU3_Time"));

				rec.LN1Pusk = reader.GetInt32(reader.GetOrdinal("LN1_Pusk"));
				rec.LN2Pusk = reader.GetInt32(reader.GetOrdinal("LN2_Pusk"));
				rec.DN1Pusk = reader.GetInt32(reader.GetOrdinal("DN1_Pusk"));
				rec.DN2Pusk = reader.GetInt32(reader.GetOrdinal("DN2_Pusk"));
				rec.MNU1Pusk = reader.GetInt32(reader.GetOrdinal("MNU1_Pusk"));
				rec.MNU2Pusk = reader.GetInt32(reader.GetOrdinal("MNU2_Pusk"));
				rec.MNU3Pusk = reader.GetInt32(reader.GetOrdinal("MNU3_Pusk"));

				rec.GPHot = reader.GetDouble(reader.GetOrdinal("GP_Hot"));
				rec.GPCold = reader.GetDouble(reader.GetOrdinal("GP_Cold"));
				rec.GPLevel = reader.GetDouble(reader.GetOrdinal("GP_Level"));

				rec.PPHot = reader.GetDouble(reader.GetOrdinal("PP_Hot"));
				rec.PPCold = reader.GetDouble(reader.GetOrdinal("PP_Cold"));
				rec.PPLevel = reader.GetDouble(reader.GetOrdinal("PP_Level"));

				rec.GPOhlRashod= reader.GetDouble(reader.GetOrdinal("GP_OhlRash"));
				rec.PPOhlRashod = reader.GetDouble(reader.GetOrdinal("PP_OhlRash"));


				Data.Add(rec.DateStart, rec);
			}
			return Data;
		}

		public SortedList<DateTime, SvodDataRecord> ReadPumpSvod(string groupName, double start, double stop) {
			SqlConnection con = ReportOutputFile.getConnection();
			string query = String.Format(
@"select  
	format(datestart, 'dd.MM.yyyy') as dt,
	min(dateStart) as mindate,
	sum(ln1_time) as ln1Time,
	sum(ln2_time) as ln2Time,
	sum(dn1_time) as dn1Time,
	sum(dn2_time) as dn2Time,
	sum(mnu1_time) as mnu1Time,
	sum(mnu2_time) as mnu2Time,
	sum(mnu3_time) as mnu3Time,

	sum(ln1_pusk) as ln1Pusk,
	sum(ln2_pusk) as ln2Pusk,
	sum(dn1_pusk) as dn1Pusk,
	sum(dn2_pusk) as dn2Pusk,
	sum(mnu1_pusk) as mnu1Pusk,
	sum(mnu2_pusk) as mnu2Pusk,
	sum(mnu3_pusk) as mnu3Pusk	
from SvodTable
where dateStart>='{0}' and dateEnd<='{1}'  and {2}>={3} and {2}<={4}
group by format(datestart, 'dd.MM.yyyy')
order by mindate",
				StartDate.ToString(DateFormat), EndDate.ToString(DateFormat), groupName, start.ToString().Replace(",", "."), stop.ToString().Replace(",", "."));

			SqlCommand com = con.CreateCommand();
			com.CommandText = query;

			SortedList<DateTime, SvodDataRecord> Data = new SortedList<DateTime, SvodDataRecord>();
			SqlDataReader reader = com.ExecuteReader();
			while (reader.Read()) {
				SvodDataRecord rec = new SvodDataRecord();
				rec.DateStart = reader.GetDateTime(reader.GetOrdinal("mindate"));
				rec.DateEnd = rec.DateStart.AddDays(1);
				rec.IsUst = true;

				rec.LN1Time = reader.GetDouble(reader.GetOrdinal("ln1Time"));
				rec.LN2Time = reader.GetDouble(reader.GetOrdinal("ln2Time"));
				rec.DN1Time = reader.GetDouble(reader.GetOrdinal("dn1Time"));
				rec.DN2Time = reader.GetDouble(reader.GetOrdinal("dn2Time"));
				rec.MNU1Time = reader.GetDouble(reader.GetOrdinal("mnu1Time"));
				rec.MNU2Time = reader.GetDouble(reader.GetOrdinal("mnu2Time"));
				rec.MNU3Time = reader.GetDouble(reader.GetOrdinal("mnu3Time"));

				rec.LN1Pusk = reader.GetInt32(reader.GetOrdinal("ln1Pusk"));
				rec.LN2Pusk = reader.GetInt32(reader.GetOrdinal("ln2Pusk"));
				rec.DN1Pusk = reader.GetInt32(reader.GetOrdinal("dn1Pusk"));
				rec.DN2Pusk = reader.GetInt32(reader.GetOrdinal("dn2Pusk"));
				rec.MNU1Pusk = reader.GetInt32(reader.GetOrdinal("mnu1Pusk"));
				rec.MNU2Pusk = reader.GetInt32(reader.GetOrdinal("mnu2Pusk"));
				rec.MNU3Pusk = reader.GetInt32(reader.GetOrdinal("mnu3Pusk"));
				Data.Add(rec.DateStart, rec);
			}
			return Data;
		}

		public SortedList<DateTime, double> ReadGGRun() {
			SqlConnection con = ReportOutputFile.getConnection();
			string query = String.Format(
@"select  
	format(datestart, 'dd.MM.yyyy') as dt,
	min(dateStart) as mindate,
	count(p_avg) as timeRun
from SvodTable
where dateStart>='{0}' and dateEnd<='{1}'  and p_avg>10
group by format(datestart, 'dd.MM.yyyy')
order by mindate",
				StartDate.ToString(DateFormat), EndDate.ToString(DateFormat));

			SqlCommand com = con.CreateCommand();
			com.CommandText = query;

			SortedList<DateTime, double> Data = new SortedList<DateTime, double>();
			SqlDataReader reader = com.ExecuteReader();
			while (reader.Read()) {
				DateTime dateStart = reader.GetDateTime(reader.GetOrdinal("mindate"));

				double run = reader.GetInt32(reader.GetOrdinal("timeRun"));

				Data.Add(dateStart, run);
			}
			return Data;
		}

		public SortedList<DateTime, double> Approx(SortedList<DateTime, double> input) {
			SortedList<DateTime, double> result = new SortedList<DateTime, double>();
			SortedList<double, double> inputDouble = new SortedList<double, double>();
			SortedList<double, double> resultDouble = new SortedList<double, double>();
			SortedList<DateTime, double> dateKeys = new SortedList<DateTime, double>();

			DateTime first = input.Keys.First();
			foreach (DateTime date in input.Keys) {
				double sec = (date - first).TotalSeconds;
				dateKeys.Add(date, sec);
				inputDouble.Add(sec, input[date]);
				result.Add(date, 0);
				resultDouble.Add(sec, 0);
			}

			double sumXY = 0;
			double sumX = 0;
			double sumY = 0;
			double sumX2 = 0;

			foreach (KeyValuePair<double, double> de in inputDouble) {
				sumX += de.Key;
				sumY += de.Value;
				sumX2 += de.Key * de.Key;
				sumXY += de.Key * de.Value;
			}

			int n = input.Count;

			double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
			double b = (sumY - a * sumX) / n;

			foreach (KeyValuePair<DateTime, double> de in input) {
				double x = dateKeys[de.Key];
				double val = a * x + b;
				result[de.Key] = val;
			}


			return result;
		}
	}
}
