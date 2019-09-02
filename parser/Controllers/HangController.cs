using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotClass.Bot;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace parser.Controllers
{
	public class HangFireController : Controller
	{
		private IBotService botService;
		public HangFireController(IBotService botService)
		{
			this.botService = botService;
		}
		public IActionResult Index()
		{
			
			return View();
		}
	}
}
