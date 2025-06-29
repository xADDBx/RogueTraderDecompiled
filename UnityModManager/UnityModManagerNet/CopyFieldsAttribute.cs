using System;

namespace UnityModManagerNet;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class CopyFieldsAttribute : Attribute
{
	public CopyFieldMask Mask;

	public CopyFieldsAttribute(CopyFieldMask Mask)
	{
		this.Mask = Mask;
	}
}
