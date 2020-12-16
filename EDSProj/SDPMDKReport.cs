using DocumentFormat.OpenXml.Drawing.Charts;
using EDSProj.EDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{

    public class SDPMDKRecord
    {
        public DateTime DKTrigger { get; set; }
        public DateTime DKTime { get; set; }
        public DateTime DKStartTime { get; set; }
        public DateTime DKEndTime { get; set; }

        public int DKNum { get; set; }
        public double DKVal { get; set; }
        public string GOU { get; set; }
        public string DK { get; set; }

        public bool AutoDK { get; set; }
    }
    public class SDPMDKReport
    {
        protected static Dictionary<string, EDSPointInfo> Points1948New;
        protected static Dictionary<string, EDSPointInfo> Points1045New;
        protected static Dictionary<string, EDSPointInfo> Points1948Accept;
        protected static Dictionary<string, EDSPointInfo> Points1045Accept;
        protected static SortedList<String, EDSPointInfo> AllPoints;

        public Dictionary<DateTime, SDPMDKRecord> DKDataGOU1045New { get; set; }
        public Dictionary<DateTime, SDPMDKRecord> DKDataGOU1948New { get; set; }

        public Dictionary<DateTime, SDPMDKRecord> DKDataGOU1045Accept { get; set; }
        public Dictionary<DateTime, SDPMDKRecord> DKDataGOU1948Accept { get; set; }
        public SortedList<DateTime, SDPMDKRecord> DKDataFull { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public static void init(SortedList<String, EDSPointInfo> allPoints)
        {
            AllPoints = allPoints;
            Points1948New = new Dictionary<string, EDSPointInfo>();
            Points1045New = new Dictionary<string, EDSPointInfo>();

            Points1948New.Add("DKCRC", AllPoints["11VT_SPDG00A_1948-281.MCR@GRARM"]);
            //Points1948New.Add("DKID", AllPoints["11VT_SPDG00A_1948-036.MCR@GRARM"]);//+

            Points1948New.Add("DKDay", AllPoints["11VT_SPDG00A_1948-105.MCR@GRARM"]);//+
            Points1948New.Add("DKMonth", AllPoints["11VT_SPDG00A_1948-106.MCR@GRARM"]);//+
            Points1948New.Add("DKYear", AllPoints["11VT_SPDG00A_1948-107.MCR@GRARM"]);//+
            Points1948New.Add("DKStartDay", AllPoints["11VT_SPDG00A_1948-108.MCR@GRARM"]);//+
            Points1948New.Add("DKStartMonth", AllPoints["11VT_SPDG00A_1948-109.MCR@GRARM"]);//+
            Points1948New.Add("DKStartYear", AllPoints["11VT_SPDG00A_1948-110.MCR@GRARM"]);//+
            Points1948New.Add("DKEndDay", AllPoints["11VT_SPDG00A_1948-111.MCR@GRARM"]);//+
            Points1948New.Add("DKEndMonth", AllPoints["11VT_SPDG00A_1948-112.MCR@GRARM"]);//+
            Points1948New.Add("DKEndYear", AllPoints["11VT_SPDG00A_1948-113.MCR@GRARM"]);//+

            Points1948New.Add("DKHour", AllPoints["11VT_SPDG00A_1948-079.MCR@GRARM"]);//+
            Points1948New.Add("DKMin", AllPoints["11VT_SPDG00A_1948-080.MCR@GRARM"]);//+
            Points1948New.Add("DKStartHour", AllPoints["11VT_SPDG00A_1948-083.MCR@GRARM"]);//+
            Points1948New.Add("DKStartMin", AllPoints["11VT_SPDG00A_1948-084.MCR@GRARM"]);//+
            Points1948New.Add("DKEndHour", AllPoints["11VT_SPDG00A_1948-087.MCR@GRARM"]);//+
            Points1948New.Add("DKEndMin", AllPoints["11VT_SPDG00A_1948-088.MCR@GRARM"]);//+

            Points1948New.Add("DKVal", AllPoints["11VT_SPDG00A_1948-276.MCR@GRARM"]);//+
            Points1948New.Add("DKNum", AllPoints["11VT_SPDG00A_1948-275.MCR@GRARM"]);//+

            //Points1948New.Add("CRCChange", AllPoints["11VT_SPDG00D_1948-041.MCR@GRARM"]);//+

            foreach (KeyValuePair<string, EDSPointInfo> de in Points1948New)
            {
                Points1045New.Add(de.Key, AllPoints[de.Value.IESS.Replace("1948", "1045")]);
            }

            Points1948Accept = new Dictionary<string, EDSPointInfo>();
            Points1045Accept = new Dictionary<string, EDSPointInfo>();


            //Points1948Accept.Add("DKCRC", AllPoints["11VT_SPDG00A_1948-077.MCR@GRARM"]);//Дата время действ
            Points1948Accept.Add("DKIDMan", AllPoints["11VT_SPDG00A_1948-350.MCR@GRARM"]);
            Points1948Accept.Add("DKIDAuto", AllPoints["11VT_SPDG00A_1948-120.MCR@GRARM"]);

            Points1948Accept.Add("DKDay", AllPoints["11VT_SPDG00A_1948-066.MCR@GRARM"]);//+
            Points1948Accept.Add("DKMonth", AllPoints["11VT_SPDG00A_1948-067.MCR@GRARM"]);//+
            Points1948Accept.Add("DKYear", AllPoints["11VT_SPDG00A_1948-068.MCR@GRARM"]);//+
            Points1948Accept.Add("DKStartDay", AllPoints["11VT_SPDG00A_1948-069.MCR@GRARM"]);//+
            Points1948Accept.Add("DKStartMonth", AllPoints["11VT_SPDG00A_1948-070.MCR@GRARM"]);//+
            Points1948Accept.Add("DKStartYear", AllPoints["11VT_SPDG00A_1948-071.MCR@GRARM"]);//+
            Points1948Accept.Add("DKEndDay", AllPoints["11VT_SPDG00A_1948-072.MCR@GRARM"]);//+
            Points1948Accept.Add("DKEndMonth", AllPoints["11VT_SPDG00A_1948-073.MCR@GRARM"]);//+
            Points1948Accept.Add("DKEndYear", AllPoints["11VT_SPDG00A_1948-074.MCR@GRARM"]);//+

            Points1948Accept.Add("DKHour", AllPoints["11VT_SPDG00A_1948-081.MCR@GRARM"]);//+
            Points1948Accept.Add("DKMin", AllPoints["11VT_SPDG00A_1948-082.MCR@GRARM"]);//+
            Points1948Accept.Add("DKStartHour", AllPoints["11VT_SPDG00A_1948-085.MCR@GRARM"]);//+
            Points1948Accept.Add("DKStartMin", AllPoints["11VT_SPDG00A_1948-086.MCR@GRARM"]);//+
            Points1948Accept.Add("DKEndHour", AllPoints["11VT_SPDG00A_1948-089.MCR@GRARM"]);//+
            Points1948Accept.Add("DKEndMin", AllPoints["11VT_SPDG00A_1948-090.MCR@GRARM"]);//+

            Points1948Accept.Add("DKVal", AllPoints["11VT_SPDG00A_1948-116.MCR@GRARM"]);//+
            Points1948Accept.Add("DKNum", AllPoints["11VT_SPDG00A_1948-115.MCR@GRARM"]);//+


            foreach (KeyValuePair<string, EDSPointInfo> de in Points1948Accept)
            {
                Points1045Accept.Add(de.Key, AllPoints[de.Value.IESS.Replace("1948", "1045")]);
            }
        }

        protected static DateTime readDateFromGRARM(EDSReport rep, DateTime dtRep,
            EDSReportRequestRecord rYear, EDSReportRequestRecord rMonth, EDSReportRequestRecord rDay,
            EDSReportRequestRecord rHour, EDSReportRequestRecord rMin)
        {
            int year = (int)rep.ResultData[dtRep][rYear.Id];
            int month = (int)rep.ResultData[dtRep][rMonth.Id];
            int day = (int)rep.ResultData[dtRep][rDay.Id];
            int hour = (int)rep.ResultData[dtRep][rHour.Id];
            int min = (int)rep.ResultData[dtRep][rMin.Id];
            DateTime result = new DateTime(year, month, day, hour, min, 0);
            return result;
        }

        public async Task<bool> ReadData(DateTime dateStart, DateTime dateEnd)
        {
            DateStart = dateStart;
            DateEnd = dateEnd;
            DKDataGOU1045New = await ReadDataGOU(dateStart, dateEnd, "ГТП 110 Новая",false, Points1045New);
            DKDataGOU1948New = await ReadDataGOU(dateStart, dateEnd, "ГТП 220 Новая",false, Points1948New);
            DKDataGOU1045Accept = await ReadDataGOU(dateStart, dateEnd, "ГТП 110 Акцепт",true, Points1045Accept);
            DKDataGOU1948Accept = await ReadDataGOU(dateStart, dateEnd, "ГТП 220 Акцепт",true, Points1948Accept);
            DKDataFull = new SortedList<DateTime, SDPMDKRecord>();
            foreach (KeyValuePair<DateTime, SDPMDKRecord> de in DKDataGOU1045New)
            {
                DKDataFull.Add(de.Key, de.Value);
            }
            foreach (KeyValuePair<DateTime, SDPMDKRecord> de in DKDataGOU1948New)
            {
                DateTime dt = de.Key;
                while (DKDataFull.ContainsKey(dt))
                {
                    dt = dt.AddMilliseconds(1);
                }

                DKDataFull.Add(dt, de.Value);
            }
            foreach (KeyValuePair<DateTime, SDPMDKRecord> de in DKDataGOU1045Accept)
            {
                DateTime dt = de.Key;
                while (DKDataFull.ContainsKey(dt))
                {
                    dt = dt.AddMilliseconds(1);
                }

                DKDataFull.Add(dt, de.Value);
            }
            foreach (KeyValuePair<DateTime, SDPMDKRecord> de in DKDataGOU1948Accept)
            {
                DateTime dt = de.Key;
                while (DKDataFull.ContainsKey(dt))
                {
                    dt = dt.AddMilliseconds(1);
                }

                DKDataFull.Add(dt, de.Value);
            }
            return true;
        }

        protected async Task<Dictionary<DateTime, SDPMDKRecord>> ReadDataGOU(DateTime dateStart, DateTime dateEnd, string GOU, bool accept, Dictionary<string, EDSPointInfo> PointsRef)
        {
            EDSReport report = new EDSReport(dateStart, dateEnd, EDSReportPeriod.sec);
            EDSReportRequestRecord recDKCRC = null;
            EDSReportRequestRecord recDKIDAuto = null;
            EDSReportRequestRecord recDKIDMan = null;
            if (accept)
            {
                recDKIDAuto = report.addRequestField(PointsRef["DKIDAuto"], EDSReportFunction.val);
                recDKIDMan = report.addRequestField(PointsRef["DKIDMan"], EDSReportFunction.val);
            }
            else
            {
                recDKCRC = report.addRequestField(PointsRef["DKCRC"], EDSReportFunction.val);
            }
            bool ok = await report.ReadData();

            List<DateTime> dates = report.ResultData.Keys.ToList();

            SortedList<DateTime, bool> DKDates = new SortedList<DateTime, bool>();
            double prevID = -1;
            double prevIDMan = -1;
            double prevIDAuto = -1;
            foreach (DateTime date in dates)
            {
                if (accept)
                {
                    double idMan = report.ResultData[date][recDKIDMan.Id];
                    double idAuto = report.ResultData[date][recDKIDAuto.Id];
                    if (idMan != prevIDMan && prevIDMan != -1 && idMan != 0)
                    {
                        DKDates.Add(date, false);
                        
                    }

                    if (idAuto != prevIDAuto && prevIDAuto != -1 && idAuto != 0)
                    {
                        DKDates.Add(date, true);
                    }
                    prevIDMan = idMan;
                    prevIDAuto = idAuto;
                }
                else
                {
                    double id = report.ResultData[date][recDKCRC.Id];

                    if (id != prevID && prevID != -1 && id != 0)
                    {
                        DKDates.Add(date, true);
                    }
                    prevID = id;
                }

            }

            Dictionary<DateTime, SDPMDKRecord> ResultData = new Dictionary<DateTime, SDPMDKRecord>();
            foreach (DateTime date in DKDates.Keys)
            {
                EDSReport repDK = new EDSReport(date, date.AddSeconds(3), EDSReportPeriod.sec);
                Dictionary<string, EDSReportRequestRecord> repRecords = new Dictionary<string, EDSReportRequestRecord>();
                foreach (KeyValuePair<string, EDSPointInfo> de in PointsRef)
                {
                    EDSReportRequestRecord rec = repDK.addRequestField(PointsRef[de.Key], EDSReportFunction.val);
                    repRecords.Add(de.Key, rec);
                }
                ok = await repDK.ReadData();

                SDPMDKRecord DKRecord = new SDPMDKRecord();
                DKRecord.DKTrigger = date;
                DKRecord.DKTime = readDateFromGRARM(repDK, date.AddSeconds(2),
                    repRecords["DKYear"], repRecords["DKMonth"], repRecords["DKDay"],
                    repRecords["DKHour"], repRecords["DKMin"]);
                DKRecord.DKStartTime = readDateFromGRARM(repDK, date.AddSeconds(2),
                    repRecords["DKStartYear"], repRecords["DKStartMonth"], repRecords["DKStartDay"],
                    repRecords["DKStartHour"], repRecords["DKStartMin"]);
                DKRecord.DKEndTime = readDateFromGRARM(repDK, date.AddSeconds(2),
                    repRecords["DKEndYear"], repRecords["DKEndMonth"], repRecords["DKEndDay"],
                    repRecords["DKEndHour"], repRecords["DKEndMin"]);
                DKRecord.DKVal = repDK.ResultData[date.AddSeconds(2)][repRecords["DKVal"].Id];
                DKRecord.DKNum = (int)repDK.ResultData[date.AddSeconds(2)][repRecords["DKNum"].Id];
                DKRecord.GOU = GOU;
                DKRecord.AutoDK = DKDates[date];
                ResultData.Add(date, DKRecord);

                switch (DKRecord.DKNum)
                {
                    case 1:
                        DKRecord.DK = "Работать по ПДГ";
                        break;
                    case 2:
                        DKRecord.DK = String.Format("На {0:0.0} МВт выше ПДГ", DKRecord.DKVal);
                        break;
                    case 3:
                        DKRecord.DK = String.Format("На {0:0.0} МВт ниже ПДГ", DKRecord.DKVal);
                        break;
                    case 4:
                        DKRecord.DK = String.Format("Генерация {0:0.0} МВт ", DKRecord.DKVal);
                        break;
                }

            }
            return ResultData;
        }

        public void sendDKData()
        {
            try
            {
                string header = "<tr><th>Дата<br/>получения</th><th>Время<br/>команды</th><th>ГОУ</th><th>Команда</th><th>Время<br/>начала</th><th>Время</br>окончания</th>";
                string table = "";
                foreach (SDPMDKRecord rec in DKDataFull.Values)
                {
                    string s = "";
                    s += String.Format("<td>{0}{1}</td>", rec.DKTrigger.ToString("dd.MM HH:mm:ss"),(rec.AutoDK?"":"<br/>Экспресс"));
                    s += String.Format("<td>{0}</td>", rec.DKTime.ToString("dd.MM HH:mm"));
                    s += String.Format("<td>{0}</td>", rec.GOU);
                    s += String.Format("<td>{0}</td>", rec.DK);
                    s += String.Format("<td>{0}</td>", rec.DKStartTime.ToString("dd.MM HH:mm"));
                    s += String.Format("<td>{0}</td>", rec.DKEndTime.ToString("dd.MM HH:mm"));
                    s = string.Format("<tr>{0}</tr>", s);
                    table += s;
                }
                table = string.Format("<table border='1'>{0}{1}</table></html>", header, table);


                //FileInfo file = new FileInfo("c:/int/ftpfolder/"+fn);
                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();

                mess.From = new MailAddress(Settings.Single.SMTPFrom);

                mess.Subject = String.Format("Диспетчерские команды {0} - {1}", DateStart.ToString("dd.MM.yyyy"), DateEnd.ToString("dd.MM.yyyy"));
                mess.Body = String.Format("<html><h1>{0}</h1>{1}</html>", mess.Subject, table);

                string[] mails = Settings.Single.DKMail.Split(';');
                foreach (string mail in mails)
                {
                    mess.To.Add(mail);
                }

                //mess.Attachments.Add(new Attachment(fn));

                mess.SubjectEncoding = System.Text.Encoding.UTF8;
                mess.BodyEncoding = System.Text.Encoding.UTF8;
                mess.IsBodyHtml = true;
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Settings.Single.SMTPServer, Settings.Single.SMTPPort);
                client.EnableSsl = false;
                if (string.IsNullOrEmpty(Settings.Single.SMTPUser))
                {
                    client.UseDefaultCredentials = true;
                }
                else
                {
                    client.Credentials = new System.Net.NetworkCredential(Settings.Single.SMTPUser, Settings.Single.SMTPPassword, Settings.Single.SMTPDomain);
                }
                // Отправляем письмо
                client.Send(mess);
                Logger.Info("Данные в автооператор отправлены успешно");

            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при отправке почты: {0}", e.ToString()), Logger.LoggerSource.server);
                Logger.Info("Данные в автооператор не отправлены");
            }
        }
    }
}
