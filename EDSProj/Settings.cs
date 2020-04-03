using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Config;
using System.Web;
using System.Net.Mail;
using System.Xml.Serialization;
using System.IO;

namespace EDSProj
{
	public class XMLSer<T>
	{
		public static void toXML(T obj, string fileName) {
			XmlSerializer mySerializer = new XmlSerializer(typeof(T));
			// To write to a file, create a StreamWriter object.
			StreamWriter myWriter = new StreamWriter(fileName);
			mySerializer.Serialize(myWriter, obj);
			myWriter.Close();
		}

		public static T fromXML(string fileName) {
			try {
				XmlSerializer mySerializer = new XmlSerializer(typeof(T));
				// To read the file, create a FileStream.
				FileStream myFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				// Call the Deserialize method and cast to the object type.
				T data = (T)mySerializer.Deserialize(myFileStream);
				myFileStream.Close();
				return data;
			} catch (Exception e) {
				return default(T);
			}
		}
	}

	public class Settings
	{
		public string DBName { get; set; }
		public string SMTPServer { get; set; }
		public string SMTPUser { get; set; }
		public string SMTPFrom { get; set; }
		public string SMTPTo { get; set; }
		public string SMTPDomain { get; set; }
		public string SMTPPassword { get; set; }
		public string EDSUser { get; set; }
		public string EDSPassword { get; set; }
		public int SMTPPort { get; set; }

		public string LogPath { get; set; }

        public string AOMail { get; set; }

        public string DiadDBServer { get; set; }
		public string DiagDBPassword { get; set; }
		public string DiagDBUser { get; set; }
		public string DiagDBName { get; set; }

		public string DiagFolder { get; set; }

		public static Settings Single { get; protected set; }
		public static void init(string filename) {
			try {
				Settings single = XMLSer<Settings>.fromXML(filename);
				Single = single;

			} catch (Exception e) {
				Logger.Error("Ошибка при чтении файла настроек " + e, Logger.LoggerSource.server);
			}
		}
	}

	public class MailClass
	{
		public static void sendMail(string message, string header) {
			try {
				string body = message;


				System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();

				mess.From = new MailAddress(Settings.Single.SMTPFrom);
				mess.Subject = header;
				mess.Body = body;

				string[] addr = Settings.Single.SMTPTo.Split(new char[] { ';' });
				foreach (string add in addr) {
					try {
						mess.To.Add(add);
					} catch { }
				}

				mess.SubjectEncoding = System.Text.Encoding.UTF8;
				mess.BodyEncoding = System.Text.Encoding.UTF8;
				mess.IsBodyHtml = true;
				System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(Settings.Single.SMTPServer, Settings.Single.SMTPPort);
				client.EnableSsl = true;
				if (string.IsNullOrEmpty(Settings.Single.SMTPUser)) {
					client.UseDefaultCredentials = true;
				} else {
					client.Credentials = new System.Net.NetworkCredential(Settings.Single.SMTPUser, Settings.Single.SMTPPassword, Settings.Single.SMTPDomain);
				}
				// Отправляем письмо
				client.Send(mess);

			} catch (Exception e) {
				Logger.Info(String.Format("Ошибка при отправке почты: {0}", e.ToString()), Logger.LoggerSource.server);
			}
		}
	}

	public class Logger
	{
		public enum LoggerSource { server, client, service, none }
		public log4net.ILog logger;

		protected static Logger context;

		public Logger() {

		}

		public bool IsFileLogger { get; protected set; }
		public FileAppender appender { get; protected set; }
		public string Path { get; protected set; }
		public string Name { get; protected set; }
		public DateTime Date { get; protected set; }


		public static void InitFileLogger(string path, string name, Logger newLogger = null) {
			try {
				if (context != null && context.IsFileLogger && context.appender != null) {
					context.appender.Close();
				}
			} catch { }

			if (newLogger == null) {
				newLogger = new Logger();
			}

			string fileName = String.Format("{0}/{1}_{2}.txt", path, name, DateTime.Now.ToString("dd_MM_yyyy"));
			try {
				PatternLayout layout = new PatternLayout(@"[%d] %-10p %m%n");
				newLogger.appender = new FileAppender();
				newLogger.appender.Layout = layout;
				newLogger.appender.File = fileName;
				newLogger.appender.AppendToFile = true;
				BasicConfigurator.Configure(newLogger.appender);
				newLogger.appender.ActivateOptions();

				newLogger.logger = LogManager.GetLogger(name);
				newLogger.Path = path;
				newLogger.Name = name;
				newLogger.IsFileLogger = true;
				newLogger.Date = DateTime.Now.Date;
				Logger.context = newLogger;
			} catch (Exception e) {
				Console.WriteLine("Ошибка при создании log-файла " + fileName);
				Console.WriteLine(e.Message);
			}
		}

		public static void init(Logger context) {
			Logger.context = context;
		}

		public static void checkFileLogger() {
			if (context.IsFileLogger && (DateTime.Now.Date > context.Date)) {
				InitFileLogger(context.Path, context.Name);
			}
		}

		protected virtual string createMessage(string message, LoggerSource source = LoggerSource.none) {
			if (source != LoggerSource.none) {
				return String.Format("{0,-10} {1}", source.ToString(), message);
			} else {
				return String.Format("{0}", message);
			}
		}

		protected virtual void info(string str, LoggerSource source = LoggerSource.none) {
			try { logger.Info(createMessage(str, source)); } catch { }
			Console.WriteLine(createMessage(str, source));
		}

		protected virtual void error(string str, LoggerSource source = LoggerSource.none) {
			try { logger.Error(createMessage(str, source)); } catch { }
			Console.WriteLine(createMessage(str, source));
		}

		protected virtual void debug(string str, LoggerSource source = LoggerSource.none) {
			try { logger.Debug(createMessage(str, source)); } catch { }
			Console.WriteLine(createMessage(str, source));
		}


		public static void Info(string str, LoggerSource source = LoggerSource.none) {
			Logger.checkFileLogger();
			context.info(str, source);
		}

		public static void Error(string str, LoggerSource source = LoggerSource.none) {
			Logger.checkFileLogger();
			context.info(str, source);
		}

		public static void Debug(string str, LoggerSource source = LoggerSource.none) {
			Logger.checkFileLogger();
			context.info(str, source);
		}

	}
}
