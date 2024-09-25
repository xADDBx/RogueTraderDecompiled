using System;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[Serializable]
public struct ShadowResolutionSettings
{
	public ShadowResolutionTier DefaultTier;

	public ShadowResolution Low;

	public ShadowResolution Medium;

	public ShadowResolution High;

	public ShadowResolution Ultra;
}
