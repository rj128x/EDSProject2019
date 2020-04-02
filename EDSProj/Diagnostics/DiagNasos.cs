using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDSProj.EDS;
using EDSProj.EDSWebService;

namespace EDSProj.Diagnostics
{

    public class DiagNasos : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string GG { get; set; }
        public SortedList<DateTime, double> MNUASerie = new SortedList<DateTime, double>();
        

        public DiagNasos(DateTime dateStart,DateTime dateEnd,string GG)
        {
            this.DateStart = dateStart;
            this.DateEnd = dateEnd;
            this.GG = GG;

        }

        public async Task<bool> ReadData()
        {
            MNUASerie = new SortedList<DateTime, double>();
            List<PuskStopData> resultA = await EDSClass.AnalizePuskStopData(GG + "VT_PS01DI-01.MCR@GRARM", DateStart, DateEnd);
            if (resultA.Count > 0)
            {
                foreach (PuskStopData rec in resultA)
                {
                    MNUASerie.Add(rec.TimeOn.AddMilliseconds(-1), 0);
                    MNUASerie.Add(rec.TimeOn, 1);
                    MNUASerie.Add(rec.TimeOff, 1);
                    MNUASerie.Add(rec.TimeOff.AddMilliseconds(1), 0);

                }
                return true;
            }
            return false;
        }
    }
}
