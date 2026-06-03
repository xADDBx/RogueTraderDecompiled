using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker;

[TypeId("4158c1781faf47c47ba787409c368a7d")]
public class ActionsOnEntitiesInAreaEffectOnSpawn : AreaEffectSpawnLogic
{
	[SerializeField]
	private ActionList ActionsOnUnits;

	[SerializeField]
	private ActionList ActionsOnDestructibleEntities;

	protected override void OnAreaEffectSpawn(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		base.OnAreaEffectSpawn(context, areaEffect);
		ActionList actionsOnDestructibleEntities = ActionsOnDestructibleEntities;
		if (actionsOnDestructibleEntities != null && actionsOnDestructibleEntities.HasActions)
		{
			DestructibleEntity[] allDestructibleEntityInside = areaEffect.GetAllDestructibleEntityInside();
			foreach (DestructibleEntity entity in allDestructibleEntityInside)
			{
				using (context.GetDataScope(entity.ToITargetWrapper()))
				{
					ActionsOnDestructibleEntities.Run();
				}
			}
		}
		actionsOnDestructibleEntities = ActionsOnUnits;
		if (actionsOnDestructibleEntities == null || !actionsOnDestructibleEntities.HasActions)
		{
			return;
		}
		foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
		{
			using (context.GetDataScope(item.ToITargetWrapper()))
			{
				ActionsOnUnits.Run();
			}
		}
	}
}
