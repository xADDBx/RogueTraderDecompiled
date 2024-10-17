using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

public class AssigneeModelRoot
{
	[JsonProperty(PropertyName = "Chapter QA")]
	public Dictionary<string, string> ChapterQa { get; } = new Dictionary<string, string>();


	[JsonProperty(PropertyName = "fixversions")]
	public Dictionary<string, string> FixVersions { get; } = new Dictionary<string, string>();


	[JsonProperty(PropertyName = "Leads")]
	public string[] Leads { get; set; } = Array.Empty<string>();


	[JsonProperty(PropertyName = "Main Table")]
	public MainTable MainTable { get; } = new MainTable();


	[JsonProperty(PropertyName = "UI Designer")]
	public Dictionary<string, string> UiDesigners { get; } = new Dictionary<string, string>();


	[JsonProperty(PropertyName = "UI QA")]
	public Dictionary<string, string> UiQa { get; } = new Dictionary<string, string>();


	[JsonProperty(PropertyName = "Labels")]
	public string[] Labels { get; set; } = Array.Empty<string>();

}
