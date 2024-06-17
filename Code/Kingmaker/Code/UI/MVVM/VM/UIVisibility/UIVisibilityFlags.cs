using System;

namespace Kingmaker.Code.UI.MVVM.VM.UIVisibility;

[Flags]
public enum UIVisibilityFlags
{
	None = 0,
	StaticPart = 1,
	DynamicPart = 2,
	CommonPart = 4,
	Pointer = 8,
	BugReport = 0x10
}
