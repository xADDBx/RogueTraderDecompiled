using System;
using System.Collections.Generic;

namespace Owlcat.QA.Validation;

public class MetadataCollector
{
	internal Dictionary<string, string> Metadata = new Dictionary<string, string>();

	internal MetadataCollector(Action<MetadataCollector> action)
	{
		action?.Invoke(this);
	}

	public MetadataCollector Add(string key, string value)
	{
		Metadata.Add(key, value);
		return this;
	}
}
