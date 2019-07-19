using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using ReportDevExpress.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Globalization;
using System.Data;
using Microsoft.AspNetCore.Hosting;
public class HomeController : Controller
{
	private static Logic Logic { get; set; }
	// GET: /<controller>/
	public ActionResult Index(){
		return View();
	}

	[HttpPost]
	public ActionResult Upload(){
		try{
			var file = Request.Form.Files["fileUploader"];
			Stream stream = file.OpenReadStream();
			Logic = new Logic(stream);
		}catch{
			Response.StatusCode = 400;
		}
		return new EmptyResult();
	}

	public ActionResult RegulationView(){
		return View(Logic);
	}

	public ActionResult SwitchAvgView(){
		return View(Logic);
	}

	public ActionResult SwitchTimeView(){
		return View(Logic);
	}

	public ActionResult CosPhiView(){
		return View(Logic);
	}

	public ContentResult GetRegulatoryDeviationData(){
		return Content(JsonConvert.SerializeObject(Logic.RegulatoryDeviationData), "application/json");
	}
	public ContentResult GetSwitchUsageData(){
		return Content(JsonConvert.SerializeObject(Logic.SwitchUsageData), "application/json");
	}
	public ContentResult GetCosPhiDevData(){
		return Content(JsonConvert.SerializeObject(Logic.CosPhiDeviationData), "application/json");
	}
	public ContentResult GetRegulatorDeviationInfo(){
		return Content(JsonConvert.SerializeObject(Logic.DeviationInfoTable), "application/json");
	}
	public ContentResult GetSwitchInfo(){
		return Content(JsonConvert.SerializeObject(Logic.SwitchInfoTable), "application/json");
	}
	public ContentResult GetCosPhiInfo(){
		return Content(JsonConvert.SerializeObject(Logic.CosPhiInfoTable), "application/json");
	}
}
