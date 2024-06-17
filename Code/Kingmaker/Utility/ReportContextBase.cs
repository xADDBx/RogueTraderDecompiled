using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kingmaker.Utility;

[Serializable]
public abstract class ReportContextBase
{
	protected const string KeyContext = "Context";

	protected const string KeyGuid = "Guid";

	protected const string KeyName = "Name";

	public Dictionary<string, List<ContextRow>> Contexts { get; set; } = new Dictionary<string, List<ContextRow>>();


	public void AddContext(string context, params ContextParameter[] parameters)
	{
		if (string.IsNullOrEmpty(context))
		{
			context = "Unknown";
		}
		if (Contexts.ContainsKey(context))
		{
			Contexts[context].Add(new ContextRow(parameters.Where((ContextParameter x) => !string.IsNullOrEmpty(x.Value)).ToArray()));
			return;
		}
		Contexts.Add(context, new List<ContextRow>
		{
			new ContextRow(parameters.Where((ContextParameter x) => !string.IsNullOrEmpty(x.Value)).ToArray())
		});
	}

	public void AddContext(string context, params ContextRow[] rows)
	{
		if (string.IsNullOrEmpty(context))
		{
			context = "Unknown";
		}
		if (!Contexts.ContainsKey(context))
		{
			Contexts.Add(context, new List<ContextRow>());
		}
		Contexts[context].AddRange(rows);
	}

	public ReportContextBase Parse(string text)
	{
		string[] array = text.Replace("\r", "").Replace("\\n", "\n").Split('\n');
		string key = "";
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (!string.IsNullOrEmpty(text2))
			{
				List<string> list = (from Match x in Regex.Matches(text2, "([^,]*: [^,]*\\([^,()]*, [^,()]*\\)|[^,]*: [^,]*)")
					select x.Value).ToList();
				if (list.Count < 2)
				{
					key = RemoveEnd(text2, ":").Trim();
					Contexts.Add(key, new List<ContextRow>());
				}
				else
				{
					Contexts[key].Add(new ContextRow(list));
				}
			}
		}
		return this;
	}

	public Dictionary<string, List<string>> GetAllContexts()
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (ContextRow item in Contexts.Values.SelectMany((List<ContextRow> s) => s))
		{
			string text = item.Parameters.FirstOrDefault((ContextParameter x) => x.Name == "Name" || x.Name == "Blueprint")?.Value;
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = item.Parameters.FirstOrDefault((ContextParameter x) => x.Name == "Context" || x.Name == "ContextType")?.Value;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "Unknown";
				}
				if (dictionary.ContainsKey(text2))
				{
					dictionary[text2].Add(text);
					continue;
				}
				dictionary.Add(text2, new List<string> { text });
			}
		}
		return dictionary;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, List<ContextRow>> context in Contexts)
		{
			if (context.Value.Count >= 1)
			{
				stringBuilder.AppendLine(context.Key + ":");
				stringBuilder.AppendLine(string.Join("\n", context.Value.Select((ContextRow s) => s.ToString())));
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}

	private string RemoveEnd(string text, string end)
	{
		if (!text.Contains(end))
		{
			return text;
		}
		return text.Remove(text.LastIndexOf(end), end.Length);
	}
}
