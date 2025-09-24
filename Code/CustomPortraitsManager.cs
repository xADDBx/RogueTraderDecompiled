using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceManagement;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

public class CustomPortraitsManager
{
	private const string PortraitIdFileName = "id";

	public static readonly string InvalidPortraitId = 0.ToString("D4");

	private static CustomPortraitsManager s_Instance;

	private static string s_CurrentPortraitFolderPath;

	private readonly Dictionary<string, Guid> m_IdGuidMapping = new Dictionary<string, Guid>();

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

	public string GetPortraitPath(string id, PortraitType type)
	{
		return type switch
		{
			PortraitType.SmallPortrait => GetSmallPortraitPath(id), 
			PortraitType.HalfLengthPortrait => GetMediumPortraitPath(id), 
			PortraitType.FullLengthPortrait => GetBigPortraitPath(id), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
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

	public bool HasPortraits([NotNull] string id)
	{
		if (Directory.Exists(GetPortraitFolderPath(id)) && File.Exists(GetSmallPortraitPath(id)) && File.Exists(GetMediumPortraitPath(id)))
		{
			return File.Exists(GetBigPortraitPath(id));
		}
		return false;
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
			return Array.Empty<string>();
		}
		return (from p in Directory.GetDirectories(PortraitsRootFolderPath)
			select new DirectoryInfo(p).Name).ToArray();
	}

	public PortraitData CreateNew(bool fillDefaultPortraits = true)
	{
		string[] existingCustomPortraitIds = GetExistingCustomPortraitIds();
		int num = 1;
		while (existingCustomPortraitIds.HasItem(num.ToString("D4")))
		{
			num++;
		}
		PortraitData portraitData = new PortraitData(num.ToString("D4"));
		if (!Directory.Exists(PortraitsRootFolderPath))
		{
			Directory.CreateDirectory(PortraitsRootFolderPath);
		}
		EnsureDirectory(portraitData.CustomId, createNewIfNotExists: true);
		if (fillDefaultPortraits)
		{
			EnsureCustomPortraits(portraitData.CustomId);
			portraitData.EnsureImages();
		}
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

	public void UpdateGuid(string portraitId)
	{
		SetGuid(portraitId, Guid.NewGuid());
	}

	public void SetGuid(string portraitId, Guid newGuid)
	{
		m_IdGuidMapping[portraitId] = newGuid;
		File.WriteAllText(Path.Combine(GetPortraitFolderPath(portraitId), "id"), newGuid.ToString());
	}

	public Guid GetOrCreatePortraitGuid(string portraitId)
	{
		if (m_IdGuidMapping.TryGetValue(portraitId, out var value))
		{
			return value;
		}
		string path = Path.Combine(GetPortraitFolderPath(portraitId), "id");
		if (File.Exists(path) && Guid.TryParse(File.ReadAllText(path), out var result))
		{
			m_IdGuidMapping[portraitId] = result;
			return result;
		}
		value = Guid.NewGuid();
		SetGuid(portraitId, value);
		return value;
	}

	public bool TryGetPortraitId(Guid guid, out string id)
	{
		foreach (KeyValuePair<string, Guid> item in m_IdGuidMapping)
		{
			if (item.Value == guid)
			{
				id = item.Key;
				return true;
			}
		}
		id = InvalidPortraitId;
		return false;
	}

	public void FillAllPortraitsGuid()
	{
		if (Directory.Exists(PortraitsRootFolderPath))
		{
			string[] directories = Directory.GetDirectories(PortraitsRootFolderPath);
			for (int i = 0; i < directories.Length; i++)
			{
				string name = new DirectoryInfo(directories[i]).Name;
				Guid orCreatePortraitGuid = GetOrCreatePortraitGuid(name);
				m_IdGuidMapping[name] = orCreatePortraitGuid;
			}
		}
	}

	public void CheatClearGuidMapping()
	{
		m_IdGuidMapping.Clear();
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
