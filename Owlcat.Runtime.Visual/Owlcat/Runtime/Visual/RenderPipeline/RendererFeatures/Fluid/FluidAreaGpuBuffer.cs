using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

public class FluidAreaGpuBuffer
{
	public RenderTextureBuffer VelocityBuffer;

	public RenderTextureBuffer PressureBuffer;

	public RenderTextureBuffer DivergenceBuffer;

	public RenderTextureBuffer VorticityBuffer;

	public RenderTextureBuffer FluidFogColorBuffer;

	public RenderTexture StencilBuffer;

	public FluidAreaGpuBuffer(FluidArea area)
	{
		FluidFeature instance = FluidFeature.Instance;
		Vector3 size = area.Bounds.size;
		Vector2Int vector2Int = new Vector2Int((int)(size.x * instance.TextureDensity), (int)(size.z * instance.TextureDensity));
		VelocityBuffer = new RenderTextureBuffer(doubleBuffer: true, vector2Int.x, vector2Int.y, 0, RenderTextureFormat.ARGBHalf, "VelocityBuffer");
		PressureBuffer = new RenderTextureBuffer(doubleBuffer: true, vector2Int.x, vector2Int.y, 0, RenderTextureFormat.RGHalf, "PressureBuffer");
		DivergenceBuffer = new RenderTextureBuffer(doubleBuffer: false, vector2Int.x, vector2Int.y, 0, RenderTextureFormat.RGHalf, "DivergenceBuffer");
		VorticityBuffer = new RenderTextureBuffer(doubleBuffer: false, vector2Int.x, vector2Int.y, 0, RenderTextureFormat.RGHalf, "VorticityBuffer");
		FluidFogColorBuffer = new RenderTextureBuffer(doubleBuffer: true, vector2Int.x, vector2Int.y, 0, RenderTextureFormat.ARGBHalf, "FluidFogColorBuffer");
		StencilBuffer = new RenderTexture(vector2Int.x, vector2Int.y, 32, RenderTextureFormat.Depth);
		StencilBuffer.name = "FluidStencilBuffer";
	}

	public void Reset()
	{
		ResetTextureBuffer(VelocityBuffer);
		ResetTextureBuffer(PressureBuffer);
		ResetTextureBuffer(DivergenceBuffer);
		ResetTextureBuffer(VorticityBuffer);
		ResetTextureBuffer(FluidFogColorBuffer);
		ResetRenderTexture(StencilBuffer, resetDepthStencil: true);
	}

	private void ResetTextureBuffer(RenderTextureBuffer buffer)
	{
		ResetRenderTexture(buffer.Rt0, resetDepthStencil: false);
		if (buffer.IsDouble)
		{
			ResetRenderTexture(buffer.Rt1, resetDepthStencil: false);
		}
	}

	private void ResetRenderTexture(RenderTexture rt, bool resetDepthStencil)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		GL.Clear(resetDepthStencil, clearColor: true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = active;
	}

	public void Dispose()
	{
		VelocityBuffer.Dispose();
		PressureBuffer.Dispose();
		DivergenceBuffer.Dispose();
		VorticityBuffer.Dispose();
	}
}
