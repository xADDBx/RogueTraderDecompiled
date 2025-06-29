using System;

namespace UnityModManagerNet;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, AllowMultiple = false)]
public class DrawFieldsAttribute : Attribute
{
	public DrawFieldMask Mask;

	public DrawFieldsAttribute(DrawFieldMask Mask)
	{
		this.Mask = Mask;
	}
}
