using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ReportDevExpress.Models
{
	public class Logic
	{
		public DataTable Table { get; set; }
		public List<DateTime> Dates { get; set; }
		public string MonitoredPeriodStart { get; set; }
		public string MonitoredPeriodEnd { get; set; }
		public TimeSpan MonitoredPeriod { get; set; }
		public List<Deviation> RegulatoryDeviationData { get; set; }
		public List<SwitchData> SwitchUsageData { get; set; }
		public List<Deviation> CosPhiDeviationData { get; set; }
		public DataTable DeviationInfoTable { get; set; }
		public DataTable SwitchInfoTable { get; set; }
		public DataTable CosPhiInfoTable { get; set; }

		public Logic(Stream filePath)
		{
			this.Table = LoadData(filePath);
			this.Dates = LoadDates();
			this.MonitoredPeriodStart = Table.Rows[0]["čas záznamu[s]"].ToString();
			this.MonitoredPeriodEnd = Table.Rows[Table.Rows.Count - 1]["čas záznamu[s]"].ToString();
			this.MonitoredPeriod = setMonitoredPeriod();
			this.RegulatoryDeviationData = countRegulatoryDeviation();
			this.SwitchUsageData = getSwitchUsageData();
			this.CosPhiDeviationData = countCosPhiDeviation();
			this.DeviationInfoTable = createDeviationInfoTable();
			this.SwitchInfoTable = createSwitchInfoTable();
			this.CosPhiInfoTable = createCosPhiInfoTable();
		}

		private List<DateTime> LoadDates()
		{
			List<DateTime> tmp = new List<DateTime>();
			foreach(DataRow row in Table.Rows){
				tmp.Add(DateTime.Parse(row["čas záznamu[s]"].ToString()));
			}
			return tmp;
		}

		private DataTable createCosPhiInfoTable(){
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn().ColumnName = "Sledovana velicina");
			dt.Columns.Add(new DataColumn().ColumnName = "Cas");
			dt.Columns.Add(new DataColumn().ColumnName = "Skutecna hodnota");
			dt.Columns.Add(new DataColumn().ColumnName = "Hodnota odchylky");
			DataRow expectedValRow = dt.NewRow();
			expectedValRow["Sledovana velicina"] = "Očekávaná hodnota účiníku";
			expectedValRow["Skutecna hodnota"] = 1;
			dt.Rows.Add(expectedValRow);
			DataRow avgCosPhiRow = dt.NewRow();
			avgCosPhiRow["Sledovana velicina"] = "Průměrný účiník";
			avgCosPhiRow["Skutecna hodnota"] = getAvgCosPhi();
			dt.Rows.Add(avgCosPhiRow);
			DataRow maxAllowedDeviationRow = dt.NewRow();
			maxAllowedDeviationRow["Sledovana velicina"] = "Maximální povolená odchylka";
			maxAllowedDeviationRow["Skutecna hodnota"] = "0.95";
			maxAllowedDeviationRow["Hodnota odchylky"] = "0.05";
			dt.Rows.Add(maxAllowedDeviationRow);
			DataRow maxInductionRow = dt.NewRow();
			double maxInduction = CosPhiDeviationData.Max(Deviation => Deviation.DeviationValue);
			maxInductionRow["Sledovana velicina"] = "Nejvyšší indukční odchylka";
			maxInductionRow["Cas"] = CosPhiDeviationData.Find(Deviation => Deviation.DeviationValue == maxInduction).Date.ToString("dd.MM.yyyy hh:mm:ss");
			maxInductionRow["Skutecna hodnota"] = Math.Round((1 - maxInduction), 3).ToString().Replace(",", ".");
			maxInductionRow["Hodnota odchylky"] = Math.Round(maxInduction,3).ToString().Replace(",", ".");
			dt.Rows.Add(maxInductionRow);
			DataRow minInductionRow = dt.NewRow();
			minInductionRow["Sledovana velicina"] = "Nejnižší indukční odchylka";
			minInductionRow["Skutecna hodnota"] = Math.Round((1 - CosPhiDeviationData.Min(Deviation => Deviation.DeviationValue)),3).ToString().Replace(",", ".");
			minInductionRow["Hodnota odchylky"] = Math.Round(CosPhiDeviationData.Min(Deviation => Deviation.DeviationValue),3).ToString().Replace(",", ".");
			minInductionRow["Cas"] = CosPhiDeviationData.Find(Deviation => Deviation.DeviationValue ==
				double.Parse(minInductionRow["Hodnota odchylky"].ToString())).Date.ToString("dd.MM.yyyy hh:mm:ss");
			dt.Rows.Add(minInductionRow);
			DataRow maxCapacityRow = dt.NewRow();
			double maxCapacity = CosPhiDeviationData.Max(Deviation => Deviation.ExpectedValue);
			maxCapacityRow["Sledovana velicina"] = "Nejvyšší kapacitní odchylka";
			maxCapacityRow["Cas"] = CosPhiDeviationData.Find(Deviation => Deviation.DeviationValue == maxCapacity).Date.ToString("dd.MM.yyyy hh:mm:ss");
			maxCapacityRow["Skutecna hodnota"] = Math.Round(1 - maxCapacity,3).ToString().Replace(",", ".");
			maxCapacityRow["Hodnota odchylky"] = Math.Round(maxCapacity,3).ToString().Replace(",", ".");
			dt.Rows.Add(maxCapacityRow);
			DataRow minCapacityRow = dt.NewRow();
			minCapacityRow["Sledovana velicina"] = "Nejnižší kapacitní odchylka";
			minCapacityRow["Skutecna hodnota"] = Math.Round(1 - CosPhiDeviationData.Min(Deviation => Deviation.ExpectedValue),3).ToString().Replace(",", ".");
			minCapacityRow["Hodnota odchylky"] = Math.Round(CosPhiDeviationData.Min(Deviation => Deviation.ExpectedValue),3).ToString().Replace(",", ".");
			minCapacityRow["Cas"] = CosPhiDeviationData.Find(Deviation => Deviation.DeviationValue ==
				double.Parse(minCapacityRow["Hodnota odchylky"].ToString())).Date.ToString("dd.MM.yyyy hh:mm:ss");
			dt.Rows.Add(minCapacityRow);
			foreach (Deviation d in CosPhiDeviationData)
			{
				if (d.DeviationValue > 0.05)
				{
					DataRow dr = dt.NewRow();
					dr["Sledovana velicina"] = "Indukční odchylka mimo rozmezí";
					dr["Cas"] = d.Date.ToString("dd.MM.yyyy hh:mm:ss");
					dr["Hodnota odchylky"] = Math.Round(d.DeviationValue,3).ToString().Replace(",", ".");
					dr["Skutecna hodnota"] = Math.Round(1 - d.DeviationValue,3).ToString().Replace(",", ".");
					dt.Rows.Add(dr);
				}
				if (d.ExpectedValue > 0.05)
				{
					DataRow dr = dt.NewRow();
					dr["Sledovana velicina"] = "Kapacitní Odchylka mimo rozmezí";
					dr["Cas"] = d.Date.ToString("dd.MM.yyyy hh:mm:ss");
					dr["Hodnota odchylky"] = Math.Round(d.ExpectedValue,3).ToString().Replace(",", ".");
					dr["Skutecna hodnota"] = Math.Round(1 - d.ExpectedValue,3).ToString().Replace(",", ".");
					dt.Rows.Add(dr);
				}
			}
			return dt;
		}

		private string getAvgCosPhi()
		{
			double result = 0;
			int count = 0;
			foreach(DataRow row in Table.Rows){
				if(row["3Cosφ[]"].ToString().Split(" ").Length == 1){
					result += double.Parse(row["3Cosφ[]"].ToString().Split(" ")[0]);
				}else{
					result += double.Parse(row["3Cosφ[]"].ToString().Split(" ")[1]);
				}
				count++;
			}
			return Math.Round(result/count,3).ToString().Replace(",",".");
		}

		private double reactivePowerDelivery(){
			double result = 0;
			foreach(DataRow row in Table.Rows){
				if(double.Parse(row["prm.3Q[kvar]"].ToString()) < 0){
					result += double.Parse(row["prm.3Q[kvar]"].ToString())/240;
				}
			}
			return Math.Abs(Math.Round(result,3));
		}

		private DataTable createSwitchInfoTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn().ColumnName = "Stykac");
			dt.Columns.Add(new DataColumn().ColumnName = "Pocet sepnuti celkem");
			dt.Columns.Add(new DataColumn().ColumnName = "Pocet sepnuti za sledovani");
			for(int i = 0; i < MonitoredPeriod.Days; i++){
				dt.Columns.Add(new DataColumn().ColumnName = "Pocet sepnuti za den"+(i+1));
			}
			dt.Columns.Add(new DataColumn().ColumnName = "Cas v provozu celkem");
			dt.Columns.Add(new DataColumn().ColumnName = "Stupne");
			foreach (SwitchData sw in SwitchUsageData){
				DataRow row = dt.NewRow();
				row["Stykac"] = sw.SwitchName;
				row["Pocet sepnuti celkem"] = sw.TotalSwitchEver;
				row["Pocet sepnuti za sledovani"] = sw.TotalSwitchAmount;
				for (int i = 0; i < sw.SwitchInDays.Count; i++){
					row["Pocet sepnuti za den" + (i + 1)] = sw.SwitchInDays[i];
				}
				row["Cas v provozu celkem"] = sw.TotalSwitchTimeEver;
				row["Stupne"] = sw.Degree;
				dt.Rows.Add(row);
			}
			return dt;
		}

		private DataTable createDeviationInfoTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn().ColumnName = "Sledovana Velicina");
			dt.Columns.Add(new DataColumn().ColumnName = "Cas");
			dt.Columns.Add(new DataColumn().ColumnName = "Hodnota");
			dt.Columns.Add(new DataColumn().ColumnName = "Jednotka");
			DataRow cosPhiDeliveryRow = dt.NewRow();
			cosPhiDeliveryRow["Sledovana Velicina"] = "Jalová dodávka";
			cosPhiDeliveryRow["Hodnota"] = reactivePowerDelivery();
			cosPhiDeliveryRow["Jednotka"] = "Kvarh";
			dt.Rows.Add(cosPhiDeliveryRow);
			DataRow maxRow = dt.NewRow();
			maxRow["Sledovana Velicina"] = "Nejvyšší regulační odchylka";
			double maxValue = RegulatoryDeviationData.Max(Deviation => Deviation.DeviationValue);
			maxRow["Cas"] = RegulatoryDeviationData.Find(Deviation => Deviation.DeviationValue == maxValue).Date.ToString("dd.MM.yyyy hh:mm:ss");
			maxRow["Hodnota"] = Math.Round(maxValue,3);
			maxRow["Jednotka"] = "Kvar";
			dt.Rows.Add(maxRow);
			DataRow minRow = dt.NewRow();
			minRow["Sledovana Velicina"] = "Nejnižší regulační odchylka";
			double minValue = RegulatoryDeviationData.Min(Deviation => Deviation.DeviationValue);
			minRow["Cas"] = RegulatoryDeviationData.Find(Deviation => Deviation.DeviationValue == minValue).Date.ToString("dd.MM.yyyy hh:mm:ss");
			minRow["Hodnota"] = Math.Round(minValue,3);
			minRow["Jednotka"] = "Kvar";
			dt.Rows.Add(minRow);
			DataRow avgRow = dt.NewRow();
			avgRow["Sledovana Velicina"] = "Průměrná regulační odchylka";
			avgRow["Hodnota"] = Math.Round(RegulatoryDeviationData.Sum(Deviation => Deviation.DeviationValue)/ RegulatoryDeviationData.Count,3);
			avgRow["Jednotka"] = "Kvar";
			dt.Rows.Add(avgRow);
			return dt;
		}

		private List<Deviation> countCosPhiDeviation()
		{
			List<Deviation> tmp = new List<Deviation>();
			int expectedCosPhi = 1;
			foreach (DataRow row in Table.Rows){
				string[] data = row["3Cosφ[]"].ToString().Split(" ");
				if (data.Length == 1){
					continue;
				}else{
					if (data[0] == "L"){
						tmp.Add( new Deviation{
							Date = DateTime.Parse(row["čas záznamu[s]"].ToString()),
							DeviationValue = expectedCosPhi - formatPlatform(data[1])
						});
					}else{
						tmp.Add(new Deviation{
							Date = DateTime.Parse(row["čas záznamu[s]"].ToString()),
							ExpectedValue = expectedCosPhi - formatPlatform(data[1])
						});
					}
				}
			}
			return tmp;
		}

		private void getSwitchTimeData(List<SwitchData> list){
			string[] degrees = loadConfigFile();
			for (int j = 1; j < 19; j++){
				list[j - 1].TimeValue = (int.Parse(Table.Rows[Table.Rows.Count - 1]["OutputSwitch.OnTime" + j + "[s]"].ToString()) -
							int.Parse(Table.Rows[0]["OutputSwitch.OnTime" + j + "[s]"].ToString())) / 3600;
				list[j - 1].Degree = degrees[j - 1];
				list[j - 1].TotalSwitchTimeEver = int.Parse(Table.Rows[Table.Rows.Count - 1]["OutputSwitch.OnTime" + j + "[s]"].ToString()) / 3600;
				list[j - 1].SwitchNameDegree = "Stykac" + j + " (" + degrees[j - 1] + ")";
			}
		}

		private string[] loadConfigFile(){
			return System.IO.File.ReadAllText("wwwroot/Config_files/Config.txt").Split(";");
		}

		private List<SwitchData> getSwitchUsageData(){
			List<SwitchData> tmp = new List<SwitchData>();
			int index = 1;
			int j = 1;
			foreach (DataColumn col in Table.Columns){
				if (col.ColumnName.Contains("OutputSwitch.N")){
					int hourCounter = 0;
					double hourValue = 0;
					int dayCounter = 0;
					double dayValue = 0;
					//int monthCounter = 0;
					//int yearCounter = 0;
					// startovací časy nastavené na 00:00:00
					DateTime startHour = createDateTime(Table.Rows[0][0].ToString());
					DateTime startDay = createDateTime(Table.Rows[0][0].ToString());
					DateTime startMonth = createDateTime(Table.Rows[0][0].ToString());
					//první hodnoty k porovnání
					double startHourValue = formatPlatform(Table.Rows[0][col].ToString());
					double startDayValue = formatPlatform(Table.Rows[0][col].ToString());
					foreach (DataRow row in Table.Rows){
						DateTime end = createDateTime(row["čas záznamu[s]"].ToString());
						if (end.Subtract(startHour).Hours == 1){
							hourValue = hourValue + (formatPlatform(row[col].ToString()) - startHourValue);
							startHourValue = formatPlatform(row[col].ToString());
							hourCounter++;
							startHour = startHour.AddHours(1);
						}
						if (end.Subtract(startDay).Days == 1){
							dayValue += (formatPlatform(row[col].ToString()) - startDayValue);
							startDayValue = formatPlatform(row[col].ToString());
							dayCounter++;
							startDay = startDay.AddDays(1);
						}
					}
					tmp.Add(new SwitchData {
						SwitchName = "Stykac"+j, AvgSwitchHour = (hourValue / hourCounter), AvgSwitchDay = dayValue / dayCounter,
						TotalSwitchAmount = int.Parse(Table.Rows[Table.Rows.Count - 1][col].ToString()) - int.Parse(Table.Rows[0][col].ToString()),
						TotalSwitchEver = int.Parse(Table.Rows[Table.Rows.Count - 1][col].ToString()), SwitchInDays = countEveryDaySwitch(col)
						
					});
					j++;
					index++;
				}
			}
			getSwitchTimeData(tmp);
			return tmp;
		}

		private List<int> countEveryDaySwitch(DataColumn col)
		{
			List<int> tmp = new List<int>();
			DataRow previouseRow = Table.Rows[0];
			foreach (DataRow row in Table.Rows){
				if (createDateTime(row[0].ToString()).Subtract(createDateTime(previouseRow[0].ToString())).Days == 1){
					tmp.Add(int.Parse(row[col].ToString()) - int.Parse(previouseRow[col].ToString()));
					previouseRow = row;
				}
			}
			return tmp;
		}

		private TimeSpan setMonitoredPeriod()
		{
			DateTime start = createDateTime(MonitoredPeriodStart);
			DateTime end = createDateTime(MonitoredPeriodEnd);
			return end.Subtract(start);
		}

		private DateTime createDateTime(string date)
		{
			string[] dateData = date.Split(" ");
			string[] tmpDateData = dateData[0].Split(".");
			string[] tmpTimeData = dateData[1].Split(":");
			return new DateTime(int.Parse(tmpDateData[2]), int.Parse(tmpDateData[1]), int.Parse(tmpDateData[0]),
				int.Parse(tmpTimeData[0]), int.Parse(tmpTimeData[1]), int.Parse(tmpTimeData[2]));
		}

		private List<Deviation> countRegulatoryDeviation()
		{
			/*
                1 = cos( arctan( Q/P))
                arccos(1) = arctan(Q/P)
                tan(arccos(1) = Q/P
                (tan(arccos(1))/P = Q               
            */
			int expectedCosPhi = 1;
			List<Deviation> tmp = new List<Deviation>();
			for (int i = 0; i < Table.Rows.Count - 1; i++)
			{
				tmp.Add(new Deviation
				{
					Date = DateTime.Parse(Table.Rows[i]["čas záznamu[s]"].ToString()),
					ExpectedValue = Math.Tan(Math.Acos(expectedCosPhi)) / double.Parse(Table.Rows[i]["prm.3P[kW]"].ToString()),
					DeviationValue = Math.Tan(Math.Acos(expectedCosPhi)) / double.Parse(Table.Rows[i]["prm.3P[kW]"].ToString())
					- double.Parse(Table.Rows[i]["prm.3Q[kvar]"].ToString())
				});
			}
			countAvgRegulatoryValues(tmp);
			return tmp;
		}

		private void countAvgRegulatoryValues(List<Deviation> list){
			// startovací časy nastavené na 00:00:00
			DateTime startMinute10 = list[0].Date;
			DateTime startMinute30 = list[0].Date;
			DateTime startHour = list[0].Date;
			DateTime startHour6 = list[0].Date;
			DateTime startDay = list[0].Date;
			//první hodnoty k porovnání
			double startMinuteValue10 = 0;
			double startMinuteValue30 = 0;
			double startHourValue = 0;
			double startHourValue6 = 0;
			double startDayValue = 0;
			foreach(Deviation d in list){
				DateTime end = d.Date;
				startMinuteValue10 += d.DeviationValue;
				startMinuteValue30 += d.DeviationValue;
				startHourValue += d.DeviationValue;
				startHourValue6 += d.DeviationValue;
				startDayValue += d.DeviationValue;
				if (end.Subtract(startMinute10).Minutes == 10){
					d.avgValue10M = startMinuteValue10 / 10;
					startMinuteValue10 = d.DeviationValue;
					startMinute10 = d.Date;
				}
				if (end.Subtract(startMinute30).Minutes == 30){
					d.avgValue30M = startMinuteValue30 / 30;
					startMinuteValue30 = d.DeviationValue;
					startMinute30 = d.Date;
				}
				if (end.Subtract(startHour).Hours == 1){
					d.avgValue1H = startHourValue / 60;
					startHourValue = d.DeviationValue;
					startHour = d.Date;
				}
				if (end.Subtract(startHour6).Hours == 6){
					d.avgValue6H = startHourValue6 / 360;
					startHourValue6 = d.DeviationValue;
					startHour6 = d.Date;
				}
				if (end.Subtract(startDay).Days == 1)
				{
					d.avgValue1D = startDayValue / 1440;
					startDayValue = d.DeviationValue;
					startDay = d.Date;
				}
			}
		}

		private DataTable LoadData(Stream filePath)
		{
			DataTable dt = new DataTable();
			using (StreamReader sr = new StreamReader(filePath))
			{

				dt.TableName = sr.ReadLine().Split(";")[0];
				string line;
				string[] headers = null;
				while ((line = sr.ReadLine()) != null)
				{
					if (headers == null)
					{
						headers = line.Split(';');
						foreach (String header in headers)
						{
							dt.Columns.Add(header);
						}
					}
					else
					{
						string[] rows = line.Split(';');
						DataRow dr = dt.NewRow();
						for (int i = 0; i < headers.Length; i++)
						{
							dr[i] = rows[i];
						}
						dt.Rows.Add(dr);
					}
				}
			}
			dt.Columns.RemoveAt(dt.Columns.Count - 1);
			return dt;
		}

		private double formatPlatform(String s)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return double.Parse(s.Replace(",", "."));
			}
			return double.Parse(s);
		}
	}
}
