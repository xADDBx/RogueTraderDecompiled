using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[TypeId("24be9d7901731604fb3e9dcc6c21fbb6")]
public class AbilityAreaEffectRunAction : AbilityAreaEffectLogic
{
	public ActionList UnitEnter;

	public ActionList UnitExit;

	public ActionList UnitMove;

	public ActionList Round;

	public ActionList EffectEnd;

	public ActionList EffectEndForEachUnit;

	protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if (!UnitEnter.HasActions)
		{
			return;
		}
		using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
		{
			using (context.GetDataScope(unit.ToITargetWrapper()))
			{
				UnitEnter.Run();
			}
		}
	}

	protected override void OnUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if (!UnitExit.HasActions)
		{
			return;
		}
		using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
		{
			using (context.GetDataScope(unit.ToITargetWrapper()))
			{
				UnitExit.Run();
			}
		}
	}

	protected override void OnUnitMove(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		if (!UnitMove.HasActions)
		{
			return;
		}
		using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
		{
			using (context.GetDataScope(unit.ToITargetWrapper()))
			{
				UnitMove.Run();
			}
		}
	}

	protected override void OnRound(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (!Round.HasActions)
		{
			return;
		}
		using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
		{
			foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
			{
				using (context.GetDataScope(item.ToITargetWrapper()))
				{
					Round.Run();
				}
			}
		}
	}

	protected override void OnEndForEachUnit(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (!EffectEndForEachUnit.HasActions)
		{
			return;
		}
		using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
		{
			foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
			{
				using (context.GetDataScope(item.ToITargetWrapper()))
				{
					EffectEndForEachUnit.Run();
				}
			}
		}
	}

	protected override void OnEnd(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		if (!EffectEnd.HasActions)
		{
			return;
		}
		using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
		{
			using (ContextData<MechanicsContext.Data>.Request().Setup(context, areaEffect.Position))
			{
				EffectEnd.Run();
			}
		}
	}
}
