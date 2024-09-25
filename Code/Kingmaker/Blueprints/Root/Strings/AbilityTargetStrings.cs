using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class AbilityTargetStrings : StringsContainer
{
	[Header("Target unit")]
	public LocalizedString Personal;

	public LocalizedString TargetPoint;

	public LocalizedString OneCreature;

	public LocalizedString OneEnemyCreature;

	public LocalizedString OneFriendlyCreature;

	public LocalizedString AllCreatures;

	public LocalizedString AllEnemies;

	public LocalizedString AllAllies;

	public LocalizedString FirstCreature;

	public LocalizedString FirstEnemyCreature;

	public LocalizedString FirstFriendlyCreature;

	[Header("Form")]
	public LocalizedString WithinCone;

	public LocalizedString WithinLine;

	public LocalizedString WithinBurstCenteredOn;

	public LocalizedString InsideAreaOfEffect;

	public LocalizedString Movement;

	public LocalizedString InsideSelectedCombatArea;

	[Header("Position")]
	public LocalizedString CenteredOnCaster;

	public LocalizedString CenteredOnTargetPoint;

	public LocalizedString CenteredOnTargetCreature;

	[Header("Scatter shot")]
	public LocalizedString EveryShot;
}
