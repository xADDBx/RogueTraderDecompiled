using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.MainMenu;

public class SaveStreamer
{
	private static readonly Regex s_NotValidSaveCharacters = new Regex("[^a-zA-Z0-9\\.@]+");

	private static Task s_Task = Task.FromException(new OperationCanceledException());

	public SaveStreamer()
	{
		EventBus.Subscribe(this);
	}

	private static string MakeValidConsoleSaveName(string pcName)
	{
		string text = Path.GetFileNameWithoutExtension(pcName);
		if (text.StartsWith("sce_"))
		{
			text = text.Replace("sce_", "owlcat");
		}
		text = text.Replace("_", "-");
		text = s_NotValidSaveCharacters.Replace(text, "");
		if (text.Length == 0)
		{
			text = Guid.NewGuid().ToString("N");
		}
		if (text.Length > 31)
		{
			text = text.Substring(0, 31);
		}
		return text;
	}

	public Task StreamSaves()
	{
		return StreamSaves(Application.streamingAssetsPath);
	}

	public static async Task StreamSaves(string pathToSaves)
	{
		if (!s_Task.IsCompleted)
		{
			UberDebug.Log("------------> Saves streaming is already in progress, exit");
			return;
		}
		string[] files = Directory.GetFiles(pathToSaves, "*.zks");
		int savesCount = files.Length;
		UberDebug.Log($"------------> Found {savesCount} zks files in {pathToSaves}");
		int saveIndex = 1;
		string[] array = files;
		foreach (string text in array)
		{
			UberDebug.Log($"------------> {saveIndex}/{savesCount} Processing {text}");
			ISaver saver = null;
			SaveInfo saveInfo = new SaveInfo();
			try
			{
				ZipSaver zipSaver = new ZipSaver(text, ISaver.Mode.Read);
				string text2 = MakeValidConsoleSaveName(text);
				saver = new ZipSaver(Path.Combine(SaveManager.SavePath, text2 + ".zks"), ISaver.Mode.Write);
				UberDebug.Log($"------------> {saveIndex}/{savesCount} Created zipSaver");
				saveInfo.Type = SaveInfo.SaveType.Manual;
				saveInfo.Saver = saver;
				string json = zipSaver.ReadHeader();
				byte[] bytes = zipSaver.ReadBytes("header.png");
				byte[] bytes2 = zipSaver.ReadBytes("highres.png");
				UberDebug.Log($"------------> {saveIndex}/{savesCount} Read header");
				saveInfo.Name = text2;
				saveInfo.AreaNameOverride = "WhoCares";
				saveInfo.GameSaveTime = default(TimeSpan);
				saver.SaveJson("header", json);
				saver.SaveBytes("header.png", bytes);
				saver.SaveBytes("highres.png", bytes2);
				UberDebug.Log($"------------> {saveIndex}/{savesCount} Wrote header");
				UberDebug.Log($"------------> {saveIndex}/{savesCount} Started stating");
			}
			catch (Exception arg)
			{
				UberDebug.LogError($"Failed to import save {text}: {arg}");
			}
			finally
			{
				UberDebug.Log($"------------> {saveIndex}/{savesCount} Finish Staging");
				int num = saveIndex + 1;
				saveIndex = num;
				saver?.Dispose();
			}
			UberDebug.Log("------------> await UpdateSaveListAsync");
			await UpdateSaveListAsync();
		}
		UberDebug.Log("------------> Done, updating save list asynchronously");
	}

	private static async Task UpdateSaveListAsync()
	{
		Game.Instance.SaveManager.UpdateSaveListAsync();
		while (!Game.Instance.SaveManager.AreSavesUpToDate)
		{
			await Task.Delay(TimeSpan.FromMilliseconds(10.0));
		}
	}
}
