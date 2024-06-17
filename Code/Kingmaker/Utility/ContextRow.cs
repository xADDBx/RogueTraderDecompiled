using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

[Serializable]
public class ContextRow
{
	[JsonProperty]
	public List<ContextParameter> Parameters { get; set; } = new List<ContextParameter>();


	public ContextRow()
	{
	}

	public ContextRow(IEnumerable<string> row)
	{
		foreach (string item in row)
		{
			string[] array = item.Split(':');
			Parameters.Add(new ContextParameter(array[0].Trim(), array[1].Trim()));
		}
	}

	public ContextRow(IEnumerable<ContextParameter> parameters)
	{
		Parameters.AddRange(parameters);
	}

	public override string ToString()
	{
		if (Parameters != null && Parameters.Count != 0)
		{
			return string.Join(", ", Parameters.Select((ContextParameter s) => s.ToString()));
		}
		return string.Empty;
	}

	public string GetKey(string key)
	{
		try
		{
			return Parameters.First((ContextParameter x) => x.Name == key).Value;
		}
		catch
		{
			return string.Empty;
		}
	}
}
