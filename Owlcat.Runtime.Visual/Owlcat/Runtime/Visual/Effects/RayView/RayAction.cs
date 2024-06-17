using System;

namespace Owlcat.Runtime.Visual.Effects.RayView;

[Flags]
public enum RayAction
{
	None = 0,
	StartFadeOut = 1,
	StartFadeIn = 2,
	PlayAnim = 4,
	Disable = 8
}
