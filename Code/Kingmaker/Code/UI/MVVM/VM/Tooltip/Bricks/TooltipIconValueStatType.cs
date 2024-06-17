using System;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

[Flags]
public enum TooltipIconValueStatType
{
	Normal = 1,
	Small = 2,
	Centered = 4,
	Inverted = 8,
	Justified = 0x10,
	NameTextNormal = 0x20,
	NameTextBold = 0x40,
	ValueTextNormal = 0x80,
	ValueTextBold = 0x100,
	SmallInverted = 0xA,
	JustifiedInverted = 0x18
}
