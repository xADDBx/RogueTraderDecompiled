using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Owlcat.Runtime.Core.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDSkinnedBody : PBDBodyBase<SkinnedBody>
{
	private static int _PbdBonesOffset = Shader.PropertyToID("_PbdBonesOffset");

	private static int _PbdBoneIndicesOffset = Shader.PropertyToID("_PbdBoneIndicesOffset");

	private static int _PbdEnabledLocal = Shader.PropertyToID("_PbdEnabledLocal");

	private static int _PbdWeightMask = Shader.PropertyToID("_PbdWeightMask");

	[SerializeField]
	private bool m_UseGlobalGravity = true;

	[SerializeField]
	private bool m_UseGlobalWind = true;

	[SerializeField]
	private List<int> m_ParentMap = new List<int>();

	[SerializeField]
	private SkinnedMeshRenderer[] m_SkinnedMeshRenderers;

	[SerializeField]
	private MeshFilter[] m_MeshFilters;

	[SerializeField]
	private MeshRenderer[] m_MeshRenderers;

	[SerializeField]
	private List<Transform> m_Bones = new List<Transform>();

	[SerializeField]
	private List<Matrix4x4> m_Bindposes = new List<Matrix4x4>();

	[SerializeField]
	private List<int> m_BoneIndicesMap = new List<int>();

	[SerializeField]
	private List<int2> m_BoneIndicesMapOffsetCount = new List<int2>();

	[SerializeField]
	private List<int> m_BonesPerVertex = new List<int>();

	private List<MaterialPropertyBlock> m_MaterialPropertyBlocks;

	private Matrix4x4[] m_BoneposesForCPU;

	private TransformSceneHandle m_RootSceneHandle;

	private TransformStreamHandle m_RootStreamHandle;

	private NativeArray<TransformStreamHandle> m_BoneHandles;

	private PlayableGraph m_Graph;

	private AnimationScriptPlayable m_AnimationPlayable;

	private List<int> m_MaterialsCounter;

	public SkinnedMeshRenderer[] SkinnedMeshRenderers
	{
		get
		{
			return m_SkinnedMeshRenderers;
		}
		set
		{
			m_SkinnedMeshRenderers = value;
		}
	}

	public MeshFilter[] MeshFilters
	{
		get
		{
			return m_MeshFilters;
		}
		set
		{
			m_MeshFilters = value;
		}
	}

	public MeshRenderer[] MeshRenderers
	{
		get
		{
			return m_MeshRenderers;
		}
		set
		{
			m_MeshRenderers = value;
		}
	}

	public List<int> ParentMap => m_ParentMap;

	public List<Transform> Bones => m_Bones;

	public List<Matrix4x4> Bindposes => m_Bindposes;

	public List<int> BoneIndicesMap => m_BoneIndicesMap;

	public List<int2> BoneIndicesMapOffsetCount => m_BoneIndicesMapOffsetCount;

	public List<int> BonesPerVertex => m_BonesPerVertex;

	protected override bool ValidateBeforeInitialize()
	{
		if (m_SkinnedMeshRenderers.Any((SkinnedMeshRenderer smr) => smr == null))
		{
			Debug.LogError("PBDSkinnedBody " + base.name + ": SkinnedMeshRenderer is null.", this);
			return false;
		}
		if (m_MeshRenderers.Any((MeshRenderer mr) => mr == null))
		{
			Debug.LogError("PBDSkinnedBody " + base.name + ": MeshRenderer is null.", this);
			return false;
		}
		if (m_SkinnedMeshRenderers.Length != m_MeshRenderers.Length)
		{
			Debug.LogError("PBDSkinnedBody " + base.name + ": SkinnedMeshRenderers and MeshRenderers are out of sync.", this);
			return false;
		}
		if (m_BonesPerVertex.Count != m_SkinnedMeshRenderers.Length)
		{
			Debug.LogError("PBDSkinnedBody " + base.name + ": SkinnedMeshRenderers and BonesPerVertex are out of sync.", this);
			return false;
		}
		for (int i = 0; i < m_SkinnedMeshRenderers.Length; i++)
		{
			if (m_SkinnedMeshRenderers[i].bones.Length != m_BoneIndicesMapOffsetCount[i].y)
			{
				Debug.LogError($"PBDSkinnedBody {base.name}: SkinnedMeshRenderer {m_SkinnedMeshRenderers[i]} bones conflicts with generated bindposes.", this);
				return false;
			}
		}
		return true;
	}

	protected override void InitializeInternal()
	{
		InitBody();
		if (m_Body == null)
		{
			base.enabled = false;
			return;
		}
		InitAnimator();
		if (m_Body == null)
		{
			return;
		}
		PBD.RegisterBody(m_Body);
		RegisterLocalColliders();
		if (PBD.IsGpu)
		{
			if (m_MaterialsCounter != null && m_MaterialsCounter.Count != 0)
			{
				return;
			}
			m_MaterialPropertyBlocks = new List<MaterialPropertyBlock>();
			for (int i = 0; i < m_SkinnedMeshRenderers.Length; i++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = m_SkinnedMeshRenderers[i];
				MeshRenderer meshRenderer = m_MeshRenderers[i];
				Material[] sharedMaterials = skinnedMeshRenderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					_ = sharedMaterials[j];
					m_MaterialPropertyBlocks.Add(new MaterialPropertyBlock());
				}
				skinnedMeshRenderer.enabled = false;
				meshRenderer.enabled = true;
			}
			m_MaterialsCounter = new List<int>();
			int num = 0;
			MeshRenderer[] meshRenderers = m_MeshRenderers;
			foreach (MeshRenderer meshRenderer2 in meshRenderers)
			{
				int num2 = meshRenderer2.sharedMaterials.Length;
				m_MaterialsCounter.Add(num2);
				for (int k = 0; k < num2; k++)
				{
					meshRenderer2.GetPropertyBlock(m_MaterialPropertyBlocks[num], k);
					m_MaterialPropertyBlocks[num].SetFloat(_PbdEnabledLocal, 0f);
					meshRenderer2.SetPropertyBlock(m_MaterialPropertyBlocks[num], k);
					num++;
				}
			}
		}
		else
		{
			for (int l = 0; l < m_SkinnedMeshRenderers.Length; l++)
			{
				SkinnedMeshRenderer skinnedMeshRenderer2 = m_SkinnedMeshRenderers[l];
				MeshRenderer obj = m_MeshRenderers[l];
				skinnedMeshRenderer2.enabled = true;
				obj.enabled = false;
			}
		}
	}

	private void InitAnimator()
	{
		if (!PBD.IsGpu)
		{
			Animator animator = GetComponentInChildren<Animator>();
			if (animator == null)
			{
				animator = base.gameObject.AddComponent<Animator>();
			}
			m_RootSceneHandle = animator.BindSceneTransform(base.transform);
			m_RootStreamHandle = animator.BindStreamTransform(base.transform);
			m_BoneHandles = new NativeArray<TransformStreamHandle>(m_Bones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < m_Bones.Count; i++)
			{
				Transform transform = m_Bones[i];
				m_BoneHandles[i] = animator.BindStreamTransform(transform);
			}
			PBD.GetParticles(base.Body, out var particles);
			PBDSkinnedBodyAnimationJob pBDSkinnedBodyAnimationJob = default(PBDSkinnedBodyAnimationJob);
			pBDSkinnedBodyAnimationJob.Boneposes = base.Body.Boneposes;
			pBDSkinnedBodyAnimationJob.BoneHandles = m_BoneHandles;
			pBDSkinnedBodyAnimationJob.ParentMap = base.Body.ParentMap;
			pBDSkinnedBodyAnimationJob.RootSceneHandle = m_RootSceneHandle;
			pBDSkinnedBodyAnimationJob.RootStreamHandle = m_RootStreamHandle;
			pBDSkinnedBodyAnimationJob.BasePositions = particles.BasePosition;
			pBDSkinnedBodyAnimationJob.Positions = particles.Position;
			PBDSkinnedBodyAnimationJob jobData = pBDSkinnedBodyAnimationJob;
			m_Graph = PlayableGraph.Create("PBDSkinning");
			m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
			m_AnimationPlayable = AnimationScriptPlayable.Create(m_Graph, jobData);
			AnimationPlayableOutput.Create(m_Graph, "output", animator).SetSourcePlayable(m_AnimationPlayable);
			m_Graph.Play();
		}
	}

	public void InitBody()
	{
		if (m_Body != null)
		{
			return;
		}
		NativeArray<Matrix4x4> bindposes = new NativeArray<Matrix4x4>(m_Bones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray<Matrix4x4> boneposes = new NativeArray<Matrix4x4>(m_Bones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> parentMap = new NativeArray<int>(m_Bones.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> boneIndicesMap = new NativeArray<int>(m_BoneIndicesMap.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		bool flag = !PBD.IsGpu && m_BoneposesForCPU == null;
		if (flag)
		{
			m_BoneposesForCPU = new Matrix4x4[m_Bones.Count];
		}
		for (int i = 0; i < m_Bones.Count; i++)
		{
			Transform transform = m_Bones[i];
			if (PBD.IsGpu)
			{
				boneposes[i] = base.transform.worldToLocalMatrix * transform.localToWorldMatrix;
			}
			else
			{
				if (flag)
				{
					m_BoneposesForCPU[i] = base.transform.worldToLocalMatrix * transform.localToWorldMatrix;
				}
				boneposes[i] = m_BoneposesForCPU[i];
			}
			bindposes[i] = m_Bindposes[i];
			parentMap[i] = m_ParentMap[i];
		}
		List<Particle> list = ListPool<Particle>.Claim();
		List<Constraint> list2 = ListPool<Constraint>.Claim();
		for (int j = 0; j < m_Particles.Count; j++)
		{
			Particle item = m_Particles[j];
			item.Position = base.transform.TransformPoint(item.Position);
			item.BasePosition = item.Position;
			item.Orientation = base.transform.rotation * item.Orientation;
			item.PredictedOrientation = item.Orientation;
			if (!m_UseGlobalGravity)
			{
				item.Flags |= 1u;
			}
			if (!m_UseGlobalWind)
			{
				item.Flags |= 2u;
			}
			list.Add(item);
		}
		for (int k = 0; k < m_Constraints.Count; k++)
		{
			Constraint item2 = m_Constraints[k];
			item2.Refresh(list);
			list2.Add(item2);
		}
		for (int l = 0; l < m_BoneIndicesMap.Count; l++)
		{
			boneIndicesMap[l] = m_BoneIndicesMap[l];
		}
		m_Body = new SkinnedBody(base.name, list, boneposes, bindposes, boneIndicesMap, parentMap, list2, base.DisconnectedConstraintsOffsetCount);
		m_Body.Restitution = base.Restitution;
		m_Body.Friction = base.Friction;
		m_Body.TeleportDistanceTreshold = base.TeleportDistanceTreshold;
		m_Body.LocalToWorld = base.transform.localToWorldMatrix;
		ListPool<Particle>.Release(list);
		ListPool<Constraint>.Release(list2);
	}

	protected override void Dispose()
	{
		UnregisterLocalColliders();
		PBD.UnregisterBody(m_Body);
		m_Body.Dispose();
		m_Body = null;
		if (!PBD.IsGpu && m_BoneHandles.IsCreated)
		{
			m_BoneHandles.Dispose();
			m_Graph.Destroy();
		}
	}

	protected internal override void OnBodyDataUpdated()
	{
		if (!PBD.IsGpu)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_MeshRenderers.Length; i++)
		{
			MeshRenderer meshRenderer = m_MeshRenderers[i];
			int num2 = m_MaterialsCounter[i];
			int bonesPerVertex = m_BonesPerVertex[i];
			Vector4 weightMask = GetWeightMask(bonesPerVertex);
			for (int j = 0; j < num2; j++)
			{
				MaterialPropertyBlock materialPropertyBlock = m_MaterialPropertyBlocks[num];
				materialPropertyBlock.SetFloat(_PbdEnabledLocal, 1f);
				materialPropertyBlock.SetInt(_PbdBonesOffset, base.Body.BonesOffset);
				materialPropertyBlock.SetInt(_PbdBoneIndicesOffset, base.Body.BoneIndicesMapOffset + m_BoneIndicesMapOffsetCount[i].x);
				materialPropertyBlock.SetVector(_PbdWeightMask, weightMask);
				meshRenderer.SetPropertyBlock(materialPropertyBlock, j);
				num++;
			}
		}
	}

	private Vector4 GetWeightMask(int bonesPerVertex)
	{
		return bonesPerVertex switch
		{
			1 => new Vector4(1f, 0f, 0f, 0f), 
			2 => new Vector4(1f, 1f, 0f, 0f), 
			3 => new Vector4(1f, 1f, 1f, 0f), 
			4 => new Vector4(1f, 1f, 1f, 1f), 
			_ => Vector4.one, 
		};
	}

	protected override void UpdateInternal()
	{
		base.UpdateInternal();
		if (PBD.IsGpu)
		{
			SkinnedMeshRenderer[] skinnedMeshRenderers = m_SkinnedMeshRenderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
			{
				if (skinnedMeshRenderer.enabled)
				{
					skinnedMeshRenderer.enabled = false;
				}
			}
			return;
		}
		MeshRenderer[] meshRenderers = m_MeshRenderers;
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			if (meshRenderer.enabled)
			{
				meshRenderer.enabled = false;
			}
		}
		PBD.GetParticles(base.Body, out var particles);
		PBDSkinnedBodyAnimationJob jobData = m_AnimationPlayable.GetJobData<PBDSkinnedBodyAnimationJob>();
		jobData.BasePositions = particles.BasePosition;
		jobData.Positions = particles.Position;
		m_AnimationPlayable.SetJobData(jobData);
	}

	private void OnDrawGizmosSelected()
	{
		PBD.DrawGizmos(m_Body);
	}

	public void InitRenderers()
	{
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		bool flag = false;
		if (m_SkinnedMeshRenderers != null && componentsInChildren.Length == m_SkinnedMeshRenderers.Length)
		{
			flag = true;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i] != m_SkinnedMeshRenderers[i])
				{
					flag = false;
					break;
				}
			}
		}
		bool flag2 = false;
		if (m_MeshRenderers != null && m_MeshFilters != null && m_MeshFilters.Length == componentsInChildren.Length && m_MeshRenderers.Length == componentsInChildren.Length)
		{
			flag2 = true;
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (m_MeshFilters[j] == null)
				{
					flag2 = false;
					break;
				}
				if (m_MeshRenderers[j] == null)
				{
					flag2 = false;
					break;
				}
				if (m_MeshFilters[j].sharedMesh != componentsInChildren[j].sharedMesh)
				{
					flag2 = false;
					break;
				}
			}
		}
		if (flag && flag2)
		{
			return;
		}
		m_SkinnedMeshRenderers = componentsInChildren;
		m_MeshRenderers = new MeshRenderer[m_SkinnedMeshRenderers.Length];
		m_MeshFilters = new MeshFilter[m_SkinnedMeshRenderers.Length];
		for (int k = 0; k < m_SkinnedMeshRenderers.Length; k++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = m_SkinnedMeshRenderers[k];
			string text = "GPUMeshRenderer_" + skinnedMeshRenderer.sharedMesh.name;
			Transform transform = base.transform.FindChildRecursive(text);
			if (transform == null)
			{
				GameObject obj = new GameObject(text);
				obj.transform.SetParent(base.transform);
				obj.transform.localPosition = default(Vector3);
				obj.transform.localScale = Vector3.one;
				obj.transform.localRotation = Quaternion.identity;
				transform = obj.transform;
			}
			m_MeshFilters[k] = transform.gameObject.EnsureComponent<MeshFilter>();
			m_MeshFilters[k].sharedMesh = skinnedMeshRenderer.sharedMesh;
			m_MeshRenderers[k] = transform.gameObject.EnsureComponent<MeshRenderer>();
			m_MeshRenderers[k].sharedMaterials = skinnedMeshRenderer.sharedMaterials;
			m_MeshRenderers[k].enabled = false;
		}
	}
}
