using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

public class ShipCrewPanelVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public readonly ShipCrewDollVM ShipCrewDollVM;

	public readonly ReactiveProperty<float> ShipMoraleRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<string> ShipMoraleValue = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<float> ShipCrewRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<string> ShipCrewValue = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> ShipMilitaryRating = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveCollection<BuffVM> ShipBuffs = new ReactiveCollection<BuffVM>();

	private StarshipEntity m_Ship => Game.Instance.Player.PlayerShip;

	public ShipCrewPanelVM()
	{
		AddDisposable(ShipCrewDollVM = new ShipCrewDollVM());
		foreach (Buff unitBuff in GetUnitBuffs(m_Ship))
		{
			ShipBuffs.Add(new BuffVM(unitBuff));
		}
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			UpdateHandler();
		}));
	}

	protected override void DisposeImplementation()
	{
		ShipBuffs.ForEach(delegate(BuffVM buffVm)
		{
			buffVm.Dispose();
		});
		ShipBuffs.Clear();
	}

	private void UpdateHandler()
	{
		if (m_Ship.Morale.MaxMorale != 0)
		{
			float num = (float)m_Ship.Morale.MoraleLeft / (float)m_Ship.Morale.MaxMorale;
			ShipMoraleRatio.Value = num;
			ShipMoraleValue.Value = $"{num * 100f}%";
		}
		ShipCrewRatio.Value = m_Ship.Crew.Ratio;
		ShipCrewValue.Value = m_Ship.Crew.Count + "k";
		PartStarshipHull optional = m_Ship.GetOptional<PartStarshipHull>();
		ShipMilitaryRating.Value = optional?.CurrentMilitaryRating.ToString() ?? string.Empty;
	}

	private List<Buff> GetUnitBuffs(BaseUnitEntity unit)
	{
		return unit.Buffs.Enumerable.Where((Buff b) => !b.Blueprint.IsHiddenInUI).ToList();
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (m_Ship != null && m_Ship == buff.Owner && !buff.Blueprint.IsHiddenInUI)
		{
			ShipBuffs.Add(new BuffVM(buff));
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (m_Ship != null && buff.Owner != null && m_Ship == buff.Owner)
		{
			BuffVM buffVM = Enumerable.FirstOrDefault(ShipBuffs, (BuffVM b) => b.Buff == buff);
			if (buffVM != null)
			{
				buffVM.Dispose();
				ShipBuffs.Remove(buffVM);
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
