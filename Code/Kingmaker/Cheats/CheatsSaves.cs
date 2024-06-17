using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Cheats;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.GameCommands;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Cheats;

internal static class CheatsSaves
{
	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("PersistantQuickSave", PersistantQuickSave);
			SmartConsole.RegisterCommand("save_remote", RemoteSaveGame);
		}
	}

	public static void PersistantQuickSave()
	{
		if (BuildModeUtility.IsDevelopment)
		{
			CheatSaveLogic.SaveAndUpload("quicksave", null, SaveInfo.SaveType.Remote, 1);
		}
	}

	[Cheat(Name = "list_saves")]
	public static string ListSaves()
	{
		Game.Instance.SaveManager.UpdateSaveListIfNeeded(force: true);
		IEnumerable<string> values = Game.Instance.SaveManager.Select((SaveInfo save) => $"{save.Type}: [{save.Name}] in {save.Area.name}");
		return string.Join(Environment.NewLine, values);
	}

	private static void RemoteSaveGame(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, null);
		if (string.IsNullOrWhiteSpace(paramString))
		{
			SmartConsole.Print("Name not specified");
			return;
		}
		bool? paramBool = Utilities.GetParamBool(parameters, 2, null);
		int mode = 0;
		if (paramBool.HasValue && paramBool.Value)
		{
			mode = 2;
		}
		IEnumerable<string> values = Utilities.GetArguments(parameters).Skip(2);
		string description = string.Join(" ", values).Trim();
		CheatSaveLogic.SaveAndUpload(paramString, description, SaveInfo.SaveType.Remote, mode);
	}

	[Cheat(Name = "load_remote")]
	public static void RemoteLoadGame(string path)
	{
		SavesStorageAccess.Load(path);
	}

	[Cheat(Name = "save")]
	public static void SaveGame(string name)
	{
		SaveInfo saveInfo = Game.Instance.SaveManager.CreateNewSave(name);
		Game.Instance.GameCommandQueue.SaveGame(saveInfo);
	}

	[Cheat(Name = "save_auto")]
	public static void SaveGameAuto()
	{
		Game.Instance.SaveGame(Game.Instance.SaveManager.GetNextAutoslot());
	}

	[Cheat(Name = "load")]
	public static void LoadGame(string name)
	{
		SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault((SaveInfo s) => s.Name == name);
		if (saveInfo != null)
		{
			Game.Instance.LoadGame(saveInfo);
		}
	}

	[Cheat(Name = "delSave")]
	public static void DeleteSaveGame(string name)
	{
		SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault((SaveInfo s) => s.Name == name);
		if (saveInfo != null)
		{
			Game.Instance.SaveManager.DeleteSave(saveInfo);
			PFLog.SmartConsole.Log("Save deleted");
		}
		else
		{
			PFLog.SmartConsole.Log("Save not found");
		}
	}

	[Cheat(Name = "copy_saves")]
	public static void CopySaves(string filter)
	{
		string text = Path.Combine(ApplicationPaths.DevelopmentDataPath, "saves");
		if (Directory.Exists(text))
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			foreach (FileInfo item in directoryInfo.EnumerateFiles())
			{
				item.Delete();
			}
			foreach (DirectoryInfo item2 in directoryInfo.EnumerateDirectories())
			{
				item2.Delete(recursive: true);
			}
		}
		else
		{
			Directory.CreateDirectory(text);
		}
		foreach (SaveInfo item3 in Game.Instance.SaveManager)
		{
			if (!filter.IsNullOrEmpty())
			{
				if (filter[0] == '*')
				{
					if (!item3.Name.Contains(filter.Substring(1)))
					{
						continue;
					}
				}
				else if (item3.Name != filter)
				{
					continue;
				}
			}
			ReportingUtils.Instance.CopySaveFile(item3, text);
		}
	}

	[Cheat(Name = "import_saves")]
	public static async void ImportSaves(string folder = null)
	{
		if (folder.IsNullOrEmpty())
		{
			folder = Path.Combine(ApplicationPaths.persistentDataPath, "Saved Games");
			PFLog.SmartConsole.Log("Folder param is not specified, try use default: " + folder);
		}
		if (!Directory.Exists(folder))
		{
			PFLog.SmartConsole.Log("Folder doesn't exist: " + folder);
			return;
		}
		PFLog.SmartConsole.Log("Import saves from " + folder);
		await SaveStreamer.StreamSaves(folder);
		PFLog.SmartConsole.Log("Done");
	}
}
