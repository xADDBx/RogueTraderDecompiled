using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Code.Utility.ExtendedModInfo;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Modding;

public class UnityModManagerBridge : IModManagerBridge
{
	private const int UnityModManagerVersion = 12;

	private const string UmmKey = "UmmVersion";

	private const string OwlcatUnityModManagerAsset = "OwlcatUnityModManager.zip";

	private const string OwlcatUnityModManagerDllFileName = "UnityModManager.dll";

	private const string OwlcatUnityModManagerPdbFileName = "UnityModManager.pdb";

	private static readonly string OwlcatUnityModManagerPersistantDirectory = Path.Combine(Application.persistentDataPath, "UnityModManager");

	private static readonly string OwlcatUnityModManagerDllPath = Path.Combine(OwlcatUnityModManagerPersistantDirectory, "UnityModManager.dll");

	private static readonly string OwlcatUnityModManagerPdbPath = Path.Combine(OwlcatUnityModManagerPersistantDirectory, "UnityModManager.pdb");

	private static readonly List<string> ModMakerDllDependencies = new List<string> { "dnlib.dll", "Ionic.Zip.dll" };

	private bool m_Initialized;

	private Type m_OwlcatUnityModManagerType;

	public bool? DoorstopUsed { get; private set; }

	private Type OwlcatUnityModManagerType
	{
		get
		{
			if (m_OwlcatUnityModManagerType != null)
			{
				return m_OwlcatUnityModManagerType;
			}
			try
			{
				Assembly assembly = Assembly.LoadFrom(OwlcatUnityModManagerDllPath);
				m_OwlcatUnityModManagerType = assembly.GetType("UnityModManagerNet.UnityModManager");
			}
			catch (Exception ex)
			{
				Debug.LogError($"UnityModManager exception: {ex}");
				PFLog.UnityModManager.Exception(ex);
			}
			return m_OwlcatUnityModManagerType;
		}
	}

	private bool ReadyToUse
	{
		get
		{
			bool? doorstopUsed = DoorstopUsed;
			bool valueOrDefault = doorstopUsed.GetValueOrDefault();
			if (!doorstopUsed.HasValue)
			{
				valueOrDefault = DoorstopDetected();
				bool? doorstopUsed2 = valueOrDefault;
				DoorstopUsed = doorstopUsed2;
			}
			if (m_Initialized)
			{
				return !DoorstopUsed.Value;
			}
			return false;
		}
	}

	public void TryStart(string passedGameVersion)
	{
		string text = EnsureValidGameVersion(passedGameVersion);
		bool? doorstopUsed = DoorstopUsed;
		bool valueOrDefault = doorstopUsed.GetValueOrDefault();
		if (!doorstopUsed.HasValue)
		{
			valueOrDefault = DoorstopDetected();
			bool? doorstopUsed2 = valueOrDefault;
			DoorstopUsed = doorstopUsed2;
		}
		if (DoorstopUsed.Value || !PrepareOwlcatUnityModManagerDll())
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("Start", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[1] { text });
			m_Initialized = true;
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"UnityModManager exception: {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public void TryStartUI()
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("UiFirstLaunch", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while starting UnityModManagerUi; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public List<ExtendedModInfo> GetAllModsInfo()
	{
		if (!ReadyToUse)
		{
			return null;
		}
		List<ExtendedModInfo> result = null;
		try
		{
			object obj = OwlcatUnityModManagerType.GetMethod("GetAllModsInfo", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
			if (obj == null)
			{
				return null;
			}
			result = (List<ExtendedModInfo>)obj;
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while getting all mods info from UMM; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
		return result;
	}

	public ExtendedModInfo GetModInfo(string modId)
	{
		if (!ReadyToUse)
		{
			return null;
		}
		ExtendedModInfo result = null;
		try
		{
			object obj = OwlcatUnityModManagerType.GetMethod("GetModInfo", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[1] { modId });
			if (obj == null)
			{
				return null;
			}
			result = (ExtendedModInfo)obj;
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while getting ModInfo from UMM; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
		return result;
	}

	public void CheckForUpdates()
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("GetModInfo", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while trying to update mods in UMM; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public void OpenModInfoWindow(string modId)
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("OpenModInfoWindow", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[1] { modId });
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while trying to open mod settings UMM ui window; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	public void EnableMod(string modId, bool state)
	{
		if (!ReadyToUse)
		{
			return;
		}
		try
		{
			OwlcatUnityModManagerType.GetMethod("EnableMod", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[2] { modId, state });
		}
		catch (Exception ex)
		{
			PFLog.UnityModManager.Error($"Troubles while trying to open mod settings UMM ui window; {ex}");
			PFLog.UnityModManager.Exception(ex);
		}
	}

	private string EnsureValidGameVersion(string passedGameVersion)
	{
		if (!Version.TryParse(passedGameVersion, out var _))
		{
			return "1.0.0";
		}
		return passedGameVersion;
	}

	[UsedImplicitly]
	private void ModifyDependentDllsSearch()
	{
		if (m_Initialized)
		{
			return;
		}
		AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
		{
			string text = (args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name);
			text += ".dll";
			if (!ModMakerDllDependencies.Contains(text))
			{
				return (Assembly)null;
			}
			string path = Path.Combine(OwlcatUnityModManagerPersistantDirectory, text);
			try
			{
				return Assembly.LoadFile(path);
			}
			catch (Exception arg)
			{
				Debug.LogError($"{arg}");
				return (Assembly)null;
			}
		};
	}

	private bool DoorstopDetected()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("DOORSTOP_INITIALIZED");
		if (environmentVariable == null || environmentVariable != "TRUE")
		{
			return false;
		}
		PFLog.UnityModManager.Error("External UnityModManager detected. Won't start OwlcatUnityModManager.");
		return true;
	}

	private bool UnityModManagerIntegrationIsUpToDate()
	{
		return PlayerPrefs.GetInt("UmmVersion", 0) == 12;
	}

	private void SaveActualUmmVersion()
	{
		PlayerPrefs.SetInt("UmmVersion", 12);
		PlayerPrefs.Save();
	}

	private bool PrepareOwlcatUnityModManagerDll()
	{
		if (File.Exists(OwlcatUnityModManagerDllPath) && File.Exists(Path.Combine(OwlcatUnityModManagerPersistantDirectory, ModMakerDllDependencies[0])))
		{
			if (UnityModManagerIntegrationIsUpToDate())
			{
				PFLog.UnityModManager.Log("Owlcat UnityModManager.dll is already set up.");
				return true;
			}
			try
			{
				File.Delete(OwlcatUnityModManagerDllPath);
				File.Delete(OwlcatUnityModManagerPdbPath);
				File.Delete(Path.Combine(OwlcatUnityModManagerPersistantDirectory, ModMakerDllDependencies[0]));
				File.Delete(Path.Combine(OwlcatUnityModManagerPersistantDirectory, ModMakerDllDependencies[1]));
				PFLog.UnityModManager.Log("Detected old UnityModManager.dll version, reinstalling.");
			}
			catch (Exception ex)
			{
				PFLog.UnityModManager.Error("Failed to remove old UMM version because of exception.");
				PFLog.UnityModManager.Exception(ex);
				return false;
			}
		}
		PFLog.UnityModManager.Log($"Going to update UMM to version {12}");
		string text = Path.Combine(Application.streamingAssetsPath, "OwlcatUnityModManager.zip");
		if (!File.Exists(text))
		{
			PFLog.UnityModManager.Exception(new FileNotFoundException("OwlcatUnityModManager.zip not found in StreamingAssets"));
			return false;
		}
		try
		{
			bool flag = false;
			string persistentDataPath = Application.persistentDataPath;
			string text2 = Path.Combine(Application.persistentDataPath, "UnityModManager");
			string text3;
			if (Directory.Exists(text2))
			{
				flag = true;
				text3 = Path.Combine(persistentDataPath, "UnityModManager");
			}
			else
			{
				text3 = persistentDataPath;
			}
			ZipFile.Open(text, ZipArchiveMode.Read).ExtractToDirectory(text3);
			if (flag)
			{
				PFLog.UnityModManager.Log("Removing old UnityModManager libraries.");
				string text4 = Path.Combine(text3, "UnityModManager");
				foreach (string item in Directory.EnumerateFiles(text4))
				{
					string text5 = item.Substring(text4.Length + 1);
					PFLog.Mods.Error("filename : " + text5);
					PFLog.Mods.Error("source : " + item);
					PFLog.Mods.Error("dest : " + Path.Combine(text2, text5));
					File.Move(item, Path.Combine(text2, text5));
				}
				Directory.Delete(Path.Combine(text3, "UnityModManager"));
			}
			SaveActualUmmVersion();
			PFLog.UnityModManager.Log("Updating UnityModManager finished.");
			return true;
		}
		catch (Exception ex2)
		{
			PFLog.UnityModManager.Error("UnityModManager update failed.");
			PFLog.UnityModManager.Exception(ex2, "while trying to extract owlcat UnityModManager.dll");
			return false;
		}
	}
}
