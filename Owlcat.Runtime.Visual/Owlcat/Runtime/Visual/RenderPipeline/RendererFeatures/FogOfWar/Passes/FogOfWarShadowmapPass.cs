using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar.Passes;

public class FogOfWarShadowmapPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fog Of War Rendering";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fog Of War Rendering");

	private Material m_FowMaterial;

	private Material m_BlurMaterial;

	private FogOfWarSettings m_FowSettings;

	private FogOfWarFeature m_FowFeature;

	private FogOfWarArea m_Area;

	private RenderTextureDescriptor m_ShadowmapDesc;

	private RenderTargetHandle m_FowShadowmapRT;

	private RenderTargetHandle m_Blur0RT;

	private RenderTargetHandle m_Blur1RT;

	private int m_FowClearPass;

	private int m_FowDrawShadowsPass;

	private int m_FowDrawCharacterQuadPass;

	private int m_FowDrawCharacterQuadMaskPass;

	private int m_FowFinalBlendPass;

	private int m_FowFinalBlendAndMaskPass;

	public override bool IsOncePerFrame => true;

	public FogOfWarShadowmapPass(RenderPassEvent evt, Material fowMaterial, Material blurMaterial)
	{
		base.RenderPassEvent = evt;
		m_FowMaterial = fowMaterial;
		m_BlurMaterial = blurMaterial;
		m_FowClearPass = fowMaterial.FindPass("FOW CLEAR");
		m_FowDrawShadowsPass = fowMaterial.FindPass("FOW DRAW SHADOWS");
		m_FowDrawCharacterQuadPass = fowMaterial.FindPass("DRAW CHARACTER QUAD");
		m_FowDrawCharacterQuadMaskPass = fowMaterial.FindPass("DRAW CHARACTER QUAD MASK");
		m_FowFinalBlendPass = fowMaterial.FindPass("FINAL BLEND");
		m_FowFinalBlendAndMaskPass = fowMaterial.FindPass("FINAL BLEND AND STATIC MASK");
		m_FowShadowmapRT.Init("_FowShadowmapRT");
		m_Blur0RT.Init("_FowBlur0RT");
		m_Blur1RT.Init("_FowBlur1RT");
	}

	public void Setup(FogOfWarSettings settings, FogOfWarFeature feature, FogOfWarArea area)
	{
		m_Area = area;
		m_FowSettings = settings;
		m_FowFeature = feature;
		if (m_Area == null || !m_Area.isActiveAndEnabled)
		{
			return;
		}
		m_ShadowmapDesc = new RenderTextureDescriptor(m_Area.FogOfWarMapRT.rt.width, m_Area.FogOfWarMapRT.rt.height, RenderTextureFormat.ARGB32);
		foreach (FogOfWarRevealer item in FogOfWarRevealer.All)
		{
			if (item != null)
			{
				try
				{
					item.HeightMinMax = settings.CalculateHeightMinMax(item.Position.y);
					item.RebuildShadowMesh();
				}
				finally
				{
				}
			}
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			renderingData.CameraData.Camera.GetComponent<OwlcatAdditionalCameraData>();
			if (!(m_Area == null) && m_Area.isActiveAndEnabled)
			{
				int num = 0;
				commandBuffer.GetTemporaryRT(m_FowShadowmapRT.Id, m_ShadowmapDesc, FilterMode.Bilinear);
				commandBuffer.SetRenderTarget(m_FowShadowmapRT.Identifier());
				commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, m_Area.RevealOnStart ? 1 : 0, 0f, 1f));
				Matrix4x4 matrix4x = m_Area.CalculateProjMatrix();
				Matrix4x4 matrix4x2 = m_Area.CalculateViewMatrix();
				commandBuffer.SetGlobalMatrix(FogOfWarConstantBuffer.VIEW_PROJ, matrix4x * matrix4x2);
				foreach (FogOfWarRevealer item in FogOfWarRevealer.All)
				{
					commandBuffer.EnableScissorRect(CalculateScissorRect(item));
					if (num > 0)
					{
						commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._ColorMask, 1f);
						commandBuffer.SetGlobalColor(FogOfWarConstantBuffer._ClearColor, new Color(0f, 0f, 0f, 1f));
						commandBuffer.Blit(null, m_FowShadowmapRT.Identifier(), m_FowMaterial, m_FowClearPass);
					}
					commandBuffer.SetGlobalVectorArray(FogOfWarConstantBuffer._Vertices, FogOfWarRevealer.DefaultVertices);
					commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._Radius, item.Radius);
					commandBuffer.SetGlobalVector(FogOfWarConstantBuffer._LightPosition, new Vector2(item.Position.x, item.Position.z));
					commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._Falloff, m_FowSettings.ShadowFalloff);
					commandBuffer.DrawMesh(item.ShadowMesh, Matrix4x4.identity, m_FowMaterial, 0, m_FowDrawShadowsPass);
					if (item.MaskTexture == null)
					{
						Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(item.Range, 1f, item.Range), pos: item.Position, q: Quaternion.identity);
						commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRadius, item.Range);
						commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarBorderWidth, m_FowSettings.BorderWidth);
						commandBuffer.DrawMesh(m_FowFeature.QuadMesh, matrix, m_FowMaterial, 0, m_FowDrawCharacterQuadPass);
					}
					else
					{
						Matrix4x4 matrix2 = Matrix4x4.TRS(s: new Vector3(item.Scale.x, 1f, item.Scale.y), pos: item.Position, q: Quaternion.Euler(new Vector3(0f, item.Rotation, 0f)));
						commandBuffer.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarCustomRevealerMask, item.MaskTexture);
						commandBuffer.DrawMesh(m_FowFeature.QuadMesh, matrix2, m_FowMaterial, 0, m_FowDrawCharacterQuadMaskPass);
					}
					commandBuffer.DisableScissorRect();
					num++;
				}
				if (m_FowSettings.IsBlurEnabled && m_Area.IsBlurEnabled)
				{
					float num2 = 1f / (1f * (float)(1 << m_FowSettings.BlurDownsample));
					commandBuffer.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(m_FowSettings.BlurSize * num2, (0f - m_FowSettings.BlurSize) * num2, 0f, 0f));
					int width = m_ShadowmapDesc.width >> m_FowSettings.BlurDownsample;
					int height = m_ShadowmapDesc.height >> m_FowSettings.BlurDownsample;
					commandBuffer.GetTemporaryRT(m_Blur0RT.Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
					commandBuffer.GetTemporaryRT(m_Blur1RT.Id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
					commandBuffer.Blit(m_FowShadowmapRT.Identifier(), m_Blur0RT.Identifier(), m_BlurMaterial, 0);
					int num3 = ((m_FowSettings.BlurType != 0) ? 2 : 0);
					for (int i = 0; i < m_FowSettings.BlurIterations; i++)
					{
						float num4 = (float)i * 1f;
						commandBuffer.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(m_FowSettings.BlurSize * num2 + num4, (0f - m_FowSettings.BlurSize) * num2 - num4, 0f, 0f));
						commandBuffer.Blit(m_Blur0RT.Identifier(), m_Blur1RT.Identifier(), m_BlurMaterial, 1 + num3);
						commandBuffer.Blit(m_Blur1RT.Identifier(), m_Blur0RT.Identifier(), m_BlurMaterial, 2 + num3);
					}
					commandBuffer.Blit(m_Blur0RT.Identifier(), m_FowShadowmapRT.Identifier());
					commandBuffer.ReleaseTemporaryRT(m_Blur0RT.Id);
					commandBuffer.ReleaseTemporaryRT(m_Blur1RT.Id);
				}
				commandBuffer.SetRenderTarget(m_Area.FogOfWarMapRT);
				commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._ColorMask, 10f);
				commandBuffer.SetGlobalColor(FogOfWarConstantBuffer._ClearColor, new Color(0f, 0f, 0f, 1f));
				commandBuffer.Blit(null, m_Area.FogOfWarMapRT, m_FowMaterial, m_FowClearPass);
				commandBuffer.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarShadowMap, m_FowShadowmapRT.Identifier());
				commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._BorderEnabled, m_Area.BorderSettings.Enabled ? 1 : 0);
				if (m_Area.BorderSettings.Enabled)
				{
					commandBuffer.SetGlobalVector(FogOfWarConstantBuffer._WorldSize, m_Area.Bounds.size.To2D());
					commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._BorderWidth, m_Area.BorderSettings.Width);
					commandBuffer.SetGlobalFloat(FogOfWarConstantBuffer._BorderOffset, m_Area.BorderSettings.Offset);
				}
				if (m_Area.StaticMask == null)
				{
					commandBuffer.Blit(null, m_Area.FogOfWarMapRT, m_FowMaterial, m_FowFinalBlendPass);
				}
				else
				{
					commandBuffer.SetGlobalTexture(FogOfWarConstantBuffer._StaticMask, m_Area.StaticMask);
					commandBuffer.Blit(null, m_Area.FogOfWarMapRT, m_FowMaterial, m_FowFinalBlendAndMaskPass);
				}
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private Rect CalculateScissorRect(FogOfWarRevealer revealer)
	{
		float2 @float = revealer.Range;
		if (revealer.MaskTexture != null)
		{
			@float.x = revealer.Scale.x;
			@float.y = revealer.Scale.y;
		}
		Bounds worldBounds = m_Area.GetWorldBounds();
		float4 float2 = new float4(worldBounds.min.x, worldBounds.min.z, worldBounds.max.x, worldBounds.max.z);
		float4 float3 = math.saturate((new float4(revealer.Position.x - @float.x, revealer.Position.z - @float.y, revealer.Position.x + @float.x, revealer.Position.z + @float.y) - float2.xyxy) / (float2.zw - float2.xy).xyxy) * new float2(m_Area.FogOfWarMapRT.rt.width, m_Area.FogOfWarMapRT.rt.height).xyxy;
		return new Rect(float3.xy, float3.zw - float3.xy);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		base.FrameCleanup(cmd);
		cmd.ReleaseTemporaryRT(m_FowShadowmapRT.Id);
	}
}
