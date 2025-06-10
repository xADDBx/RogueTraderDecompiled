using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public abstract class AbilityRange : MonoBehaviour, IAbilityTargetSelectionUIHandler, ISubscriber, IAbilityTargetHoverUIHandler, ITurnBasedModeHandler, IFlippedZoneAbilityHandler
{
	protected AbilityData Ability;

	private bool m_IsActive;

	private AbilityData m_SelectedAbility;

	[UsedImplicitly]
	public virtual void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	[UsedImplicitly]
	public virtual void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	[UsedImplicitly]
	private void Update()
	{
		if (m_IsActive && Game.Instance.ClickEventsController != null)
		{
			SetRangeToCasterPosition();
		}
	}

	private void SetRange(bool state)
	{
		if (state == m_IsActive)
		{
			return;
		}
		m_IsActive = state;
		if (state)
		{
			SetFirstSpecs();
			SetRangeToCasterPosition();
		}
		else
		{
			EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
			{
				h.HandleAoECancel();
			});
		}
	}

	protected virtual void SetFirstSpecs()
	{
	}

	protected void SetRangeToCasterPosition()
	{
		SetRangeToWorldPosition(Game.Instance.VirtualPositionController.GetDesiredPosition(Ability.Caster));
	}

	protected virtual void SetRangeToWorldPosition(Vector3 castPosition, bool ignoreCache = false)
	{
	}

	public virtual void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_SelectedAbility = ability;
		SetAbility(ability);
	}

	public virtual void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_SelectedAbility = null;
		SetAbility(null);
	}

	protected virtual bool CanEnable()
	{
		return Ability != null;
	}

	private void SetAbility(AbilityData ability)
	{
		Ability = ability;
		SetRange(CanEnable());
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		SetAbility(hover ? ability : m_SelectedAbility);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			SetAbility(null);
		}
	}

	void IFlippedZoneAbilityHandler.HandleFlippedZoneAbility()
	{
		if (!(this is AbilitySingleTargetRange))
		{
			SetRangeToWorldPosition(Game.Instance.VirtualPositionController.GetDesiredPosition(Ability.Caster), ignoreCache: true);
		}
	}
}
