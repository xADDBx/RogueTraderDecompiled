using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class SpacePointMarkersVM : PointMarkersVM, IAreaHandler, ISubscriber, IAreaActivationHandler, IReloadMechanicsHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	protected override IEnumerable<BaseUnitEntity> UnitsSelector => Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => !unit.IsInCombat && unit.Faction.IsPlayer);

	protected override IEnumerable<Entity> AnotherEntitiesSelector => Game.Instance.State.SectorMapObjects.Where((SectorMapObjectEntity o) => o.View.CheckQuests());

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
}
