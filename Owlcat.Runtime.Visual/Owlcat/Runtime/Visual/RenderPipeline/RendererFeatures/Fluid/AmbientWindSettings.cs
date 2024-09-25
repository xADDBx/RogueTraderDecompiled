using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

[Serializable]
public class AmbientWindSettings
{
	public Vector2 Direction;

	public Texture2D NoiseMap;

	public Vector2 NoiseMapTiling = new Vector2(1f, 1f);

	public static AmbientWindSettings DefaultSettings => new AmbientWindSettings
	{
		Direction = default(Vector2),
		NoiseMapTiling = new Vector2(1f, 1f)
	};
}
