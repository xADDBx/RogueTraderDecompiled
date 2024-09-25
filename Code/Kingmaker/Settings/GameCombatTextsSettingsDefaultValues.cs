using System;

namespace Kingmaker.Settings;

[Serializable]
public class GameCombatTextsSettingsDefaultValues
{
	public EntitiesType ShowSpellName;

	public EntitiesType ShowAvoid;

	public EntitiesType ShowMiss;

	public EntitiesType ShowAttackOfOpportunity;

	public EntitiesType ShowCriticalHit;

	public EntitiesType ShowSneakAttack;

	public EntitiesType ShowDamage;

	public EntitiesType ShowSaves;

	public ExtendedConditionType ShowPartyActions;

	public ConditionType ShowPartyActionsNoIdle;

	public ConditionType ShowEnemyActions;

	public ConditionType ShowPartyHP;

	public ConditionType ShowEnemyHP;

	public bool PartyHPIsShort;

	public bool EnemyHPIsShort;

	public ConditionType ShowNumericCooldownParty;

	public ConditionType ShowNumericCooldownEnemy;

	public ConditionType ShowNamesForParty;

	public ConditionType ShowNamesForEnemy;

	public ConditionType ShowPartyAttackIntentions;

	public ConditionType ShowPartyCastIntentions;

	public ConditionType ShowEnemyIntentions;
}
