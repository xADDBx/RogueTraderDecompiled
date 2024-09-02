using Kingmaker.Pathfinding;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UI.Pointer.AbilityTarget;

public class AbilityCounterAttackRange : AbilityRange
{
	protected override bool CanEnable()
	{
		if (base.CanEnable())
		{
			return Ability.GetPatternSettings() == null;
		}
		return false;
	}

	protected override void SetRangeToWorldPosition(Vector3 castPosition)
	{
		int counterAttackRange = Ability.CounterAttackRange;
		if (counterAttackRange >= 0)
		{
			NodeList nodes = Ability.Caster.GetOccupiedNodes(Game.Instance.VirtualPositionController.GetDesiredPosition(Ability.Caster));
			if (GridPatterns.TryGetEnclosingRect(in nodes, out var result))
			{
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHudInfo = default(CombatHUDRenderer.AbilityAreaHudInfo);
				abilityAreaHudInfo.pattern = OrientedPatternData.Empty;
				abilityAreaHudInfo.casterRect = result;
				abilityAreaHudInfo.minRange = counterAttackRange;
				abilityAreaHudInfo.maxRange = counterAttackRange;
				abilityAreaHudInfo.effectiveRange = 0;
				abilityAreaHudInfo.ignoreRangesByDefault = false;
				abilityAreaHudInfo.ignorePatternPrimaryAreaByDefault = Ability.IsStarshipAttack;
				abilityAreaHudInfo.combatHudCommandsOverride = Ability.Blueprint.CombatHudCommandsOverride;
				CombatHUDRenderer.AbilityAreaHudInfo abilityAreaHUD = abilityAreaHudInfo;
				CombatHUDRenderer.Instance.SetAbilityAreaHUD(abilityAreaHUD);
			}
		}
	}
}
