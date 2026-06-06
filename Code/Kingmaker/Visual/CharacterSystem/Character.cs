using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Utility;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using RogueTrader.Code.ShaderConsts;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.CharacterSystem;

[KnowledgeDatabaseID("49ec7da2c03301e4ca927c5c1a2e00ed")]
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

		public Material[] OwnedMaterials;

		public OutfitPartInfo(EquipmentEntity.OutfitPart outfitPart, GameObject gameObject, EquipmentEntity ee, Material[] ownedMaterials)
		{
			OutfitPart = outfitPart;
			GameObject = gameObject;
			Ee = ee;
			OwnedMaterials = ownedMaterials;
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

	private List<BodyPart> m_AugOverlayBodyParts;

	private readonly List<CharacterAtlas> m_Atlases = new List<CharacterAtlas>();

	private SkinnedMeshRenderer m_AtlasRenderer;

	private Material m_AtlasMaterial;

	private Material m_AugmentationMaterial;

	private readonly HashSet<Skeleton.Bone> m_EquipmentBoneModifiers = new HashSet<Skeleton.Bone>();

	private BoneUpdateJob m_BoneUpdateJob;

	private TransformAccessArray m_BonesForJob;

	private NativeArray<Skeleton.BoneData> m_FilteredBoneDataForJob;

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

	public AnimationSet OverrideAnimationSet;

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

	[Tooltip("Отключает создание и запекание текстурных атласов для быстрой сборки только геометрии")]
	public bool makeTextures = true;

	public bool SaveRagdoll;

	public AtlasSize MaxAtlasSize = AtlasSize.AtlasSize2048;

	public BakedCharacter BakedCharacter;

	[SerializeField]
	private CharacterBonesList m_BonesList;

	[SerializeField]
	private List<EquipmentEntityLink> m_SavedEquipmentEntities = new List<EquipmentEntityLink>();

	private List<EquipmentEntity> m_SavedBeforeCutsceneEquipment = new List<EquipmentEntity>();

	private List<SelectedRampIndices> m_SavedBeforeCutsceneRampIndices = new List<SelectedRampIndices>();

	private bool? m_SavedBeforeCutsceneShowHelmAboveAll;

	[SerializeField]
	public List<SavedSelectedRampIndices> m_SavedRampIndices = new List<SavedSelectedRampIndices>();

	private readonly EquipmentEntity.PaintedTextures m_EquipmentEntitiesTextures = new EquipmentEntity.PaintedTextures();

	private bool m_PeacefulMode;

	private bool m_ShowHelmet = true;

	private bool m_ShowCloth = true;

	private bool m_ShowBackpack = true;

	private bool m_ShowHelmetAboveAll;

	private bool m_ShowGloves = true;

	private bool m_ShowBoots = true;

	private bool m_ShowArmor = true;

	private bool m_BackEquipmentIsDirty;

	public Func<EquipmentEntity.OutfitPart, GameObject, bool> OutfitFilter;

	public HashSet<UnitAnimationManager> MechsAnimationManagers = new HashSet<UnitAnimationManager>();

	private Dictionary<EquipmentEntity, ItemSlot> m_EquipmentEntityToSlot = new Dictionary<EquipmentEntity, ItemSlot>();

	private BaseUnitEntity m_SourceUnit;

	public List<EquipmentEntityLink> EquipmentEntitiesForPreload = new List<EquipmentEntityLink>();

	[SerializeField]
	private RenderingLayerEnum m_DefaultRenderingLayer = RenderingLayerEnum.RenderingLayer2;

	[HideInInspector]
	public uint CurrentLayer;

	public bool canNotBeRebaked;

	public ClothCollider[] ClothColliders;

	private EquipmentEntity m_AlwaysVisibleHelmetEe;

	private EquipmentEntity m_ProxyHelmetEe;

	private List<EquipmentEntity> m_ProxyEquipmentEntities = new List<EquipmentEntity>();

	private Dictionary<string, Transform> m_AttachBonesCache = new Dictionary<string, Transform>();

	private const BodyPartType LegCoverageMask = BodyPartType.Feet | BodyPartType.KneeCops | BodyPartType.LowerLegs;

	private static readonly BodyPartType[] _AuxArmTypes = new BodyPartType[1] { BodyPartType.LowerArmsExtra };

	public bool OverlaysMerged { get; private set; } = true;


	public Material AtlasMaterial
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

	public AugmentationAtlasController AugmentationAtlas { get; private set; }

	public IReadOnlyList<OutfitPartInfo> OutfitObjectsSpawned => m_OutfitObjectsSpawned;

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

	public bool HasBonesList => m_BonesList != null;

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

	public bool? SavedBeforeCutsceneShowHelmAboveAll
	{
		get
		{
			return m_SavedBeforeCutsceneShowHelmAboveAll;
		}
		set
		{
			m_SavedBeforeCutsceneShowHelmAboveAll = value;
		}
	}

	public UnitAnimationManager AnimationManager { get; private set; }

	public List<EquipmentEntity> EquipmentEntities { get; } = new List<EquipmentEntity>();


	public HashSet<EquipmentEntity> EquippedItemsEntities { get; } = new HashSet<EquipmentEntity>();


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
			if (OverrideAnimationSet != null)
			{
				AnimationSet = OverrideAnimationSet;
			}
			AnimationManager.AnimationSet = AnimationSet;
		}
		m_BonesList = Animator.EnsureComponent<CharacterBonesList>();
		if (m_BonesList != null)
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
				if (m_AtlasRenderer.sharedMesh != null)
				{
					UnityEngine.Object.Destroy(m_AtlasRenderer.sharedMesh);
				}
				UnityEngine.Object.Destroy(m_AtlasRenderer.gameObject);
			}
			UnityEngine.Object.Destroy(m_AtlasMaterial);
			m_AtlasMaterial = null;
		}
		if (m_AugmentationMaterial != null)
		{
			UnityEngine.Object.Destroy(m_AugmentationMaterial);
			m_AugmentationMaterial = null;
		}
		AugmentationAtlas?.Dispose();
		AugmentationAtlas = null;
		ClearAtlases();
		ClearMeshes();
		foreach (OutfitPartInfo item in m_OutfitObjectsSpawned)
		{
			DestroyOwnedOutfitMaterials(item);
			if (item?.GameObject != null)
			{
				UnityEngine.Object.Destroy(item.GameObject);
			}
		}
		m_OutfitObjectsSpawned.Clear();
		m_EquipmentEntitiesTextures.Clear();
		if (m_BonesForJob.isCreated)
		{
			m_BonesForJob.Dispose();
		}
		if (m_FilteredBoneDataForJob.IsCreated)
		{
			m_FilteredBoneDataForJob.Dispose();
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
					ProbeAnchorOverrider.UpdateProbeAnchorsOnObject(base.gameObject, Renderers);
				}
				finally
				{
					IsDirty = false;
				}
			}
			if (!OverlaysMerged && m_OverlayBodyParts != null && Services.GetInstance<CharacterAtlasService>().RequestsCount == 0 && Services.GetInstance<DxtCompressorServiceNew>().RequestsCount == 0)
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
			atlase.Build(m_EquipmentEntitiesTextures, AtlasMaterial, cleanAtlas: true);
		}
		StandardMaterialController component = base.gameObject.GetComponent<StandardMaterialController>();
		if (component != null)
		{
			component.InvalidateMaterialsTextures();
		}
		Services.GetInstance<CharacterAtlasService>().QueueAtlasRebuild(this, m_Atlases, OnAtlasCompressed, OnAtlasNotCompressed, base.name);
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
			if (primaryRampIndex.HasValue && selectedRampIndices.PrimaryIndex != primaryRampIndex)
			{
				selectedRampIndices.PrimaryIndex = primaryRampIndex.Value;
			}
			if (secondaryRampIndex.HasValue && selectedRampIndices.SecondaryIndex != secondaryRampIndex)
			{
				selectedRampIndices.SecondaryIndex = secondaryRampIndex.Value;
			}
		}
		else
		{
			SelectedRampIndices item = new SelectedRampIndices
			{
				EquipmentEntity = ee,
				PrimaryIndex = primaryRampIndex.GetValueOrDefault(),
				SecondaryIndex = secondaryRampIndex.GetValueOrDefault()
			};
			RampIndices.Add(item);
		}
		IsAtlasesDirty = true;
		UpdateColorizedOutfitRamps(ee);
	}

	public void CacheSkeletonBones()
	{
		if (m_BonesForJob.isCreated)
		{
			m_BonesForJob.Dispose();
		}
		if (m_FilteredBoneDataForJob.IsCreated)
		{
			m_FilteredBoneDataForJob.Dispose();
		}
		if (m_BonesList == null)
		{
			PFLog.Default.Error(base.gameObject, base.gameObject?.name + ": m_BonesList is null, Character.OnStart() has not been called or failed?");
			m_BonesForJob = new TransformAccessArray(0);
			m_FilteredBoneDataForJob = new NativeArray<Skeleton.BoneData>(0, Allocator.Persistent);
			m_BoneUpdateJob = new BoneUpdateJob
			{
				Scales = m_FilteredBoneDataForJob
			};
			m_SkeletonChanged = false;
			return;
		}
		NativeArray<Skeleton.BoneData> boneData = Skeleton.GetBoneData();
		List<Transform> list = new List<Transform>(Skeleton.Bones.Count);
		List<Skeleton.BoneData> list2 = new List<Skeleton.BoneData>(Skeleton.Bones.Count);
		for (int i = 0; i < Skeleton.Bones.Count; i++)
		{
			Transform byName = m_BonesList.GetByName(Skeleton.Bones[i].Name);
			if (byName != null)
			{
				list.Add(byName);
				list2.Add(boneData[i]);
			}
		}
		m_BonesForJob = new TransformAccessArray(list.ToArray());
		m_FilteredBoneDataForJob = new NativeArray<Skeleton.BoneData>(list2.Count, Allocator.Persistent);
		for (int j = 0; j < list2.Count; j++)
		{
			m_FilteredBoneDataForJob[j] = list2[j];
		}
		m_BoneUpdateJob = new BoneUpdateJob
		{
			Scales = m_FilteredBoneDataForJob
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
		AddEquipmentEntity(ee, saved, isFromEquippedItem: false);
	}

	public void AddEquipmentEntity(EquipmentEntity ee, bool saved, bool isFromEquippedItem, ItemSlot sourceSlot = null)
	{
		if (ee == null)
		{
			return;
		}
		if (isFromEquippedItem)
		{
			EquippedItemsEntities.Add(ee);
			if (sourceSlot != null)
			{
				m_EquipmentEntityToSlot[ee] = sourceSlot;
				PFLog.TechArt.Log("[AddEquipmentEntity] EE slot-map: name='" + ee?.name + "', slot='" + sourceSlot?.GetType().Name + "'");
			}
			else
			{
				PFLog.TechArt.Log("[AddEquipmentEntity] EE marked as equipped but NO sourceSlot: name='" + ee?.name + "'");
			}
		}
		if (base.name.Contains("Pregen") && !ee.name.ToLower().Contains("head") && !EquipmentEntities.Any((EquipmentEntity existing) => existing.name.ToLower().Contains("head")))
		{
			UnitEntityView unitEntityView = GetComponentInParent<UnitEntityView>();
			if (unitEntityView == null)
			{
				unitEntityView = GetComponent<UnitEntityView>();
			}
			if (unitEntityView == null)
			{
				unitEntityView = base.transform.root.GetComponentInChildren<UnitEntityView>();
			}
			if (unitEntityView?.EntityData != null)
			{
				PregenDollSettings component = unitEntityView.EntityData.Blueprint.GetComponent<PregenDollSettings>();
				if (component?.Default?.Head != null)
				{
					EquipmentEntity equipmentEntity = component.Default.Head.Load();
					if (equipmentEntity != null && !EquipmentEntities.Contains(equipmentEntity))
					{
						EquipmentEntities.Add(equipmentEntity);
					}
				}
			}
		}
		if (!EquipmentEntities.Contains(ee))
		{
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
		if (isFromEquippedItem)
		{
			EquippedItemsEntities.Add(ee);
			if (sourceSlot != null)
			{
				m_EquipmentEntityToSlot[ee] = sourceSlot;
			}
		}
	}

	public void AddEquipmentEntities(IEnumerable<EquipmentEntity> ees, bool saved = false)
	{
		AddEquipmentEntities(ees, saved, isFromEquippedItems: false);
	}

	public void AddEquipmentEntities(IEnumerable<EquipmentEntity> ees, bool saved, bool isFromEquippedItems, ItemSlot sourceSlot = null)
	{
		using (ProfileScope.NewScope("AddEquipmentEntities"))
		{
			ees.ForEach(delegate(EquipmentEntity ee)
			{
				AddEquipmentEntity(ee, saved, isFromEquippedItems, sourceSlot);
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
		bool flag = EquipmentEntities.Remove(ee);
		IsDirty |= flag;
		EquippedItemsEntities.Remove(ee);
		m_EquipmentEntityToSlot.Remove(ee);
	}

	public void RemoveAllEquipmentEntities(bool saved = false)
	{
		IsDirty |= EquipmentEntities.Any();
		EquipmentEntities.Clear();
		EquippedItemsEntities.Clear();
		m_EquipmentEntityToSlot.Clear();
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

	public void SetSourceUnit(BaseUnitEntity unit)
	{
		m_SourceUnit = unit;
	}

	public void CopyEquipmentFrom(Character originalAvatar)
	{
		RemoveAllEquipmentEntities();
		AddEquipmentEntities(originalAvatar.EquipmentEntities);
		AddEquipmentEntities(originalAvatar.m_SavedEquipmentEntities.Select((EquipmentEntityLink eel) => eel.Load()), saved: true);
		foreach (EquipmentEntity equippedItemsEntity in originalAvatar.EquippedItemsEntities)
		{
			EquippedItemsEntities.Add(equippedItemsEntity);
		}
		foreach (KeyValuePair<EquipmentEntity, ItemSlot> item in originalAvatar.m_EquipmentEntityToSlot)
		{
			m_EquipmentEntityToSlot[item.Key] = item.Value;
		}
		CopyRampIndicesFrom(originalAvatar);
		m_ShowBackpack = originalAvatar.m_ShowBackpack;
		m_ShowHelmet = originalAvatar.m_ShowHelmet;
		m_ShowCloth = originalAvatar.m_ShowCloth;
		m_ShowGloves = originalAvatar.m_ShowGloves;
		m_ShowBoots = originalAvatar.m_ShowBoots;
		m_ShowArmor = originalAvatar.m_ShowArmor;
		m_ShowHelmetAboveAll = originalAvatar.m_ShowHelmetAboveAll;
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
		root = ObjectExtensions.Or(root, Animator.transform);
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
		return false;
	}

	private bool ShouldHideEquipmentEntity(EquipmentEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity.CantBeHiddenByDollRoom)
		{
			return false;
		}
		if (m_EquipmentEntityToSlot.TryGetValue(entity, out var value))
		{
			return ShouldHideSlot(value);
		}
		if (entity.ShowAboveAllIgnoreLayer && !m_ShowHelmetAboveAll)
		{
			return true;
		}
		return false;
	}

	private bool ShouldHideSlot(ItemSlot slot)
	{
		if (slot == null)
		{
			return false;
		}
		PartUnitBody partUnitBody = null;
		if (m_SourceUnit?.Body != null)
		{
			partUnitBody = m_SourceUnit.Body;
		}
		else
		{
			UnitEntityView componentInParent = GetComponentInParent<UnitEntityView>();
			if (componentInParent?.EntityData?.Body != null)
			{
				partUnitBody = componentInParent.EntityData.Body;
			}
		}
		if (partUnitBody == null)
		{
			return false;
		}
		if (!m_ShowArmor && slot == partUnitBody.Armor)
		{
			return true;
		}
		if (!m_ShowHelmet && slot == partUnitBody.Head)
		{
			return true;
		}
		if (!m_ShowGloves && slot == partUnitBody.Gloves)
		{
			return true;
		}
		if (!m_ShowBoots && slot == partUnitBody.Feet)
		{
			return true;
		}
		return false;
	}

	private bool ShouldHideBodyPartFromEquippedItem(BodyPart bodyPart, EquipmentEntity fromEntity)
	{
		if (!EquippedItemsEntities.Contains(fromEntity))
		{
			return false;
		}
		if (fromEntity.CantBeHiddenByDollRoom)
		{
			return false;
		}
		if (!m_ShowGloves && IsGlovesType(bodyPart))
		{
			return true;
		}
		if (!m_ShowBoots && IsBootsType(bodyPart))
		{
			return true;
		}
		if (!m_ShowArmor && IsArmorType(bodyPart))
		{
			return true;
		}
		return false;
	}

	private void SetAlwaysVisibleHelmetProxyEe()
	{
		m_AlwaysVisibleHelmetEe = null;
		m_ProxyHelmetEe = null;
		foreach (EquipmentEntity equipmentEntity in EquipmentEntities)
		{
			if (equipmentEntity != null && equipmentEntity.ShowAboveAllIgnoreLayer)
			{
				m_AlwaysVisibleHelmetEe = equipmentEntity;
				break;
			}
		}
	}

	private EquipmentEntity CreateProxyHeadwearEe(EquipmentEntity ee)
	{
		EquipmentEntity equipmentEntity = ScriptableObject.CreateInstance<EquipmentEntity>();
		equipmentEntity.CantBeHiddenByDollRoom = ee.CantBeHiddenByDollRoom;
		equipmentEntity.ShowAboveAllIgnoreLayer = ee.ShowAboveAllIgnoreLayer;
		equipmentEntity.Layer = 999;
		equipmentEntity.HideBodyParts = ee.HideBodyParts;
		equipmentEntity.ShowLowerMaterials = ee.ShowLowerMaterials;
		equipmentEntity.SkeletonModifiers = ee.SkeletonModifiers;
		if (ee.PrimaryColorsProfile != null)
		{
			equipmentEntity.PrimaryColorsProfile = ee.PrimaryColorsProfile;
			List<Texture2D> primaryRamps = ee.PrimaryRamps;
			if (primaryRamps != null && primaryRamps.Count > 0)
			{
				equipmentEntity.PrimaryRamps = ee.PrimaryRamps.ToList();
			}
		}
		if (ee.SecondaryColorsProfile != null)
		{
			equipmentEntity.SecondaryColorsProfile = ee.SecondaryColorsProfile;
			List<Texture2D> primaryRamps = ee.SecondaryRamps;
			if (primaryRamps != null && primaryRamps.Count > 0)
			{
				equipmentEntity.SecondaryRamps = ee.SecondaryRamps.ToList();
			}
		}
		if (ee.ColorPresets != null)
		{
			equipmentEntity.ColorPresets = ee.ColorPresets;
		}
		equipmentEntity.BodyParts = ee.BodyParts;
		equipmentEntity.OutfitParts = ee.OutfitParts;
		equipmentEntity.ForcedPrimaryIndex = ee.ForcedPrimaryIndex;
		equipmentEntity.ForcedSecondaryIndex = ee.ForcedSecondaryIndex;
		return equipmentEntity;
	}

	private void SetAlwaysVisibleHelmet()
	{
		SetAlwaysVisibleHelmetProxyEe();
		if (m_AlwaysVisibleHelmetEe != null && !m_ShowHelmetAboveAll)
		{
			m_ProxyEquipmentEntities.Remove(m_AlwaysVisibleHelmetEe);
		}
	}

	private void UpdateCharacter()
	{
		m_ProxyEquipmentEntities.Clear();
		foreach (EquipmentEntity equipmentEntity in EquipmentEntities)
		{
			m_ProxyEquipmentEntities.Add(equipmentEntity);
		}
		PFLog.TechArt.Log($"[UpdateCharacter] START: ProxyEEs count={m_ProxyEquipmentEntities.Count}, ShowHelmet={m_ShowHelmet}, ShowArmor={m_ShowArmor}, ShowHelmetAboveAll={m_ShowHelmetAboveAll}");
		if (EquippedItemsEntities != null)
		{
			string.Join(", ", EquippedItemsEntities.Select((EquipmentEntity e) => e?.name + ":" + ((!m_EquipmentEntityToSlot.TryGetValue(e, out var value2)) ? "NO_SLOT" : value2?.GetType().Name)));
		}
		SetAlwaysVisibleHelmet();
		Dictionary<BodyPart, EquipmentEntity> dictionary = new Dictionary<BodyPart, EquipmentEntity>();
		List<EquipmentEntity> list = (from ee in m_ProxyEquipmentEntities
			where ee != null && ee.BodyParts.Count > 0 && !ShouldHideEquipmentEntity(ee) && !IsHelmetThatShouldBeHidden(ee)
			orderby ee.Layer
			select ee).ToList();
		foreach (EquipmentEntity item in list)
		{
			if (!(item == null))
			{
				string text = string.Join(",", from bp in item.BodyParts
					where bp != null
					select (!(bp.SkinnedRenderer == null)) ? ((!(bp.Material == null)) ? bp.Type.ToString() : $"{bp.Type}(no-material)") : $"{bp.Type}(no-renderer)");
				PFLog.TechArt.Log($"[UpdateCharacter] RenderEE: '{item.name}', Layer={item.Layer}, IsAug={item.IsAugmentation}, HideBP={(long)item.HideBodyParts}, Types=[{text}]");
			}
		}
		foreach (EquipmentEntity item2 in list)
		{
			if (ShouldHideEquipmentEntity(item2) || IsHelmetThatShouldBeHidden(item2))
			{
				continue;
			}
			BodyPartType bodyPartType = (BodyPartType)0L;
			foreach (EquipmentEntity proxyEquipmentEntity in m_ProxyEquipmentEntities)
			{
				if (!(proxyEquipmentEntity == null) && !(item2 == proxyEquipmentEntity) && !ShouldHideEquipmentEntity(proxyEquipmentEntity) && !IsHelmetThatShouldBeHidden(proxyEquipmentEntity))
				{
					bodyPartType |= proxyEquipmentEntity.HideBodyParts;
					if (proxyEquipmentEntity.IsAugmentation && proxyEquipmentEntity.AugmentationArmSide == EquipmentEntity.AugmentArmSide.None)
					{
						bodyPartType |= ComputeAugmentAutoHideMask(proxyEquipmentEntity);
					}
				}
			}
			foreach (BodyPart bodyPart in item2.BodyParts)
			{
				if (bodyPart == null || ShouldHideBodyPartFromEquippedItem(bodyPart, item2) || (bodyPartType & bodyPart.Type) != 0 || bodyPart.SkinnedRenderer == null || bodyPart.Material == null)
				{
					continue;
				}
				bool flag = bodyPart.Type == BodyPartType.Forearms || bodyPart.Type == BodyPartType.Hands || bodyPart.Type == BodyPartType.UpperArms || bodyPart.Type == BodyPartType.ForearmAugRight || bodyPart.Type == BodyPartType.HandsAugRight || bodyPart.Type == BodyPartType.UpperArmsAugRight;
				bool flag2 = item2.IsAugmentation && item2.AugmentationArmSide != EquipmentEntity.AugmentArmSide.None && flag;
				KeyValuePair<BodyPart, EquipmentEntity> keyValuePair = dictionary.FirstOrDefault((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Key.Type == bodyPart.Type);
				if (keyValuePair.Key != null)
				{
					bool flag3 = bodyPart.Type == BodyPartType.Forearms;
					if ((!flag3 || (flag3 && !item2.isOnlyRightBP && !flag2)) && !flag2)
					{
						dictionary.Remove(keyValuePair.Key);
					}
				}
				dictionary[bodyPart] = item2;
			}
		}
		m_OverlayBodyParts = new List<BodyPart>();
		m_AugOverlayBodyParts = new List<BodyPart>();
		foreach (KeyValuePair<BodyPart, EquipmentEntity> item3 in dictionary)
		{
			item3.Deconstruct(out var key, out var value);
			BodyPart bodyPart2 = key;
			EquipmentEntity entity = value;
			List<BodyPart> bodyParts = (entity.IsAugmentation ? m_AugOverlayBodyParts : m_OverlayBodyParts);
			if (entity.ShowLowerMaterials)
			{
				foreach (EquipmentEntity item4 in from ee in m_ProxyEquipmentEntities
					where ee != null && ee != entity && ee.Layer < entity.Layer && !IsHelmetThatShouldBeHidden(ee)
					orderby ee.Layer
					select ee)
				{
					if (!ShouldHideEquipmentEntity(item4) && !IsHelmetThatShouldBeHidden(item4))
					{
						List<BodyPart> bodyParts2 = (item4.IsAugmentation ? m_AugOverlayBodyParts : m_OverlayBodyParts);
						AddBodyParts(bodyParts2, bodyPart2.Type, item4);
					}
				}
			}
			AddBodyParts(bodyParts, bodyPart2.Type, entity);
			foreach (EquipmentEntity item5 in from ee in m_ProxyEquipmentEntities
				where ee != null && ee != entity && ee.Layer > entity.Layer && !IsHelmetThatShouldBeHidden(ee)
				orderby ee.Layer
				select ee)
			{
				if (!ShouldHideEquipmentEntity(item5) && !IsHelmetThatShouldBeHidden(item5))
				{
					List<BodyPart> bodyParts3 = (item5.IsAugmentation ? m_AugOverlayBodyParts : m_OverlayBodyParts);
					AddBodyParts(bodyParts3, bodyPart2.Type, item5);
				}
			}
		}
		if (m_OverlayBodyParts != null && m_OverlayBodyParts.Count > 0)
		{
			m_AtlasMaterial = AtlasMaterial;
			if (m_AtlasMaterial == null)
			{
				if (makeTextures)
				{
					m_AtlasMaterial = new Material(m_OverlayBodyParts.FirstOrDefault((BodyPart x) => null != x.Material)?.Material);
				}
				else
				{
					m_AtlasMaterial = new Material(Shader.Find("Owlcat/Lit"));
					m_AtlasMaterial.name = "SimpleMesh_" + base.name;
				}
			}
		}
		if ((false || !LoadingProcess.Instance.IsLoadingInProcess) && makeTextures)
		{
			MergeOverlays(m_OverlayBodyParts);
		}
		else
		{
			OverlaysMerged = !makeTextures;
		}
		BuildMesh(dictionary);
		RebuildOutfit();
		SetUpCharacterRenderingLayerMask();
		PFLog.TechArt.Log(string.Format("[UpdateCharacter] COMPLETED: Renderers count={0}, m_AtlasRenderer={1}", Renderers.Count, m_AtlasRenderer?.name ?? "null"));
	}

	private void AddBodyParts(List<BodyPart> bodyParts, BodyPartType type, EquipmentEntity entity)
	{
		foreach (BodyPart bodyPart3 in entity.BodyParts)
		{
			if (ShouldHideBodyPartFromEquippedItem(bodyPart3, entity))
			{
				continue;
			}
			if (bodyPart3.Type == BodyPartType.Forearms && entity.isOnlyRightBP)
			{
				BodyPart bodyPart = new BodyPart
				{
					Type = BodyPartType.Augment1,
					RendererPrefab = bodyPart3.RendererPrefab,
					Material = bodyPart3.Material,
					Textures = bodyPart3.Textures
				};
				bool flag = true;
				foreach (CharacterTextureDescription texture in bodyPart.Textures)
				{
					if (texture.GetSourceTexture() == null)
					{
						if (Application.isEditor)
						{
							PFLog.TechArt.Error($"Missing texture in {type} body part in {entity} when merging overlays for {this}");
						}
						flag = false;
						break;
					}
				}
				if (flag)
				{
					bodyParts.Add(bodyPart);
				}
			}
			else
			{
				if (bodyPart3.Type != type)
				{
					continue;
				}
				if (entity.IsAugmentation && entity.AugmentationArmSide == EquipmentEntity.AugmentArmSide.Right)
				{
					BodyPartType bodyPartType = bodyPart3.Type switch
					{
						BodyPartType.Forearms => BodyPartType.ForearmAugRight, 
						BodyPartType.Hands => BodyPartType.HandsAugRight, 
						BodyPartType.UpperArms => BodyPartType.UpperArmsAugRight, 
						_ => bodyPart3.Type, 
					};
					if (bodyPartType != bodyPart3.Type)
					{
						BodyPart bodyPart2 = new BodyPart
						{
							Type = bodyPartType,
							RendererPrefab = bodyPart3.RendererPrefab,
							Material = bodyPart3.Material,
							Textures = bodyPart3.Textures
						};
						bool flag2 = true;
						foreach (CharacterTextureDescription texture2 in bodyPart2.Textures)
						{
							if (texture2.GetSourceTexture() == null)
							{
								if (Application.isEditor)
								{
									PFLog.TechArt.Error($"Missing texture in {type} body part in {entity} when merging overlays for {this}");
								}
								flag2 = false;
								break;
							}
						}
						if (flag2)
						{
							bodyParts.Add(bodyPart2);
						}
						continue;
					}
				}
				bool flag3 = true;
				foreach (CharacterTextureDescription texture3 in bodyPart3.Textures)
				{
					if (texture3.GetSourceTexture() == null)
					{
						if (Application.isEditor)
						{
							PFLog.TechArt.Error($"Missing texture in {type} body part in {entity} when merging overlays for {this}");
						}
						flag3 = false;
						break;
					}
				}
				if (flag3)
				{
					bodyParts.Add(bodyPart3);
				}
			}
		}
	}

	public void ClearSpawnedOutfit(List<OutfitPartInfo> outfitPartsListToClearAndDestroy)
	{
		foreach (OutfitPartInfo item in outfitPartsListToClearAndDestroy)
		{
			if (!IsInDollRoom || item.GameObject.GetComponent<MechadendriteSettings>() == null)
			{
				DestroyOwnedOutfitMaterials(item);
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

	private static void DestroyOwnedOutfitMaterials(OutfitPartInfo info)
	{
		if (info?.OwnedMaterials == null)
		{
			return;
		}
		Material[] ownedMaterials = info.OwnedMaterials;
		foreach (Material material in ownedMaterials)
		{
			if (material != null)
			{
				UnityEngine.Object.Destroy(material);
			}
		}
		info.OwnedMaterials = null;
	}

	public Material[] ColorizeOutfitPart(GameObject newOutfitObject, EquipmentEntity ee, EquipmentEntity.OutfitPart outfitPart)
	{
		if (outfitPart.ColorMask == null)
		{
			return null;
		}
		SelectedRampIndices selectedRampIndices = RampIndices.FirstOrDefault((SelectedRampIndices i) => i.EquipmentEntity == ee);
		if (selectedRampIndices == null)
		{
			return null;
		}
		if (ee.PrimaryRamps.Count < selectedRampIndices.PrimaryIndex)
		{
			PFLog.TechArt.Error("Character " + base.gameObject.name + ". Can't find color ramp index " + selectedRampIndices.PrimaryIndex + " in EE: " + ee.name);
			return null;
		}
		if (ee.SecondaryRamps.Count < selectedRampIndices.SecondaryIndex)
		{
			PFLog.TechArt.Error("Character " + base.gameObject.name + ". Can't find color ramp index " + selectedRampIndices.SecondaryIndex + " in EE: " + ee.name);
			return null;
		}
		Renderer componentInChildren = newOutfitObject.GetComponentInChildren<Renderer>();
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("No renderer in " + newOutfitObject);
			return null;
		}
		int num = componentInChildren.sharedMaterials.Length;
		Material[] array = new Material[num];
		Shader equipmentColorizerShader = BlueprintRoot.Instance.CharGenRoot.EquipmentColorizerShader;
		for (int j = 0; j < num; j++)
		{
			Material material = componentInChildren.sharedMaterials[j];
			Material material2 = new Material(equipmentColorizerShader);
			material2.SetTexture(ShaderProps._BaseMap, material.GetTexture(ShaderProps._BaseMap));
			material2.SetTexture(ShaderProps._BumpMap, material.GetTexture(ShaderProps._BumpMap));
			material2.SetTexture(ShaderProps._MasksMap, material.GetTexture(ShaderProps._MasksMap));
			material2.SetTexture(ShaderProps._ColorMask, outfitPart.ColorMask);
			material2.SetTexture(ShaderProps._Ramp1, ee.PrimaryRamps[selectedRampIndices.PrimaryIndex]);
			material2.SetTexture(ShaderProps._Ramp2, ee.SecondaryRamps[selectedRampIndices.SecondaryIndex]);
			material2.name = newOutfitObject.name + "_material";
			array[j] = material2;
		}
		if (array.Length != 0)
		{
			componentInChildren.sharedMaterials = array;
		}
		ColorizedOutfitParts.Add(componentInChildren);
		return array;
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
				Material[] ownedMaterials = ColorizeOutfitPart(tuple.Item1, outfit.Value, outfit.Key);
				m_OutfitObjectsSpawned.Add(new OutfitPartInfo(outfit.Key, tuple.Item1, outfit.Value, ownedMaterials));
			}
		}
		FilterOutfit();
		if (IsInDollRoom)
		{
			this.OnUpdated?.Invoke(this);
		}
	}

	public void UpdateColorizedOutfitRamps(EquipmentEntity ee)
	{
		foreach (OutfitPartInfo item in m_OutfitObjectsSpawned)
		{
			if (!(item.Ee != ee))
			{
				if (item.OutfitPart.ColorMask == null)
				{
					break;
				}
				SelectedRampIndices selectedRampIndices = RampIndices.FirstOrDefault((SelectedRampIndices i) => i.EquipmentEntity == ee);
				if (selectedRampIndices == null)
				{
					break;
				}
				if (ee.PrimaryRamps.Count < selectedRampIndices.PrimaryIndex)
				{
					PFLog.TechArt.Error("Character " + base.gameObject.name + ". Can't find color ramp index " + selectedRampIndices.PrimaryIndex + " in EE: " + ee.name);
					break;
				}
				if (ee.SecondaryRamps.Count < selectedRampIndices.SecondaryIndex)
				{
					PFLog.TechArt.Error("Character " + base.gameObject.name + ". Can't find color ramp index " + selectedRampIndices.SecondaryIndex + " in EE: " + ee.name);
					break;
				}
				Renderer componentInChildren = item.GameObject.GetComponentInChildren<Renderer>();
				if (componentInChildren == null)
				{
					PFLog.TechArt.Error("No renderer in " + item.GameObject.name);
					break;
				}
				Material[] sharedMaterials = componentInChildren.sharedMaterials;
				foreach (Material obj in sharedMaterials)
				{
					obj.SetTexture(ShaderProps._Ramp1, ee.PrimaryRamps[selectedRampIndices.PrimaryIndex]);
					obj.SetTexture(ShaderProps._Ramp2, ee.SecondaryRamps[selectedRampIndices.SecondaryIndex]);
				}
			}
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
		if (m_ShowHelmet != showHelmet)
		{
			IsDirty = true;
			m_ShowHelmet = showHelmet;
		}
	}

	public void UpdateHelmetVisibilityAboveAll(bool showHelmetAboveAll)
	{
		if (m_ShowHelmetAboveAll != showHelmetAboveAll)
		{
			IsDirty = true;
			m_ShowHelmetAboveAll = showHelmetAboveAll;
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

	public void UpdateGlovesVisibility(bool showGloves)
	{
		if (m_ShowGloves == !showGloves)
		{
			IsDirty = true;
			m_ShowGloves = showGloves;
		}
	}

	public void UpdateBootsVisibility(bool showBoots)
	{
		if (m_ShowBoots == !showBoots)
		{
			IsDirty = true;
			m_ShowBoots = showBoots;
		}
	}

	public void UpdateArmorVisibility(bool showArmor)
	{
		if (m_ShowArmor == !showArmor)
		{
			IsDirty = true;
			m_ShowArmor = showArmor;
		}
	}

	private void ClearAtlases()
	{
		foreach (CharacterAtlas atlase in m_Atlases)
		{
			atlase.Dispose();
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
		PFLog.TechArt.Log(string.Format("[BuildMesh] START: geometryBodyParts count={0}, m_AtlasMaterial={1}", geometryBodyParts.Count, m_AtlasMaterial?.name ?? "null"));
		ClearMeshes();
		Renderers.Clear();
		m_AtlasRenderer = null;
		if (m_AugmentationMaterial != null)
		{
			UnityEngine.Object.Destroy(m_AugmentationMaterial);
			m_AugmentationMaterial = null;
		}
		List<Transform> list = new List<Transform>();
		List<BoneWeight> list2 = new List<BoneWeight>();
		List<Matrix4x4> list3 = new List<Matrix4x4>();
		Dictionary<string, Transform> cachedBones = CacheHierarchy();
		if (m_AtlasMaterial != null)
		{
			List<CombineInstance> list4 = new List<CombineInstance>();
			List<Vector2> list5 = new List<Vector2>();
			GameObject obj = new GameObject("Renderer_" + m_AtlasMaterial.name);
			obj.transform.parent = Animator.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.transform.localRotation = Quaternion.identity;
			SkinnedMeshRenderer skinnedMeshRenderer = obj.AddComponent<SkinnedMeshRenderer>();
			Mesh mesh = new Mesh
			{
				name = "Character"
			};
			mesh.Clear();
			GameObject gameObject = null;
			if (geometryBodyParts.Count((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Key.Type == BodyPartType.Forearms && !kvp.Value.IsAugmentation) > 1)
			{
				gameObject = EditForearmRightMesh(geometryBodyParts);
			}
			foreach (KeyValuePair<BodyPart, EquipmentEntity> geometryBodyPart in geometryBodyParts)
			{
				PFLog.TechArt.Log($"[BuildMesh PRE-CUT] Type={geometryBodyPart.Key.Type}, EE='{geometryBodyPart.Value?.name}', IsAug={geometryBodyPart.Value?.IsAugmentation}, ArmSide={geometryBodyPart.Value?.AugmentationArmSide}, Renderer={geometryBodyPart.Key.RendererPrefab?.name}");
			}
			List<GameObject> list6 = EditAugmentationArmMesh(geometryBodyParts);
			foreach (KeyValuePair<BodyPart, EquipmentEntity> geometryBodyPart2 in geometryBodyParts)
			{
				PFLog.TechArt.Log($"[BuildMesh POST-CUT] Type={geometryBodyPart2.Key.Type}, EE='{geometryBodyPart2.Value?.name}', IsAug={geometryBodyPart2.Value?.IsAugmentation}, Renderer={geometryBodyPart2.Key.RendererPrefab?.name}");
			}
			List<CombineInstance> list7 = new List<CombineInstance>();
			List<Vector2> list8 = new List<Vector2>();
			List<BoneWeight> list9 = new List<BoneWeight>();
			bool flag = false;
			foreach (KeyValuePair<BodyPart, EquipmentEntity> geometryBodyPart3 in geometryBodyParts)
			{
				SkinnedMeshRenderer skinnedRenderer = geometryBodyPart3.Key.SkinnedRenderer;
				if (skinnedRenderer == null)
				{
					continue;
				}
				if (skinnedMeshRenderer.rootBone == null && skinnedRenderer.rootBone != null)
				{
					skinnedMeshRenderer.rootBone = Animator.transform;
				}
				int[] bonesMapping = new int[skinnedRenderer.sharedMesh.bindposes.Length];
				EnsureBones(geometryBodyPart3.Key, list, list3, bonesMapping, cachedBones);
				Vector2[] uv = skinnedRenderer.sharedMesh.uv;
				int num;
				List<CombineInstance> list10;
				if (geometryBodyPart3.Value != null)
				{
					num = (geometryBodyPart3.Value.IsAugmentation ? 1 : 0);
					if (num != 0)
					{
						list10 = list7;
						goto IL_03d9;
					}
				}
				else
				{
					num = 0;
				}
				list10 = list4;
				goto IL_03d9;
				IL_03d9:
				List<CombineInstance> list11 = list10;
				List<Vector2> list12 = ((num != 0) ? list8 : list5);
				List<BoneWeight> boneWeights = ((num != 0) ? list9 : list2);
				if (num != 0)
				{
					flag = true;
				}
				for (int i = 0; i < uv.Length; i++)
				{
					list12.Add(uv[i]);
				}
				CombineInstance combineInstance = default(CombineInstance);
				combineInstance.mesh = skinnedRenderer.sharedMesh;
				combineInstance.transform = Matrix4x4.identity;
				CombineInstance item = combineInstance;
				InsertBoneWeights(boneWeights, bonesMapping, skinnedRenderer);
				list11.Add(item);
			}
			if (flag && AugmentationAtlas != null && AugmentationAtlas.IsInitialized)
			{
				Mesh mesh2 = new Mesh
				{
					name = "Character_Base"
				};
				mesh2.CombineMeshes(list4.ToArray());
				mesh2.boneWeights = list2.ToArray();
				mesh2.uv = list5.ToArray();
				Mesh mesh3 = new Mesh
				{
					name = "Character_Aug"
				};
				mesh3.CombineMeshes(list7.ToArray());
				mesh3.boneWeights = list9.ToArray();
				mesh3.uv = list8.ToArray();
				CombineInstance[] combine = new CombineInstance[2]
				{
					new CombineInstance
					{
						mesh = mesh2,
						transform = Matrix4x4.identity
					},
					new CombineInstance
					{
						mesh = mesh3,
						transform = Matrix4x4.identity
					}
				};
				mesh.CombineMeshes(combine, mergeSubMeshes: false);
				mesh.bindposes = list3.ToArray();
				List<BoneWeight> list13 = new List<BoneWeight>(list2.Count + list9.Count);
				list13.AddRange(list2);
				list13.AddRange(list9);
				mesh.boneWeights = list13.ToArray();
				mesh.RecalculateBounds();
				mesh.UploadMeshData(markNoLongerReadable: true);
				skinnedMeshRenderer.sharedMesh = mesh;
				skinnedMeshRenderer.bones = list.ToArray();
				Material material = new Material(m_AtlasMaterial);
				material.name = "AugmentationAtlas_Material";
				m_AugmentationMaterial = material;
				if (AugmentationAtlas.DiffuseAtlas != null)
				{
					material.SetTexture(ShaderProps._BaseMap, AugmentationAtlas.DiffuseAtlas);
				}
				if (AugmentationAtlas.NormalAtlas != null)
				{
					material.EnableKeyword("_NORMALMAP");
					material.SetTexture(ShaderProps._BumpMap, AugmentationAtlas.NormalAtlas);
				}
				if (AugmentationAtlas.MasksAtlas != null)
				{
					material.EnableKeyword("_MASKSMAP");
					material.SetTexture(ShaderProps._MasksMap, AugmentationAtlas.MasksAtlas);
				}
				skinnedMeshRenderer.sharedMaterials = new Material[2] { m_AtlasMaterial, material };
				UnityEngine.Object.Destroy(mesh2);
				UnityEngine.Object.Destroy(mesh3);
				PFLog.TechArt.Log($"[BuildMesh] Created 2 sub-meshes: base={list4.Count} parts, aug={list7.Count} parts");
			}
			else
			{
				if (flag)
				{
					list4.AddRange(list7);
					list5.AddRange(list8);
					list2.AddRange(list9);
				}
				mesh.CombineMeshes(list4.ToArray());
				mesh.bindposes = list3.ToArray();
				mesh.boneWeights = list2.ToArray();
				mesh.uv = list5.ToArray();
				mesh.RecalculateBounds();
				mesh.UploadMeshData(markNoLongerReadable: true);
				skinnedMeshRenderer.sharedMesh = mesh;
				skinnedMeshRenderer.bones = list.ToArray();
			}
			m_AtlasRenderer = skinnedMeshRenderer;
			Renderers.Add(skinnedMeshRenderer);
			skinnedMeshRenderer.gameObject.layer = 9;
			if (!flag || AugmentationAtlas == null || !AugmentationAtlas.IsInitialized)
			{
				skinnedMeshRenderer.sharedMaterial = m_AtlasMaterial;
			}
			Animator.Rebind();
			PFLog.TechArt.Log($"[BuildMesh] Created SkinnedMeshRenderer: {skinnedMeshRenderer.name}, mesh={mesh.name}, bones={list.Count}");
			if (gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			if (list6 != null)
			{
				foreach (GameObject item2 in list6)
				{
					if (item2 != null)
					{
						UnityEngine.Object.DestroyImmediate(item2);
					}
				}
			}
		}
		else
		{
			PFLog.TechArt.Warning("[BuildMesh] m_AtlasMaterial is null!");
		}
		PFLog.TechArt.Log($"[BuildMesh] COMPLETED: Renderers count={Renderers.Count}");
	}

	private GameObject EditForearmRightMesh(Dictionary<BodyPart, EquipmentEntity> geometryBodyParts)
	{
		GameObject newRendererPrefab = null;
		KeyValuePair<BodyPart, EquipmentEntity> keyValuePair = geometryBodyParts.FirstOrDefault((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Key.Type == BodyPartType.Forearms && !kvp.Value.isOnlyRightBP);
		if (keyValuePair.Key.SkinnedRenderer != null)
		{
			Mesh mesh = UnityEngine.Object.Instantiate(keyValuePair.Key.SkinnedRenderer.sharedMesh);
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			int[] triangles = mesh.triangles;
			Vector3[] normals = mesh.normals;
			Vector4[] tangents = mesh.tangents;
			BoneWeight[] boneWeights = mesh.boneWeights;
			Matrix4x4[] bindposes = mesh.bindposes;
			Transform[] bones = keyValuePair.Key.SkinnedRenderer.bones;
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] != null && bones[i].name.StartsWith("R_"))
				{
					hashSet.Add(i);
				}
			}
			bool[] array = new bool[vertices.Length];
			for (int j = 0; j < vertices.Length; j++)
			{
				BoneWeight boneWeight = boneWeights[j];
				bool flag = (!hashSet.Contains(boneWeight.boneIndex0) || !(boneWeight.weight0 > 0.01f)) && (!hashSet.Contains(boneWeight.boneIndex1) || !(boneWeight.weight1 > 0.01f)) && (!hashSet.Contains(boneWeight.boneIndex2) || !(boneWeight.weight2 > 0.01f)) && (!hashSet.Contains(boneWeight.boneIndex3) || !(boneWeight.weight3 > 0.01f));
				array[j] = flag;
			}
			int num = 0;
			for (int k = 0; k < vertices.Length; k++)
			{
				if (array[k])
				{
					num++;
				}
			}
			int[] array2 = new int[vertices.Length];
			Vector3[] array3 = new Vector3[num];
			Vector2[] array4 = new Vector2[num];
			Vector3[] array5 = new Vector3[num];
			Vector4[] array6 = new Vector4[num];
			BoneWeight[] array7 = new BoneWeight[num];
			int num2 = 0;
			for (int l = 0; l < vertices.Length; l++)
			{
				if (array[l])
				{
					array2[l] = num2;
					array3[num2] = vertices[l];
					array4[num2] = uv[l];
					array5[num2] = normals[l];
					array6[num2] = tangents[l];
					array7[num2] = boneWeights[l];
					num2++;
				}
				else
				{
					array2[l] = -1;
				}
			}
			List<int> list = new List<int>();
			for (int m = 0; m < triangles.Length; m += 3)
			{
				int num3 = array2[triangles[m]];
				int num4 = array2[triangles[m + 1]];
				int num5 = array2[triangles[m + 2]];
				if (num3 != -1 && num4 != -1 && num5 != -1)
				{
					list.Add(num3);
					list.Add(num4);
					list.Add(num5);
				}
			}
			int[] array8 = list.ToArray();
			if (array3.Length == 0 || array8.Length == 0)
			{
				PFLog.TechArt.Error("Результат фильтрации пустой: вершины или треугольники отсутствуют.");
				UnityEngine.Object.Destroy(mesh);
				return null;
			}
			mesh.Clear();
			mesh.vertices = array3;
			mesh.uv = array4;
			mesh.normals = array5;
			mesh.tangents = array6;
			mesh.boneWeights = array7;
			mesh.bindposes = bindposes;
			mesh.triangles = array8;
			mesh.RecalculateBounds();
			newRendererPrefab = UnityEngine.Object.Instantiate(keyValuePair.Key.RendererPrefab);
			newRendererPrefab.name = "Modified_Forearms_Prefab";
			SkinnedMeshRenderer componentInChildren = newRendererPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
			componentInChildren.sharedMesh = mesh;
			componentInChildren.rootBone = keyValuePair.Key.SkinnedRenderer.rootBone;
			componentInChildren.bones = keyValuePair.Key.SkinnedRenderer.bones;
			EquipmentEntity equipmentEntity = UnityEngine.Object.Instantiate(keyValuePair.Value);
			equipmentEntity.name = keyValuePair.Value.name + "_Modified";
			for (int n = 0; n < equipmentEntity.BodyParts.Count; n++)
			{
				if (equipmentEntity.BodyParts[n].Type == BodyPartType.Forearms && equipmentEntity.BodyParts[n].RendererPrefab == keyValuePair.Key.RendererPrefab)
				{
					equipmentEntity.BodyParts[n] = new BodyPart
					{
						RendererPrefab = newRendererPrefab,
						Type = keyValuePair.Key.Type,
						Material = keyValuePair.Key.Material,
						Textures = new List<CharacterTextureDescription>(keyValuePair.Key.Textures)
					};
					break;
				}
			}
			geometryBodyParts.Remove(keyValuePair.Key);
			geometryBodyParts.Add(equipmentEntity.BodyParts.Find((BodyPart bp) => bp.Type == BodyPartType.Forearms && bp.RendererPrefab == newRendererPrefab), equipmentEntity);
		}
		return newRendererPrefab;
	}

	private static bool IsArmType(BodyPartType t)
	{
		if (t != BodyPartType.Forearms && t != BodyPartType.Hands && t != BodyPartType.UpperArms && t != BodyPartType.ForearmAugRight && t != BodyPartType.HandsAugRight)
		{
			return t == BodyPartType.UpperArmsAugRight;
		}
		return true;
	}

	private static BodyPartType ComputeAugmentAutoHideMask(EquipmentEntity aug)
	{
		BodyPartType bodyPartType = (BodyPartType)0L;
		bool flag = false;
		foreach (BodyPart bodyPart in aug.BodyParts)
		{
			if (bodyPart != null && !IsArmType(bodyPart.Type))
			{
				bodyPartType |= bodyPart.Type;
				if ((bodyPart.Type & (BodyPartType.Feet | BodyPartType.KneeCops | BodyPartType.LowerLegs)) != (BodyPartType)0L)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			bodyPartType |= BodyPartType.Feet | BodyPartType.KneeCops | BodyPartType.LowerLegs;
		}
		return bodyPartType;
	}

	private List<GameObject> EditAugmentationArmMesh(Dictionary<BodyPart, EquipmentEntity> geometryBodyParts)
	{
		List<GameObject> list = new List<GameObject>();
		bool flag = false;
		bool flag2 = false;
		foreach (EquipmentEntity proxyEquipmentEntity in m_ProxyEquipmentEntities)
		{
			if (!(proxyEquipmentEntity == null) && proxyEquipmentEntity.IsAugmentation)
			{
				if (proxyEquipmentEntity.AugmentationArmSide == EquipmentEntity.AugmentArmSide.Left)
				{
					flag = true;
				}
				if (proxyEquipmentEntity.AugmentationArmSide == EquipmentEntity.AugmentArmSide.Right)
				{
					flag2 = true;
				}
			}
		}
		if (!flag2)
		{
			flag2 = geometryBodyParts.Any((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Key.Type == BodyPartType.ForearmAugRight || kvp.Key.Type == BodyPartType.HandsAugRight || kvp.Key.Type == BodyPartType.UpperArmsAugRight);
		}
		if (!flag && !flag2)
		{
			return list;
		}
		BodyPartType[] array = new BodyPartType[3]
		{
			BodyPartType.Forearms,
			BodyPartType.Hands,
			BodyPartType.UpperArms
		};
		foreach (BodyPartType armType in array)
		{
			BodyPartType augRightType = ((armType == BodyPartType.Forearms) ? BodyPartType.ForearmAugRight : ((armType == BodyPartType.Hands) ? BodyPartType.HandsAugRight : BodyPartType.UpperArmsAugRight));
			bool flag3 = geometryBodyParts.Any((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Value.IsAugmentation && kvp.Value.AugmentationArmSide == EquipmentEntity.AugmentArmSide.Left && kvp.Key.Type == armType);
			bool flag4 = geometryBodyParts.Any((KeyValuePair<BodyPart, EquipmentEntity> kvp) => (kvp.Value.IsAugmentation && kvp.Value.AugmentationArmSide == EquipmentEntity.AugmentArmSide.Right && kvp.Key.Type == armType) || kvp.Key.Type == augRightType);
			if (!flag3 && !flag4)
			{
				PFLog.TechArt.Log($"[EditAugmentationArmMesh] Type={armType}: no aug coverage for this type — non-aug body parts kept intact");
			}
			else
			{
				CutArmBodyPartsOfType(geometryBodyParts, armType, flag3, flag4, list);
			}
		}
		array = _AuxArmTypes;
		foreach (BodyPartType armType2 in array)
		{
			if (flag || flag2)
			{
				CutArmBodyPartsOfType(geometryBodyParts, armType2, flag, flag2, list);
			}
		}
		return list;
	}

	private void CutArmBodyPartsOfType(Dictionary<BodyPart, EquipmentEntity> geometryBodyParts, BodyPartType armType, bool cutLeft, bool cutRight, List<GameObject> createdPrefabs)
	{
		List<KeyValuePair<BodyPart, EquipmentEntity>> list = geometryBodyParts.Where((KeyValuePair<BodyPart, EquipmentEntity> kvp) => kvp.Key.Type == armType && !kvp.Value.IsAugmentation && kvp.Key.SkinnedRenderer != null).ToList();
		if (list.Count == 0)
		{
			return;
		}
		PFLog.TechArt.Log($"[EditAugmentationArmMesh] Type={armType}: cutLeft={cutLeft}, cutRight={cutRight}, non-aug entries={list.Count}");
		foreach (KeyValuePair<BodyPart, EquipmentEntity> item in list)
		{
			BodyPart bodyPart = item.Key;
			EquipmentEntity value = item.Value;
			string arg = value?.name;
			bool flag = false;
			if (cutLeft)
			{
				GameObject gameObject = AugmentationBodyPartReplacer.CutVerticesByBonePrefix(bodyPart, value, "L_");
				if (gameObject != null)
				{
					createdPrefabs.Add(gameObject);
					BodyPart bodyPart2 = new BodyPart
					{
						Type = armType,
						RendererPrefab = gameObject,
						Material = bodyPart.Material,
						Textures = new List<CharacterTextureDescription>(bodyPart.Textures)
					};
					geometryBodyParts.Remove(bodyPart);
					geometryBodyParts[bodyPart2] = value;
					bodyPart = bodyPart2;
					flag = true;
					PFLog.TechArt.Log($"[EditAugmentationArmMesh] Cut 'L_' from {armType} of '{arg}'");
				}
				else
				{
					PFLog.TechArt.Log($"[EditAugmentationArmMesh] Cut 'L_' produced no mesh for {armType} of '{arg}' (no L_ geometry on this side)");
				}
			}
			if (cutRight)
			{
				GameObject gameObject2 = AugmentationBodyPartReplacer.CutVerticesByBonePrefix(bodyPart, value, "R_");
				if (gameObject2 != null)
				{
					createdPrefabs.Add(gameObject2);
					BodyPart key = new BodyPart
					{
						Type = armType,
						RendererPrefab = gameObject2,
						Material = bodyPart.Material,
						Textures = new List<CharacterTextureDescription>(bodyPart.Textures)
					};
					geometryBodyParts.Remove(bodyPart);
					geometryBodyParts[key] = value;
					PFLog.TechArt.Log($"[EditAugmentationArmMesh] Cut 'R_' from {armType} of '{arg}'");
				}
				else if (flag)
				{
					geometryBodyParts.Remove(bodyPart);
					PFLog.TechArt.Log($"[EditAugmentationArmMesh] Removed {armType} of '{arg}' — both sides augmented");
				}
				else
				{
					PFLog.TechArt.Log($"[EditAugmentationArmMesh] Cut 'R_' produced no mesh for {armType} of '{arg}' (no R_ geometry on this side)");
				}
			}
		}
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
		if (overlayBodyParts.Count == 0 || !makeTextures)
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
			atlase.Build(m_EquipmentEntitiesTextures, AtlasMaterial, cleanAtlas: true);
		}
		StandardMaterialController component = base.gameObject.GetComponent<StandardMaterialController>();
		if (component != null)
		{
			component.InvalidateMaterialsTextures();
		}
		Services.GetInstance<CharacterAtlasService>().QueueAtlasRebuild(this, m_Atlases, OnAtlasCompressed, OnAtlasNotCompressed, base.name);
		m_EquipmentEntitiesTextures.Clear();
		if (m_AugOverlayBodyParts != null && m_AugOverlayBodyParts.Count > 0)
		{
			if (AugmentationAtlas == null || !AugmentationAtlas.IsInitialized)
			{
				CharacterAtlasData characterAtlasData = UIConfig.Instance?.AugmentationAtlasData;
				if (characterAtlasData != null)
				{
					AugmentationAtlas?.Dispose();
					AugmentationAtlas = new AugmentationAtlasController();
					AugmentationAtlas.Initialize(characterAtlasData);
					PFLog.TechArt.Log("[MergeOverlays] Lazy-created augmentation atlas from UIConfig");
				}
				else
				{
					PFLog.TechArt.Warning("[MergeOverlays] Cannot create augmentation atlas: UIConfig.AugmentationAtlasData is null");
				}
			}
			if (AugmentationAtlas != null && AugmentationAtlas.IsInitialized)
			{
				foreach (BodyPart augOverlayBodyPart in m_AugOverlayBodyParts)
				{
					foreach (CharacterTextureDescription texture2 in augOverlayBodyPart.Textures)
					{
						Texture2D diffuseTexture2 = texture2.DiffuseTexture;
						Texture2D normalTexture = texture2.NormalTexture;
						Texture2D maskTexture = texture2.MaskTexture;
						PFLog.TechArt.Log($"[AugAtlas] Slot={augOverlayBodyPart.Type}, Diffuse={diffuseTexture2?.name}({diffuseTexture2?.format}|{diffuseTexture2?.width}x{diffuseTexture2?.height}), Normal={normalTexture?.name}({normalTexture?.format}), Mask={maskTexture?.name}({maskTexture?.format})");
						AugmentationAtlas.UpdateSlot(augOverlayBodyPart.Type, diffuseTexture2, normalTexture, maskTexture);
					}
				}
				AugmentationAtlas.ApplyToMaterial(AtlasMaterial);
				PFLog.TechArt.Log($"[MergeOverlays] Baked {m_AugOverlayBodyParts.Count} augmentation body parts to augmentation atlas");
			}
		}
		else if (AugmentationAtlas != null)
		{
			AugmentationAtlas.Dispose();
			AugmentationAtlas = null;
			PFLog.TechArt.Log("[MergeOverlays] Disposed augmentation atlas (no aug body parts)");
		}
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

	private static bool IsGlovesType(BodyPart bodyPart)
	{
		return bodyPart.Type switch
		{
			BodyPartType.Hands => true, 
			BodyPartType.Forearms => true, 
			BodyPartType.Cuffs => true, 
			BodyPartType.CuffL => true, 
			BodyPartType.CuffR => true, 
			BodyPartType.LowerArmsExtra => true, 
			_ => false, 
		};
	}

	private static bool IsBootsType(BodyPart bodyPart)
	{
		return bodyPart.Type switch
		{
			BodyPartType.Feet => true, 
			BodyPartType.LowerLegs => true, 
			BodyPartType.LowerLegsExtra => true, 
			BodyPartType.KneeCops => true, 
			_ => false, 
		};
	}

	private static bool IsArmorType(BodyPart bodyPart)
	{
		return bodyPart.Type switch
		{
			BodyPartType.Torso => true, 
			BodyPartType.TorsoExtra => true, 
			BodyPartType.UpperArms => true, 
			BodyPartType.UpperLegs => true, 
			BodyPartType.Spaulders => true, 
			BodyPartType.SpaulderL => true, 
			BodyPartType.SpaulderR => true, 
			BodyPartType.Skirt => true, 
			BodyPartType.HighCollar => true, 
			BodyPartType.Hoses => true, 
			BodyPartType.Belt => true, 
			_ => false, 
		};
	}
}
