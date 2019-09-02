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
	public class ParserController : Controller
	{
		private IBotService botService;
		public ParserController(IBotService botService)
		{
			this.botService = botService;
		}
		[HttpPost]
		public async Task StartParsing(int MinValue)
		{
			RecurringJob.AddOrUpdate(
				() => botService.SendMess(),
				Cron.Minutely);

			RecurringJob.AddOrUpdate(
				() => botService.DeleteDbLines(),
				Cron.Hourly);

			await botService.SetMinValue(MinValue);

		}
	}
}
