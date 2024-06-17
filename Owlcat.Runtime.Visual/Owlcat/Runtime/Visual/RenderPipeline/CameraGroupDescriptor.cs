using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

internal struct CameraGroupDescriptor
{
	public RenderTexture ColorTexture;

	public RenderTexture DepthTexture;

	public Rect Viewport;

	public CameraGroupDescriptor(RenderTexture colorTexture, RenderTexture depthTexture, Rect viewport)
	{
		ColorTexture = colorTexture.Or(null);
		DepthTexture = depthTexture.Or(null);
		Viewport = viewport;
	}

	public override bool Equals(object obj)
	{
		if (obj is CameraGroupDescriptor cameraGroupDescriptor && EqualityComparer<RenderTexture>.Default.Equals(ColorTexture, cameraGroupDescriptor.ColorTexture) && EqualityComparer<RenderTexture>.Default.Equals(DepthTexture, cameraGroupDescriptor.DepthTexture))
		{
			return Viewport.Equals(cameraGroupDescriptor.Viewport);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((813922571 * -1521134295 + EqualityComparer<RenderTexture>.Default.GetHashCode(ColorTexture)) * -1521134295 + EqualityComparer<RenderTexture>.Default.GetHashCode(DepthTexture)) * -1521134295 + EqualityComparer<Rect>.Default.GetHashCode(Viewport);
	}
}
