using HtmlAgilityPack;
using PuppeteerSharp;
using ScraperPuppet.Quran.Aya;
using ScraperPuppet.Quran.Surah;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScraperPuppet
{

	public partial class Form1 : Form
	{
		HttpClient httpClient = new HttpClient();
		private List<string> sSuraNames = new List<string>();
		public Form1()
		{
			InitializeComponent();
			cm();
		}

		long startMemory;

		public void gm()
		{

			long endMemory = GC.GetTotalMemory(true);
			Console.WriteLine("Heap memory  = " + endMemory.ToString());
			Debug.WriteLine("No. of bytes since last cm = " + (endMemory - startMemory));
			Debug.WriteLine("No. of pages since last cm = " + (endMemory - startMemory) / 8192);
		}
		public void cm()
		{
			Console.WriteLine("Start memory before clearing = " + startMemory.ToString());
			startMemory = GC.GetTotalMemory(true);
			Console.WriteLine("New Start memory set to current heap = " + startMemory.ToString());
		}
		private async void button1_Click(object sender, EventArgs e)
		{

			string fullUrl = "https://alqurankarim.net/surah-al-mursalat";
			Stopwatch sw = Stopwatch.StartNew();

			sw.Start();
			//cm();
			//gm();
			//Console.WriteLine(startMemory.ToString());
			//sSuraNames = await GetSuraNamesPuppetAsync(fullUrl);
			//sSuraNames = await GetSuraNamesAsync(fullUrl);
			sSuraNames = GetSuraNames(httpClient, fullUrl);
			//gm();
			//await DisplayPage();
			//await DisplayPage(fullUrl);
			//await DisplayPageAsync(fullUrl);

			//foreach (string item in sSuraNames)
			//{
			//	Debug.WriteLine(item);
			//}
			//GetSurah(httpClient, 106, sSuraNames[110], false);
			//string sAya = await GetAyaDataPuppetAsync(sSuraNames[2], 10);
			//AyaData sAya = await GetAyaDataAsync(sSuraNames[2], 10);
			//gm();
			for (int i = 1; i < sSuraNames.Count; i++)
			{
				//await GetSurahAsync(httpClient, i, sSuraNames[i]);
				GetSurah(httpClient, i, sSuraNames[i], true);
			}
			sw.Stop();
			Console.WriteLine(sw.Elapsed);
			gm();
		}

		#region Code using Puppeteer Sharp

		private async Task<List<string>> GetSuraNamesPuppetAsync(string url)
		{
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
		#region Async Methods

		public async Task<List<string>> GetSuraNamesAsync(HttpClient client, string url)
		{
			string response = await CallUrlAsync(client, url);
			//gm();
			var sSuraNames = ParseHtml(response);
			//gm();
			return sSuraNames;
		}
		private static async Task<string> CallUrlAsync(HttpClient client, string fullUrl)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			client.DefaultRequestHeaders.Accept.Clear();
			var response = client.GetStringAsync(fullUrl);
			return await response;
		}
		private async Task GetSurahAsync(HttpClient client, int surahID, string surahName)
		{
			SurahData surahData = new SurahData();
			surahData.SurahID = surahID;
			//Gets aya count
			//string aya = "10";
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();

			string url = $"https://alqurankarim.net/{surahName}";
			string response = await CallUrlAsync(client, url);
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(response);
			surahData.SurahNameEn = surahName.Substring(6, surahName.Length - 6);
			GetSurahData(htmlDoc, ref surahData);
			GetSurahLocation(htmlDoc, ref surahData);
			#region Synchronous
			//long startMemory = GC.GetTotalMemory(true);
			//ayaData.sIntro = null;
			//ayaData.sTafseer = null;
			//ayaData.IntroSegments = null;
			//ayaData.TafseerSegments = null;

			for (int i = 1; i <= surahData.AyaCount; i++)
			{
				surahData.Ayas.Add(await GetAyaDataAsync(client, surahData.SurahID, surahName, i));
				/*
								surahData.Ayas[surahData.Ayas.Count - 1].sIntro = null;
								surahData.Ayas[surahData.Ayas.Count - 1].sTafseer = null;
								surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegments = null;
								surahData.Ayas[surahData.Ayas.Count - 1].IntroSegments = null;

								surahData.Ayas[surahData.Ayas.Count - 1].Intro = null;
								surahData.Ayas[surahData.Ayas.Count - 1].IntroSegmentedTag = null;
								surahData.Ayas[surahData.Ayas.Count - 1].IntroSegmentedText = null;
								surahData.Ayas[surahData.Ayas.Count - 1].IntroSegmentedXml = null;
								surahData.Ayas[surahData.Ayas.Count - 1].IntroTag = null;
								surahData.Ayas[surahData.Ayas.Count - 1].IntroXml = null;
								surahData.Ayas[surahData.Ayas.Count - 1].Tafseer = null;
								surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegmentedTag = null;
								surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegmentedText = null;
								surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegmentedXml = null;
								surahData.Ayas[surahData.Ayas.Count - 1].TafseerTag = null;
								surahData.Ayas[surahData.Ayas.Count - 1].TafseerXml = null;
				*/
			}

			//surahData.Ayas = null;
			//surahData = null;
			Console.WriteLine("Completed");
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
		private async Task<AyaData> GetAyaDataAsync(HttpClient client, int surahId, string surahName, int aya)
		{
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();
			ayaData.SurahID = surahId;
			ayaData.SurahName = surahName;
			ayaData.AyaID = aya;
			string url = $"https://alqurankarim.net/ur/{surahName}/ayat-{aya}/translation/tafsir";
			string response = await CallUrlAsync(client, url);
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(response);
			ayaData.KIrfan = await GetAyaKIrfanAsync(htmlDoc);

			//var sAyaTask = GetAya(surahName, aya, htmlDoc);

			ayaData.Aya = await GetAyaAsync(htmlDoc);
			ayaData.KIman = await GetAyaKImanAsync(htmlDoc);

			GetAyaTafseer(htmlDoc, ref ayaData);

			//await Task.WhenAny(sAyaTask);
			return ayaData;
		}
		private async Task<string> GetAyaAsync(HtmlAgilityPack.HtmlDocument htmlDocument)
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

		#endregion

		public List<string> GetSuraNames(HttpClient client, string url)
		{
			string response = CallUrl(client, url);
			//gm();
			var sSuraNames = ParseHtml(response);
			//gm();
			return sSuraNames;
		}

		private string CallUrl(HttpClient client, string fullUrl)
		{
			//gm();
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			//client.DefaultRequestHeaders.Accept.Clear();
			//gm();
			var response = client.GetStringAsync(fullUrl).Result;
			//gm();
			return response;
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

		private void GetSurah(HttpClient client, int surahID, string surahName, bool outputFile = true)
		{
			SurahData surahData = new SurahData();
			surahData.SurahID = surahID;
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();
			StringBuilder sbAlQuranXml = new StringBuilder();
			StringBuilder sbAlQuranSegmentedXml = new StringBuilder();
			StringBuilder sbIntroXml = new StringBuilder();
			StringBuilder sbIntroSeqmentedXml = new StringBuilder();
			StringBuilder sbSurahTafseerXml = new StringBuilder();
			StringBuilder sbTafseerXml = new StringBuilder();
			StringBuilder sbTafseerSegmentedXml = new StringBuilder();

			StringBuilder sbAlQuran = new StringBuilder();
			StringBuilder sbAlQuranSegmented = new StringBuilder();
			StringBuilder sbIntro = new StringBuilder();
			StringBuilder sbIntroSegmented = new StringBuilder();
			StringBuilder sbTafseer = new StringBuilder();
			StringBuilder sbTafseerSegmented = new StringBuilder();
			StringBuilder sbAlQuranAya = new StringBuilder();
			StringBuilder sbKanzulIman = new StringBuilder();
			StringBuilder sbKanzulIrfan = new StringBuilder();

			string url = $"https://alqurankarim.net/{surahName}";
			string response = CallUrl(client, url);
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(response);
			surahData.SurahNameEn = surahName.Substring(6, surahName.Length - 6);
			GetSurahData(htmlDoc, ref surahData);
			GetSurahLocation(htmlDoc, ref surahData);

			#region Synchronous
			for (int i = 1; i <= surahData.AyaCount; i++)
			{
				//surahData.Ayas.Add(await GetAyaDataAsync(client, surahData.SurahID, surahName, i));
				surahData.Ayas.Add(GetAya(client, surahData.SurahID, surahName, i, false));
				/*
					surahData.Ayas[surahData.Ayas.Count - 1].sIntro = null;
					surahData.Ayas[surahData.Ayas.Count - 1].sTafseer = null;
					surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegments = null;
					surahData.Ayas[surahData.Ayas.Count - 1].IntroSegments = null;

					surahData.Ayas[surahData.Ayas.Count - 1].Intro = null;
					surahData.Ayas[surahData.Ayas.Count - 1].IntroSegmentedTag = null;
					surahData.Ayas[surahData.Ayas.Count - 1].IntroSegmentedText = null;
					surahData.Ayas[surahData.Ayas.Count - 1].IntroSegmentedXml = null;
					surahData.Ayas[surahData.Ayas.Count - 1].IntroTag = null;
					surahData.Ayas[surahData.Ayas.Count - 1].IntroXml = null;
					surahData.Ayas[surahData.Ayas.Count - 1].Tafseer = null;
					surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegmentedTag = null;
					surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegmentedText = null;
					surahData.Ayas[surahData.Ayas.Count - 1].TafseerSegmentedXml = null;
					surahData.Ayas[surahData.Ayas.Count - 1].TafseerTag = null;
					surahData.Ayas[surahData.Ayas.Count - 1].TafseerXml = null;

				*/
			}
			//surahData.Ayas = null;
			//surahData = null;

			Console.WriteLine("Completed");
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
			/*			
				var ayaTasks = new List<Task<AyaData>>();
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

			if (outputFile)
			{
				//sbAlQuranXml.Append($"\t<quran>\n");
				sbAlQuranXml.Append($"\t<surah id =\"surah_{surahData.SurahID}\">\n");
				//sbAlQuranSegmentedXml.Append($"\t<qurah>\n");
				sbAlQuranSegmentedXml.Append($"\t<surah id =\"surah_{surahData.SurahID}\">\n");

				sbAlQuranSegmentedXml.Append(surahData.Ayas[0].IntroSegmentedXml);
				sbIntroXml.Append(surahData.Ayas[0].IntroTag);
				sbIntroSeqmentedXml.Append(surahData.Ayas[0].IntroSegmentedTag);
				sbSurahTafseerXml.Append(surahData.Ayas[0].IntroTag);

				sbAlQuran.Append(surahData.Ayas[0].Intro);
				sbAlQuranSegmented.Append(surahData.Ayas[0].IntroSegmentedText);
				sbIntro.Append(surahData.Ayas[0].Intro);
				sbIntroSegmented.Append(surahData.Ayas[0].IntroSegmentedText);
				sbAlQuranXml.Append($"\t\t<ayaat count =\"{surahData.AyaCount}\">\n");
				sbAlQuranSegmentedXml.Append($"\t\t <ayaat count =\"{surahData.AyaCount}\">\n");
				for (int i = 0; i < surahData.AyaCount; i++)
				{
					sbAlQuranXml.Append(surahData.Ayas[i].Xml);
					sbAlQuranSegmentedXml.Append(surahData.Ayas[i].XmlSegmented);
					sbSurahTafseerXml.Append(surahData.Ayas[i].TafseerTag);
					sbSurahTafseerXml.Append(surahData.Ayas[i].TafseerTag);
					sbTafseerSegmentedXml.Append(surahData.Ayas[i].TafseerSegmentedTag);

					sbAlQuran.Append(surahData.Ayas[i].Aya);
					sbAlQuran.Append(surahData.Ayas[i].KIman);
					sbAlQuran.Append(surahData.Ayas[i].KIrfan);
					sbAlQuran.Append(surahData.Ayas[i].Tafseer);
					sbAlQuranSegmented.Append(surahData.Ayas[i].TafseerSegmentedText);
					sbTafseer.Append(surahData.Ayas[i].Tafseer);
					sbTafseerSegmented.Append(surahData.Ayas[i].TafseerSegmentedText);
					sbAlQuranAya.Append(surahData.Ayas[i].Aya);
					sbKanzulIman.Append(surahData.Ayas[i].KIman);
					sbKanzulIrfan.Append(surahData.Ayas[i].KIrfan);
				}
				sbAlQuranXml.Append($"\t\t</ayaat>\n");
				sbAlQuranXml.Append($"\t</surah>\n");
				//sbAlQuranXml.Append($"\t</quran>\n");

				sbAlQuranSegmentedXml.Append($"\t\t</ayaat>\n");
				sbAlQuranSegmentedXml.Append($"\t</surah>\n");
				//sbAlQuranSegmentedXml.Append($"\t</qurah>\n");

				#region File writing area.

				File.AppendAllText("Output Files\\AlQuran.xml", sbAlQuranXml.ToString()); // Will be returned to parent method for proper placement in file.
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", sbAlQuranSegmentedXml.ToString());
				File.AppendAllText("Output Files\\Intro.xml", sbIntroXml.ToString());
				File.AppendAllText("Output Files\\IntroSegmented.xml", sbIntroSeqmentedXml.ToString());
				File.AppendAllText("Output Files\\SurahTafseer.xml", sbSurahTafseerXml.ToString());
				File.AppendAllText("Output Files\\Tafseer.xml", sbTafseerXml.ToString());
				File.AppendAllText("Output Files\\TafseerSegmented.xml", sbTafseerSegmentedXml.ToString());

				File.AppendAllText("Output Files\\AlQuran.txt", sbAlQuran.ToString());
				File.AppendAllText("Output Files\\AlQuranSegmented.txt", sbAlQuranSegmented.ToString());
				File.AppendAllText("Output Files\\Intro.txt", sbIntro.ToString());
				File.AppendAllText("Output Files\\IntroSegmented.txt", sbIntroSegmented.ToString());
				File.AppendAllText("Output Files\\Tafseer.txt", sbTafseer.ToString());
				File.AppendAllText("Output Files\\TafseerSegmented.txt", sbTafseerSegmented.ToString());
				File.AppendAllText("Output Files\\AlQuranAya.txt", sbAlQuranAya.ToString());
				File.AppendAllText("Output Files\\KanzulIman.txt", sbKanzulIman.ToString());
				File.AppendAllText("Output Files\\KanzulIrfan.txt", sbKanzulIrfan.ToString());

				#endregion

			}
		}

		private void GetSurahData(HtmlAgilityPack.HtmlDocument htmlDocument, ref SurahData surahData)
		{
			// UPDATE
			// Swap surah method names.
			// Add continuous aya id index.
			// Add surah data to output like surah location and revelation ID.
			/*
			//*[@id="araayat"]/div/div[1]/table/tbody/tr[2]/td[1]/span/br
			//*[@id="araayat"]/div[1]/table/tbody/tr[2]/td[1]/span/br
			//html/body/div/section[2]/div/div[1]/div[2]/div[2]/div[2]/div[1]/table/tbody/tr[2]/td[1]/span/br
			//document.querySelector("#araayat > div:nth-child(1) > table > tbody > tr:nth-child(2) > td.two__row__td_one > span > br")
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

			var nodesArray = nodes.ToArray();

			string[] elements = nodesArray[0].InnerText.Split('\n');
			surahData.AyaCount = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			//Debug.WriteLine(surahData.AyaCount);

			elements = nodesArray[1].InnerText.Split('\n');
			surahData.SurahID = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			//Debug.WriteLine("Surah ID = " + surahData.SurahID);

			elements = nodesArray[2].InnerText.Split('\n');
			surahData.SurahNameAr = elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim();
			Debug.WriteLine("Surah name = " + surahData.SurahNameAr);

			elements = nodesArray[3].InnerText.Split('\n');
			surahData.RevelationID = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			//Debug.WriteLine("Revelation ID = " + surahData.RevelationID);

			elements = nodesArray[4].InnerText.Split('\n');
			surahData.RukuCount = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			//Debug.WriteLine("Surah has rukus = " + surahData.RukuCount);
		}
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
			//Debug.WriteLine(surahData.SurahLocation);
		}
		private AyaData GetAya(HttpClient client, int surahId, string surahName, int aya, bool outputFile = false)
		{
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();
			ayaData.SurahID = surahId;
			ayaData.SurahName = surahName;
			ayaData.AyaID = aya;
			string url = $"https://alqurankarim.net/ur/{surahName}/ayat-{aya}/translation/tafsir";
			string response = CallUrl(client, url);
			// Navigates to page.	#araayat > div > div.col-lg-12.col-12 > div.row > div > p     document.querySelector("#araayat > div > div.col-lg-12.col-12 > div.row > div > p")
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(response);

			GetAyaText(htmlDoc, ayaData, false);
			GetAyaKIman(htmlDoc, ref ayaData, false);
			GetAyaKIrfan(htmlDoc, ref ayaData, false);
			GetAyaTafseer(htmlDoc, ref ayaData, false);

			if (outputFile)
			{

				if (ayaData.AyaID == 1)
				{

					File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Intro);
					File.AppendAllText("Output Files\\AlQuranSegmented.txt", ayaData.IntroSegmentedText);

					File.AppendAllText("Output Files\\AlQuran.xml", ayaData.IntroXml); // Will be returned to parent method for proper placement in file.
					File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.IntroSegmentedXml);

				}

				File.AppendAllText("Output Files\\AlQuranAya.txt", ayaData.Aya);
				File.AppendAllText("Output Files\\KanzulIman.txt", ayaData.KIman);
				File.AppendAllText("Output Files\\KanzulIrfan.txt", ayaData.KIrfan);

				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Aya);
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.KIman);
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.KIrfan);

				File.AppendAllText("Output Files\\AlQuran.xml", ayaData.Xml);
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.XmlSegmented);

				if (ayaData.AyaID == 1)
				{
					File.AppendAllText("Output Files\\Intro.txt", ayaData.Intro);
					File.AppendAllText("Output Files\\Intro.xml", ayaData.IntroTag);
					File.AppendAllText("Output Files\\SurahTafseer.xml", ayaData.IntroTag);
					File.AppendAllText("Output Files\\IntroSegmented.txt", ayaData.IntroSegmentedText);
					File.AppendAllText("Output Files\\IntroSegmented.xml", ayaData.IntroSegmentedTag);
				}
				File.AppendAllText("Output Files\\Tafseer.txt", ayaData.Tafseer);
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Tafseer);

				File.AppendAllText("Output Files\\Tafseer.xml", ayaData.TafseerTag);
				File.AppendAllText("Output Files\\SurahTafseer.xml", ayaData.TafseerTag);

				File.AppendAllText("Output Files\\TafseerSegmented.txt", ayaData.TafseerSegmentedText);
				File.AppendAllText("Output Files\\AlQuranSegmented.txt", ayaData.TafseerSegmentedText);

				File.AppendAllText("Output Files\\TafseerSegmented.xml", ayaData.TafseerSegmentedTag);
			}
			return ayaData;
		}
		private void GetAyaText(HtmlAgilityPack.HtmlDocument htmlDocument, AyaData ayaData, bool outputFile = false)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			var node0 = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div/div[1]/div[2]/div/p");
			var node1 = htmlDocument.DocumentNode.Descendants("p").Where(p => p.GetAttributeValue("class", "").Contains("arabic")).First();
			//Debug.WriteLine(node.InnerText);
			//return await Task.FromResult(node.InnerText);

			//ayaData.Aya = node1.InnerText;
			string[] aAya = node1.InnerText.Split(')');
			ayaData.AyaCount = aAya.Length - 1;
			ayaData.AyaIndex = Array.FindIndex(aAya, str => str.Contains(ayaData.AyaID.ToString()));
			string sAya = aAya[ayaData.AyaIndex] + ")";
			ayaData.Aya = sAya;
			ayaData.AyaXml = $"\t\t\t\t<ayatext id=\"arabic_{ayaData.SurahID}.{ayaData.AyaID}\">\n\t\t\t\t\t{ayaData.Aya}\n\t\t\t\t</ayatext>\n";
			//textBox1.Text += ayaData.Aya;
			if (outputFile)
			{
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Aya);
				File.AppendAllText("Output Files\\AlQuranAya.txt", ayaData.Aya);

				File.AppendAllText("Output Files\\AlQuran.xml", ayaData.AyaXml);
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.AyaXml);
			}
		}
		private void GetAyaKIman(HtmlAgilityPack.HtmlDocument htmlDocument, ref AyaData ayaData, bool outputFile = false)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div/div[1]/span[2]");
			var nodes = htmlDocument.DocumentNode.Descendants("span").Where(p => p.GetAttributeValue("class", "").Contains("ur"));
			List<string> lTranslation = new List<string>();
			foreach (var item in nodes)
			{
				lTranslation.Add(item.InnerText.Trim());
			}
			string[] aTranslation = new string[lTranslation.Count];
			aTranslation = lTranslation.ToArray();
			var index = Array.FindIndex(aTranslation, str => str.Contains("کنزالایمان"));

			//var index = Array.FindIndex(aTranslation, str => str.Contains("کنزالعرفان"));
			var sTranslation = aTranslation[ayaData.AyaIndex + index + 1];
			//Debug.WriteLine(node.InnerText);
			//ayaData.KIman = node.InnerText.Trim();
			ayaData.KIman = sTranslation;
			ayaData.KImanXml = $"\t\t\t\t<kiman id=\"urdu_1_{ayaData.SurahID}.{ayaData.AyaID}\">\n\t\t\t\t\t{ayaData.KIman}\n\t\t\t\t</kiman>\n";
			//textBox1.Text += ayaData.KIman;
			if (outputFile)
			{
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.KIman);
				File.AppendAllText("Output Files\\KanzulIman.txt", ayaData.KIman);
				File.AppendAllText("Output Files\\AlQuran.xml", ayaData.KImanXml);
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.KImanXml);
			}
		}
		private void GetAyaKIrfan(HtmlAgilityPack.HtmlDocument htmlDocument, ref AyaData ayaData, bool outputFile = false)
		{
			// UPDATE: CHANGE CODE TO USE DESCENDANTS INSTEAD OF SELECTNODES.
			var node = htmlDocument.DocumentNode.SelectSingleNode("//*[@id=\"araayat\"]/div/div[1]/span[4]");
			//Debug.WriteLine(node.InnerText);
			var nodes = htmlDocument.DocumentNode.Descendants("span").Where(p => p.GetAttributeValue("class", "").Contains("ur"));
			List<string> lTranslation = new List<string>();
			foreach (var item in nodes)
			{
				lTranslation.Add(item.InnerText.Trim());
			}
			string[] aTranslation = new string[lTranslation.Count];
			aTranslation = lTranslation.ToArray();
			//var index = Array.FindIndex(aTranslation, str => str.Contains("کنزالایمان"));

			var index = Array.FindIndex(aTranslation, str => str.Contains("کنزالعرفان"));
			var sTranslation = aTranslation[ayaData.AyaIndex + index + 1];
			//Debug.WriteLine(node.InnerText);
			ayaData.KIrfan = sTranslation;
			ayaData.KIrfanXml = $"\t\t\t\t<kirfam id=\"urdu_2_{ayaData.SurahID}.{ayaData.AyaID}\">\n\t\t\t\t\t{ayaData.KIrfan}\n\t\t\t\t</kirfan>\n";
			if (outputFile)
			{
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.KIrfan);
				File.AppendAllText("Output Files\\KanzulIrfan.txt", ayaData.KIrfan);
				File.AppendAllText("Output Files\\AlQuran.xml", ayaData.KIrfanXml);
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.KIrfanXml);
			}
		}
		private void GetAyaTafseer(HtmlAgilityPack.HtmlDocument htmlDocument, ref AyaData ayaData, bool outputFile = false)
		{

			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			StringBuilder sb3 = new StringBuilder();
			List<string> sAya = new List<string>();
			int iSegmentID = 0;
			//Each node has 2 attributes, style and lang.
			//style gives font size and fontfamily while l
			//ang gives language, which here is Arabic from Saudi Arabia.
			// Using descendents is 6000 times faster than SelectNodes.
			//var nodes0 = htmlDocument.DocumentNode.SelectNodes("//*[@id=\"araayat\"]/div/div[1]/div[3]/p");//Gives tafseer but no intro, Slower
			//var nodes1 = htmlDocument.DocumentNode.SelectNodes("//span[contains(@lang,'AR-SA')]");//Gives tafseer and intro segments, Slower
			var segnodes = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("lang", "").Contains("A")); //Returns segments of tafseer and intro.
			var nodes = htmlDocument.DocumentNode.Descendants("p").Where(node => node.GetAttributeValue("style", "").Contains("rtl")); //Returns intro and tafseer

			var segnodesTafseer = segnodes.Where(node => node.ParentNode.GetAttributeValue("dir", "").Contains("RTL")); //Returns segments of tafseer.
			var nodesTafseer = nodes.Where(node => node.GetAttributeValue("dir", "").Contains("RTL"));//Returns tafseer

			if (ayaData.AyaID == 1)
			{
				var segnodesIntro = segnodes.Where(node => node.ParentNode.GetAttributeValue("dir", "").Contains("rtl")); //Returns segments of intro.
				var nodesIntro = nodes.Where(node => node.GetAttributeValue("dir", "").Contains("rtl")); //Returns intro

				#region Code for Intro lines.

				// Stores list of intro line.
				foreach (var node in nodesIntro)
				{
					//ayaData.sIntro.Add(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '));
					ayaData.sIntro.Add(Regex.Replace(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '), @"\s+", " "));

				}

				sb.Clear(); sb2.Clear(); sb3.Clear();
				iSegmentID = 0;
				sb3.Append($"\t\t<intro id=\"intro_{ayaData.SurahID}\">\n");
				foreach (var node in ayaData.sIntro)
				{
					iSegmentID++;
					sb.Append($"{node}. "); // For ayaData.Intro, Intro.txt.
					sb2.Append($"\t{node}\n"); // For Intro.Xml.
					sb3.Append($"\t\t\t\t\t\t<segment id=\"intro_{ayaData.SurahID}.{iSegmentID}\">\n");
					sb3.Append($"\t\t\t\t\t\t\t{node}\n"); // For ayaData.IntroXml, AlQuran.Xml
					sb3.Append($"\t\t\t\t\t\t</segment>\n");
				}
				sb3.Append($"\t\t</intro>\n");

				ayaData.Intro = sb.ToString();
				ayaData.IntroTag = $"<intro id=\"intro_{ayaData.SurahID}\">\n{sb2}</intro>\n";
				ayaData.IntroXml = sb3.ToString();
				if (outputFile)
				{
					File.AppendAllText("Output Files\\Intro.txt", ayaData.Intro);
					File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Intro);

					File.AppendAllText("Output Files\\Intro.xml", ayaData.IntroTag);
					File.AppendAllText("Output Files\\SurahTafseer.xml", ayaData.IntroTag);
					File.AppendAllText("Output Files\\AlQuran.xml", ayaData.IntroXml); // Will be returned to parent method for proper placement in file.
				}
				#endregion

				#region Code for Intro Segments

				// Stores intro segments.
				sb.Clear(); sb2.Clear(); sb3.Clear();
				iSegmentID = 0;
				sb3.Append($"\t\t<intro id=\"intro_{ayaData.SurahID}\">\n");
				foreach (var segment in ayaData.IntroSegments)
				{
					if (segment.Data.Contains(';'))
					{
						MessageBox.Show("Intro segment contains \";\"", "ERROR IN INTRO SEGMENT");
					}
					iSegmentID++;
					sb.Append($"Intro;{ayaData.SurahID}.{iSegmentID};{segment.Data};{segment.Font};{segment.Size}\n"); //for ayaData.TafseerSegmentedText and TafseerSegmented.txt
					sb2.Append($"\t{segment.Data}\n");// For IntroSegmented.Xml
					sb3.Append($"\t\t\t\t\t\t<segment id=\"isegment_{ayaData.SurahID}.{iSegmentID}\">\n");
					sb3.Append($"\t\t\t\t\t\t\t<language id=\"lang_{segment.LangID}\">{segment.Lang}</language>\n");
					sb3.Append($"\t\t\t\t\t\t\t<font id=\"font_{segment.FontID}\">{segment.Font}</font>\n");
					sb3.Append($"\t\t\t\t\t\t\t<fontsize>{segment.Size}</fontsize>\n");
					sb3.Append($"\t\t\t\t\t\t\t<text id=\"itext_{ayaData.SurahID}.{iSegmentID}\">{segment.Data}</text>\n");
					sb3.Append($"\t\t\t\t\t\t</segment>\n");
				}
				sb3.Append($"\t\t</intro>\n");

				ayaData.IntroSegmentedText = sb.ToString();
				ayaData.IntroSegmentedTag = $"<intro id=\"intro_{ayaData.SurahID}\">\n{sb2}</intro>\n";
				ayaData.IntroSegmentedXml = sb3.ToString();

				if (outputFile)
				{
					File.AppendAllText("Output Files\\IntroSegmented.txt", ayaData.IntroSegmentedText);
					File.AppendAllText("Output Files\\AlQuranSegmented.txt", ayaData.IntroSegmentedText);

					File.AppendAllText("Output Files\\IntroSegmented.xml", ayaData.IntroSegmentedTag);
					File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.IntroSegmentedXml);
				}
				#endregion

			}

			#region Code for tafseer lines.

			// Stores list of tafseer line.
			foreach (var node in nodesTafseer)
			{

				//ayaData.sTafseer.Add(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '));
				ayaData.sTafseer.Add(Regex.Replace(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '), @"\s+", " "));
				//sInnerText = Regex.Replace(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '), @"\s+", " ");

			}
			sb.Clear(); sb2.Clear(); sb3.Clear();
			iSegmentID = 0;
			sb3.Append($"\t\t\t\t\t<tafseer id=\"tafseer_{ayaData.SurahID}.{ayaData.AyaID}\">\n");
			foreach (var node in ayaData.sTafseer)
			{
				iSegmentID++;
				sb.Append($"{node}. "); // For ayaData.Tafseer, Tafseer.txt
				sb2.Append($"\t{node}\n"); // For Tafseer.Xml
				sb3.Append($"\t\t\t\t\t\t<segment id=\"tafseer_{ayaData.SurahID}.{ayaData.AyaID}.{iSegmentID}\">\n");
				sb3.Append($"\t\t\t\t\t\t\t{node}\n"); // For ayaData.TafseerXml, AlQuran.Xml
				sb3.Append($"\t\t\t\t\t\t</segment>\n");
			}
			sb3.Append($"\t\t\t\t\t</tafseer>\n");

			ayaData.Tafseer = sb.ToString();
			ayaData.TafseerTag = $"<tafseer id=\"tafseer_{ayaData.SurahID}.{ayaData.AyaID}\">\n{sb2}</tafseer>\n";
			ayaData.TafseerXml = sb3.ToString();
			if (outputFile)
			{
				File.AppendAllText("Output Files\\Tafseer.txt", ayaData.Tafseer);
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Tafseer);

				File.AppendAllText("Output Files\\Tafseer.xml", ayaData.TafseerTag);
				File.AppendAllText("Output Files\\SurahTafseer.xml", ayaData.TafseerTag);
				File.AppendAllText("Output Files\\AlQuran.xml", ayaData.TafseerXml);
			}
			#endregion

			#region Code for tafseer segments.
			//before extracting method.
			// Stores tafseer segments.
			ExtractSegments(ayaData.AddTafseer, ayaData, segnodesTafseer);
			//Before Extracting methode.
			iSegmentID = 0;
			sb.Clear(); sb2.Clear(); sb3.Clear();
			sb3.Append($"\t\t\t\t\t<tafseer id=\"tafseer_{ayaData.SurahID}.{ayaData.AyaID}\">\n");
			foreach (var segment in ayaData.TafseerSegments)
			{
				if (segment.Data.Contains(';'))
				{
					MessageBox.Show("Tafseer segment contains \";\"", "ERROR IN TAFSEER SEGMENT");
				}
				iSegmentID++;
				sb.Append($"Tafseer;{ayaData.SurahID}.{ayaData.AyaID}.{iSegmentID};{segment.Data};{segment.Font};{segment.Size}\n"); //for ayaData.TafseerSegmentedText and TafseerSegmented.txt

				sb2.Append($"\t{segment.Data}\n");// For TafseerSegmented.Xml
				sb3.Append($"\t\t\t\t\t\t<segment id=\"tsegment_{ayaData.SurahID}.{ayaData.AyaID}.{iSegmentID}\">\n");
				sb3.Append($"\t\t\t\t\t\t\t<language id=\"lang_{segment.LangID}\">{segment.Lang}</language>\n");
				sb3.Append($"\t\t\t\t\t\t\t<font id=\"font_{segment.FontID}\">{segment.Font}</font>\n");
				sb3.Append($"\t\t\t\t\t\t\t<fontsize>{segment.Size}</fontsize>\n");
				sb3.Append($"\t\t\t\t\t\t\t<text id=\"ttext_{ayaData.SurahID}.{ayaData.AyaID}.{iSegmentID}\">{segment.Data}</text>\n");
				sb3.Append($"\t\t\t\t\t\t</segment>\n");

			}
			sb3.Append($"\t\t\t\t\t</tafseer>\n");
			sb3.Append($"\t\t\t\t</ayah>\n");

			ayaData.TafseerSegmentedText = sb.ToString();
			ayaData.TafseerSegmentedTag = $"<tafseer id=\"tafseer_{ayaData.SurahID}.{ayaData.AyaID}\">\n{sb2}</tafseer>\n";
			ayaData.TafseerSegmentedXml = sb3.ToString();
			if (outputFile)
			{
				File.AppendAllText("Output Files\\TafseerSegmented.txt", ayaData.TafseerSegmentedText);
				File.AppendAllText("Output Files\\AlQuranSegmented.txt", ayaData.TafseerSegmentedText);

				File.AppendAllText("Output Files\\TafseerSegmented.xml", ayaData.TafseerSegmentedTag);
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.TafseerSegmentedXml);
			}
			#endregion

			ayaData.sIntro = null;
			ayaData.sTafseer = null;
			ayaData.IntroSegments = null;
			ayaData.TafseerSegments = null;
		}

		private static void ExtractSegments(AddSegmentDel addmethod, AyaData ayaData, IEnumerable<HtmlNode> segments)
		{
			string sInnerText = ""; // To store segment value.
			string sFontText; // To store style attribute value giving font details.
			string[] sFontTexts; // To store font details from style attribute.
			string sFontSize; // To store font size.
			string sFontFamily = "Times New Roman";
			string sLanguage; // To store text language.
			int iLangID; // To store language ID.
			int iFontID; // To store font ID.

			foreach (var node in segments)
			{
				//sInnerText = node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' ').Replace("  ", " ");
				sInnerText = Regex.Replace(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '), @"\s+", " ");
				//sInnerText = Regex.Replace(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '), " {2,}", " ");
				sFontText = node.GetAttributeValue("style", "No Attribute");
				sFontTexts = sFontText.Replace("&quot;", "").Replace("\r\n", "").Split(';', ':');
				var iFontIndex = Array.FindIndex(sFontTexts, str => str.Contains("family")) + 1;
				//Console.WriteLine($"Index of font = {iFontIndex.ToString()} - Font is = {sFontTexts[iFontIndex]}");
				sFontSize = sFontTexts[1].Replace("pt", "").Trim();

				if (sFontTexts.Length > 1)
				{
					sFontFamily = sFontTexts[iFontIndex].Trim();
				}
				else
				{
					Console.WriteLine($"ERROR!!! - sFontTexts Index out of range - {sFontTexts.Length.ToString()}");
					MessageBox.Show("ERROR!!! - sFontTexts Index out of range", "ERROR IN TAFSEER SEGMENT");
					sFontFamily = sFontTexts[iFontIndex].Trim();
				}

				if (sFontFamily.Contains("Noori"))
				{
					sLanguage = "UR";
					iLangID = 1;
					iFontID = 1;
				}
				else if (sFontFamily.Contains("Mushaf"))
				{
					sLanguage = "AR-SA";
					iLangID = 2;
					iFontID = 2;
				}
				else if (sFontFamily.Contains("Qalam"))
				{
					sLanguage = "AR-SA";
					iLangID = 2;
					iFontID = 3;
				}
				else if (sFontFamily.Contains("Arial"))
				{
					sLanguage = "EN";
					iLangID = 0;
					iFontID = 4;
				}
				else if (sFontFamily.Contains("Cambria"))
				{
					sLanguage = "EN";
					iLangID = 0;
					iFontID = 5;
				}
				else if (sFontFamily.Contains("Times"))
				{
					sLanguage = "EN";
					iLangID = 0;
					iFontID = 0;
				}
				else
				{
					sFontFamily = "Times New Roman";
					sLanguage = "EN";
					iLangID = 0;
					iFontID = 0;
				}
				addmethod(sInnerText, sLanguage, iLangID, sFontFamily, iFontID, sFontSize);
			}
		}

		public delegate void AddSegmentDel(string text, string language, int langid, string fontfamily, int fontid, string fontsize);
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
				public int AyaID { get; set; }
				public int AyaCount { get; set; }
				public int AyaIndex { get; set; }
				public int SurahID { get; set; }
				public string SurahName { get; set; }

				#region Output Data 

				/// <summary>
				/// A string containing Aya in arabic in single line.
				/// </summary>
				public string Aya { get; set; }
				/// <summary>
				/// A string containing Aya in arabic in xml format.
				/// </summary>
				public string AyaXml { get; set; }

				/// <summary>
				/// A string containing kanz-ul-iman translation in single line.
				/// </summary>
				public string KIman { get; set; }
				/// <summary>
				/// A string containing kanz-ul-iman translation in xml format.
				/// </summary>
				public string KImanXml { get; set; }

				/// <summary>
				/// A string containing kanz-ul-irfan translation in single line.
				/// </summary>
				public string KIrfan { get; set; }
				/// <summary>
				/// A string containing kanz-ul-irfan translation in xml format.
				/// </summary>
				public string KIrfanXml { get; set; }

				/// <summary>
				/// A string containing Aya, tanslation and tafseer in xml format.
				/// </summary>
				public string Xml
				{
					get
					{
						StringBuilder sReturnXml = new StringBuilder();
						sReturnXml.Append($"\t\t\t < ayah id =\"ayah_{SurahID}.{AyaID}\">\n");
						sReturnXml.Append(AyaXml);
						sReturnXml.Append(KImanXml);
						sReturnXml.Append(KIrfanXml);
						sReturnXml.Append(TafseerXml);
						sReturnXml.Append($"\t\t\t</ayah>\n");
						return sReturnXml.ToString();
					}
					//set;
				}
				/// <summary>
				/// A string containing Aya, translation and tafseer segments with font and font size in xml format.
				/// </summary>
				public string XmlSegmented
				{
					get
					{
						StringBuilder sReturnXml = new StringBuilder();
						sReturnXml.Append($"\t\t\t < ayah id =\"ayah_{SurahID}.{AyaID}\">\n");
						sReturnXml.Append(AyaXml);
						sReturnXml.Append(KImanXml);
						sReturnXml.Append(KIrfanXml);
						sReturnXml.Append(TafseerSegmentedXml);
						sReturnXml.Append($"\t\t\t</ayah>\n");
						return sReturnXml.ToString();
					}
					//set;
				}

				/// <summary>
				/// A string containing Intro in single line, separated by ". " full stops.
				/// Will be saved in a file. Intro.txt, AlQuran.txt
				/// </summary>
				public string Intro { get; set; }
				/// <summary>
				/// A string containing Aya intro in paragraphs in xml format.
				/// Will be saved in a file. AlQuran.xml
				/// </summary>
				public string IntroXml { get; set; }
				/// <summary>
				/// A string containing Aya intro in paragraphs with xml tags.
				/// Will be saved in a file. Intro.xml, SurahTafseer.xml
				/// </summary>
				public string IntroTag { get; set; }

				/// <summary>
				///  A string containing intro segments separated by line breaks, containing segment IDs, font and font size.
				///  Will be saved in a file. IntroSegmented.txt, AlQuranSegmented.txt
				/// </summary>
				public string IntroSegmentedText { get; set; }
				/// <summary>
				/// A string containing intro segments in xml format, containing segment IDs, font and font size.
				/// Will be saved in a file. AlQuranSegmented.xml
				/// </summary>
				public string IntroSegmentedXml { get; set; }
				/// <summary>
				/// A string containing intro segments with xml tags.
				/// Will be saved in a file. IntroSegmented.xml
				/// </summary>
				public string IntroSegmentedTag { get; set; }

				/// <summary>
				/// A string containing tafseer in single line, separated by line ". " full stops.
				/// Will be saved in a file. Tafseer.txt, AlQuran.txt
				/// </summary>
				public string Tafseer { get; set; }
				/// <summary>
				/// A string containing Aya tafseer in paragraphs with xml tags.
				/// Will be saved in a file. AlQuran.xml
				/// </summary>
				public string TafseerXml { get; set; }
				/// <summary>
				/// A string containing Aya tafseer in paragraphs with xml tags.
				/// Will be saved in a file. Tafseer.xml, SurahTafseer.xml
				/// </summary>
				public string TafseerTag { get; set; }

				/// <summary>
				///  A string containing tafseer segments separated by line breaks, containing segment IDs, font and font size.
				///  Will be saved in a file. AlQuranSegmented.txt, TafseerSegmented.txt
				/// </summary>
				public string TafseerSegmentedText { get; set; }
				/// <summary>
				/// A string containing tafseer segments in xml format, containing segment IDs, font and font size.
				/// Will be saved in a file. AlQuranSegmented.Xml
				/// </summary>
				public string TafseerSegmentedXml { get; set; }
				/// <summary>
				/// A string containing tafseer segments with xml tags.
				/// Will be saved in a file. TafseerSegmented.xml
				/// </summary>
				public string TafseerSegmentedTag { get; set; }

				#endregion

				#region Helper Data	
				/// <summary>
				/// List of intro paragraphs. Helper data structure.
				/// </summary>
				public List<string> sIntro { get; set; }
				/// <summary>
				/// List of tafseer paragraphs. Helper data structure.
				/// </summary>
				public List<string> sTafseer { get; set; }

				/// <summary>
				/// List of tafseer segments, Helper data structure.
				/// </summary>
				public List<(string Data, string Lang, int LangID, string Font, int FontID, string Size)> TafseerSegments { get; set; }// = new List<(string Data, FontFamily Font)>();
				/// <summary>
				/// List of intro segments, Helper data structure.
				/// </summary>
				public List<(string Data, string Lang, int LangID, string Font, int FontID, string Size)> IntroSegments { get; set; }// = new List<(string Data, FontFamily Font)>();
				#endregion
				//public List<(string Data, FontFamily Font)> lTafseer; // Using tuple to save text and font of text

				#region Methods

				public AyaData()
				{
					sIntro = new List<string>();
					sTafseer = new List<string>();
					TafseerSegments = new List<(string Data, string Lang, int LangID, string Font, int FontID, string Size)>();
					IntroSegments = new List<(string Data, string Lang, int LangID, string Font, int FontID, string Size)>();
					//lTafseer = new List<(string Data, FontFamily Font)>();
				}

				/// <summary>
				/// To store tafseer segments.
				/// </summary>
				/// <param name="data"></param>
				/// <param name="font"></param>
				public void AddTafseer(string data, string lang, int langId, string font, int fontId, string size) => TafseerSegments.Add((data, lang, langId, font, fontId, size));

				/// <summary>
				/// To store intro segments.
				/// </summary>
				/// <param name="data"></param>
				/// <param name="font"></param>
				public void AddIntro(string data, string lang, int langId, string font, int fontId, string size) => IntroSegments.Add((data, lang, langId, font, fontId, size));

				/// <summary>
				/// Not used at present.
				/// </summary>
				/// <param name="data"></param>
				/// <param name="font"></param>
				//public void AddTafsir(string data, string font) => lTafseer.Add((data, new FontFamily(font)));

				#endregion
			}
		}

		enum FontEnum
		{
			None,
			TimesNewRoman,
			NooriNastalique,
			Mushif
		}
	}
}
