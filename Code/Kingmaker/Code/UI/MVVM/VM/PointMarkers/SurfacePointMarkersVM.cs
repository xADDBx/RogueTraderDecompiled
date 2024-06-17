using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Code.UI.MVVM.VM.PointMarkers;

public class SurfacePointMarkersVM : PointMarkersVM, IAreaHandler, ISubscriber, IAreaActivationHandler, IReloadMechanicsHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ITurnBasedModeHandler, ITurnBasedModeResumeHandler
{
	protected override IEnumerable<BaseUnitEntity> UnitsSelector => Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => (!unit.IsStarship() && unit.Faction.IsPlayer && unit.IsDirectlyControllable) || (unit.IsInCombat && unit.Faction.IsPlayerEnemy));

	public SurfacePointMarkersVM()
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			HandleTurnBasedModeResumed();
		}
	}

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		UpdateUnits();
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && Units.Contains(baseUnitEntity))
		{
			UpdateUnits();
		}
	}

	void IUnitHandler.HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && Units.Contains(baseUnitEntity))
		{
			UpdateUnits();
		}
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		Clear();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		UpdateUnits();
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		UpdateUnits();
	}

	void IReloadMechanicsHandler.OnBeforeMechanicsReload()
	{
		Clear();
	}

	void IReloadMechanicsHandler.OnMechanicsReloaded()
	{
		UpdateUnits();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateUnits();
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdateUnits();
	}
}
