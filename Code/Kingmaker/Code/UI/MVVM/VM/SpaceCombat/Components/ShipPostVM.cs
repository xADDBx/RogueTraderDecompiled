using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;

public class ShipPostVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IStarshipPostHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IUnitCommandStartHandler, IWarhammerAttackHandler, IUnitCommandActHandler, IUnitCommandEndHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IDeliverAbilityEffectHandler
{
	public readonly int Index;

	public readonly Sprite Portrait;

	public readonly PostType PostType;

	public readonly AbilitiesGroupVM AbilitiesGroup;

	public readonly ReactiveProperty<bool> IsPostBlocked = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> BlockDuration = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> FXActivated = new ReactiveProperty<bool>();

	private readonly Post m_Post;

	private bool m_SlotsUpdateQueued;

	public ShipPostVM(int index, Post post)
	{
		Index = index;
		m_Post = post;
		if (post != null)
		{
			Portrait = post.CurrentUnit?.Portrait?.SmallPortrait;
			PostType = post.PostType;
		}
		IsPostBlocked.Value = false;
		BlockDuration.Value = "0";
		FXActivated.Value = false;
		AddDisposable(AbilitiesGroup = new AbilitiesGroupVM(string.Empty));
		UpdateAbilities();
		AddDisposable(Game.Instance.SelectionCharacter.SingleSelectedUnit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandlePostBlocked(Post post)
	{
		if (post == m_Post)
		{
			IsPostBlocked.Value = post.IsBlocked;
			BlockDuration.Value = post.BlockingBuff?.DurationInRounds.Rounds().ToString();
			FXActivated.Value = true;
		}
	}

	public void HandleBuffDidAdded(Post post, Buff buff)
	{
	}

	public void HandleBuffDidRemoved(Post post, Buff buff)
	{
		if (m_Post == post && post.BlockingBuff == null)
		{
			IsPostBlocked.Value = false;
			BlockDuration.Value = "0";
		}
	}

	private void UpdateAbilities()
	{
		List<Ability> list = new List<Ability>();
		foreach (Ability item in m_Post.UnlockedAbilities())
		{
			if (!item.Hidden && !item.Blueprint.IsCantrip)
			{
				list.Add(item);
			}
		}
		AbilitiesGroup.SetAbilities(list, m_Post.Ship);
	}

	private void UpdateSlots(bool onTurnStart = false)
	{
		if (m_SlotsUpdateQueued)
		{
			return;
		}
		m_SlotsUpdateQueued = true;
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
		{
			m_SlotsUpdateQueued = false;
			((Action<IList<ActionBarSlotVM>>)delegate(IList<ActionBarSlotVM> slots)
			{
				foreach (ActionBarSlotVM slot in slots)
				{
					slot.UpdateResources();
					if (onTurnStart)
					{
						slot.CloseConvertsOnTurnStart();
					}
				}
			})(AbilitiesGroup.Slots);
		});
	}

	private void OnUnitChanged()
	{
		UpdateSlots();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitStartTurnInternal();
	}

	private void HandleUnitStartTurnInternal()
	{
		BlockDuration.Value = m_Post.BlockingBuff?.ExpirationInRounds.Rounds().ToString();
		UpdateSlots(onTurnStart: true);
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateSlots();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateSlots();
	}

	public void OnDeliverAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		UpdateSlots();
	}
}
