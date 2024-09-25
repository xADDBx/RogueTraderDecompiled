using System;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

[Serializable]
public class ContextParameter
{
	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public string Value { get; set; }

	public ContextParameter()
	{
	}

	public ContextParameter(string name, string val)
	{
		Name = name;
		Value = val;
	}

	public override string ToString()
	{
		return Name + ": " + Value;
	}
}
