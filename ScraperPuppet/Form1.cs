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
using System.Threading.Tasks;
using System.Windows.Forms;
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
				//await GetSurah(2, sSuraNames[2]);
			}
			for (int i = 110; i < sSuraNames.Count; i++)
			{
				await GetSurah(i, sSuraNames[i]);
			}
			//await DisplayPageAsync(fullUrl);
			//string sAya = await GetAyaDataPuppetAsync(sSuraNames[2], 10);
			//AyaData sAya = await GetAyaDataAsync(sSuraNames[2], 10);

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
			surahData.SurahID = surahID;
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
			for (int i = 1; i <= surahData.AyaCount; i++)
			{
				surahData.Ayas.Add(await GetAyaDataAsync(surahData.SurahID, surahName, i));
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
			Debug.WriteLine("Surah ID = " + surahData.SurahID);

			elements = nodesArray[2].InnerText.Split('\n');
			surahData.SurahNameAr = elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim();
			Debug.WriteLine("Surah name = " + surahData.SurahNameAr);

			elements = nodesArray[3].InnerText.Split('\n');
			surahData.RevelationID = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			Debug.WriteLine("Revelation ID = " + surahData.RevelationID);

			elements = nodesArray[4].InnerText.Split('\n');
			surahData.RukuCount = int.Parse(elements[elements.Length - (elements.Length == 1 ? 1 : 2)].Trim());
			Debug.WriteLine("Surah has rukus = " + surahData.RukuCount);
		}
		private async Task<AyaData> GetAyaDataAsync(int surahId, string surahName, int aya)
		{
			//CURRENTLY THIS METHOD IS COMPLETING ITS INTERNAL TASKS SYNCRONOUSLY.
			AyaData ayaData = new AyaData();
			ayaData.SurahID = surahId;
			ayaData.SurahName = surahName;
			ayaData.AyaID = aya;
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
			string sInnerText; // To store segment value.
			string sFontText; // To store style attribute value giving font details.
			string[] sFontTexts; // To store font details from style attribute.
			string sFontSize; // To store font size.
			string sFontFamily = "Times New Roman";
			string sLanguage; // To store text language.
			int iLangID; // To store language ID.
			int iFontID; // To store font ID.
			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			StringBuilder sb3 = new StringBuilder();
			List<string> sAya = new List<string>();
			string sAyaData;
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
					ayaData.sIntro.Add(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '));
				}

				sb.Clear(); sb2.Clear(); sb3.Clear();
				iSegmentID = 0;
				sb3.Append($"\t\t\t\t\t<intro id=\"intro_{ayaData.SurahID}\">\n");
				foreach (var node in ayaData.sIntro)
				{
					iSegmentID++;
					sb.Append($"{node}. "); // For ayaData.Intro, Intro.txt.
					sb2.Append($"\t{node}\n"); // For Intro.Xml.
					sb3.Append($"\t\t\t\t\t\t<segment id=\"intro_{ayaData.SurahID}.{iSegmentID}\">\n");
					sb3.Append($"\t\t\t\t\t\t\t{node}\n"); // For ayaData.IntroXml, AlQuran.Xml
					sb3.Append($"\t\t\t\t\t\t</segment>\n");
				}
				sb3.Append($"\t\t\t\t\t</intro>\n");

				ayaData.Intro = sb.ToString();
				ayaData.IntroTag = $"<intro id=\"intro_{ayaData.SurahID}\">\n{sb2}</intro>\n";
				ayaData.IntroXml = sb3.ToString();

				File.AppendAllText("Output Files\\Intro.txt", ayaData.Intro + "\n");
				File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Intro + "\n");

				File.AppendAllText("Output Files\\Intro.xml", ayaData.IntroTag);
				File.AppendAllText("Output Files\\SurahTafseer.xml", ayaData.IntroTag);
				File.AppendAllText("Output Files\\AlQuran.xml", ayaData.IntroXml); // Will be returned to parent method for proper placement in file.
				#endregion

				#region Code for Intro Segments

				// Stores intro segments.
				foreach (var node in segnodesIntro)
				{
					sInnerText = node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' ');
					sFontText = node.GetAttributeValue("style", "No Attribute");
					sFontTexts = sFontText.Replace("&quot;", "").Replace("\r\n", "").Split(';', ':');
					sFontSize = sFontTexts[1].Replace("pt", "").Trim();
					sFontFamily = sFontTexts[3].Trim();
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
					else
					{
						sLanguage = "EN";
						iLangID = 0;
						iFontID = 0;
					}
					ayaData.AddIntro(sInnerText, sLanguage, iLangID, sFontFamily, iFontID, sFontSize);
				}

				sb.Clear(); sb2.Clear(); sb3.Clear();
				iSegmentID = 0;
				sb3.Append($"\t\t\t\t\t<intro id=\"intro_{ayaData.SurahID}\">\n");
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
				sb3.Append($"\t\t\t\t\t</intro>\n");

				ayaData.IntroSegmentedText = sb.ToString();
				ayaData.IntroSegmentedTag = $"<intro id=\"intro_{ayaData.SurahID}\">\n{sb2}</intro>\n";
				ayaData.IntroSegmentedXml = sb3.ToString();

				//ayaData.Intro = sAyaData.Replace("&nbsp", " ");
				File.AppendAllText("Output Files\\IntroSegmented.txt", ayaData.IntroSegmentedText);
				File.AppendAllText("Output Files\\AlQuranSegmented.txt", ayaData.IntroSegmentedText);

				File.AppendAllText("Output Files\\IntroSegmented.xml", ayaData.IntroSegmentedTag);
				File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.IntroSegmentedXml);

				#endregion

			}

			#region Code for tafseer lines.

			// Stores list of tafseer line.
			foreach (var node in nodesTafseer)
			{
				ayaData.sTafseer.Add(node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' '));
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

			File.AppendAllText("Output Files\\Tafseer.txt", ayaData.Tafseer + "\n");
			File.AppendAllText("Output Files\\AlQuran.txt", ayaData.Tafseer + "\n");

			File.AppendAllText("Output Files\\Tafseer.xml", ayaData.TafseerTag);
			File.AppendAllText("Output Files\\SurahTafseer.xml", ayaData.TafseerTag);
			File.AppendAllText("Output Files\\AlQuran.xml", ayaData.TafseerXml);
			#endregion

			#region Code for tafseer segments.

			// Stores tafseer segments.
			foreach (var node in segnodesTafseer)
			{
				//i++;
				//if (i == 42)
				//{
				//	Console.WriteLine("Houston! we have a problem");
				//}
				//Console.WriteLine(i.ToString());
				sInnerText = node.InnerText.Replace("\r\n", " ").Replace("&nbsp", " ").Replace(';', ' ');

				sFontText = node.GetAttributeValue("style", "No Attribute");
				sFontTexts = sFontText.Replace("&quot;", "").Replace("\r\n", "").Split(';', ':');
				sFontSize = sFontTexts[1].Replace("pt", "").Trim();

				if (sFontTexts.Length > 3)
				{
					sFontFamily = sFontTexts[3].Trim();
				}
				else
				{
					Console.WriteLine($"ERROR!!! - sFontTexts Index out of range - {sFontTexts.Length.ToString()}");
					MessageBox.Show("ERROR!!! - sFontTexts Index out of range", "ERROR IN TAFSEER SEGMENT");

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
				else
				{
					sLanguage = "EN";
					iLangID = 0;
					iFontID = 0;
				}
				ayaData.AddTafseer(sInnerText, sLanguage, iLangID, sFontFamily, iFontID, sFontSize);
			}

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

			ayaData.TafseerSegmentedText = sb.ToString();
			ayaData.TafseerSegmentedTag = $"<tafseer id=\"tafseer_{ayaData.SurahID}.{ayaData.AyaID}\">\n{sb2}</tafseer>\n";
			ayaData.TafseerSegmentedXml = sb3.ToString();

			File.AppendAllText("Output Files\\TafseerSegmented.txt", ayaData.TafseerSegmentedText);
			File.AppendAllText("Output Files\\AlQuranSegmented.txt", ayaData.TafseerSegmentedText);

			File.AppendAllText("Output Files\\TafseerSegmented.xml", ayaData.TafseerSegmentedTag);
			File.AppendAllText("Output Files\\AlQuranSegmented.xml", ayaData.TafseerSegmentedXml);

			#endregion

			Debug.WriteLine(ayaData.Aya);

			//var attrib = nodes[1].GetDataAttributes();
			//var attributes = nodes[20].GetDataAttributes();

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
				public int SurahID { get; set; }
				public string SurahName { get; set; }
				public int AyaID { get; set; }

				public string Aya { get; set; }
				public string KIman { get; set; }
				public string KIrfan { get; set; }

				#region Output Data 				
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
				public List<(string Data, string Lang, int LangID, string Font, int FontID, string Size)> TafseerSegments;// = new List<(string Data, FontFamily Font)>();
				/// <summary>
				/// List of intro segments, Helper data structure.
				/// </summary>
				public List<(string Data, string Lang, int LangID, string Font, int FontID, string Size)> IntroSegments;// = new List<(string Data, FontFamily Font)>();
				#endregion
				//public List<(string Data, FontFamily Font)> lTafseer; // Using tuple to save text and font of text

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
