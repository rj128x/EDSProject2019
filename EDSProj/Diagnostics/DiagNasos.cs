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
        public SortedList<DateTime, double> MNUBSerie = new SortedList<DateTime, double>();
        public SortedList<DateTime, double> MNUCSerie = new SortedList<DateTime, double>();
        public SortedList<DateTime, double> GGRunSerie = new SortedList<DateTime, double>();


        public DiagNasos(DateTime dateStart,DateTime dateEnd,string GG)
        {
            this.DateStart = dateStart;
            this.DateEnd = dateEnd;
            this.GG = GG;

        }

        protected SortedList<DateTime, double> createPuskStopSerie(string type_data)
        {
            SortedList<DateTime, double> serie = new SortedList<DateTime, double>();
            DiagDBEntities diagDB = new DiagDBEntities();
            int gg = Int32.Parse(GG);
            IQueryable<PuskStopInfo> req = 
                    from pi in diagDB.PuskStopInfoes
                    where
                        pi.GG == gg && pi.TypeData == type_data &&
                        pi.TimeOff > DateStart && pi.TimeOn < DateEnd
                    select pi;
            foreach (PuskStopInfo pi in req)
            {
                serie.Add(pi.TimeOn, 0);
                serie.Add(pi.TimeOn.AddMilliseconds(1), 1);
                serie.Add(pi.TimeOff, 1);
                serie.Add(pi.TimeOff.AddMilliseconds(1), 0);
            }
            return serie;
        }
        public async Task<bool> ReadData()
        {
            GGRunSerie = createPuskStopSerie("GG_STOP");
            MNUASerie = createPuskStopSerie("MNU_1");
            MNUBSerie = createPuskStopSerie("MNU_2");
            MNUCSerie = createPuskStopSerie("MNU_3");
            return true;
        }
    }
}
