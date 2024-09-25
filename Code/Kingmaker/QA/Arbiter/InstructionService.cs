using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.QA.Arbiter;

internal class InstructionService
{
	private readonly ArbiterStartupParameters m_Parameters;

	private string m_PlayerPrefsDataString;

	private static string s_ServerAddress = "https://localhost:7179";

	public static string AvailableScenariosUrl => s_ServerAddress + "/Instructions";

	public InstructionService(ArbiterStartupParameters parameters)
	{
		m_Parameters = parameters;
	}

	public string[] GetInstructions()
	{
		m_PlayerPrefsDataString = PlayerPrefs.GetString("ArbiterInstruction", "");
		if (!string.IsNullOrWhiteSpace(m_PlayerPrefsDataString))
		{
			PlayerPrefs.DeleteKey("ArbiterInstruction");
			PlayerPrefs.Save();
		}
		if (!string.IsNullOrWhiteSpace(m_PlayerPrefsDataString))
		{
			return new string[1] { m_PlayerPrefsDataString };
		}
		m_PlayerPrefsDataString = m_Parameters.ArbiterInstruction ?? string.Empty;
		if (!string.IsNullOrWhiteSpace(m_PlayerPrefsDataString))
		{
			return new string[1] { m_PlayerPrefsDataString };
		}
		string text = m_Parameters.ArbiterInstructionsFile ?? m_Parameters.Arbiter;
		m_PlayerPrefsDataString = ((text != null && text.Length > 0) ? text : "arbiter.txt");
		string[] array = new string[3]
		{
			m_PlayerPrefsDataString,
			Path.Combine(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')), m_PlayerPrefsDataString),
			Path.Combine(Application.persistentDataPath, m_PlayerPrefsDataString)
		};
		foreach (string text2 in array)
		{
			PFLog.Arbiter.Log("Search file '" + text2 + "'");
			if (File.Exists(text2))
			{
				PFLog.Arbiter.Log("Read data from file '" + text2 + "'");
				return ParseScenarios(GetAvailableScenariosFromFile(text2));
			}
		}
		PFLog.Arbiter.Log("File '" + m_PlayerPrefsDataString + "' not found. Trying get instructions from server.");
		return ParseScenarios(GetAvailableScenariosFromSever());
	}

	private string GetAvailableScenariosFromSever()
	{
		ServicePointManager.ServerCertificateValidationCallback = (object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true;
		WebClient webClient = new WebClient();
		webClient.QueryString.Add("project", BlueprintArbiterRoot.Instance.Project);
		if (m_Parameters.ArbiterInstructionsPart != null)
		{
			webClient.QueryString.Add("part", m_Parameters.ArbiterInstructionsPart);
		}
		s_ServerAddress = m_Parameters.ArbiterServer ?? s_ServerAddress;
		return webClient.DownloadString(AvailableScenariosUrl);
	}

	private string GetAvailableScenariosFromFile(string filename)
	{
		return File.ReadAllText(filename);
	}

	private string[] ParseScenarios(string jsonScenarios)
	{
		List<string> list = JObject.Parse(jsonScenarios).SelectToken("Instructions")?.Select((JToken x) => (string?)x).ToList() ?? new List<string>();
		PFLog.Arbiter.Log($"Received {list.Count} scenarios from server");
		return list.ToArray();
	}
}
