using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;

public class StatCheckLootMainPageVM : StatCheckLootPageVM
{
	public readonly AutoDisposingDictionary<StatType, StatCheckLootUnitCardVM> UnitSlotVMByStatType = new AutoDisposingDictionary<StatType, StatCheckLootUnitCardVM>();

	public readonly ReactiveCommand UpdateUnitSlots = new ReactiveCommand();

	public readonly ReactiveCommand ClearUnitSlots = new ReactiveCommand();

	private readonly Action<BaseUnitEntity, StatType> m_SwitchUnitAction;

	private readonly Action<BaseUnitEntity, StatType> m_CheckStatAction;

	private readonly Action m_CloseAction;

	private List<BaseUnitEntity> m_Party => Game.Instance.Player.Party.Where((BaseUnitEntity u) => !u.LifeState.IsDead).ToList();

	public StatCheckLootMainPageVM(Action<BaseUnitEntity, StatType> switchUnitAction, Action<BaseUnitEntity, StatType> checkStatAction, Action closeAction)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_SwitchUnitAction = switchUnitAction;
		m_CheckStatAction = checkStatAction;
		m_CloseAction = closeAction;
	}

	protected override void DisposeImplementation()
	{
		ClearUnitSlotVMs();
	}

	public void SwitchUnit()
	{
		foreach (KeyValuePair<StatType, StatCheckLootUnitCardVM> item in UnitSlotVMByStatType)
		{
			StatCheckLootUnitCardVM value = item.Value;
			if (value.IsSelected)
			{
				value.SwitchUnit();
				break;
			}
		}
	}

	public void CheckStat()
	{
		foreach (KeyValuePair<StatType, StatCheckLootUnitCardVM> item in UnitSlotVMByStatType)
		{
			StatCheckLootUnitCardVM value = item.Value;
			if (value.IsSelected)
			{
				value.CheckStat();
				break;
			}
		}
	}

	public void CloseDialog()
	{
		m_CloseAction?.Invoke();
	}

	private void AddUnitSlot(BaseUnitEntity unitEntity, StatType stat)
	{
		StatCheckLootUnitCardVM value = new StatCheckLootUnitCardVM(unitEntity, stat, m_CheckStatAction, m_SwitchUnitAction);
		UnitSlotVMByStatType.Add(stat, value);
	}

	private void ClearUnitSlotVMs()
	{
		ClearUnitSlots.Execute();
		UnitSlotVMByStatType.Clear();
	}

	[CanBeNull]
	private BaseUnitEntity SelectBestUnit(List<BaseUnitEntity> units, StatType stat)
	{
		int bestStatValue = int.MinValue;
		BaseUnitEntity result = null;
		foreach (BaseUnitEntity item in units.Where((BaseUnitEntity u) => (int)u.Stats.GetStat(stat) > bestStatValue))
		{
			bestStatValue = item.Stats.GetStat(stat);
			result = item;
		}
		return result;
	}

	public void HandlePageOpened(ICheckForLoot checkForLoot)
	{
		UnitSlotVMByStatType.Clear();
		List<StatDC> stats = checkForLoot.GetStats();
		for (int i = 0; i < stats.Count; i++)
		{
			StatType stat = stats[i].Stat;
			BaseUnitEntity unitEntity = SelectBestUnit(m_Party, stat);
			AddUnitSlot(unitEntity, stat);
		}
		UpdateUnitSlots.Execute();
	}

	public void HandleConfirmSelectedUnit(BaseUnitEntity unitEntity, StatType statType)
	{
		if (UnitSlotVMByStatType.TryGetValue(statType, out var value))
		{
			value.Dispose();
			value = new StatCheckLootUnitCardVM(unitEntity, statType, m_CheckStatAction, m_SwitchUnitAction);
			UnitSlotVMByStatType[statType] = value;
		}
		UpdateUnitSlots.Execute();
	}
}
