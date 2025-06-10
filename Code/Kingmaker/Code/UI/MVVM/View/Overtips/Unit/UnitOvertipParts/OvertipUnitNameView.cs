using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipUnitNameView : ViewBase<OvertipHitChanceBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_NameText;

	private CanvasRenderer m_CharacterNameCanvasRenderer;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private List<RectTransform> m_ContainersList;

	private CanvasRenderer CharacterNameCanvasRenderer => m_CharacterNameCanvasRenderer ?? (m_CharacterNameCanvasRenderer = m_NameText.GetComponent<CanvasRenderer>());

	private bool CheckDestructible
	{
		get
		{
			if (!base.ViewModel.UnitState.IsDestructible.Value)
			{
				return true;
			}
			if (!base.ViewModel.UnitState.IsTBM.Value)
			{
				return false;
			}
			if (base.ViewModel.UnitState.IsDestructibleNotCover.Value)
			{
				return true;
			}
			return base.ViewModel.UnitState.IsMouseOverUnit.Value;
		}
	}

	private bool CheckVisibleTrigger
	{
		get
		{
			if ((!base.ViewModel.UnitState.IsTBM.Value || !base.ViewModel.UnitState.IsCurrentUnitTurn.Value) && !base.ViewModel.UnitState.ForceHotKeyPressed.Value)
			{
				return base.ViewModel.UnitState.IsMouseOverUnit.Value;
			}
			return true;
		}
	}

	private bool CheckCanBeVisible
	{
		get
		{
			if (base.ViewModel.UnitState.Unit.IsVisibleForPlayer && !base.ViewModel.UnitState.HasHiddenCondition.Value)
			{
				if (base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Value)
				{
					return !base.ViewModel.UnitState.IsTBM.Value;
				}
				return true;
			}
			return false;
		}
	}

	private bool AimBlockVisible
	{
		get
		{
			if (!base.ViewModel.IsVisibleTrigger.Value || (!base.ViewModel.HasHit.Value && !base.ViewModel.HitAlways.Value) || (!base.ViewModel.UnitState.IsMouseOverUnit.Value && !base.ViewModel.UnitState.IsAoETarget.Value))
			{
				return base.ViewModel.UnitState.HoverSelfTargetAbility.Value;
			}
			return true;
		}
	}

	private bool CheckVisibility
	{
		get
		{
			if (CheckCanBeVisible && CheckVisibleTrigger && CheckDestructible)
			{
				return !AimBlockVisible;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UnitState.IsVisibleForPlayer.CombineLatest(base.ViewModel.UnitState.ForceHotKeyPressed, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.HasHiddenCondition, base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead, base.ViewModel.HasHit, base.ViewModel.UnitState.HoverSelfTargetAbility, (bool _, bool _, bool _, bool _, bool _, bool _, bool _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		AddDisposable(base.ViewModel.UnitState.IsCurrentUnitTurn.CombineLatest(base.ViewModel.UnitState.Ability, base.ViewModel.UnitState.IsActing, (bool _, AbilityData _, bool _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		AddDisposable(base.ViewModel.UnitState.Name.Subscribe(delegate(string value)
		{
			SetName(value);
		}));
		if (m_MultiSelectable != null)
		{
			AddDisposable(base.ViewModel.UnitState.IsEnemy.Subscribe(delegate(bool value)
			{
				m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.UnitState.IsPlayer.Value ? "Party" : "Ally"));
			}));
		}
		AddDisposable(m_NameText.SetHint(base.ViewModel.UnitState.Name, null, CharacterNameCanvasRenderer.GetColor()));
	}

	private void SetName(string value)
	{
		((RectTransform)m_NameText.transform).sizeDelta = new Vector2(1000f, ((RectTransform)m_NameText.transform).sizeDelta.y);
		m_NameText.text = value;
		m_NameText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		((RectTransform)m_NameText.transform).sizeDelta = new Vector2(m_NameText.renderedWidth, ((RectTransform)m_NameText.transform).sizeDelta.y);
		foreach (RectTransform containers in m_ContainersList)
		{
			containers.sizeDelta = new Vector2(m_NameText.renderedWidth, containers.sizeDelta.y);
		}
	}

	private void UpdateVisibility()
	{
		if (base.ViewModel.UnitState.IsEnemy.Value && base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Value && !base.ViewModel.UnitState.IsTBM.Value && !base.ViewModel.UnitState.HasLoot.Value)
		{
			m_Animator.PlayAnimation(value: false);
		}
		else
		{
			m_Animator.PlayAnimation(CheckVisibility);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
