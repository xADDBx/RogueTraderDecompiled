using Kingmaker.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.Controllers;

public class PartyEncumbranceController : IControllerEnable, IController, IControllerTick
{
	public void OnEnable()
	{
		UpdatePartyEncumbrance();
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			UpdateUnitEncumbrance(partyAndPet);
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		using (ProfileScope.New("UpdatePartyEncumbrance"))
		{
			UpdatePartyEncumbrance();
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			using (ProfileScope.New("UpdateUnitEncumbrance"))
			{
				UpdateUnitEncumbrance(partyAndPet);
			}
		}
	}

	private static void UpdatePartyEncumbrance()
	{
		EncumbranceHelper.CarryingCapacity partyCarryingCapacity = EncumbranceHelper.GetPartyCarryingCapacity();
		Encumbrance encumbrance = ((!Game.Instance.LoadedAreaState.Settings.IgnorePartyEncumbrance) ? partyCarryingCapacity.GetEncumbrance() : Encumbrance.Light);
		if (BuildModeUtility.IsDevelopment && CheatsCommon.IgnoreEncumbrance)
		{
			encumbrance = Encumbrance.Light;
		}
		Encumbrance prevEncumbrance = Game.Instance.Player.Encumbrance;
		if (prevEncumbrance != encumbrance)
		{
			EventBus.RaiseEvent(delegate(IPartyEncumbranceHandler h)
			{
				h.ChangePartyEncumbrance(prevEncumbrance);
			});
		}
	}

	private static void UpdateUnitEncumbrance(BaseUnitEntity unit)
	{
		bool flag = false;
		EncumbranceHelper.GetCarryingCapacity(unit);
		Encumbrance encumbrance = Encumbrance.Light;
		Encumbrance prevEncumbrance = unit.EncumbranceData?.Value ?? Encumbrance.Light;
		if (prevEncumbrance != encumbrance)
		{
			if (encumbrance == Encumbrance.Light)
			{
				unit.Remove<UnitPartEncumbrance>();
			}
			else
			{
				unit.GetOrCreate<UnitPartEncumbrance>();
			}
			unit.EncumbranceData?.Set(encumbrance);
			EventBus.RaiseEvent(delegate(IUnitEncumbranceHandler h)
			{
				h.ChangeUnitEncumbrance(prevEncumbrance);
			});
			flag = true;
		}
		if (flag)
		{
			EventBus.RaiseEvent(delegate(IUIUnitStatsRefresh h)
			{
				h.Refresh();
			});
		}
	}
}
