using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Overrides.HBAO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class ScreenSpaceReflectionsPass : ScriptableRenderPass
{
	private static class ShaderConstants
	{
		public static int _SsrHitPointTexture = Shader.PropertyToID("_SsrHitPointTexture");

		public static int _SsrHitPointTextureSize = Shader.PropertyToID("_SsrHitPointTextureSize");

		public static int _SsrResolveTexture = Shader.PropertyToID("_SsrResolveTexture");

		public static int _SsrIterLimit = Shader.PropertyToID("_SsrIterLimit");

		public static int _SsrThicknessScale = Shader.PropertyToID("_SsrThicknessScale");

		public static int _SsrThicknessBias = Shader.PropertyToID("_SsrThicknessBias");

		public static int _SsrFresnelPower = Shader.PropertyToID("_SsrFresnelPower");

		public static int _SsrConfidenceScale = Shader.PropertyToID("_SsrConfidenceScale");

		public static int _DepthPyramidMipRects = Shader.PropertyToID("_DepthPyramidMipRects");

		public static int _SsrMinDepthMipLevel = Shader.PropertyToID("_SsrMinDepthMipLevel");

		public static int _SsrScreenSize = Shader.PropertyToID("_SsrScreenSize");

		public static int _SsrRoughnessFadeEnd = Shader.PropertyToID("_SsrRoughnessFadeEnd");

		public static int _SsrRoughnessFadeStart = Shader.PropertyToID("_SsrRoughnessFadeStart");

		public static int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static int _StencilComp = Shader.PropertyToID("_StencilComp");

		public static int _ProjectToPixelMatrix = Shader.PropertyToID("_ProjectToPixelMatrix");

		public static int _WorldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");

		public static int _ProjInfo = Shader.PropertyToID("_ProjInfo");

		public static int _CameraClipInfo = Shader.PropertyToID("_CameraClipInfo");

		public static int _MaxRayTraceDistance = Shader.PropertyToID("_MaxRayTraceDistance");

		public static int _LayerThickness = Shader.PropertyToID("_LayerThickness");

		public static int _RayStepSize = Shader.PropertyToID("_RayStepSize");
	}

	private const string kProfilerTag = "Screen Space Reflections";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Screen Space Reflections");

	private Material m_SsrMaterial;

	private Material m_DeferredReflectionsMaterial;

	private ComputeShader m_SsrComputeShader;

	private int m_TraceHiZKernel;

	private int m_TraceScreenSpaceKernel;

	private GBuffer m_GBuffer;

	private RenderTextureDescriptor m_Desc;

	private ScreenSpaceReflections m_Settings;

	private int m_SsrMinDepthMipLevel;

	private bool m_DistortionEnabled;

	public ScreenSpaceReflectionsPass(RenderPassEvent evt, Material ssrMaterial, Material deferredReflectionsMaterial, ComputeShader ssrCompute)
	{
		base.RenderPassEvent = evt;
		m_SsrMaterial = ssrMaterial;
		m_DeferredReflectionsMaterial = deferredReflectionsMaterial;
		m_SsrComputeShader = ssrCompute;
		m_TraceHiZKernel = m_SsrComputeShader.FindKernel("TracingHiZ");
		m_TraceScreenSpaceKernel = m_SsrComputeShader.FindKernel("TracingScreenSpace");
	}

	internal void Setup(GBuffer gBuffer, RenderTextureDescriptor desc, bool distortionEnabled)
	{
		m_GBuffer = gBuffer;
		m_Desc = desc;
		m_DistortionEnabled = distortionEnabled;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_Settings = stack.GetComponent<ScreenSpaceReflections>();
		if (!m_Settings.IsActive())
		{
			return;
		}
		Hbao component = stack.GetComponent<Hbao>();
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			CoreUtils.SetKeyword(commandBuffer, "HBAO_ON", component.IsActive());
			if (m_DistortionEnabled)
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraDeferredReflectionsRT.Identifier(), m_GBuffer.CameraDepthRt.Identifier());
				commandBuffer.SetGlobalFloat(ShaderConstants._StencilRef, 4f);
				commandBuffer.SetGlobalFloat(ShaderConstants._StencilComp, 3f);
				commandBuffer.SetGlobalTexture(DeferredBuffer._SpecCube0, ReflectionProbe.defaultTexture);
				commandBuffer.SetGlobalVector(DeferredBuffer._SpecCube0_HDR, ReflectionProbe.defaultTextureHDRDecodeValues);
				commandBuffer.SetGlobalFloat(DeferredBuffer._UseBoxProjection, 0f);
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_DeferredReflectionsMaterial, 0, MeshTopology.Triangles, 3);
				NativeArray<VisibleReflectionProbe> visibleReflectionProbes = renderingData.CullResults.visibleReflectionProbes;
				for (int i = 0; i < visibleReflectionProbes.Length; i++)
				{
					VisibleReflectionProbe visibleReflectionProbe = visibleReflectionProbes[i];
					if (!(visibleReflectionProbe.reflectionProbe == null))
					{
						Vector3 position = visibleReflectionProbe.reflectionProbe.transform.position;
						Vector4 value = position;
						value.w = visibleReflectionProbe.blendDistance;
						commandBuffer.SetGlobalTexture(DeferredBuffer._SpecCube0, visibleReflectionProbe.texture);
						commandBuffer.SetGlobalVector(DeferredBuffer._SpecCube0_HDR, visibleReflectionProbe.hdrData);
						commandBuffer.SetGlobalVector(DeferredBuffer._SpecCube0_ProbePosition, value);
						commandBuffer.SetGlobalVector(DeferredBuffer._SpecCube0_BoxMin, visibleReflectionProbe.bounds.min);
						commandBuffer.SetGlobalVector(DeferredBuffer._SpecCube0_BoxMax, visibleReflectionProbe.bounds.max);
						commandBuffer.SetGlobalFloat(DeferredBuffer._UseBoxProjection, visibleReflectionProbe.isBoxProjection ? 1 : 0);
						Matrix4x4 matrix = Matrix4x4.TRS(visibleReflectionProbe.center + position, Quaternion.identity, visibleReflectionProbe.bounds.size);
						commandBuffer.DrawMesh(RenderingUtils.CubeMesh, matrix, m_DeferredReflectionsMaterial, 0, 1);
					}
				}
			}
			RenderTextureDescriptor desc = m_Desc;
			desc.enableRandomWrite = true;
			desc.colorFormat = RenderTextureFormat.RGHalf;
			desc.useMipMap = false;
			desc.autoGenerateMips = false;
			desc.depthBufferBits = 0;
			if (m_Settings.Quality.value == ScreenSpaceReflectionsQuality.Half)
			{
				desc.width = m_Desc.width / 2;
				desc.height = m_Desc.height / 2;
				m_SsrMinDepthMipLevel = 1;
			}
			else
			{
				m_SsrMinDepthMipLevel = 0;
			}
			commandBuffer.GetTemporaryRT(ShaderConstants._SsrHitPointTexture, desc);
			commandBuffer.SetRenderTarget(ShaderConstants._SsrHitPointTexture);
			commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
			commandBuffer.SetComputeIntParam(m_SsrComputeShader, ShaderConstants._SsrIterLimit, m_Settings.MaxRaySteps.value);
			float nearClipPlane = renderingData.CameraData.Camera.nearClipPlane;
			float farClipPlane = renderingData.CameraData.Camera.farClipPlane;
			float value2 = m_Settings.ObjectThickness.value;
			float num = 1f / (1f + value2);
			float val = (0f - nearClipPlane) / (farClipPlane - nearClipPlane) * (value2 * num);
			commandBuffer.SetComputeFloatParam(m_SsrComputeShader, ShaderConstants._SsrThicknessScale, num);
			commandBuffer.SetComputeFloatParam(m_SsrComputeShader, ShaderConstants._SsrThicknessBias, val);
			commandBuffer.SetComputeVectorArrayParam(m_SsrComputeShader, ShaderConstants._DepthPyramidMipRects, m_GBuffer.DepthPyramidMipRects);
			commandBuffer.SetComputeIntParam(m_SsrComputeShader, ShaderConstants._SsrMinDepthMipLevel, m_SsrMinDepthMipLevel);
			commandBuffer.SetComputeVectorParam(m_SsrComputeShader, ShaderConstants._SsrScreenSize, new Vector4(desc.width, desc.height, 1f / (float)desc.width, 1f / (float)desc.height));
			commandBuffer.SetComputeFloatParam(m_SsrComputeShader, ShaderConstants._SsrRoughnessFadeEnd, m_Settings.MaxRoughness.value);
			commandBuffer.SetComputeFloatParam(m_SsrComputeShader, ShaderConstants._MaxRayTraceDistance, m_Settings.MaxDistance.value);
			if (m_Settings.TracingMethod.value == TracingMethod.HiZ)
			{
				commandBuffer.SetComputeTextureParam(m_SsrComputeShader, m_TraceHiZKernel, ShaderConstants._SsrHitPointTexture, ShaderConstants._SsrHitPointTexture);
				commandBuffer.SetComputeVectorParam(m_SsrComputeShader, ShaderConstants._SsrHitPointTextureSize, new Vector4(desc.width, desc.height));
				commandBuffer.DispatchCompute(m_SsrComputeShader, m_TraceHiZKernel, RenderingUtils.DivRoundUp(desc.width, 8), RenderingUtils.DivRoundUp(desc.height, 8), 1);
			}
			else
			{
				Camera camera = renderingData.CameraData.Camera;
				float num2 = m_Desc.width;
				float num3 = m_Desc.height;
				float num4 = num2 / 2f;
				float num5 = num3 / 2f;
				Matrix4x4 projectionMatrix = camera.projectionMatrix;
				Vector4 val2 = new Vector4(-2f / (num2 * projectionMatrix[0]), -2f / (num3 * projectionMatrix[5]), (1f - projectionMatrix[2]) / projectionMatrix[0], (1f + projectionMatrix[6]) / projectionMatrix[5]);
				Matrix4x4 matrix4x = default(Matrix4x4);
				matrix4x.SetRow(0, new Vector4(num4, 0f, 0f, num4));
				matrix4x.SetRow(1, new Vector4(0f, num5, 0f, num5));
				matrix4x.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
				matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
				Matrix4x4 val3 = matrix4x * projectionMatrix;
				Vector3 vector = (float.IsPositiveInfinity(camera.farClipPlane) ? new Vector3(camera.nearClipPlane, -1f, 1f) : new Vector3(camera.nearClipPlane * camera.farClipPlane, camera.nearClipPlane - camera.farClipPlane, camera.farClipPlane));
				commandBuffer.SetComputeMatrixParam(m_SsrComputeShader, ShaderConstants._ProjectToPixelMatrix, val3);
				commandBuffer.SetComputeMatrixParam(m_SsrComputeShader, ShaderConstants._WorldToCameraMatrix, camera.worldToCameraMatrix);
				commandBuffer.SetComputeVectorParam(m_SsrComputeShader, ShaderConstants._ProjInfo, val2);
				commandBuffer.SetComputeVectorParam(m_SsrComputeShader, ShaderConstants._CameraClipInfo, vector);
				commandBuffer.SetComputeFloatParam(m_SsrComputeShader, ShaderConstants._LayerThickness, m_Settings.ObjectThickness.value);
				commandBuffer.SetComputeFloatParam(m_SsrComputeShader, ShaderConstants._RayStepSize, m_Settings.ScreenSpaceStepSize.value);
				commandBuffer.SetComputeTextureParam(m_SsrComputeShader, m_TraceScreenSpaceKernel, ShaderConstants._SsrHitPointTexture, ShaderConstants._SsrHitPointTexture);
				commandBuffer.SetComputeVectorParam(m_SsrComputeShader, ShaderConstants._SsrHitPointTextureSize, new Vector4(desc.width, desc.height));
				commandBuffer.DispatchCompute(m_SsrComputeShader, m_TraceScreenSpaceKernel, RenderingUtils.DivRoundUp(desc.width, 8), RenderingUtils.DivRoundUp(desc.height, 8), 1);
			}
			commandBuffer.SetGlobalTexture(ShaderConstants._SsrHitPointTexture, ShaderConstants._SsrHitPointTexture);
			commandBuffer.SetGlobalFloat(ShaderConstants._SsrFresnelPower, m_Settings.FresnelPower.value);
			commandBuffer.SetGlobalFloat(ShaderConstants._SsrConfidenceScale, m_Settings.ConfidenceScale.value);
			commandBuffer.SetGlobalInt(ShaderConstants._SsrMinDepthMipLevel, m_SsrMinDepthMipLevel);
			commandBuffer.SetGlobalFloat(ShaderConstants._SsrRoughnessFadeEnd, m_Settings.MaxRoughness.value);
			commandBuffer.SetGlobalFloat(ShaderConstants._SsrRoughnessFadeStart, m_Settings.RoughnessFadeStart.value);
			if (m_Settings.Quality.value == ScreenSpaceReflectionsQuality.Full || !m_Settings.UseUpsamplePass.value)
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraColorRt.Identifier());
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_SsrMaterial, 0, MeshTopology.Triangles, 3);
			}
			else
			{
				RenderTextureDescriptor desc2 = m_Desc;
				desc2.useMipMap = false;
				desc2.autoGenerateMips = false;
				desc2.depthBufferBits = 0;
				if (m_Settings.Quality.value == ScreenSpaceReflectionsQuality.Half)
				{
					desc2.width = m_Desc.width / 2;
					desc2.height = m_Desc.height / 2;
				}
				commandBuffer.GetTemporaryRT(ShaderConstants._SsrResolveTexture, desc2, FilterMode.Bilinear);
				commandBuffer.SetRenderTarget(ShaderConstants._SsrResolveTexture);
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_SsrMaterial, 1, MeshTopology.Triangles, 3);
				commandBuffer.SetGlobalTexture(ShaderConstants._SsrResolveTexture, ShaderConstants._SsrResolveTexture);
				commandBuffer.SetRenderTarget(m_GBuffer.CameraColorRt.Identifier());
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_SsrMaterial, 2, MeshTopology.Triangles, 3);
			}
			commandBuffer.ReleaseTemporaryRT(ShaderConstants._SsrHitPointTexture);
			commandBuffer.ReleaseTemporaryRT(ShaderConstants._SsrResolveTexture);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
