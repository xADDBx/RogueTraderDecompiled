using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarShadowmapPass : ScriptableRenderPass<FogOfWarShadowmapPassData>
{
	private Material m_FowMaterial;

	private Material m_BlurMaterial;

	private FogOfWarSettings m_FowSettings;

	private FogOfWarFeature m_FowFeature;

	private FogOfWarArea m_Area;

	private int m_FowClearPass;

	private int m_FowDrawShadowsPass;

	private int m_FowDrawCharacterQuadPass;

	private int m_FowDrawCharacterQuadMaskPass;

	private int m_FowFinalBlendPass;

	private int m_FowFinalBlendAndMaskPass;

	private int m_FowHistoryCopyPass;

	private TextureDesc m_ShadowmapDesc;

	public override string Name => "FogOfWarShadowmapPass";

	public FogOfWarShadowmapPass(RenderPassEvent evt, Material fowMaterial, Material blurMaterial)
		: base(evt)
	{
		m_FowMaterial = fowMaterial;
		m_BlurMaterial = blurMaterial;
		m_FowClearPass = fowMaterial.FindPass("FOW CLEAR");
		m_FowDrawShadowsPass = fowMaterial.FindPass("FOW DRAW SHADOWS");
		m_FowDrawCharacterQuadPass = fowMaterial.FindPass("DRAW CHARACTER QUAD");
		m_FowDrawCharacterQuadMaskPass = fowMaterial.FindPass("DRAW CHARACTER QUAD MASK");
		m_FowFinalBlendPass = fowMaterial.FindPass("FINAL BLEND");
		m_FowFinalBlendAndMaskPass = fowMaterial.FindPass("FINAL BLEND AND STATIC MASK");
		m_FowHistoryCopyPass = fowMaterial.FindPass("HISTORY COPY");
	}

	internal void Init(FogOfWarArea area, FogOfWarFeature feature, FogOfWarSettings settings)
	{
		m_Area = area;
		m_FowSettings = settings;
		m_FowFeature = feature;
		m_ShadowmapDesc = new TextureDesc(m_Area.FogOfWarMapRT.rt.width, m_Area.FogOfWarMapRT.rt.height);
		m_ShadowmapDesc.name = "_FowShadowmapRT";
		m_ShadowmapDesc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		m_ShadowmapDesc.depthBufferBits = DepthBits.None;
		m_ShadowmapDesc.filterMode = FilterMode.Bilinear;
		m_ShadowmapDesc.wrapMode = TextureWrapMode.Clamp;
		foreach (FogOfWarRevealer item in FogOfWarRevealer.All)
		{
			if (item != null)
			{
				float num = item.Position.y + settings.ShadowCullingHeightOffset;
				item.HeightMinMax = new Vector2(num, num + settings.ShadowCullingHeight);
				item.RebuildShadowMesh();
			}
		}
	}

	protected override void Setup(RenderGraphBuilder builder, FogOfWarShadowmapPassData data, ref RenderingData renderingData)
	{
		if (!m_Area.RevealOnStart)
		{
			TextureDesc desc = m_ShadowmapDesc;
			desc.name = "_FOWHistoryCopy";
			desc.colorFormat = GraphicsFormat.R8_UNorm;
			data.FowHistoryCopyRT = builder.CreateTransientTexture(in desc);
		}
		data.Proj = m_Area.CalculateProjMatrix();
		data.View = m_Area.CalculateViewMatrix();
		data.Area = m_Area;
		data.Settings = m_FowSettings;
		data.Feature = m_FowFeature;
		int width = m_ShadowmapDesc.width >> data.Settings.BlurDownsample;
		int height = m_ShadowmapDesc.height >> data.Settings.BlurDownsample;
		TextureDesc desc2 = new TextureDesc(width, height);
		desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
		desc2.depthBufferBits = DepthBits.None;
		desc2.filterMode = FilterMode.Bilinear;
		desc2.wrapMode = TextureWrapMode.Clamp;
		desc2.name = "FowBlur0RT";
		data.FowBlur0RT = builder.CreateTransientTexture(in desc2);
		desc2.name = "FowBlur1RT";
		data.FowBlur1RT = builder.CreateTransientTexture(in desc2);
		data.FowMaterial = m_FowMaterial;
		data.BlurMaterial = m_BlurMaterial;
		data.FowClearPass = m_FowClearPass;
		data.FowDrawShadowsPass = m_FowDrawShadowsPass;
		data.FowDrawCharacterQuadPass = m_FowDrawCharacterQuadPass;
		data.FowDrawCharacterQuadMaskPass = m_FowDrawCharacterQuadMaskPass;
		data.FowFinalBlendPass = m_FowFinalBlendPass;
		data.FowFinalBlendAndMaskPass = m_FowFinalBlendAndMaskPass;
		data.FowHistoryCopyPass = m_FowHistoryCopyPass;
	}

	protected override void Render(FogOfWarShadowmapPassData data, RenderGraphContext context)
	{
		if (!data.Area.RevealOnStart)
		{
			context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarShadowMap, data.Area.FogOfWarMapRT);
			context.cmd.Blit(null, data.FowHistoryCopyRT, data.FowMaterial, data.FowHistoryCopyPass);
		}
		context.cmd.SetRenderTarget(data.Area.FogOfWarMapRT);
		context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._ColorMask, 15f);
		context.cmd.SetGlobalColor(FogOfWarConstantBuffer._ClearColor, new Color(0f, data.Area.RevealOnStart ? 1 : 0, 0f, 1f));
		context.cmd.Blit(null, data.Area.FogOfWarMapRT, data.FowMaterial, data.FowClearPass);
		context.cmd.SetGlobalMatrix(FogOfWarConstantBuffer.VIEW_PROJ, data.Proj * data.View);
		bool flag = true;
		foreach (FogOfWarRevealer item in FogOfWarRevealer.All)
		{
			context.cmd.EnableScissorRect(CalculateScissorRect(item));
			if (!flag)
			{
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._ColorMask, 1f);
				context.cmd.SetGlobalColor(FogOfWarConstantBuffer._ClearColor, new Color(0f, 0f, 0f, 1f));
				context.cmd.Blit(null, data.Area.FogOfWarMapRT, data.FowMaterial, data.FowClearPass);
			}
			context.cmd.SetGlobalVectorArray(FogOfWarConstantBuffer._Vertices, FogOfWarRevealer.DefaultVertices);
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._Radius, item.Radius);
			Vector3 size = data.Area.Bounds.size;
			_ = data.Area.FogOfWarMapRT.rt.width;
			_ = data.Area.FogOfWarMapRT.rt.height;
			context.cmd.SetGlobalVector("_FogOfWarAreaSize", new Vector4(size.x, size.z));
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRadius, item.Range);
			context.cmd.SetGlobalVector(FogOfWarConstantBuffer._LightPosition, new Vector2(item.Position.x, item.Position.z));
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._Falloff, data.Settings.ShadowFalloff);
			context.cmd.DrawMesh(item.ShadowMesh, Matrix4x4.identity, data.FowMaterial, 0, data.FowDrawShadowsPass);
			if (item.MaskTexture == null)
			{
				Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(item.Range, 1f, item.Range), pos: item.Position, q: Quaternion.identity);
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRadius, item.Range);
				context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarBorderWidth, data.Settings.BorderWidth);
				context.cmd.DrawMesh(data.Feature.QuadMesh, matrix, data.FowMaterial, 0, data.FowDrawCharacterQuadPass);
			}
			else
			{
				Matrix4x4 matrix2 = Matrix4x4.TRS(s: new Vector3(item.Scale.x, 1f, item.Scale.y), pos: item.Position, q: Quaternion.Euler(new Vector3(0f, item.Rotation, 0f)));
				context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarCustomRevealerMask, item.MaskTexture);
				context.cmd.DrawMesh(data.Feature.QuadMesh, matrix2, data.FowMaterial, 0, data.FowDrawCharacterQuadMaskPass);
			}
			context.cmd.DisableScissorRect();
			flag = false;
		}
		if (data.Settings.IsBlurEnabled && data.Area.IsBlurEnabled)
		{
			float num = 1f / (1f * (float)(1 << data.Settings.BlurDownsample));
			context.cmd.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(data.Settings.BlurSize * num, (0f - data.Settings.BlurSize) * num, 0f, 0f));
			context.cmd.Blit(data.Area.FogOfWarMapRT, data.FowBlur0RT, data.BlurMaterial, 0);
			int num2 = ((data.Settings.BlurType != 0) ? 2 : 0);
			for (int i = 0; i < data.Settings.BlurIterations; i++)
			{
				float num3 = (float)i * 1f;
				context.cmd.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(data.Settings.BlurSize * num + num3, (0f - data.Settings.BlurSize) * num - num3, 0f, 0f));
				context.cmd.Blit(data.FowBlur0RT, data.FowBlur1RT, data.BlurMaterial, 1 + num2);
				context.cmd.Blit(data.FowBlur1RT, data.FowBlur0RT, data.BlurMaterial, 2 + num2);
			}
			context.cmd.Blit(data.FowBlur0RT, data.Area.FogOfWarMapRT);
		}
		context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._BorderEnabled, data.Area.BorderSettings.Enabled ? 1 : 0);
		if (data.Area.BorderSettings.Enabled)
		{
			context.cmd.SetGlobalVector(FogOfWarConstantBuffer._WorldSize, data.Area.Bounds.size.To2D());
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._BorderWidth, data.Area.BorderSettings.Width);
			context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._BorderOffset, data.Area.BorderSettings.Offset);
		}
		if (!data.Area.RevealOnStart)
		{
			context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarHistoryCopyMap, data.FowHistoryCopyRT);
		}
		context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarRevealOnStart, data.Area.RevealOnStart ? 1 : 0);
		if (data.Area.StaticMask == null)
		{
			context.cmd.Blit(null, data.Area.FogOfWarMapRT, data.FowMaterial, data.FowFinalBlendPass);
			return;
		}
		context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._StaticMask, data.Area.StaticMask);
		context.cmd.Blit(null, data.Area.FogOfWarMapRT, data.FowMaterial, data.FowFinalBlendAndMaskPass);
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
}
