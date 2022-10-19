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
using System.Windows;

namespace NPRCHApp
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

	public class SettingsNPRCH
	{
        public bool FTPActive { get; set; }
        public string FTPServer { get; set; }
        public int FTPPort { get; set; }
        public string FTPUser { get; set; }
        public string FTPPassword { get; set; }
        public string FTPFolder { get; set; }
        public string BlocksString { get; set; }
        
        public static Dictionary<string, BlockData> BlocksDict;


        public static SettingsNPRCH Single { get; protected set; }
		public static void init(string filename) {
			try {
				SettingsNPRCH single = XMLSer<SettingsNPRCH>.fromXML(filename);
				Single = single;
                string blocks = single.BlocksString;
                string[] blocksArr = blocks.Split(new char[] { ';' });
                BlocksDict = new Dictionary<string, BlockData>();
                foreach (string blockStr in blocksArr)
                {
                    try
                    {
                        string[] arr = blockStr.Split(new char[] { '~' });
                        BlockData bd = new BlockData(arr[0], 
							Int32.Parse(arr[1]), 
							Int32.Parse(arr[2]), 
							Int32.Parse(arr[3]), 60.0 / Int32.Parse(arr[4]) * 50.0, 
							Int32.Parse(arr[5])/1000.0);
                        BlocksDict.Add(bd.BlockNumber, bd);
                    }
                    catch { }
                }
			} catch (Exception e) {
				//Logger.Error("Ошибка при чтении файла настроек " + e, Logger.LoggerSource.server);
			}
		}
	}

	
	
}
