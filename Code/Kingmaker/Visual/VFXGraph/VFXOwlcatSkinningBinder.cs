using System;
using Owlcat.Runtime.Visual.GPUSkinning;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Kingmaker.Visual.VFXGraph;

public class VFXOwlcatSkinningBinder : VFXBinderBase
{
	private static class ShaderPropertyId
	{
		public static int GPUSkinningRotationsConstantBuffer = Shader.PropertyToID("GPUSkinningRotationsConstantBuffer");

		public static int GPUSkinningPositionsConstantBuffer = Shader.PropertyToID("GPUSkinningPositionsConstantBuffer");
	}

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	public ExposedProperty SkinningTexture = "SkinningTexture";

	[VFXPropertyBinding(new string[] { "System.Single" })]
	public ExposedProperty SkinningFps = "SkinningFps";

	[VFXPropertyBinding(new string[] { "System.Single" })]
	public ExposedProperty SkinningBonesCount = "SkinningBonesCount";

	[VFXPropertyBinding(new string[] { "System.Single" })]
	public ExposedProperty SkinningClipsCount = "SkinningClipsCount";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector4" })]
	public ExposedProperty SkinningFramesOffset = "SkinningFramesOffset";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector4" })]
	public ExposedProperty SkinningFramesCount = "SkinningFramesCount";

	public GPUSkinningAnimation GPUSkinningAnimation;

	protected override void OnEnable()
	{
		if (!binder.m_Bindings.Contains(this))
		{
			binder.m_Bindings.Add(this);
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (component.HasTexture(SkinningTexture) && component.HasFloat(SkinningFps) && component.HasFloat(SkinningBonesCount) && component.HasFloat(SkinningClipsCount) && component.HasVector4(SkinningFramesOffset))
		{
			return component.HasVector4(SkinningFramesCount);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		if (GPUSkinningAnimation != null)
		{
			if (GPUSkinningAnimation.AnimationTexture != null && component.HasTexture(SkinningTexture))
			{
				component.SetTexture(SkinningTexture, GPUSkinningAnimation.AnimationTexture);
			}
			if (component.HasFloat(SkinningFps))
			{
				component.SetFloat(SkinningFps, GPUSkinningAnimation.BakeFPS);
			}
			if (component.HasFloat(SkinningBonesCount))
			{
				component.SetFloat(SkinningBonesCount, GPUSkinningAnimation.BonesCount);
			}
			int num = Math.Min(4, GPUSkinningAnimation.AnimationClips.Count);
			if (component.HasFloat(SkinningClipsCount))
			{
				component.SetFloat(SkinningClipsCount, num);
			}
			int num2 = 0;
			Vector4 v = default(Vector4);
			Vector4 v2 = default(Vector4);
			for (int i = 0; i < num; i++)
			{
				GPUAnimationClip gPUAnimationClip = GPUSkinningAnimation.AnimationClips[i];
				v2[i] = gPUAnimationClip.FrameCount;
				v[i] = num2;
				num2 += gPUAnimationClip.FrameCount;
			}
			if (component.HasVector4(SkinningFramesOffset))
			{
				component.SetVector4(SkinningFramesOffset, v);
			}
			if (component.HasVector4(SkinningFramesCount))
			{
				component.SetVector4(SkinningFramesCount, v2);
			}
		}
	}
}
