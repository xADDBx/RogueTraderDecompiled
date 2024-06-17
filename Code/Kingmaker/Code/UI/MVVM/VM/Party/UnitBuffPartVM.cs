using System;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class UnitBuffPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitBuffHandler<EntitySubscriber>, IEventTag<IUnitBuffHandler, EntitySubscriber>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber
{
	private MechanicEntity m_Unit;

	private IDisposable m_Subscription;

	public readonly ReactiveCollection<BuffVM> Buffs = new ReactiveCollection<BuffVM>();

	public IEntity GetSubscribingEntity()
	{
		return m_Unit;
	}

	public UnitBuffPartVM(MechanicEntity unit)
	{
		if (unit != null)
		{
			SetUnitData(unit);
		}
		else
		{
			ClearBuffs();
		}
	}

	public void SetUnitData(MechanicEntity unit)
	{
		m_Subscription?.Dispose();
		m_Unit = unit;
		if (unit != null)
		{
			m_Subscription = EventBus.Subscribe(this);
		}
		UpdateData();
	}

	private void ClearBuffs()
	{
		Buffs.ForEach(delegate(BuffVM vm)
		{
			vm.Dispose();
		});
		Buffs.Clear();
	}

	protected override void DisposeImplementation()
	{
		m_Subscription?.Dispose();
		m_Subscription = null;
		ClearBuffs();
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (m_Unit != null && m_Unit == buff.Owner && !buff.Blueprint.IsHiddenInUI)
		{
			Buffs.Add(new BuffVM(buff));
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (m_Unit != null && buff.Owner != null && m_Unit == buff.Owner)
		{
			BuffVM buffVM = Buffs.FirstOrDefault((BuffVM b) => b.Buff == buff);
			if (buffVM != null)
			{
				buffVM.Dispose();
				Buffs.Remove(buffVM);
			}
		}
	}

	public void UpdateData()
	{
		ClearBuffs();
		if (m_Unit == null)
		{
			return;
		}
		foreach (Buff buff in m_Unit.Buffs)
		{
			if (!buff.Blueprint.IsHiddenInUI && !(buff.Icon == null) && !Buffs.Any((BuffVM b) => b.Buff == buff))
			{
				Buffs.Add(new BuffVM(buff));
			}
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}
}
