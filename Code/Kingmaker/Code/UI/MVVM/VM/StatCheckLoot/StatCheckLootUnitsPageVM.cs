using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;

public class StatCheckLootUnitsPageVM : StatCheckLootPageVM
{
	public readonly AutoDisposingReactiveCollection<StatCheckLootSmallUnitCardVM> SmallUnitSlotsVMs = new AutoDisposingReactiveCollection<StatCheckLootSmallUnitCardVM>();

	public readonly ReactiveCommand UpdateSmallUnitSlots = new ReactiveCommand();

	public readonly ReactiveCommand ClearSmallUnitSlots = new ReactiveCommand();

	public readonly ReactiveProperty<StatCheckLootUnitCardVM> SelectedUnitCardVM = new ReactiveProperty<StatCheckLootUnitCardVM>();

	private BaseUnitEntity m_OldUnit;

	private BaseUnitEntity m_CurrentSelectedUnit;

	private StatType m_CurrentSelectedStatType;

	private readonly Action<BaseUnitEntity, StatType> m_CloseAction;

	private List<BaseUnitEntity> m_Party => Game.Instance.Player.Party.Where((BaseUnitEntity u) => !u.LifeState.IsDead).ToList();

	public StatCheckLootUnitsPageVM(Action<BaseUnitEntity, StatType> closeAction)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_CloseAction = closeAction;
	}

	protected override void DisposeImplementation()
	{
		ClearSmallUnitSlotVMs();
	}

	public void ConfirmUnit()
	{
		ClosePage(confirmUnit: true);
	}

	public void BackWithoutConfirmUnit()
	{
		ClosePage(confirmUnit: false);
	}

	public void HandlePageOpened(BaseUnitEntity baseUnitEntity, StatType statType)
	{
		m_OldUnit = baseUnitEntity;
		m_CurrentSelectedUnit = baseUnitEntity;
		m_CurrentSelectedStatType = statType;
		SmallUnitSlotsVMs.Clear();
		foreach (BaseUnitEntity item in m_Party)
		{
			AddSmallUnitSlot(item, statType);
		}
		UpdateSmallUnitSlots.Execute();
		UpdateSelectedUnit();
	}

	public void HandleUnitSelected(BaseUnitEntity unitEntity)
	{
		m_CurrentSelectedUnit = unitEntity;
		UpdateSelectedUnit();
	}

	private void AddSmallUnitSlot(BaseUnitEntity unitEntity, StatType stat)
	{
		StatCheckLootSmallUnitCardVM item = new StatCheckLootSmallUnitCardVM(unitEntity, stat, HandleUnitSelected, unitEntity == m_CurrentSelectedUnit);
		SmallUnitSlotsVMs.Add(item);
	}

	private void ClearSmallUnitSlotVMs()
	{
		ClearSmallUnitSlots.Execute();
		SmallUnitSlotsVMs.Clear();
	}

	private void UpdateSelectedUnit()
	{
		SelectedUnitCardVM.Value?.Dispose();
		StatCheckLootUnitCardVM disposable = (SelectedUnitCardVM.Value = new StatCheckLootUnitCardVM(m_CurrentSelectedUnit, m_CurrentSelectedStatType, null, null));
		AddDisposable(disposable);
	}

	private void ClosePage(bool confirmUnit)
	{
		m_CloseAction?.Invoke(confirmUnit ? m_CurrentSelectedUnit : m_OldUnit, m_CurrentSelectedStatType);
		ClearSmallUnitSlotVMs();
	}
}
