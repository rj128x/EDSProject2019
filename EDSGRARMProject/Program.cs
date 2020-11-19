using EDSProj;
using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSGRARMProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger(Settings.Single.LogPath, "pbrExport");

            string folderName = @"C:\EDSPBRReports";
            DirectoryInfo di = new DirectoryInfo(folderName);
            FileInfo[] filesArr = di.GetFiles();
            SortedList<DateTime, FileInfo> files = new SortedList<DateTime, FileInfo>();
            foreach (FileInfo fi in filesArr)
            {
                files.Add(fi.CreationTime, fi);
            }
            foreach (FileInfo fi in files.Values)
            {
                GRARMPBRData PBR_gtp1 = new GRARMPBRData("PBR_GTP1.EDS@CALC", "GRARM_PBR_GTP1_SMOOTH.EDS@CALC", false);
                GRARMPBRData PBR_gtp2 = new GRARMPBRData("PBR_GTP2.EDS@CALC", "GRARM_PBR_GTP2_SMOOTH.EDS@CALC", false);
                GRARMPBRData PBR_ges = new GRARMPBRData("PBR_GES.EDS@CALC", "GRARM_PBR_GES_SMOOTH.EDS@CALC", true);
                PBR_ges.IntegratedEDSPoint = "GRARM_PBR_GES_VYR_PLAN.EDS@CALC";
                StreamReader sr = new StreamReader(fi.FullName);
                String text = sr.ReadToEnd();
                string[] lines = text.Split(new char[] { '\n' });
                foreach (string line in lines)
                {
                    try
                    {
                        string[] vals = line.Split(';');
                        DateTime dt = DateTime.Parse(vals[0]);
                        double valGTP1 = double.Parse(vals[2]);
                        double valGTP2 = double.Parse(vals[4]);
                        double valGES = double.Parse(vals[6]);
                        PBR_ges.AddValue(dt, valGES);
                        PBR_gtp1.AddValue(dt, valGTP1);
                        PBR_gtp2.AddValue(dt, valGTP2);
                        Console.WriteLine(String.Format("{0} {1} {2} {3}", dt, valGTP1, valGTP2, valGES));
                    }
                    catch
                    {
                        Logger.Info("Ошибка в формате");
                    }
                }
                bool ok = true;
                PBR_ges.CreateData();
                ok = ok && PBR_ges.ProcessData();

                PBR_gtp1.CreateData();
                ok = ok && PBR_gtp1.ProcessData();

                PBR_gtp2.CreateData();
                ok = ok && PBR_gtp2.ProcessData();

                if (ok)
                {

                    Logger.Info("Запись номера ПБР");

                    DateTime ds = DateTime.Now;
                    DateTime de = ds.Date.AddDays(1);
                    List<ShadeSelector> sel = new List<ShadeSelector>();
                    sel.Add(new ShadeSelector()
                    {
                        period = new TimePeriod()
                        {
                            from = new Timestamp() { second = EDSClass.toTS(ds) },
                            till = new Timestamp() { second = EDSClass.toTS(de) }
                        },
                        pointId = new PointId()
                        {
                            iess = "GRARM_PBR_IND.EDS@CALC"
                        }
                    });

                    Logger.Info("Удаление номера ПБР");
                    uint id = EDSClass.Client.requestShadesClear(EDSClass.AuthStr, sel.ToArray());
                    ok = EDSClass.ProcessQuery(id);

                    string dt = ds.ToString("HHmm");
                    long NPBR = long.Parse(dt);
                    if (ok)
                    {
                        List<Shade> shades = new List<Shade>();
                        List<ShadeValue> vals = new List<ShadeValue>();
                        vals.Add(new ShadeValue()
                        {
                            period = new TimePeriod()
                            {
                                from = new Timestamp() { second = EDSClass.toTS(ds) },
                                till = new Timestamp() { second = EDSClass.toTS(de) }
                            },
                            quality = Quality.QUALITYGOOD,
                            value = new PointValue() { av = NPBR, avSpecified = true }
                        });
                        shades.Add(new Shade()
                        {
                            pointId = new PointId() { iess = "GRARM_PBR_IND.EDS@CALC" },
                            values = vals.ToArray()
                        });
                        Logger.Info("Запись данных");
                        id = EDSClass.Client.requestShadesWrite(EDSClass.AuthStr, shades.ToArray());
                        ok = EDSClass.ProcessQuery(id);
                    }
                }


                sr.Close();
                fi.Delete();

            }
        }
    }
}
