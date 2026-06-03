using System;

namespace Kingmaker.Utility.Attributes;

public class ExpandInspectorAttribute : Attribute
{
	public readonly bool WithChildren;

	public ExpandInspectorAttribute(bool withChildren = false)
	{
		WithChildren = withChildren;
	}
}
