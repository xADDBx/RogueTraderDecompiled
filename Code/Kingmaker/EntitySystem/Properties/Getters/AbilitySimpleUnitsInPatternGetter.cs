using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f48cb5fd68404437b868e072a7ba6d37")]
public class AbilitySimpleUnitsInPatternGetter : PropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IRule
{
	public bool IncludeCaster;

	public bool OnlyEnemy;

	protected override int GetBaseValue()
	{
		int num = 0;
		AbilityExecutionContext sourceAbilityContext = this.GetMechanicContext().SourceAbilityContext;
		if (sourceAbilityContext == null)
		{
			throw new Exception("Can't count targets in pattern of not casted ability");
		}
		foreach (CustomGridNodeBase node in sourceAbilityContext.Pattern.Nodes)
		{
			BaseUnitEntity unit = node.GetUnit();
			if (unit != null && (!IncludeCaster || unit != sourceAbilityContext.Caster) && (!OnlyEnemy || sourceAbilityContext.Caster == null || !unit.IsAlly(sourceAbilityContext.Caster)))
			{
				num++;
			}
		}
		return num;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!OnlyEnemy)
		{
			return "Count of units in ability pattern";
		}
		return "Count of caster's enemies in ability pattern";
	}
}
