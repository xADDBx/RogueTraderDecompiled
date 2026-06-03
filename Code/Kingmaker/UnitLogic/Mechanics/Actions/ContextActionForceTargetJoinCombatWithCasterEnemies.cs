using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Groups;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d81449bb835f4991becfc22b7f097370")]
[KDB("Экшен, который принудительно заставляет контекстную цель вступить в бой с текущими противниками кастера.\nРаботает даже если противники не являются атакуемой фракцией для цели.")]
public class ContextActionForceTargetJoinCombatWithCasterEnemies : ContextAction
{
	public override string GetCaption()
	{
		return "Force target to join combat with caster enemies";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null || !maybeCaster.IsInCombat)
		{
			Element.LogError(this, "Caster is missing or not in combat!");
			return;
		}
		UnitGroup unitGroup = maybeCaster.GetCombatGroupOptional()?.Group;
		if (unitGroup == null)
		{
			Element.LogError(this, "Caster combat group is missing!");
			return;
		}
		if (!(base.Target.Entity is BaseUnitEntity baseUnitEntity))
		{
			Element.LogError(this, "Target is missing or not a BaseUnitEntity!");
			return;
		}
		UnitCombatJoinController controller = Game.Instance.GetController<UnitCombatJoinController>(includeInactive: true);
		if (controller == null)
		{
			return;
		}
		PartCombatGroup combatGroup = baseUnitEntity.CombatGroup;
		foreach (UnitGroupMemory.UnitInfo enemy in unitGroup.Memory.Enemies)
		{
			BaseUnitEntity unit = enemy.Unit;
			PartCombatGroup combatGroup2 = unit.CombatGroup;
			if (!combatGroup2.IsEnemy(combatGroup.Group))
			{
				unit.Faction.AttackFactions.Add(baseUnitEntity.Faction.Blueprint);
				combatGroup2.UpdateAttackFactionsCache();
				combatGroup.UpdateAttackFactionsCache();
				controller.StartScriptedCombat(unit, baseUnitEntity);
			}
		}
	}
}
