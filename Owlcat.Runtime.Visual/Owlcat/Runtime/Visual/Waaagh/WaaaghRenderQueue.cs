using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class WaaaghRenderQueue
{
	public enum Priority
	{
		Background = 1000,
		Opaque = 2000,
		OpaqueAlphaTest = 2450,
		OpaqueLast = 2500,
		OpaqueDistortion = 2900,
		OpaqueDistortionLast = 2999,
		Transparent = 3000,
		TransparentLast = 3999,
		Overlay = 4000,
		OverlayLast = 4999
	}

	public static readonly RenderQueueRange OpaquePreDistortion = new RenderQueueRange(0, 2500);

	public static readonly RenderQueueRange OpaqueDistortion = new RenderQueueRange(2900, 2999);

	public static readonly RenderQueueRange Opaque = new RenderQueueRange(0, 2999);

	public static readonly RenderQueueRange Transparent = new RenderQueueRange(3000, 3999);

	public static readonly RenderQueueRange Overlay = new RenderQueueRange(4000, 4999);

	public static readonly RenderQueueRange All = new RenderQueueRange(0, 4999);
}
