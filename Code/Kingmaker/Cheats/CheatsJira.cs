using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Base;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameInfo;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.Cheats;

public class CheatsJira
{
	public static string JiraIssuesUrl = "http://jira.owlcat.local/browse/";

	public static string JiraUrl = GetJiraUrl();

	private static readonly List<string> s_BlueprintInfoNamesOnly = new List<string>();

	[UsedImplicitly]
	public static CheatsJira s_CheatsJira = new CheatsJira();

	private static readonly List<BlueprintScriptableObject> s_BlueprintInfo = new List<BlueprintScriptableObject>();

	private static readonly string[] s_ExceptionToParse = new string[2] { "NullReferenceException", "Assertion failed" };

	[Cheat(Name = "qa_mode", Description = "Set to true to see all exceptions as a message box")]
	public static bool QaMode { get; set; } = BuildModeUtility.IsDevelopment && !Application.isEditor;


	public static string GetJiraUrl()
	{
		if (BuildModeUtility.IsDevelopment)
		{
			return "http://jira.owlcat.local/secure/CreateIssueDetails!init.jspa?pid=10000";
		}
		string path = Application.streamingAssetsPath + "/Jira.info";
		if (!File.Exists(path))
		{
			return "";
		}
		return File.ReadAllText(path);
	}

	private CheatsJira()
	{
		EventBus.Subscribe(this);
	}

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (!BuildModeUtility.CheatsEnabled)
		{
			return;
		}
		keyboard.Bind("RapidAreaBug", delegate
		{
			RapidBug(delegate
			{
				RapidAreaBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidInterfaceBug", delegate
		{
			RapidBug(delegate
			{
				RapidInterfaceBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidEngineBug", delegate
		{
			RapidBug(delegate
			{
				RapidEngineBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidCoreMechanicsBug", delegate
		{
			RapidBug(delegate
			{
				RapidCoreMechanicsBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidLoreBug", delegate
		{
			RapidBug(delegate
			{
				RapidLoreBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidGlobalMapBug", delegate
		{
			RapidBug(delegate
			{
				RapidGlobalMapBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidCreaturesBug", delegate
		{
			RapidBug(delegate
			{
				RapidCreaturesBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidItemsBug", delegate
		{
			RapidBug(delegate
			{
				RapidItemsBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidLightingBug", delegate
		{
			RapidBug(delegate
			{
				RapidLightningBug(saves: true);
			}, b: true);
		});
		keyboard.Bind("RapidAreaBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidAreaBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidInterfaceBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidInterfaceBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidEngineBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidEngineBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidCoreMechanicsBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidCoreMechanicsBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidLoreBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidLoreBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidGlobalMapBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidGlobalMapBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidCreaturesBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidCreaturesBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidItemsBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidItemsBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("RapidLightingBugNoSave", delegate
		{
			RapidBug(delegate
			{
				RapidLightningBug(saves: false);
			}, b: false);
		});
		keyboard.Bind("BrowserOpenLastReport", BrowserOpenLastReport);
		keyboard.Bind("GameInfo", GameInfo);
		keyboard.Bind("AddGameInfo", AddGameInfo);
		keyboard.Bind("ExtendedInfo", ExtendedInfo);
	}

	private static void ExtendedInfo()
	{
		CollectGameInfo();
		if (s_BlueprintInfo.Count == 0)
		{
			return;
		}
		string text = "{HTML}";
		foreach (BlueprintScriptableObject item in s_BlueprintInfo)
		{
			text = text + Utilities.GetBlueprintName(item) + ": <a href='owlcat://open?type=simple&guid" + item.AssetGuid;
		}
		GUIUtility.systemCopyBuffer = text + "{HTML}";
	}

	private static void AddGameInfo()
	{
		CollectGameInfo();
		if (s_BlueprintInfo.Count != 0)
		{
			GUIUtility.systemCopyBuffer = s_BlueprintInfo.Aggregate("", (string current, BlueprintScriptableObject blueprint) => current + Utilities.GetBlueprintName(blueprint) + "\n");
		}
		if (s_BlueprintInfoNamesOnly.Count > 0)
		{
			GUIUtility.systemCopyBuffer = s_BlueprintInfoNamesOnly.Aggregate("", (string current, string blueprint) => current + blueprint + "\n");
		}
	}

	private static void GameInfo()
	{
		s_BlueprintInfo.Clear();
		s_BlueprintInfoNamesOnly.Clear();
		AddGameInfo();
	}

	private static void CollectGameInfo()
	{
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		BlueprintScriptableObject[] array = Tooltip();
		string text = string.Empty;
		string text2 = string.Empty;
		try
		{
			text = MouseHoverBlueprintSystem.Instance?.UnderMouseBlueprintName;
			text2 = ReportingUtils.GetCharScreenBlueprintName();
		}
		catch
		{
		}
		if (!string.IsNullOrEmpty(text2) && text2 != "null")
		{
			s_BlueprintInfoNamesOnly.Add(text2);
		}
		else if (!string.IsNullOrEmpty(text) && text != "null")
		{
			s_BlueprintInfoNamesOnly.Add(text);
		}
		else if (unitUnderMouse != null)
		{
			s_BlueprintInfo.Add(unitUnderMouse.Blueprint);
			s_BlueprintInfoNamesOnly.Add(unitUnderMouse.Blueprint.NameSafe());
		}
		else if (array != null)
		{
			BlueprintScriptableObject[] array2 = array;
			foreach (BlueprintScriptableObject blueprintScriptableObject in array2)
			{
				s_BlueprintInfo.Add(blueprintScriptableObject);
				s_BlueprintInfoNamesOnly.Add(blueprintScriptableObject.NameSafe());
			}
		}
	}

	public static BlueprintScriptableObject[] Tooltip()
	{
		return Array.Empty<BlueprintScriptableObject>();
	}

	internal static void RapidBug(Action action, bool b)
	{
		Utilities.CreateGameHistoryLog();
		if (b)
		{
			CheatSaveLogic.SaveAndUpload("bugreport", "", SaveInfo.SaveType.Bugreport);
		}
		LoadingProcess.Instance.StartLoadingProcess(BugActionProcess(action));
	}

	private static IEnumerator BugActionProcess(Action action)
	{
		action();
		yield break;
	}

	internal static void RapidAreaBug(bool saves)
	{
		BlueprintAreaPart currentAreaPart = GetCurrentAreaPart();
		string exceptionMessage = GetExceptionMessage();
		string text = ((currentAreaPart == null) ? "" : ("[" + currentAreaPart?.ToString() + "]"));
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + text + ((exceptionMessage != "") ? (" " + exceptionMessage) : "") + "&assignee=" + Utilities.GetDesigner(GetCurrentArea()) + "&customfield_10104=PF-194&customfield_10031=" + GetAreaQualityAssurance(currentAreaPart) + "&priority=10001&description=" + GetDescription(saves, currentAreaPart) + "%0A%0A" + GetPosition());
	}

	internal static void RapidInterfaceBug(bool saves)
	{
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + GetExceptionMessage() + "&assignee=rotfort&customfield_10104=PF-191&customfield_10031=vertyankin&priority=10001&description=" + GetDescription(saves));
	}

	internal static void RapidEngineBug(bool saves)
	{
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + GetExceptionMessage() + "&assignee=drobyshevsky&customfield_10104=PF-96&customfield_10031=vertyankin&priority=10001&description=" + GetDescription(saves, GetCurrentAreaPart()));
	}

	internal static void RapidCoreMechanicsBug(bool saves)
	{
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + GetExceptionMessage() + "&assignee=a.gusev&customfield_10104=PF-117&customfield_10031=al.gusev&priority=10001&description=" + GetDescription(saves, GetCurrentAreaPart()));
	}

	internal static void RapidLoreBug(bool saves)
	{
		BlueprintAreaPart currentAreaPart = GetCurrentAreaPart();
		string text = ((currentAreaPart == null) ? "" : ("[" + currentAreaPart?.ToString() + "]"));
		BlueprintDialog dialog = Game.Instance.DialogController.Dialog;
		string text2 = "";
		string text3 = "";
		if (dialog != null)
		{
			text2 = "[" + dialog?.ToString() + "]";
			text3 = Utilities.GetBlueprintPath(dialog);
		}
		BlueprintCue currentCue = Game.Instance.DialogController.CurrentCue;
		string text4 = "";
		if (currentCue != null)
		{
			text4 = MakeOpenString("Cue", Utilities.GetBlueprintName(currentCue), "dialog", currentCue.AssetGuid);
		}
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + text + text2 + "&assignee=komzolov&customfield_10104=PF-154&customfield_10031=vertyankin&priority=10001&description=Dialog:%20" + text3 + "%0A" + text4 + "%0A" + GetDescription(saves, GetCurrentAreaPart()));
	}

	internal static void RapidGlobalMapBug(bool saves)
	{
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + GetExceptionMessage() + "&assignee=silaev&customfield_10104=PF-481&customfield_10031=vertyankin&priority=10001&description=" + GetDescription(saves, GetCurrentAreaPart()));
	}

	internal static void RapidCreaturesBug(bool saves)
	{
		BaseUnitEntity unitUnderMouse = Utilities.GetUnitUnderMouse();
		string text = "";
		string text2 = "";
		if (unitUnderMouse != null)
		{
			BlueprintUnit blueprint = unitUnderMouse.Blueprint;
			text = "[" + blueprint.name + "]";
			text2 = MakeOpenString("Creature", Utilities.GetBlueprintName(blueprint), "simple", blueprint.AssetGuid) + "%0A";
		}
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + text + GetExceptionMessage() + "&assignee=a.gusev&customfield_10104=PF-33&customfield_10031=al.gusev&priority=10001&description=" + text2 + GetDescription(saves, GetCurrentAreaPart()));
	}

	internal static void RapidItemsBug(bool saves)
	{
		Utilities.CreateGameHistoryLog();
		BlueprintScriptableObject[] array = Tooltip();
		string text = "";
		string text2 = "";
		if (array != null)
		{
			BlueprintScriptableObject[] array2 = array;
			foreach (BlueprintScriptableObject blueprintScriptableObject in array2)
			{
				if (blueprintScriptableObject != null)
				{
					text = text + "[" + Utilities.GetBlueprintName(blueprintScriptableObject) + "]";
					text2 = text2 + MakeOpenString("Item", Utilities.GetBlueprintName(blueprintScriptableObject), "simple", blueprintScriptableObject.AssetGuid) + "%0A";
				}
			}
		}
		Process.Start(JiraUrl + "&issuetype=10003&summary=" + text + GetExceptionMessage() + "&assignee=a.gusev&customfield_10104=PF-490&customfield_10031=al.gusev&priority=10001&description=" + text2 + GetDescription(saves, GetCurrentAreaPart()));
	}

	private static void RapidLightningBug(bool saves)
	{
		Utilities.CreateGameHistoryLog();
		BlueprintAreaPart currentAreaPart = GetCurrentAreaPart();
		string exceptionMessage = GetExceptionMessage();
		string text = ((currentAreaPart == null) ? "" : ("[" + GetLightScene() + "]"));
		Process.Start(JiraUrl + "&issuetype=1&summary=" + text + ((exceptionMessage != "") ? (" " + exceptionMessage) : "") + "&assignee=nikitchenko&customfield_10104=PF-194&customfield_10031=" + GetAreaQualityAssurance(currentAreaPart) + "&priority=10001&description=" + GetDescription(saves, currentAreaPart) + GetPosition());
	}

	internal static void BrowserOpenLastReport()
	{
		string reportIssueId = ReportingUtils.LastEntry.ReportIssueId;
		if (string.IsNullOrEmpty(reportIssueId))
		{
			if (GameVersion.Mode != 0)
			{
				string msg = "No sent bugreports.";
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(msg);
				});
			}
			PFLog.Default.Log("No sent bugreports");
		}
		else
		{
			Process.Start(JiraIssuesUrl + reportIssueId);
		}
	}

	private static string GetDescription(bool saves, BlueprintAreaPart areaPart = null)
	{
		string text = "Version: " + Application.version;
		string revision = GameVersion.Revision;
		if (revision != null)
		{
			text = text + "%0A" + revision;
		}
		if (saves && CheatSaveLogic.UploadedLink != null)
		{
			string text2 = "{HTML}Save: <a href='" + CheatSaveLogic.UploadedLink + "'>load in editor</a>{HTML}";
			text = text + "%0A" + text2;
		}
		string text3 = ((areaPart == null) ? "" : ("%0AAreaPart: " + areaPart));
		text += text3;
		string blueprintName = Utilities.GetBlueprintName(GameHelper.GetPlayerCharacter().Blueprint.Race);
		Gender gender = GameHelper.GetPlayerCharacter().Gender;
		string arg = GameHelper.GetPlayerCharacter().Progression.Classes.Aggregate("", (string current, ClassData classData) => current + $" {Utilities.GetBlueprintName(classData.CharacterClass)}:{classData.Level}");
		text += $"%0AMain Character: {blueprintName}/{gender} {arg}";
		text += GetCutscenesInfo().Replace("\n", "%0A");
		if (!GUIUtility.systemCopyBuffer.Contains(".cs:"))
		{
			return text;
		}
		return text + "%0A{code}%0A{code}";
	}

	public static string GetCutscenesInfo()
	{
		string result = string.Empty;
		List<CutscenePlayerData> list = new List<CutscenePlayerData>(Game.Instance.State.Cutscenes);
		if (list.Count > 0)
		{
			result = $"\nCutscenes running ({list.Count})";
			result = list.Aggregate(result, (string current, CutscenePlayerData c) => current + "\n" + Utilities.GetBlueprintName(c.Cutscene));
		}
		return result;
	}

	private static string GetPosition()
	{
		return "";
	}

	private static string GetExceptionMessage()
	{
		if (!GUIUtility.systemCopyBuffer.Contains(".cs:"))
		{
			return "";
		}
		string[] array = GUIUtility.systemCopyBuffer.Split('\n');
		string[] array2 = s_ExceptionToParse;
		foreach (string text in array2)
		{
			if (array[0].Contains(text))
			{
				return GetMessage(text, array);
			}
		}
		return array[0];
	}

	private static string GetMessage(string exceptionType, ICollection<string> stacktrace)
	{
		string text = "";
		string text2 = "";
		if (stacktrace.Count > 1)
		{
			foreach (string item in stacktrace)
			{
				string text3 = item.Replace("  at ", "");
				if (item.Contains("Kingmaker"))
				{
					text = " at " + text3.Substring(0, text3.LastIndexOf('(') - 1);
					text2 = " (" + text3.Substring(text3.LastIndexOf('\\') + 1).Replace(" ", "");
					break;
				}
			}
		}
		return exceptionType + text + text2;
	}

	public static BlueprintArea GetCurrentArea()
	{
		return Game.Instance.CurrentlyLoadedArea;
	}

	public static BlueprintAreaPart GetCurrentAreaPart()
	{
		return Game.Instance.CurrentlyLoadedArea;
	}

	public static string GetLightScene()
	{
		SceneReference sceneReference = (Game.Instance?.CurrentlyLoadedArea)?.LightScene;
		if (sceneReference == null)
		{
			return "NoLight";
		}
		return sceneReference.SceneName;
	}

	private static string GetAreaQualityAssurance(BlueprintScriptableObject blueprintArea)
	{
		return "cherkasov";
	}

	public static string MakeOpenString(string label, string path, string type, string guid)
	{
		PFLog.Default.Log("Making link with label '" + label + "'; path '" + path + "'; type '" + type + "'; guid '" + guid + "'");
		return label + ": " + path + " {HTML}<a href=\"owlcat://open?type=" + type + "%26guid=" + guid + "\" target=\"_blank\">Open in Editor</a>{HTML}";
	}
}
