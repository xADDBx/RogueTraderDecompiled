using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class ColorGradingLutPassData : PassDataBase
{
	public TextureDesc OutputDesc;

	public TextureHandle Output;

	public ChannelMixer ChannelMixer;

	public ColorAdjustments ColorAdjustments;

	public ColorCurves ColorCurves;

	public LiftGammaGain LiftGammaGain;

	public SlopePowerOffset SlopePowerOffset;

	public ShadowsMidtonesHighlights ShadowsMidtonesHighlights;

	public SplitToning SplitToning;

	public Tonemapping Tonemapping;

	public WhiteBalance WhiteBalance;

	public Material Material;

	public int LutWidth;

	public int LutHeight;

	public bool IsHdr;
}
