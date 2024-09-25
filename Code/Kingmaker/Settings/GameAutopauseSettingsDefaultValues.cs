using System;

namespace Kingmaker.Settings;

[Serializable]
public class GameAutopauseSettingsDefaultValues
{
	public bool ContinueMovementOnEngagement;

	public bool PauseOnLostFocus;

	public bool PauseOnTrapDetected;

	public bool PauseOnHiddenObjectDetected;

	public bool PauseOnEngagement;

	public bool PauseOnMeleeEngagement;

	public bool PauseOnPartyIsAttacked;

	public bool PauseOnEndOfPartyMembersRound;

	public bool PauseOnEndOfRound;

	public bool PauseOnPartyMemberFinishedAbility;

	public EntitiesType PauseOnSpellcastInterrupted;

	public EntitiesType PauseOnSpellcastStarted;

	public bool PauseOnSpellcastFinished;

	public bool PauseOnEndedBuffSummon;

	public bool PauseOnAllyDown;

	public bool PauseOnEnemyDown;

	public bool PauseOnNewEnemyAppeared;

	public bool PauseOnLowHealth;

	public bool PauseOnAttackOfOpportunity;

	public bool PauseOnPartyMemberRanOutOfConsumable;

	public bool PauseOnEnemySpotted;

	public bool PauseOnWeaponIsIneffective;

	public bool PauseOnAreaLoaded;

	public bool PauseWhenAllyUnconscious;

	public bool PauseWhenEnemyUnconscious;

	public bool PauseOnLoadingScreen;
}
