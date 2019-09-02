using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Dapper;
using System.Net;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using parser.Models;
using System.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;
using AngleSharp.Html.Dom;

namespace BotClass.Bot
{
	public interface IBotService
	{
		 Task SendMess();
		Task DeleteDbLines();
		Task SetMinValue(int minValue);
	}
	public class BotService : IBotService
	{
		TelegramBotClient Bot;
		List<string> list = new List<string>();
		string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HangfireTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
		public BotService()
		{
			Bot = new TelegramBotClient("700405329:AAFbO87PG7QIsgnFeQfobLp4wEb_GWNiVlo");
		}
		public async Task SetMinValue(int minValue)
		{
			using(var db = new SqlConnection(connectionString))
			{
				var request = await db.QueryAsync("SELECT * FROM Price");
				if(request.Count() == 0)
				{
					await db.ExecuteAsync("INSERT INTO Price (MinPrice) VALUES(@minValue)", new { minValue });
				}
				else
				{
					await db.ExecuteAsync("UPDATE Price SET MinPrice = @minValue WHERE Id = 1", new { minValue });
				}
			}
		}
		public async Task DeleteDbLines()
		{
			using(var db = new SqlConnection(connectionString))
			{
				var query = @"WITH CTE AS(SELECT TOP(50) * FROM ItemsHistory ORDER BY Date)
						DELETE CTE; ";

				await db.ExecuteAsync(query);

			}
		}
		public async Task SendMess()
		{
			string html;
			using (WebClient client = new WebClient())
			{
				html = client.DownloadString("https://bitskins.com/?appid=730&is_stattrak=0&has_stickers=0&is_souvenir=0&show_trade_delayed_items=0&sort_by=created_at&order=asc");
			}

			var parser = new HtmlParser();
			var document = await parser.ParseDocumentAsync(html);
			var test = document.GetElementsByClassName("item-solo");
			List<Item> items = new List<Item>();
			foreach (IElement el in test)
			{
				Item item = new Item();
				item.Price = Convert.ToDouble(el.GetElementsByClassName("item-price-display").FirstOrDefault(s => s.InnerHtml != null).TextContent.Split("$".ToCharArray())[1].Replace('.', ','));
				item.Name = el.GetElementsByClassName("panel-heading item-title").FirstOrDefault(s => s.InnerHtml != null).TextContent;
				item.itemId = el.GetElementsByClassName("hidden buyItemId").FirstOrDefault(s => s.InnerHtml != null).TextContent;
				item.Url1 = "https://bitskins.com/?appid=730&is_stattrak=0&has_stickers=0&is_souvenir=0&show_trade_delayed_items=0&sort_by=created_at&order=asc";
				item.Url2  = "https://bitskins.com" + el.GetElementsByTagName("small").FirstOrDefault().GetElementsByTagName("a").FirstOrDefault().GetAttribute("href");
				
			items.Add(item);

			}
			string jsonstring = JsonConvert.SerializeObject(items.Take(50));

			List<Item> newItem = new List<Item>(); 

			using(var db = new SqlConnection(connectionString))
			{
				var query = "SELECT TOP(1) * FROM ItemsHistory ORDER BY Date DESC";	
				ItemsViewModel item = await db.QueryFirstOrDefaultAsync<ItemsViewModel>(query);

				int minPrice = await db.QueryFirstOrDefaultAsync<int>("SELECT MinPrice FROM Price WHERE Id = 1");

				if (item != null)
				{
					if (item.ListItems != jsonstring)
					{
						List<Item> listItemsList = JsonConvert.DeserializeObject<List<Item>>(item.ListItems);
						List<Item> jsonstringList = JsonConvert.DeserializeObject<List<Item>>(jsonstring);

						for (int i = 0; i < listItemsList.Count; i++)
						{
							if (listItemsList[i] == jsonstringList[i])
							{
								break;
							}
							else if (listItemsList[i] != jsonstringList[i] && jsonstringList[i].Price >= minPrice)
							{
								newItem.Add(jsonstringList[i]);
							}
						}
					}
				}
				
			}

			using (var db = new SqlConnection(connectionString))
			{
				DateTime Date = DateTime.Now;
				string query = "INSERT INTO ItemsHistory (ListItems, Date) VALUES(@jsonstring, @Date)";
				await db.ExecuteAsync(query, new { jsonstring, Date });
			}
			foreach(var item in newItem)
			{
				await Bot.SendTextMessageAsync(
										  chatId: 466739920,
										  text: $"{item.Name}\n\nЦена: {item.Price} $\n\nСсылка на все товары - {item.Url1}\nСсылка на график - {item.Url2}"
										);
			}
			
		}
	}
}
