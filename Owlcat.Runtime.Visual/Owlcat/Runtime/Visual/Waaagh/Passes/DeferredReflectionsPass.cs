using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.BilateralUpsample;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredReflectionsPass : ScriptableRenderPass<DeferredReflectionsPassData>
{
	private Material m_DeferredReflectionsMaterial;

	private ComputeShader m_BilateralUpsampleCs;

	private ComputeShaderKernelDescriptor m_BilateralUpSampleColorKernel;

	public override string Name => "DeferredReflectionsPass";

	public DeferredReflectionsPass(RenderPassEvent evt, Material deferredReflectionsMaterial, ComputeShader bilateralUpsampleCs)
		: base(evt)
	{
		m_DeferredReflectionsMaterial = deferredReflectionsMaterial;
		m_BilateralUpsampleCs = bilateralUpsampleCs;
		m_BilateralUpSampleColorKernel = m_BilateralUpsampleCs.GetKernelDescriptor("BilateralUpSampleColor4");
	}

	protected unsafe override void Setup(RenderGraphBuilder builder, DeferredReflectionsPassData data, ref RenderingData renderingData)
	{
		data.CameraColorRT = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraDeferredReflectionsRT", renderingData.CameraData.CameraTargetDescriptor);
		desc.depthBufferBits = DepthBits.None;
		desc.colorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		data.CameraDeferredReflectionsRT = builder.CreateTransientTexture(in desc);
		data.CameraDepthRT = builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Read);
		data.CameraDepthCopytRT = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		data.CameraNormalsRT = builder.ReadTexture(in data.Resources.CameraNormalsRT);
		data.CameraTranslucencyRT = builder.ReadTexture(in data.Resources.CameraTranslucencyRT);
		data.SsrEnabled = renderingData.CameraData.IsSSREnabled;
		if (data.SsrEnabled)
		{
			data.SsrRT = builder.ReadTexture(in data.Resources.SsrRT);
			ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
			if (component == null)
			{
				data.SsrNeedUpsamplePass = false;
			}
			else
			{
				data.SsrNeedUpsamplePass = component.UseUpsamplePass.value && component.Quality.value == ScreenSpaceReflectionsQuality.Half;
			}
			if (data.SsrNeedUpsamplePass)
			{
				data.BilateralUpsampleCS = m_BilateralUpsampleCs;
				data.BilateralUpSampleColorKernel = m_BilateralUpSampleColorKernel;
				TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrUpsampledRT", renderingData.CameraData.CameraTargetDescriptor);
				desc2.depthBufferBits = DepthBits.None;
				desc2.filterMode = FilterMode.Bilinear;
				desc2.wrapMode = TextureWrapMode.Clamp;
				desc2.width = renderingData.CameraData.CameraTargetDescriptor.width;
				desc2.height = renderingData.CameraData.CameraTargetDescriptor.height;
				desc2.enableRandomWrite = true;
				if (component.ColorPrecision.value == ColorPrecision.Color32)
				{
					desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
				}
				else
				{
					desc2.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
				}
				data.SsrUpsampledRT = builder.CreateTransientTexture(in desc2);
				data.ShaderVariablesBilateralUpsample._HalfScreenSize = new Vector4(desc2.width / 2, desc2.height / 2, 1f / ((float)desc2.width * 0.5f), 1f / ((float)desc2.height * 0.5f));
				for (int i = 0; i < 16; i++)
				{
					data.ShaderVariablesBilateralUpsample._DistanceBasedWeights[i] = Owlcat.Runtime.Visual.Waaagh.BilateralUpsample.BilateralUpsample.DistanceBasedWeights_2x2[i];
				}
				for (int j = 0; j < 32; j++)
				{
					data.ShaderVariablesBilateralUpsample._TapOffsets[j] = Owlcat.Runtime.Visual.Waaagh.BilateralUpsample.BilateralUpsample.TapOffsets_2x2[j];
				}
				data.UpsampledSize = new int2(desc2.width, desc2.height);
			}
		}
		data.VisibleReflectionProbes = renderingData.CullingResults.visibleReflectionProbes;
		data.DeferredReflectionsMaterial = m_DeferredReflectionsMaterial;
		data.ActiveColorSpace = QualitySettings.activeColorSpace;
		builder.DependsOn(in data.Resources.RendererLists.OpaqueGBuffer.List);
		builder.AllowRendererListCulling(value: true);
	}

	protected override void Render(DeferredReflectionsPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.CameraDepthCopytRT);
		context.cmd.SetRenderTarget(data.CameraDeferredReflectionsRT, data.CameraDepthRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._SpecCube0, ReflectionProbe.defaultTexture);
		context.cmd.SetGlobalVector(ShaderPropertyId._SpecCube0_HDR, ReflectionProbe.defaultTextureHDRDecodeValues);
		context.cmd.SetGlobalFloat(ShaderPropertyId._UseBoxProjection, 0f);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredReflectionsMaterial, 0, MeshTopology.Triangles, 3);
		NativeArray<VisibleReflectionProbe> visibleReflectionProbes = data.VisibleReflectionProbes;
		for (int i = 0; i < visibleReflectionProbes.Length; i++)
		{
			VisibleReflectionProbe visibleReflectionProbe = visibleReflectionProbes[i];
			if (!(visibleReflectionProbe.reflectionProbe == null))
			{
				Vector3 position = visibleReflectionProbe.reflectionProbe.transform.position;
				Vector4 value = position;
				value.w = visibleReflectionProbe.blendDistance;
				context.cmd.SetGlobalTexture(ShaderPropertyId._SpecCube0, visibleReflectionProbe.texture);
				context.cmd.SetGlobalVector(ShaderPropertyId._SpecCube0_HDR, visibleReflectionProbe.hdrData);
				context.cmd.SetGlobalVector(ShaderPropertyId._SpecCube0_ProbePosition, value);
				context.cmd.SetGlobalVector(ShaderPropertyId._SpecCube0_BoxMin, visibleReflectionProbe.bounds.min);
				context.cmd.SetGlobalVector(ShaderPropertyId._SpecCube0_BoxMax, visibleReflectionProbe.bounds.max);
				context.cmd.SetGlobalFloat(ShaderPropertyId._UseBoxProjection, visibleReflectionProbe.isBoxProjection ? 1 : 0);
				Matrix4x4 matrix = Matrix4x4.TRS(visibleReflectionProbe.center + position, Quaternion.identity, visibleReflectionProbe.bounds.size);
				context.cmd.DrawMesh(RenderingUtils.CubeMesh, matrix, data.DeferredReflectionsMaterial, 0, 1);
			}
		}
		if (data.SsrEnabled)
		{
			if (data.SsrNeedUpsamplePass)
			{
				if (FrameDebugger.enabled)
				{
					context.cmd.SetRenderTarget(data.SsrUpsampledRT);
				}
				int3 dispatchSize = data.BilateralUpSampleColorKernel.GetDispatchSize(data.UpsampledSize.x, data.UpsampledSize.y, 1);
				ConstantBuffer.PushGlobal(context.cmd, in data.ShaderVariablesBilateralUpsample, ShaderPropertyId.ShaderVariablesBilateralUpsample);
				context.cmd.SetComputeTextureParam(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, ShaderPropertyId._DepthTexture, data.CameraDepthCopytRT);
				context.cmd.SetComputeTextureParam(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, ShaderPropertyId._LowResolutionTexture, data.SsrRT);
				context.cmd.SetComputeTextureParam(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, ShaderPropertyId._OutputUpscaledTexture, data.SsrUpsampledRT);
				context.cmd.DispatchCompute(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
				if (FrameDebugger.enabled)
				{
					context.cmd.SetRenderTarget(data.CameraDeferredReflectionsRT, data.CameraDepthRT);
				}
				context.cmd.SetGlobalTexture(ShaderPropertyId._SsrRT, data.SsrUpsampledRT);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredReflectionsMaterial, 2, MeshTopology.Triangles, 3);
			}
			else
			{
				context.cmd.SetGlobalTexture(ShaderPropertyId._SsrRT, data.SsrRT);
				context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredReflectionsMaterial, 2, MeshTopology.Triangles, 3);
			}
		}
		if (data.ActiveColorSpace != ColorSpace.Linear)
		{
			context.cmd.SetRenderTarget(data.CameraColorRT, data.CameraDepthRT);
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDeferredReflectionsRT, data.CameraDeferredReflectionsRT);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredReflectionsMaterial, 3, MeshTopology.Triangles, 3);
			return;
		}
		context.cmd.SetRenderTarget(data.CameraDeferredReflectionsRT, data.CameraDepthRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorRT, data.CameraColorRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredReflectionsMaterial, 4, MeshTopology.Triangles, 3);
		context.cmd.SetRenderTarget(data.CameraColorRT, data.CameraDepthRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDeferredReflectionsRT, data.CameraDeferredReflectionsRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredReflectionsMaterial, 5, MeshTopology.Triangles, 3);
	}
}
