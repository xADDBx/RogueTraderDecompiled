using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class SetCameraShaderVariablesPass : ScriptableRenderPass<SetCameraShaderVariablesPassData>
{
	public override string Name => "SetCameraShaderVariablesPass";

	public SetCameraShaderVariablesPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, SetCameraShaderVariablesPassData data, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		Camera camera = cameraData.Camera;
		float num = cameraData.CameraTargetDescriptor.width;
		float num2 = cameraData.CameraTargetDescriptor.height;
		float num3 = cameraData.CameraTargetDescriptor.width;
		float num4 = cameraData.CameraTargetDescriptor.height;
		if (camera.allowDynamicResolution)
		{
			num *= ScalableBufferManager.widthScaleFactor;
			num2 *= ScalableBufferManager.heightScaleFactor;
		}
		float nearClipPlane = camera.nearClipPlane;
		float farClipPlane = camera.farClipPlane;
		float num5 = (Mathf.Approximately(nearClipPlane, 0f) ? 0f : (1f / nearClipPlane));
		float num6 = (Mathf.Approximately(farClipPlane, 0f) ? 0f : (1f / farClipPlane));
		float w = (camera.orthographic ? 1f : 0f);
		float num7 = 1f - farClipPlane * num5;
		float num8 = farClipPlane * num5;
		Vector4 zBufferParams = new Vector4(num7, num8, num7 * num6, num8 * num6);
		if (SystemInfo.usesReversedZBuffer)
		{
			zBufferParams.y += zBufferParams.x;
			zBufferParams.x = 0f - zBufferParams.x;
			zBufferParams.w += zBufferParams.z;
			zBufferParams.z = 0f - zBufferParams.z;
		}
		data.CameraRenderType = cameraData.RenderType;
		if (data.CameraRenderType == CameraRenderType.Overlay)
		{
			float x = ((true && SystemInfo.graphicsUVStartsAtTop) ? (-1f) : 1f);
			Vector4 projectionParams = new Vector4(x, nearClipPlane, farClipPlane, 1f * num6);
			data.ProjectionParams = projectionParams;
		}
		data.OrthoParams = new Vector4(camera.orthographicSize * cameraData.FinalTargetAspectRatio, camera.orthographicSize, 0f, w);
		data.WorldSpaceCameraPos = cameraData.WorldSpaceCameraPos;
		data.ScreenParams = new Vector4(num3, num4, 1f + 1f / num3, 1f + 1f / num4);
		data.ScaledScreenParams = new Vector4(num, num2, 1f + 1f / num, 1f + 1f / num2);
		data.ZBufferParams = zBufferParams;
		data.ScreenSize = new Vector4(num3, num4, 1f / num3, 1f / num4);
		float num9 = ((cameraData.CameraRenderTargetBufferType == CameraRenderTargetType.Scaled) ? Math.Min((float)(0.0 - Math.Log(1f / cameraData.RenderScale, 2.0)), 0f) : 0f);
		data.GlobalMipBias = new Vector2(num9, Mathf.Pow(2f, num9));
	}

	protected override void Render(SetCameraShaderVariablesPassData data, RenderGraphContext context)
	{
		CommandBuffer cmd = context.cmd;
		cmd.SetGlobalVector(ShaderPropertyId._WorldSpaceCameraPos, data.WorldSpaceCameraPos);
		cmd.SetGlobalVector(ShaderPropertyId._ScreenParams, data.ScreenParams);
		cmd.SetGlobalVector(ShaderPropertyId._ScaledScreenParams, data.ScaledScreenParams);
		cmd.SetGlobalVector(ShaderPropertyId._ZBufferParams, data.ZBufferParams);
		cmd.SetGlobalVector(ShaderPropertyId.unity_OrthoParams, data.OrthoParams);
		if (data.CameraRenderType == CameraRenderType.Overlay)
		{
			cmd.SetGlobalVector(ShaderPropertyId._ProjectionParams, data.ProjectionParams);
		}
		cmd.SetGlobalVector(ShaderPropertyId._ScreenSize, data.ScreenSize);
		cmd.SetGlobalVector(ShaderPropertyId._GlobalMipBias, data.GlobalMipBias);
	}
}
