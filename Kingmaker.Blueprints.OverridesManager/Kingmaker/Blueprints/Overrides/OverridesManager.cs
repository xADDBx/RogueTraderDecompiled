using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Overrides;

[Serializable]
public class OverridesManager
{
	private static readonly Regex s_RgxArrayElementIndex = new Regex("\\.Array\\.data\\[(\\d+)\\]");

	[SerializeField]
	[SerializeReference]
	private readonly OverriddenProperty m_Root = new OverriddenProperty("");

	public bool IsOverridden(string path)
	{
		return EnumerateProperties(PathToComponents(path)).Any((OverriddenProperty p) => p.IsOverrided);
	}

	public void SetOverridden(string path, bool overridden)
	{
		if (IsOverridden(path) != overridden)
		{
			PFLog.Default.Log("Override property '{0}': {1}", path, overridden);
			Get(path).IsOverrided = overridden;
		}
	}

	private static IEnumerable<string> PathToComponents(string path)
	{
		return s_RgxArrayElementIndex.Replace(path, (Match match) => "." + match.Groups[1]).Split('.');
	}

	private IEnumerable<OverriddenProperty> EnumerateProperties(IEnumerable<string> pathComponents)
	{
		OverriddenProperty current = m_Root;
		foreach (string pathComponent in pathComponents)
		{
			current = current.Children.FirstOrDefault((OverriddenProperty node) => node.Name.Equals(pathComponent));
			if (current != null)
			{
				yield return current;
				continue;
			}
			yield break;
		}
	}

	public void ForEach(Action<OverriddenProperty> action)
	{
		ForEach(action, m_Root);
	}

	private static void ForEach(Action<OverriddenProperty> action, OverriddenProperty property)
	{
		action(property);
		property.Children.ForEach(delegate(OverriddenProperty p)
		{
			ForEach(action, p);
		});
	}

	private OverriddenProperty Get(IEnumerable<string> pathComponents)
	{
		OverriddenProperty overriddenProperty = m_Root;
		foreach (string pathComponent in pathComponents)
		{
			OverriddenProperty overriddenProperty2 = overriddenProperty;
			overriddenProperty = overriddenProperty.Children.FirstOrDefault((OverriddenProperty node) => node.Name.Equals(pathComponent));
			if (overriddenProperty == null)
			{
				overriddenProperty = new OverriddenProperty(overriddenProperty2.Path.Empty() ? pathComponent : (overriddenProperty2.Path + "." + pathComponent));
				overriddenProperty2.Children.Add(overriddenProperty);
			}
		}
		return overriddenProperty;
	}

	private OverriddenProperty Get(string path)
	{
		return Get(PathToComponents(path));
	}

	private static void Serialize(OverriddenProperty property, List<string> result)
	{
		if (property.IsOverrided)
		{
			result.Add(property.Path);
			return;
		}
		property.Children.ForEach(delegate(OverriddenProperty p)
		{
			Serialize(p, result);
		});
	}

	public List<string> Serialize()
	{
		List<string> result = new List<string>();
		m_Root.Children.ForEach(delegate(OverriddenProperty p)
		{
			Serialize(p, result);
		});
		return result;
	}

	public void Deserialize(IEnumerable<string> paths)
	{
		m_Root.Children.Clear();
		paths.Where((string path) => !IsOverridden(path)).ForEach(delegate(string path)
		{
			Get(path).IsOverrided = true;
		});
	}

	public void Clear()
	{
		m_Root.Children.Clear();
	}

	public bool Empty()
	{
		return m_Root.Children.Empty();
	}
}
