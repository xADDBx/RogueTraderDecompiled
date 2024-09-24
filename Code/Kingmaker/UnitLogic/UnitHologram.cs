using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class UnitHologram : MonoBehaviour
{
	private Character m_OriginalAvatar;

	private BaseUnitEntity m_OriginalBaseUnit;

	private Character m_HologramAvatar;

	private UnitViewHandsEquipment m_AvatarHands;

	private GameObject m_Shading;

	private UnitEntityView m_HologramEntityView;

	private bool m_IsStarshipHologram;

	private UnitAnimationManager AnimationManager => m_HologramAvatar?.AnimationManager;

	public LosCalculations.CoverType CoverType
	{
		get
		{
			return AnimationManager?.CoverType ?? LosCalculations.CoverType.None;
		}
		set
		{
			if (AnimationManager != null)
			{
				AnimationManager.CoverType = value;
			}
		}
	}

	public Vector3 Direction
	{
		get
		{
			return base.transform.forward;
		}
		set
		{
			value.y = 0f;
			base.transform.forward = value;
			if (AnimationManager != null)
			{
				AnimationManager.Orientation = Quaternion.LookRotation(value).eulerAngles.y;
			}
		}
	}

	public BaseUnitEntity Parent => m_OriginalBaseUnit;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		Object.Destroy(m_Shading);
	}

	public void Setup([NotNull] UnitEntityView hologramUnit, [NotNull] UnitEntityView originalUnit)
	{
		m_OriginalBaseUnit = originalUnit.EntityData;
		m_HologramEntityView = hologramUnit;
		Character characterAvatar = hologramUnit.CharacterAvatar;
		m_OriginalAvatar = characterAvatar;
		m_HologramAvatar = SetupAvatar(m_OriginalAvatar);
		m_AvatarHands = originalUnit.HandsEquipment;
		hologramUnit.Blueprint = originalUnit.Blueprint;
		SetupShading(BlueprintRoot.Instance.FxRoot.Hologram.MainFx);
	}

	public void SetupStarship([NotNull] UnitEntityView hologramUnit, [NotNull] UnitEntityView originalUnit)
	{
		m_IsStarshipHologram = true;
		m_OriginalBaseUnit = originalUnit.EntityData;
		m_HologramEntityView = hologramUnit;
		hologramUnit.Blueprint = originalUnit.Blueprint;
		SetupShading(BlueprintRoot.Instance.FxRoot.Hologram.MainFx);
	}

	[NotNull]
	private Character SetupAvatar(Character originalAvatar)
	{
		Character component = base.gameObject.GetComponent<Character>();
		component.PreventUpdate = false;
		component.transform.localScale = originalAvatar.transform.localScale;
		component.IsInDollRoom = true;
		component.ForbidBeltItemVisualization = originalAvatar.ForbidBeltItemVisualization;
		component.AnimatorPrefab = originalAvatar.AnimatorPrefab;
		component.OnStart();
		if (component.Animator != null)
		{
			if (!component.Animator.gameObject.GetComponent<UnitAnimationCallbackReceiver>())
			{
				component.Animator.gameObject.AddComponent<UnitAnimationCallbackReceiver>();
			}
			component.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		if (component.AnimationManager != null)
		{
			component.AnimationManager.AtachToView(m_HologramEntityView, m_OriginalBaseUnit?.Progression.Race);
			if ((bool)component.AnimationManager)
			{
				component.AnimationManager.IsInCombat = true;
				component.AnimationManager.Tick(RealTimeController.SystemStepDurationSeconds);
			}
		}
		return component;
	}

	private void Update()
	{
		if (!m_IsStarshipHologram && m_HologramAvatar.AnimationManager != null)
		{
			m_HologramAvatar.AnimationManager.ActiveMainHandWeaponStyle = m_AvatarHands.ActiveMainHandWeaponStyle;
			m_HologramAvatar.AnimationManager.ActiveOffHandWeaponStyle = m_AvatarHands.ActiveOffHandWeaponStyle;
			m_HologramAvatar.AnimationManager.Tick(Time.deltaTime);
			UnitViewHandsEquipment avatarHands = m_AvatarHands;
			if (avatarHands != null && !avatarHands.InCombat)
			{
				m_AvatarHands.IsUsingHologram = true;
				m_AvatarHands.SetCombatVisualState(inCombat: true);
				m_AvatarHands.MatchWithCurrentCombatState();
				m_AvatarHands.ForceEndChangeEquipment();
				m_AvatarHands.AnimationManager.CustomUpdate(2f);
				m_AvatarHands.AnimationManager.CustomUpdate(2f);
			}
		}
	}

	public void LookAt(Vector3 position)
	{
		Direction = (position - base.transform.position).normalized;
	}

	public void LookAt(MechanicEntity entity)
	{
		LookAt(entity.Position);
	}

	public void SetupShading(PrefabLink fx = null)
	{
		if (m_Shading != null)
		{
			Object.Destroy(m_Shading);
		}
		GameObject prefab = fx.Load();
		if (fx != null && m_HologramEntityView != null)
		{
			m_Shading = FxHelper.SpawnFxOnEntity(prefab, m_HologramEntityView);
		}
	}
}
