using System;
using Kingmaker.Visual.HitSystem;
using UnityEngine;

namespace Kingmaker.Sound;

[Serializable]
public struct TextureToSoundSurface
{
	public Texture2D Texture;

	public SurfaceType SurfaceSoundType;
}
