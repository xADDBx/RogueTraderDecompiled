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
	private BaseUnitEntity m_OriginalBaseUnit;

	private UnitViewHandsEquipment m_AvatarHands;

	private GameObject m_Shading;

	private UnitEntityView m_HologramEntityView;

	private UnitAnimationManager m_AnimationManager;

	private bool m_IsStarshipHologram;

	private UnitAnimationManager AnimationManager => m_AnimationManager;

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
		SetupAvatar(hologramUnit.CharacterAvatar);
		m_AvatarHands = ((originalUnit.HandsEquipment.Character != null) ? originalUnit.HandsEquipment : null);
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

	private void SetupAvatar(Character originalAvatar)
	{
		Character component = base.gameObject.GetComponent<Character>();
		if (component == null || originalAvatar == null)
		{
			UnitAnimationManager componentInChildren = base.gameObject.GetComponentInChildren<UnitAnimationManager>();
			if (!(componentInChildren == null))
			{
				SetupAnimator(componentInChildren.Animator);
				SetupAnimationManager(componentInChildren);
			}
		}
		else
		{
			component.PreventUpdate = false;
			component.transform.localScale = originalAvatar.transform.localScale;
			component.IsInDollRoom = true;
			component.ForbidBeltItemVisualization = originalAvatar.ForbidBeltItemVisualization;
			component.AnimatorPrefab = originalAvatar.AnimatorPrefab;
			component.OnStart();
			SetupAnimator(component.Animator);
			SetupAnimationManager(component.AnimationManager);
		}
	}

	private static void SetupAnimator(Animator animator)
	{
		if (!(animator == null))
		{
			if (!animator.gameObject.GetComponent<UnitAnimationCallbackReceiver>())
			{
				animator.gameObject.AddComponent<UnitAnimationCallbackReceiver>();
			}
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
	}

	private void SetupAnimationManager(UnitAnimationManager animationManager)
	{
		if (!(animationManager == null))
		{
			m_AnimationManager = animationManager;
			animationManager.AttachToView(m_HologramEntityView, m_OriginalBaseUnit?.Progression.Race);
			animationManager.IsInCombat = true;
			animationManager.OnAnimationSetChanged();
			animationManager.Tick(RealTimeController.SystemStepDurationSeconds);
		}
	}

	private void Update()
	{
		if (!m_IsStarshipHologram && AnimationManager != null && m_AvatarHands != null)
		{
			AnimationManager.ActiveMainHandWeaponStyle = m_AvatarHands.ActiveMainHandWeaponStyle;
			AnimationManager.ActiveOffHandWeaponStyle = m_AvatarHands.ActiveOffHandWeaponStyle;
			AnimationManager.Tick(Time.deltaTime);
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
