using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d1933e412cc64482ae7990aaacdf44a8")]
public class WarhammerContextActionRestoreActionPoints : ContextAction
{
	[HideIf("ActionPointsToMax")]
	public ContextValue ActionPoints;

	[HideIf("MovePointsToMax")]
	public ContextValue MovePoints;

	[HideIf("IgnoreActionPointsMaximum")]
	public bool ActionPointsToMax;

	[HideIf("IgnoreMovePointsMaximum")]
	public bool MovePointsToMax;

	[HideIf("MovePointsToMax")]
	public bool IgnoreMovePointsMaximum;

	[HideIf("ActionPointsToMax")]
	public bool IgnoreActionPointsMaximum;

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = base.Target?.Entity;
		if (mechanicEntity == null)
		{
			return;
		}
		PartUnitCombatState combatStateOptional = mechanicEntity.GetCombatStateOptional();
		if (combatStateOptional != null)
		{
			int result = Rulebook.Trigger(new RuleCalculateActionPoints(mechanicEntity, isTurnBased: true)).Result;
			int num = ActionPoints.Calculate(base.Context);
			if (IgnoreActionPointsMaximum && num != 0)
			{
				combatStateOptional.GainYellowPoint(num, base.Context);
			}
			else if (ActionPointsToMax || num != 0)
			{
				int value = (ActionPointsToMax ? result : Math.Clamp(combatStateOptional.ActionPointsYellow + num, 0, result));
				combatStateOptional.SetYellowPoint(value, base.Context);
			}
			float actionPointsBlueMax = combatStateOptional.ActionPointsBlueMax;
			int num2 = MovePoints.Calculate(base.Context);
			if (IgnoreMovePointsMaximum && num2 != 0)
			{
				combatStateOptional.GainBluePoint(num2, base.Context);
			}
			else if (MovePointsToMax || num2 != 0)
			{
				float value2 = (MovePointsToMax ? actionPointsBlueMax : Math.Clamp(combatStateOptional.ActionPointsBlue + (float)num2, 0f, actionPointsBlueMax));
				combatStateOptional.SetBluePoint(value2, base.Context);
			}
			EventBus.RaiseEvent((IMechanicEntity)mechanicEntity, (Action<IUnitActionPointsHandler>)delegate(IUnitActionPointsHandler h)
			{
				h.HandleRestoreActionPoints();
			}, isCheckRuntime: true);
		}
	}

	public override string GetCaption()
	{
		return "Restore Action Points and Move Points";
	}
}
