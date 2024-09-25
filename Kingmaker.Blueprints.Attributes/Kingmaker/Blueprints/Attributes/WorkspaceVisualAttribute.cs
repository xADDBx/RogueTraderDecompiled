using System;

namespace Kingmaker.Blueprints.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class WorkspaceVisualAttribute : Attribute
{
	public float R = 1f;

	public float G = 1f;

	public float B = 1f;

	public string DefaultIcon;
}
