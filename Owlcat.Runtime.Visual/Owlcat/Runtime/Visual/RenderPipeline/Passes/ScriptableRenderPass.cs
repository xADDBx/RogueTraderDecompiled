using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public abstract class ScriptableRenderPass
{
	public RenderPassEvent RenderPassEvent { get; set; }

	public virtual bool IsOncePerFrame => false;

	public ScriptableRenderPass()
	{
		RenderPassEvent = RenderPassEvent.AfterRenderingOpaques;
	}

	public virtual void FrameCleanup(CommandBuffer cmd)
	{
	}

	public abstract void Execute(ScriptableRenderContext context, ref RenderingData renderingData);

	public void RenderPostProcessing(CommandBuffer cmd, ref CameraData cameraData, RenderTextureDescriptor sourceDescriptor, RenderTargetIdentifier source, RenderTargetIdentifier destination, bool opaqueOnly, bool flip)
	{
	}

	public DrawingSettings CreateDrawingSettings(ShaderTagId shaderTagId, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		Camera camera = renderingData.CameraData.Camera;
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = sortingCriteria;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings result = new DrawingSettings(shaderTagId, sortingSettings2);
		result.perObjectData = renderingData.PerObjectData;
		result.enableInstancing = true;
		result.mainLightIndex = renderingData.LightData.MainLightIndex;
		result.enableDynamicBatching = renderingData.SupportsDynamicBatching;
		return result;
	}

	public DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, ref RenderingData renderingData, SortingCriteria sortingCriteria)
	{
		if (shaderTagIdList == null || shaderTagIdList.Count == 0)
		{
			Debug.LogWarning("ShaderTagId list is invalid. DrawingSettings is created with default pipeline ShaderTagId");
			return CreateDrawingSettings(new ShaderTagId("OwlcatPipeline"), ref renderingData, sortingCriteria);
		}
		DrawingSettings result = CreateDrawingSettings(shaderTagIdList[0], ref renderingData, sortingCriteria);
		for (int i = 1; i < shaderTagIdList.Count; i++)
		{
			result.SetShaderPassName(i, shaderTagIdList[i]);
		}
		return result;
	}

	public static bool operator <(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.RenderPassEvent < rhs.RenderPassEvent;
	}

	public static bool operator >(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.RenderPassEvent > rhs.RenderPassEvent;
	}

	internal static void SortStable(List<ScriptableRenderPass> list)
	{
		for (int i = 1; i < list.Count; i++)
		{
			ScriptableRenderPass scriptableRenderPass = list[i];
			int num = i - 1;
			while (num >= 0 && scriptableRenderPass < list[num])
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = scriptableRenderPass;
		}
	}

	internal void SetRenderTarget(CommandBuffer cmd, RenderTargetIdentifier colorAttachment, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, ClearFlag clearFlags, Color clearColor, TextureDimension dimension)
	{
		if (dimension == TextureDimension.Tex2DArray)
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, clearFlags, clearColor);
		}
		else
		{
			CoreUtils.SetRenderTarget(cmd, colorAttachment, colorLoadAction, colorStoreAction, clearFlags, clearColor);
		}
	}

	public void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material = null, int passIndex = 0)
	{
		ScriptableRenderer.SetRenderTarget(cmd, destination, BuiltinRenderTextureType.CameraTarget, ClearFlag.None, Color.black);
		cmd.Blit(source, destination, material, passIndex);
	}
}
