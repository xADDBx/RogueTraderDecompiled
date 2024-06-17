using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Spawners;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Controllers.SpaceCombat;

public class AutoJoinSpaceCombatController : IControllerEnable, IController, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	public void OnEnable()
	{
		foreach (EntityReference activatedSpawner in Game.Instance.Player.ActivatedSpawners)
		{
			UnitSpawnerBase unitSpawner = GameHelper.GetUnitSpawner(activatedSpawner);
			if (!(unitSpawner == null))
			{
				unitSpawner.Spawn();
			}
		}
	}

	public void HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.IsStarship() && !baseUnitEntity.IsInCombat)
		{
			baseUnitEntity.CombatState.JoinCombat();
		}
	}
}
