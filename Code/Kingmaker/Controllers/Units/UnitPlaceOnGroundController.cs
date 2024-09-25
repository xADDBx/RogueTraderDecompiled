using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.Units;

public class UnitPlaceOnGroundController : IControllerTick, IController, IAreaPartHandler, ISubscriber, IAreaActivationHandler
{
	private bool m_NeedForcedPlace;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!m_NeedForcedPlace)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			allUnit.View.ForcePlaceAboveGround();
		}
		m_NeedForcedPlace = false;
	}

	public void OnAreaPartChanged(BlueprintAreaPart previous)
	{
		m_NeedForcedPlace = true;
	}

	public void OnAreaActivated()
	{
		m_NeedForcedPlace = true;
	}
}
