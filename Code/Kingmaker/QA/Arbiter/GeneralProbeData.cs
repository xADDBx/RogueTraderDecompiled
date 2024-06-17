using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Kingmaker.GameInfo;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
public class GeneralProbeData
{
	[JsonProperty]
	public string Project;

	[JsonProperty]
	public string Build;

	[JsonProperty]
	public string Revision;

	[JsonProperty]
	public string Platform;

	[JsonProperty]
	public string Branch;

	[JsonProperty]
	public string Run;

	[JsonProperty]
	public string Guid;

	[JsonProperty]
	public long Timestamp;

	[JsonProperty]
	public string Instruction;

	[JsonProperty]
	public string ProbeType;

	[JsonProperty]
	public HardwareInfo HardwareInfo;

	[JsonProperty]
	public List<ISpecificProbeData> ProbeDataList;

	private string m_ZipFilePath = string.Empty;

	private string m_DataFolder = string.Empty;

	public string DataFolder
	{
		get
		{
			if (string.IsNullOrEmpty(m_DataFolder))
			{
				m_DataFolder = GetDataFolder();
			}
			return m_DataFolder;
		}
	}

	[JsonConstructor]
	public GeneralProbeData()
	{
	}

	public GeneralProbeData(string instructionName, string probeType)
	{
		Project = Arbiter.Root.Project;
		Build = Arbiter.Instance.GetVersion();
		Revision = GetRevision();
		Platform = ArbiterUtils.GetPlatform();
		Branch = GetBranch();
		Guid = Arbiter.JobGuid.ToString();
		Run = Arbiter.Run.ToString();
		Timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
		Instruction = instructionName;
		ProbeType = probeType;
		ProbeDataList = new List<ISpecificProbeData>();
		HardwareInfo = Arbiter.HardwareInfo;
	}

	public void SendToServer()
	{
		if (!Arbiter.SendToServer)
		{
			return;
		}
		try
		{
			PFLog.Arbiter.Log("Sending instruction '" + Instruction + "' to server... ");
			SaveJsonData();
			string fileName = ArchivateData();
			ServicePointManager.ServerCertificateValidationCallback = (object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true;
			new WebClient().UploadFile(Arbiter.SendProbeUrl, "POST", fileName);
			PFLog.Arbiter.Log("Sending instruction '" + Instruction + "' success.");
		}
		catch (Exception ex)
		{
			PFLog.Arbiter.Log("Sending instruction '" + Instruction + "' error:");
			PFLog.Arbiter.Exception(ex);
		}
	}

	public void SaveJsonData()
	{
		File.WriteAllText(Path.Combine(DataFolder, "info.json"), ArbiterClientIntegration.SerializeObject(this));
	}

	private string ArchivateData()
	{
		m_ZipFilePath = DataFolder + ".zip";
		if (File.Exists(m_ZipFilePath))
		{
			File.Delete(m_ZipFilePath);
		}
		ZipFile.CreateFromDirectory(DataFolder, m_ZipFilePath);
		return m_ZipFilePath;
	}

	private string GetRevision()
	{
		string[] array = GameVersion.Revision.Split(' ');
		if (array.Length >= 3)
		{
			return array[2];
		}
		return array[0];
	}

	private string GetBranch()
	{
		return GameVersion.Revision.Split(' ').Last();
	}

	private string GetDataFolder()
	{
		string text = Path.Combine(Arbiter.PlatformDataPath, "Arbiter/" + ProbeType + "/" + Instruction);
		if (Directory.Exists(text))
		{
			Directory.Delete(text, recursive: true);
		}
		Directory.CreateDirectory(text);
		return text;
	}

	public void CleanupData()
	{
		if (!Application.isEditor && !Arbiter.Instance.Arguments.ArbiterKeepFilesAfterUpload)
		{
			if (Directory.Exists(DataFolder))
			{
				Directory.Delete(DataFolder, recursive: true);
			}
			if (File.Exists(m_ZipFilePath))
			{
				File.Delete(m_ZipFilePath);
			}
		}
	}
}
