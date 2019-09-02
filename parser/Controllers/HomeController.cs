using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BotClass.Bot;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using parser.Models;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace parser.Controllers
{
	public class HomeController : Controller
	{
		
		public IActionResult Index()
		{
			

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
