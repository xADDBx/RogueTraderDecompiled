using System;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows;

[Flags]
public enum ShadowFlags
{
	None = 0,
	Point = 1,
	SoftShadows = 2
}
