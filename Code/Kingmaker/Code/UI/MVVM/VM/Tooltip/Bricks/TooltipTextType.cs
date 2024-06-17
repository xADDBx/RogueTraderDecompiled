using System;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

[Flags]
public enum TooltipTextType
{
	Simple = 1,
	Paragraph = 2,
	Italic = 4,
	Bold = 8,
	Centered = 0x10,
	GlossarySize = 0x20,
	BlackColor = 0x40,
	BrightColor = 0x80,
	BoldCentered = 0x18
}
