using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ClearPass : ScriptableRenderPass<ClearPassData>
{
	public override string Name => "ClearPass";

	public ClearPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, ClearPassData data, ref RenderingData renderingData)
	{
		data.CameraColorBuffer = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		data.CameraDepthBuffer = builder.WriteTexture(in data.Resources.CameraDepthBuffer);
		data.CameraDepthCopy = builder.WriteTexture(in data.Resources.CameraDepthCopyRT);
		data.DepthCopyClearColor = (SystemInfo.usesReversedZBuffer ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 0f));
		SetupClearData(data, in renderingData);
	}

	private void SetupClearData(ClearPassData data, in RenderingData renderingData)
	{
		if (renderingData.CameraData.RenderType != 0)
		{
			data.ClearFlags = (renderingData.CameraData.ClearDepth ? RTClearFlags.DepthStencil : RTClearFlags.Stencil);
			data.ClearColor = Color.clear;
			return;
		}
		switch (renderingData.CameraData.Camera.clearFlags)
		{
		case CameraClearFlags.Skybox:
			data.ClearFlags = RTClearFlags.All;
			data.ClearColor = Color.clear;
			break;
		case CameraClearFlags.Color:
			data.ClearFlags = RTClearFlags.All;
			data.ClearColor = renderingData.CameraData.Camera.backgroundColor;
			data.ClearColor.a = 0f;
			break;
		case CameraClearFlags.Depth:
			data.ClearFlags = RTClearFlags.DepthStencil;
			data.ClearColor = Color.clear;
			break;
		default:
			data.ClearFlags = RTClearFlags.None;
			data.ClearColor = Color.clear;
			break;
		}
	}

	protected override void Render(ClearPassData data, RenderGraphContext context)
	{
		if (data.ClearFlags != 0)
		{
			context.cmd.SetRenderTarget(data.CameraColorBuffer, data.CameraDepthBuffer);
			context.cmd.ClearRenderTarget(data.ClearFlags, data.ClearColor, 1f, 0u);
		}
		context.cmd.SetRenderTarget(data.CameraDepthCopy);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, data.DepthCopyClearColor);
	}
}
