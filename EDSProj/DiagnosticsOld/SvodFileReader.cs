using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{


	public class SvodFileReader
	{
		public static string InsertIntoHeader = "INSERT INTO SvodTable (DateStart, DateEnd,GG, P_Min,P_Max,P_Avg,IsUst,LN1_Time,LN2_Time,DN1_Time,DN2_Time,MNU1_Time,MNU2_Time,MNU3_Time,LN1_Pusk,LN2_Pusk,DN1_Pusk,DN2_Pusk,MNU1_Pusk,MNU2_Pusk,MNU3_Pusk,GP_Hot,GP_Cold,GP_Level,PP_Hot,PP_Cold,PP_Level,IsUstGP,IsUstPP,GP_OhlRash,PP_OhlRash,IsUstGPOhl,IsUstPPOhl)";
		public static string InsertIntoFormat = "SELECT '{0}','{1}',{2}, {3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32}";
		public static string DateFormat = "yyyy-MM-dd HH:mm:ss";
		SortedList<DateTime, SvodDataRecord> Data = new SortedList<DateTime, SvodDataRecord>();

		public string FileName { get; set; }
		public SvodFileReader(string fileName) {
			FileName = fileName;

		}

		public static bool IsUst(double min, double max, double avg, double proc) {
			double diff = Math.Abs(avg / 100 * proc);
			bool res = Math.Abs(avg - max) <= diff;
			res = res && Math.Abs(avg - min) <= diff;
			return res;
		}

		public bool ReadData() {
			Data = new SortedList<DateTime, SvodDataRecord>();
			PumpTypeEnum type = PumpTypeEnum.Drenage;
			ReportOutputFile outFile = new ReportOutputFile(FileName);
			try {
				outFile.ReadData();
			} catch (Exception e) {
				Logger.Info("Ошибка инициализации файла " + FileName);
				Logger.Info(e.ToString());
				return false;
			}

			

			for (int i = 2; i < outFile.Data.Count; i++) {
				try {
					Dictionary<int, string> fileRec = outFile.Data[i];
					SvodDataRecord rec = new SvodDataRecord();
					rec.DateStart = ReportOutputFile.getDate(fileRec[0]);
					rec.DateEnd = ReportOutputFile.getDate(fileRec[1]);
					rec.PMin = ReportOutputFile.getDouble(fileRec[2]);
					rec.PMax = ReportOutputFile.getDouble(fileRec[3]);
					rec.PAvg = ReportOutputFile.getDouble(fileRec[4]);

					rec.LN1Time = ReportOutputFile.getDouble(fileRec[5]);
					rec.LN2Time = ReportOutputFile.getDouble(fileRec[6]);
					rec.DN1Time = ReportOutputFile.getDouble(fileRec[7]);
					rec.DN2Time = ReportOutputFile.getDouble(fileRec[8]);
					rec.MNU1Time = ReportOutputFile.getDouble(fileRec[9]);
					rec.MNU2Time = ReportOutputFile.getDouble(fileRec[10]);
					rec.MNU3Time = ReportOutputFile.getDouble(fileRec[11]);

					rec.LN1Pusk = (int)ReportOutputFile.getDouble(fileRec[12]);
					rec.LN2Pusk = (int)ReportOutputFile.getDouble(fileRec[13]);
					rec.DN1Pusk = (int)ReportOutputFile.getDouble(fileRec[14]);
					rec.DN2Pusk = (int)ReportOutputFile.getDouble(fileRec[15]);
					rec.MNU1Pusk = (int)ReportOutputFile.getDouble(fileRec[16]);
					rec.MNU2Pusk = (int)ReportOutputFile.getDouble(fileRec[17]);
					rec.MNU3Pusk = (int)ReportOutputFile.getDouble(fileRec[18]);

					rec.GPHot = ReportOutputFile.getDouble(fileRec[19]);
					rec.GPCold = ReportOutputFile.getDouble(fileRec[20]);
					rec.GPLevel = ReportOutputFile.getDouble(fileRec[21]);
				

					rec.PPHot = ReportOutputFile.getDouble(fileRec[22]);
					rec.PPCold = ReportOutputFile.getDouble(fileRec[23]);
					rec.PPLevel = ReportOutputFile.getDouble(fileRec[24]);

					double GPLevelMin= ReportOutputFile.getDouble(fileRec[25]);
					double PPLevelMin = ReportOutputFile.getDouble(fileRec[26]);
					double GPLevelMax = ReportOutputFile.getDouble(fileRec[27]);
					double PPLevelMax = ReportOutputFile.getDouble(fileRec[28]);

					double gpHotMin= ReportOutputFile.getDouble(fileRec[29]);
					double gpColdMin = ReportOutputFile.getDouble(fileRec[30]);
					double gpHotMax = ReportOutputFile.getDouble(fileRec[31]);					
					double gpColdMax = ReportOutputFile.getDouble(fileRec[32]);

					double ppHotMin = ReportOutputFile.getDouble(fileRec[33]);
					double ppColdMin = ReportOutputFile.getDouble(fileRec[34]);
					double ppHotMax = ReportOutputFile.getDouble(fileRec[35]);
					double ppColdMax = ReportOutputFile.getDouble(fileRec[36]);

					double gpOhlAvg= ReportOutputFile.getDouble(fileRec[37]);
					double gpOhlMin = ReportOutputFile.getDouble(fileRec[38]);
					double gpOhlMax = ReportOutputFile.getDouble(fileRec[39]);

					double ppOhlAvg1 = ReportOutputFile.getDouble(fileRec[40]);
					double ppOhlMin1 = ReportOutputFile.getDouble(fileRec[41]);
					double ppOhlMax1 = ReportOutputFile.getDouble(fileRec[42]);

					double ppOhlAvg2 = ReportOutputFile.getDouble(fileRec[43]);
					double ppOhlMin2 = ReportOutputFile.getDouble(fileRec[44]);
					double ppOhlMax2 = ReportOutputFile.getDouble(fileRec[45]);

					
					rec.GPOhlRashod = gpOhlAvg;
					rec.PPOhlRashod = ppOhlAvg1 + ppOhlAvg2;


					rec.IsUst=IsUst(rec.PMin, rec.PMax, rec.PAvg, 2);

					rec.IsUstGP = IsUst(GPLevelMin, GPLevelMax, rec.GPLevel,3);
					rec.IsUstGP = rec.IsUstGP&&IsUst(gpHotMin, gpHotMax, rec.GPHot, 3);
					rec.IsUstGP = rec.IsUstGP && IsUst(gpColdMin, gpColdMax, rec.GPCold, 3);

					rec.IsUstPP = IsUst(PPLevelMin, PPLevelMax, rec.PPLevel, 3);
					rec.IsUstPP = rec.IsUstPP && IsUst(ppHotMin, ppHotMax, rec.PPHot, 3);
					rec.IsUstPP = rec.IsUstPP && IsUst(ppColdMin, ppColdMax, rec.PPCold, 3);

					rec.IsUstGPOhl = IsUst(gpOhlMin, gpOhlMax, gpOhlAvg,5);
					rec.IsUstPPOhl = IsUst(ppOhlMin1, ppOhlMax1, ppOhlAvg1, 5) && IsUst(ppOhlMin2, ppOhlMax2, ppOhlAvg2, 5);
					

					while (Data.ContainsKey(rec.DateStart))
						rec.DateStart = rec.DateStart.AddMilliseconds(1);
					Data.Add(rec.DateStart, rec);
				} catch (Exception e) {
					Logger.Info("ошибка при разборе строки");
					Logger.Info(e.ToString());
					return false;
				}

			}
			return true;
		}

		public bool writeToDB(int gg) {
			if (Data.Count == 0)
				return true;
			SqlConnection con = ReportOutputFile.getConnection();
			try {
				List<string> insQueries = new List<string>();

				string delQ = String.Format("delete FROM SvodTable where  DateStart>='{0}' and DateStart<='{1}' and GG={2}",
						Data.Keys.Min().ToString(DateFormat), Data.Keys.Max().ToString(DateFormat), gg);
				
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					string ins = string.Format(InsertIntoFormat, de.Value.DateStart.ToString(DateFormat), de.Value.DateEnd.ToString(DateFormat), gg,
						de.Value.PMin.ToString().Replace(",", "."),
						de.Value.PMax.ToString().Replace(",", "."),
						de.Value.PAvg.ToString().Replace(",", "."),					
						de.Value.IsUst ? 1 : 0,
						de.Value.LN1Time.ToString().Replace(",", "."),
						de.Value.LN2Time.ToString().Replace(",", "."),
						de.Value.DN1Time.ToString().Replace(",", "."),
						de.Value.DN2Time.ToString().Replace(",", "."),
						de.Value.MNU1Time.ToString().Replace(",", "."),
						de.Value.MNU2Time.ToString().Replace(",", "."),
						de.Value.MNU3Time.ToString().Replace(",", "."),
						de.Value.LN1Pusk.ToString().Replace(",", "."),
						de.Value.LN2Pusk.ToString().Replace(",", "."),
						de.Value.DN1Pusk.ToString().Replace(",", "."),
						de.Value.DN2Pusk.ToString().Replace(",", "."),
						de.Value.MNU1Pusk.ToString().Replace(",", "."),
						de.Value.MNU2Pusk.ToString().Replace(",", "."),
						de.Value.MNU3Pusk.ToString().Replace(",", "."),
						de.Value.GPHot.ToString().Replace(",", "."),
						de.Value.GPCold.ToString().Replace(",", "."),
						de.Value.GPLevel.ToString().Replace(",", "."),
						de.Value.PPHot.ToString().Replace(",", "."),
						de.Value.PPCold.ToString().Replace(",", "."),
						de.Value.PPLevel.ToString().Replace(",", "."),
						de.Value.IsUstGP ? 1 : 0,
						de.Value.IsUstPP ? 1 : 0,
						de.Value.GPOhlRashod.ToString().Replace(",", "."),
						de.Value.PPOhlRashod.ToString().Replace(",", "."),
						de.Value.IsUstGPOhl ? 1 : 0,
						de.Value.IsUstPPOhl ? 1 : 0
					);
					insQueries.Add(ins);
				}

				SqlTransaction trans = con.BeginTransaction();
				SqlCommand com = con.CreateCommand();
				com.CommandText = delQ;
				com.Transaction = trans;
				com.ExecuteNonQuery();

				string insertStr = String.Format("{0} {1}", InsertIntoHeader, String.Join("\nUNION ALL\n", insQueries));

				//Logger.Info(insertStr);

				SqlCommand comIns = con.CreateCommand();
				comIns.CommandText = insertStr;
				comIns.Transaction = trans;
				comIns.ExecuteNonQuery();

				trans.Commit();
				con.Close();
				return true;
			} catch (Exception e) {
				Logger.Info(e.ToString());
				return false;
			} finally {
				try {
					con.Close();
				} catch { }
			}
		}
	}
}
