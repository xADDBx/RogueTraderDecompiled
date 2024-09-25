using System.Collections.Generic;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

public class IndirectRenderingSystem
{
	private class GPUData
	{
		public IndirectArgs IndirectArgs;

		public List<IndirectInstanceData> InstanceData;

		public bool DynamicDataIsDirty;
	}

	private const string kIndirectDrawOpaqueTag = "Draw Indirect Pass";

	private const string kIndirectCullingPassTag = "Indirect Culling";

	private const string kSelectionHighlight = "DetailsSelectionHighlight";

	private const int kIndirectArgsStride = 20;

	private const int kIndirectArgsStep = 5;

	private const int kInstanceBufferSizeDelta = 1024;

	private const int kMeshArgsStep = 4;

	private ProfilingSampler m_DrawProfilingSampler = new ProfilingSampler("Draw Indirect Pass");

	private ProfilingSampler m_CullingProfilingSampler = new ProfilingSampler("Indirect Culling");

	private ComputeBuffer m_ArgsBuffer;

	private ComputeBuffer m_MeshArgsBuffer;

	private ComputeBuffer m_InstanceDataBuffer;

	private ComputeBuffer m_IsVisibleBuffer;

	private ComputeBuffer m_MeshDataBuffer;

	private ComputeBuffer m_LightProbesBuffer;

	private MaterialPropertyBlock m_MaterialPropertyBlock = new MaterialPropertyBlock();

	private bool m_IsDirty;

	private bool m_ShouldDirtyAfterPBD;

	private ComputeShader m_ShaderCulling;

	private int m_KernelClear;

	private int m_KernelCulling;

	private int m_KernelArgsCopy;

	private Dictionary<IIndirectMesh, GPUData> m_MeshData = new Dictionary<IIndirectMesh, GPUData>();

	private Dictionary<string, string> m_PassNameCache = new Dictionary<string, string>();

	private Dictionary<Material, Material> m_OverdrawMaterialCache = new Dictionary<Material, Material>();

	private List<uint> m_ArgsList = new List<uint>();

	private List<uint> m_MeshArgsList = new List<uint>();

	private List<MeshData> m_DrawCallData = new List<MeshData>();

	private List<Vector3> m_InstancePositions = new List<Vector3>();

	private List<Color> m_InstanceLightProbeColor = new List<Color>();

	private List<SphericalHarmonicsL2> m_LightProbes = new List<SphericalHarmonicsL2>();

	private Vector3[] m_LightProbeEvaluationDir = new Vector3[1]
	{
		new Vector3(0f, 1f, 0f)
	};

	private Color[] m_LightProbeEvaluationResult = new Color[1];

	public Camera DebugCamera;

	public static IndirectRenderingSystem Instance { get; private set; }

	static IndirectRenderingSystem()
	{
		Instance = new IndirectRenderingSystem();
	}

	private IndirectRenderingSystem()
	{
	}

	public void Initialize(ComputeShader cullingShader)
	{
		m_ShaderCulling = cullingShader;
		m_KernelClear = m_ShaderCulling.FindKernel("Clear");
		m_KernelCulling = m_ShaderCulling.FindKernel("Culling");
		m_KernelArgsCopy = m_ShaderCulling.FindKernel("ArgsCopy");
		LightProbes.tetrahedralizationCompleted += OnTetrahedralizationCompleted;
	}

	public void Dispose()
	{
		if (m_ArgsBuffer != null)
		{
			m_ArgsBuffer.Release();
		}
		if (m_MeshArgsBuffer != null)
		{
			m_MeshArgsBuffer.Release();
		}
		if (m_InstanceDataBuffer != null)
		{
			m_InstanceDataBuffer.Release();
		}
		if (m_IsVisibleBuffer != null)
		{
			m_IsVisibleBuffer.Release();
		}
		if (m_MeshDataBuffer != null)
		{
			m_MeshDataBuffer.Release();
		}
		if (m_LightProbesBuffer != null)
		{
			m_LightProbesBuffer.Release();
		}
		m_MeshData.Clear();
		LightProbes.tetrahedralizationCompleted -= OnTetrahedralizationCompleted;
	}

	public static void SetupDummyComputeBufferStubs(CommandBuffer cmd, ComputeBuffer buffer)
	{
		cmd.SetGlobalBuffer(ShaderPropertyId._IndirectInstanceDataBuffer, buffer);
		cmd.SetGlobalBuffer(ShaderPropertyId._LightProbesBuffer, buffer);
		cmd.SetGlobalBuffer(ShaderPropertyId._ArgsBuffer, buffer);
		cmd.SetGlobalBuffer(ShaderPropertyId._IsVisibleBuffer, buffer);
	}

	private void FindAndRegisterMeshes()
	{
		MonoBehaviour[] array = Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (monoBehaviour.enabled && monoBehaviour.gameObject.activeInHierarchy && monoBehaviour is IIndirectMesh indirectMesh)
			{
				RegisterMesh(indirectMesh);
				indirectMesh.UpdateInstances();
			}
		}
	}

	public void RegisterMesh(IIndirectMesh mesh)
	{
		if (mesh.Mesh == null || mesh.Materials.Count == 0 || m_MeshData.ContainsKey(mesh))
		{
			return;
		}
		int subMeshCount = mesh.Mesh.subMeshCount;
		GPUData gPUData = new GPUData();
		gPUData.IndirectArgs = new IndirectArgs(subMeshCount);
		for (int i = 0; i < subMeshCount; i++)
		{
			gPUData.IndirectArgs[i].BaseVertex = mesh.Mesh.GetBaseVertex(i);
			gPUData.IndirectArgs[i].IndexCountPerInstance = mesh.Mesh.GetIndexCount(i);
			gPUData.IndirectArgs[i].StartIndex = mesh.Mesh.GetIndexStart(i);
			gPUData.IndirectArgs[i].InstanceCount = 0u;
			gPUData.IndirectArgs[i].StartInstance = 0u;
		}
		gPUData.InstanceData = new List<IndirectInstanceData>();
		m_MeshData.Add(mesh, gPUData);
		if (mesh.IsDynamic)
		{
			for (int j = 0; j < mesh.MaxDynamicInstances; j++)
			{
				gPUData.InstanceData.Add(new IndirectInstanceData
				{
					hidden = 1u
				});
			}
		}
		m_IsDirty = true;
	}

	public void UnregisterMesh(IIndirectMesh mesh)
	{
		if (m_MeshData.ContainsKey(mesh))
		{
			m_MeshData.Remove(mesh);
			m_IsDirty = true;
		}
	}

	public void SetMeshInstances(IIndirectMesh mesh, IEnumerable<IndirectInstanceData> instances)
	{
		if (m_MeshData.TryGetValue(mesh, out var value))
		{
			value.InstanceData.Clear();
			value.InstanceData.AddRange(instances);
			if (mesh.IsDynamic)
			{
				value.DynamicDataIsDirty = true;
			}
			else
			{
				m_IsDirty = true;
			}
		}
	}

	public List<IndirectInstanceData> GetMeshInstances(IIndirectMesh mesh)
	{
		if (m_MeshData.TryGetValue(mesh, out var value))
		{
			return value.InstanceData;
		}
		return null;
	}

	public void SetMeshDirty(IIndirectMesh mesh)
	{
		if (mesh.IsDynamic)
		{
			if (m_MeshData.TryGetValue(mesh, out var value))
			{
				value.DynamicDataIsDirty = true;
			}
		}
		else
		{
			m_IsDirty = true;
		}
	}

	public void Submit()
	{
		if (!m_IsDirty && m_ShouldDirtyAfterPBD && !PBD.IsSceneInitialization)
		{
			m_IsDirty = true;
			m_ShouldDirtyAfterPBD = false;
		}
		if (m_IsDirty)
		{
			if (HasPBDDependency() && PBD.IsSceneInitialization)
			{
				if (Application.platform != RuntimePlatform.OSXPlayer && Application.platform != 0)
				{
					return;
				}
				m_ShouldDirtyAfterPBD = true;
			}
			m_IsDirty = false;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
			{
				_ = meshDatum.Key.IsDynamic;
				num += meshDatum.Value.InstanceData.Count;
				num2 += meshDatum.Key.Mesh.subMeshCount;
				num3++;
			}
			if (m_ArgsBuffer == null || !m_ArgsBuffer.IsValid() || m_ArgsBuffer.count < num2 * 5)
			{
				if (m_ArgsBuffer != null)
				{
					m_ArgsBuffer.Release();
				}
				if (num2 > 0)
				{
					m_ArgsBuffer = new ComputeBuffer(5 * num2, 4, ComputeBufferType.DrawIndirect);
					m_ArgsBuffer.name = "Indirect Args";
				}
			}
			if (m_MeshArgsBuffer == null || !m_MeshArgsBuffer.IsValid() || m_MeshArgsBuffer.count < num3 * 4)
			{
				if (m_MeshArgsBuffer != null)
				{
					m_MeshArgsBuffer.Release();
				}
				if (m_MeshDataBuffer != null)
				{
					m_MeshDataBuffer.Release();
				}
				if (num3 > 0)
				{
					m_MeshArgsBuffer = new ComputeBuffer(4 * num3, 4, ComputeBufferType.Structured);
					m_MeshArgsBuffer.name = "Indirect Mesh Args Buffer";
					m_MeshDataBuffer = new ComputeBuffer(num3, Marshal.SizeOf(typeof(MeshData)), ComputeBufferType.Structured);
					m_MeshDataBuffer.name = "Indirect DrawCall Data";
				}
			}
			if (m_InstanceDataBuffer == null || !m_InstanceDataBuffer.IsValid() || m_InstanceDataBuffer.count < num)
			{
				if (m_InstanceDataBuffer != null && m_InstanceDataBuffer.IsValid())
				{
					m_InstanceDataBuffer.Release();
				}
				if (m_IsVisibleBuffer != null && m_IsVisibleBuffer.IsValid())
				{
					m_IsVisibleBuffer.Release();
				}
				if (m_LightProbesBuffer != null && m_LightProbesBuffer.IsValid())
				{
					m_LightProbesBuffer.Release();
				}
				if (num > 0)
				{
					int count = Mathf.CeilToInt((float)num / 1024f) * 1024;
					m_InstanceDataBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(IndirectInstanceData)), ComputeBufferType.Structured);
					m_InstanceDataBuffer.name = "Indirect Instance Data";
					m_IsVisibleBuffer = new ComputeBuffer(count, 4, ComputeBufferType.Structured);
					m_IsVisibleBuffer.name = "Indirect Visibility Buffer";
					m_LightProbesBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(Color)), ComputeBufferType.Structured);
					m_LightProbesBuffer.name = "Indirect Light Probe Color Buffer";
				}
			}
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._IndirectInstanceDataBuffer, m_InstanceDataBuffer);
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._LightProbesBuffer, m_LightProbesBuffer);
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
			m_MaterialPropertyBlock.SetBuffer(ShaderPropertyId._IsVisibleBuffer, m_IsVisibleBuffer);
			if (num2 == 0 || num == 0)
			{
				return;
			}
			int num4 = 0;
			uint num5 = 0u;
			uint num6 = 0u;
			m_ArgsList.Clear();
			m_MeshArgsList.Clear();
			m_DrawCallData.Clear();
			foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum2 in m_MeshData)
			{
				List<IndirectInstanceData> instanceData = meshDatum2.Value.InstanceData;
				if (instanceData.Count > 0)
				{
					for (int i = 0; i < instanceData.Count; i++)
					{
						IndirectInstanceData value = instanceData[i];
						value.meshID = num5;
						if (PBD.IsGpu)
						{
							value.physicsDataIndex = GetPhysicsData(meshDatum2.Key, i);
						}
						instanceData[i] = value;
					}
					m_InstanceDataBuffer.SetData(instanceData, 0, num4, instanceData.Count);
				}
				m_MeshArgsList.Add(num6);
				m_MeshArgsList.Add((uint)meshDatum2.Key.Mesh.subMeshCount);
				m_MeshArgsList.Add(0u);
				m_MeshArgsList.Add((uint)num4);
				IndirectArgs indirectArgs = meshDatum2.Value.IndirectArgs;
				for (int j = 0; j < indirectArgs.Entries.Length; j++)
				{
					indirectArgs.Entries[j].StartInstance = (uint)num4;
				}
				num2 = meshDatum2.Key.Mesh.subMeshCount;
				Bounds bounds = meshDatum2.Key.Mesh.bounds;
				MeshData meshData = default(MeshData);
				meshData.aabbMax = bounds.max;
				meshData.aabbMin = bounds.min;
				meshData.unused = default(Vector2);
				MeshData item = meshData;
				m_DrawCallData.Add(item);
				num4 += instanceData.Count;
				m_ArgsList.AddRange(indirectArgs.Args);
				num5++;
				num6 += (uint)meshDatum2.Key.Mesh.subMeshCount;
			}
			m_ArgsBuffer.SetData(m_ArgsList);
			m_MeshDataBuffer.SetData(m_DrawCallData);
			m_MeshArgsBuffer.SetData(m_MeshArgsList);
			return;
		}
		int num7 = 0;
		uint num8 = 0u;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum3 in m_MeshData)
		{
			List<IndirectInstanceData> instanceData2 = meshDatum3.Value.InstanceData;
			if (instanceData2.Count > 0 && meshDatum3.Key.IsDynamic && meshDatum3.Value.DynamicDataIsDirty)
			{
				for (int k = 0; k < instanceData2.Count; k++)
				{
					IndirectInstanceData value2 = instanceData2[k];
					value2.meshID = num8;
					instanceData2[k] = value2;
				}
				m_InstanceDataBuffer.SetData(instanceData2, 0, num7, instanceData2.Count);
				meshDatum3.Value.DynamicDataIsDirty = false;
			}
			num7 += instanceData2.Count;
			num8++;
		}
	}

	private uint GetPhysicsData(IIndirectMesh mesh, int i)
	{
		MonoBehaviour monoBehaviour = mesh as MonoBehaviour;
		if (monoBehaviour != null)
		{
			monoBehaviour.TryGetComponent<PBDGrassBody>(out var component);
			if (component != null && component.Body != null)
			{
				return (uint)(PBD.GetParticlesOffset(component.Body) + i * 2);
			}
		}
		return 0u;
	}

	private bool HasPBDDependency()
	{
		foreach (IIndirectMesh key in m_MeshData.Keys)
		{
			MonoBehaviour monoBehaviour = key as MonoBehaviour;
			if (monoBehaviour != null)
			{
				monoBehaviour.TryGetComponent<PBDGrassBody>(out var component);
				if (component != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Cull(ref ScriptableRenderContext context, Camera camera)
	{
		if (m_InstanceDataBuffer == null || m_ArgsBuffer == null || m_IsVisibleBuffer == null || !m_InstanceDataBuffer.IsValid() || !m_ArgsBuffer.IsValid() || !m_IsVisibleBuffer.IsValid())
		{
			return;
		}
		Camera camera2 = ((DebugCamera != null) ? DebugCamera : camera);
		Matrix4x4 worldToCameraMatrix = camera2.worldToCameraMatrix;
		Matrix4x4 val = camera2.projectionMatrix * worldToCameraMatrix;
		int num = 0;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			num += meshDatum.Value.InstanceData.Count;
		}
		if (num != 0)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_CullingProfilingSampler))
			{
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._MeshCount, m_MeshData.Count);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelClear, ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._ArgsBufferSize, m_ArgsBuffer.count);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelClear, ShaderPropertyId._MeshArgsBuffer, m_MeshArgsBuffer);
				commandBuffer.DispatchCompute(m_ShaderCulling, m_KernelClear, RenderingUtils.DivRoundUp(m_MeshData.Count, 64), 1, 1);
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._TotalInstanceCount, num);
				commandBuffer.SetComputeMatrixParam(m_ShaderCulling, ShaderPropertyId._CamViewProj, val);
				commandBuffer.SetComputeVectorParam(m_ShaderCulling, ShaderPropertyId._CamPosition, camera2.transform.position);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._InstanceDataBuffer, m_InstanceDataBuffer);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._IsVisibleBuffer, m_IsVisibleBuffer);
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._IsVisibleBufferSize, m_IsVisibleBuffer.count);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._MeshDataBuffer, m_MeshDataBuffer);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelCulling, ShaderPropertyId._MeshArgsBuffer, m_MeshArgsBuffer);
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._MeshArgsBufferSize, m_MeshArgsBuffer.count);
				commandBuffer.DispatchCompute(m_ShaderCulling, m_KernelCulling, RenderingUtils.DivRoundUp(num, 64), 1, 1);
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._MeshCount, m_MeshData.Count);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelArgsCopy, ShaderPropertyId._ArgsBuffer, m_ArgsBuffer);
				commandBuffer.SetComputeIntParam(m_ShaderCulling, ShaderPropertyId._ArgsBufferSize, m_ArgsBuffer.count);
				commandBuffer.SetComputeBufferParam(m_ShaderCulling, m_KernelArgsCopy, ShaderPropertyId._MeshArgsBuffer, m_MeshArgsBuffer);
				commandBuffer.DispatchCompute(m_ShaderCulling, m_KernelArgsCopy, RenderingUtils.DivRoundUp(m_MeshData.Count, 64), 1, 1);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}

	private void OnTetrahedralizationCompleted()
	{
		CollectLightProbes();
	}

	public void CollectLightProbes()
	{
		if (m_LightProbesBuffer == null || !m_LightProbesBuffer.IsValid())
		{
			return;
		}
		if (m_IsDirty)
		{
			Debug.LogWarning("IndirectRenderingSystem.CollectLightProbes was invoked while m_IsDirty == true");
		}
		int num = 0;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			if (meshDatum.Value.InstanceData.Count <= 0)
			{
				continue;
			}
			m_InstancePositions.Clear();
			m_LightProbes.Clear();
			m_InstanceLightProbeColor.Clear();
			foreach (IndirectInstanceData instanceDatum in meshDatum.Value.InstanceData)
			{
				List<Vector3> instancePositions = m_InstancePositions;
				Matrix4x4 objectToWorld = instanceDatum.objectToWorld;
				instancePositions.Add(objectToWorld.GetColumn(3));
			}
			LightProbes.CalculateInterpolatedLightAndOcclusionProbes(m_InstancePositions, m_LightProbes, null);
			foreach (SphericalHarmonicsL2 lightProbe in m_LightProbes)
			{
				lightProbe.Evaluate(m_LightProbeEvaluationDir, m_LightProbeEvaluationResult);
				m_InstanceLightProbeColor.Add(m_LightProbeEvaluationResult[0]);
			}
			if (m_LightProbesBuffer.count < m_InstanceLightProbeColor.Count + num || m_InstanceLightProbeColor.Count == 0)
			{
				break;
			}
			m_LightProbesBuffer.SetData(m_InstanceLightProbeColor, 0, num, m_InstanceLightProbeColor.Count);
			num += m_InstanceLightProbeColor.Count;
		}
	}

	public void DrawPass(ref ScriptableRenderContext renderContext, CameraType cameraType, bool isIndirectRenderingEnabled, bool isSceneViewInPrefabEditMode, string passName, RenderQueueRange renderQueueRange, bool debugOverdraw = false)
	{
		if (isIndirectRenderingEnabled && (cameraType == CameraType.Game || cameraType == CameraType.SceneView) && !(cameraType == CameraType.SceneView && isSceneViewInPrefabEditMode))
		{
			if (!m_PassNameCache.TryGetValue(passName, out var value))
			{
				value = "Draw Indirect Pass " + passName;
				m_PassNameCache.Add(passName, value);
			}
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_DrawProfilingSampler))
			{
				DrawPassInternal(commandBuffer, passName, renderQueueRange, debugOverdraw);
			}
			renderContext.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}

	public void DrawPass(CommandBuffer cmd, CameraType cameraType, bool isIndirectRenderingEnabled, bool isSceneViewInPrefabEditMode, string passName, RenderQueueRange renderQueueRange, bool debugOverdraw = false)
	{
		if (isIndirectRenderingEnabled && (cameraType == CameraType.Game || cameraType == CameraType.SceneView) && !(cameraType == CameraType.SceneView && isSceneViewInPrefabEditMode))
		{
			if (!m_PassNameCache.TryGetValue(passName, out var value))
			{
				value = "Draw Indirect Pass " + passName;
				m_PassNameCache.Add(passName, value);
			}
			DrawPassInternal(cmd, passName, renderQueueRange, debugOverdraw);
		}
	}

	private void DrawPassInternal(CommandBuffer cmd, string passName, RenderQueueRange renderQueueRange, bool debugOverdraw)
	{
		if (m_ArgsBuffer == null || !m_ArgsBuffer.IsValid() || m_InstanceDataBuffer == null || !m_InstanceDataBuffer.IsValid())
		{
			return;
		}
		if (m_MeshData.Count > 0)
		{
			RenderingUtils.SetDefaultReflectionProbe(m_MaterialPropertyBlock);
		}
		int num = 0;
		int num2 = 4;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			if (meshDatum.Value.InstanceData.Count == 0 || meshDatum.Key.Materials.Count == 0)
			{
				num += 20 * meshDatum.Key.Mesh.subMeshCount;
				num2 += 5 * meshDatum.Key.Mesh.subMeshCount;
				continue;
			}
			IIndirectMesh key = meshDatum.Key;
			_ = meshDatum.Value;
			int subMeshCount = key.Mesh.subMeshCount;
			bool pBDInitialized = GetPBDInitialized(key);
			for (int i = 0; i < subMeshCount; i++)
			{
				int num3 = i;
				if (num3 > key.Materials.Count - 1)
				{
					num3 = key.Materials.Count - 1;
				}
				Material material = key.Materials[num3];
				if (material != null)
				{
					int num4 = material.FindPass(passName);
					if (num4 > -1 && material.renderQueue >= renderQueueRange.lowerBound && material.renderQueue <= renderQueueRange.upperBound)
					{
						m_MaterialPropertyBlock.SetInt(ShaderPropertyId._ArgsOffset, num2);
						m_MaterialPropertyBlock.SetFloat(ShaderPropertyId._PbdEnabledLocal, pBDInitialized ? 1 : 0);
						float4 @float = math.asfloat(new uint4((uint)meshDatum.Key.RenderingLayerMask));
						m_MaterialPropertyBlock.SetVector(ShaderPropertyId.unity_RenderingLayer, @float);
						cmd.DrawMeshInstancedIndirect(meshDatum.Key.Mesh, i, material, num4, m_ArgsBuffer, num, m_MaterialPropertyBlock);
					}
				}
				num += 20;
				num2 += 5;
			}
		}
	}

	public bool HasOpaqueObjects()
	{
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			foreach (Material material in meshDatum.Key.Materials)
			{
				if (material.HasProperty(ShaderPropertyId._Surface) && material.HasProperty(ShaderPropertyId._DistortionEnabled) && material.GetFloat(ShaderPropertyId._Surface) < 1f && material.GetFloat(ShaderPropertyId._DistortionEnabled) < 1f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasOpaqueDistortion()
	{
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			foreach (Material material in meshDatum.Key.Materials)
			{
				if (material.HasProperty(ShaderPropertyId._Surface) && material.HasProperty(ShaderPropertyId._DistortionEnabled) && material.GetFloat(ShaderPropertyId._Surface) < 1f && material.GetFloat(ShaderPropertyId._DistortionEnabled) > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasTransparentObjects()
	{
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			foreach (Material material in meshDatum.Key.Materials)
			{
				if (material.HasProperty(ShaderPropertyId._Surface) && material.GetFloat(ShaderPropertyId._Surface) > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool GetPBDInitialized(IIndirectMesh mesh)
	{
		MonoBehaviour monoBehaviour = mesh as MonoBehaviour;
		if (monoBehaviour != null)
		{
			monoBehaviour.TryGetComponent<PBDGrassBody>(out var component);
			if (component != null)
			{
				return component.IsPBDBodyDataInitialized;
			}
		}
		return false;
	}

	public IndirectRenderingStatisticsInfo GetStats()
	{
		int num = 0;
		foreach (KeyValuePair<IIndirectMesh, GPUData> meshDatum in m_MeshData)
		{
			num += meshDatum.Value.InstanceData.Count;
		}
		if (num == 0)
		{
			return default(IndirectRenderingStatisticsInfo);
		}
		IndirectRenderingStatisticsInfo result = default(IndirectRenderingStatisticsInfo);
		result.ArgsBufferCount = m_ArgsBuffer.count;
		result.MeshBufferCount = m_MeshArgsBuffer.count;
		result.InstanceBufferCount = m_InstanceDataBuffer.count;
		result.DrawcallCount = m_ArgsBuffer.count / 5;
		result.MeshCount = m_MeshArgsBuffer.count / 4;
		result.InstanceCount = num;
		return result;
	}
}
