using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

[CreateAssetMenu(menuName = "ScriptableObjects/CombatHudSurfaceRendererAsset")]
public sealed class CombatHudSurfaceRendererAsset : ScriptableObject
{
	public OutlineSettings outlineSettings = OutlineSettings.Default;

	public FillSettings fillSettings = FillSettings.Default;

	public CombatHudCommand[] deploymentCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] movementCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] spaceMovementCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] abilityRangeCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] abilityPatternRangeCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] abilityPatternCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] allyStratagemStartCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] allyStratagemLoopCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] allyStratagemFinishCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] hostileStratagemStartCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] hostileStratagemLoopCommands = Array.Empty<CombatHudCommand>();

	public CombatHudCommand[] hostileStratagemFinishCommands = Array.Empty<CombatHudCommand>();

	[UsedImplicitly]
	private void OnValidate()
	{
		CombatHudCommand[] array = movementCommands;
		foreach (CombatHudCommand combatHudCommand in array)
		{
			combatHudCommand.OnValidate();
		}
		array = abilityRangeCommands;
		foreach (CombatHudCommand combatHudCommand2 in array)
		{
			combatHudCommand2.OnValidate();
		}
		array = abilityPatternRangeCommands;
		foreach (CombatHudCommand combatHudCommand3 in array)
		{
			combatHudCommand3.OnValidate();
		}
		array = abilityPatternCommands;
		foreach (CombatHudCommand combatHudCommand4 in array)
		{
			combatHudCommand4.OnValidate();
		}
		array = allyStratagemStartCommands;
		foreach (CombatHudCommand combatHudCommand5 in array)
		{
			combatHudCommand5.OnValidate();
		}
		array = allyStratagemLoopCommands;
		foreach (CombatHudCommand combatHudCommand6 in array)
		{
			combatHudCommand6.OnValidate();
		}
		array = allyStratagemFinishCommands;
		foreach (CombatHudCommand combatHudCommand7 in array)
		{
			combatHudCommand7.OnValidate();
		}
		array = hostileStratagemStartCommands;
		foreach (CombatHudCommand combatHudCommand8 in array)
		{
			combatHudCommand8.OnValidate();
		}
		array = hostileStratagemLoopCommands;
		foreach (CombatHudCommand combatHudCommand9 in array)
		{
			combatHudCommand9.OnValidate();
		}
		array = hostileStratagemFinishCommands;
		foreach (CombatHudCommand combatHudCommand10 in array)
		{
			combatHudCommand10.OnValidate();
		}
	}
}
