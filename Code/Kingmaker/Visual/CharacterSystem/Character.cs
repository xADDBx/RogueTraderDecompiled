using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.DxtCompressor;
using RogueTrader.Code.ShaderConsts;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.CharacterSystem;

public class Character : RegisteredBehaviour, IUpdatable
{
	public enum AtlasSize
	{
		AtlasSize512 = 0x200,
		AtlasSize1024 = 0x400,
		AtlasSize2048 = 0x800
	}

	public class SelectedRampIndices : EquipmentEntity.IColorRampIndicesProvider
	{
		public EquipmentEntity EquipmentEntity { get; set; }

		public int PrimaryIndex { get; set; }

		public int SecondaryIndex { get; set; }
	}

	[Serializable]
	public class SavedSelectedRampIndices
	{
		public EquipmentEntityLink EquipmentEntityLink;

		public int PrimaryIndex;

		public int SecondaryIndex;
	}

	[Serializable]
	public class OutfitPartInfo
	{
		public EquipmentEntity.OutfitPart OutfitPart;

		public GameObject GameObject;

		public EquipmentEntity Ee;

		public OutfitPartInfo(EquipmentEntity.OutfitPart outfitPart, GameObject gameObject, EquipmentEntity ee)
		{
			OutfitPart = outfitPart;
			GameObject = gameObject;
			Ee = ee;
		}
	}

	[Flags]
	public enum RenderingLayerEnum
	{
		Nothing = 0,
		[InspectorName("0: RenderingLayer1")]
		RenderingLayer1 = 1,
		[InspectorName("1: RenderingLayer2")]
		RenderingLayer2 = 2,
		[InspectorName("2: RenderingLayer3")]
		RenderingLayer3 = 4,
		[InspectorName("3: RenderingLayer4")]
		RenderingLayer4 = 8,
		[InspectorName("4: RenderingLayer5")]
		RenderingLayer5 = 0x10,
		[InspectorName("5: RenderingLayer6")]
		RenderingLayer6 = 0x20,
		[InspectorName("6: RenderingLayer7")]
		RenderingLayer7 = 0x40,
		[InspectorName("7: RenderingLayer8")]
		RenderingLayer8 = 0x80,
		Everything = 0xFF
	}

	public bool? PreventUpdate;

	private List<BodyPart> m_OverlayBodyParts;

	private readonly List<CharacterAtlas> m_Atlases = new List<CharacterAtlas>();

	private SkinnedMeshRenderer m_AtlasRenderer;

	private Material m_AtlasMaterial;

	private readonly HashSet<Skeleton.Bone> m_EquipmentBoneModifiers = new HashSet<Skeleton.Bone>();

	private BoneUpdateJob m_BoneUpdateJob;

	private TransformAccessArray m_BonesForJob;

	private readonly List<OutfitPartInfo> m_OutfitObjectsSpawned = new List<OutfitPartInfo>();

	public readonly List<Renderer> ColorizedOutfitParts = new List<Renderer>();

	private bool m_IsInitialized;

	public readonly List<SelectedRampIndices> RampIndices = new List<SelectedRampIndices>();

	[SerializeField]
	private Skeleton m_Skeleton;

	[SerializeField]
	public CharacterAtlasData AtlasData;

	[SerializeField]
	private bool m_Mirror;

	private bool m_SkeletonChanged = true;

	[FormerlySerializedAs("AnimationSet")]
	public AnimationSet m_AnimationSet;

	public Animator AnimatorPrefab;

	public bool IsDirty;

	public bool IsAtlasesDirty;

	public bool IsInDollRoom;

	[Tooltip("Галка, которая дает возможность собирать кричу в чаргене без необходимости экипировать ее оружием")]
	public bool IsCreatureAsCharacter;

	[Tooltip("Sometimes we need to forbid visualization of belt items, for Example on Ulfar")]
	public bool ForbidBeltItemVisualization;

	public bool SaveRagdoll;

	public AtlasSize MaxAtlasSize = AtlasSize.AtlasSize2048;

	public BakedCharacter BakedCharacter;

	private DxtCompressorService m_DxtService;

	[SerializeField]
	private CharacterBonesList m_BonesList;

	[SerializeField]
	private List<EquipmentEntityLink> m_SavedEquipmentEntities = new List<EquipmentEntityLink>();

	private List<EquipmentEntity> m_SavedBeforeCutsceneEquipment = new List<EquipmentEntity>();

	private List<SelectedRampIndices> m_SavedBeforeCutsceneRampIndices = new List<SelectedRampIndices>();

	[SerializeField]
	public List<SavedSelectedRampIndices> m_SavedRampIndices = new List<SavedSelectedRampIndices>();

	private readonly EquipmentEntity.PaintedTextures m_EquipmentEntitiesTextures = new EquipmentEntity.PaintedTextures();

	private bool m_PeacefulMode;

	private bool m_ShowHelmet = true;

	private bool m_ShowCloth = true;

	private bool m_ShowBackpack = true;

	private bool m_BackEquipmentIsDirty;

	public Func<EquipmentEntity.OutfitPart, GameObject, bool> OutfitFilter;

	public HashSet<UnitAnimationManager> MechsAnimationManagers = new HashSet<UnitAnimationManager>();

	public List<EquipmentEntityLink> EquipmentEntitiesForPreload = new List<EquipmentEntityLink>();

	[SerializeField]
	private RenderingLayerEnum m_DefaultRenderingLayer = RenderingLayerEnum.RenderingLayer2;

	[HideInInspector]
	public uint CurrentLayer;

	public bool canNotBeRebaked;

	public ClothCollider[] ClothColliders;

	private Dictionary<string, Transform> m_AttachBonesCache = new Dictionary<string, Transform>();

	public bool OverlaysMerged { get; private set; } = true;


	private Material AtlasMaterial
	{
		get
		{
			if (!m_AtlasRenderer)
			{
				return m_AtlasMaterial;
			}
			return m_AtlasRenderer.sharedMaterial;
		}
	}

	public AnimationSet AnimationSet
	{
		get
		{
			if (!(m_AnimationSet == null))
			{
				return m_AnimationSet;
			}
			return BlueprintRoot.Instance.HumanAnimationSet;
		}
		set
		{
			m_AnimationSet = value;
		}
	}

	public bool IsCharacterStudio { get; set; }

	public List<EquipmentEntityLink> SavedEquipmentEntities
	{
		get
		{
			return m_SavedEquipmentEntities;
		}
		set
		{
			m_SavedEquipmentEntities = value;
		}
	}

	public List<EquipmentEntity> SavedBeforeCutsceneEquipment
	{
		get
		{
			return m_SavedBeforeCutsceneEquipment;
		}
		set
		{
			m_SavedBeforeCutsceneEquipment = value;
		}
	}

	public List<SelectedRampIndices> SavedBeforeCutsceneRampIndices
	{
		get
		{
			return m_SavedBeforeCutsceneRampIndices;
		}
		set
		{
			m_SavedBeforeCutsceneRampIndices = value;
		}
	}

	public UnitAnimationManager AnimationManager { get; private set; }

	public List<EquipmentEntity> EquipmentEntities { get; } = new List<EquipmentEntity>();


	public int EquipmentEntityCount => EquipmentEntities.Count;

	public List<SkinnedMeshRenderer> Renderers { get; } = new List<SkinnedMeshRenderer>();


	public RenderingLayerEnum DefaultRenderingLayer
	{
		get
		{
			return m_DefaultRenderingLayer;
		}
		set
		{
			m_DefaultRenderingLayer = value;
		}
	}

	public Skeleton Skeleton
	{
		get
		{
			return m_Skeleton;
		}
		set
		{
			m_Skeleton = value;
			m_SkeletonChanged = true;
		}
	}

	public Animator Animator { get; private set; }

	public bool PeacefulMode
	{
		get
		{
			return m_PeacefulMode;
		}
		set
		{
			if (m_PeacefulMode != value)
			{
				m_PeacefulMode = value;
				RebuildOutfit();
			}
		}
	}

	public ParticlesSnapMap ParticlesSnapMap { get; private set; }

	public event Action OnBackEquipmentUpdated;

	public event Action<Character> OnUpdated;

	public void ShowEmptyBakedCharacter()
	{
		if (m_BonesList == null && BakedCharacter != null)
		{
			PFLog.TechArt.Error("Spam alert1. Null Bones List:" + BakedCharacter.name);
		}
	}

	public void OnStart()
	{
		if (m_IsInitialized)
		{
			return;
		}
		m_DxtService = Services.GetInstance<DxtCompressorService>();
		Animator = GetComponentInChildren<Animator>();
		if (BakedCharacter == null && Animator != null)
		{
			Utils.EditorSafeDestroy(Animator.gameObject);
			Animator = null;
		}
		if ((bool)AnimatorPrefab && !Animator)
		{
			Animator animator = UnityEngine.Object.Instantiate(AnimatorPrefab, base.transform);
			Transform transform = animator.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			animator.gameObject.name = base.name + ".animator";
			Animator = animator;
			ClothColliders = transform.GetComponentsInChildren<ClothCollider>();
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Utils.EditorSafeDestroy(componentsInChildren[i].gameObject);
			}
		}
		if (Animator != null)
		{
			Animator.runtimeAnimatorController = null;
			Animator.enabled = true;
			AnimationManager = Animator.EnsureComponent<UnitAnimationManager>();
			AnimationManager.IsInDollRoom = IsInDollRoom;
			if (Skeleton.AnimationSetOverride != null)
			{
				AnimationSet = Skeleton.AnimationSetOverride;
			}
			AnimationManager.AnimationSet = AnimationSet;
		}
		m_BonesList = Animator.EnsureComponent<CharacterBonesList>();
		if (m_BonesList.Bones == null)
		{
			m_BonesList.UpdateCache(CharacterBonesSetup.Instance);
		}
		if (BakedCharacter != null)
		{
			foreach (BakedCharacter.RendererDescription rendererDescription in BakedCharacter.RendererDescriptions)
			{
				if (rendererDescription.Mesh != null)
				{
					rendererDescription.Mesh.UploadMeshData(markNoLongerReadable: true);
				}
			}
		}
		else
		{
			RestoreSavedEquipment();
			IsDirty = true;
			m_SkeletonChanged = true;
			DoUpdate();
			SkeletonUpdateService.Ensure();
		}
		m_IsInitialized = true;
		SetUpCharacterRenderingLayerMask();
	}

	public void UpdateMesh()
	{
		IsDirty = true;
		m_SkeletonChanged = true;
		DoUpdate();
	}

	private void OnDestroy()
	{
		if ((bool)m_AtlasMaterial)
		{
			if ((bool)m_AtlasRenderer)
			{
				m_AtlasRenderer.sharedMaterial = null;
			}
			UnityEngine.Object.Destroy(m_AtlasMaterial);
			m_AtlasMaterial = null;
		}
		ClearAtlases();
		ClearMeshes();
		m_EquipmentEntitiesTextures.Clear();
		if (m_BonesForJob.isCreated)
		{
			m_BonesForJob.Dispose();
		}
	}

	private void LoadBakedCharacter()
	{
		foreach (BakedCharacter.RendererDescription rendererDescription in BakedCharacter.RendererDescriptions)
		{
			Transform[] array = new Transform[rendererDescription.Bones.Length];
			for (int i = 0; i < rendererDescription.Bones.Length; i++)
			{
				Transform byName = m_BonesList.GetByName(rendererDescription.Bones[i]);
				if ((bool)byName)
				{
					array[i] = byName;
				}
			}
			SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
			if (componentsInChildren.Length == 0)
			{
				GameObject obj = new GameObject(rendererDescription.Name);
				obj.transform.parent = Animator.transform;
				obj.transform.position = default(Vector3);
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
				SkinnedMeshRenderer skinnedMeshRenderer = obj.AddComponent<SkinnedMeshRenderer>();
				skinnedMeshRenderer.bones = array;
				skinnedMeshRenderer.sharedMesh = rendererDescription.Mesh;
				skinnedMeshRenderer.sharedMaterial = rendererDescription.Material;
				skinnedMeshRenderer.rootBone = m_BonesList.GetByName(rendererDescription.RootBone);
				Renderers.Add(skinnedMeshRenderer);
			}
			else
			{
				Renderers.Add(componentsInChildren[0]);
			}
		}
		Animator.Rebind();
		this.OnUpdated?.Invoke(this);
	}

	public void DoUpdate()
	{
		if (!PreventUpdate.HasValue || PreventUpdate.Value || !Game.HasInstance || (MainMenuUI.Instance != null && (!RootUIContext.Instance.IsChargenShown || !IsInDollRoom)))
		{
			return;
		}
		bool flag = false;
		if (!BakedCharacter)
		{
			if (IsDirty)
			{
				try
				{
					if (m_Skeleton != null && m_Skeleton.CharacterFxBonesMap != null)
					{
						ParticlesSnapMap = this.EnsureComponent<ParticlesSnapMap>();
						ParticlesSnapMap.CharacterFxBonesMap = m_Skeleton.CharacterFxBonesMap;
						ParticlesSnapMap.Init();
					}
					UpdateCharacter();
					CacheSkeletonBones();
					flag = true;
					if (this.OnUpdated != null)
					{
						this.OnUpdated(this);
					}
					if (m_BackEquipmentIsDirty)
					{
						m_BackEquipmentIsDirty = false;
						this.OnBackEquipmentUpdated?.Invoke();
					}
				}
				finally
				{
					IsDirty = false;
				}
			}
			if (!OverlaysMerged && m_OverlayBodyParts != null && Services.GetInstance<CharacterAtlasService>().RequestsCount == 0 && Services.GetInstance<DxtCompressorService>().RequestsCount == 0)
			{
				MergeOverlays(m_OverlayBodyParts);
			}
		}
		if ((bool)m_Skeleton && (m_Skeleton.IsDirty() || m_SkeletonChanged))
		{
			CacheSkeletonBones();
			flag = true;
		}
		if (!flag)
		{
			foreach (EquipmentEntity equipmentEntity in EquipmentEntities)
			{
				if (equipmentEntity.IsDirty())
				{
					CacheSkeletonBones();
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (EquipmentEntity equipmentEntity2 in EquipmentEntities)
		{
			equipmentEntity2.ResetDirty();
		}
	}

	private void OnRenderObject()
	{
		if (m_Atlases.Count == 0 || !IsAtlasesDirty || !OverlaysMerged)
		{
			return;
		}
		IsAtlasesDirty = false;
		foreach (EquipmentEntity ee in EquipmentEntities)
		{
			if (ee.ForcedPrimaryIndex >= 0 || ee.ForcedSecondaryIndex >= 0)
			{
				ee.RepaintTextures(m_EquipmentEntitiesTextures, ee.ForcedPrimaryIndex, ee.ForcedSecondaryIndex);
				continue;
			}
			SelectedRampIndices selectedRampIndices = RampIndices.Find((SelectedRampIndices x) => x.EquipmentEntity == ee);
			if (selectedRampIndices != null)
			{
				ee.RepaintTextures(m_EquipmentEntitiesTextures, selectedRampIndices.PrimaryIndex, selectedRampIndices.SecondaryIndex);
			}
			else
			{
				ee.RepaintTextures(m_EquipmentEntitiesTextures, 0, 0);
			}
		}
		foreach (CharacterAtlas atlase in m_Atlases)
		{
			atlase.Build(m_EquipmentEntitiesTextures, AtlasMaterial, cleanAtlas: true, delayTextureCreation: true);
		}
		Services.GetInstance<CharacterAtlasService>().QueueAtlasRebuild(m_Atlases, AtlasMaterial, OnAtlasCompressed, OnAtlasNotCompressed, base.name);
		m_EquipmentEntitiesTextures.Clear();
	}

	protected override void OnEnabled()
	{
		ShowEmptyBakedCharacter();
		base.OnEnabled();
		IsDirty = BakedCharacter == null && (!OverlaysMerged || m_Atlases.Count == 0);
	}

	public void OnApplicationFocus(bool isFocused)
	{
		if (isFocused && !ApplicationFocusEvents.CharacterDisabled && !BakedCharacter && Screen.fullScreen)
		{
			IsAtlasesDirty = true;
		}
	}

	public void SetPrimaryRampIndex(EquipmentEntity ee, int primaryRampIndex, bool saved = false)
	{
		SetRampIndices(ee, primaryRampIndex, null, saved);
	}

	public void SetSecondaryRampIndex(EquipmentEntity ee, int secondaryRampIndex, bool saved = false)
	{
		SetRampIndices(ee, null, secondaryRampIndex, saved);
	}

	public void SetRampIndices(EquipmentEntity ee, int? primaryRampIndex, int? secondaryRampIndex, bool saved = false)
	{
		if (ee == null || (!primaryRampIndex.HasValue && !secondaryRampIndex.HasValue))
		{
			return;
		}
		SelectedRampIndices selectedRampIndices = RampIndices.FirstOrDefault((SelectedRampIndices i) => i.EquipmentEntity == ee);
		if (selectedRampIndices != null)
		{
			if (primaryRampIndex.HasValue && selectedRampIndices.PrimaryIndex != primaryRampIndex && primaryRampIndex.HasValue)
			{
				selectedRampIndices.PrimaryIndex = primaryRampIndex.Value;
			}
			if (secondaryRampIndex.HasValue && selectedRampIndices.SecondaryIndex != secondaryRampIndex && secondaryRampIndex.HasValue)
			{
				selectedRampIndices.SecondaryIndex = secondaryRampIndex.Value;
			}
		}
		else
		{
			SelectedRampIndices selectedRampIndices2 = new SelectedRampIndices
			{
				EquipmentEntity = ee
			};
			if (primaryRampIndex.HasValue)
			{
				selectedRampIndices2.PrimaryIndex = primaryRampIndex.Value;
			}
			if (secondaryRampIndex.HasValue)
			{
				selectedRampIndices2.SecondaryIndex = secondaryRampIndex.Value;
			}
			RampIndices.Add(selectedRampIndices2);
		}
		IsAtlasesDirty = true;
	}

	public void CacheSkeletonBones()
	{
		if (m_BonesForJob.isCreated)
		{
			m_BonesForJob.Dispose();
		}
		Transform[] array = new Transform[Skeleton.Bones.Count];
		for (int i = 0; i < Skeleton.Bones.Count; i++)
		{
			Skeleton.Bone bone2 = Skeleton.Bones[i];
			array[i] = m_BonesList.GetByName(bone2.Name);
		}
		m_BonesForJob = new TransformAccessArray(array);
		m_BoneUpdateJob = new BoneUpdateJob
		{
			Scales = Skeleton.GetBoneData()
		};
		if (!BakedCharacter || IsCharacterStudio)
		{
			foreach (Skeleton.Bone equipmentBoneModifier in m_EquipmentBoneModifiers)
			{
				equipmentBoneModifier.Transform.localPosition = equipmentBoneModifier.OriginalOffset;
			}
			m_EquipmentBoneModifiers.Clear();
			foreach (EquipmentEntity equipmentEntity in EquipmentEntities)
			{
				foreach (Skeleton.Bone modifier in equipmentEntity.SkeletonModifiers)
				{
					if ((modifier.Scale == Vector3.one && (!modifier.ApplyOffset || modifier.Offset == Vector3.zero)) || EquipmentEntities.Intersect(modifier.IgnoreIfCharacterContainsEE).Any())
					{
						continue;
					}
					if (m_EquipmentBoneModifiers.TryFind((Skeleton.Bone bone) => bone.Name == modifier.Name, out var result))
					{
						result.Scale.Scale(modifier.Scale);
						if (modifier.ApplyOffset)
						{
							result.Offset += modifier.Offset;
						}
					}
					else
					{
						m_EquipmentBoneModifiers.Add(new Skeleton.Bone
						{
							Name = modifier.Name,
							Transform = m_BonesList.GetByName(modifier.Name),
							Scale = modifier.Scale,
							ApplyOffset = modifier.ApplyOffset,
							Offset = modifier.Offset,
							OriginalOffset = m_BonesList.GetByName(modifier.Name).localPosition
						});
					}
				}
			}
		}
		m_SkeletonChanged = false;
	}

	public void RestoreSavedEquipment()
	{
		AddEquipmentEntities(m_SavedEquipmentEntities.Select((EquipmentEntityLink eel) => eel.Load()));
		foreach (SavedSelectedRampIndices savedRampIndex in m_SavedRampIndices)
		{
			EquipmentEntity ee = savedRampIndex.EquipmentEntityLink.Load();
			SetRampIndices(ee, savedRampIndex.PrimaryIndex, savedRampIndex.SecondaryIndex);
		}
	}

	public void RestoreEquipment()
	{
		AddEquipmentEntities(m_SavedBeforeCutsceneEquipment);
		foreach (SavedSelectedRampIndices savedRampIndex in m_SavedRampIndices)
		{
			EquipmentEntity ee = savedRampIndex.EquipmentEntityLink.Load();
			SetRampIndices(ee, savedRampIndex.PrimaryIndex, savedRampIndex.SecondaryIndex);
		}
	}

	public void AddEquipmentEntity(EquipmentEntityLink eel, bool saved = false)
	{
		EquipmentEntitiesForPreload.Add(eel);
		AddEquipmentEntity(eel.Load(), saved);
	}

	public void RemoveEquipmentEntity(EquipmentEntityLink eel, bool saved = false)
	{
		EquipmentEntitiesForPreload.Remove(eel);
		RemoveEquipmentEntity(eel.Load(), saved);
	}

	public void AddEquipmentEntity(EquipmentEntity ee, bool saved = false)
	{
		if (ee == null || EquipmentEntities.Contains(ee))
		{
			return;
		}
		EquipmentEntities.Add(ee);
		if (ee.ForcedPrimaryIndex >= 0 || ee.ForcedSecondaryIndex >= 0)
		{
			SelectedRampIndices selectedRampIndices = RampIndices.FirstOrDefault((SelectedRampIndices rampIndices) => rampIndices.EquipmentEntity == ee);
			if (selectedRampIndices == null)
			{
				selectedRampIndices = new SelectedRampIndices
				{
					EquipmentEntity = ee,
					PrimaryIndex = ee.ForcedPrimaryIndex,
					SecondaryIndex = ee.ForcedSecondaryIndex
				};
				RampIndices.Add(selectedRampIndices);
			}
			else
			{
				selectedRampIndices.PrimaryIndex = ee.ForcedPrimaryIndex;
				selectedRampIndices.PrimaryIndex = ee.ForcedSecondaryIndex;
			}
			IsDirty = true;
			return;
		}
		if (ee.PrimaryRamps.Count > 0 || ee.SecondaryRamps.Count > 0)
		{
			SelectedRampIndices selectedRampIndices2 = RampIndices.FirstOrDefault((SelectedRampIndices rampIndices) => rampIndices.EquipmentEntity == ee);
			if (selectedRampIndices2 == null)
			{
				selectedRampIndices2 = new SelectedRampIndices
				{
					EquipmentEntity = ee
				};
				RampIndices.Add(selectedRampIndices2);
			}
			IsAtlasesDirty = true;
		}
		IsDirty = true;
	}

	public void AddEquipmentEntities(IEnumerable<EquipmentEntity> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("AddEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntity ee)
			{
				AddEquipmentEntity(ee, saved);
			});
		}
	}

	public void AddEquipmentEntities(IEnumerable<EquipmentEntityLink> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("AddEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntityLink ee)
			{
				AddEquipmentEntity(ee);
			});
		}
	}

	public IEnumerator AddEquipmentEntitiesCo(IEnumerable<EquipmentEntityLink> ees, Action onComplete)
	{
		using (ProfileScope.NewScope("AddEquipmentEntitiesCo"))
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (EquipmentEntityLink ee in ees)
			{
				AddEquipmentEntity(ee);
				if (!(Time.realtimeSinceStartup - realtimeSinceStartup < 0.033f))
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
			}
			onComplete?.Invoke();
		}
	}

	public void RemoveEquipmentEntity(EquipmentEntity ee, bool saved = false)
	{
		if (ee == null)
		{
			return;
		}
		m_EquipmentEntitiesTextures.RemoveEquipmentEntity(ee);
		for (int i = 0; i < RampIndices.Count; i++)
		{
			if (RampIndices[i].EquipmentEntity == ee)
			{
				RampIndices.RemoveAt(i);
				break;
			}
		}
		IsDirty |= EquipmentEntities.Remove(ee);
	}

	public void RemoveAllEquipmentEntities(bool saved = false)
	{
		IsDirty |= EquipmentEntities.Any();
		EquipmentEntities.Clear();
		RampIndices.Clear();
	}

	public void RemoveEquipmentEntities(IEnumerable<EquipmentEntityLink> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("RemoveEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntityLink ee)
			{
				RemoveEquipmentEntity(ee, saved);
			});
		}
	}

	public void RemoveEquipmentEntities(IEnumerable<EquipmentEntity> ees, bool saved = false)
	{
		using (ProfileScope.NewScope("RemoveEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntity ee)
			{
				RemoveEquipmentEntity(ee, saved);
			});
		}
	}

	public void CopyEquipmentFrom(Character originalAvatar)
	{
		RemoveAllEquipmentEntities();
		AddEquipmentEntities(originalAvatar.EquipmentEntities);
		AddEquipmentEntities(originalAvatar.m_SavedEquipmentEntities.Select((EquipmentEntityLink eel) => eel.Load()), saved: true);
		CopyRampIndicesFrom(originalAvatar);
		m_ShowBackpack = originalAvatar.m_ShowBackpack;
		m_ShowHelmet = originalAvatar.m_ShowHelmet;
		m_ShowCloth = originalAvatar.m_ShowCloth;
		IsDirty = true;
	}

	public void CopyRampIndicesFrom(Character originalAvatar)
	{
		foreach (SelectedRampIndices rampIndex in originalAvatar.RampIndices)
		{
			SetRampIndices(rampIndex.EquipmentEntity, rampIndex.PrimaryIndex, rampIndex.SecondaryIndex);
		}
		foreach (SavedSelectedRampIndices savedRampIndex in originalAvatar.m_SavedRampIndices)
		{
			EquipmentEntity ee = savedRampIndex.EquipmentEntityLink.Load();
			SetRampIndices(ee, savedRampIndex.PrimaryIndex, savedRampIndex.SecondaryIndex);
		}
		IsAtlasesDirty = true;
	}

	private void UpdateMirrorScale()
	{
		if (!(Animator == null))
		{
			Vector3 localScale = Animator.transform.localScale;
			localScale.x = Mathf.Abs(localScale.x) * (float)((!m_Mirror) ? 1 : (-1));
			Animator.transform.localScale = localScale;
		}
	}

	public JobHandle ScheduleBoneUpdateJob()
	{
		if (!m_BonesForJob.isCreated || m_SkeletonChanged)
		{
			CacheSkeletonBones();
		}
		return m_BoneUpdateJob.Schedule(m_BonesForJob);
	}

	public void UpdateSkeleton(bool runJob = true)
	{
		using (ProfileScope.New("UpdateSkeleton"))
		{
			if (runJob)
			{
				ScheduleBoneUpdateJob().Complete();
			}
			if (!BakedCharacter || IsCharacterStudio)
			{
				foreach (Skeleton.Bone equipmentBoneModifier in m_EquipmentBoneModifiers)
				{
					Vector3 localScale = equipmentBoneModifier.Transform.localScale;
					localScale.Scale(equipmentBoneModifier.Scale);
					equipmentBoneModifier.Transform.localScale = localScale;
					if (equipmentBoneModifier.ApplyOffset)
					{
						equipmentBoneModifier.Transform.localPosition = equipmentBoneModifier.Offset;
					}
				}
			}
			if ((bool)Skeleton && Skeleton.IsDirty())
			{
				Skeleton.ResetDirty();
			}
		}
	}

	public void UpdateSkeletonDirectly(Transform root = null)
	{
		root = root.Or(Animator.transform);
		using (ProfileScope.New("UpdateSkeletonEditor"))
		{
			foreach (Skeleton.Bone bone in Skeleton.Bones)
			{
				Transform byName = m_BonesList.GetByName(bone.Name);
				if ((bool)byName)
				{
					byName.localScale = bone.Scale;
					if (bone.ApplyOffset)
					{
						byName.localPosition = bone.Offset;
					}
				}
			}
			if ((bool)Skeleton && Skeleton.IsDirty())
			{
				Skeleton.ResetDirty();
			}
		}
	}

	private bool IsHelmetThatShouldBeHidden(EquipmentEntity e)
	{
		if (m_ShowHelmet)
		{
			return false;
		}
		if (e.CantBeHiddenByDollRoom)
		{
			return false;
		}
		if (!e.BodyParts.Any((BodyPart p) => IsHelmetType(p)))
		{
			return false;
		}
		return true;
	}

	private void UpdateCharacter()
	{
		Dictionary<BodyPart, EquipmentEntity> dictionary = new Dictionary<BodyPart, EquipmentEntity>();
		foreach (EquipmentEntity item in from ee in EquipmentEntities
			where ee != null && ee.BodyParts.Count > 0
			orderby ee.Layer
			select ee)
		{
			if (IsHelmetThatShouldBeHidden(item))
			{
				continue;
			}
			BodyPartType bodyPartType = (BodyPartType)0L;
			foreach (EquipmentEntity equipmentEntity2 in EquipmentEntities)
			{
				if (!(equipmentEntity2 == null) && !(item == equipmentEntity2) && !IsHelmetThatShouldBeHidden(equipmentEntity2))
				{
					bodyPartType |= equipmentEntity2.HideBodyParts;
				}
			}
			foreach (BodyPart bodyPart in item.BodyParts)
			{
				if (bodyPart != null && (bodyPartType & bodyPart.Type) == (BodyPartType)0L && !(bodyPart.SkinnedRenderer == null) && !(bodyPart.Material == null))
				{
					KeyValuePair<BodyPart, EquipmentEntity> keyValuePair = dictionary.FirstOrDefault((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Key.Type == bodyPart.Type);
					if (keyValuePair.Key != null)
					{
						dictionary.Remove(keyValuePair.Key);
					}
					dictionary[bodyPart] = item;
				}
			}
		}
		m_OverlayBodyParts = new List<BodyPart>();
		foreach (KeyValuePair<BodyPart, EquipmentEntity> item2 in dictionary)
		{
			var (bodyPart3, entity) = (KeyValuePair<BodyPart, EquipmentEntity>)(ref item2);
			if (entity.ShowLowerMaterials)
			{
				foreach (EquipmentEntity item3 in from ee in EquipmentEntities
					where ee != null && ee != entity && ee.Layer < entity.Layer && !IsHelmetThatShouldBeHidden(ee)
					orderby ee.Layer
					select ee)
				{
					AddBodyParts(m_OverlayBodyParts, bodyPart3.Type, item3);
				}
			}
			AddBodyParts(m_OverlayBodyParts, bodyPart3.Type, entity);
			foreach (EquipmentEntity item4 in from ee in EquipmentEntities
				where ee != null && ee != entity && ee.Layer > entity.Layer && !IsHelmetThatShouldBeHidden(ee)
				orderby ee.Layer
				select ee)
			{
				AddBodyParts(m_OverlayBodyParts, bodyPart3.Type, item4);
			}
		}
		if (m_OverlayBodyParts != null && m_OverlayBodyParts.Count > 0)
		{
			m_AtlasMaterial = AtlasMaterial;
			if (m_AtlasMaterial == null)
			{
				m_AtlasMaterial = new Material(m_OverlayBodyParts.FirstOrDefault((BodyPart x) => null != x.Material)?.Material);
			}
		}
		if (false || !LoadingProcess.Instance.IsLoadingInProcess)
		{
			MergeOverlays(m_OverlayBodyParts);
		}
		else
		{
			OverlaysMerged = false;
		}
		BuildMesh(dictionary);
		RebuildOutfit();
		SetUpCharacterRenderingLayerMask();
	}

	private void AddBodyParts(List<BodyPart> bodyParts, BodyPartType type, EquipmentEntity entity)
	{
		foreach (BodyPart bodyPart in entity.BodyParts)
		{
			if (bodyPart.Type != type)
			{
				continue;
			}
			bool flag = true;
			foreach (CharacterTextureDescription texture in bodyPart.Textures)
			{
				if (texture.GetSourceTexture() == null)
				{
					PFLog.Default.Error($"Missing texture in {type} body part in {entity} when merging overlays for {this}");
					flag = false;
					break;
				}
			}
			if (flag)
			{
				bodyParts.Add(bodyPart);
			}
		}
	}

	public void ClearSpawnedOutfit(List<OutfitPartInfo> outfitPartsListToClearAndDestroy)
	{
		foreach (OutfitPartInfo item in outfitPartsListToClearAndDestroy)
		{
			if (!IsInDollRoom || item.GameObject.GetComponent<MechadendriteSettings>() == null)
			{
				UnityEngine.Object.Destroy(item.GameObject);
			}
		}
		if (IsInDollRoom)
		{
			outfitPartsListToClearAndDestroy.RemoveAll((OutfitPartInfo x) => x.GameObject.GetComponent<MechadendriteSettings>() == null);
		}
		else
		{
			outfitPartsListToClearAndDestroy.Clear();
		}
	}

	public void ColorizeOutfitPart(GameObject newOutfitObject, EquipmentEntity ee, EquipmentEntity.OutfitPart outfitPart)
	{
		if (outfitPart.ColorMask == null)
		{
			return;
		}
		SelectedRampIndices selectedRampIndices = RampIndices.FirstOrDefault((SelectedRampIndices i) => i.EquipmentEntity == ee);
		if (selectedRampIndices == null)
		{
			return;
		}
		if (ee.PrimaryRamps.Count < selectedRampIndices.PrimaryIndex)
		{
			PFLog.TechArt.Error("Character " + base.gameObject.name + ". Can't find color ramp index " + selectedRampIndices.PrimaryIndex + " in EE: " + ee.name);
			return;
		}
		if (ee.SecondaryRamps.Count < selectedRampIndices.SecondaryIndex)
		{
			PFLog.TechArt.Error("Character " + base.gameObject.name + ". Can't find color ramp index " + selectedRampIndices.SecondaryIndex + " in EE: " + ee.name);
			return;
		}
		Renderer componentInChildren = newOutfitObject.GetComponentInChildren<Renderer>();
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("No renderer in " + newOutfitObject);
			return;
		}
		List<Material> list = new List<Material>();
		Shader equipmentColorizerShader = BlueprintRoot.Instance.CharGenRoot.EquipmentColorizerShader;
		Material[] sharedMaterials = componentInChildren.sharedMaterials;
		foreach (Material material in sharedMaterials)
		{
			Material material2 = new Material(equipmentColorizerShader);
			material2.SetTexture(ShaderProps._BaseMap, material.GetTexture(ShaderProps._BaseMap));
			material2.SetTexture(ShaderProps._BumpMap, material.GetTexture(ShaderProps._BumpMap));
			material2.SetTexture(ShaderProps._MasksMap, material.GetTexture(ShaderProps._MasksMap));
			material2.SetTexture(ShaderProps._ColorMask, outfitPart.ColorMask);
			material2.SetTexture(ShaderProps._Ramp1, ee.PrimaryRamps[selectedRampIndices.PrimaryIndex]);
			material2.SetTexture(ShaderProps._Ramp2, ee.SecondaryRamps[selectedRampIndices.SecondaryIndex]);
			material2.name = newOutfitObject.name + "_material";
			list.Add(material2);
		}
		if (list.Count > 0)
		{
			componentInChildren.sharedMaterials = list.ToArray();
		}
		ColorizedOutfitParts.Add(componentInChildren);
	}

	public void SetupCloakPhysics(GameObject newOutfitObject)
	{
		Cloth componentInChildren = newOutfitObject.GetComponentInChildren<Cloth>();
		PBDMeshBody componentInChildren2 = newOutfitObject.GetComponentInChildren<PBDMeshBody>();
		if ((bool)componentInChildren)
		{
			SetupClothCpuColliders(componentInChildren);
		}
		if ((bool)componentInChildren2)
		{
			SetupClothGpuColliders(componentInChildren2);
		}
	}

	public Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity> GetOutfitWithLayerAndTypeInCount(Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity> outfit)
	{
		List<EquipmentEntity.OutfitPartSpecialType> list = new List<EquipmentEntity.OutfitPartSpecialType>();
		Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity> dictionary = new Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity>();
		foreach (KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> item in outfit)
		{
			if (item.Key.Special == EquipmentEntity.OutfitPartSpecialType.None)
			{
				dictionary.Add(item.Key, item.Value);
			}
			else if (!list.Contains(item.Key.Special))
			{
				list.Add(item.Key.Special);
			}
		}
		foreach (EquipmentEntity.OutfitPartSpecialType item2 in list)
		{
			int num = 0;
			EquipmentEntity.OutfitPart key = null;
			EquipmentEntity value = null;
			foreach (KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> item3 in outfit)
			{
				if (item3.Key.Special == item2 && item3.Value.Layer >= num)
				{
					num = item3.Value.Layer;
					key = item3.Key;
					value = item3.Value;
				}
			}
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	public void RebuildOutfit()
	{
		if ((bool)BakedCharacter)
		{
			return;
		}
		ClearSpawnedOutfit(m_OutfitObjectsSpawned);
		Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity> dictionary = new Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (EquipmentEntity equipmentEntity in EquipmentEntities)
		{
			if (equipmentEntity == null)
			{
				continue;
			}
			foreach (EquipmentEntity.OutfitPart outfitPart in equipmentEntity.OutfitParts)
			{
				if (outfitPart != null && (outfitPart.StaysInPeacefulMode || !PeacefulMode) && (!outfitPart.OnlyInDollRoom || IsInDollRoom))
				{
					if (outfitPart.Special == EquipmentEntity.OutfitPartSpecialType.Cloak)
					{
						flag2 = true;
					}
					if (outfitPart.Special == EquipmentEntity.OutfitPartSpecialType.CloakSquashed)
					{
						flag3 = true;
					}
					if (outfitPart.Special == EquipmentEntity.OutfitPartSpecialType.Backpack)
					{
						flag = true;
					}
					dictionary.Add(outfitPart, equipmentEntity);
				}
			}
		}
		if (flag && flag2)
		{
			dictionary = dictionary.Where((KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit) => outfit.Key.Special != EquipmentEntity.OutfitPartSpecialType.Cloak).ToDictionary((KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit) => outfit.Key, (KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit) => outfit.Value);
		}
		if (!flag && flag2 && flag3)
		{
			dictionary = dictionary.Where((KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit) => outfit.Key.Special != EquipmentEntity.OutfitPartSpecialType.CloakSquashed).ToDictionary((KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit) => outfit.Key, (KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit) => outfit.Value);
		}
		Dictionary<EquipmentEntity.OutfitPart, EquipmentEntity> outfitWithLayerAndTypeInCount = GetOutfitWithLayerAndTypeInCount(dictionary);
		ColorizedOutfitParts.Clear();
		foreach (KeyValuePair<EquipmentEntity.OutfitPart, EquipmentEntity> outfit in outfitWithLayerAndTypeInCount)
		{
			if (IsInDollRoom && m_OutfitObjectsSpawned.Contains((OutfitPartInfo x) => x.OutfitPart == outfit.Key))
			{
				continue;
			}
			(GameObject, Transform) tuple = outfit.Key.Attach(base.transform, m_AttachBonesCache);
			if (!(tuple.Item1 == null))
			{
				if (!m_AttachBonesCache.ContainsKey(tuple.Item2.name))
				{
					m_AttachBonesCache.Add(tuple.Item2.name, tuple.Item2);
				}
				if (outfit.Key.Special == EquipmentEntity.OutfitPartSpecialType.Cloak || outfit.Key.Special == EquipmentEntity.OutfitPartSpecialType.CloakSquashed)
				{
					SetupCloakPhysics(tuple.Item1);
				}
				ColorizeOutfitPart(tuple.Item1, outfit.Value, outfit.Key);
				m_OutfitObjectsSpawned.Add(new OutfitPartInfo(outfit.Key, tuple.Item1, outfit.Value));
			}
		}
		FilterOutfit();
		if (IsInDollRoom)
		{
			this.OnUpdated?.Invoke(this);
		}
	}

	public void SetupClothCpuColliders(Cloth cloth)
	{
		List<CapsuleCollider> list = new List<CapsuleCollider>();
		List<SphereCollider> list2 = new List<SphereCollider>();
		List<SphereCollider> list3 = new List<SphereCollider>();
		ClothSphereColliderPair item = default(ClothSphereColliderPair);
		ClothSphereColliderPair item2 = default(ClothSphereColliderPair);
		List<ClothSphereColliderPair> list4 = new List<ClothSphereColliderPair>();
		ClothCollider[] clothColliders = ClothColliders;
		foreach (ClothCollider clothCollider in clothColliders)
		{
			if ((bool)clothCollider && (bool)clothCollider.clothColliderCpu)
			{
				switch (clothCollider.bodyPartType)
				{
				case ClothCollider.ClothColliderBodyPartType.Body:
					list.Add(clothCollider.clothColliderCpu as CapsuleCollider);
					break;
				case ClothCollider.ClothColliderBodyPartType.LeftArm:
				case ClothCollider.ClothColliderBodyPartType.RightArm:
					list2.Add(clothCollider.clothColliderCpu as SphereCollider);
					break;
				case ClothCollider.ClothColliderBodyPartType.LeftLeg:
				case ClothCollider.ClothColliderBodyPartType.RightLeg:
					list3.Add(clothCollider.clothColliderCpu as SphereCollider);
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			cloth.capsuleColliders = list.ToArray();
		}
		if (list2.Count > 1)
		{
			item.first = list2[0];
			item.second = list2[1];
			list4.Add(item);
		}
		if (list3.Count > 1)
		{
			item2.first = list3[0];
			item2.second = list3[1];
			list4.Add(item2);
		}
		if (list4.Count > 0)
		{
			cloth.sphereColliders = list4.ToArray();
		}
	}

	public void SetupClothGpuColliders(PBDMeshBody cloth)
	{
		ClothCollider[] clothColliders = ClothColliders;
		foreach (ClothCollider clothCollider in clothColliders)
		{
			if ((bool)clothCollider && (bool)clothCollider.clothColliderGpu)
			{
				cloth.LocalColliders.Add(clothCollider.clothColliderGpu);
			}
		}
	}

	public void FilterOutfit(Func<EquipmentEntity.OutfitPart, GameObject, bool> filter = null)
	{
		if ((bool)BakedCharacter)
		{
			return;
		}
		OutfitFilter = filter ?? OutfitFilter;
		foreach (OutfitPartInfo item in m_OutfitObjectsSpawned)
		{
			if (!(item.GameObject == null))
			{
				item.GameObject.SetActive(OutfitFilter?.Invoke(item.OutfitPart, item.GameObject) ?? true);
			}
		}
	}

	public void ResetCloth(Cloth cloth)
	{
		if ((bool)cloth)
		{
			cloth.ClearTransformMotion();
		}
	}

	public void UpdateHelmetVisibility(bool showHelmet)
	{
		if (m_ShowHelmet == !showHelmet)
		{
			IsDirty = true;
			m_ShowHelmet = showHelmet;
		}
	}

	public void UpdateClothVisibility(bool showCloth)
	{
		if (m_ShowCloth == !showCloth)
		{
			IsDirty = true;
			m_ShowCloth = showCloth;
		}
	}

	public void UpdateBackpackVisibility(bool showBackpack)
	{
		if (m_ShowBackpack == !showBackpack)
		{
			IsDirty = true;
			m_ShowBackpack = showBackpack;
			m_BackEquipmentIsDirty = true;
		}
	}

	private void ClearAtlases()
	{
		foreach (CharacterAtlas atlase in m_Atlases)
		{
			if (atlase.AtlasTexture != null)
			{
				if (atlase.AtlasTexture.CompressionComplete)
				{
					Texture2D texture = atlase.AtlasTexture.Texture;
					if ((bool)texture)
					{
						UnityEngine.Object.Destroy(texture);
					}
				}
				else
				{
					atlase.AtlasTexture.Destroyed = true;
				}
			}
			else
			{
				atlase.Destroyed = true;
			}
		}
		m_Atlases.Clear();
	}

	private void ClearMeshes()
	{
		if (BakedCharacter != null)
		{
			return;
		}
		foreach (SkinnedMeshRenderer renderer in Renderers)
		{
			if (renderer != null)
			{
				UnityEngine.Object.Destroy(renderer.sharedMesh);
				UnityEngine.Object.Destroy(renderer.gameObject);
			}
		}
	}

	private void BuildMesh(Dictionary<BodyPart, EquipmentEntity> geometryBodyParts)
	{
		ClearMeshes();
		Renderers.Clear();
		m_AtlasRenderer = null;
		List<Transform> list = new List<Transform>();
		List<BoneWeight> list2 = new List<BoneWeight>();
		List<Matrix4x4> list3 = new List<Matrix4x4>();
		Dictionary<string, Transform> cachedBones = CacheHierarchy();
		if (!(m_AtlasMaterial != null))
		{
			return;
		}
		List<CombineInstance> list4 = new List<CombineInstance>();
		List<Vector2> list5 = new List<Vector2>();
		GameObject obj = new GameObject("Renderer_" + m_AtlasMaterial.name);
		obj.transform.parent = Animator.transform;
		obj.transform.localPosition = default(Vector3);
		obj.transform.localScale = Vector3.one;
		obj.transform.localRotation = Quaternion.identity;
		SkinnedMeshRenderer skinnedMeshRenderer = obj.AddComponent<SkinnedMeshRenderer>();
		Mesh mesh = new Mesh
		{
			name = "Character"
		};
		mesh.Clear();
		foreach (KeyValuePair<BodyPart, EquipmentEntity> geometryBodyPart in geometryBodyParts)
		{
			SkinnedMeshRenderer skinnedRenderer = geometryBodyPart.Key.SkinnedRenderer;
			if (!(skinnedRenderer == null))
			{
				if (skinnedMeshRenderer.rootBone == null && skinnedRenderer.rootBone != null)
				{
					skinnedMeshRenderer.rootBone = Animator.transform;
				}
				int[] bonesMapping = new int[skinnedRenderer.sharedMesh.bindposes.Length];
				EnsureBones(geometryBodyPart.Key, list, list3, bonesMapping, cachedBones);
				Vector2[] uv = skinnedRenderer.sharedMesh.uv;
				foreach (Vector2 item in uv)
				{
					list5.Add(item);
				}
				CombineInstance item2 = default(CombineInstance);
				item2.mesh = skinnedRenderer.sharedMesh;
				item2.transform = Matrix4x4.identity;
				InsertBoneWeights(list2, bonesMapping, skinnedRenderer);
				list4.Add(item2);
			}
		}
		mesh.CombineMeshes(list4.ToArray());
		mesh.bindposes = list3.ToArray();
		mesh.boneWeights = list2.ToArray();
		mesh.uv = list5.ToArray();
		mesh.RecalculateBounds();
		mesh.UploadMeshData(markNoLongerReadable: true);
		skinnedMeshRenderer.sharedMesh = mesh;
		skinnedMeshRenderer.bones = list.ToArray();
		m_AtlasRenderer = skinnedMeshRenderer;
		Renderers.Add(skinnedMeshRenderer);
		skinnedMeshRenderer.gameObject.layer = 9;
		skinnedMeshRenderer.sharedMaterial = m_AtlasMaterial;
		Animator.Rebind();
	}

	internal Dictionary<string, Transform> CacheHierarchy()
	{
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(base.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			if (!dictionary.ContainsKey(transform.name))
			{
				dictionary.Add(transform.name, transform);
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				stack.Push(child);
			}
		}
		return dictionary;
	}

	private void ValidateDuplicateNames(ValidationContext context, int parentIndex)
	{
		HashSet<string> names = new HashSet<string>();
		AnimatorPrefab.gameObject.ForAllChildren(delegate(GameObject o)
		{
			if (names.Contains(o.name))
			{
				context.CreateChild(o.name, ValidationNodeType.Object, parentIndex, o.activeInHierarchy).AddError("Object has duplicate bone in animator: " + o.name);
			}
			names.Add(o.name);
		});
	}

	private void MergeOverlays(List<BodyPart> overlayBodyParts)
	{
		if (overlayBodyParts.Count == 0)
		{
			return;
		}
		int atlasSize;
		if (true)
		{
			int b = 512;
			atlasSize = Mathf.Max(2048 >> QualitySettings.globalTextureMipmapLimit, b);
		}
		else
		{
			atlasSize = (int)MaxAtlasSize;
		}
		foreach (EquipmentEntity ee in EquipmentEntities)
		{
			SelectedRampIndices selectedRampIndices = RampIndices.Find((SelectedRampIndices x) => x.EquipmentEntity == ee);
			if (selectedRampIndices != null)
			{
				ee.RepaintTextures(m_EquipmentEntitiesTextures, selectedRampIndices.PrimaryIndex, selectedRampIndices.SecondaryIndex);
			}
			else
			{
				ee.RepaintTextures(m_EquipmentEntitiesTextures, 0, 0);
			}
		}
		if (AtlasMaterial == null)
		{
			m_AtlasMaterial = new Material(overlayBodyParts.FirstOrDefault((BodyPart x) => x.Material != null)?.Material);
		}
		CharacterAtlas atlas = GetAtlas(atlasSize, CharacterTextureChannel.Diffuse);
		CharacterAtlas atlas2 = GetAtlas(atlasSize, CharacterTextureChannel.Normal);
		CharacterAtlas atlas3 = GetAtlas(atlasSize, CharacterTextureChannel.Masks);
		atlas.RefreshData();
		atlas2.RefreshData();
		atlas3.RefreshData();
		foreach (BodyPart overlayBodyPart in overlayBodyParts)
		{
			foreach (CharacterTextureDescription texture in overlayBodyPart.Textures)
			{
				Texture2D diffuseTexture = texture.DiffuseTexture;
				if (texture.DiffuseTexture != null)
				{
					atlas.AddPrimaryTexture(texture, overlayBodyPart.Type);
				}
				if (texture.NormalTexture != null)
				{
					atlas2.AddSecondaryTexture(texture, diffuseTexture, overlayBodyPart.Type, overlayBodyPart.Material);
				}
				if (texture.MaskTexture != null)
				{
					atlas3.AddSecondaryTexture(texture, diffuseTexture, overlayBodyPart.Type, overlayBodyPart.Material);
				}
			}
		}
		foreach (CharacterAtlas atlase in m_Atlases)
		{
			atlase.Build(m_EquipmentEntitiesTextures, AtlasMaterial, cleanAtlas: true, delayTextureCreation: true);
		}
		Services.GetInstance<CharacterAtlasService>().QueueAtlasRebuild(m_Atlases, AtlasMaterial, OnAtlasCompressed, OnAtlasNotCompressed, base.name);
		m_EquipmentEntitiesTextures.Clear();
		IsAtlasesDirty = false;
	}

	private void OnAtlasNotCompressed(CharacterAtlas atlas)
	{
		OverlaysMerged = true;
	}

	private void OnAtlasCompressed(CharacterAtlas atlas, Texture2D tex)
	{
		if (!(this == null) && !(base.gameObject == null))
		{
			atlas.UpdateMaterial(AtlasMaterial, tex);
			OverlaysMerged = true;
			StandardMaterialController component = base.gameObject.GetComponent<StandardMaterialController>();
			if (component != null)
			{
				component.InvalidateMaterialsTextures();
			}
		}
	}

	private CharacterAtlas GetAtlas(int atlasSize, CharacterTextureChannel channel)
	{
		CharacterAtlas characterAtlas = m_Atlases.FirstOrDefault((CharacterAtlas a) => a.Channel == channel);
		if (characterAtlas == null)
		{
			characterAtlas = new CharacterAtlas(atlasSize, channel, AtlasData);
			m_Atlases.Add(characterAtlas);
		}
		return characterAtlas;
	}

	private void EnsureBones(BodyPart bodyPart, List<Transform> bones, List<Matrix4x4> bindposes, int[] bonesMapping, Dictionary<string, Transform> cachedBones)
	{
		Matrix4x4[] bindposes2 = bodyPart.SkinnedRenderer.sharedMesh.bindposes;
		Transform[] bones2 = bodyPart.SkinnedRenderer.bones;
		for (int i = 0; i < bones2.Length; i++)
		{
			Transform transform = bones2[i];
			int num = -1;
			if (!cachedBones.TryGetValue(transform.name, out var value))
			{
				break;
			}
			for (int j = 0; j < bones.Count; j++)
			{
				if (CompareSkinningMatrices(bindposes[j], ref bindposes2[i]) && bones2[i].transform.name == bones[j].name)
				{
					num = j;
					break;
				}
			}
			if (num < 0)
			{
				num = bones.Count;
				bones.Add(value);
				bindposes.Add(bindposes2[i]);
			}
			bonesMapping[i] = num;
		}
	}

	private static bool CompareSkinningMatrices(Matrix4x4 m1, ref Matrix4x4 m2)
	{
		if ((double)Mathf.Abs(m1.m00 - m2.m00) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m01 - m2.m01) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m02 - m2.m02) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m03 - m2.m03) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m10 - m2.m10) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m11 - m2.m11) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m12 - m2.m12) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m13 - m2.m13) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m20 - m2.m20) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m21 - m2.m21) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m22 - m2.m22) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m23 - m2.m23) > 0.0001)
		{
			return false;
		}
		return true;
	}

	private void InsertBoneWeights(List<BoneWeight> boneWeights, int[] bonesMapping, SkinnedMeshRenderer renderer)
	{
		BoneWeight[] boneWeights2 = renderer.sharedMesh.boneWeights;
		for (int i = 0; i < boneWeights2.Length; i++)
		{
			BoneWeight item = boneWeights2[i];
			item.boneIndex0 = bonesMapping[item.boneIndex0];
			item.boneIndex1 = bonesMapping[item.boneIndex1];
			item.boneIndex2 = bonesMapping[item.boneIndex2];
			item.boneIndex3 = bonesMapping[item.boneIndex3];
			boneWeights.Add(item);
		}
	}

	public static Transform FindBone(Transform parent, string name)
	{
		if (parent.name == name)
		{
			return parent;
		}
		int childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform transform = FindBone(parent.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public void PrepareBake()
	{
	}

	public void BindBakedCharacter()
	{
		if (!(BakedCharacter != null) || !(Animator != null))
		{
			return;
		}
		foreach (SkinnedMeshRenderer renderer in Renderers)
		{
			if (null != renderer)
			{
				Utils.EditorSafeDestroy(renderer.gameObject);
			}
		}
		Renderers.Clear();
		m_AtlasRenderer = null;
		LoadBakedCharacter();
	}

	public void SetUpCharacterRenderingLayerMask()
	{
		byte b = 1;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			uint renderingLayerMask = renderer.renderingLayerMask;
			uint num = (uint)(((renderingLayerMask & (1 << b - 1)) == 0L) ? renderingLayerMask : (renderingLayerMask ^ (1 << b - 1)));
			if (num > 254)
			{
				num &= 0xFEu;
			}
			if (num == 0)
			{
				renderer.renderingLayerMask = (byte)DefaultRenderingLayer;
			}
			else
			{
				renderer.renderingLayerMask = num;
			}
			CurrentLayer = renderer.renderingLayerMask;
		}
	}

	private static bool IsHelmetType(BodyPart bodyPart)
	{
		return bodyPart.Type switch
		{
			BodyPartType.Helmet => true, 
			BodyPartType.MaskTop => true, 
			BodyPartType.MaskBottom => true, 
			BodyPartType.Goggles => true, 
			_ => false, 
		};
	}
}
