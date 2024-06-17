using System;
using UnityEngine;

namespace Kingmaker.Visual;

public class EditorWeakReferenceAttribute : PropertyAttribute
{
	public Type RefType { get; }

	public EditorWeakReferenceAttribute(Type type)
	{
		RefType = type;
	}
}
