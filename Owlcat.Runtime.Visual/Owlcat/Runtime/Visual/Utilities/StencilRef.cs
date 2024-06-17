using System;

namespace Owlcat.Runtime.Visual.Utilities;

[Flags]
public enum StencilRef
{
	ReceiveDecals = 1,
	SpecialPostProcessFlag = 2,
	Distortion = 4,
	OccludedObjectHighlighting = 8,
	Reserve4 = 0x10,
	Reserve5 = 0x20,
	Reserve6 = 0x40,
	DeferredLightingOff = 0x80
}
