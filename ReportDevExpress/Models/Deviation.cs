using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportDevExpress.Models
{
	public class Deviation
	{
		public DateTime Date { get; set; }
		public double ExpectedValue { get; set; }
		public double DeviationValue { get; set; }
		public double avgValue10M { get; set; }
		public double avgValue30M { get; set; }
		public double avgValue1H { get; set; }
		public double avgValue6H { get; set; }
		public double avgValue1D { get; set; }
	}
}
