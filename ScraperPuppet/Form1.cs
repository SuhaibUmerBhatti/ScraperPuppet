using HtmlAgilityPack;
using PuppeteerSharp;
using ScraperPuppet.Quran;
using ScraperPuppet.Quran.Aya;
using ScraperPuppet.Quran.Surah;
using ScrappingLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
namespace ScraperPuppet
{
	public partial class Form1 : Form
	{
		private List<string> sSuraNames = new List<string>();
		public Form1()
		{
			InitializeComponent();
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			string fullUrl = "https://alqurankarim.net/surah-al-mursalat";
			Stopwatch sw = Stopwatch.StartNew();
			sw.Start();

			//sSuraNames = await GetSuraNamesPuppet(fullUrl);
			sSuraNames = await GetSuraNamesAsync(fullUrl);
			//await DisplayPage(fullUrl);
			foreach (string item in sSuraNames)
			{
				Debug.WriteLine(item);
			}
			//await DisplayPageAsync(fullUrl);
			//string sAya = await GetAyaDataPuppetAsync(sSuraNames[2], 10);
			//AyaData sAya = await GetAyaDataAsync(sSuraNames[2], 10);
			await GetSurah(100, sSuraNames[100]);
			sw.Stop();
			//await DisplayPage();

			Console.WriteLine(sw.Elapsed);
			//Debug.WriteLine(sAya);
		}

		#region Code using Puppeteer Sharp

		private async Task<List<string>> GetSuraNamesPuppetAsync(string url)
		{
			//await new BrowserFetcher().DownloadAsync();

			List<string> sSuraNames = new List<string>();
			var options = new LaunchOptions()
			{
				Headless = true,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
			};

			var browser = await Puppeteer.LaunchAsync(options, null);
			// Starts browser.

			var page = await browser.NewPageAsync();
			// Opens a new tab.

			await page.GoToAsync(url);
			// Navigates to page.

			var tags = await page.QuerySelectorAllAsync("#surah > option");

			foreach (var tag in tags)
			{
				sSuraNames.Add((await tag.GetPropertyAsync("value")).RemoteObject.Value.ToString());
			}
			//sSuraNames.Remove("0");
			return sSuraNames;
		}

		private async Task DisplaySurahPuppetAsync(string surahName)
		{
			var options = new LaunchOptions()
			{
				Headless = false,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
				//SlowMo = 10
			};

			var browser = await Puppeteer.LaunchAsync(options, null);
			// Starts browser.

			var page = await browser.NewPageAsync();
			// Opens a new tab.
			string url = $"https://alqurankarim.net/{surahName}";
			var navigationTask = page.WaitForNavigationAsync();
			await page.GoToAsync(url);
			await navigationTask;
			// Navigates to page.			
			var btn = await page.QuerySelectorAsync("button[id=moreAyaatBtn]");//button.btn
																			   // Gets more ayaat button.
			Debug.WriteLine((await btn.GetPropertyAsync("innerText")).RemoteObject.Value.ToString());
			// Displays text of button.
			while ((await btn.GetPropertyAsync("innerText")).RemoteObject.Value.ToString() == "More Ayat")
			// Clicks button as lon as it shows more ayaat.
			{

				//var navigationTask = page.WaitForNavigationAsync();
				//await page.ClickAsync("a.my-link");
				await btn.ClickAsync();
				await page.WaitForTimeoutAsync(10);
				//await navigationTask;

				//Debug.WriteLine(navigationTask.ToString());
				btn = await page.QuerySelectorAsync("button.btn");

			}
		}

		private async Task DisplayAyaPuppetAsync(string surahName, int aya)
		{
			var options = new LaunchOptions()
			{
				Headless = false,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
				//SlowMo = 10
			};

			var browser = await Puppeteer.LaunchAsync(options, null);
			// Starts browser.

			var page = await browser.NewPageAsync();
			// Opens a new tab.
			string url = $"https://alqurankarim.net/ur/{surahName}/ayat-{aya}/translation/tafsir";
			var navigationTask = page.WaitForNavigationAsync();
			await page.GoToAsync(url);
			await navigationTask;
			// Navigates to page.			
		}

		private async Task<string> GetAyaDataPuppetAsync(string surahName, int aya)
		{
			string sAya;
			var options = new LaunchOptions()
			{
				Headless = true,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
				//SlowMo = 10
			};

			var browser = await Puppeteer.LaunchAsync(options, null);
			// Starts browser.

			var page = await browser.NewPageAsync();
			// Opens a new tab.
			string url = $"https://alqurankarim.net/ur/{surahName}/ayat-{aya}/translation/tafsir";
			var navigationTask = page.WaitForNavigationAsync();
			await page.GoToAsync(url);
			await navigationTask;
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			sAya = await GetAyaPuppetAsync(page);
			sAya = await GetAyaKImanPuppetAsync(page);
			sAya = await GetAyaKIrfanPuppetAsync(page);
			List<string> sTafseer = await GetAyaTafseerPuppetAsync(page);
			return sAya;
		}

		private async Task<string> GetAyaPuppetAsync(IPage page)
		{
			string sAya;
			var tag = await page.QuerySelectorAsync("#araayat > div > div.col-lg-12.col-12 > div.row > div > p");
			// Gets the required element from the page.
			sAya = (await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();
			Debug.WriteLine((await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString());
			Debug.WriteLine(sAya);
			return sAya;
		}

		private async Task<string> GetAyaKImanPuppetAsync(IPage page)
		{
			string sAya;
			var tag = await page.QuerySelectorAsync("#araayat > div > div.col-lg-12.col-12 > span:nth-child(4)");
			// Gets the required element from the page.
			sAya = (await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();
			Debug.WriteLine((await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString());
			Debug.WriteLine(sAya);
			return sAya;
		}

		private async Task<string> GetAyaKIrfanPuppetAsync(IPage page)
		{
			string sAya;
			var tag = await page.QuerySelectorAsync("#araayat > div > div.col-lg-12.col-12 > span:nth-child(6)");
			// Gets the required element from the page.
			sAya = (await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();
			Debug.WriteLine((await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString());
			Debug.WriteLine(sAya);
			return sAya;
		}

		private async Task<List<string>> GetAyaTafseerPuppetAsync(IPage page)
		{
			//#araayat > div > div.col-lg-12.col-12 > div.tafseer-font-size
			List<string> sAya = new List<string>();
			var tags = await page.QuerySelectorAllAsync("#araayat > div > div.col-lg-12.col-12 > div.tafseer-font-size > p");
			// Gets the required element from the page.
			foreach (var tag in tags)
			{
				sAya.Add((await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString());
			}

			Debug.WriteLine(sAya[1]);
			//Debug.WriteLine(sAya);
			return sAya;
		}
		#endregion

		#region Code using System Http Client

		public async Task<List<string>> GetSuraNamesAsync(string url)
		{

			string response = await CallUrlAsync(url);

			var sSuraNames = ParseHtml(response);

			//WriteToCsv(linkList);

			return sSuraNames;
		}

		private static async Task<string> CallUrlAsync(string fullUrl)
		{
			HttpClient client = new HttpClient();
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			client.DefaultRequestHeaders.Accept.Clear();
			var response = client.GetStringAsync(fullUrl);
			return await response;
		}

		private List<string> ParseHtml(string html)
		{
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(html);
			var tags = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"surah\"]/option");

			List<string> sSuraNames = new List<string>();

			foreach (var tag in tags)
			{
				sSuraNames.Add(tag.GetAttributeValue("value", ""));
			}
			//sSuraNames.Remove("0");
			return sSuraNames;

		}

		private async Task GetSurah(int surahID, string surahName)
		{
			SurahData surahData = new SurahData();
			//Gets aya count
			//string aya = "10";
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();
			string url = $"https://alqurankarim.net/{surahName}";
			string response = await CallUrlAsync(url);
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(response);
			surahData.SurahNameEn = surahName.Substring(6, surahName.Length - 6);
			GetSurahData(htmlDoc, ref surahData);
			GetSurahLocation(htmlDoc, ref surahData);
			#region Synchronous
			for (int i = 0; i < surahData.AyaCount; i++)
			{
				surahData.Ayas.Add(await GetAyaDataAsync(surahName, i));
			}
			Console.WriteLine("Completed");
			surahData = null;
			#endregion

			#region Asynchronous with Task.WhenAll

			//var ayaTasks = new List<Task<AyaData>>();
			//for (int i = 0; i < surahData.AyaCount; i++)
			//{
			//	ayaTasks.Add(GetAyaDataAsync(surahName, i));
			//	//surahData.Ayas.Add(await GetAyaDataAsync(surahName, i));
			//}

			//var completedTasks = Task.WhenAll(ayaTasks);
			//await completedTasks;
			//foreach (var task in ayaTasks)
			//{
			//	surahData.Ayas.Add(task.Result);
			//}
			//Console.WriteLine("Completed");

			#endregion
			#region Asynchronous with Task.WhenAny
			/*			var ayaTasks = new List<Task<AyaData>>();
						for (int i = 0; i < surahData.AyaCount; i++)
						{
							ayaTasks.Add(GetAyaDataAsync(surahName, i));
							//surahData.Ayas.Add(await GetAyaDataAsync(surahName, i));
						}
						while (ayaTasks.Count > 0)
						{
							var finishedTask = await Task.WhenAny(ayaTasks);
							surahData.Ayas.Add(finishedTask.Result);
							ayaTasks.Remove(finishedTask);
						}

						Console.WriteLine("Completed");*/
			#endregion
			// Gets aya count
		}
		private delegate AyaData ayaTaskDelegate(string surahName, int ayaID);
		private void GetSurahLocation(HtmlAgilityPack.HtmlDocument htmlDocument, ref SurahData surahData)
		{
			IEnumerable<HtmlNode> nodes = htmlDocument.DocumentNode.Descendants("sub").Where(node => node.GetAttributeValue("class", "").Contains("sura__name__res"));
			/*
						string[] elements0; string[] elements;
						Stopwatch stopwatch = new Stopwatch();
						stopwatch.Start();
						for (int i = 0; i < 100; i++)
							elements0 = nodes.First().InnerText.Split('\n');
						stopwatch.Stop();
						Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
						stopwatch.Reset();
						stopwatch.Restart();
						for (int i = 0; i < 100; i++)
						{
							nodesArray = nodes.ToArray();
							elements = nodesArray[0].InnerText.Split('\n');
						}
						stopwatch.Stop();
						Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
			*/
			string[] elements = nodes.First().InnerText.Split('\n');
			string temp = elements[elements.Length - 2].Trim();
			surahData.SurahLocation = temp.Substring(2, temp.Length - 3);
			Debug.WriteLine(surahData.SurahLocation);
		}
		private void GetSurahData(HtmlAgilityPack.HtmlDocument htmlDocument, ref SurahData surahData)
		{
			/*
			//*[@id="araayat"]/div/div[1]/table/tbody/tr[2]/td[1]/span/br
			//*[@id="araayat"]/div[1]/table/tbody/tr[2]/td[1]/span/br
			//html/body/div/section[2]/div/div[1]/div[2]/div[2]/div[2]/div[1]/table/tbody/tr[2]/td[1]/span/br
			//document.querySelector("#araayat > div:nth-child(1) > table > tbody > tr:nth-child(2) > td.two__row__td_one > span > br")
			//#araayat > div:nth-child(1) > table > tbody > tr:nth-child(2) > td.two__row__td_one > span > br
			var node1 = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div[1]/table/tr[2]/td[1]/span");
			Console.WriteLine(node1.InnerText.Split('\n')[3]);
			
			IEnumerable<HtmlNode> nodes;
			
				HtmlNodeCollection nodes0 = null;
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				for (int i = 0; i < 1000; i++)
					nodes0 = htmlDocument.DocumentNode.SelectNodes("//span[contains(@class,'res')]");
				stopwatch.Stop();
				Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
				stopwatch.Reset();
				stopwatch.Start();
				for (int i = 0; i < 1000000; i++)
			*/
			IEnumerable<HtmlNode> nodes = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("sura__name__res"));
			/*
				stopwatch.Stop();
				Console.WriteLine(stopwatch.ElapsedMilliseconds.ToString());
			

			foreach (var item in nodes)
			{
				var elements0 = item.InnerText.Split('\n');
				int count = elements0.Length;
				int subtract = count == 1 ? 1 : 2;
				Debug.WriteLine(elements0[count - subtract].Trim());
				var attributes = item.GetAttributes();
				foreach (var attrib in attributes)
				{
					Console.WriteLine(attrib.Name);
					Console.WriteLine(attrib.Value);
				}
			}
			*/
			var nodesArray = nodes.ToArray();

			string[] elements = nodesArray[0].InnerText.Split('\n');
			surahData.AyaCount = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			Debug.WriteLine(surahData.AyaCount);

			elements = nodesArray[1].InnerText.Split('\n');
			surahData.SurahID = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			Debug.WriteLine(surahData.SurahID);

			elements = nodesArray[2].InnerText.Split('\n');
			surahData.SurahNameAr = elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim();
			Debug.WriteLine(surahData.SurahNameAr);

			elements = nodesArray[3].InnerText.Split('\n');
			surahData.RevelationID = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			Debug.WriteLine(surahData.RevelationID);

			elements = nodesArray[4].InnerText.Split('\n');
			surahData.RukuCount = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			Debug.WriteLine(surahData.RukuCount);
		}
		private async Task<AyaData> GetAyaDataAsync(string surahName, int aya)
		{
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();
			string url = $"https://alqurankarim.net/ur/{surahName}/ayat-{aya}/translation/tafsir";
			string response = await CallUrlAsync(url);
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(response);

			//var sAyaTask = GetAya(surahName, aya, htmlDoc);
			ayaData.Aya = await GetAya(htmlDoc);
			ayaData.KIman = await GetAyaKImanAsync(htmlDoc);
			ayaData.KIrfan = await GetAyaKIrfanAsync(htmlDoc);
			GetAyaTafseer(htmlDoc, ref ayaData);
			//await Task.WhenAny(sAyaTask);
			return ayaData;
		}
		private async Task<string> GetAya(HtmlAgilityPack.HtmlDocument htmlDocument)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div/div[1]/div[2]/div/p");
			//Debug.WriteLine(node.InnerText);
			//return await Task.FromResult(node.InnerText);
			return await Task.Run(() => { return node.InnerText; });
		}

		private async Task<string> GetAyaKImanAsync(HtmlAgilityPack.HtmlDocument htmlDocument)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div/div[1]/span[2]");
			//Debug.WriteLine(node.InnerText);
			return await Task.Run(() => node.InnerText);
		}

		private async Task<string> GetAyaKIrfanAsync(HtmlAgilityPack.HtmlDocument htmlDocument)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div/div[1]/span[4]");
			//Debug.WriteLine(node.InnerText);
			return await Task.Run(() => node.InnerText);
		}

		private void GetAyaTafseer(HtmlAgilityPack.HtmlDocument htmlDocument, ref AyaData ayaData)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			List<string> sAya = new List<string>();
			//Each node has 2 attributes, style and lang.
			//style gives font size and fontfamily while lang gives language, which here is Arabic from Saudi Arabia.
			var nodes = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"araayat\"]/div/div[1]/div[3]/p");//Return about 14 nodes.
			var nodes1 = htmlDocument.DocumentNode.SelectNodes("//span[contains(@lang,'AR-SA')]");
			//var nodes2 = htmlDocument.DocumentNode.Descendants("span"); //Returns all span nodes, about 288
			//var nodes3 = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("lang", "").Contains("AR-SA")); //Return span nodes where lang is AR-SA
			//var attrib = nodes[1].GetDataAttributes();
			//var attributes = nodes[20].GetDataAttributes();
			foreach (var node in nodes)
			{
				ayaData.sTafseer.Add(node.InnerText);
				//Debug.WriteLine(node.InnerText);
			}
			foreach (var node in nodes1)
			{
				//Debug.WriteLine(node.InnerText);
				//Console.WriteLine(node.GetAttributeValue("style", "No Attribute Fount"));
				ayaData.AddTafsir(node.InnerText, node.GetAttributeValue("style", "No Attribute"));
				/*
								foreach (var item in node.GetAttributes())
								{
									Console.WriteLine("-----------------------------------------");
									Console.WriteLine(item.Name);
									Console.WriteLine(item.Value);
									Console.WriteLine("-----------------------------------------");
								}
				*/
			}
		}

		#endregion
		#region Miscellaneous Code

		private async Task DisplayPageAsync(string url)
		{
			//await new BrowserFetcher().DownloadAsync();
			List<string> programmerLinks = new List<string>();

			var options = new LaunchOptions()
			{
				Headless = false,
				ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
				SlowMo = 10
			};

			var browser = await Puppeteer.LaunchAsync(options, null);
			//var page1 = await browser.NewPageAsync();
			//await page1.GoToAsync(fullUrl);

			// Open a new tab
			var page = await browser.NewPageAsync();

			// Navigate to the LinkedIn login page
			await page.GoToAsync(url);
			// Find more ayat button

			var content = page.GetContentAsync();
			//var tag1 = await page.QuerySelectorAsync("#surah > option:nth-child(5)");
			var tag1 = await page.QuerySelectorAsync("#surah > option");
			//var tags = await page.QuerySelectorAllAsync("#surah > option");
			var tags = await page.QuerySelectorAllAsync("#surah > option");
			var tag3 = (await tag1.GetPropertyAsync("value")).RemoteObject.ToString();
			//var tag4 = (await tag1.GetPropertyAsync("value")).RemoteObject.Description;
			var tag5 = (await tag1.GetPropertyAsync("value")).RemoteObject.Type.ToString();
			var tag6 = (await tag1.GetPropertyAsync("value")).RemoteObject.Value.ToString();
			var tag7 = (await tag1.GetPropertyAsync("value")).JsonValueAsync().Result.ToString();
			var tag8 = tag1.JsonValueAsync().Result.ToString();
			var tag9 = await page.QuerySelectorAllHandleAsync("#surah > option");

			foreach (var tag in tags)
			{
				var one = await tag.GetPropertiesAsync();
				foreach (var item in one)
				{
					Debug.WriteLine($"value of {0} is {1} ", item.Key, item.Value);
				}
				//Debug.WriteLine(tag.GetPropertiesAsync())
				Debug.WriteLine("The name of surah is   " + (await tag.GetPropertyAsync("value")).JsonValueAsync().Result.ToString());
				Debug.WriteLine("The name of surah is   " + (await tag.GetPropertyAsync("value")).RemoteObject.ToString());
				Debug.WriteLine("The name of surah is   " + (await tag.GetPropertyAsync("value")).RemoteObject.Value.ToString());
				sSuraNames.Add((await tag.GetPropertyAsync("value")).JsonValueAsync().Result.ToString());

				//Debug.WriteLine(links);
			}
			var btn = await page.QuerySelectorAsync("button[id=moreAyaatBtn]");//button.btn
			Debug.WriteLine((await btn.GetPropertyAsync("Text")).RemoteObject.Value.ToString());
			Debug.WriteLine((await btn.GetPropertyAsync("innerText")).RemoteObject.Value.ToString());
			//while ((await tag.GetPropertyAsync("innerText")).RemoteObject.Value.ToString() != "No More")
			{

				while ((await btn.GetPropertyAsync("innerText")).RemoteObject.Value.ToString() == "More Ayat")
				{
					await btn.ClickAsync();
					//await page.WaitForNavigationAsync();
					btn = await page.QuerySelectorAsync("button.btn");
				}
				//await tag.ClickAsync();
				await page.WaitForNavigationAsync();
				//tag = await page.QuerySelectorAsync("a.next-parah-o");
				btn = await page.QuerySelectorAsync("button.btn");
			}
			//await btn.ClickAsync();
			//content.RunSynchronously
			Debug.WriteLine(content.Result);
			bool result = content.Result.Contains("moreAyaatBtn");
			// Fill in the email field
			//await page.TypeAsync("input[id=username]", "doctor_suhaib@hotmail.com");

			// Fill in the password field
			//await page.TypeAsync("input[name=session_password]", "your_password");

			// Click the login button
			await page.ClickAsync("a.next-parah-o");//button[type=submit]

			await page.WaitForNavigationAsync();

			//await page.GoToAsync("https://www.linkedin.com/notifications");
		}

		#endregion
	}

	namespace MyNamespace
	{
		namespace MyNamespace2
		{
			namespace MyNamespace4
			{

			}
		}
		namespace MyNamespace3
		{

		}
	}
	namespace Quran
	{
		namespace Surah
		{
			internal class SurahData
			{
				public int SurahID { get; set; }
				public string SurahNameAr { get; set; }
				public string SurahNameEn { get; set; }
				public int RevelationID { get; set; }
				public int RukuCount { get; set; }
				public int AyaCount { get; set; }
				public string SurahLocation { get; set; }
				public List<AyaData> Ayas { get; set; }
				public SurahData()
				{
					Ayas = new List<AyaData>();
				}
			}
		}
		namespace Aya
		{
			internal class AyaData
			{
				public int SurahID;
				public string SurahName;
				public int AyaID;
				public string Aya;
				public string KIman;
				public string KIrfan;
				public List<string> sTafseer;
				//private List<Tuple<string , FontFamily >> Tafsir;
				public List<(string Data, FontFamily Font)> Tafseer;// = new List<(string Data, FontFamily Font)>();
				public List<(string Data, string Font)> Tafsir; // Using tuple to save text and font of text

				public AyaData()
				{
					sTafseer = new List<string>();
					Tafseer = new List<(string Data, FontFamily Font)>();
					Tafsir = new List<(string Data, string Font)>();
				}
				public void AddTafseer(string data, string font) => Tafseer.Add((data, new FontFamily(font)));
				public void AddTafsir(string data, string font) => Tafsir.Add((data, font));

			}
		}
	}
}
