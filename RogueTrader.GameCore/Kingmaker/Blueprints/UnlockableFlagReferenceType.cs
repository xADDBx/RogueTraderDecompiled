using System;

namespace Kingmaker.Blueprints;

[Flags]
public enum UnlockableFlagReferenceType
{
	None = 0,
	Unlock = 1,
	Lock = 2,
	Check = 4,
	CheckValue = 8,
	SetValue = 0x10
}
