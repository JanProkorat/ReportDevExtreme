using System.Collections.Generic;

namespace ReportDevExpress.Models
{
	public class SwitchData
	{
		public string SwitchName { get; set; }
		public string SwitchNameDegree { get; set; }
		public double AvgSwitchHour { get; set; }
		public double AvgSwitchDay { get; set; }
		public int TotalSwitchAmount { get; set; }
		public double TimeValue { get; set; }
		public string Degree { get; set; }
		public int TotalSwitchEver { get; set; }
		public double TotalSwitchTimeEver { get; set; }
		public List<int> SwitchInDays { get; set; }
	}
}