using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceManagement;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

public class CustomPortraitsManager
{
	private static CustomPortraitsManager s_Instance;

	private static string s_CurrentPortraitFolderPath;

	public static CustomPortraitsManager Instance => s_Instance ?? (s_Instance = new CustomPortraitsManager());

	public static string PortraitsRootFolderPath => Path.Combine(ApplicationPaths.persistentDataPath, BlueprintRoot.Instance.CharGenRoot.PortraitFolderName);

	public ResourceStorage<Sprite> Storage { get; } = new ResourceStorage<Sprite>((string path) => new SpriteLoadingRequest(path));


	public string GetPortraitFolderPath(string id)
	{
		s_CurrentPortraitFolderPath = Path.Combine(PortraitsRootFolderPath, id);
		return GetCurrentPortraitFolderPath();
	}

	public string GetCurrentPortraitFolderPath()
	{
		return s_CurrentPortraitFolderPath;
	}

	public string GetSmallPortraitPath(string id)
	{
		return Path.Combine(GetPortraitFolderPath(id), BlueprintRoot.Instance.CharGenRoot.PortraitSmallName + BlueprintRoot.Instance.CharGenRoot.PortraitsFormat);
	}

	public string GetMediumPortraitPath(string id)
	{
		return Path.Combine(GetPortraitFolderPath(id), BlueprintRoot.Instance.CharGenRoot.PortraitMediumName + BlueprintRoot.Instance.CharGenRoot.PortraitsFormat);
	}

	public string GetBigPortraitPath(string id)
	{
		return Path.Combine(GetPortraitFolderPath(id), BlueprintRoot.Instance.CharGenRoot.PortraitBigName + BlueprintRoot.Instance.CharGenRoot.PortraitsFormat);
	}

	public bool EnsureCustomPortraits(string id)
	{
		if (!EnsureDirectory(id, createNewIfNotExists: false))
		{
			return false;
		}
		EnsurePortrait(GetSmallPortraitPath(id), BlueprintRoot.Instance.CharGenRoot.BasePortraitSmall);
		EnsurePortrait(GetMediumPortraitPath(id), BlueprintRoot.Instance.CharGenRoot.BasePortraitMedium);
		EnsurePortrait(GetBigPortraitPath(id), BlueprintRoot.Instance.CharGenRoot.BasePortraitBig);
		return true;
	}

	public bool EnsureDirectory(string id, bool createNewIfNotExists)
	{
		string portraitFolderPath = GetPortraitFolderPath(id);
		try
		{
			if (Directory.Exists(portraitFolderPath))
			{
				return true;
			}
			if (createNewIfNotExists)
			{
				Directory.CreateDirectory(portraitFolderPath);
				PFLog.Default.Log("CustomPortraitsManager: The directory was created successfully at " + portraitFolderPath);
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			PFLog.Default.Log("CustomPortraitsManager: The process failed: " + ex.ToString());
			return false;
		}
	}

	private bool EnsurePortrait(string portrait, SpriteLink baseSprite)
	{
		try
		{
			if (File.Exists(portrait))
			{
				return true;
			}
			CreateBaseImages(portrait, baseSprite.Load());
			return false;
		}
		catch (Exception ex)
		{
			PFLog.Default.Log("CustomPortraitsManager: The process failed: " + ex.ToString());
			return false;
		}
	}

	private static void CreateBaseImages(string portrait, Sprite baseSprite)
	{
		byte[] bytes = baseSprite.texture.EncodeToPNG();
		File.WriteAllBytes(portrait, bytes);
		PFLog.Default.Log("CustomPortraitsManager: The file was created successfully at " + portrait);
	}

	public static void OpenInMacFileBrowser(string path)
	{
		bool flag = false;
		string text = path.Replace("\\", "/");
		if (Directory.Exists(text))
		{
			flag = true;
		}
		if (!text.StartsWith("\""))
		{
			text = "\"" + text;
		}
		if (!text.EndsWith("\""))
		{
			text += "\"";
		}
		string arguments = (flag ? "" : "-R ") + text;
		try
		{
			Process.Start("open", arguments);
		}
		catch (Win32Exception ex)
		{
			ex.HelpLink = "";
		}
	}

	public static void OpenInWinFileBrowser(string path)
	{
		bool flag = false;
		string text = path.Replace("/", "\\");
		if (Directory.Exists(text))
		{
			flag = true;
		}
		try
		{
			Process.Start("explorer.exe", (flag ? "/root," : "/select,") + text);
		}
		catch (Win32Exception ex)
		{
			ex.HelpLink = "";
		}
	}

	public void OpenPortraitFolder(string id)
	{
		string portraitFolderPath = GetPortraitFolderPath(id);
		OpenInWinFileBrowser(portraitFolderPath);
		OpenInMacFileBrowser(portraitFolderPath);
	}

	public string[] GetExistingCustomPortraitIds()
	{
		if (!Directory.Exists(PortraitsRootFolderPath))
		{
			Directory.CreateDirectory(PortraitsRootFolderPath);
		}
		return (from p in Directory.GetDirectories(PortraitsRootFolderPath)
			select new DirectoryInfo(p).Name).ToArray();
	}

	public void DeletePortraitFolder(string id)
	{
		string path = Path.Combine(PortraitsRootFolderPath, id);
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
	}

	public PortraitData CreateNew()
	{
		string[] existingCustomPortraitIds = GetExistingCustomPortraitIds();
		int num = 1;
		while (existingCustomPortraitIds.HasItem(num.ToString("D4")))
		{
			num++;
		}
		PortraitData portraitData = new PortraitData(num.ToString("D4"));
		EnsureDirectory(portraitData.CustomId, createNewIfNotExists: true);
		EnsureCustomPortraits(portraitData.CustomId);
		portraitData.EnsureImages();
		return portraitData;
	}

	public PortraitData CreateNewOrLoadDefault()
	{
		string[] existingCustomPortraitIds = GetExistingCustomPortraitIds();
		if (!existingCustomPortraitIds.Empty())
		{
			return new PortraitData(existingCustomPortraitIds[0]);
		}
		return CreateNew();
	}

	public void LoadAllPortraitsSmoothly(Action<PortraitData> callback)
	{
		CoroutineRunner.Start(LoadPortraitCoroutine(callback));
	}

	public IEnumerable<PortraitData> LoadAllPortraits(int instantlyLoadCount)
	{
		string[] existingCustomPortraitIds = GetExistingCustomPortraitIds();
		string[] array = existingCustomPortraitIds;
		for (int i = 0; i < array.Length; i++)
		{
			PortraitData portraitData = new PortraitData(array[i]);
			if (instantlyLoadCount > 0)
			{
				portraitData.SmallPortraitHandle.Load();
			}
			else
			{
				portraitData.SmallPortraitHandle.LoadAsync();
			}
			instantlyLoadCount--;
			yield return portraitData;
		}
	}

	public void Cleanup()
	{
		using PooledHashSet<string> pooledHashSet = PooledHashSet<string>.Get();
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			PortraitData portrait = allCharacter.UISettings.Portrait;
			if (portrait.IsCustom)
			{
				pooledHashSet.Add(GetSmallPortraitPath(portrait.CustomId));
				pooledHashSet.Add(GetMediumPortraitPath(portrait.CustomId));
				pooledHashSet.Add(GetBigPortraitPath(portrait.CustomId));
			}
		}
		foreach (string item in Storage.GetAll.ToTempList())
		{
			if (!pooledHashSet.Contains(item))
			{
				Storage.Unload(item);
			}
		}
	}

	private IEnumerator LoadPortraitCoroutine(Action<PortraitData> callback)
	{
		string[] existingCustomPortraitIds = GetExistingCustomPortraitIds();
		string[] array = existingCustomPortraitIds;
		for (int i = 0; i < array.Length; i++)
		{
			PortraitData obj = CreatePortraitData(array[i]);
			callback(obj);
			yield return null;
		}
		yield return null;
	}

	private static PortraitData CreatePortraitData(string id)
	{
		PortraitData portraitData = new PortraitData(id);
		portraitData.SmallPortraitHandle.Load();
		portraitData.HalfPortraitHandle.Load();
		portraitData.FullPortraitHandle.Load();
		portraitData.CheckIfDefaultPortraitData();
		return portraitData;
	}
}
