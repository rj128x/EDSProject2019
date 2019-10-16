using EDSProj;
using EDSProj.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings.init("Data/Settings.xml");
            Logger.InitFileLogger(Settings.Single.LogPath, "Diagnostics");
            if (args.Length >= 1)
            {
                string task = args[0];
                task = task.ToLower();
                switch (task)
                {
                    case "processpump":
                        try
                        {
                            NetworkCredential nc = new NetworkCredential("10.7.227.12\\Administrator", "Ovation1", "");
                            CredentialCache cache = new CredentialCache();
                            cache.Add(new Uri(@"\\10.7.227.12"), "Basic", nc);
                            DirectoryInfo di = new DirectoryInfo(Settings.Single.DiagFolder);
                            DirectoryInfo newDI = new DirectoryInfo(Settings.Single.DiagFolder + "/archive");
                            if (!newDI.Exists)
                                newDI.Create();
                            FileInfo[] files = di.GetFiles();
                            foreach (FileInfo fi in files)
                            {
                                try
                                {
                                    if (fi.Name.Contains("DN") || fi.Name.Contains("LN") || fi.Name.Contains("MNU"))
                                    {
                                        Logger.Info("Обработка файла " + fi.FullName);
                                        PumpFileReader pumpReader = new PumpFileReader(fi.FullName);
                                        Logger.Info("====Чтение данных");
                                        bool ok = pumpReader.ReadData();
                                        Logger.Info("======" + ok);
                                        if (ok)
                                        {
                                            Logger.Info("====Запись данных");
                                            ok = pumpReader.writeToDB(4);
                                            Logger.Info("======" + ok);
                                            if (ok)
                                            {
                                                fi.MoveTo(newDI.FullName + "/" + fi.Name);
                                            }
                                        }
                                    }
                                    else if (fi.Name.Contains("SVOD"))
                                    {
                                        Logger.Info("Обработка файла " + fi.FullName);
                                        SvodFileReader pumpReader = new SvodFileReader(fi.FullName);
                                        Logger.Info("====Чтение данных");
                                        bool ok = pumpReader.ReadData();
                                        Logger.Info("======" + ok);
                                        if (ok)
                                        {
                                            Logger.Info("====Запись данных");
                                            ok = pumpReader.writeToDB(4);
                                            Logger.Info("======" + ok);
                                            if (ok)
                                            {
                                                fi.MoveTo(newDI.FullName + "/" + fi.Name);
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.Info("Ошибка при обработке файла " + fi.FullName);
                                    Logger.Info(e.ToString());
                                }
                            }


                        }
                        catch (Exception e)
                        {
                            Logger.Info("Ошибка при обработке каталога " + e.ToString());
                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine("Ключи командной строки: \r\n import: для импорта ПБР \r\n copy: для копирования файлов на ftp");
                Console.ReadLine();
            }
        }
    }
}
