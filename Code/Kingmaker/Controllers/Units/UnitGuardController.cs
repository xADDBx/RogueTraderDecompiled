using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Spawners;

namespace Kingmaker.Controllers.Units;

public class UnitGuardController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity entity)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		UnitPartGuard optional = baseUnitEntity.GetOptional<UnitPartGuard>();
		if (!optional)
		{
			return;
		}
		float num = optional.Range * optional.Range;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (!allBaseUnit.Faction.IsPlayer)
			{
				continue;
			}
			float num2 = baseUnitEntity.SqrDistanceTo(allBaseUnit);
			bool num3 = num >= num2;
			bool flag = (optional.UseLosInsteadOfVisibility ? GameHelper.CheckLOS(baseUnitEntity, allBaseUnit) : baseUnitEntity.CombatGroup.Memory.ContainsVisible(allBaseUnit));
			bool flag2 = num3 && flag;
			bool flag3 = optional.DetectedUnits.Contains(allBaseUnit);
			if (flag2 && !flag3)
			{
				optional.DetectedUnits.Add(allBaseUnit);
				SpawnerGuardSettings.Part part = optional.Source.Entity?.GetOptional<SpawnerGuardSettings.Part>();
				if (part == null)
				{
					break;
				}
				using (ContextData<SpawnedUnitData>.Request().Setup(baseUnitEntity, baseUnitEntity.HoldingState))
				{
					using (ContextData<InteractingUnitData>.Request().Setup(allBaseUnit))
					{
						part.Source.OnDetect.Get().Actions.Run();
						break;
					}
				}
			}
			if (!flag2 && flag3)
			{
				optional.DetectedUnits.Remove(allBaseUnit);
			}
		}
	}
}
