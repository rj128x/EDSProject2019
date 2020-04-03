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
        public SortedList<DateTime, double> GGRunSerie = new SortedList<DateTime, double>();


        public DiagNasos(DateTime dateStart,DateTime dateEnd,string GG)
        {
            this.DateStart = dateStart;
            this.DateEnd = dateEnd;
            this.GG = GG;

        }

        public async Task<bool> ReadData()
        {
            DiagDBEntities diagDB = new DiagDBEntities();
            IQueryable<PuskStopInfo> req = from pi in diagDB.PuskStopInfoes where 
                                           pi.GG == Int32.Parse(GG) && pi.TypeData == "GG_STOP" &&
                                           pi.TimeOff > DateStart && pi.TimeOn < DateEnd 
                                           select pi;
            foreach (PuskStopInfo pi in req)
            {
                GGRunSerie.Add(pi.TimeOn, 0);
                GGRunSerie.Add(pi.TimeOn.AddSeconds(1), 1);
                GGRunSerie.Add(pi.TimeOff, 1);
                GGRunSerie.Add(pi.TimeOff.AddSeconds(1), 0);

            }
            return true;
        }
    }
}
