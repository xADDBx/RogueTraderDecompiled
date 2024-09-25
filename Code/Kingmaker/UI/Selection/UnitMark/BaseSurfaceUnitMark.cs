using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.UI.Selection.UnitMark;

public abstract class BaseSurfaceUnitMark : BaseUnitMark, IUnitDirectHoverUIHandler, ISubscriber
{
	protected List<UnitMarkDecal> AllUnitMarkDecal = new List<UnitMarkDecal>();

	private Sequence m_AbilityTargetAnimator;

	private bool m_CanTarget;

	private bool m_AbilityTargetSelectionStarted;

	protected override void OnEnabled()
	{
		AllUnitMarkDecal = GetAllDecals();
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		m_AbilityTargetAnimator?.Kill();
		m_AbilityTargetAnimator = null;
		m_CanTarget = false;
	}

	protected abstract List<UnitMarkDecal> GetAllDecals();

	protected void SetUnitSize(bool isBig)
	{
		AllUnitMarkDecal.ForEach(delegate(UnitMarkDecal um)
		{
			um.SetBigSize(isBig);
		});
	}

	protected abstract UnitMarkDecal GetAbilityTargetDecal();

	public sealed override void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		HandleAbilityTargetSelectionStartImpl(ability);
		CheckAndStartAbilitySelectionAnimation(ability);
		m_AbilityTargetSelectionStarted = true;
	}

	protected virtual void HandleAbilityTargetSelectionStartImpl(AbilityData ability)
	{
	}

	public sealed override void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		HandleAbilityTargetSelectionEndImpl(ability);
		StopAbilitySelectionAnimation(GetAbilityTargetDecal());
		m_AbilityTargetSelectionStarted = false;
	}

	protected virtual void HandleAbilityTargetSelectionEndImpl(AbilityData ability)
	{
	}

	private void CheckAndStartAbilitySelectionAnimation(AbilityData ability)
	{
		if (ability.Weapon == null && !ability.IsAOE)
		{
			TargetWrapper targetForDesiredPosition = Game.Instance.SelectedAbilityHandler.GetTargetForDesiredPosition(base.Unit.View.gameObject, Game.Instance.ClickEventsController.WorldPosition);
			m_CanTarget = ability.CanTargetFromDesiredPosition(targetForDesiredPosition, out var _);
			if (m_CanTarget)
			{
				StartAbilitySelectionAnimation(GetAbilityTargetDecal());
			}
		}
	}

	private void StartAbilitySelectionAnimation(UnitMarkDecal decal)
	{
		decal.SetActive(state: true);
		m_AbilityTargetAnimator?.Kill();
		m_AbilityTargetAnimator = DOTween.Sequence().SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true);
		m_AbilityTargetAnimator.Append(decal.DecalMeshRenderer.material.DOFade(0.2f, 0.4f));
		m_AbilityTargetAnimator.Join(decal.GameObject.transform.DOScale(1.2f, 0.4f));
		m_AbilityTargetAnimator.Play();
	}

	private void StopAbilitySelectionAnimation(UnitMarkDecal decal)
	{
		decal.SetActive(state: false);
		m_AbilityTargetAnimator.Rewind();
		m_AbilityTargetAnimator?.Kill();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		if (m_AbilityTargetSelectionStarted)
		{
			GetAbilityTargetDecal().SetActive(!isHover && m_CanTarget);
		}
	}
}
