using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.RenderPipeline.Lighting;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class DeferredLightingPass : ScriptableRenderPass
{
	[Flags]
	private enum KernelAliasesFlags
	{
		None = 0,
		Debug = 1,
		Shadowmask = 2,
		ShadowsSoft = 4,
		ShadowsHard = 8
	}

	public static class ShaderConstants
	{
		public static int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static int _StencilComp = Shader.PropertyToID("_StencilComp");
	}

	private const string kProfilerTag = "Deferred Lighting";

	private static int _LightingResultUAV = Shader.PropertyToID("_LightingResultUAV");

	private static int _LightingResultUAVSize = Shader.PropertyToID("_LightingResultUAVSize");

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Deferred Lighting");

	private bool m_UseCompute;

	private ClusteredLights m_ClusteredLights;

	private Material m_DeferredLightingMaterial;

	private Material m_DeferredReflectionsMaterial;

	private ComputeShader m_DeferredLightingCompute;

	private Dictionary<KernelAliasesFlags, int> m_Kernels;

	private GBuffer m_GBuffer;

	private RenderTextureDescriptor m_Desc;

	private Vector3Int m_ThreadGroupsCount;

	private Vector4 m_UAVSize;

	public DeferredLightingPass(RenderPassEvent evt, Material deferredLightingMaterial, Material deferredReflectionsMaterial, ComputeShader deferredLightingComputeShader, bool useCompute)
	{
		base.RenderPassEvent = evt;
		m_UseCompute = useCompute;
		m_DeferredLightingMaterial = deferredLightingMaterial;
		m_DeferredReflectionsMaterial = deferredReflectionsMaterial;
		m_DeferredLightingCompute = deferredLightingComputeShader;
		m_Kernels = new Dictionary<KernelAliasesFlags, int>();
		KernelAliasesFlags key = KernelAliasesFlags.None;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel("Shade");
		key = KernelAliasesFlags.Debug;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Debug}");
		key = KernelAliasesFlags.Shadowmask;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Shadowmask}");
		key = KernelAliasesFlags.ShadowsSoft;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.ShadowsSoft}");
		key = KernelAliasesFlags.ShadowsHard;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.ShadowsHard}");
		key = KernelAliasesFlags.Debug | KernelAliasesFlags.Shadowmask;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Debug}{KernelAliasesFlags.Shadowmask}");
		key = KernelAliasesFlags.Debug | KernelAliasesFlags.ShadowsSoft;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Debug}{KernelAliasesFlags.ShadowsSoft}");
		key = KernelAliasesFlags.Debug | KernelAliasesFlags.ShadowsHard;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Debug}{KernelAliasesFlags.ShadowsHard}");
		key = KernelAliasesFlags.Debug | KernelAliasesFlags.Shadowmask | KernelAliasesFlags.ShadowsSoft;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Debug}{KernelAliasesFlags.ShadowsSoft}{KernelAliasesFlags.Shadowmask}");
		key = KernelAliasesFlags.Debug | KernelAliasesFlags.Shadowmask | KernelAliasesFlags.ShadowsHard;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.Debug}{KernelAliasesFlags.ShadowsHard}{KernelAliasesFlags.Shadowmask}");
		key = KernelAliasesFlags.Shadowmask | KernelAliasesFlags.ShadowsSoft;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.ShadowsSoft}{KernelAliasesFlags.Shadowmask}");
		key = KernelAliasesFlags.Shadowmask | KernelAliasesFlags.ShadowsHard;
		m_Kernels[key] = m_DeferredLightingCompute.FindKernel($"Shade{KernelAliasesFlags.ShadowsHard}{KernelAliasesFlags.Shadowmask}");
	}

	public void Setup(RenderTextureDescriptor baseDesc, GBuffer gBuffer, ClusteredLights clusteredLights)
	{
		m_Desc = baseDesc;
		m_GBuffer = gBuffer;
		m_ClusteredLights = clusteredLights;
		m_ThreadGroupsCount = new Vector3Int(RenderingUtils.DivRoundUp(m_Desc.width, 16), RenderingUtils.DivRoundUp(m_Desc.height, 16), 1);
		m_UAVSize = new Vector4(m_Desc.width, m_Desc.height);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			if (renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraDeferredReflectionsRT.Identifier());
			}
			else
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraColorPyramidRt.Identifier());
			}
			commandBuffer.SetGlobalFloat(ShaderConstants._StencilRef, 0f);
			commandBuffer.SetGlobalFloat(ShaderConstants._StencilComp, 0f);
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
			commandBuffer.SetGlobalTexture(m_GBuffer.CameraColorRt.Id, m_GBuffer.CameraColorRt.Identifier());
			commandBuffer.DrawProcedural(Matrix4x4.identity, m_DeferredReflectionsMaterial, 2, MeshTopology.Triangles, 3);
			if (renderingData.CameraData.IsScreenSpaceReflectionsEnabled)
			{
				commandBuffer.SetGlobalTexture(m_GBuffer.CameraDeferredReflectionsRT.Id, m_GBuffer.CameraDeferredReflectionsRT.Identifier());
			}
			else
			{
				commandBuffer.SetGlobalTexture(m_GBuffer.CameraDeferredReflectionsRT.Id, m_GBuffer.CameraColorPyramidRt.Identifier());
			}
			if (m_UseCompute)
			{
				int kernel = GetKernel(ref renderingData);
				commandBuffer.SetComputeVectorParam(m_DeferredLightingCompute, _LightingResultUAVSize, m_UAVSize);
				commandBuffer.SetComputeTextureParam(m_DeferredLightingCompute, kernel, _LightingResultUAV, m_GBuffer.CameraColorRt.Identifier());
				commandBuffer.DispatchCompute(m_DeferredLightingCompute, kernel, m_ThreadGroupsCount.x, m_ThreadGroupsCount.y, m_ThreadGroupsCount.z);
			}
			else
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraColorRt.Identifier());
				commandBuffer.DrawProcedural(Matrix4x4.identity, m_DeferredLightingMaterial, 0, MeshTopology.Triangles, 3);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private int GetKernel(ref RenderingData renderingData)
	{
		KernelAliasesFlags kernelAliasesFlags = KernelAliasesFlags.None;
		bool flag = Shader.IsKeywordEnabled(ShaderKeywordStrings.DEBUG_DISPLAY);
		kernelAliasesFlags |= (flag ? KernelAliasesFlags.Debug : KernelAliasesFlags.None);
		kernelAliasesFlags |= (m_ClusteredLights.ShadowmaskEnabled ? KernelAliasesFlags.Shadowmask : KernelAliasesFlags.None);
		bool flag2 = Shader.IsKeywordEnabled(ShaderKeywordStrings.SHADOWS_HARD);
		kernelAliasesFlags |= (flag2 ? KernelAliasesFlags.ShadowsHard : KernelAliasesFlags.None);
		bool flag3 = Shader.IsKeywordEnabled(ShaderKeywordStrings.SHADOWS_SOFT);
		kernelAliasesFlags |= (flag3 ? KernelAliasesFlags.ShadowsSoft : KernelAliasesFlags.None);
		return m_Kernels[kernelAliasesFlags];
	}
}
