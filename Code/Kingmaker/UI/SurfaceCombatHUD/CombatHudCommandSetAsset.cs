using System;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

[CreateAssetMenu(menuName = "ScriptableObjects/CombatHudCommandSetAsset")]
public sealed class CombatHudCommandSetAsset : ScriptableObject
{
	public CombatHudCommand[] Commands = Array.Empty<CombatHudCommand>();

	public CombatHudAreas GetUsedAreas()
	{
		CombatHudAreas combatHudAreas = ~(CombatHudAreas.Walkable | CombatHudAreas.Movement | CombatHudAreas.ActiveUnit | CombatHudAreas.AttackOfOpportunity | CombatHudAreas.AbilityMinRange | CombatHudAreas.AbilityMaxRange | CombatHudAreas.AbilityEffectiveRange | CombatHudAreas.AbilityPrimary | CombatHudAreas.AbilitySecondary | CombatHudAreas.StratagemAlly | CombatHudAreas.StratagemAllyIntersection | CombatHudAreas.StratagemHostile | CombatHudAreas.StratagemHostileIntersection | CombatHudAreas.SpaceMovement1 | CombatHudAreas.SpaceMovement2 | CombatHudAreas.SpaceMovement3);
		if (Commands != null)
		{
			CombatHudCommand[] commands = Commands;
			foreach (CombatHudCommand combatHudCommand in commands)
			{
				combatHudAreas |= combatHudCommand.GetUsedAreas();
			}
		}
		return combatHudAreas;
	}
}
