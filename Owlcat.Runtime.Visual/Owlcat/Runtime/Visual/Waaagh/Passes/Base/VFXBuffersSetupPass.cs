using Unity.Mathematics;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

public class VFXBuffersSetupPass : ScriptableRenderPass<VFXBuffersSetupPassData>
{
	public override string Name => "VFXBuffersSetupPass";

	public VFXBuffersSetupPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, VFXBuffersSetupPassData data, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.CameraData;
		data.Camera = cameraData.Camera;
		data.ScreenSize = new int2(cameraData.CameraTargetDescriptor.width, cameraData.CameraTargetDescriptor.height);
		VFXCameraBufferTypes vFXCameraBufferTypes = VFXManager.IsCameraBufferNeeded(data.Camera);
		data.NeedDepth = vFXCameraBufferTypes.HasFlag(VFXCameraBufferTypes.Depth);
		data.NeedColor = vFXCameraBufferTypes.HasFlag(VFXCameraBufferTypes.Color);
		bool flag = cameraData.PostProcessEnabled && cameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing;
		bool isSSREnabled = cameraData.IsSSREnabled;
		data.NeedColorBlit = data.NeedColor && !isSSREnabled && !flag;
		if (data.NeedDepth)
		{
			TextureHandle input = data.Resources.CameraHistoryDepthBuffer;
			data.CameraHistoryDepthRT = builder.ReadTexture(in input);
		}
		if (data.NeedColor)
		{
			TextureHandle input = data.Resources.CameraHistoryColorBuffer;
			data.CameraHistoryColorRT = builder.ReadWriteTexture(in input);
			if (data.NeedColorBlit)
			{
				data.CameraColorRT = builder.ReadTexture(in data.Resources.CameraColorBuffer);
			}
		}
	}

	protected override void Render(VFXBuffersSetupPassData data, RenderGraphContext context)
	{
		if (data.NeedDepth)
		{
			VFXManager.SetCameraBuffer(data.Camera, VFXCameraBufferTypes.Depth, data.CameraHistoryDepthRT, 0, 0, data.ScreenSize.x, data.ScreenSize.y);
		}
		if (data.NeedColor)
		{
			if (data.NeedColorBlit)
			{
				context.cmd.Blit(data.CameraColorRT, data.CameraHistoryColorRT);
			}
			VFXManager.SetCameraBuffer(data.Camera, VFXCameraBufferTypes.Color, data.CameraHistoryColorRT, 0, 0, data.ScreenSize.x, data.ScreenSize.y);
		}
	}
}
