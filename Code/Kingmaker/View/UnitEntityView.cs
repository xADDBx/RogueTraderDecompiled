using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.OcclusionGeometryClip;
using UnityEngine;

namespace Kingmaker.View;

[DisallowMultipleComponent]
[KnowledgeDatabaseID("bbf398b7d73be774299f8a932569287e")]
public class UnitEntityView : AbstractUnitEntityView, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IUnitActiveEquipmentSetHandler<EntitySubscriber>, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitActiveEquipmentSetHandler, EntitySubscriber>, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, ILootDroppedAsAttachedHandler<EntitySubscriber>, ILootDroppedAsAttachedHandler, IEventTag<ILootDroppedAsAttachedHandler, EntitySubscriber>, ICellAbilityHandler, IShowAoEAffectedUIHandler
{
	private const string FogOfWarKeyword = "FOG_OF_WAR_AFFECTED";

	private const float ReduceSizeScaleStep = 0.66f;

	[SerializeField]
	[Tooltip("Don't setup size, scale, etc of soft and core collider automatically'")]
	private bool m_KeepCollidersSetupAsIs;

	[SerializeField]
	[HideInInspector]
	private bool m_HorizontalCollider;

	[SerializeField]
	[ShowIf("m_HorizontalCollider")]
	private float m_SoftColliderRadius = 1f;

	[SerializeField]
	private int m_Corpulence;

	[SerializeField]
	private UnitAnimationSettings m_AnimationSettings;

	[SerializeField]
	private bool m_IgnoreRaceAnimationSettings;

	[SerializeField]
	[CanBeNull]
	private GameObject m_BuiltinArmorMediumView;

	[SerializeField]
	[CanBeNull]
	private GameObject m_BuiltinArmorHeavyView;

	private bool m_ObstacleHighlighted;

	private Vector3 m_OriginalScale;

	private float m_Scale;

	[CanBeNull]
	private SwarmBehaviour m_Swarm;

	private GameObject m_StealthEffect;

	private ILocalMapMarker m_LootLocalMapMarker;

	private OcclusionGeometryClipTarget m_OcclusionGeometryClipTarget;

	private bool m_IsInAoePattern;

	private UnitMovementAgentBase m_AgentOverride;

	private UnitViewHandsEquipment m_HandsEquipment;

	private GameObject m_AttachedDroppedLoot;

	private bool m_ParticipateInLockControlCutscene;

	private Vector3? m_OvertipPosition;

	public override bool IsInAoePattern => m_IsInAoePattern;

	public bool DisableSizeScaling { get; set; }

	public override float Corpulence => Mathf.Max(m_Corpulence, 0.3f);

	public bool DoNotAdjustScale { get; set; }

	public Vector3 OriginalScale => m_OriginalScale;

	public UnitMovementAgentBase AgentOverride
	{
		get
		{
			return m_AgentOverride;
		}
		set
		{
			if (value == null)
			{
				UnityEngine.Object.DestroyImmediate(m_AgentOverride);
			}
			m_AgentOverride = value;
		}
	}

	public override UnitMovementAgentBase MovementAgent
	{
		get
		{
			if (!(AgentOverride == null))
			{
				return AgentOverride;
			}
			return base.AgentASP;
		}
	}

	[CanBeNull]
	public FogOfWarRevealerSettings FogOfWarRevealer { get; private set; }

	public UnitHitFxManager HitFxManager { get; private set; }

	public UnitViewHandsEquipment HandsEquipment => m_HandsEquipment;

	public LosCalculations.CoverType CoverType { get; set; }

	public bool InCover { get; set; }

	public UnitVisualSettings.MusicCombatState CombatMusic
	{
		get
		{
			if (base.Blueprint.DifficultyType <= UnitDifficultyType.Common)
			{
				return UnitVisualSettings.MusicCombatState.Normal;
			}
			return UnitVisualSettings.MusicCombatState.Hard;
		}
	}

	public new BaseUnitEntity Data => (BaseUnitEntity)base.Data;

	public new BaseUnitEntity EntityData => Data;

	public override bool KeepCollidersSetupAsIs => m_KeepCollidersSetupAsIs;

	public override bool UseHorizontalSoftCollider => m_HorizontalCollider;

	internal override float HorizontalSoftColliderRadius => m_SoftColliderRadius;

	public override List<ItemEntity> Mechadendrites => Data.Body.Mechadendrites.Select((EquipmentSlot<BlueprintItemMechadendrite> mechadendrite) => mechadendrite.MaybeItem).ToList();

	public Vector3 CorpseOvertipPosition
	{
		get
		{
			if (m_OvertipPosition.HasValue)
			{
				UnitAnimationManager animationManager = base.AnimationManager;
				if ((object)animationManager == null || !animationManager.IsAnimating)
				{
					return m_OvertipPosition.Value;
				}
			}
			Bounds? bounds = null;
			if (m_Colliders != null)
			{
				foreach (Collider collider in m_Colliders)
				{
					if (!(collider == null) && collider.enabled && collider.gameObject.activeInHierarchy)
					{
						if (!bounds.HasValue)
						{
							bounds = collider.bounds;
						}
						else
						{
							bounds.Value.Encapsulate(collider.bounds);
						}
					}
				}
			}
			Bounds valueOrDefault = bounds.GetValueOrDefault();
			if (!bounds.HasValue)
			{
				valueOrDefault = new Bounds(base.ViewTransform.position, Vector3.one);
				bounds = valueOrDefault;
			}
			m_OvertipPosition = bounds.Value.center + new Vector3(0f, bounds.Value.extents.y, 0f);
			return m_OvertipPosition.Value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_OriginalScale = base.ViewTransform.localScale;
		m_Scale = 1f;
	}

	public void ConvertCharacterController()
	{
		CharacterController componentInChildren = GetComponentInChildren<CharacterController>();
		if ((bool)componentInChildren)
		{
			float num = Mathf.Max(base.ViewTransform.localScale.x, base.ViewTransform.localScale.z);
			m_Corpulence = Mathf.RoundToInt(Mathf.Max(num * componentInChildren.radius, 0.5f));
			UnityEngine.Object.DestroyImmediate(componentInChildren, allowDestroyingAssets: true);
		}
	}

	public void UpdateCombatSwitch()
	{
		if (Data != null)
		{
			SetOccluderColorAndState();
			AkSoundEngine.SetRTPCValue("CharacterCombat", Data.IsInCombat ? 1 : 0, base.gameObject);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateCombatSwitch();
	}

	protected override void OnDisable()
	{
		UpdateCombatSwitch();
		base.OnDisable();
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		m_Swarm = GetComponent<SwarmBehaviour>();
		if (m_Swarm != null)
		{
			float percent = (EntityData.LifeState.IsDead ? 0f : (1f * (float)EntityData.Health.HitPointsLeft / (float)EntityData.Health.MaxHitPoints));
			m_Swarm.UpdateHealth(percent);
		}
		HitFxManager = GetComponentInChildren<UnitHitFxManager>();
		HitFxManager = (HitFxManager ? HitFxManager : base.gameObject.AddComponent<UnitHitFxManager>());
		HitFxManager.SetView(this);
		foreach (WeaponSlot item in EntityData.Body.AllSlots.OfType<WeaponSlot>())
		{
			item.FindSnapMapForNaturalWeapon();
		}
		base.ViewTransform.localScale = m_OriginalScale * (m_Scale = GetSizeScale());
		m_HandsEquipment?.Dispose();
		m_HandsEquipment = new UnitViewHandsEquipment(this, base.CharacterAvatar);
		base.MechadendritesEquipment = new UnitViewMechadendritesEquipment(this, base.CharacterAvatar);
		UpdateBodyEquipmentModel();
		UpdateClassEquipment();
		UpdateEquipmentColorRampIndices();
		base.CharacterAvatar?.RebuildOutfit();
		HandsEquipment.UpdateAll();
		base.MechadendritesEquipment.UpdateAll();
		SpawnAttachedDroppedLoot();
	}

	public override void UpdateViewActive()
	{
		base.UpdateViewActive();
		if (Data == null)
		{
			return;
		}
		Data.GetOptional<UnitPartFamiliarLeader>()?.UpdateFamiliarsVisibility();
		if (Data.Commands.Current != null && Data.Commands.Current.ForcedPath != null)
		{
			if (!Data.IsViewActive)
			{
				Data.Commands.Current.ForcedPath.persistentPath = true;
				return;
			}
			Data.Commands.Current.ForcedPath.persistentPath = false;
			Data.Commands.Current.ForcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return CreateEntityData();
	}

	public BaseUnitEntity CreateEntityData([NotNull] BlueprintUnit blueprint)
	{
		base.Blueprint = blueprint;
		return CreateEntityData();
	}

	private BaseUnitEntity CreateEntityData()
	{
		if (base.Blueprint == null)
		{
			base.Blueprint = BlueprintRoot.Instance.Cheats.PrefabUnit;
		}
		BaseUnitEntity baseUnitEntity = base.Blueprint.CreateEntity(UniqueId, base.IsInGameBySettings);
		float y = base.ViewTransform.rotation.eulerAngles.y;
		baseUnitEntity.SetOrientation(y);
		baseUnitEntity.Position = SizePathfindingHelper.FromViewToMechanicsPosition(baseUnitEntity, base.ViewTransform.position);
		baseUnitEntity.AttachView(this);
		return baseUnitEntity;
	}

	internal override void OnMovementStarted(Vector3 pathDestination, bool preview = false)
	{
		base.OnMovementStarted(pathDestination, preview);
		UnitMoveTo currentOrQueued = EntityData.Commands.GetCurrentOrQueued<UnitMoveTo>();
		UnitMoveToProper currentOrQueued2 = EntityData.Commands.GetCurrentOrQueued<UnitMoveToProper>();
		UnitAreaTransition current = EntityData.Commands.GetCurrent<UnitAreaTransition>();
		if (currentOrQueued != null || current != null || currentOrQueued2 != null || preview)
		{
			TryShowPointer(pathDestination, preview);
		}
	}

	internal void TryShowPointer(Vector3 pathDestination, bool preview = false)
	{
		if (EntityData.IsDirectlyControllable && Game.Instance.CurrentMode != GameModeType.Cutscene && Game.Instance.CurrentMode != GameModeType.SpaceCombat)
		{
			Vector3 vector = SizePathfindingHelper.FromMechanicsToViewPosition(Data, pathDestination);
			if (Physics.Raycast(new UnityEngine.Ray(vector + 500f * Vector3.up, Vector3.down), out var hitInfo, 1000f, 2359553))
			{
				vector = hitInfo.point;
			}
			if (preview)
			{
				ClickPointerManager.Instance.AddPreviewPointer(vector, EntityData);
			}
			else
			{
				ClickPointerManager.Instance.AddPointer(vector, EntityData);
			}
		}
	}

	internal override void OnMovementInterrupted(Vector3 destination)
	{
		base.OnMovementInterrupted(destination);
		(EntityData.Commands.Current as UnitMoveToProper)?.Interrupt();
		EntityData.Commands.GetCurrent<UnitGroupCommand>()?.Interrupt();
		EventBus.RaiseEvent((IBaseUnitEntity)EntityData, (Action<IUnitMovementInterruptHandler>)delegate(IUnitMovementInterruptHandler h)
		{
			h.HandleMovementInterruption(destination);
		}, isCheckRuntime: true);
	}

	internal override void OnMovementComplete()
	{
		base.OnMovementComplete();
		using (ProfileScope.New("Unit complete movement"))
		{
			EventBus.RaiseEvent((IBaseUnitEntity)EntityData, (Action<IUnitMovementHandler>)delegate(IUnitMovementHandler h)
			{
				h.HandleMovementComplete();
			}, isCheckRuntime: true);
		}
	}

	internal override void OnMovementWaypointUpdate(int index)
	{
		base.OnMovementWaypointUpdate(index);
		using (ProfileScope.New("Unit move waypoint update"))
		{
			EventBus.RaiseEvent((IBaseUnitEntity)EntityData, (Action<IUnitMovementHandler>)delegate(IUnitMovementHandler h)
			{
				h.HandleWaypointUpdate(index);
			}, isCheckRuntime: true);
		}
	}

	public override void HandleDeath()
	{
		base.HandleDeath();
		if (m_Swarm != null)
		{
			m_Swarm.UpdateHealth(0f);
		}
	}

	public override void HandleDamage()
	{
		if (m_Swarm != null)
		{
			float percent = 1f * (float)EntityData.Health.HitPointsLeft / (float)EntityData.Health.MaxHitPoints;
			m_Swarm.UpdateHealth(percent);
		}
		base.HandleDamage();
	}

	protected override void OnExitProneState()
	{
		base.OnExitProneState();
		if ((bool)RigidbodyController && RigidbodyController.IsActive)
		{
			PFLog.Default.Error("UnitEntityView.LeaveProneState: RigidbodyController.IsActive");
			RigidbodyController.ReturnToAnimationState();
		}
		HandsEquipment.OnLeaveProne();
	}

	public void UpdateBodyEquipmentModel()
	{
		if (base.CharacterAvatar == null || (bool)base.CharacterAvatar.BakedCharacter)
		{
			return;
		}
		foreach (ItemSlot allSlot in EntityData.Body.AllSlots)
		{
			IEnumerable<EquipmentEntity> enumerable = ExtractEquipmentEntities(allSlot);
			PFLog.TechArt.Log(string.Format("UpdateBodyEquipmentModel: slot='{0}', eesCount={1} -> [{2}]", allSlot?.GetType().Name, enumerable?.Count(), string.Join(", ", enumerable.Select((EquipmentEntity e) => e?.name))));
			base.CharacterAvatar.AddEquipmentEntities(enumerable, saved: false, isFromEquippedItems: true, allSlot);
			TryForceRampIndices(allSlot, enumerable);
		}
	}

	public void PreloadBodyEquipment()
	{
		Gender gender = EntityData.Gender;
		Race race = EntityData.Progression.Race?.RaceId ?? Race.Human;
		foreach (ItemSlot allSlot in EntityData.Body.AllSlots)
		{
			ItemEntity maybeItem = allSlot.MaybeItem;
			if (maybeItem != null && maybeItem.Blueprint is BlueprintItemEquipment { EquipmentEntity: not null } blueprintItemEquipment)
			{
				blueprintItemEquipment.EquipmentEntity.Preload(gender, race);
			}
		}
	}

	public void UpdateEquipmentColorRampIndices()
	{
		if (EntityData == null || base.CharacterAvatar == null)
		{
			return;
		}
		int num = EntityData.ViewSettings.Doll?.ClothesPrimaryIndex ?? (-1);
		int num2 = EntityData.ViewSettings.Doll?.ClothesSecondaryIndex ?? (-1);
		IEnumerable<KingmakerEquipmentEntity> unitEquipmentEntities = CharGenUtility.GetUnitEquipmentEntities(EntityData);
		if (!unitEquipmentEntities.Any())
		{
			return;
		}
		Race race = EntityData.Progression.Race?.RaceId ?? Race.Human;
		foreach (EquipmentEntityLink clothe in CharGenUtility.GetClothes(unitEquipmentEntities, EntityData.Gender, race))
		{
			EquipmentEntity ee = clothe.Load();
			base.CharacterAvatar.AddEquipmentEntity(ee, saved: false, isFromEquippedItem: false);
			if (num >= 0)
			{
				base.CharacterAvatar.SetPrimaryRampIndex(ee, num);
			}
			if (num2 >= 0)
			{
				base.CharacterAvatar.SetSecondaryRampIndex(ee, num2);
			}
		}
	}

	[Obsolete]
	public void PreloadClassEquipment()
	{
	}

	[Obsolete]
	public void UpdateClassEquipment()
	{
	}

	private IEnumerable<EquipmentEntity> ExtractEquipmentEntities([CanBeNull] ItemEntity item)
	{
		using (ProfileScope.NewScope("ExtractEquipmentEntities"))
		{
			if (item == null)
			{
				return Enumerable.Empty<EquipmentEntity>();
			}
			Gender gender = EntityData.Gender;
			Race race = EntityData.Progression.Race?.RaceId ?? Race.Human;
			return (item.Blueprint is BlueprintItemEquipment { EquipmentEntity: not null } blueprintItemEquipment) ? blueprintItemEquipment.EquipmentEntity.Load(gender, race) : Enumerable.Empty<EquipmentEntity>();
		}
	}

	public IEnumerable<EquipmentEntity> ExtractEquipmentEntities(ItemSlot slot)
	{
		using (ProfileScope.NewScope("ExtractEquipmentEntities"))
		{
			return (!slot.HasItem) ? Enumerable.Empty<EquipmentEntity>() : ExtractEquipmentEntities(slot.Item);
		}
	}

	public void TryForceRampIndicesFromDollRoom(ItemSlot slot, Character dollRoomCharacter)
	{
		TryForceRampIndices(slot, dollRoomCharacter.EquipmentEntities, dollRoomCharacter);
	}

	private void TryForceRampIndices(ItemSlot slot, IEnumerable<EquipmentEntity> ees, Character character = null)
	{
		Character character2 = ((character == null) ? base.CharacterAvatar : character);
		if (!slot.HasItem || character2 == null || !(slot.Item.Blueprint is BlueprintItemEquipment { ForcedRampColorPresetIndex: >=0 } blueprintItemEquipment))
		{
			return;
		}
		foreach (EquipmentEntity ee in ees)
		{
			if (!(ee.ColorPresets == null) && ee.ColorPresets.IndexPairs.Count > 0 && ee.ColorPresets.IndexPairs.Count - 1 >= blueprintItemEquipment.ForcedRampColorPresetIndex)
			{
				int primaryIndex = ee.ColorPresets.IndexPairs[blueprintItemEquipment.ForcedRampColorPresetIndex].PrimaryIndex;
				int secondaryIndex = ee.ColorPresets.IndexPairs[blueprintItemEquipment.ForcedRampColorPresetIndex].SecondaryIndex;
				character2.SetRampIndices(ee, primaryIndex, secondaryIndex);
			}
		}
	}

	private void UpdateStealthMarks()
	{
		bool active = EntityData.Stealth.Active;
		bool flag = EntityData.GetOptional<UnitPartMimic>();
		int num;
		if (active)
		{
			num = ((!flag) ? 1 : 0);
			if (num != 0 && !m_StealthEffect)
			{
				GameObject stealthEffectPrefab = Game.Instance.BlueprintRoot.StealthEffectPrefab;
				m_StealthEffect = FxHelper.SpawnFxOnEntity(stealthEffectPrefab, this);
			}
		}
		else
		{
			num = 0;
		}
		if (num == 0 && (bool)m_StealthEffect)
		{
			FxHelper.Destroy(m_StealthEffect);
			m_StealthEffect = null;
			FxHelper.SpawnFxOnEntity(Game.Instance.BlueprintRoot.ExitStealthEffectPrefab, this);
		}
	}

	private DeathFxFromEnergyEntry LastDamageFxOptions()
	{
		DamageData damageData = EntityData?.Health.LastHandledDamage?.Damage;
		if (damageData == null)
		{
			return DeathFxFromEnergyEntry.Default;
		}
		return BlueprintRoot.Instance.FxRoot.DeathFxOptionForEnergyDamage(damageData.Type);
	}

	public void UpdateLootLocalMapMark()
	{
		bool isDeadAndHasLoot = EntityData.IsDeadAndHasLoot;
		if (isDeadAndHasLoot && m_LootLocalMapMarker == null)
		{
			m_LootLocalMapMarker = new UnitLocalMapMarker(this);
			LocalMapModel.Markers.Add(m_LootLocalMapMarker);
			UpdateHighlight();
		}
		else if (!isDeadAndHasLoot && m_LootLocalMapMarker != null)
		{
			LocalMapModel.Markers.Remove(m_LootLocalMapMarker);
			m_LootLocalMapMarker = null;
			UpdateHighlight();
		}
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		HandsEquipment?.HandleEquipmentSetChanged();
		RefreshHighlighters();
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		using (ProfileScope.NewScope("HandleEquipmentSlotUpdated"))
		{
			if (EntityData == null || slot.Owner != EntityData)
			{
				return;
			}
			if (!(slot is HandSlot slot2))
			{
				if (slot is UsableSlot)
				{
					HandsEquipment.UpdateBeltPrefabs();
					RefreshHighlighters();
				}
			}
			else
			{
				HandsEquipment.HandleEquipmentSlotUpdated(slot2, previousItem);
				RefreshHighlighters();
			}
			if (!(base.CharacterAvatar == null))
			{
				IEnumerable<EquipmentEntity> enumerable = ExtractEquipmentEntities(previousItem);
				base.CharacterAvatar.RemoveEquipmentEntities(enumerable);
				PFLog.TechArt.Log(string.Format("HandleEquipmentSlotUpdated: slot='{0}', eesCount={1} -> [{2}]", slot?.GetType().Name, enumerable?.Count(), string.Join(", ", enumerable.Select((EquipmentEntity e) => e?.name))));
				enumerable = ExtractEquipmentEntities(slot);
				base.CharacterAvatar.AddEquipmentEntities(enumerable, saved: false, isFromEquippedItems: true, slot);
				base.CharacterAvatar.IsDirty = true;
				TryForceRampIndices(slot, enumerable);
			}
		}
	}

	protected override void OnDoLateUpdate()
	{
		base.OnDoLateUpdate();
		if (EntityData == null)
		{
			return;
		}
		UpdateStealthMarks();
		UpdateLootLocalMapMark();
		UpdateBuiltinArmorView();
		float sizeScale = GetSizeScale();
		if (!sizeScale.Equals(m_Scale) && !DoNotAdjustScale)
		{
			float num = sizeScale - m_Scale;
			float deltaTime = Game.Instance.TimeController.DeltaTime;
			float num2 = num * deltaTime * 2f;
			m_Scale = ((num > 0f) ? Math.Min(sizeScale, m_Scale + num2) : Math.Max(sizeScale, m_Scale + num2));
			base.ViewTransform.localScale = m_OriginalScale * m_Scale;
		}
		if ((bool)ParticlesSnapMap)
		{
			ParticlesSnapMap.AdditionalScale = base.ViewTransform.localScale.x / m_OriginalScale.x;
		}
		bool valueOrDefault = (EntityData.CutsceneControlledUnit?.GetCurrentlyActive()?.LockControl).GetValueOrDefault();
		if (valueOrDefault == m_ParticipateInLockControlCutscene)
		{
			return;
		}
		foreach (Renderer renderer in Renderers)
		{
			if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				skinnedMeshRenderer.updateWhenOffscreen = valueOrDefault;
			}
		}
		m_ParticipateInLockControlCutscene = valueOrDefault;
	}

	public void HandleAttackCommandRun()
	{
		HandsEquipment?.SetCombatVisualState(inCombat: true);
	}

	public void HandleAttackCommandEnd()
	{
		HandsEquipment?.SetCombatVisualState(inCombat: false);
	}

	public void AnimateWeaponTrail(float duration)
	{
	}

	protected override void HandleOnDestroy()
	{
		if (m_LootLocalMapMarker != null)
		{
			LocalMapModel.Markers.Remove(m_LootLocalMapMarker);
			m_LootLocalMapMarker = null;
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		if (EntityData != null)
		{
			Gizmos.color = Color.gray;
			DebugDraw.DrawCircle(base.ViewTransform.position, Vector3.up, EntityData.Vision.RangeMeters);
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		BaseUnitEntity entityData = EntityData;
		if (entityData != null && !entityData.IsSleeping)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(EntityData.Position, EntityData.Position + Vector3.up * 5f);
		}
	}

	protected override void OnVisibilityChanged()
	{
		HandsEquipment?.UpdateVisibility(base.IsVisible);
		base.OnVisibilityChanged();
	}

	public override void OnInFogOfWarChanged()
	{
		base.OnInFogOfWarChanged();
		UpdateHighlight();
	}

	public override void ForcePeacefulLook(bool peaceful)
	{
		base.ForcePeacefulLook(peaceful);
		if (base.CharacterAvatar != null && HandsEquipment != null)
		{
			HandsEquipment.UpdateVisibility(base.IsVisible);
			HandsEquipment.UpdateBeltPrefabs();
		}
	}

	public float GetSizeScale()
	{
		if (EntityData == null || DisableSizeScaling)
		{
			return 1f;
		}
		Size originalSize = EntityData.OriginalSize;
		Size size = EntityData.State.Size;
		float num = 1f;
		if (originalSize == size)
		{
			return num;
		}
		int num2 = size - originalSize;
		for (int i = 0; i < Math.Abs(num2); i++)
		{
			num = ((num2 < 0) ? (num * 0.66f) : (num / 0.66f));
		}
		return num;
	}

	public void HideOffWeapon(bool hide)
	{
		HandsEquipment.HideOffWeapon(hide);
	}

	[NotNull]
	public FogOfWarRevealerSettings SureFogOfWarRevealer()
	{
		if (FogOfWarRevealer != null)
		{
			return FogOfWarRevealer;
		}
		FogOfWarRevealer = base.gameObject.GetComponent<FogOfWarRevealerSettings>();
		return FogOfWarRevealer ?? (FogOfWarRevealer = base.gameObject.AddComponent<FogOfWarRevealerSettings>());
	}

	public override float GetSpeedAnimationCoeff(WalkSpeedType type, bool inCombat)
	{
		BlueprintRace blueprintRace = EntityData?.Progression.Race;
		if ((bool)base.CharacterAvatar && !blueprintRace)
		{
			blueprintRace = BlueprintRoot.Instance.Progression.CharacterRaces.FirstOrDefault((BlueprintRace r) => r.RaceId == Race.Human);
		}
		UnitAnimationSettings unitAnimationSettings = null;
		if (!m_IgnoreRaceAnimationSettings)
		{
			unitAnimationSettings = blueprintRace?.GetSpeedSettings((EntityData?.Description.Gender).Value);
		}
		if (unitAnimationSettings == null)
		{
			unitAnimationSettings = m_AnimationSettings;
		}
		return (unitAnimationSettings?.GetCoeff(type, inCombat) ?? 1f) / GetSizeScale();
	}

	protected override void OnWillDetachFromData()
	{
		base.OnWillDetachFromData();
		HandsEquipment?.Dispose();
	}

	public void AddRagdollImpulse(Vector3 direction, float addMagnitude, DamageType damageType, bool isCutscene = false)
	{
		if ((bool)RigidbodyController)
		{
			if (isCutscene)
			{
				RigidbodyController.maxRagdollValue = addMagnitude;
			}
			RigidbodyController.ApplyImpulse(direction, addMagnitude);
		}
		if ((bool)base.DismembermentManager)
		{
			base.DismembermentManager.ApplyImpulse(direction, addMagnitude, damageType);
		}
		if ((bool)m_Swarm)
		{
			m_Swarm.ApplyImpulse(direction, addMagnitude);
		}
	}

	private void UpdateBuiltinArmorView()
	{
		ArmorProficiencyGroup armorProficiencyGroup = Data.Body.Armor.MaybeArmor?.Blueprint.Type.ProficiencyGroup ?? ArmorProficiencyGroup.None;
		bool flag = armorProficiencyGroup == ArmorProficiencyGroup.Light || armorProficiencyGroup == ArmorProficiencyGroup.Medium;
		bool flag2 = armorProficiencyGroup == ArmorProficiencyGroup.Heavy || armorProficiencyGroup == ArmorProficiencyGroup.Power;
		if (m_BuiltinArmorMediumView != null)
		{
			if (flag && !m_BuiltinArmorMediumView.activeSelf)
			{
				m_BuiltinArmorMediumView.SetActive(value: true);
			}
			else if (!flag && m_BuiltinArmorMediumView.activeSelf)
			{
				m_BuiltinArmorMediumView.SetActive(value: false);
			}
		}
		if (m_BuiltinArmorHeavyView != null)
		{
			if (flag2 && !m_BuiltinArmorHeavyView.activeSelf)
			{
				m_BuiltinArmorHeavyView.SetActive(value: true);
			}
			else if (!flag2 && m_BuiltinArmorHeavyView.activeSelf)
			{
				m_BuiltinArmorHeavyView.SetActive(value: false);
			}
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		UpdateHighlight();
	}

	public void SetOccluderClippingEnabled(bool value)
	{
		if (value)
		{
			if ((object)m_OcclusionGeometryClipTarget == null)
			{
				m_OcclusionGeometryClipTarget = base.gameObject.EnsureComponent<OcclusionGeometryClipTarget>();
				m_OcclusionGeometryClipTarget.ActivationMode = OcclusionGeometryClipTarget.ActivationModeType.Manual;
			}
			m_OcclusionGeometryClipTarget.ClippingEnabled = true;
		}
		else if ((object)m_OcclusionGeometryClipTarget != null)
		{
			m_OcclusionGeometryClipTarget.ClippingEnabled = false;
		}
	}

	public void HandleCellAbility(List<AbilityTargetUIData> abilityTargets)
	{
		bool isInAoePattern = m_IsInAoePattern;
		m_IsInAoePattern = abilityTargets.Any((AbilityTargetUIData n) => n.Target == Data);
		if (isInAoePattern != m_IsInAoePattern)
		{
			UpdateHighlight();
		}
	}

	public void HandleAoEMove(Vector3 pos, AbilityData ability)
	{
	}

	public void HandleAoECancel()
	{
		bool isInAoePattern = m_IsInAoePattern;
		m_IsInAoePattern = false;
		if (isInAoePattern != m_IsInAoePattern)
		{
			UpdateHighlight();
		}
	}

	public void SpawnAttachedDroppedLoot()
	{
		if (Data.IsDeadAndHasLoot && Data.Inventory.IsLootDroppedAsAttached && !(m_AttachedDroppedLoot != null))
		{
			PartInventory.DroppedLootData attachedDroppedLootData = Data.Inventory.AttachedDroppedLootData;
			GameObject original = BlueprintRoot.Instance.Prefabs.DroppedLootBagAttachedLink.Load();
			m_AttachedDroppedLoot = UnityEngine.Object.Instantiate(original, attachedDroppedLootData.Position, Quaternion.identity);
			UnitHelper.UpdateDropTransform(Data, m_AttachedDroppedLoot.transform, attachedDroppedLootData.Rotation);
			if (RigidbodyController != null)
			{
				RigidbodyController.EnsureComponent<HighlighterBlockerHierarchy>();
			}
			m_AttachedDroppedLoot.transform.SetParent(base.ViewTransform);
			RefreshHighlighters();
		}
	}

	public void HandleLootDroppedAsAttached()
	{
		SpawnAttachedDroppedLoot();
	}
}
