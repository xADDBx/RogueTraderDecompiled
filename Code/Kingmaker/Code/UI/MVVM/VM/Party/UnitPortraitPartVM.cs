using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class UnitPortraitPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEntitySubscriber, IUnitPortraitChangedHandler<EntitySubscriber>, IUnitPortraitChangedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitPortraitChangedHandler, EntitySubscriber>
{
	public readonly ReactiveProperty<Sprite> Portrait = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveProperty<bool> IsDead = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsCrippled = new ReactiveProperty<bool>();

	private EntityRef<BaseUnitEntity> m_Unit;

	public IEntity GetSubscribingEntity()
	{
		return m_Unit.Entity;
	}

	public UnitPortraitPartVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateFields();
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateFields()
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess && m_Unit.Entity != null)
		{
			bool isFinallyDead = m_Unit.Entity.LifeState.IsFinallyDead;
			bool value = (bool)m_Unit.Entity.GetOptional<UnitPartDeathDoor>() && !isFinallyDead;
			IsDead.Value = isFinallyDead;
			IsCrippled.Value = value;
		}
	}

	public void SetUnitData(BaseUnitEntity unit)
	{
		m_Unit = unit;
		Portrait.Value = m_Unit.Entity?.Portrait.SmallPortrait;
		UpdateFields();
	}

	public void HandlePortraitChanged()
	{
		Portrait.Value = m_Unit.Entity?.Portrait.SmallPortrait;
	}
}
