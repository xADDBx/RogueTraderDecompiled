using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class ClockworkReporter
{
	private static string s_ReportXml = "junit.xml";

	private static string s_ReportJsonDir = "../ClockworkReport/";

	public List<string> ErrorList = new List<string>();

	public bool NeedReport;

	private Stopwatch m_Stopwatch;

	private string m_ErrorCommandGuid;

	public ClockworkReporter()
	{
		m_Stopwatch = Stopwatch.StartNew();
		string text = CommandLineArguments.Parse().Get("-BotReportPath");
		s_ReportJsonDir = ((text.Length > 0) ? text : "../ClockworkReport/");
		Directory.CreateDirectory(s_ReportJsonDir);
	}

	private void SaveError(string error, string prefix = "")
	{
		ClockworkCommand e = Clockwork.Instance?.Runner?.LastCommand;
		string text = prefix + ":" + $"\n\tArea: {Game.Instance?.CurrentlyLoadedArea}" + $"\n\tTask: {Clockwork.Instance?.Runner?.CurrentTask}" + "\n\tCommand: " + ElementExtendAsObject.Or(e, null)?.GetCaption() + "\n\tMessage: " + error + "\n";
		PFLog.Clockwork.Error(text);
		ErrorList.Add(text);
		NeedReport = true;
		ReportResults();
	}

	public void HandleWarning(string error)
	{
		SaveError(error, "Warning");
	}

	public void HandleError(string error)
	{
		SaveError(error, "Error");
		_ = Clockwork.Instance.Scenario;
		if (Clockwork.Instance.ShowDebugEndMessage)
		{
			if (Clockwork.Instance.Runner != null)
			{
				Clockwork.Instance.Runner.Paused = true;
			}
		}
		else
		{
			NeedReport = true;
			ErrorList.Add("Stop Clockwork due to error");
			ReportResults();
			PFLog.Clockwork.Error("Stop Clockwork due to error");
			Clockwork.Instance.Stop();
		}
	}

	public void HandleFatalError(Exception ex)
	{
		string error = "Well shit - " + ex.Message + "\n\tStackTrace: " + ex.StackTrace + ((ex.InnerException != null) ? $"\n\tInner Exception: {ex.InnerException}\n{ex.InnerException.StackTrace}" : "");
		HandleError(error);
	}

	public void ForceReport()
	{
		NeedReport = true;
		ReportResults();
	}

	public void ReportResults()
	{
		if (NeedReport)
		{
			NeedReport = false;
			ReportJson();
		}
	}

	private void ReportJson()
	{
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
		dictionary.Add("General", new Dictionary<string, string>
		{
			{
				"Errors",
				string.Join("\n", ErrorList)
			},
			{
				"Scenario Time",
				m_Stopwatch.Elapsed.ToString("hh\\:mm\\:ss")
			}
		});
		if (!m_ErrorCommandGuid.IsNullOrEmpty())
		{
			dictionary["General"].Add("ErrorCommandGuid", m_ErrorCommandGuid);
		}
		BlueprintClockworkScenario scenario = Clockwork.Instance.Scenario;
		string text = SimpleBlueprintExtendAsObject.Or(scenario, null)?.ScenarioName ?? "NoScenario";
		if (scenario != null)
		{
			dictionary["General"].Add("QA", Utilities.GetEnumDescription(scenario.ScenarioAuthor));
			BlueprintComponent[] componentsArray = scenario.ComponentsArray;
			for (int i = 0; i < componentsArray.Length; i++)
			{
				if (!(componentsArray[i] is AreaTest areaTest))
				{
					continue;
				}
				List<ClockworkCheck> allChecks = areaTest.CommandList.GetAllChecks();
				if (allChecks.Count == 0)
				{
					continue;
				}
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				foreach (ClockworkCheck item in allChecks)
				{
					if (!item.LastResult.HasValue)
					{
						dictionary2.Add(item.GetCaption(), "SKIPPED");
					}
					else if (item.LastResult == false)
					{
						dictionary2.Add(item.GetCaption(), "FAILED");
					}
					else
					{
						dictionary2.Add(item.GetCaption(), "OK");
					}
				}
				dictionary.Add(areaTest.Area.Get().AreaDisplayName, dictionary2);
			}
		}
		JObject jObject = new JObject();
		jObject[text] = JToken.Parse(JsonConvert.SerializeObject(dictionary, Newtonsoft.Json.Formatting.Indented));
		using StreamWriter streamWriter = new StreamWriter(Path.Combine(s_ReportJsonDir, text + ".json"));
		streamWriter.Write(JsonConvert.SerializeObject(jObject, Newtonsoft.Json.Formatting.Indented));
	}

	private void ReportXml()
	{
		BlueprintClockworkScenario scenario = Clockwork.Instance.Scenario;
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Auto;
		xmlWriterSettings.Indent = true;
		using XmlWriter xmlWriter = XmlWriter.Create(s_ReportXml, xmlWriterSettings);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteStartElement("testsuites");
		xmlWriter.WriteAttributeString("time", Time.unscaledTime.ToString());
		WriteErrorsToXml(xmlWriter);
		if (scenario != null)
		{
			BlueprintComponent[] componentsArray = scenario.ComponentsArray;
			for (int i = 0; i < componentsArray.Length; i++)
			{
				if (componentsArray[i] is AreaTest areaTest)
				{
					WriteTestsuiteToXml(xmlWriter, areaTest.CommandList, areaTest.Area.Get().AreaDisplayName);
				}
			}
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();
		xmlWriter.Close();
	}

	private void WriteErrorsToXml(XmlWriter xmlWriter)
	{
		string text = SimpleBlueprintExtendAsObject.Or(Clockwork.Instance.Scenario, null)?.ScenarioName ?? "NoScenario";
		string text2 = "";
		foreach (string error in ErrorList)
		{
			text2 = text2 + "\n" + error;
		}
		xmlWriter.WriteStartElement("testsuite");
		xmlWriter.WriteAttributeString("name", "General");
		xmlWriter.WriteAttributeString("tests", "1");
		xmlWriter.WriteAttributeString("time", "0.0");
		xmlWriter.WriteStartElement("testcase");
		xmlWriter.WriteAttributeString("classname", text + ".General");
		xmlWriter.WriteAttributeString("name", "Errors");
		if (ErrorList.Count != 0)
		{
			xmlWriter.WriteStartElement("failure");
			xmlWriter.WriteString(text2);
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
	}

	private void WriteTestsuiteToXml(XmlWriter xmlWriter, ClockworkCommandList commandList, string testsuiteName)
	{
		List<ClockworkCheck> allChecks = commandList.GetAllChecks();
		if (allChecks.Count == 0)
		{
			return;
		}
		string text = SimpleBlueprintExtendAsObject.Or(Clockwork.Instance.Scenario, null)?.ScenarioName ?? "NoScenario";
		xmlWriter.WriteStartElement("testsuite");
		xmlWriter.WriteAttributeString("name", text);
		xmlWriter.WriteAttributeString("tests", allChecks.Count.ToString());
		xmlWriter.WriteAttributeString("failures", commandList.FailCount.ToString());
		xmlWriter.WriteAttributeString("skips", commandList.SkipCount.ToString());
		xmlWriter.WriteAttributeString("time", "0.0");
		foreach (ClockworkCheck item in allChecks)
		{
			xmlWriter.WriteStartElement("testcase");
			xmlWriter.WriteAttributeString("name", item.GetCaption());
			xmlWriter.WriteAttributeString("classname", text + "." + testsuiteName);
			if (!item.LastResult.HasValue)
			{
				xmlWriter.WriteStartElement("skipped");
				xmlWriter.WriteEndElement();
			}
			else if (item.LastResult == false)
			{
				xmlWriter.WriteStartElement("failure");
				xmlWriter.WriteString(item.GetErrorMessage());
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}
}
