using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ComputeTilesMinMaxZPass : ScriptableRenderPass<ComputeTilesMinMaxZPassData>
{
	private ComputeShader m_Shader;

	private WaaaghLights m_Lights;

	private static readonly int _TilesMinMaxZUAV = Shader.PropertyToID("_TilesMinMaxZUAV");

	private static readonly string MINMAXZ_TILE_SIZE_8 = "MINMAXZ_TILE_SIZE_8";

	private static readonly string MINMAXZ_TILE_SIZE_16 = "MINMAXZ_TILE_SIZE_16";

	private static readonly string MINMAXZ_TILE_SIZE_32 = "MINMAXZ_TILE_SIZE_32";

	private static readonly string MAX_ONLY = "MAX_ONLY";

	public override string Name => "ComputeTilesMinMaxZPass";

	public ComputeTilesMinMaxZPass(RenderPassEvent evt, ComputeShader tileMinMaxZShader, WaaaghLights waaaghLights)
		: base(evt)
	{
		m_Shader = tileMinMaxZShader;
		m_Lights = waaaghLights;
	}

	protected override void Setup(RenderGraphBuilder builder, ComputeTilesMinMaxZPassData data, ref RenderingData renderingData)
	{
		data.Shader = m_Shader;
		data.CameraDepthCopyTexture = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		data.MaxDepthOnly = true;
		TextureDesc desc = new TextureDesc((int)m_Lights.ClusteringParams.x, (int)m_Lights.ClusteringParams.y);
		desc.depthBufferBits = DepthBits.None;
		desc.colorFormat = (data.MaxDepthOnly ? GraphicsFormat.R32_SFloat : GraphicsFormat.R32G32_SFloat);
		desc.enableRandomWrite = true;
		desc.name = "TilesMinMaxZ";
		data.Resources.TilesMinMaxZTexture = data.Resources.RenderGraph.CreateTexture(in desc);
		data.TilesMinMaxZTexture = builder.WriteTexture(in data.Resources.TilesMinMaxZTexture);
		data.DispatchSize = new int3(desc.width, desc.height, 1);
		data.TileSize = (int)m_Lights.ClusteringParams.z;
	}

	protected override void Render(ComputeTilesMinMaxZPassData data, RenderGraphContext context)
	{
		context.cmd.DisableShaderKeyword(MINMAXZ_TILE_SIZE_8);
		context.cmd.DisableShaderKeyword(MINMAXZ_TILE_SIZE_16);
		context.cmd.DisableShaderKeyword(MINMAXZ_TILE_SIZE_32);
		switch (data.TileSize)
		{
		case 8:
			context.cmd.EnableShaderKeyword(MINMAXZ_TILE_SIZE_8);
			break;
		case 16:
			context.cmd.EnableShaderKeyword(MINMAXZ_TILE_SIZE_16);
			break;
		case 32:
			context.cmd.EnableShaderKeyword(MINMAXZ_TILE_SIZE_32);
			break;
		}
		if (data.MaxDepthOnly)
		{
			context.cmd.EnableShaderKeyword(MAX_ONLY);
		}
		else
		{
			context.cmd.DisableShaderKeyword(MAX_ONLY);
		}
		context.cmd.SetComputeTextureParam(data.Shader, 0, _TilesMinMaxZUAV, data.TilesMinMaxZTexture);
		context.cmd.DispatchCompute(data.Shader, 0, data.DispatchSize.x, data.DispatchSize.y, data.DispatchSize.z);
	}
}
