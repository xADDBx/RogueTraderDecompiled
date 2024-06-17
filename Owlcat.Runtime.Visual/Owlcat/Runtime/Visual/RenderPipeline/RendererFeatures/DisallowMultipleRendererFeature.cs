using System;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures;

[AttributeUsage(AttributeTargets.Class)]
public class DisallowMultipleRendererFeature : Attribute
{
	public string customTitle { get; private set; }

	public DisallowMultipleRendererFeature(string customTitle = null)
	{
		this.customTitle = customTitle;
	}
}
