using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("b8417df48902432b9b5a5b22e9f30a75")]
public class ProvokeAttackOfOpportunity : ContextAction
{
	public enum Type
	{
		TargetFromAnyone,
		TargetFromCaster,
		CasterFromAnyone,
		CasterFromTarget,
		TargetFromAlliesAdjacentToCaster
	}

	public Type m_Type;

	[ShowIf("m_ShowConditions")]
	public ConditionsChecker ConditionsOnOpportunityAttacker;

	[ShowIf("m_ShowConditions")]
	public ActionList ActionsOnOpportunityAttacker;

	[UsedImplicitly]
	private bool m_ShowConditions => m_Type == Type.TargetFromAlliesAdjacentToCaster;

	public override string GetCaption()
	{
		return $"Provoke attack of opportunity {m_Type}";
	}

	protected override void RunAction()
	{
		BaseUnitEntity casterUnit = base.Caster as BaseUnitEntity;
		BaseUnitEntity targetUnit = base.Target.Entity as BaseUnitEntity;
		AttackOfOpportunityController attackOfOpportunityController = Game.Instance.AttackOfOpportunityController;
		switch (m_Type)
		{
		case Type.TargetFromAnyone:
			if (targetUnit != null)
			{
				attackOfOpportunityController.Provoke(targetUnit, base.Context.AssociatedBlueprint as BlueprintFact);
			}
			break;
		case Type.TargetFromCaster:
			if (targetUnit != null && casterUnit != null)
			{
				attackOfOpportunityController.Provoke(targetUnit, casterUnit, base.Context.AssociatedBlueprint as BlueprintFact);
			}
			break;
		case Type.CasterFromAnyone:
			if (casterUnit != null)
			{
				attackOfOpportunityController.Provoke(casterUnit, base.Context.AssociatedBlueprint as BlueprintFact);
			}
			break;
		case Type.CasterFromTarget:
			if (targetUnit != null && casterUnit != null)
			{
				attackOfOpportunityController.Provoke(casterUnit, targetUnit, base.Context.AssociatedBlueprint as BlueprintFact);
			}
			break;
		case Type.TargetFromAlliesAdjacentToCaster:
			if (targetUnit == null || casterUnit == null)
			{
				break;
			}
			{
				foreach (BaseUnitEntity item in from p in GameHelper.GetTargetsAround(casterUnit.Position, 1)
					where p != casterUnit && p.IsAlly(casterUnit) && targetUnit.GetEngagedByUnits().Contains(p)
					select p)
				{
					using (base.Context.GetDataScope(item.ToITargetWrapper()))
					{
						if (ConditionsOnOpportunityAttacker.Check())
						{
							attackOfOpportunityController.Provoke(targetUnit, item, base.Context.AssociatedBlueprint as BlueprintFact);
							ActionsOnOpportunityAttacker.Run();
						}
					}
				}
				break;
			}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
