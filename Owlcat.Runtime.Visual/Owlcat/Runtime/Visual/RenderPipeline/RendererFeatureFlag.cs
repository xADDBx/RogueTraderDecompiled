using System;

namespace Owlcat.Runtime.Visual.RenderPipeline;

[Serializable]
public class RendererFeatureFlag
{
	public string FeatureIdentifier;

	public bool Enabled;
}
