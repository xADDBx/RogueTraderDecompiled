using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Base;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.Stores;
using Kingmaker.UI.Models;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Profiling;

namespace Kingmaker.Utility;

public class ReportingUtils : IDisposable, IFullScreenUIHandler, ISubscriber, IPortraitHoverUIHandler, ISubscriber<IBaseUnitEntity>, IBugReportDescriptionUIHandler, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, IGlobalRulebookSubscriber, IBugReportUIHandler, IReportSender, IService
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("ReportingUtils");

	private const string Project = "WH";

	private const string SaveFileName = "save.zks";

	private const string ReportingUtilsLogFile = "reporting_util.txt";

	private const int ReportingUtilsLogFileMaxSize = 5248000;

	private const string TemporarySaveDefaultLabel = "SavedFromBug";

	private const string TemporarySaveFileName = "saveatthemoment.zks";

	private const string SystemInfoFileName = "systemInfo.txt";

	private const string MessageFileName = "message.txt";

	public const string ParametersFileName = "parameters.json";

	private const string MetadataFileName = "GameMetadata.json";

	private const string CombatLogFileName = "combatLog.txt";

	private const string OutputLogFileName = "output_log.txt";

	private const string LogFileName = "log.txt";

	private const string DataOutputLogResultFileName = "output_log.txt";

	private const string DataLogResultLogFileName = "log0.txt";

	private const string PersistentDataOutputLogResultFileName = "output_log0.txt";

	private const string PersistentDataLogResultLogFileName = "log.txt";

	private const string PersistentDataGameLogFileName = "GameLog.txt";

	private const string PersistentDataGameLogPrevFileName = "GameLogFullPrev.txt";

	private const string PersistentDataGameLogFullFileName = "GameLogFull.txt";

	private const string PersistentDataEditorLogFullFileName = "EditorLogFull.txt";

	private const string PersistentDataEditorLogFileName = "EditorLog.txt";

	private const string PersistentDataGameHistoryLogFileName = "game-history.txt";

	private const string ReporterErrorLogFileName = "ReporterErrorLog.txt";

	private const string ConsoleLogFullFileName = "ConsoleLogFull.txt";

	private const string ConsoleLogFullPrevFileName = "ConsoleLogFullPrev.txt";

	private const string StartupFileName = "startup.json";

	private const string AssigneeFileName = "assignee.json";

	private static readonly List<string> OwlcatDomains = new List<string> { "@owlcat.games", "@sutstrikestudios.com" };

	public static readonly string[] FixVersions = new string[4] { "Current", "Next", "After", "Empty" };

	private readonly CancellationTokenSource m_Cts = new CancellationTokenSource();

	private string m_TemporarySaveLabel = string.Empty;

	private string m_DiscordContacts = string.Empty;

	private BugContext m_Context;

	private readonly List<BugContext> m_ContextVariants = new List<BugContext>();

	private readonly string m_GameVersion;

	private string m_SelectedFixVersion = "Current";

	private readonly List<string> m_ReporterErrors = new List<string>();

	private FullScreenUIType m_ActiveFullScreenUIType;

	private BlueprintUnit m_HoveredPortraitUnitBlueprint;

	private readonly List<PartyContext.ReportParameterHelper> m_SpellCastHistory = new List<PartyContext.ReportParameterHelper>();

	private readonly List<PartyContext.ReportParameterHelper> m_ItemUseHistory = new List<PartyContext.ReportParameterHelper>();

	private bool m_IsGlobalMapOpened;

	private readonly ReportCombatLogManager m_ReportCombatLogManager;

	private string m_SuggestedAssignee = string.Empty;

	private Exception m_Exception;

	private string[] m_ErrorMessages;

	private readonly ReportFilesMd5Manager m_ReportFilesMd5Manager;

	private string m_UiFeatureName = string.Empty;

	private const string UiFeatureAssignee = "ui_designer";

	private readonly ReportPrivacyManager m_ReportPrivacyManager;

	private Dictionary<string, bool> m_labelsDictionary = new Dictionary<string, bool>();

	private ReportSender ReportSender;

	private bool m_WaitForSave;

	public static ReportingUtils Instance => Services.GetInstance<ReportingUtils>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public static string BugReportsPath => Path.Combine(ApplicationPaths.persistentDataPath, "Reports");

	public string CurrentReportFolder { get; private set; } = string.Empty;


	public BlueprintScriptableObject ExceptionSource
	{
		get
		{
			return ReportingUtilsSourceHolder.Instance.ExceptionSource;
		}
		set
		{
			ReportingUtilsSourceHolder.Instance.ExceptionSource = value;
		}
	}

	public Task<AssigneeModelRoot> Assignees { get; set; }

	public static ReportSendEntry LastEntry { get; set; }

	public string CurrentContextName => m_Context.Type.ToString("G");

	private static string DefaultPath => ApplicationPaths.persistentDataPath;

	public ReportingUtils()
	{
		EventBus.Subscribe(this);
		m_ReportCombatLogManager = new ReportCombatLogManager(ApplicationPaths.persistentDataPath, "combatLog.txt", this);
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(new ReportingUberLoggerFilter(new UberLoggerFile(GetPlatform().ToLower() + "_reporting_util.txt", null, includeCallStacks: false, 5248000, append: true)));
		Logger.Log("");
		Logger.Log("Instantiate ReportingUtils");
		m_ReportFilesMd5Manager = new ReportFilesMd5Manager();
		ReportFilesMd5Manager reportFilesMd5Manager = m_ReportFilesMd5Manager;
		reportFilesMd5Manager.ReportError = (Action<string>)Delegate.Combine(reportFilesMd5Manager.ReportError, new Action<string>(LogReporterError));
		m_ReportPrivacyManager = new ReportPrivacyManager();
		m_GameVersion = GameVersion.GetVersion();
		CrashReportHandler.SetUserMetadata("Store", StoreManager.Store.ToString());
		Task.Delay(10000).ContinueWith(delegate
		{
			Assignees = (BuildModeUtility.IsDevelopment ? Task.Run(() => AssigneeContainer.LoadAssigneesAsync("WH", m_Cts.Token)) : Task.FromCanceled<AssigneeModelRoot>(CancellationToken.None));
			ReportSender = new ReportSender("WH", m_GameVersion, m_Cts.Token);
		});
	}

	public void Dispose()
	{
		if (m_ReportFilesMd5Manager != null)
		{
			ReportFilesMd5Manager reportFilesMd5Manager = m_ReportFilesMd5Manager;
			reportFilesMd5Manager.ReportError = (Action<string>)Delegate.Remove(reportFilesMd5Manager.ReportError, new Action<string>(LogReporterError));
		}
		EventBus.Unsubscribe(this);
		m_ReportCombatLogManager.Dispose();
		m_Cts.Cancel();
		m_Cts.Dispose();
		ReportSender?.Dispose();
	}

	private void CreateReportFolder()
	{
		CurrentReportFolder = Path.Combine(BugReportsPath, Guid.NewGuid().ToString());
		try
		{
			Directory.CreateDirectory(CurrentReportFolder);
		}
		catch (Exception)
		{
			CurrentReportFolder = string.Empty;
		}
	}

	private string CreateSaveFileForBugReport(string outputPath)
	{
		try
		{
			SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave();
			if (latestSave == null)
			{
				Game.Instance.SaveManager.UpdateSaveListIfNeeded();
				latestSave = Game.Instance.SaveManager.GetLatestSave();
			}
			Logger.Log("Create save file with name: " + latestSave.FileName);
			if (latestSave != null && File.Exists(latestSave.FolderName))
			{
				bool flag = !string.IsNullOrEmpty(m_TemporarySaveLabel) && latestSave.Name.Contains(m_TemporarySaveLabel);
				string text = Path.Combine(outputPath, flag ? "saveatthemoment.zks" : "save.zks");
				File.Copy(latestSave.FolderName, text);
				Logger.Log("Copy save file " + latestSave.FileName + " to " + text);
				if (flag)
				{
					Game.Instance.SaveManager.DeleteSave(latestSave);
				}
				m_TemporarySaveLabel = string.Empty;
				return text;
			}
			if (latestSave != null)
			{
				LogReporterError("Failed to add save file: " + latestSave.Name + " has no real save file on disk");
				return "";
			}
			LogReporterError("Failed to add save file: no latest save in save manager");
			return "";
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to add save file: \n" + ex.Message + "\n" + ex.StackTrace);
			return "";
		}
	}

	public string CopySaveFile([NotNull] SaveInfo saveInfo, [NotNull] string outputPath, string outputName = null)
	{
		try
		{
			string text = Path.Combine(outputPath, outputName ?? (Path.GetFileNameWithoutExtension(saveInfo.FolderName) + ".zks"));
			Logger.Log("Copy save file " + saveInfo.FileName + " to " + text);
			if (File.Exists(saveInfo.FolderName))
			{
				File.Copy(saveInfo.FolderName, text);
				return text;
			}
			LogReporterError("Failed to copy save file: " + saveInfo.Name + " has no real save file on disk");
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to copy save file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		return "";
	}

	private void CreateSystemInfoFile()
	{
		try
		{
			string text = Path.Combine(CurrentReportFolder, "systemInfo.txt");
			LogReporterError("$Creating systemInfo file at: " + text);
			using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
			StringBuilder builder = pooledStringBuilder.Builder;
			builder.AppendLine("operatingSystem: " + SystemInfo.operatingSystem);
			builder.AppendLine($"operatingSystemFamily: {SystemInfo.operatingSystemFamily}");
			builder.AppendLine($"processorCount: {SystemInfo.processorCount}");
			builder.AppendLine($"processorFrequency: {SystemInfo.processorFrequency}");
			builder.AppendLine("processorType: " + SystemInfo.processorType);
			builder.AppendLine($"systemMemorySize: {SystemInfo.systemMemorySize}");
			builder.AppendLine($"deviceType: {SystemInfo.deviceType}");
			builder.AppendLine("graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
			builder.AppendLine($"graphicsDeviceType: {SystemInfo.graphicsDeviceType}");
			builder.AppendLine("graphicsDeviceVendor: " + SystemInfo.graphicsDeviceVendor);
			builder.AppendLine("graphicsDeviceVersion: " + SystemInfo.graphicsDeviceVersion);
			builder.AppendLine($"graphicsMemorySize: {SystemInfo.graphicsMemorySize}");
			builder.AppendLine($"graphicsMultiThreaded: {SystemInfo.graphicsMultiThreaded}");
			builder.AppendLine($"graphicsShaderLevel: {SystemInfo.graphicsShaderLevel}");
			builder.AppendLine($"maxTextureSize: {SystemInfo.maxTextureSize}");
			builder.AppendLine($"npotSupport: {SystemInfo.npotSupport}");
			builder.AppendLine($"supportedRenderTargetCount: {SystemInfo.supportedRenderTargetCount}");
			builder.AppendLine("moneHeapSize: " + HumanReadableNumbers(Profiler.GetMonoHeapSizeLong()));
			builder.AppendLine("monoUsedSize: " + HumanReadableNumbers(Profiler.GetMonoUsedSizeLong()));
			builder.AppendLine("tempAllocatorSize: " + HumanReadableNumbers(Profiler.GetTempAllocatorSize()));
			builder.AppendLine("totalReservedMemory: " + HumanReadableNumbers(Profiler.GetTotalReservedMemoryLong()));
			builder.AppendLine("totalAllocatedMemory: " + HumanReadableNumbers(Profiler.GetTotalAllocatedMemoryLong()));
			builder.AppendLine("totalUnusedReservedMemory: " + HumanReadableNumbers(Profiler.GetTotalUnusedReservedMemoryLong()));
			builder.AppendLine($"screenMode: {Screen.fullScreenMode}");
			builder.AppendLine($"screenResolution: {Screen.currentResolution} {Screen.width}x{Screen.height}");
			File.WriteAllText(text, builder.ToString());
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create systemInfo file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void CreateMessageFile(string message)
	{
		try
		{
			string text = Path.Combine(CurrentReportFolder, "message.txt");
			File.WriteAllText(text, message);
			Logger.Log("Create " + text);
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create message file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private string CreateParametersFile(string email, string uniqueIdentifier, string issueType, string message, List<string> historyLog, string additionalContacts = "", bool isSendMarketing = false)
	{
		try
		{
			string fileName = Path.GetFileName(CurrentReportFolder);
			BlueprintAreaPart currentAreaPart = CheatsJira.GetCurrentAreaPart();
			string text = CheatsJira.GetCurrentAreaPart()?.StaticScene.SceneName;
			string lightScene = CheatsJira.GetLightScene();
			string text2 = "";
			string text3 = "";
			try
			{
				text2 = new PartyContext(m_ItemUseHistory, m_SpellCastHistory).ToString();
				text3 = new ExtendedContext(historyLog).ToString();
			}
			catch
			{
			}
			try
			{
				text2 = Regex.Unescape(text2);
				text3 = Regex.Unescape(text3);
			}
			catch
			{
			}
			string modifiedSaveFiles;
			if (ReportingCheats.IsDebugSaveFileChecksumEnabled)
			{
				try
				{
					using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
					StringBuilder builder = pooledStringBuilder.Builder;
					modifiedSaveFiles = (from filePath in Directory.GetFiles(CurrentReportFolder)
						where filePath.EndsWith(".zks") && File.Exists(filePath)
						select m_ReportFilesMd5Manager.GetCompareChecksumsString(filePath) into s
						where !string.IsNullOrEmpty(s)
						select s).Aggregate(builder, (StringBuilder current, string s) => current.Append(s)).ToString();
				}
				catch (Exception ex)
				{
					modifiedSaveFiles = ex.Message;
				}
			}
			else
			{
				modifiedSaveFiles = "check_disabled";
			}
			string text4;
			try
			{
				text4 = new ModContext().ToString();
			}
			catch (Exception ex2)
			{
				text4 = ex2.Message;
			}
			if (string.IsNullOrEmpty(text4))
			{
				text4 = "None";
			}
			string otherContext;
			try
			{
				otherContext = new OtherContext(m_ContextVariants, m_Context).ToString();
			}
			catch (Exception ex3)
			{
				otherContext = ex3.Message;
			}
			string label = string.Empty;
			if (!string.IsNullOrEmpty(m_UiFeatureName))
			{
				label = ReportingRaycaster.Instance.GetJiraLabel(m_UiFeatureName);
			}
			string labels = "";
			List<string> list = new List<string>();
			string coopPlayersCount = "";
			foreach (KeyValuePair<string, bool> item in m_labelsDictionary)
			{
				if (item.Value)
				{
					list.Add(item.Key);
				}
			}
			if (NetworkingManager.IsActive)
			{
				list.Add("Coop");
				coopPlayersCount = NetworkingManager.PlayersCount.ToString();
			}
			if (m_Context.Type == BugContext.ContextType.Encounter)
			{
				list.Add(m_Context.ContextObject.NameSafe() + "_Encounters");
			}
			if (m_Context.Type == BugContext.ContextType.SurfaceCombat)
			{
				list.Add("SurfaceCombat");
			}
			if (m_Context.Type == BugContext.ContextType.SpaceCombat)
			{
				list.Add("SpaceCombat");
			}
			if (m_Context.Type == BugContext.ContextType.Dialog && (m_Context.Aspect == BugContext.AspectType.Code || m_Context.Aspect == BugContext.AspectType.Mechanics || m_Context.Aspect == BugContext.AspectType.UI || m_Context.Aspect == BugContext.AspectType.Sound || m_Context.Aspect == BugContext.AspectType.None))
			{
				list.Add("Dialog");
			}
			if (m_Context.Type == BugContext.ContextType.Item)
			{
				list.Add("Loot");
			}
			if (m_Context.Type == BugContext.ContextType.CharacterClass)
			{
				list.Add("Careers");
			}
			if (m_Context.Type == BugContext.ContextType.Exploration)
			{
				list.Add("Exploration");
			}
			if (list.Count > 0)
			{
				labels = string.Join("|", list.ToArray());
			}
			string empty = string.Empty;
			string text5 = ((currentAreaPart == null) ? empty : GetChapterNormalOrCrash().ToString());
			if (Regex.IsMatch(text5, "^\\d+$"))
			{
				text5 = "Chapter0" + text5;
			}
			ReportParameters value = new ReportParameters
			{
				Project = "WH",
				Email = (string.IsNullOrEmpty(email) ? empty : email),
				Discord = (string.IsNullOrEmpty(additionalContacts) ? empty : additionalContacts),
				IsSendMarketingMats = isSendMarketing,
				ReportDateTime = DateTime.UtcNow,
				PlayerLanguage = LocalizationManager.Instance.CurrentLocale.ToString(),
				operatingSystem = SystemInfo.operatingSystem,
				operatingSystemFamily = SystemInfo.operatingSystemFamily.ToString(),
				UniqueIdentifier = (string.IsNullOrEmpty(uniqueIdentifier) ? empty : uniqueIdentifier),
				Blueprint = m_Context.GetContextObjectBlueprintName(),
				BlueprintArea = ((currentAreaPart == null) ? empty : Utilities.GetBlueprintName(currentAreaPart)),
				Chapter = text5,
				Version = (string.IsNullOrEmpty(GameVersion.GetVersion()) ? empty : GameVersion.GetVersion()),
				Guid = (string.IsNullOrEmpty(fileName) ? empty : fileName),
				IssueType = (string.IsNullOrEmpty(issueType) ? empty : issueType),
				StaticScene = (string.IsNullOrEmpty(text) ? empty : text),
				LightScene = (string.IsNullOrEmpty(lightScene) ? empty : lightScene),
				MainCharacter = $"{Utilities.GetBlueprintName(GetBlueprintRace())}/{GetGender()}",
				KingdomEvent = empty,
				CurrentDialog = m_Context.GetDialogGuid(),
				AreaDesigner = Utilities.GetDesigner(CheatsJira.GetCurrentArea()),
				Context = m_Context.Type.ToString("G"),
				Aspect = m_Context.Aspect.ToString("G"),
				FixVersion = m_SelectedFixVersion,
				ExtendedContext = text3,
				PartyContext = text2,
				OtherContext = otherContext,
				Cutscenes = CheatsJira.GetCutscenesInfo(),
				SuggestedAssignee = ((m_SuggestedAssignee != "Assignee to me") ? m_SuggestedAssignee : email.Split("@")[0]),
				ModifiedSaveFiles = modifiedSaveFiles,
				Revision = ReportVersionManager.GetCommitOrRevision(),
				Label = label,
				Labels = labels,
				ModManagerMods = text4,
				Store = StoreManager.Store.ToString(),
				ControllerModeType = Convert.ToString(Game.Instance.ControllerMode),
				Platform = Application.platform.ToString(),
				ConsoleHardwareType = getConsoleHardwareType(),
				Exception = JsonConvert.SerializeObject(m_Exception),
				CoopPlayersCount = coopPlayersCount
			};
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				PreserveReferencesHandling = PreserveReferencesHandling.None
			};
			string contents = JsonConvert.SerializeObject(value, settings);
			string text6 = Path.Combine(CurrentReportFolder, "parameters.json");
			File.WriteAllText(text6, contents);
			Logger.Log("Create " + text6);
			string contents2 = JsonConvert.SerializeObject(GameMetaData.Create(ReportDllChecksumManager.GetDllCRC(), ReportDllChecksumManager.IsUnityModManagerActive()), settings);
			string text7 = Path.Combine(CurrentReportFolder, "GameMetadata.json");
			File.WriteAllText(text7, contents2);
			Logger.Log("Create " + text7);
			string header = m_Context.GetHeader();
			string contextLink = m_Context.GetContextLink();
			message = AddErrorMessages(message);
			message = message.Replace("\v", "\r\n");
			return header + " " + message + "\n\n\n\n" + contextLink;
		}
		catch (Exception ex4)
		{
			LogReporterError("Failed create parameters file: \n" + ex4.Message + "\n" + ex4.StackTrace);
			return message;
		}
		static string getConsoleHardwareType()
		{
			return string.Empty;
		}
	}

	private void CreateCombatLogFile()
	{
		try
		{
			string text = Path.Combine(CurrentReportFolder, "combatLog.txt");
			m_ReportCombatLogManager.CopyFile(text, this);
			Logger.Log("Copy " + text);
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create combat log file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private static BlueprintRace GetBlueprintRace()
	{
		return (Game.Instance?.Player.MainCharacterEntity)?.Blueprint.Race;
	}

	private static Gender GetGender()
	{
		return (Game.Instance?.Player.MainCharacterEntity)?.Blueprint.Gender ?? Gender.Male;
	}

	private void CreateReporterErrorLog()
	{
		try
		{
			if (m_ReporterErrors.Count == 0)
			{
				return;
			}
			string text = Path.Combine(CurrentReportFolder, "ReporterErrorLog.txt");
			StreamWriter streamWriter = new StreamWriter(text);
			foreach (string reporterError in m_ReporterErrors)
			{
				streamWriter.WriteLine(reporterError);
			}
			m_ReporterErrors.Clear();
			streamWriter.Close();
			Logger.Log("Create " + text);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed to create ReporterErrorLog file");
		}
	}

	private void CreateScreenshot()
	{
		try
		{
			if (string.IsNullOrEmpty(CurrentReportFolder))
			{
				return;
			}
			string text = Path.Combine(CurrentReportFolder, "screen.png");
			Logger.Log("Create screenshot for bug report at '" + text + "'");
			string text2 = Path.Combine(ApplicationPaths.persistentDataPath, "crash_screen.png");
			if (File.Exists(text2))
			{
				File.Move(text2, text);
				return;
			}
			Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
			if (texture2D != null)
			{
				byte[] bytes = texture2D.EncodeToPNG();
				File.WriteAllBytes(text, bytes);
				UnityEngine.Object.Destroy(texture2D);
			}
			else
			{
				ScreenCapture.CaptureScreenshot(text);
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create screenshot file: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private IEnumerator CreateSave()
	{
		try
		{
			Logger.Log("Create save for bug report");
			m_TemporarySaveLabel = string.Empty;
			if (Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Bugreport))
			{
				m_TemporarySaveLabel = "SavedFromBug";
				SaveInfo saveInfo = Game.Instance.SaveManager.CreateNewSave(m_TemporarySaveLabel);
				saveInfo.Type = SaveInfo.SaveType.Bugreport;
				m_WaitForSave = true;
				Game.Instance.GameCommandQueue.SaveGame(saveInfo, null, delegate
				{
					m_WaitForSave = false;
				});
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed create save: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		while (m_WaitForSave)
		{
			yield return null;
		}
	}

	private void CopyLastAutosave()
	{
		try
		{
			Logger.Log("Copy last autosave for bug report");
			SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave((SaveInfo save) => save.Type == SaveInfo.SaveType.Auto && save.GameId == Game.Instance.Player.GameId);
			if (latestSave == null)
			{
				Logger.Log("No autosave found");
			}
			else if (string.IsNullOrEmpty(latestSave.FolderName))
			{
				Logger.Log("Folder name is " + ((latestSave.FolderName == null) ? "<null>" : "empty"));
			}
			else
			{
				CopySaveFile(latestSave, CurrentReportFolder, "lastAutosave.zks");
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed copy last autoSave: \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	private void AddCrushDump()
	{
		string @string = PlayerPrefs.GetString("LatestCrashdump", "");
		Logger.Log("Add crush dump for bug report");
		string[] array = new string[3] { "crash.dmp", "error.log", "Player.log" };
		foreach (string path in array)
		{
			try
			{
				string text = Path.Combine(@string, path);
				if (File.Exists(text))
				{
					File.Copy(text, Path.Combine(CurrentReportFolder, path), overwrite: true);
				}
			}
			catch (Exception ex)
			{
				LogReporterError("Failed copy file: \n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	private void CopyLogFiles()
	{
		if (string.IsNullOrEmpty(CurrentReportFolder))
		{
			return;
		}
		string bugReportsPath = BugReportsPath;
		(string, string)[] array = new(string, string)[16]
		{
			(Path.Combine(Application.dataPath, "output_log.txt"), Path.Combine(CurrentReportFolder, "output_log.txt")),
			(Path.Combine(Application.dataPath, "log.txt"), Path.Combine(CurrentReportFolder, "log0.txt")),
			(Path.Combine(ApplicationPaths.persistentDataPath, "output_log.txt"), Path.Combine(CurrentReportFolder, "output_log0.txt")),
			(Path.Combine(ApplicationPaths.persistentDataPath, "log.txt"), Path.Combine(CurrentReportFolder, "log.txt")),
			(Path.Combine(bugReportsPath, "SentReports.txt"), Path.Combine(CurrentReportFolder, "SentReports.txt")),
			(Path.Combine(ApplicationPaths.LogsDir, "GameLog.txt"), Path.Combine(CurrentReportFolder, "GameLog.txt")),
			(Path.Combine(ApplicationPaths.LogsDir, "GameLogFullPrev.txt"), Path.Combine(CurrentReportFolder, "GameLogFullPrev.txt")),
			(Path.Combine(ApplicationPaths.LogsDir, "GameLogFull.txt"), Path.Combine(CurrentReportFolder, "GameLogFull.txt")),
			(Path.Combine(ApplicationPaths.persistentDataPath, "game-history.txt"), Path.Combine(CurrentReportFolder, "game-history.txt")),
			(Path.Combine(Application.dataPath, "..", "EditorLogFull.txt"), Path.Combine(CurrentReportFolder, "EditorLogFull.txt")),
			(Path.Combine(Application.dataPath, "..", "EditorLog.txt"), Path.Combine(CurrentReportFolder, "EditorLog.txt")),
			(Path.Combine(SettingsController.Instance.GeneralSettingsProviderPath), Path.Combine(CurrentReportFolder, "general_settings.json")),
			(Path.Combine(ApplicationPaths.LogsDir, "ConsoleLogFull.txt"), Path.Combine(CurrentReportFolder, "ConsoleLogFull.txt")),
			(Path.Combine(ApplicationPaths.LogsDir, "ConsoleLogFullPrev.txt"), Path.Combine(CurrentReportFolder, "ConsoleLogFullPrev.txt")),
			(AssigneeContainer.AssigneeLocalCacheFileName, Path.Combine(CurrentReportFolder, "assignee.json")),
			(BuildModeUtility.ActualPathToStartUp, Path.Combine(CurrentReportFolder, "startup.json"))
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (text, text2) = array[i];
			try
			{
				if (!File.Exists(text))
				{
					LogReporterError("File " + text + " does not exists");
					continue;
				}
				File.Copy(text, text2, overwrite: true);
				Logger.Log("Copy " + text + " to " + text2);
			}
			catch (Exception ex)
			{
				LogReporterError("Failed to copy log file: \n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	private static IEnumerable<BugContext> MakeBugContextList(params BlueprintScriptableObject[] objects)
	{
		foreach (BlueprintScriptableObject item in objects.Where((BlueprintScriptableObject v) => v != null))
		{
			BugContext bugContext = new BugContext(item);
			if (item is BlueprintActivatableAbility || item is BlueprintAbility)
			{
				BaseUnitEntity selectedPartyCharUnitEntityData = GetSelectedPartyCharUnitEntityData();
				if (selectedPartyCharUnitEntityData != null)
				{
					bugContext.AdditionalContextObjects.Add(selectedPartyCharUnitEntityData.Progression.Classes.Last()?.CharacterClass);
				}
			}
			yield return bugContext;
		}
	}

	private void CollectBugContextVariants(bool addCrashDump)
	{
		try
		{
			m_Context = null;
			m_ContextVariants.Clear();
			if (addCrashDump)
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.Crash));
				m_Context = m_ContextVariants[0];
				return;
			}
			if (m_Exception != null)
			{
				if (m_Exception is SpamDetectingException)
				{
					m_ContextVariants.Add(new BugContext(BugContext.ContextType.ExceptionSpam));
				}
				else
				{
					m_ContextVariants.Add(new BugContext(BugContext.ContextType.Exception));
				}
				m_Context = m_ContextVariants[0];
				return;
			}
			m_ContextVariants.Add(new BugContext(Game.Instance?.DialogController.Dialog));
			m_ContextVariants.AddRange(MakeBugContextList(CheatsJira.Tooltip()));
			if (!string.IsNullOrEmpty(m_UiFeatureName))
			{
				try
				{
					List<string> source = m_UiFeatureName.Split(' ').ToList();
					m_ContextVariants.AddRange(source.Select(CreateContextFromFeature));
				}
				catch
				{
					m_UiFeatureName = string.Empty;
				}
			}
			BlueprintUnit blueprintUnit = null;
			bool flag = false;
			if (m_ActiveFullScreenUIType == FullScreenUIType.Unknown)
			{
				blueprintUnit = m_HoveredPortraitUnitBlueprint;
				if (blueprintUnit == null)
				{
					BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
					if (unitUnderMouse != null)
					{
						flag = unitUnderMouse.IsInCombat;
						blueprintUnit = unitUnderMouse.Blueprint;
						if (blueprintUnit != null)
						{
							m_ContextVariants.Add(new BugContext(blueprintUnit));
						}
					}
				}
				else
				{
					flag = true;
					m_ContextVariants.Add(new BugContext(blueprintUnit));
				}
			}
			BlueprintUnit blueprintUnit2 = SimpleBlueprintExtendAsObject.Or(blueprintUnit, null);
			if (blueprintUnit2 != null && blueprintUnit2.IsCompanion && m_ActiveFullScreenUIType == FullScreenUIType.Unknown)
			{
				foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
				{
					if (partyCharacter.Entity.ToBaseUnitEntity().Blueprint != blueprintUnit)
					{
						continue;
					}
					foreach (ClassData @class in partyCharacter.Entity.ToBaseUnitEntity().Progression.Classes)
					{
						BugContext bugContext = new BugContext(BugContext.ContextType.CharacterClass);
						bugContext.ContextObject = @class.CharacterClass;
						bugContext.AdditionalContextObjects.Add(partyCharacter.Entity.ToBaseUnitEntity().Blueprint);
						m_ContextVariants.Add(bugContext);
						using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
						StringBuilder builder = pooledStringBuilder.Builder;
						string classArchetypes = @class.Archetypes.Aggregate(builder, (StringBuilder current, BlueprintArchetype c) => current.Append(c.Name).Append('\n')).ToString();
						bugContext.ClassArchetypes = classArchetypes;
					}
				}
			}
			if (MainMenuUI.IsActive || Game.Instance.CurrentMode == GameModeType.GameOver || m_ActiveFullScreenUIType == FullScreenUIType.Journal || m_ActiveFullScreenUIType == FullScreenUIType.Encyclopedia || Game.Instance.CurrentMode == GameModeType.Default)
			{
				BlueprintScriptableObject underMouseBlueprint = ReportingRaycaster.Instance.GetUnderMouseBlueprint();
				if (underMouseBlueprint != null)
				{
					try
					{
						BugContext item = new BugContext(underMouseBlueprint);
						m_ContextVariants.Add(item);
					}
					catch
					{
					}
				}
				else
				{
					string featureProgressionBlueprintName = ReportingRaycaster.Instance.GetFeatureProgressionBlueprintName();
					if (!string.IsNullOrEmpty(featureProgressionBlueprintName))
					{
						BugContext item2 = new BugContext(BugContext.ContextType.Interface)
						{
							UiFeature = "LevelUp(" + featureProgressionBlueprintName + ")",
							UiFeatureAssignee = "turchin"
						};
						m_ContextVariants.Add(item2);
					}
				}
			}
			if (m_IsGlobalMapOpened && !m_ContextVariants.Any((BugContext x) => x.ContextObject != null))
			{
				BlueprintScriptableObject underMouseBlueprint2 = ReportingRaycaster.Instance.GetUnderMouseBlueprint();
				if (underMouseBlueprint2 != null)
				{
					try
					{
						BugContext item3 = new BugContext(underMouseBlueprint2, BugContext.ContextType.Crusade);
						m_ContextVariants.Add(item3);
					}
					catch
					{
					}
				}
			}
			TooltipData tooltipData = MouseHoverBlueprintSystem.Instance.TooltipData;
			if (tooltipData != null)
			{
				try
				{
					TooltipBaseTemplate mainTemplate = tooltipData.MainTemplate;
					BugContext.ContextType context = ((Game.Instance.IsModeActive(GameModeType.SpaceCombat) || Game.Instance.IsModeActive(GameModeType.StarSystem)) ? BugContext.ContextType.SpellSpace : BugContext.ContextType.Spell);
					if (!(mainTemplate is TooltipTemplateBuff tooltipTemplateBuff))
					{
						if (!(mainTemplate is TooltipTemplateFeature tooltipTemplateFeature))
						{
							if (!(mainTemplate is TooltipTemplateUIFeature tooltipTemplateUIFeature))
							{
								if (!(mainTemplate is TooltipTemplateAbility tooltipTemplateAbility))
								{
									if (!(mainTemplate is TooltipTemplateActivatableAbility tooltipTemplateActivatableAbility))
									{
										if (mainTemplate is TooltipTemplateItem tooltipTemplateItem && !m_ContextVariants.Any((BugContext x) => x.Type == BugContext.ContextType.Item))
										{
											try
											{
												BugContext bugContext2 = new BugContext(BugContext.ContextType.Item);
												if (tooltipTemplateItem.Item != null)
												{
													bugContext2.ContextObject = tooltipTemplateItem.Item.Blueprint;
													goto IL_05f5;
												}
												ItemEntity itemEntity = (ItemEntity)typeof(TooltipTemplateItem).GetField("m_Item", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tooltipTemplateItem);
												if (itemEntity != null)
												{
													bugContext2.ContextObject = itemEntity.Blueprint;
													goto IL_05f5;
												}
												goto end_IL_0595;
												IL_05f5:
												m_ContextVariants.Add(bugContext2);
												end_IL_0595:;
											}
											catch (Exception ex)
											{
												PFLog.Default.Log("BugReport collect  Item tooltip exception: " + ex.Message + "\n" + ex.StackTrace);
											}
										}
									}
									else
									{
										BugContext bugContext2 = new BugContext(context);
										bugContext2.ContextObject = tooltipTemplateActivatableAbility.BlueprintActivatableAbility;
										m_ContextVariants.Add(bugContext2);
									}
								}
								else
								{
									BugContext bugContext2 = new BugContext(context);
									bugContext2.ContextObject = tooltipTemplateAbility.BlueprintAbility;
									m_ContextVariants.Add(bugContext2);
								}
							}
							else
							{
								BugContext bugContext2 = new BugContext(context);
								bugContext2.ContextObject = tooltipTemplateUIFeature.UIFeature.Feature;
								m_ContextVariants.Add(bugContext2);
							}
						}
						else
						{
							BugContext bugContext2 = new BugContext(context);
							bugContext2.ContextObject = tooltipTemplateFeature.BlueprintFeatureBase;
							m_ContextVariants.Add(bugContext2);
						}
					}
					else
					{
						BugContext bugContext2 = new BugContext(context);
						bugContext2.ContextObject = tooltipTemplateBuff.Buff.Blueprint;
						m_ContextVariants.Add(bugContext2);
					}
				}
				catch (Exception ex2)
				{
					PFLog.Default.Log("BugReport collect tooltip exception: " + ex2.Message + "\n" + ex2.StackTrace);
				}
			}
			BlueprintArea currentArea = CheatsJira.GetCurrentArea();
			if (currentArea != null)
			{
				m_ContextVariants.Add(new BugContext(currentArea));
				if (currentArea is BlueprintStarSystemMap)
				{
					m_ContextVariants.Add(new BugContext(currentArea, BugContext.ContextType.Exploration));
					if (StarSystemObjectStateVM.Instance.HasColony)
					{
						m_ContextVariants.Add(new BugContext(BugContext.ContextType.Colonization));
					}
				}
			}
			string otherUiFeatureName = ReportingRaycaster.Instance.GetOtherUiFeatureName();
			if (otherUiFeatureName == "Ingame Menu")
			{
				BugContext bugContext3 = new BugContext(BugContext.ContextType.Interface);
				bugContext3.ActiveFullScreenUIType = m_ActiveFullScreenUIType;
				bugContext3.OtherUiFeature = otherUiFeatureName;
				m_ContextVariants.Add(bugContext3);
			}
			if (Game.Instance.IsModeActive(GameModeType.SpaceCombat))
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.SpaceCombat));
			}
			else if (Game.Instance.Player.IsInCombat)
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.SurfaceCombat));
				m_ContextVariants.Add(new BugContext(CheatsJira.GetCurrentArea(), BugContext.ContextType.Encounter));
			}
			if (CheatsJira.GetCurrentArea().NameSafe().Contains("KoronusExpanse"))
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.GlobalMap));
			}
			if (NetworkingManager.IsActive)
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.Coop));
			}
			if (PhotonManager.Initialized && PhotonManager.Sync.HasDesync)
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.Desync));
			}
			m_ContextVariants.Remove((BugContext x) => x.Type == BugContext.ContextType.Area && x.ContextObject == null);
			m_ContextVariants.Sort();
			if (!flag && blueprintUnit != null)
			{
				List<BugContext> collection = new List<BugContext>(m_ContextVariants);
				try
				{
					BugContext bugContext4 = m_ContextVariants.FirstOrDefault((BugContext x) => x.Type == BugContext.ContextType.Area);
					if (bugContext4 != null)
					{
						List<BugContext> list = m_ContextVariants.SkipWhile((BugContext x) => x.Type < BugContext.ContextType.Unit).ToList();
						list.Remove(bugContext4);
						List<BugContext> collection2 = m_ContextVariants.TakeWhile((BugContext x) => x.Type <= BugContext.ContextType.Army).ToList();
						m_ContextVariants.Clear();
						m_ContextVariants.AddRange(collection2);
						m_ContextVariants.Add(bugContext4);
						m_ContextVariants.AddRange(list);
					}
				}
				catch
				{
					m_ContextVariants.Clear();
					m_ContextVariants.AddRange(collection);
				}
			}
			if (m_ContextVariants.Count((BugContext x) => x.Type == BugContext.ContextType.Unit && x.ContextObject != null) > 1)
			{
				int num = 0;
				for (int num2 = m_ContextVariants.Count - 1; num2 > 0; num2--)
				{
					if (m_ContextVariants[num2].Type == BugContext.ContextType.Unit)
					{
						num = num2;
						break;
					}
				}
				if (num > 0)
				{
					m_ContextVariants.RemoveAt(num);
				}
			}
			if (otherUiFeatureName == "Bark")
			{
				BugContext bugContext5 = new BugContext(BugContext.ContextType.Interface);
				bugContext5.ActiveFullScreenUIType = m_ActiveFullScreenUIType;
				bugContext5.OtherUiFeature = otherUiFeatureName;
				m_ContextVariants.Add(bugContext5);
			}
			if (ExceptionSource != null && !(ExceptionSource is Cutscene))
			{
				m_ContextVariants.Add(new BugContext(ExceptionSource));
				ExceptionSource = null;
			}
			BugContext bugContext6 = m_ContextVariants.FirstOrDefault((BugContext x) => x.UiFeature == "EscMenu");
			if (bugContext6 != null)
			{
				m_ContextVariants.Remove(bugContext6);
				m_ContextVariants.Insert(0, bugContext6);
			}
			if (GameVersion.Mode == BuildMode.Development)
			{
				m_ContextVariants.Add(new BugContext(BugContext.ContextType.Debug));
			}
			if (m_Context == null && m_ContextVariants.Count > 0)
			{
				m_Context = m_ContextVariants[0];
			}
		}
		catch (Exception ex3)
		{
			LogReporterError("Failed to collect bug contexts: \n" + ex3.Message + "\n" + ex3.StackTrace);
		}
		finally
		{
			if (m_Context == null)
			{
				m_Context = new BugContext(BugContext.ContextType.None);
				m_ContextVariants.Clear();
				m_ContextVariants.Add(m_Context);
			}
		}
	}

	private BugContext CreateContextFromFeature(string featureName)
	{
		if (featureName == "Colonization")
		{
			return new BugContext(BugContext.ContextType.Colonization);
		}
		return new BugContext(BugContext.ContextType.Interface)
		{
			UiFeature = featureName,
			UiFeatureAssignee = "ui_designer"
		};
	}

	public IEnumerator MakeNewReport(bool makeScreenshot, bool makeSave, bool addCrashDump)
	{
		yield return new WaitForEndOfFrame();
		try
		{
			Logger.Log("Create new bug report");
			CollectBugContextVariants(addCrashDump);
			m_ContextVariants.Sort(new BugContextComparer());
			CreateReportFolder();
			if (makeScreenshot)
			{
				CreateScreenshot();
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to MakeNewReport: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		if (makeSave)
		{
			yield return CreateSave();
		}
		try
		{
			if (makeSave)
			{
				CopyLastAutosave();
			}
			if (addCrashDump)
			{
				AddCrushDump();
			}
		}
		catch (Exception ex2)
		{
			LogReporterError("Failed to MakeNewReport: \n" + ex2.Message + "\n" + ex2.StackTrace);
		}
	}

	public void SendReport(string message, string email, string uniqueIdentifier, string issueType, string additionalContacts = "", bool isSendMarketing = false, string id = null)
	{
		if (id == null && NetworkingManager.IsMultiplayer)
		{
			id = Guid.NewGuid().ToString("N");
			message = "[" + id + "] " + message;
			PFLog.Net.Log($"New multiplayer bug report id={id} player={NetworkingManager.LocalNetPlayer.Index}/{NetworkingManager.PlayersCount}");
		}
		try
		{
			if (string.IsNullOrEmpty(CurrentReportFolder))
			{
				CurrentReportFolder = BugReportsPath;
			}
			if (!Directory.Exists(CurrentReportFolder))
			{
				Directory.CreateDirectory(CurrentReportFolder);
			}
			if (!string.IsNullOrEmpty(CurrentReportFolder))
			{
				Utilities.CreateGameHistoryLog();
				string text = CreateSaveFileForBugReport(CurrentReportFolder);
				List<string> historyLog = new List<string>();
				try
				{
					string fullName = new DirectoryInfo(text).FullName;
					fullName = fullName.Substring(0, fullName.LastIndexOf(Path.DirectorySeparatorChar));
					fullName = Path.Combine(fullName, "history");
					if (!string.IsNullOrEmpty(text))
					{
						using (ZipArchive zipArchive = ZipFile.OpenRead(text))
						{
							using IEnumerator<ZipArchiveEntry> enumerator = zipArchive.Entries.Where((ZipArchiveEntry e) => e.Name.Equals("history")).GetEnumerator();
							if (enumerator.MoveNext())
							{
								enumerator.Current.ExtractToFile(fullName);
							}
						}
						if (File.Exists(fullName))
						{
							historyLog = File.ReadAllLines(fullName).ToList();
							File.Delete(fullName);
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Exception(ex, "Exception trying parse history");
				}
				CreateSystemInfoFile();
				SettingsController.Instance.ConfirmAllTempValues();
				SettingsController.Instance.SaveAll();
				CopyLogFiles();
				string message2 = CreateParametersFile(email, uniqueIdentifier, issueType, message, historyLog, additionalContacts, isSendMarketing);
				CreateMessageFile(message2);
				CreateReporterErrorLog();
				CreateCombatLogFile();
				m_DiscordContacts = additionalContacts;
				string text2 = CurrentReportFolder + ".zks";
				ZipFile.CreateFromDirectory(CurrentReportFolder, text2);
				Logger.Log("Create " + text2);
				if (Directory.Exists(CurrentReportFolder))
				{
					Directory.Delete(CurrentReportFolder, recursive: true);
				}
				Logger.Log("Delete folder '" + CurrentReportFolder + "'");
				LastEntry = ReportSender?.Enqueue(text2);
				CurrentReportFolder = string.Empty;
				PlayerPrefs.SetInt("BugReportMarketingMaterialsToggle", isSendMarketing ? 1 : 0);
			}
		}
		catch (Exception ex2)
		{
			Logger.Exception(ex2, "Failed send report {0}", CurrentReportFolder);
		}
		PhotonManager.BugReport.Sync(id, message, issueType);
	}

	public void Clear()
	{
		try
		{
			if (string.IsNullOrEmpty(CurrentReportFolder))
			{
				return;
			}
			if (Directory.Exists(CurrentReportFolder))
			{
				Directory.Delete(CurrentReportFolder, recursive: true);
			}
			CurrentReportFolder = string.Empty;
			if (!string.IsNullOrEmpty(m_TemporarySaveLabel))
			{
				SaveInfo latestSave = Game.Instance.SaveManager.GetLatestSave();
				if (latestSave != null && File.Exists(latestSave.FolderName) && latestSave.Name.Contains(m_TemporarySaveLabel))
				{
					Game.Instance.SaveManager.DeleteSave(latestSave);
				}
				m_TemporarySaveLabel = string.Empty;
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed to clear");
		}
	}

	public (string Description, List<(BugContext.AspectType? Aspect, string Assignee)> Assignees)[] GetContextDescriptions()
	{
		List<(string, List<(BugContext.AspectType?, string)>)> list = new List<(string, List<(BugContext.AspectType?, string)>)>();
		foreach (BugContext contextVariant in m_ContextVariants)
		{
			string contextDescription = contextVariant.GetContextDescription();
			List<(BugContext.AspectType?, string)> contextAspectAssignees = contextVariant.GetContextAspectAssignees();
			List<(BugContext.AspectType?, string)> collection = (from x in BugContext.GetLeadAssignees()
				select ((BugContext.AspectType?, string x))(null, x: x)).ToList();
			contextAspectAssignees.AddRange(collection);
			contextAspectAssignees.Add((null, "Assignee to me"));
			list.Add((contextDescription, contextAspectAssignees));
		}
		if (list.Count == 0)
		{
			list.Add(("None", null));
		}
		return list.ToArray();
	}

	public void SelectContext(int contextIdx)
	{
		if (contextIdx >= 0 && contextIdx < m_ContextVariants.Count)
		{
			m_Context = m_ContextVariants[contextIdx];
		}
	}

	public void SelectAspect(int aspectIdx)
	{
		m_Context?.SelectAspect(aspectIdx);
	}

	public void SelectAssignee(string assignee)
	{
		m_SuggestedAssignee = assignee;
	}

	public void SelectFixVersion(int versionIdx)
	{
		if (versionIdx >= 0 && versionIdx < FixVersions.Length)
		{
			m_SelectedFixVersion = FixVersions[versionIdx];
		}
	}

	public bool CheckCrashDumpFound()
	{
		try
		{
			string fullPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "..\\..\\..\\Local\\Temp\\Owlcat Games\\Pathfinder Wrath Of The Righteous\\Crashes\\"));
			if (Application.platform == RuntimePlatform.OSXPlayer)
			{
				fullPath = Path.GetFullPath(Path.Combine(ApplicationPaths.persistentDataPath, "\\Users\\g\\Library\\Logs\\DiagnosticReports\\"));
			}
			string @string = PlayerPrefs.GetString("LatestCrashdump", "");
			if (@string.IsNullOrEmpty())
			{
				if (Directory.Exists(fullPath))
				{
					foreach (string item in Directory.EnumerateDirectories(fullPath))
					{
						Directory.Delete(item, recursive: true);
					}
				}
				PlayerPrefs.SetString("LatestCrashdump", "we are ok, no dumps!");
				PlayerPrefs.Save();
			}
			else
			{
				if (!Directory.Exists(fullPath))
				{
					return false;
				}
				IEnumerable<string> source = Directory.EnumerateDirectories(fullPath);
				if (source.Empty())
				{
					return false;
				}
				string text = source.OrderBy((string x) => Directory.GetCreationTime(x)).Last();
				if (@string != text)
				{
					PlayerPrefs.SetString("LatestCrashdump", text);
					PlayerPrefs.Save();
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			LogReporterError("Failed to check for available crash dumps: \n" + ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	public void HandleFullScreenUiChanged(bool active, FullScreenUIType fullScreenUIType)
	{
		m_ActiveFullScreenUIType = (active ? fullScreenUIType : FullScreenUIType.Unknown);
	}

	public void HandleFullScreenUIItJustWorks(bool active, FullScreenUIType fullScreenUIType)
	{
		HandleFullScreenUiChanged(active, fullScreenUIType);
	}

	public void LogReporterError(string message)
	{
		Logger.Log(message);
		m_ReporterErrors.Add(message);
	}

	public void HandlePortraitHover(bool hover)
	{
		m_HoveredPortraitUnitBlueprint = (hover ? EventInvokerExtensions.BaseUnitEntity.Blueprint : null);
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		string contextName = evt.Spell.Blueprint.NameSafe();
		string assetGuid = evt.Spell.Blueprint.AssetGuid;
		string partyMemberName = evt.Spell.Caster.GetDescriptionOptional()?.Name ?? evt.Spell.Caster.Blueprint.Name;
		PartyContext.ReportParameterHelper item = new PartyContext.ReportParameterHelper
		{
			Guid = assetGuid,
			ContextName = contextName,
			PartyMemberName = partyMemberName,
			ContextType = BugContext.InnerContextType.Spell.ToString()
		};
		if (m_SpellCastHistory.Count > 500)
		{
			m_SpellCastHistory.RemoveRange(0, 100);
		}
		m_SpellCastHistory.Add(item);
		string text = evt.Spell.SourceItem?.Blueprint.AssetGuid ?? string.Empty;
		if (!(text == string.Empty))
		{
			string contextName2 = evt.Spell.SourceItem?.Blueprint.NameSafe();
			PartyContext.ReportParameterHelper item2 = new PartyContext.ReportParameterHelper
			{
				Guid = text,
				ContextName = contextName2,
				PartyMemberName = partyMemberName,
				ContextType = BugContext.InnerContextType.Item.ToString()
			};
			if (m_ItemUseHistory.Count > 500)
			{
				m_ItemUseHistory.RemoveRange(0, 100);
			}
			m_ItemUseHistory.Add(item2);
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
	}

	private static BaseUnitEntity GetSelectedPartyCharUnitEntityData()
	{
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnitInUI.Value;
		if (!value.Faction.IsPlayer)
		{
			return null;
		}
		return value;
	}

	public void HandleException(Exception exception)
	{
		m_Exception = exception;
	}

	public void HandleErrorMessages(string[] errorMessages)
	{
		m_ErrorMessages = errorMessages;
	}

	public void HandleBugReportOpen(bool showBugReportOnly)
	{
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
	}

	public void HandleBugReportShow()
	{
	}

	public void HandleBugReportHide()
	{
		m_Exception = null;
		m_ErrorMessages = null;
	}

	public void HandleUIElementFeature(string featureName)
	{
		m_UiFeatureName = featureName;
	}

	private string AddErrorMessages(string message)
	{
		if (m_ErrorMessages == null || m_ErrorMessages.Length == 0)
		{
			return message;
		}
		try
		{
			string[] array = m_ErrorMessages[0].Split('\n');
			string text = array[0];
			if (text.StartsWith("Error: "))
			{
				text += array[1];
			}
			if (text.Contains("Object reference not set to an instance of an object"))
			{
				text = "Error: Object reference not set at " + array[1].Split(' ')[1];
			}
			return text + "\n" + message + "\n\n\n----------- Call stack -----------\n{noformat}\n" + string.Join('\n', m_ErrorMessages) + "\n{noformat}";
		}
		catch
		{
			return string.Join('\n', m_ErrorMessages) + "\n" + message;
		}
	}

	public static string GetCharScreenBlueprintName()
	{
		return ReportingRaycaster.Instance.GetUnderMouseBlueprintName();
	}

	private static string HumanReadableNumbers(long input)
	{
		try
		{
			string text = input.ToString();
			for (int num = text.Length - 3; num >= 0; num -= 3)
			{
				text = text.Insert(num, " ");
			}
			return text + " B";
		}
		catch
		{
			return input.ToString();
		}
	}

	public (bool promo, bool priv) PrivacyStuffGetEmailAgreements(string email)
	{
		return m_ReportPrivacyManager.GetEmailAgreements(email);
	}

	public void PrivacyStuffManage(string email, bool promo, bool privacy, bool isSend)
	{
		m_ReportPrivacyManager.ManageClose(email, promo, privacy, isSend);
	}

	public bool IsReportWithMods(bool isCrash, string callstack = null)
	{
		try
		{
			string[] source = new string[5] { "_Patch", "Postfix", "Prefix", "(wrapper dynamic-method)", "ToyBox" };
			if (isCrash)
			{
				string @string = PlayerPrefs.GetString("LatestCrashdump", "");
				string[] array = new string[2]
				{
					Path.Combine(@string, "error.log"),
					Path.Combine(@string, "Player.log")
				};
				foreach (string path in array)
				{
					if (File.Exists(path))
					{
						string @object = File.ReadAllText(path);
						if (source.Any(@object.Contains))
						{
							return true;
						}
					}
				}
				return false;
			}
			return (!string.IsNullOrEmpty(callstack) && source.Any(callstack.Contains)) || (m_Exception != null && source.Any(m_Exception.StackTrace.Contains));
		}
		catch
		{
			return false;
		}
	}

	public bool IsDiskFreeSpaceCrash(bool isCrash)
	{
		try
		{
			if (isCrash)
			{
				string @string = PlayerPrefs.GetString("LatestCrashdump", "");
				string[] array = new string[2]
				{
					Path.Combine(@string, "error.log"),
					Path.Combine(@string, "Player.log")
				};
				foreach (string path in array)
				{
					if (File.Exists(path))
					{
						string text = File.ReadAllText(path);
						if (text.Contains("Error: array too small. numBytes/offset wrong.\nParameter name: array\n  at System.IO.FileStream.Dispose") || text.Contains("Error: array too small. numBytes/offset wrong.\r\nParameter name: array\r\n  at System.IO.FileStream.Dispose"))
						{
							return true;
						}
					}
				}
			}
			else
			{
				DriveInfo[] drives = DriveInfo.GetDrives();
				string value = DefaultPath.Substring(0, 2);
				DriveInfo[] array2 = drives;
				foreach (DriveInfo driveInfo in array2)
				{
					if (driveInfo.Name.Contains(value))
					{
						return driveInfo.AvailableFreeSpace < 31457280;
					}
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			LogReporterError("Bugreport: " + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public bool IsOutOfMemoryCrash()
	{
		try
		{
			string @string = PlayerPrefs.GetString("LatestCrashdump", "");
			string[] array = new string[2]
			{
				Path.Combine(@string, "error.log"),
				Path.Combine(@string, "Player.log")
			};
			foreach (string path in array)
			{
				if (File.Exists(path) && File.ReadAllText(path).Contains("Could not allocate memory:"))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public bool IsCorruptedBundleCrash(bool isCrash)
	{
		try
		{
			string path = (isCrash ? "GameLogFullPrev.txt" : "GameLogFull.txt");
			string path2 = Path.Combine(ApplicationPaths.persistentDataPath, path);
			if (File.Exists(path2))
			{
				string @object = File.ReadAllText(path2);
				if (new string[3] { "couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded", "Failed to read data for the AssetBundle", "is corrupted! Remove it and launch unity again!" }.Any(@object.Contains))
				{
					return true;
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public string GetStoreFileConsInstruction()
	{
		return StoreManager.Store switch
		{
			StoreType.None => "CHECK LOCAL FILE CONSISTENCY!", 
			StoreType.Steam => "CHECK LOCAL FILE CONSISTENCY!", 
			StoreType.GoG => "CHECK LOCAL FILE CONSISTENCY!", 
			_ => "CHECK LOCAL FILE CONSISTENCY!", 
		};
	}

	private int GetChapterNormalOrCrash()
	{
		try
		{
			if (m_Context.Type != BugContext.ContextType.Exception && m_Context.Type != BugContext.ContextType.Crash && m_Context.Type != BugContext.ContextType.ExceptionSpam)
			{
				return Game.Instance.Player.Chapter;
			}
			if (string.IsNullOrEmpty(CurrentReportFolder))
			{
				return -1;
			}
			string[] files = Directory.GetFiles(CurrentReportFolder);
			foreach (string text in files)
			{
				if (!text.EndsWith(".zks") || !File.Exists(text))
				{
					continue;
				}
				string fileName = Path.GetFileName(text);
				string directoryName = Path.GetDirectoryName(text);
				string text2 = Path.Combine(directoryName, "_tempChap");
				string archiveFileName = Path.Combine(directoryName, fileName);
				try
				{
					if (Directory.Exists(text2))
					{
						Directory.Delete(text2, recursive: true);
					}
					Directory.CreateDirectory(text2);
					using (ZipArchive zipArchive = ZipFile.OpenRead(archiveFileName))
					{
						foreach (ZipArchiveEntry item in zipArchive.Entries.Where((ZipArchiveEntry e) => e.Name == "player.json"))
						{
							item.ExtractToFile(Path.Combine(text2, item.Name));
						}
					}
					string[] files2 = Directory.GetFiles(text2);
					foreach (string text3 in files2)
					{
						if (text3.EndsWith("player.json"))
						{
							string input = File.ReadAllText(text3);
							MatchCollection matchCollection = new Regex("\"Chapter\":([0-10]|[1-9][0-9]),").Matches(input);
							if (matchCollection.Count >= 1)
							{
								return int.Parse(matchCollection[0].Groups[1].Value);
							}
							break;
						}
					}
				}
				catch (Exception ex)
				{
					LogReporterError("Report Getting Chapter Exception: " + ex.Message + "\n" + ex.StackTrace);
				}
				finally
				{
					try
					{
						if (Directory.Exists(text2))
						{
							Directory.Delete(text2, recursive: true);
						}
					}
					catch (Exception ex2)
					{
						LogReporterError("Report Remove temp folder Exception: " + ex2.Message + "\n" + ex2.StackTrace);
					}
				}
			}
			return -1;
		}
		catch
		{
			return -1;
		}
	}

	public void FillLabelsDictionary()
	{
		if (m_labelsDictionary.Count <= 0)
		{
			string[] labels = Instance.Assignees.Result.Labels;
			foreach (string key in labels)
			{
				m_labelsDictionary.Add(key, value: false);
			}
		}
	}

	public Dictionary<string, bool> GetLabelsList()
	{
		return m_labelsDictionary;
	}

	public void ResetLabelsList()
	{
		foreach (string item in m_labelsDictionary.Keys.ToList())
		{
			m_labelsDictionary[item] = false;
		}
	}

	public void LabelChangeValue(string label, bool isOn)
	{
		if (!m_labelsDictionary.ContainsKey(label))
		{
			LogReporterError("Cannot find label: " + label);
		}
		else
		{
			m_labelsDictionary[label] = isOn;
		}
	}

	private bool IsHasViewBase<T>() where T : class, IViewModel
	{
		return UnityEngine.Object.FindObjectsOfType<ViewBase<T>>().Any((ViewBase<T> x) => x.isActiveAndEnabled);
	}

	public static bool IsEmailOwlcatDomain(string str)
	{
		foreach (string owlcatDomain in OwlcatDomains)
		{
			if (str.Contains(owlcatDomain))
			{
				return true;
			}
		}
		return false;
	}

	public void HandleReportSend(ReportSendEntry sendEntry)
	{
	}

	public void HandleReportResultReceived(ReportSendEntry sendEntry)
	{
		if (sendEntry == LastEntry)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler wrn)
			{
				wrn.HandleWarning("Press Ctrl+Alt+F11 to open issue " + sendEntry.ReportIssueId + " in browser");
			});
		}
	}

	private string GetPlatform()
	{
		if (Application.isEditor)
		{
			return "Editor";
		}
		return Application.platform.ToString("G").Replace("Player", "");
	}

	public void SetMode(ReportSendingMode mode)
	{
		ReportSender?.SetMode(mode);
	}
}
