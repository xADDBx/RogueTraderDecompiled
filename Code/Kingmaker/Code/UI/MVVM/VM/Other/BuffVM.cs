using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Damage;
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

	public readonly ReactiveProperty<bool> IsDamage = new ReactiveProperty<bool>();

	public List<BuffVM> AdditionalSources = new List<BuffVM>();

	private readonly Subject<Unit> m_CalculateDamageSubject = new Subject<Unit>();

	private readonly UnitPartNonStackBonuses m_NonStackBonus;

	public Buff Buff { get; }

	public BuffUIGroup Group { get; private set; }

	public int SortOrder { get; private set; }

	public IEntity GetSubscribingEntity()
	{
		return Buff?.Owner;
	}

	public BuffVM(Buff buff)
	{
		Buff = buff;
		m_NonStackBonus = Buff?.Owner?.GetOptional<UnitPartNonStackBonuses>();
		Icon.Value = ObjectExtensions.Or(buff.Icon, UIConfig.Instance.UIIcons.DefaultAbilityIcon);
		ShowNonStackNotification.Value = ShowNonStackWarning();
		DOTLogicVisual dOTLogicVisual = buff.Blueprint?.GetComponent<DOTLogicVisual>();
		IsDamage.Value = dOTLogicVisual != null;
		SetGroup();
		SetSortOrder();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_CalculateDamageSubject.ThrottleFirstFrame(1).Subscribe(delegate
		{
			CalculateDamage();
		}));
		UpdateRank();
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

	private void SetGroup()
	{
		if (Buff != null)
		{
			if (Buff.Blueprint.IsDOTVisual)
			{
				Group = BuffUIGroup.DOT;
				return;
			}
			bool flag = Buff.Owner?.IsEnemy(Buff.Context.MaybeCaster) ?? false;
			Group = (flag ? BuffUIGroup.Enemy : BuffUIGroup.Ally);
		}
	}

	private void SetSortOrder()
	{
		if (Buff != null)
		{
			if (Buff.Blueprint.HasBuffOverrideUIOrder)
			{
				SortOrder = 0;
			}
			else if (Buff.Blueprint.IsDOTVisual)
			{
				SortOrder = 1;
			}
			else
			{
				SortOrder = 2;
			}
		}
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
		if (Buff.Blueprint?.GetComponent<DOTLogicVisual>() == null)
		{
			Rank.Value = Buff.Rank;
		}
		else
		{
			m_CalculateDamageSubject.OnNext(default(Unit));
		}
	}

	private void CalculateDamage()
	{
		DamageData damageData = DOTLogicUIExtensions.CalculateDOTDamage(Buff);
		if (damageData != null)
		{
			Rank.Value = damageData.AverageValue;
		}
	}

	private bool ShouldHandle(MechanicEntity owner)
	{
		return Buff.Owner == owner;
	}
}
