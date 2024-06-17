using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Kingmaker.Blueprints;
using Kingmaker.GameInfo;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json.Linq;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

public class Arbiter : MonoBehaviour, IArbiterEventHandler, ISubscriber
{
	public static Arbiter Instance;

	private static string s_ServerAddress = "https://localhost:7179";

	public static Action RepaintEditorWindow;

	public static Guid JobGuid;

	public static Guid Run = default(Guid);

	public ArbiterReporter Reporter;

	private static ArbiterStatusServer s_StatusServer;

	[SerializeField]
	private BlueprintAreaPresetReference StartGamePreset;

	private Queue<string> m_InstructionsToRun;

	private BlueprintArbiterInstruction m_Instruction;

	private ArbiterTask m_Task;

	private bool m_IsFinished;

	private float m_Delay;

	private bool m_IsFatalError;

	private static bool m_Indeteminated = false;

	private static bool s_ServerConnectionError;

	private static bool s_IsBadCacheStateError;

	private Exception m_Exception;

	private string m_Version;

	private const string ArbiterLogFile = "arbiter.log";

	public static BlueprintArbiterRoot Root => BlueprintArbiterRoot.Instance;

	public static bool SendToServer { get; private set; }

	public static int ProcessTimeout { get; private set; } = 720;


	public static string SendProbeUrl => s_ServerAddress + "/Create";

	public static string AvailableScenariosUrl => s_ServerAddress + "/Instructions";

	public static string JobFinishedUrl => s_ServerAddress + "/OnJobFinished";

	public static string StatusString => (Instance ? Instance : null)?.m_Task?.Status;

	public static bool IsInitialized => Instance != null;

	public static bool IsRunning
	{
		get
		{
			if (IsInitialized)
			{
				return !Instance.m_IsFinished;
			}
			return false;
		}
	}

	private static bool IsFinished
	{
		get
		{
			if (IsInitialized)
			{
				return Instance.m_IsFinished;
			}
			return false;
		}
	}

	public static BlueprintArbiterInstruction Instruction => (Instance ? Instance : null)?.m_Instruction;

	public static string PlatformDataPath { get; private set; }

	public static HardwareInfo HardwareInfo { get; private set; }

	public ArbiterStartupParameters Arguments { get; private set; }

	public static string SnapshotNameFirst => Path.Combine(PlatformDataPath, "first.snap");

	public static string SnapshotNameLast => Path.Combine(PlatformDataPath, "last.snap");

	public static string SnapshotNameTemp => Path.Combine(PlatformDataPath, "temp.snap");

	public static string GetStatus()
	{
		if (m_Indeteminated)
		{
			return "indeterminated";
		}
		if (IsRunning)
		{
			return Instance.Reporter?.GetStatus() ?? "running";
		}
		if (IsFinished)
		{
			return "finished";
		}
		if (s_IsBadCacheStateError)
		{
			return "bad cache state";
		}
		if (!IsInitialized)
		{
			return "not started";
		}
		if (s_ServerConnectionError)
		{
			return "server connection error";
		}
		return "unknown";
	}

	public void Stop()
	{
		StopAllCoroutines();
		m_Task = null;
		m_IsFinished = true;
		RepaintEditorWindow = null;
	}

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (IsInitialized)
		{
			return;
		}
		m_Version = GameVersion.GetVersion();
		if (m_Version == "Editor")
		{
			m_Version = m_Version + "." + DateTime.Now.ToString("yyMMddhhmm");
		}
		Arguments = CommandLineArgumentsToArbiterStartupParametersConverter.Convert(CommandLineArguments.Parse());
		if (Arguments.Arbiter == null && !Application.isEditor)
		{
			return;
		}
		InitializeLogger();
		InitPlatformDataPath();
		HardwareInfo = new HardwareInfo();
		ArbiterClientIntegration.SetQaMode(b: false);
		SendToServer = Arguments.Arbiter != null || Application.isEditor;
		s_ServerAddress = Arguments.ArbiterServer ?? s_ServerAddress;
		PlatformDataPath = Arguments.ArbiterPlatformDataPath ?? PlatformDataPath;
		if (s_StatusServer == null)
		{
			s_StatusServer = new ArbiterStatusServer();
		}
		if (Arguments.ArbiterRestart)
		{
			try
			{
				File.Delete(SnapshotNameFirst);
				File.Delete(SnapshotNameLast);
			}
			catch (Exception ex)
			{
				PFLog.Arbiter.Exception(ex, "Failed to delete memory snapshot files");
			}
		}
		Reporter = new ArbiterReporter(Arguments);
		InstructionSettings instructionSettings = new InstructionSettings(Arguments);
		if (instructionSettings.IsSingleInstruction)
		{
			Reporter.DeleteCache();
			Reporter.InitReportData(new string[1] { instructionSettings.Data });
		}
		else if (instructionSettings.IsInstructionList)
		{
			if (Arguments.ArbiterRestart)
			{
				Reporter.DeleteCache();
			}
			if (File.Exists(instructionSettings.Data))
			{
				PFLog.Arbiter.Log("Read data from file '" + instructionSettings.Data + "'");
				Reporter.InitReportData(File.ReadLines(instructionSettings.Data));
			}
			else
			{
				PFLog.Arbiter.Log("File '" + instructionSettings.Data + "' not found. Trying get instructions from server.");
				Reporter.InitReportData(GetAvailableScenariosFromServer());
			}
		}
		m_InstructionsToRun = new Queue<string>(Reporter.GetPendingInstructions());
		PFLog.Arbiter.Log($"Pending instructions: {m_InstructionsToRun.Count}");
		JobGuid = Reporter.CachedJobGuid;
		Instance = this;
		EventBus.Subscribe(Instance);
		UnityEngine.Object.DontDestroyOnLoad(Instance);
	}

	private static void InitializeLogger()
	{
		if (!LoggingConfiguration.IsLoggingEnabled)
		{
			return;
		}
		string logsDir = ApplicationPaths.LogsDir;
		string text = Path.Combine(logsDir, "arbiter.log");
		if (File.Exists(text))
		{
			string text2 = Path.Combine(logsDir, Path.GetFileName(text) + ".bak");
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Move(text, text2);
		}
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(new ArbiterUberLoggerFilter(new UberLoggerFile("arbiter.log", null, includeCallStacks: false)));
	}

	private static void InitPlatformDataPath()
	{
		PlatformDataPath = ApplicationPaths.DevelopmentDataPath;
		ArbiterMeasurements.s_IsMemoryToolAvailable = true;
	}

	public void RunInstruction(string instructionName)
	{
		if (IsInitialized)
		{
			m_InstructionsToRun.Enqueue(instructionName);
			m_IsFinished = false;
		}
	}

	public void RunInstructionsFile(string path)
	{
		if (IsInitialized && File.Exists(path))
		{
			m_InstructionsToRun = File.ReadLines(path).ToQueue();
			m_IsFinished = false;
		}
	}

	private void Update()
	{
		if (!IsRunning)
		{
			return;
		}
		RepaintEditorWindow?.Invoke();
		if (m_Delay > 0f)
		{
			m_Delay -= Time.unscaledDeltaTime;
			return;
		}
		try
		{
			ArbiterTask task = m_Task;
			if (task != null && task.Ticker.MoveNext())
			{
				m_Delay = m_Task.Delay;
				m_Task = m_Task.SubtaskOrSelf;
			}
			else if (m_Task?.ParentTask != null)
			{
				m_Task = m_Task.ParentTask;
			}
			else
			{
				GetNextTest();
			}
		}
		catch (Exception ex)
		{
			PFLog.Arbiter.Exception(ex);
			Reporter.ReportInstructionError(Instruction?.name, ex.Message + "\n" + ex.StackTrace, m_Task?.ElapsedTestTime ?? TimeSpan.Zero);
			m_Exception = ex;
			m_Task = null;
			if (m_IsFatalError)
			{
				m_Indeteminated = true;
			}
		}
	}

	public void HandleFatalError(Exception ex)
	{
		Reporter.ReportInstructionError(Instruction?.name, ex.Message + "\n" + ex.StackTrace, m_Task?.ElapsedTestTime ?? TimeSpan.Zero);
		PFLog.Arbiter.Error(ex, "Skip instruction [{0}] because of a fatal error (see previous log message)", Instruction?.name);
		m_Delay = 10f;
		m_Task = null;
		m_IsFatalError = true;
	}

	private void GetNextTest()
	{
		if (m_InstructionsToRun.Empty())
		{
			m_IsFinished = true;
			OnJobFinished();
			Reporter.DeleteCache();
			EventBus.RaiseEvent(delegate(IArbiterEventHandler handler)
			{
				handler.ArbiterFinished(Instance);
			});
		}
		else
		{
			if (IsRecoveringAfterFatalError())
			{
				return;
			}
			if (m_InstructionsToRun.Peek() == "ExportMaps")
			{
				m_InstructionsToRun.Dequeue();
				m_Task = new MapsExportTask();
				return;
			}
			if (m_InstructionsToRun.Peek().Contains("instant_move_camera"))
			{
				ArbiterInstantMoveCameraTask task = new ArbiterInstantMoveCameraTask(m_InstructionsToRun.Dequeue());
				m_Task = task;
				return;
			}
			string text = m_InstructionsToRun.Dequeue();
			m_Instruction = ArbiterInstructionIndex.Instance.GetInstruction(text);
			if (m_Instruction == null)
			{
				PFLog.Arbiter.Error("Instruction " + text + " is not found.");
				Reporter.ReportInstructionNotFound(text, "Instruction " + text + " is not found.", TimeSpan.Zero);
			}
			else if (SimpleBlueprintExtendAsObject.Or(m_Instruction, null)?.Test is IArbiterCheckerComponent arbiterCheckerComponent)
			{
				m_Task = arbiterCheckerComponent.GetArbiterTask(Arguments);
			}
			else
			{
				PFLog.Arbiter.Error("Unknown test type");
			}
		}
	}

	private bool IsRecoveringAfterFatalError()
	{
		if (m_IsFatalError)
		{
			if (!ArbiterClientIntegration.IsMainMenuActive())
			{
				ArbiterUtils.CheckTimeout("Recovering After Fatal Error");
				return true;
			}
			m_Delay = 10f;
		}
		m_IsFatalError = false;
		return false;
	}

	private List<string> GetAvailableScenariosFromServer()
	{
		List<string> list = new List<string>();
		try
		{
			ServicePointManager.ServerCertificateValidationCallback = (object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true;
			WebClient webClient = new WebClient();
			webClient.QueryString.Add("project", Root.Project);
			if (Arguments.ArbiterInstructionsPart != null)
			{
				webClient.QueryString.Add("part", Arguments.ArbiterInstructionsPart);
			}
			list = (from x in JObject.Parse(webClient.DownloadString(AvailableScenariosUrl)).SelectToken("Instructions")
				select (string?)x).ToList();
			PFLog.Arbiter.Log($"Received {list.Count} scenarios from server");
			s_ServerConnectionError = false;
		}
		catch (Exception ex)
		{
			PFLog.Arbiter.Exception(ex);
			s_ServerConnectionError = true;
		}
		return list;
	}

	private void OnJobFinished()
	{
		if (Arguments.ArbiterDisableNotifications != null)
		{
			return;
		}
		try
		{
			PFLog.Arbiter.Log("Sending job '" + JobGuid.ToString() + "' finished event to server... ");
			ServicePointManager.ServerCertificateValidationCallback = (object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true;
			WebClient webClient = new WebClient();
			webClient.QueryString.Add("guid", JobGuid.ToString());
			webClient.UploadString(JobFinishedUrl, "");
			PFLog.Arbiter.Log("Sending job '" + JobGuid.ToString() + "' finished event to server success.");
		}
		catch (Exception ex)
		{
			PFLog.Arbiter.Log("Sending job '" + JobGuid.ToString() + "' finished event to server error:");
			PFLog.Arbiter.Exception(ex);
		}
	}

	public static void GenerateReportCache(string serverAddress)
	{
		s_ServerAddress = serverAddress;
		PlatformDataPath = ApplicationPaths.persistentDataPath;
		List<string> instructions = ArbiterInstructionIndex.Instance.Instructions.ToList();
		new ArbiterReporter(CommandLineArgumentsToArbiterStartupParametersConverter.Convert(CommandLineArguments.Parse())).InitReportData(instructions);
		PFLog.Arbiter.Log("Cache generated");
	}

	public string GetVersion()
	{
		return m_Version;
	}

	public void ArbiterFinished(Arbiter arbiter)
	{
		PFLog.Arbiter.Log("Arbiter finished");
		PFLog.SmartConsole.Log("Arbiter finished");
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler wrn)
		{
			wrn.HandleWarning("Arbiter finished");
		});
		RepaintEditorWindow = null;
		if (Arguments.ArbiterExitOnFinish)
		{
			s_StatusServer.Dispose();
			Application.Quit();
		}
	}
}
