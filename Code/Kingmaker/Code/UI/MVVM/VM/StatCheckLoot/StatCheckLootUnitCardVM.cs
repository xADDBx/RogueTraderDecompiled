using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;

public class StatCheckLootUnitCardVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> UnitName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<Sprite> UnitPortrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> StatName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<int> StatValue = new ReactiveProperty<int>(0);

	private readonly BaseUnitEntity m_UnitEntity;

	private readonly StatType m_StatType;

	private readonly Action<BaseUnitEntity, StatType> m_CheckStatAction;

	private readonly Action<BaseUnitEntity, StatType> m_SwitchUnitAction;

	private bool m_IsSelected;

	public bool IsSelected => m_IsSelected;

	public StatCheckLootUnitCardVM(BaseUnitEntity unitEntity, StatType stat, Action<BaseUnitEntity, StatType> checkStatAction, Action<BaseUnitEntity, StatType> switchUnitAction)
	{
		m_UnitEntity = unitEntity;
		m_StatType = stat;
		m_CheckStatAction = checkStatAction;
		m_SwitchUnitAction = switchUnitAction;
		UnitName.Value = unitEntity.Name;
		UnitPortrait.Value = unitEntity.Portrait.SmallPortrait;
		StatName.Value = UIUtilityTexts.GetStatShortName(stat);
		StatValue.Value = unitEntity.GetStatBaseValue(stat).Value;
	}

	public void SetSelected(bool value)
	{
		m_IsSelected = value;
	}

	public void CheckStat()
	{
		m_CheckStatAction?.Invoke(m_UnitEntity, m_StatType);
	}

	public void SwitchUnit()
	{
		m_SwitchUnitAction?.Invoke(m_UnitEntity, m_StatType);
	}

	protected override void DisposeImplementation()
	{
	}
}
