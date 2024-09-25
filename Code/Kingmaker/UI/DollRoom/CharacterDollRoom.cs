using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Trails;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.UniRx;
using Owlcat.Runtime.Visual.DxtCompressor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace Kingmaker.UI.DollRoom;

public class CharacterDollRoom : DollRoomBase, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IEntitySubscriber, IUnitActiveEquipmentSetHandler<EntitySubscriber>, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitActiveEquipmentSetHandler, EntitySubscriber>, IUnitVisualChangeHandler
{
	private BaseUnitEntity m_Unit;

	private UnitViewHandsEquipment m_AvatarHands;

	private UnitViewMechadendritesEquipment m_Mechadendrites;

	[SerializeField]
	protected GameObject m_PlatformPrefab;

	private Character m_OriginalAvatar;

	protected Character m_Avatar;

	private UnitEntityView m_SimpleAvatar;

	private GameObject m_SnowDecal;

	private readonly List<Renderer> m_TempRenderers = new List<Renderer>();

	private readonly List<ParticleSystem> m_TempParticles = new List<ParticleSystem>();

	private readonly List<ParticlesMaterialController> m_TempMatControllers = new List<ParticlesMaterialController>();

	private readonly List<CompositeTrailRenderer> m_TempTrails = new List<CompositeTrailRenderer>();

	protected GameObject m_PlatformInstance;

	public BaseUnitEntity Unit => m_Unit;

	public IEntity GetSubscribingEntity()
	{
		return m_Unit;
	}

	public override void Show()
	{
		base.Show();
		SetupDollPostProcessAndAnimation(isCharGen: false);
		UISounds.Instance.Sounds.Character.CharacterDollAnimationShow.Play();
		if ((bool)m_PlatformPrefab && m_PlatformInstance == null)
		{
			m_PlatformInstance = Object.Instantiate(m_PlatformPrefab, m_TargetPlaceholder.transform);
			m_PlatformInstance.transform.localPosition = Vector3.zero;
			m_PlatformInstance.transform.rotation = Quaternion.identity;
		}
	}

	public override void Hide()
	{
		base.Hide();
		if ((bool)m_PlatformInstance)
		{
			Object.Destroy(m_PlatformInstance);
		}
		if ((bool)m_OriginalAvatar)
		{
			m_OriginalAvatar.enabled = true;
			m_OriginalAvatar = null;
		}
	}

	public virtual void SetupUnit(BaseUnitEntity player)
	{
		if (m_Unit == player)
		{
			return;
		}
		PFLog.UI.Log("SetupInfo");
		Cleanup();
		m_Unit = player;
		EventBus.Unsubscribe(this);
		EventBus.Subscribe(this);
		m_OriginalAvatar = player.View.Or(null)?.CharacterAvatar;
		if (m_OriginalAvatar == null)
		{
			CreateSimpleAvatar(player);
			SetupAnimationManager(m_SimpleAvatar.GetComponentInChildren<UnitAnimationManager>());
			return;
		}
		Character character = CreateAvatar(m_OriginalAvatar, m_Unit.ToString());
		SetAvatar(character);
		IKController iKController = m_Avatar.gameObject.AddComponent<IKController>();
		iKController.IsDollRoom = true;
		iKController.CharacterSystem = m_Avatar;
		if ((bool)m_OriginalAvatar.GetComponent<UnitEntityView>())
		{
			iKController.CharacterUnitEntity = m_OriginalAvatar.GetComponent<UnitEntityView>();
		}
		SetupAnimationManager(character.AnimationManager);
		HashSet<UnitAnimationManager> mechsAnimationManagers = character.MechsAnimationManagers;
		if (mechsAnimationManagers == null || mechsAnimationManagers.Count <= 0)
		{
			return;
		}
		foreach (UnitAnimationManager mechsAnimationManager in character.MechsAnimationManagers)
		{
			if (mechsAnimationManager != null)
			{
				SetupAnimationManager(mechsAnimationManager);
			}
		}
	}

	protected override void Cleanup()
	{
		m_TargetPlaceholder.rotation = Quaternion.identity;
		BaseUnitEntity unit = m_Unit;
		if ((bool)m_Avatar)
		{
			Character[] componentsInChildren = m_TargetPlaceholder.GetComponentsInChildren<Character>(includeInactive: true);
			foreach (Character obj in componentsInChildren)
			{
				obj.OnUpdated -= OnCharacterUpdated;
				Object.Destroy(obj.gameObject);
			}
			m_Avatar = null;
			m_AvatarHands?.Dispose();
			m_AvatarHands = null;
			m_Mechadendrites = null;
			m_Unit = null;
			if (unit?.View != null)
			{
				unit?.View.HandsEquipment.UpdateVisibility(unit.View.IsVisible);
			}
		}
		if ((bool)m_SimpleAvatar)
		{
			Object.Destroy(m_SimpleAvatar.gameObject);
			m_SimpleAvatar = null;
		}
		base.Cleanup();
	}

	private void UpdateAvatarRenderers()
	{
		GameObject gameObject = (m_Avatar ? m_Avatar.gameObject : m_SimpleAvatar.gameObject);
		if (!gameObject)
		{
			return;
		}
		gameObject.GetComponentsInChildren(m_TempRenderers);
		foreach (Renderer tempRenderer in m_TempRenderers)
		{
			tempRenderer.gameObject.layer = 15;
		}
		UnscaleFxTimes(gameObject);
		WeaponParticlesSnapMap weaponParticlesSnapMap = m_AvatarHands?.GetWeaponModel(offHand: false)?.GetComponent<WeaponParticlesSnapMap>();
		if ((bool)weaponParticlesSnapMap && weaponParticlesSnapMap == m_Unit?.Body?.PrimaryHand?.FxSnapMap)
		{
			foreach (ItemEnchantment item in (m_Unit?.Body?.PrimaryHand?.MaybeItem?.Enchantments).EmptyIfNull())
			{
				UnscaleFxTimes(item.FxObject);
			}
		}
		weaponParticlesSnapMap = m_AvatarHands?.GetWeaponModel(offHand: true)?.GetComponent<WeaponParticlesSnapMap>();
		if (!weaponParticlesSnapMap || !(weaponParticlesSnapMap == m_Unit?.Body?.SecondaryHand?.FxSnapMap))
		{
			return;
		}
		foreach (ItemEnchantment item2 in (m_Unit?.Body?.SecondaryHand?.MaybeItem?.Enchantments).EmptyIfNull())
		{
			UnscaleFxTimes(item2.FxObject);
		}
	}

	private void UnscaleFxTimes(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		PooledGameObject component = obj.GetComponent<PooledGameObject>();
		if ((bool)component)
		{
			Object.Destroy(component);
		}
		obj.GetComponentsInChildren(m_TempRenderers);
		foreach (Renderer tempRenderer in m_TempRenderers)
		{
			tempRenderer.gameObject.layer = 15;
			tempRenderer.lightProbeUsage = LightProbeUsage.Off;
		}
		obj.GetComponentsInChildren(m_TempParticles);
		foreach (ParticleSystem tempParticle in m_TempParticles)
		{
			ParticleSystem.MainModule main = tempParticle.main;
			main.useUnscaledTime = true;
		}
		obj.GetComponentsInChildren(m_TempTrails);
		foreach (CompositeTrailRenderer tempTrail in m_TempTrails)
		{
			tempTrail.Emitters.ForEach(delegate(TrailEmitter e)
			{
				e.UseUnscaledTime = true;
			});
		}
		obj.GetComponentsInChildren(m_TempMatControllers);
		ParticlesMaterialController[] componentsInChildren = obj.GetComponentsInChildren<ParticlesMaterialController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UnscaledTime = true;
		}
		FxFadeOut component2 = obj.GetComponent<FxFadeOut>();
		if ((bool)component2)
		{
			component2.Duration = 0f;
		}
	}

	private void CreateSimpleAvatar(BaseUnitEntity player)
	{
		m_SimpleAvatar = Object.Instantiate(player.View, m_TargetPlaceholder, worldPositionStays: false);
		Transform viewTransform = m_SimpleAvatar.ViewTransform;
		viewTransform.localPosition = Vector3.zero;
		viewTransform.localRotation = Quaternion.identity;
		viewTransform.localScale = player.View.ViewTransform.localScale;
		if (!m_SimpleAvatar.gameObject.activeSelf)
		{
			m_SimpleAvatar.gameObject.SetActive(value: true);
			Renderer[] componentsInChildren = m_SimpleAvatar.GetComponentsInChildren<Renderer>();
			if (componentsInChildren.Any((Renderer r) => !r.enabled))
			{
				PFLog.UI.Warning("SimpleAvatar has disabled renderers " + player.View.gameObject.name);
			}
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}
		UpdateAvatarRenderers();
		Bounds bounds = (from r in m_SimpleAvatar.GetComponentsInChildren<Renderer>()
			select r.bounds).Aggregate(SumBounds);
		if (bounds.size.y * viewTransform.localScale.y > 2.5f)
		{
			float num = 2.5f / (bounds.size.y * viewTransform.localScale.y);
			viewTransform.localScale *= num;
		}
		if ((bool)m_Camera)
		{
			string text = m_SimpleAvatar.CharacterAvatar?.Skeleton?.DollRoomZoomPreset.TargetBoneName ?? "Head";
			Transform targetTransform = viewTransform.FindChildRecursive(text);
			m_Camera.LookAt(targetTransform, m_SimpleAvatar.CharacterAvatar?.Skeleton?.DollRoomZoomPreset);
		}
	}

	protected void SetupAnimationManager(UnitAnimationManager animationManager)
	{
		if ((bool)animationManager)
		{
			animationManager.PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);
			animationManager.IsInCombat = true;
			animationManager.Tick(RealTimeController.SystemStepDurationSeconds);
		}
	}

	private static Bounds SumBounds(Bounds b1, Bounds b2)
	{
		b1.Encapsulate(b2.min);
		b1.Encapsulate(b2.max);
		return b1;
	}

	[NotNull]
	protected Character CreateAvatar(Character originalAvatar, string dollName)
	{
		Character character = new GameObject("Doll [" + dollName + "]").AddComponent<Character>();
		character.PreventUpdate = false;
		character.transform.localScale = originalAvatar.transform.localScale;
		character.IsInDollRoom = true;
		character.ForbidBeltItemVisualization = originalAvatar.ForbidBeltItemVisualization;
		character.AnimatorPrefab = originalAvatar.AnimatorPrefab;
		character.Skeleton = originalAvatar.Skeleton;
		character.AnimationSet = originalAvatar.AnimationSet;
		character.OnUpdated += OnCharacterUpdated;
		character.AtlasData = originalAvatar.AtlasData;
		character.CopyEquipmentFrom(originalAvatar);
		character.OnStart();
		character.Animator.gameObject.AddComponent<UnitAnimationCallbackReceiver>();
		character.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		character.transform.SetParent(m_TargetPlaceholder, worldPositionStays: false);
		character.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
		return character;
	}

	protected void SetAvatar(Character avatar, bool activateAvatar = true)
	{
		if (!(m_Avatar == avatar) || !avatar.gameObject.activeSelf)
		{
			if (m_Avatar != null)
			{
				m_Avatar.gameObject.SetActive(value: false);
			}
			m_Avatar = avatar;
			m_Avatar.gameObject.SetActive(activateAvatar);
			if ((bool)m_Camera)
			{
				string text = avatar.Skeleton?.DollRoomZoomPreset.TargetBoneName ?? "Head";
				Transform targetTransform = avatar.transform.FindChildRecursive(text);
				m_Camera.LookAt(targetTransform, m_Avatar.Skeleton.DollRoomZoomPreset);
			}
			m_AvatarHands?.Dispose();
			m_AvatarHands = null;
			m_Mechadendrites = null;
			UpdateCharacter();
		}
	}

	private void OnCharacterUpdated(Character character)
	{
		Renderer[] componentsInChildren = character.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 15;
		}
		character.GetComponentsInChildren(m_TempRenderers);
		if (character == m_Avatar)
		{
			UpdateCharacter();
		}
	}

	private void UpdateCharacter()
	{
		m_Avatar.UpdateSkeleton();
		if (m_Mechadendrites == null && m_Unit != null)
		{
			m_Mechadendrites = new UnitViewMechadendritesEquipment(m_Unit.View, m_Avatar);
			m_Mechadendrites.UpdateAll();
		}
		if (m_AvatarHands?.Owner == null)
		{
			m_AvatarHands?.Dispose();
			m_AvatarHands = null;
			if (m_Unit != null && m_Unit.View != null)
			{
				m_AvatarHands = new UnitViewHandsEquipment(m_Unit.View, m_Avatar);
				m_AvatarHands.OnModelSpawned += UpdateAvatarRenderers;
				m_AvatarHands.UpdateAll();
			}
		}
		Animator[] componentsInChildren = m_Avatar.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		UpdateAvatarRenderers();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Hide();
	}

	[UsedImplicitly]
	private void Update()
	{
		if (Game.HasInstance && Game.Instance.CurrentMode == GameModeType.None)
		{
			Services.GetInstance<DxtCompressorService>()?.Update();
			TempList.Release();
			TempHashSet.Release();
		}
		UpdateInternal();
		if (!m_OriginalAvatar || !m_Avatar)
		{
			if ((bool)m_SimpleAvatar)
			{
				UnitAnimationManager componentInChildren = m_SimpleAvatar.GetComponentInChildren<UnitAnimationManager>();
				if ((bool)componentInChildren)
				{
					componentInChildren.Tick(Time.deltaTime);
				}
			}
			return;
		}
		if ((bool)m_PlatformInstance)
		{
			m_PlatformInstance.transform.rotation = m_Avatar.transform.rotation;
		}
		if (m_Avatar.MechsAnimationManagers.Count > 0)
		{
			WeaponAnimationStyle weaponAnimationStyle = ((m_AvatarHands.GetSelectedWeaponSet().MainHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: not false }) ? m_AvatarHands.ActiveMainHandWeaponStyle : ((m_AvatarHands.GetSelectedWeaponSet().OffHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: not false }) ? m_AvatarHands.ActiveOffHandWeaponStyle : WeaponAnimationStyle.None));
			WeaponAnimationStyle weaponAnimationStyle2 = ((m_AvatarHands.GetSelectedWeaponSet().MainHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: false }) ? m_AvatarHands.ActiveMainHandWeaponStyle : ((m_AvatarHands.GetSelectedWeaponSet().OffHand.VisibleItem?.Blueprint is BlueprintItemWeapon { IsMelee: false }) ? m_AvatarHands.ActiveOffHandWeaponStyle : WeaponAnimationStyle.None));
			m_Avatar.AnimationManager.ActiveMainHandWeaponStyle = weaponAnimationStyle;
			m_Avatar.AnimationManager.ActiveOffHandWeaponStyle = weaponAnimationStyle;
			m_Avatar.AnimationManager.Tick(Time.deltaTime);
			foreach (UnitAnimationManager mechsAnimationManager in m_Avatar.MechsAnimationManagers)
			{
				mechsAnimationManager.ActiveMainHandWeaponStyle = weaponAnimationStyle2;
				mechsAnimationManager.ActiveOffHandWeaponStyle = weaponAnimationStyle2;
				mechsAnimationManager.Tick(Time.deltaTime);
			}
		}
		else
		{
			m_Avatar.AnimationManager.ActiveMainHandWeaponStyle = m_AvatarHands.ActiveMainHandWeaponStyle;
			m_Avatar.AnimationManager.ActiveOffHandWeaponStyle = m_AvatarHands.ActiveOffHandWeaponStyle;
			m_Avatar.AnimationManager.Tick(Time.deltaTime);
			foreach (UnitAnimationManager mechsAnimationManager2 in m_Avatar.MechsAnimationManagers)
			{
				mechsAnimationManager2.ActiveMainHandWeaponStyle = m_AvatarHands.ActiveMainHandWeaponStyle;
				mechsAnimationManager2.ActiveOffHandWeaponStyle = m_AvatarHands.ActiveOffHandWeaponStyle;
				mechsAnimationManager2.Tick(Time.deltaTime);
			}
		}
		if (m_AvatarHands != null && !m_AvatarHands.InCombat)
		{
			m_AvatarHands.SetCombatVisualState(inCombat: true);
			m_AvatarHands.MatchWithCurrentCombatState();
		}
		Physics.Simulate(Time.unscaledDeltaTime);
	}

	protected virtual void UpdateInternal()
	{
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (slot.Owner != m_Unit)
		{
			return;
		}
		IEnumerable<EquipmentEntity> enumerable = m_OriginalAvatar.EquipmentEntities.Concat(m_OriginalAvatar.SavedEquipmentEntities.Select((EquipmentEntityLink ee) => ee.Load()));
		IEnumerable<EquipmentEntity> enumerable2 = m_Avatar.EquipmentEntities.Concat(m_Avatar.SavedEquipmentEntities.Select((EquipmentEntityLink ee) => ee.Load()));
		EquipmentEntity[] ees = enumerable2.Except(enumerable).ToArray();
		EquipmentEntity[] ees2 = enumerable.Except(enumerable2).ToArray();
		IEnumerable<Character.SelectedRampIndices> collection = m_OriginalAvatar.RampIndices.Except(m_Avatar.RampIndices);
		m_Avatar.RampIndices.Clear();
		m_Avatar.RampIndices.AddRange(collection);
		m_Avatar.IsAtlasesDirty = true;
		m_Avatar.RemoveEquipmentEntities(ees);
		m_Avatar.AddEquipmentEntities(ees2);
		if (slot is HandSlot slot2)
		{
			m_AvatarHands?.HandleEquipmentSlotUpdated(slot2, previousItem);
		}
		else
		{
			PartUnitBody bodyOptional = slot.Owner.GetBodyOptional();
			if (bodyOptional != null && bodyOptional.QuickSlots.Contains(slot))
			{
				m_AvatarHands?.UpdateBeltPrefabs();
			}
		}
		if ((bool)m_OriginalAvatar && m_Unit != null && !m_OriginalAvatar.gameObject.activeInHierarchy && (bool)m_Unit.View)
		{
			m_Unit.View.HandleEquipmentSlotUpdated(slot, previousItem);
		}
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		m_AvatarHands?.HandleEquipmentSetChanged();
	}

	public void HandleUnitChangeEquipmentColor(int rampIndex, bool secondary)
	{
		if (m_Unit.View.Or(null)?.CharacterAvatar != null)
		{
			DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
			{
				m_Avatar.CopyRampIndicesFrom(m_Unit.View.CharacterAvatar);
			});
		}
	}
}
