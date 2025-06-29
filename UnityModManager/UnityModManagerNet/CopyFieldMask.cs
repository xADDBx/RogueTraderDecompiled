using System;

namespace UnityModManagerNet;

[Flags]
public enum CopyFieldMask
{
	Any = 0,
	Matching = 1,
	Public = 2,
	Serialized = 4,
	SkipNotSerialized = 8,
	OnlyCopyAttr = 0x10
}
