using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
	public class OptimRashod
	{
		public static double getOptimRashod(double power, double h, List<int> availGG = null) {
			if (availGG == null) {
				availGG = new List<int>();
				for (int gg = 1; gg <= 10; gg++)
					availGG.Add(gg);
			}
			double minQ = 0;
			for (int count = 1; count <= availGG.Count; count++) {
				double divPower = power / count;
				if (divPower <= 115 && power >= 35) {
					List<double> Rashods = new List<double>();
					foreach(int gg in availGG) {
						double q = InerpolationRashod.getRashodGA(gg, divPower, h);
						Rashods.Add(q);
					}
					Rashods.Sort();
					double curQ = 0;
					for (int i = 1; i <= count;i++) {
						curQ += Rashods[i - 1];
					}
					if (curQ < minQ || minQ==0) {
						minQ = curQ;
					}
				}
			}
			return minQ;
		}

		public static SortedList<double,List<int>> getOptimRashodFull(double power, double h, List<int> availGG = null) {
			if (availGG == null) {
				availGG = new List<int>();
				for (int gg = 1; gg <= 10; gg++)
					availGG.Add(gg);
			}
			SortedList<double, List<int>> Result = new SortedList<double, List<int>>();
			for (int count = 1; count <= availGG.Count; count++) {
				double divPower = power / count;
				if (divPower <= 115 && power >= 35) {
					SortedList<double, int> Rashods = new SortedList<double, int>();
					foreach (int gg in availGG) {
						double q = InerpolationRashod.getRashodGA(gg, divPower, h);
						while (Rashods.ContainsKey(q))
							q += 10e-10;
						Rashods.Add(q, gg);
					}					
					double curQ = 0;
					List<int> sostav = new List<int>();
					for (int i = 1; i <= count; i++) {
						curQ += Rashods.Keys[i - 1];
						sostav.Add(Rashods.Values[i - 1]);
					}
					while (Result.ContainsKey(curQ)) {
						curQ += 10e-10;
					}
					sostav.Sort();
					Result.Add(curQ, sostav);
				}
			}
			return Result;
		}



	}
}
