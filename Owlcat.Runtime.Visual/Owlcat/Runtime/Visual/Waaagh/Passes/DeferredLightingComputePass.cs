using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal sealed class DeferredLightingComputePass : ScriptableRenderPass<DeferredLightingComputePass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public ShadowQuality ShadowQuality;

		public Color GlossyEnvironmentColor;

		public TextureHandle CameraColorRT;

		public int2 CameraColorSize;

		public int Kernel;

		public int DispatchThreadGroupsX;

		public int DispatchThreadGroupsY;

		public TileSize DispatchTileSize;
	}

	private static readonly int s_Result = Shader.PropertyToID("_Result");

	private static readonly int s_ResultSizeX = Shader.PropertyToID("_ResultSizeX");

	private static readonly int s_ResultSizeY = Shader.PropertyToID("_ResultSizeY");

	private readonly TileSize m_TileSize;

	private readonly ComputeShader m_ComputeShader;

	private readonly int m_MainKernel;

	private readonly LocalKeyword m_TileSize8;

	private readonly LocalKeyword m_TileSize16;

	private readonly LocalKeyword m_TileSize32;

	public override string Name => "DeferredLightingComputePass";

	public DeferredLightingComputePass(RenderPassEvent evt, TileSize tileSize, [NotNull] ComputeShader computeShader)
		: base(evt)
	{
		m_TileSize = tileSize;
		m_ComputeShader = computeShader;
		m_MainKernel = computeShader.FindKernel("main");
		m_TileSize8 = new LocalKeyword(m_ComputeShader, "TILE_SIZE_8");
		m_TileSize16 = new LocalKeyword(m_ComputeShader, "TILE_SIZE_16");
		m_TileSize32 = new LocalKeyword(m_ComputeShader, "TILE_SIZE_32");
	}

	protected override void Setup(RenderGraphBuilder builder, PassData passData, ref RenderingData renderingData)
	{
		builder.DependsOn(in passData.Resources.RendererLists.OpaqueGBuffer.List);
		builder.AllowRendererListCulling(!renderingData.IrsHasOpaques);
		passData.CameraColorRT = builder.WriteTexture(in passData.Resources.CameraColorBuffer);
		builder.ReadTexture(in passData.Resources.CameraAlbedoRT);
		builder.ReadTexture(in passData.Resources.CameraBakedGIRT);
		builder.ReadTexture(in passData.Resources.CameraDepthBuffer);
		builder.ReadTexture(in passData.Resources.CameraNormalsRT);
		builder.ReadTexture(in passData.Resources.CameraShadowmaskRT);
		builder.ReadTexture(in passData.Resources.CameraSpecularRT);
		builder.ReadTexture(in passData.Resources.CameraTranslucencyRT);
		if (passData.Resources.NativeShadowmap.IsValid())
		{
			TextureHandle input = passData.Resources.NativeShadowmap;
			builder.ReadTexture(in input);
		}
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		Color glossyEnvironmentColor = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
		passData.GlossyEnvironmentColor = glossyEnvironmentColor;
		passData.ShadowQuality = renderingData.ShadowData.ShadowQuality;
		passData.CameraColorSize.x = renderingData.CameraData.CameraTargetDescriptor.width;
		passData.CameraColorSize.y = renderingData.CameraData.CameraTargetDescriptor.height;
		passData.Kernel = m_MainKernel;
		passData.DispatchThreadGroupsX = Mathf.CeilToInt((float)passData.CameraColorSize.x / (float)m_TileSize);
		passData.DispatchThreadGroupsY = Mathf.CeilToInt((float)passData.CameraColorSize.y / (float)m_TileSize);
		passData.DispatchTileSize = m_TileSize;
	}

	protected override void Render(PassData passData, RenderGraphContext context)
	{
		ComputeShader computeShader = m_ComputeShader;
		int kernel = passData.Kernel;
		context.cmd.SetKeyword(m_ComputeShader, in m_TileSize8, passData.DispatchTileSize == TileSize.Tile8);
		context.cmd.SetKeyword(m_ComputeShader, in m_TileSize16, passData.DispatchTileSize == TileSize.Tile16);
		context.cmd.SetKeyword(m_ComputeShader, in m_TileSize32, passData.DispatchTileSize == TileSize.Tile32);
		context.cmd.SetComputeTextureParam(computeShader, kernel, s_Result, passData.CameraColorRT);
		context.cmd.SetComputeIntParam(computeShader, s_ResultSizeX, passData.CameraColorSize.x);
		context.cmd.SetComputeIntParam(computeShader, s_ResultSizeY, passData.CameraColorSize.y);
		context.cmd.SetComputeVectorParam(computeShader, ShaderPropertyId._GlossyEnvironmentColor, Color.clear);
		context.cmd.DispatchCompute(computeShader, kernel, passData.DispatchThreadGroupsX, passData.DispatchThreadGroupsY, 1);
		context.cmd.SetComputeVectorParam(computeShader, ShaderPropertyId._GlossyEnvironmentColor, passData.GlossyEnvironmentColor);
	}
}
