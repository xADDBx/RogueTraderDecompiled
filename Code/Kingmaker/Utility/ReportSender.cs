using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Reporting.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owlcat.Core.Overlays;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility;

public class ReportSender : IDisposable
{
	public const string SentReportsFileName = "SentReports.txt";

	public const string AddressInternal = "http://siren.owlcat.local:5050";

	public const string AddressExternal = "http://213.149.169.10:5050";

	public const string AddressSecured = "http://89.17.52.236:5050";

	private const string ResponseSuccessful = "\"success\": true";

	private static readonly LogChannel Logger;

	private static readonly LogChannel Console;

	private readonly string m_Project;

	private readonly string m_GameVersion;

	private readonly CancellationToken m_CtsToken;

	private ReportSendingMode m_Mode;

	private CancellationTokenSource m_WorkerToken;

	private readonly ConcurrentDictionary<string, ReportSendEntry> m_PendingReports = new ConcurrentDictionary<string, ReportSendEntry>();

	private Task m_SetModeTask;

	private ReportSendParameters m_ReportSendParameters;

	private readonly ConcurrentQueue<ReportSendEntry> m_CheckReportQueue = new ConcurrentQueue<ReportSendEntry>();

	public int PendingReports => m_PendingReports.Count;

	public ReportSendingMode Mode => m_Mode;

	static ReportSender()
	{
		Logger = LogChannelFactory.GetOrCreate("ReportSender");
		Console = LogChannelFactory.GetOrCreate("Console");
	}

	public ReportSender(string project, string gameVersion, CancellationToken ctsToken)
	{
		m_Project = project;
		m_GameVersion = gameVersion;
		m_CtsToken = ctsToken;
		WorkerReportResultChecker(m_CtsToken);
		RegisterOverlay();
		SetMode(ReportSendingMode.All);
	}

	private void RegisterOverlay()
	{
		Overlay o = new Overlay("Reporting Utils", new Label("Reporting Utils", () => ""), new Label("Mode", () => ((int)Mode).ToString()), new Label("Pending Reports", () => PendingReports.ToString()));
		OverlayService.Instance?.RegisterOverlay(o);
	}

	public void SetMode(ReportSendingMode mode)
	{
		Logger.Log("Set mode to {0}", mode);
		m_WorkerToken?.Cancel();
		m_WorkerToken?.Dispose();
		m_WorkerToken = new CancellationTokenSource();
		CancellationToken ctsToken = m_CtsToken;
		ctsToken.Register(delegate
		{
			m_WorkerToken.Cancel();
		});
		m_Mode = mode;
		m_SetModeTask = SetModeAsync(mode);
	}

	private async Task SetModeAsync(ReportSendingMode mode)
	{
		await Awaiters.ThreadPool;
		switch (m_Mode)
		{
		case ReportSendingMode.OneError:
			await WorkerStopOnErrorAsync(m_GameVersion, m_WorkerToken.Token);
			break;
		case ReportSendingMode.OneRound:
			await WorkerForRoundAsync(m_GameVersion, m_WorkerToken.Token);
			break;
		case ReportSendingMode.All:
			await WorkerForCycleAsync(m_GameVersion, m_WorkerToken.Token);
			break;
		default:
			Logger.Error("Wrong worker mode: {0}", mode);
			throw new ArgumentOutOfRangeException();
		}
	}

	private async Task WorkerStopOnErrorAsync(string gameVersion, CancellationToken token)
	{
		IEnumerable<ReportSendEntry> enumerable = DeleteOrSendReports(gameVersion);
		foreach (ReportSendEntry item in enumerable)
		{
			try
			{
				await SendAsync(item, token);
			}
			catch (Exception)
			{
				break;
			}
		}
	}

	private async Task WorkerForRoundAsync(string gameVersion, CancellationToken token)
	{
		IEnumerable<ReportSendEntry> enumerable = DeleteOrSendReports(gameVersion);
		foreach (ReportSendEntry report in enumerable)
		{
			try
			{
				await SendAsync(report, token);
			}
			catch (Exception ex)
			{
				Logger.Log(ex, "Report sending failed ({0})", report.File);
			}
			await Task.Delay(TimeSpan.FromSeconds(1.0), token);
			token.ThrowIfCancellationRequested();
		}
	}

	private async Task WorkerForCycleAsync(string gameVersion, CancellationToken token)
	{
		_ = 1;
		try
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					await WorkerForRoundAsync(gameVersion, token);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception ex2)
				{
					Logger.Error(ex2);
				}
				await Task.Delay(TimeSpan.FromSeconds(1.0), token);
			}
			token.ThrowIfCancellationRequested();
		}
		catch (Exception ex3)
		{
			Logger.Exception(ex3, "Interrupt worker");
		}
		finally
		{
			Logger.Log("Exit from worker");
		}
	}

	private async ValueTask SendAsync(ReportSendEntry sendEntry, CancellationToken token)
	{
		string reportFileName = sendEntry.File;
		ReportSendParameters reportSendParameters = sendEntry.SendParameters;
		if (!File.Exists(reportFileName))
		{
			Logger.Error("Report {0} is not exist", reportFileName);
			return;
		}
		ReportSendEntry retry = m_PendingReports.GetOrAdd(reportSendParameters.Guid, (string guid) => new ReportSendEntry(guid, m_ReportSendParameters));
		retry.SendRetryCount++;
		retry.LastRetry = DateTime.Now;
		Logger.Log("Trying to resend bug report with guid {0}. Retry count: {1}", reportSendParameters.Guid, retry.SendRetryCount);
		Console.Log("Trying to resend bug report with guid {0}. Retry count: {1}", reportSendParameters.Guid, retry.SendRetryCount);
		try
		{
			bool devReport = ReportingUtils.IsEmailOwlcatDomain(reportSendParameters.Email) && ReportingCheats.IsDebugOpenReportLastIssueInBrowser;
			ReportUploadStatus reportUploadStatus = await new SirenClient().Report.UploadAsync(reportFileName, reportSendParameters, token);
			string messageFormat = "Bug report with guid {0} send retry lasted {1}. Response: \r{2}";
			string guid2 = reportSendParameters.Guid;
			TimeSpan timeSpan = DateTime.Now - retry.LastRetry;
			Logger.Log(messageFormat, guid2, timeSpan, reportUploadStatus);
			Console.Log(messageFormat, guid2, timeSpan, reportUploadStatus);
			bool? flag = reportUploadStatus?.Success;
			if (!flag.HasValue || !flag.Value)
			{
				return;
			}
			if (devReport)
			{
				retry.ReportId = reportUploadStatus.ReportId;
				Logger.Log("Bug report with guid {0} gets ReportId: {1}", reportSendParameters.Guid, retry.ReportId);
			}
			try
			{
				File.Delete(reportFileName);
				m_PendingReports.TryRemove(retry.Guid, out var _);
				if (m_CheckReportQueue.All((ReportSendEntry x) => x.Guid != retry.Guid))
				{
					m_CheckReportQueue.Enqueue(retry);
				}
				return;
			}
			catch (Exception ex)
			{
				Logger.Exception(ex, "Failed to delete report file after send: {0}", reportFileName);
			}
			await File.AppendAllLinesAsync(Path.Combine(ReportingUtils.BugReportsPath, "SentReports.txt"), new string[1] { reportFileName }, token);
		}
		catch (Exception ex2)
		{
			Logger.Exception(ex2, "Failed to process report: {0}", reportFileName);
			throw;
		}
	}

	private IEnumerable<ReportSendEntry> DeleteOrSendReports(string gameVersion)
	{
		string bugReportsPath = ReportingUtils.BugReportsPath;
		string text = Path.Combine(bugReportsPath, "SentReports.txt");
		List<string> list = new List<string>();
		if (File.Exists(text))
		{
			try
			{
				list.AddRange(File.ReadAllLines(text));
			}
			catch (Exception ex)
			{
				Logger.Log(ex, "Failed read reports file");
			}
			list.RemoveAll(delegate(string report)
			{
				try
				{
					if (File.Exists(report))
					{
						File.Delete(report);
					}
					return true;
				}
				catch (Exception ex4)
				{
					Logger.Exception(ex4, "Failed to delete report file: {0}", report);
					return false;
				}
			});
			if (list.Any())
			{
				try
				{
					File.WriteAllLines(text, list);
				}
				catch (Exception ex2)
				{
					Logger.Exception(ex2, "Failed to write to SentReports file {0}", text);
				}
			}
			else
			{
				try
				{
					File.Delete(text);
				}
				catch (Exception ex3)
				{
					Logger.Exception(ex3, "Failed delete SentReports file {0}", text);
				}
			}
		}
		if (Directory.Exists(bugReportsPath))
		{
			string[] files = Directory.GetFiles(bugReportsPath, "*.zks", SearchOption.TopDirectoryOnly);
			int num = 0;
			string[] array = files;
			foreach (string text2 in array)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text2);
				if (!list.Contains(text2) && !m_PendingReports.ContainsKey(fileNameWithoutExtension) && Enqueue(text2) != null)
				{
					num++;
				}
			}
			if (num > 0)
			{
				Logger.Log($"Find new {m_PendingReports.Count} reports.");
			}
		}
		return m_PendingReports.Values;
	}

	private async Task GetReportResult(ReportSendEntry sendEntry, CancellationToken token)
	{
		if (string.IsNullOrEmpty(sendEntry.ReportId))
		{
			return;
		}
		Logger.Log("Check result for report ( " + sendEntry.Guid + " ) with reportId ( " + sendEntry.ReportId + " )");
		string requestUri = await SirenAddressFinder.GetReportServerAddressAsync(token) + "/api/report/issues?filename=" + sendEntry.ReportId;
		try
		{
			using HttpClient client = new HttpClient();
			JObject jObject = JObject.Parse(await (await client.GetAsync(requestUri, token)).Content.ReadAsStringAsync());
			if ((bool)jObject.SelectToken("success"))
			{
				sendEntry.ReportIssueId = (string?)jObject.SelectToken("reportId");
				Logger.Log("Get reportId for report (" + sendEntry.Guid + ") success. Report Issue Id: " + sendEntry.ReportIssueId + " ");
				EventBus.RaiseEvent(delegate(IReportSender rs)
				{
					rs.HandleReportResultReceived(sendEntry);
				});
			}
			else
			{
				Logger.Log("Get reportId for report (" + sendEntry.Guid + ") failed");
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed to get report issue Id for report ( {0} ) with reportId ( {1} )", sendEntry.Guid, sendEntry.ReportIssueId);
		}
	}

	public void Dispose()
	{
		m_WorkerToken?.Dispose();
	}

	public ReportSendEntry Enqueue(string zipFilePath)
	{
		ReportParameters reportParameters = null;
		try
		{
			Logger.Log("Try to Enqueue '" + zipFilePath + "'");
			using FileStream stream = File.OpenRead(zipFilePath);
			using ZipArchive zipArchive = new ZipArchive(stream);
			ZipArchiveEntry zipArchiveEntry = Enumerable.FirstOrDefault(zipArchive.Entries, (ZipArchiveEntry zipEntry) => zipEntry.Name == "parameters.json");
			if (zipArchiveEntry != null)
			{
				using Stream stream2 = zipArchiveEntry?.Open();
				using StreamReader streamReader = new StreamReader(stream2);
				reportParameters = JsonConvert.DeserializeObject<ReportParameters>(streamReader.ReadToEnd());
			}
		}
		catch (Exception ex)
		{
			Logger.Log(ex);
			if (ex.Message == "Central Directory corrupt.")
			{
				Logger.Log("Delete corrupted archive: '" + zipFilePath + "'");
				File.Delete(zipFilePath);
			}
		}
		if (reportParameters == null || reportParameters.Guid == null)
		{
			Logger.Log("Cannot enqueue '" + zipFilePath + "'");
			return null;
		}
		ReportSendParameters reportSendParameters = new ReportSendParameters
		{
			Email = (reportParameters?.Email ?? "unknown@e.mail"),
			Version = reportParameters?.Version,
			Project = m_Project,
			dev = ReportingUtils.IsEmailOwlcatDomain(reportParameters?.Email ?? "unknown@e.mail").ToString(),
			Guid = reportParameters?.Guid,
			Priority = (reportParameters?.IssueType ?? string.Empty)
		};
		ReportSendEntry result = new ReportSendEntry(zipFilePath, reportSendParameters);
		result = m_PendingReports.GetOrAdd(reportParameters?.Guid, (string s) => result);
		Logger.Log("Add '" + zipFilePath + "' to send queue");
		return result;
	}

	private async Task WorkerReportResultChecker(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromMilliseconds(1000.0), token);
			if (m_CheckReportQueue.TryDequeue(out var sendEntry))
			{
				sendEntry.ReportIssueCheckRetryCount++;
				Logger.Log($"Try get report issue id {sendEntry.ReportIssueCheckRetryCount} times for report ( {sendEntry.Guid} ) with reportId ( {sendEntry.ReportId} )");
				await GetReportResult(sendEntry, token);
				if (string.IsNullOrEmpty(sendEntry.ReportIssueId) && sendEntry.ReportIssueCheckRetryCount < 50)
				{
					m_CheckReportQueue.Enqueue(sendEntry);
				}
				sendEntry = null;
			}
		}
	}
}
