using System;
using UnityEngine;

namespace Kingmaker.Utility.Attributes;

public abstract class EnumOrderAttribute : PropertyAttribute
{
	public abstract Enum[] Order { get; }
}
