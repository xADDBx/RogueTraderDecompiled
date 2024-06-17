using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;

public class StatCheckLootSmallUnitCardVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> UnitName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<Sprite> UnitPortrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<int> StatValue = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>(initialValue: false);

	private readonly BaseUnitEntity m_UnitEntity;

	private readonly Action<BaseUnitEntity> m_UnitSelectedAction;

	public StatCheckLootSmallUnitCardVM(BaseUnitEntity unitEntity, StatType stat, Action<BaseUnitEntity> unitSelectedAction, bool isSelected)
	{
		m_UnitEntity = unitEntity;
		m_UnitSelectedAction = unitSelectedAction;
		UnitName.Value = unitEntity.Name;
		UnitPortrait.Value = unitEntity.Portrait.SmallPortrait;
		StatValue.Value = unitEntity.GetStatBaseValue(stat).Value;
		IsSelected.Value = isSelected;
	}

	public void SelectUnit()
	{
		m_UnitSelectedAction?.Invoke(m_UnitEntity);
	}

	protected override void DisposeImplementation()
	{
	}
}
