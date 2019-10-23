using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppTest.Models;
using Framework.Result;
using Framework.Validator;

namespace AppTest.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";
			return View();
		}

		[HttpPost]
		public ActionResult Test(TestModel model)
		{
			return View();
		}
	}
}