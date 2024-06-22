using System.Collections.Generic;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Highlighting.Passes;

public class HighlighterPass : ScriptableRenderPass
{
	[BurstCompile]
	private struct CullingJob : IJobParallelFor
	{
		public NativeArray<HighlightingFeature.BoundsVisibility> Bounds;

		[ReadOnly]
		public NativeArray<Plane> CameraPlanes;

		public void Execute(int index)
		{
			HighlightingFeature.BoundsVisibility value = Bounds[index];
			value.Visibility = TestPlanesAABBInternalFast(ref CameraPlanes, ref value.Bounds);
			Bounds[index] = value;
		}

		public static HighlightingFeature.TestPlanesResults TestPlanesAABBInternalFast(ref NativeArray<Plane> planes, ref Bounds bounds)
		{
			Vector3 boundsMin = bounds.min;
			Vector3 boundsMax = bounds.max;
			return TestPlanesAABBInternalFast(ref planes, ref boundsMin, ref boundsMax);
		}

		public static HighlightingFeature.TestPlanesResults TestPlanesAABBInternalFast(ref NativeArray<Plane> planes, ref Vector3 boundsMin, ref Vector3 boundsMax, bool testIntersection = false)
		{
			HighlightingFeature.TestPlanesResults result = HighlightingFeature.TestPlanesResults.Inside;
			Vector3 vector = default(Vector3);
			Vector3 vector2 = default(Vector3);
			for (int i = 0; i < planes.Length; i++)
			{
				Vector3 normal = planes[i].normal;
				float distance = planes[i].distance;
				if (normal.x < 0f)
				{
					vector.x = boundsMin.x;
					vector2.x = boundsMax.x;
				}
				else
				{
					vector.x = boundsMax.x;
					vector2.x = boundsMin.x;
				}
				if (normal.y < 0f)
				{
					vector.y = boundsMin.y;
					vector2.y = boundsMax.y;
				}
				else
				{
					vector.y = boundsMax.y;
					vector2.y = boundsMin.y;
				}
				if (normal.z < 0f)
				{
					vector.z = boundsMin.z;
					vector2.z = boundsMax.z;
				}
				else
				{
					vector.z = boundsMax.z;
					vector2.z = boundsMin.z;
				}
				if (normal.x * vector.x + normal.y * vector.y + normal.z * vector.z + distance < 0f)
				{
					return HighlightingFeature.TestPlanesResults.Outside;
				}
				if (testIntersection && normal.x * vector2.x + normal.y * vector2.y + normal.z * vector2.z + distance <= 0f)
				{
					result = HighlightingFeature.TestPlanesResults.Intersect;
				}
			}
			return result;
		}
	}

	private const string kProfilerTag = "Highlighter Pass";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Highlighter Pass");

	private List<Renderer> m_Renderers = new List<Renderer>();

	private Dictionary<Renderer, Highlighter> m_RendererHighlighterMap = new Dictionary<Renderer, Highlighter>();

	private Plane[] m_CameraPlanesTemp = new Plane[6];

	private RenderTargetHandle m_DepthAttachment;

	private RenderTargetHandle m_ColorAttachment;

	private RenderTargetHandle m_HighlightTarget;

	private RenderTargetHandle m_Blur1;

	private RenderTargetHandle m_Blur2;

	private Material m_HighlighterMaterial;

	private Material m_BlurMaterial;

	private Material m_CutMaterial;

	private Material m_CompositeMaterial;

	private HighlightingFeature m_Feature;

	private JobHandle m_JobHandle;

	private CullingJob m_Job;

	public HighlighterPass(RenderPassEvent evt, HighlightingFeature feature, Material highlighterMaterial, Material blurMaterial, Material cutMaterial, Material compositeMaterial)
	{
		base.RenderPassEvent = evt;
		m_Feature = feature;
		m_HighlighterMaterial = highlighterMaterial;
		m_BlurMaterial = blurMaterial;
		m_CutMaterial = cutMaterial;
		m_CompositeMaterial = compositeMaterial;
		m_HighlightTarget.Init("_HighlightRT");
		m_Blur1.Init("_HighlightBlur1");
		m_Blur2.Init("_HighlightBlur2");
		m_Job = default(CullingJob);
	}

	public void Setup(Camera camera, RenderTargetHandle colorAttachment, RenderTargetHandle depthAttachment)
	{
		m_ColorAttachment = colorAttachment;
		m_DepthAttachment = depthAttachment;
		m_Renderers.Clear();
		m_RendererHighlighterMap.Clear();
		int count = m_Renderers.Count;
		if (ObjectRegistry<Highlighter>.Instance != null)
		{
			foreach (Highlighter item in ObjectRegistry<Highlighter>.Instance)
			{
				List<Highlighter.RendererInfo> rendererInfos = item.GetRendererInfos();
				if (rendererInfos == null)
				{
					continue;
				}
				foreach (Highlighter.RendererInfo item2 in rendererInfos)
				{
					m_Renderers.Add(item2.renderer);
				}
				if (m_Renderers.Count > count)
				{
					item.UpdateColors();
					for (int i = count; i < m_Renderers.Count; i++)
					{
						m_RendererHighlighterMap[m_Renderers[i]] = item;
					}
				}
				count = m_Renderers.Count;
			}
		}
		if (!m_Feature.Bounds.IsCreated || m_Feature.Bounds.Length < m_Renderers.Count)
		{
			if (m_Feature.Bounds.IsCreated)
			{
				m_Feature.Bounds.Dispose();
			}
			m_Feature.Bounds = new NativeArray<HighlightingFeature.BoundsVisibility>((int)((float)m_Renderers.Count * 1.5f), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		NativeArray<Plane> cameraPlanes = m_Feature.CameraPlanes;
		for (int j = 0; j < m_Renderers.Count; j++)
		{
			if ((bool)m_Renderers[j])
			{
				m_Feature.Bounds[j] = new HighlightingFeature.BoundsVisibility
				{
					Bounds = m_Renderers[j].bounds,
					Visibility = HighlightingFeature.TestPlanesResults.Outside
				};
			}
		}
		GeometryUtility.CalculateFrustumPlanes(camera, m_CameraPlanesTemp);
		cameraPlanes.CopyFrom(m_CameraPlanesTemp);
		m_Job.Bounds = m_Feature.Bounds;
		m_Job.CameraPlanes = cameraPlanes;
		m_JobHandle = IJobParallelForExtensions.Schedule(m_Job, m_Renderers.Count, 4);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_JobHandle.Complete();
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.CameraData.CameraTargetDescriptor;
			cameraTargetDescriptor.colorFormat = RenderTextureFormat.ARGB32;
			cameraTargetDescriptor.depthBufferBits = ((!m_Feature.ZTestEnabled) ? 24 : 0);
			cameraTargetDescriptor.sRGB = false;
			commandBuffer.GetTemporaryRT(m_HighlightTarget.Id, cameraTargetDescriptor, FilterMode.Bilinear);
			RenderTargetIdentifier depth = (m_Feature.ZTestEnabled ? m_DepthAttachment.Identifier() : m_HighlightTarget.Identifier());
			commandBuffer.SetRenderTarget(m_HighlightTarget.Identifier(), depth);
			commandBuffer.ClearRenderTarget(!m_Feature.ZTestEnabled, clearColor: true, new Color(0f, 0f, 0f, 0f));
			commandBuffer.SetGlobalFloat(HighlightConstantBuffer._ZTest, m_Feature.ZTestEnabled ? 3 : 4);
			commandBuffer.SetGlobalFloat(HighlightConstantBuffer._ZWrite, (!m_Feature.ZTestEnabled) ? 1 : 0);
			for (int i = 0; i < m_Renderers.Count; i++)
			{
				if (m_Job.Bounds[i].Visibility == HighlightingFeature.TestPlanesResults.Outside)
				{
					continue;
				}
				Renderer renderer = m_Renderers[i];
				if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
				{
					continue;
				}
				Highlighter highlighter = m_RendererHighlighterMap[renderer];
				commandBuffer.SetGlobalColor(HighlightConstantBuffer._Color, highlighter.CurrentColor);
				Material[] sharedMaterials = renderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material material = sharedMaterials[j];
					if (!(material != null))
					{
						continue;
					}
					if (!m_Feature.ZTestEnabled)
					{
						bool state = false;
						Texture texture = null;
						float value = 0f;
						CullMode cullMode = CullMode.Back;
						if (material.HasProperty(HighlightConstantBuffer._Alphatest))
						{
							state = material.GetFloat(HighlightConstantBuffer._Alphatest) > 0f;
						}
						if (material.shader != null && material.shader.name == "Owlcat/Particles")
						{
							state = true;
						}
						if (material.HasProperty(HighlightConstantBuffer._CullMode))
						{
							cullMode = (CullMode)material.GetFloat(HighlightConstantBuffer._CullMode);
						}
						if (material.HasProperty(HighlightConstantBuffer._BaseMap))
						{
							texture = material.GetTexture(HighlightConstantBuffer._BaseMap);
						}
						if (material.HasProperty(HighlightConstantBuffer._Cutoff))
						{
							value = material.GetFloat(HighlightConstantBuffer._Cutoff);
						}
						commandBuffer.SetGlobalFloat(HighlightConstantBuffer._CullMode, (float)cullMode);
						commandBuffer.SetGlobalTexture(HighlightConstantBuffer._BaseMap, texture);
						commandBuffer.SetGlobalFloat(HighlightConstantBuffer._Cutoff, value);
						CoreUtils.SetKeyword(commandBuffer, HighlightKeywords._ALPHATEST_ON, state);
						bool flag = material.IsKeywordEnabled(HighlightKeywords.VAT_ENABLED);
						CoreUtils.SetKeyword(commandBuffer, HighlightKeywords.VAT_ENABLED, flag);
						if (flag)
						{
							bool flag2 = material.IsKeywordEnabled(HighlightKeywords._VAT_ROTATIONMAP);
							CoreUtils.SetKeyword(commandBuffer, HighlightKeywords._VAT_ROTATIONMAP, flag2);
							if (flag2)
							{
								SetTexture(HighlightConstantBuffer._RotVatMap, commandBuffer, material);
							}
							SetTexture(HighlightConstantBuffer._PosVatMap, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatCurrentFrame, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatLerp, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatNumOfFrames, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatPivMax, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatPivMin, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatPosMax, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatPosMin, commandBuffer, material);
							SetFloat(HighlightConstantBuffer._VatType, commandBuffer, material);
						}
						bool state2 = material.IsKeywordEnabled(HighlightKeywords.PBD_MESH);
						CoreUtils.SetKeyword(commandBuffer, HighlightKeywords.PBD_MESH, state2);
						bool state3 = material.IsKeywordEnabled(HighlightKeywords.PBD_SKINNING);
						CoreUtils.SetKeyword(commandBuffer, HighlightKeywords.PBD_SKINNING, state3);
					}
					else
					{
						CoreUtils.SetKeyword(commandBuffer, HighlightKeywords._ALPHATEST_ON, state: false);
					}
					commandBuffer.DrawRenderer(renderer, m_HighlighterMaterial, j, 0);
				}
			}
			int num = Mathf.Max(1, (int)m_Feature.DownsampleFactor);
			RenderTextureDescriptor desc = cameraTargetDescriptor;
			desc.width = cameraTargetDescriptor.width / num;
			desc.height = cameraTargetDescriptor.height / num;
			commandBuffer.GetTemporaryRT(m_Blur1.Id, desc, FilterMode.Bilinear);
			commandBuffer.GetTemporaryRT(m_Blur2.Id, desc, FilterMode.Bilinear);
			commandBuffer.Blit(m_HighlightTarget.Identifier(), m_Blur1.Identifier());
			CoreUtils.SetKeyword(m_BlurMaterial, HighlightKeywords.STRAIGHT_DIRECTIONS, m_Feature.BlurDirectons == HighlightingFeature.BlurDirections.Straight);
			CoreUtils.SetKeyword(m_BlurMaterial, HighlightKeywords.ALL_DIRECTIONS, m_Feature.BlurDirectons == HighlightingFeature.BlurDirections.All);
			bool flag3 = true;
			for (int k = 0; k < m_Feature.BlurIterations; k++)
			{
				float value2 = m_Feature.BlurMinSpread + m_Feature.BlurSpread * (float)k;
				commandBuffer.SetGlobalFloat(HighlightConstantBuffer._HighlightingBlurOffset, value2);
				if (flag3)
				{
					commandBuffer.Blit(m_Blur1.Identifier(), m_Blur2.Identifier(), m_BlurMaterial);
				}
				else
				{
					commandBuffer.Blit(m_Blur2.Identifier(), m_Blur1.Identifier(), m_BlurMaterial);
				}
				flag3 = !flag3;
			}
			commandBuffer.Blit(flag3 ? m_Blur1.Identifier() : m_Blur2.Identifier(), m_HighlightTarget.Identifier(), m_CutMaterial);
			commandBuffer.Blit(m_HighlightTarget.Identifier(), m_ColorAttachment.Identifier(), m_CompositeMaterial);
			commandBuffer.ReleaseTemporaryRT(m_Blur1.Id);
			commandBuffer.ReleaseTemporaryRT(m_Blur2.Id);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	private void SetFloat(int property, CommandBuffer cmd, Material mat)
	{
		if (mat.HasProperty(property))
		{
			cmd.SetGlobalFloat(property, mat.GetFloat(property));
		}
	}

	private void SetTexture(int property, CommandBuffer cmd, Material mat)
	{
		if (mat.HasProperty(property))
		{
			cmd.SetGlobalTexture(property, mat.GetTexture(property));
		}
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(m_HighlightTarget.Id);
	}
}
