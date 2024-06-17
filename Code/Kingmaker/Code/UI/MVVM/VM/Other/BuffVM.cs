using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Other;

public class BuffVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitBuffHandler<EntitySubscriber>, IEventTag<IUnitBuffHandler, EntitySubscriber>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, IEntitySubscriber
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> ShowNonStackNotification = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<int> Rank = new ReactiveProperty<int>();

	private readonly UnitPartNonStackBonuses m_NonStackBonus;

	public Buff Buff { get; }

	public IEntity GetSubscribingEntity()
	{
		return Buff?.Owner;
	}

	public BuffVM(Buff buff)
	{
		Buff = buff;
		m_NonStackBonus = Buff?.Owner?.GetOptional<UnitPartNonStackBonuses>();
		Icon.Value = buff.Icon.Or(UIConfig.Instance.UIIcons.DefaultAbilityIcon);
		ShowNonStackNotification.Value = ShowNonStackWarning();
		Rank.Value = buff.Rank;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private bool ShowNonStackWarning()
	{
		if (m_NonStackBonus != null)
		{
			return m_NonStackBonus.ShouldShowWarning(Buff);
		}
		return false;
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (ShouldHandle(slot.Owner))
		{
			ShowNonStackNotification.Value = ShowNonStackWarning();
		}
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (ShouldHandle(buff.Owner))
		{
			ShowNonStackNotification.Value = ShowNonStackWarning();
			UpdateRank();
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (ShouldHandle(buff.Owner))
		{
			ShowNonStackNotification.Value = ShowNonStackWarning();
			UpdateRank();
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
		if (ShouldHandle(buff.Owner))
		{
			UpdateRank();
		}
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
		if (ShouldHandle(buff.Owner))
		{
			UpdateRank();
		}
	}

	private void UpdateRank()
	{
		Rank.Value = Buff.Rank;
	}

	private bool ShouldHandle(MechanicEntity owner)
	{
		return Buff.Owner == owner;
	}
}
