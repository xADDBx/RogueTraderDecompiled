using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Screen Space Cloud Shadows")]
public class ScreenSpaceCloudShadows : VolumeComponent, IPostProcessComponent
{
	public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);

	public TextureParameter Texture0 = new TextureParameter(null);

	public Vector2Parameter Texture0Tiling = new Vector2Parameter(new Vector2(1f, 1f));

	public Vector2Parameter Texture0ScrollSpeed = new Vector2Parameter(default(Vector2));

	public ColorParameter Texture0Color = new ColorParameter(Color.white);

	public TextureParameter Texture1 = new TextureParameter(null);

	public Vector2Parameter Texture1Tiling = new Vector2Parameter(new Vector2(1f, 1f));

	public Vector2Parameter Texture1ScrollSpeed = new Vector2Parameter(default(Vector2));

	public ColorParameter Texture1Color = new ColorParameter(Color.white);

	public bool IsActive()
	{
		return Intensity.value > 0f;
	}

	public bool IsTileCompatible()
	{
		return false;
	}
}
