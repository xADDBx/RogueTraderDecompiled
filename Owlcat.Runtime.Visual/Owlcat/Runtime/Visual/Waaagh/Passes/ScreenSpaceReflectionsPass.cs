using Owlcat.Runtime.Visual.Overrides;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ScreenSpaceReflectionsPass : ScriptableRenderPass<ScreenSpaceReflectionsPassData>
{
	private static class ShaderConstants
	{
		public static readonly int _SsrHitPointTexture = Shader.PropertyToID("_SsrHitPointTexture");

		public static readonly int _SsrHitPointTextureSize = Shader.PropertyToID("_SsrHitPointTextureSize");

		public static readonly int _SsrResolveTexture = Shader.PropertyToID("_SsrResolveTexture");

		public static readonly int _SsrIterLimit = Shader.PropertyToID("_SsrIterLimit");

		public static readonly int _SsrThicknessScale = Shader.PropertyToID("_SsrThicknessScale");

		public static readonly int _SsrThicknessBias = Shader.PropertyToID("_SsrThicknessBias");

		public static readonly int _SsrFresnelPower = Shader.PropertyToID("_SsrFresnelPower");

		public static readonly int _DepthPyramidMipRects = Shader.PropertyToID("_DepthPyramidMipRects");

		public static readonly int _SsrMinDepthMipLevel = Shader.PropertyToID("_SsrMinDepthMipLevel");

		public static readonly int _SsrScreenSize = Shader.PropertyToID("_SsrScreenSize");

		public static readonly int _SsrRoughnessFadeEnd = Shader.PropertyToID("_SsrRoughnessFadeEnd");

		public static readonly int _SsrRoughnessFadeStart = Shader.PropertyToID("_SsrRoughnessFadeStart");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilComp = Shader.PropertyToID("_StencilComp");

		public static readonly int _ProjectToPixelMatrix = Shader.PropertyToID("_ProjectToPixelMatrix");

		public static readonly int _WorldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");

		public static readonly int _ProjInfo = Shader.PropertyToID("_ProjInfo");

		public static readonly int _CameraClipInfo = Shader.PropertyToID("_CameraClipInfo");

		public static readonly int _MaxRayTraceDistance = Shader.PropertyToID("_MaxRayTraceDistance");

		public static readonly int _LayerThickness = Shader.PropertyToID("_LayerThickness");

		public static readonly int _RayStepSize = Shader.PropertyToID("_RayStepSize");

		public static readonly int _SsrPyramidTexture = Shader.PropertyToID("_SsrPyramidTexture");

		public static readonly int _SsrPyramidLodCount = Shader.PropertyToID("_SsrPyramidLodCount");

		public static readonly int _HighlightSuppression = Shader.PropertyToID("_HighlightSuppression");

		public static readonly int _MotionVectorsTex = Shader.PropertyToID("_MotionVectorsTex");

		public static readonly int _UseMotionVectorsForReprojection = Shader.PropertyToID("_UseMotionVectorsForReprojection");

		public static readonly int _SsrBlruEnabled = Shader.PropertyToID("_SsrBlruEnabled");

		public static readonly int _SsrRoughnessRemap = Shader.PropertyToID("_SsrRoughnessRemap");
	}

	private const int kMaxSsrPyramidLodCount = 3;

	private ComputeShader m_SsrShader;

	private int m_TraceHiZKernel;

	private int m_TraceScreenSpaceKernel;

	private Material m_SsrResolveMaterial;

	private int m_SsrResolvePass;

	private int m_SsrBlurPass;

	private int m_SsrCompositeSsrPass;

	private Material m_BlitMaterial;

	private ScreenSpaceReflections m_Settings;

	public override string Name => "ScreenSpaceReflectionsPass";

	public ScreenSpaceReflectionsPass(RenderPassEvent evt, ComputeShader ssrCS, Material ssrResolveMaterial, Material blitMaterial)
		: base(evt)
	{
		m_SsrShader = ssrCS;
		m_TraceHiZKernel = m_SsrShader.FindKernel("TracingHiZ");
		m_TraceScreenSpaceKernel = m_SsrShader.FindKernel("TracingScreenSpace");
		m_SsrResolveMaterial = ssrResolveMaterial;
		m_SsrResolvePass = m_SsrResolveMaterial.FindPass("Resolve");
		m_SsrBlurPass = m_SsrResolveMaterial.FindPass("BilateralBlur");
		m_SsrCompositeSsrPass = m_SsrResolveMaterial.FindPass("CompositeSSR");
		m_BlitMaterial = blitMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, ScreenSpaceReflectionsPassData data, ref RenderingData renderingData)
	{
		builder.DependsOn(in data.Resources.RendererLists.OpaqueGBuffer.List);
		builder.AllowRendererListCulling(value: true);
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<ScreenSpaceReflections>();
		if (m_Settings.IsActive())
		{
			int2 @int = new int2(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
			switch (m_Settings.Quality.value)
			{
			case ScreenSpaceReflectionsQuality.Half:
				@int /= 2;
				data.MinDepthLevel = 1;
				break;
			case ScreenSpaceReflectionsQuality.Full:
				data.MinDepthLevel = 0;
				break;
			}
			TextureDesc desc = RenderingUtils.CreateTextureDesc("SsrHiPointRT", renderingData.CameraData.CameraTargetDescriptor);
			desc.colorFormat = GraphicsFormat.R16G16_SFloat;
			desc.depthBufferBits = DepthBits.None;
			desc.filterMode = FilterMode.Point;
			desc.wrapMode = TextureWrapMode.Clamp;
			desc.enableRandomWrite = true;
			desc.width = @int.x;
			desc.height = @int.y;
			TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrRT", renderingData.CameraData.CameraTargetDescriptor);
			desc2.depthBufferBits = DepthBits.None;
			desc2.filterMode = FilterMode.Bilinear;
			desc2.wrapMode = TextureWrapMode.Clamp;
			desc2.width = @int.x;
			desc2.height = @int.y;
			data.Resources.SsrRT = renderingData.RenderGraph.CreateTexture(in desc2);
			data.SsrRT = builder.WriteTexture(in data.Resources.SsrRT);
			bool blurEnabled = data.BlurEnabled;
			desc2.name = "SsrPyramidRT";
			desc2.autoGenerateMips = false;
			desc2.useMipMap = blurEnabled;
			data.SsrPyramidRT = builder.CreateTransientTexture(in desc2);
			if (blurEnabled)
			{
				desc2.width /= 2;
				desc2.height /= 2;
				desc2.useMipMap = false;
				desc2.name = "TempRT0";
				data.TempRT0 = builder.CreateTransientTexture(in desc2);
			}
			data.SsrHitPointRT = builder.CreateTransientTexture(in desc);
			data.CameraDepthRT = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
			if (m_Settings.TracingMethod.value == TracingMethod.HiZ)
			{
				data.CameraDepthPyramidRT = builder.ReadTexture(in data.Resources.CameraDepthPyramidRT);
			}
			TextureHandle input = data.Resources.CameraHistoryColorBuffer;
			data.CameraHistoryColorRT = builder.ReadTexture(in input);
			data.CameraNormalsRT = builder.ReadTexture(in data.Resources.CameraNormalsRT);
			data.CameraTranslucencyRT = builder.ReadTexture(in data.Resources.CameraTranslucencyRT);
			data.SsrShader = m_SsrShader;
			data.TraceHiZKernel = m_TraceHiZKernel;
			data.TraceScreenSpaceKernel = m_TraceScreenSpaceKernel;
			data.SsrResolveMaterial = m_SsrResolveMaterial;
			data.SsrResolvePass = m_SsrResolvePass;
			data.SsrBlurPass = m_SsrBlurPass;
			data.SsrCompositeSsrPass = m_SsrCompositeSsrPass;
			data.BlitMaterial = m_BlitMaterial;
			data.Camera = renderingData.CameraData.Camera;
			data.SsrScreenSize = new Vector4(desc.width, desc.height, 1f / (float)desc.width, 1f / (float)desc.height);
			data.TextureSize = new int2(@int.x, @int.y);
			data.ScreenSize = new int2(renderingData.CameraData.CameraTargetDescriptor.width, renderingData.CameraData.CameraTargetDescriptor.height);
			data.HighlightSupression = m_Settings.HighlightSupression.value;
			data.UseMotionVectorsForReprojection = m_Settings.UseMotionVectorsForReprojection.value;
			if (data.UseMotionVectorsForReprojection)
			{
				data.MotionVectorsRT = builder.ReadTexture(in data.Resources.CameraMotionVectorsRT);
			}
			data.BlurEnabled = m_Settings.BlurEnabled.value;
			Vector4 vector = m_Settings.RoughnessRemap.value;
			float4 @float = 1f - (float4)vector;
			@float.xy = @float.yx;
			@float.z = @float.y - @float.x;
			data.RoughnessRemap = @float;
			data.MaxRaySteps = m_Settings.MaxRaySteps.value;
			float nearClipPlane = data.Camera.nearClipPlane;
			float farClipPlane = data.Camera.farClipPlane;
			float value = m_Settings.ObjectThickness.value;
			data.ThicknessScale = 1f / (1f + value);
			data.ThicknessBias = (0f - nearClipPlane) / (farClipPlane - nearClipPlane) * (value * data.ThicknessScale);
			data.ObjectThickness = value;
			data.MaxRoughness = m_Settings.MaxRoughness.value;
			data.MaxDistance = m_Settings.MaxDistance.value;
			data.ScreenSpaceStepSize = m_Settings.ScreenSpaceStepSize.value;
			data.FresnelPower = m_Settings.FresnelPower.value;
			data.RoughnessFadeStart = m_Settings.RoughnessFadeStart.value;
		}
	}

	protected override void Render(ScreenSpaceReflectionsPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.SsrHitPointRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetComputeIntParam(data.SsrShader, ShaderConstants._SsrIterLimit, data.MaxRaySteps);
		context.cmd.SetComputeFloatParam(data.SsrShader, ShaderConstants._SsrThicknessScale, data.ThicknessScale);
		context.cmd.SetComputeFloatParam(data.SsrShader, ShaderConstants._SsrThicknessBias, data.ThicknessBias);
		context.cmd.SetComputeIntParam(data.SsrShader, ShaderConstants._SsrMinDepthMipLevel, data.MinDepthLevel);
		context.cmd.SetComputeVectorParam(data.SsrShader, ShaderConstants._SsrScreenSize, data.SsrScreenSize);
		context.cmd.SetComputeFloatParam(data.SsrShader, ShaderConstants._SsrRoughnessFadeEnd, data.MaxRoughness);
		context.cmd.SetComputeFloatParam(data.SsrShader, ShaderConstants._MaxRayTraceDistance, data.MaxDistance);
		if (m_Settings.TracingMethod.value == TracingMethod.HiZ)
		{
			context.cmd.SetComputeTextureParam(data.SsrShader, data.TraceHiZKernel, ShaderPropertyId._CameraDepthPyramidRT, data.CameraDepthPyramidRT);
			context.cmd.SetComputeTextureParam(data.SsrShader, data.TraceHiZKernel, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
			context.cmd.SetComputeVectorParam(data.SsrShader, ShaderConstants._SsrHitPointTextureSize, data.SsrScreenSize);
			context.cmd.DispatchCompute(data.SsrShader, data.TraceHiZKernel, RenderingUtils.DivRoundUp((int)data.SsrScreenSize.x, 8), RenderingUtils.DivRoundUp((int)data.SsrScreenSize.y, 8), 1);
		}
		else
		{
			Camera camera = data.Camera;
			float num = data.ScreenSize.x;
			float num2 = data.ScreenSize.y;
			float num3 = num / 2f;
			float num4 = num2 / 2f;
			Matrix4x4 projectionMatrix = camera.projectionMatrix;
			Vector4 val = new Vector4(-2f / (num * projectionMatrix[0]), -2f / (num2 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]);
			Matrix4x4 matrix4x = default(Matrix4x4);
			matrix4x.SetRow(0, new Vector4(num3, 0f, 0f, num3));
			matrix4x.SetRow(1, new Vector4(0f, num4, 0f, num4));
			matrix4x.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
			matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 val2 = matrix4x * projectionMatrix;
			Vector3 vector = (float.IsPositiveInfinity(camera.farClipPlane) ? new Vector3(camera.nearClipPlane, -1f, 1f) : new Vector3(camera.nearClipPlane * camera.farClipPlane, camera.nearClipPlane - camera.farClipPlane, camera.farClipPlane));
			context.cmd.SetComputeMatrixParam(data.SsrShader, ShaderConstants._ProjectToPixelMatrix, val2);
			context.cmd.SetComputeMatrixParam(data.SsrShader, ShaderConstants._WorldToCameraMatrix, camera.worldToCameraMatrix);
			context.cmd.SetComputeVectorParam(data.SsrShader, ShaderConstants._ProjInfo, val);
			context.cmd.SetComputeVectorParam(data.SsrShader, ShaderConstants._CameraClipInfo, vector);
			context.cmd.SetComputeFloatParam(data.SsrShader, ShaderConstants._LayerThickness, data.ObjectThickness);
			context.cmd.SetComputeFloatParam(data.SsrShader, ShaderConstants._RayStepSize, data.ScreenSpaceStepSize);
			context.cmd.SetComputeTextureParam(data.SsrShader, data.TraceScreenSpaceKernel, ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
			context.cmd.SetComputeVectorParam(data.SsrShader, ShaderConstants._SsrHitPointTextureSize, data.SsrScreenSize);
			context.cmd.DispatchCompute(data.SsrShader, data.TraceScreenSpaceKernel, RenderingUtils.DivRoundUp((int)data.SsrScreenSize.x, 8), RenderingUtils.DivRoundUp((int)data.SsrScreenSize.y, 8), 1);
		}
		context.cmd.SetGlobalTexture(ShaderConstants._SsrHitPointTexture, data.SsrHitPointRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorPyramidRT, data.CameraHistoryColorRT);
		context.cmd.SetGlobalFloat(ShaderConstants._UseMotionVectorsForReprojection, data.UseMotionVectorsForReprojection ? 1 : 0);
		if (data.UseMotionVectorsForReprojection)
		{
			context.cmd.SetGlobalTexture(ShaderConstants._MotionVectorsTex, data.MotionVectorsRT);
		}
		context.cmd.SetGlobalFloat(ShaderConstants._SsrFresnelPower, data.FresnelPower);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrRoughnessFadeEnd, data.MaxRoughness);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrRoughnessFadeStart, data.RoughnessFadeStart);
		context.cmd.SetRenderTarget(data.SsrRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrResolveMaterial, data.SsrResolvePass, MeshTopology.Triangles, 3);
		int num5 = 0;
		int num6 = data.TextureSize.x;
		int num7 = data.TextureSize.y;
		int num8 = num6 / 2;
		int num9 = num7 / 2;
		Vector4 value = new Vector4(1f, 1f, 0f, 0f);
		context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, data.SsrRT);
		context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
		context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
		context.cmd.SetGlobalFloat(ShaderConstants._HighlightSuppression, data.HighlightSupression ? 1 : 0);
		context.cmd.SetRenderTarget(data.SsrPyramidRT, 0);
		context.cmd.SetViewport(new Rect(0f, 0f, num6, num7));
		context.cmd.DrawProcedural(Matrix4x4.identity, data.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
		if (data.BlurEnabled)
		{
			while (num6 >= 8 && num7 >= 8)
			{
				int num10 = math.max(1, num6 >> 1);
				int num11 = math.max(1, num7 >> 1);
				context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.SsrPyramidRT);
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(1f, 1f, 0f, 0f));
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(1f, 1f, 1f / (float)num6, 0f));
				context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, num5);
				context.cmd.SetRenderTarget(data.TempRT0, 0);
				context.cmd.SetViewport(new Rect(0f, 0f, num10, num11));
				context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrResolveMaterial, data.SsrBlurPass, MeshTopology.Triangles, 3, 1);
				float x = (float)num10 / (float)num8;
				float y = (float)num11 / (float)num9;
				context.cmd.SetGlobalTexture(ShaderPropertyId._Source, data.TempRT0);
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcScaleBias, new Vector4(x, y, 0f, 0f));
				context.cmd.SetGlobalVector(ShaderPropertyId._SrcUvLimits, new Vector4(((float)num10 - 0.5f) / (float)num8, ((float)num11 - 0.5f) / (float)num9, 0f, 1f / (float)num9));
				context.cmd.SetGlobalFloat(ShaderPropertyId._SourceMip, 0f);
				context.cmd.SetRenderTarget(data.SsrPyramidRT, num5 + 1);
				context.cmd.SetViewport(new Rect(0f, 0f, num10, num11));
				context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrResolveMaterial, data.SsrBlurPass, MeshTopology.Triangles, 3, 1);
				num5++;
				num6 >>= 1;
				num7 >>= 1;
				if (num5 > 3)
				{
					break;
				}
			}
		}
		context.cmd.SetGlobalFloat(ShaderConstants._SsrPyramidLodCount, num5);
		context.cmd.SetGlobalTexture(ShaderConstants._SsrPyramidTexture, data.SsrPyramidRT);
		context.cmd.SetRenderTarget(data.SsrRT);
		context.cmd.SetGlobalFloat(ShaderConstants._SsrBlruEnabled, data.BlurEnabled ? 1 : 0);
		context.cmd.SetGlobalVector(ShaderConstants._SsrRoughnessRemap, data.RoughnessRemap);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.SsrResolveMaterial, data.SsrCompositeSsrPass, MeshTopology.Triangles, 3);
	}
}
