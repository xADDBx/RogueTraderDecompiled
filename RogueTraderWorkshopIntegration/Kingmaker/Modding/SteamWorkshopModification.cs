using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Modding;

public class SteamWorkshopModification
{
	public class WorkshopSettingsData
	{
		[JsonProperty]
		public const string ModManagingInfoFileName = "WorkshopManaged.json";

		[JsonProperty]
		public uint LocalTimestamp;

		[JsonProperty]
		public PublishedFileId_t SteamFileId;

		[JsonProperty]
		public string UniqueName;
	}

	public const string UMMInfoFileName = "Info.json";

	public PublishedFileId_t SteamFileId;

	public WorkshopSettingsData WorkshopSettings;

	public string SteamLocation;

	public string LocalLocation;

	public OwlcatModificationManifest Manifest;

	public bool IsUmm;

	public bool IsOwlcatTemplate;

	public bool IsDownloading;

	public bool NeedsDownload;

	public uint SteamTimestamp;

	public uint LocalTimestamp => WorkshopSettings?.LocalTimestamp ?? 0;

	public SteamWorkshopModification(PublishedFileId_t mod)
	{
		SteamFileId = mod;
		EItemState itemState = (EItemState)SteamUGC.GetItemState(mod);
		bool flag = itemState.HasFlag(EItemState.k_EItemStateInstalled);
		bool flag2 = itemState.HasFlag(EItemState.k_EItemStateNeedsUpdate);
		IsDownloading = itemState.HasFlag(EItemState.k_EItemStateDownloading);
		NeedsDownload = (flag2 || !flag) && !IsDownloading;
		if (flag)
		{
			SteamUGC.GetItemInstallInfo(mod, out var _, out var pchFolder, 4096u, out var punTimeStamp);
			SteamLocation = pchFolder;
			SteamTimestamp = punTimeStamp;
		}
	}

	public void Install(bool isUpdate = false)
	{
		LogChannel mods = PFLog.Mods;
		string obj = (isUpdate ? "Updating" : "Installing");
		PublishedFileId_t steamFileId = SteamFileId;
		mods.Log(obj + " Steam Workshop with Id: " + steamFileId.ToString());
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "RogueTraderWorkshopMod" + SteamFileId.m_PublishedFileId));
		try
		{
			if (directoryInfo.Exists)
			{
				directoryInfo.Delete(recursive: true);
			}
			directoryInfo.Create();
			string text = null;
			if (SteamLocation.EndsWith("_legacy.bin"))
			{
				text = SteamLocation;
			}
			else
			{
				string[] files = Directory.GetFiles(SteamLocation, "*.zip");
				if (files.Length != 0)
				{
					text = Path.Combine(SteamLocation, files.First());
				}
			}
			if (text != null)
			{
				ZipFile.ExtractToDirectory(text, directoryInfo.FullName);
			}
			else
			{
				CopyDirectoryRecursively(new DirectoryInfo(SteamLocation), directoryInfo.FullName);
			}
			int num = 0;
			while (directoryInfo.GetFiles().Length == 1)
			{
				FileInfo[] files2 = directoryInfo.GetFiles("*.zip");
				if (files2 == null || files2.Length != 1)
				{
					break;
				}
				num++;
				if (num > 3)
				{
					throw new Exception("Error while trying to Install Workshop Item. Exceeded 3 nested zip archives.");
				}
				DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "RogueTraderWorkshopMod" + SteamFileId.m_PublishedFileId + "_nested"));
				if (directoryInfo2.Exists)
				{
					directoryInfo2.Delete(recursive: true);
				}
				try
				{
					directoryInfo2.Create();
					FileInfo fileInfo = directoryInfo.GetFiles().First();
					ZipFile.ExtractToDirectory(fileInfo.FullName, directoryInfo2.FullName);
					fileInfo.Delete();
					CopyDirectoryRecursively(directoryInfo2, directoryInfo.FullName);
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					if (directoryInfo2 != null && directoryInfo2.Exists)
					{
						directoryInfo2.Delete(recursive: true);
					}
				}
			}
			string path = Path.Combine(directoryInfo.FullName, "OwlcatModificationManifest.json");
			if (File.Exists(path))
			{
				try
				{
					Manifest = JsonUtility.FromJson<OwlcatModificationManifest>(File.ReadAllText(path));
				}
				catch (Exception ex2)
				{
					PFLog.Mods.Exception(ex2);
				}
			}
			string text2 = null;
			if (Manifest != null)
			{
				PFLog.Mods.Log("Steam Workshop Item recognized as " + Manifest.DisplayName + " with Id " + Manifest.UniqueName);
				if (!Manifest.UniqueName.IsNullOrEmpty())
				{
					if ((directoryInfo.GetFiles()?.Where((FileInfo file) => file.Name == "Info.json")?.Any()).GetValueOrDefault())
					{
						IsUmm = true;
						text2 = Path.Combine(SteamWorkshopIntegration.DefaultUMMDirectory, Manifest.UniqueName);
					}
					else if ((directoryInfo.GetFiles()?.Where((FileInfo file) => file.Name == "OwlcatModificationSettings.json")?.Any()).GetValueOrDefault())
					{
						IsOwlcatTemplate = true;
						text2 = Path.Combine(SteamWorkshopIntegration.DefaultOwlcatTemplateDirectory, Manifest.UniqueName);
						List<string> list = SteamWorkshopIntegration.Instance.m_Settings.EnabledModifications.ToList();
						if (!list.Contains(Manifest.UniqueName))
						{
							list.Add(Manifest.UniqueName);
							SteamWorkshopIntegration.Instance.m_Settings.EnabledModifications = list.ToArray();
						}
					}
				}
				else
				{
					PFLog.Mods.Error($"Steam Workshop Item {Manifest.DisplayName} with Steam FileId {SteamFileId} has null or empty UniqueName and can't be installed");
				}
			}
			if ((IsUmm || IsOwlcatTemplate) && !text2.IsNullOrEmpty())
			{
				CopyDirectoryRecursively(directoryInfo, text2);
				if (!isUpdate)
				{
					WorkshopSettings = new WorkshopSettingsData();
				}
				WorkshopSettings.LocalTimestamp = SteamTimestamp;
				WorkshopSettings.UniqueName = Manifest.UniqueName;
				WorkshopSettings.SteamFileId = SteamFileId;
				File.WriteAllText(Path.Combine(text2, "WorkshopManaged.json"), JsonUtility.ToJson(WorkshopSettings, prettyPrint: true));
			}
		}
		catch (Exception ex3)
		{
			PFLog.Mods.Exception(ex3);
		}
		finally
		{
			if (directoryInfo != null && directoryInfo.Exists)
			{
				directoryInfo.Delete(recursive: true);
			}
		}
	}

	public static void CopyDirectoryRecursively(DirectoryInfo dir, string target)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(target);
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string text = Path.Combine(target, fileInfo.Name);
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			fileInfo.CopyTo(text);
		}
		DirectoryInfo[] directories = dir.GetDirectories();
		foreach (DirectoryInfo directoryInfo2 in directories)
		{
			CopyDirectoryRecursively(directoryInfo2, Path.Combine(target, directoryInfo2.Name));
		}
	}
}
