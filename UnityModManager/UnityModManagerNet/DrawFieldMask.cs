using System;

namespace UnityModManagerNet;

[Flags]
public enum DrawFieldMask
{
	Any = 0,
	Public = 1,
	Serialized = 2,
	SkipNotSerialized = 4,
	OnlyDrawAttr = 8
}
