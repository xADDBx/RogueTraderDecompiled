using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Overrides;

[Serializable]
public class OverriddenProperty
{
	public string Name;

	public string Path;

	[SerializeReference]
	public List<OverriddenProperty> Children = new List<OverriddenProperty>();

	public bool IsOverrided { get; set; }

	public OverriddenProperty(string path)
	{
		Path = path ?? "";
		Name = Path.Split('.').LastOrDefault() ?? "";
	}
}
