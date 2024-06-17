using System;
using UnityEngine;

namespace Owlcat.Runtime.UI.Controls;

[Serializable]
public class LayoutElementExtendedValue
{
	public enum ReferenceTypes
	{
		None,
		Width,
		Height
	}

	public bool Enabled;

	public ReferenceTypes ReferenceType;

	public RectTransform Reference;

	public float ReferenceDelta;

	public float TargetValue;

	public void ProcessTargetValue(UnityEngine.Object context)
	{
		if (!Reference && ReferenceType != 0)
		{
			Debug.Log("This needs a reference to process your target Layout values", context);
			return;
		}
		switch (ReferenceType)
		{
		case ReferenceTypes.Width:
			TargetValue = Reference.rect.width;
			break;
		case ReferenceTypes.Height:
			TargetValue = Reference.rect.height;
			break;
		default:
			Debug.LogError($"No support for a referenceType of: {ReferenceType}", context);
			break;
		case ReferenceTypes.None:
			break;
		}
		TargetValue += ReferenceDelta;
	}
}
