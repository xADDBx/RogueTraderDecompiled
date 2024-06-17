using System;
using Owlcat.Runtime.Visual.RenderPipeline;
using Owlcat.Runtime.Visual.RenderPipeline.Data;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Overrides;

[Serializable]
[VolumeComponentMenu("Post-processing/Color Lookup")]
public sealed class ColorLookup : VolumeComponent, IPostProcessComponent
{
	[Tooltip("A custom 2D texture lookup table to apply.")]
	public TextureParameter texture = new TextureParameter(null);

	[Tooltip("How much of the lookup texture will contribute to the color grading effect.")]
	public ClampedFloatParameter contribution = new ClampedFloatParameter(1f, 0f, 1f);

	public bool IsActive()
	{
		if (contribution.value > 0f)
		{
			return ValidateLUT();
		}
		return false;
	}

	public bool IsTileCompatible()
	{
		return true;
	}

	public bool ValidateLUT()
	{
		OwlcatRenderPipelineAsset asset = OwlcatRenderPipeline.Asset;
		WaaaghPipelineAsset asset2 = WaaaghPipeline.Asset;
		if ((asset == null && asset2 == null) || texture.value == null)
		{
			return false;
		}
		int num = -1;
		if (asset != null)
		{
			num = asset.PostProcessSettings.ColorGradingLutSize;
		}
		if (asset2 != null)
		{
			num = asset2.PostProcessSettings.ColorGradingLutSize;
		}
		if (texture.value.height != num)
		{
			return false;
		}
		bool flag = false;
		Texture value = texture.value;
		if (!(value is Texture2D texture2D))
		{
			if (value is RenderTexture renderTexture)
			{
				flag |= renderTexture.dimension == TextureDimension.Tex2D && renderTexture.width == num * num && !renderTexture.sRGB;
			}
		}
		else
		{
			flag |= texture2D.width == num * num && !GraphicsFormatUtility.IsSRGBFormat(texture2D.graphicsFormat);
		}
		return flag;
	}
}
