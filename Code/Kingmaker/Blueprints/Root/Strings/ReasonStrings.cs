using Kingmaker.Localization;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

public class ReasonStrings : StringsContainer
{
	[Header("Spell Unavailable Reasons")]
	public LocalizedString UnavailableGeneric;

	public LocalizedString AbilityDisabled;

	public LocalizedString CastingForbidden;

	public LocalizedString AbilitiesForbidden;

	public LocalizedString CasterStaggered;

	public LocalizedString CasterPolymorphed;

	public LocalizedString NoResources;

	public LocalizedString NoResourcesWounds;

	public LocalizedString NoResourcesBuff;

	public LocalizedString OutOfSpellsPerDay;

	public LocalizedString NotInSpellList;

	[InfoBox("Not enough burn left for activating ability")]
	public LocalizedString KineticNotEnoughBurnLeft;

	[InfoBox("Caster has no required condition: {text}")]
	public LocalizedString NoRequiredCondition;

	[InfoBox("Caster has forbidden condition: {text}")]
	public LocalizedString HasForbiddenCondition;

	public LocalizedString MaterialComponentRequired;

	[InfoBox("Caster has no required alignment: {text}")]
	public LocalizedString SpecificAlignmentRequired;

	public LocalizedString SingleShotBallisticWeaponRequired;

	public LocalizedString BurstFireBallisticWeaponRequired;

	public LocalizedString MeleeWeaponRequired;

	public LocalizedString TargetsAroundRequired;

	public LocalizedString TwoHandedWeaponRequired;

	public LocalizedString ChosenWeaponRequired;

	[InfoBox("Caster's weapon does not meet prerequisites: {text}")]
	public LocalizedString SpecificWeaponRequired;

	public LocalizedString CombatRequired;

	public LocalizedString SpellbookForbidden;

	public LocalizedString SpellbookForbiddenAlignment;

	public LocalizedString SpellbookForbiddenArmor;

	public LocalizedString DualCompanionIsDead;

	public LocalizedString NotEnoughSpace;

	[Header("Target restrictions")]
	[InfoBox("Target must has any of alignment components: {text}")]
	public LocalizedString TargetAlignment;

	[InfoBox("Target must be dead no longer then 2 rounds")]
	public LocalizedString TargetBreathOfLife;

	public LocalizedString TargetDivineTroth;

	[InfoBox("Target has no required unit condition: {text}")]
	public LocalizedString TargetHasCondition;

	[InfoBox("Target has forbidden unit condition: {text}")]
	public LocalizedString TargetHasNoCondition;

	[InfoBox("Target has no required condition or buffs: {text}")]
	public LocalizedString TargetHasConditionOrBuff;

	[InfoBox("Target has forbidden condition and/or buffs: {text}")]
	public LocalizedString TargetHasNoConditionAndBuff;

	[InfoBox("Target has no any of required facts: {text}")]
	public LocalizedString TargetHasFact;

	[InfoBox("Target has any of forbidden facts: {text}")]
	public LocalizedString TargetHasNoFact;

	public LocalizedString TargetHasMeleeWeaponInPrimaryHand;

	[InfoBox("Target must has no one facts from {text} unless it has fact {description}")]
	public LocalizedString TargetHasNoFactUnless;

	public LocalizedString TargetHPCondition;

	[InfoBox("Target must be an ally")]
	public LocalizedString TargetIsAlly;

	[InfoBox("Target must be an enemy")]
	public LocalizedString TargetIsEnemy;

	public LocalizedString TargetHasMeleeWeapon;

	public LocalizedString TargetHasNoMeleeWeapon;

	public LocalizedString CasterHasNoWeaponOfClassification;

	public LocalizedString TargetIsDeadCompanion;

	public LocalizedString TargetIsDevoured;

	public LocalizedString TargetIsFavoredEnemy;

	[InfoBox("Target must be a party member")]
	public LocalizedString TargetIsPartyMember;

	[InfoBox("Target must not be a party member")]
	public LocalizedString TargetIsNotPartyMember;

	public LocalizedString TargetIsSuitableMount;

	public LocalizedString TargetMaximumHitDice;

	public LocalizedString TargetNotSelf;

	[InfoBox("Target's stat {text} must be greater than {description}")]
	public LocalizedString TargetStatCondition;

	[InfoBox("Target's stat {text} must be lower or equal than {description}")]
	public LocalizedString TargetStatConditionLowerOrEqual;

	public LocalizedString TargetStoneToFlesh;

	[InfoBox("Target must can see caster")]
	public LocalizedString TargetCanSeeCaster;

	[InfoBox("Target must can not see caster")]
	public LocalizedString TargetCanNotSeeCaster;

	[InfoBox("Target must can act")]
	public LocalizedString TargetCanAct;

	public LocalizedString TargetIsTooFar;

	public LocalizedString TargetIsTooClose;

	public LocalizedString TargetIsNotPropheticIntervention;

	public LocalizedString ObstacleBetweenCasterAndTarget;

	public LocalizedString AlreadyMovedThisTurn;

	[InfoBox("Default error message for invalid target (unit expected but target is point)")]
	public LocalizedString TargetIsInvalid;

	public LocalizedString CanNotReachTarget;

	[InfoBox("Target not in area effect {text}")]
	public LocalizedString TargetNotInAreaEffect;

	public LocalizedString TargetTooFar;

	public LocalizedString TargetTooClose;

	public LocalizedString HasNoLosToTarget;

	public LocalizedString AreaEffectsCannotOverlap;

	[Header("Tactical Combat")]
	public LocalizedString NotAllowedCellToCast;

	public LocalizedString TooManySquads;

	public LocalizedString HasNoPriorityTarget;

	public LocalizedString HasNoAttacksForPriorityTarget;

	public LocalizedString HasNoLosToPriorityTarget;

	public LocalizedString NotEnoughMorale;

	public LocalizedString IsOnCooldown;

	public LocalizedString IsOnCooldownUntilEndOfCombat;

	public LocalizedString CannotUseInThreatenedArea;

	public LocalizedString NotEnoughAmmo;

	public LocalizedString AlreadyFullAmmo;

	public LocalizedString CannotUseAbilityDuringInterruptingTurn;

	public LocalizedString CanUseAbilityOnlyDuringInterruptingTurn;

	[InfoBox("Momentum must score {text} or lower to activate Desperate Measure.")]
	public LocalizedString NotYetDesperateMeasures;

	public LocalizedString NotYetHeroicAct;

	public LocalizedString AlreadyDesperateMeasuredThisTurn;

	public LocalizedString AlreadyHeroicActed;

	public LocalizedString NotEnoughActionPoints;

	public LocalizedString InteractionIsTooFar;

	public LocalizedString AlreadyInteractedInThisCombatRound;

	public LocalizedString UltimateAbilityIsUsedThisRound;

	[Header("Space Combat")]
	[InfoBox("No suitable weapons were used during Acceleration phase")]
	public LocalizedString NoWeaponsUsedDuringPhase;

	[InfoBox("Can only be used during Acceleration phase of movement")]
	public LocalizedString PastPushPhase;

	[InfoBox("Can only target path's final node")]
	public LocalizedString CanOnlyTargetFinalNode;

	[InfoBox("Path is blocked")]
	public LocalizedString PathBlocked;

	[InfoBox("No active torpedo")]
	public LocalizedString NoActiveTorpedo;

	public LocalizedString ShouldFlyFurtherToEndTurn;

	[InfoBox("Can only be used during Ending phase of movement")]
	public LocalizedString NotEndingPhase;

	[InfoBox("Can't launch more wings")]
	public LocalizedString CantLaunchMoreWings;

	[InfoBox("Officer doesn't have enough skill")]
	public LocalizedString TooLowPostSkill;

	[Header("DisconnectCause")]
	public LocalizedString NoInternetConnection;

	public LocalizedString ReceivedAllRewards;

	public LocalizedString RedirectionToWeb;

	public LocalizedString NoRewardsAvailable;

	public LocalizedString ExceptionOnConnect;

	public LocalizedString DnsExceptionOnConnect;

	public LocalizedString ServerAddressInvalid;

	public LocalizedString Exception;

	public LocalizedString ServerTimeout;

	public LocalizedString ClientTimeout;

	public LocalizedString DisconnectByServerLogic;

	public LocalizedString DisconnectByServerReasonUnknown;

	public LocalizedString InvalidAuthentication;

	public LocalizedString CustomAuthenticationFailed;

	public LocalizedString AuthenticationTicketExpired;

	public LocalizedString MaxCcuReached;

	public LocalizedString InvalidRegion;

	public LocalizedString OperationNotAllowedInCurrentState;

	public LocalizedString DisconnectByClientLogic;

	public LocalizedString DisconnectByOperationLimit;

	public LocalizedString DisconnectByDisconnectMessage;

	public LocalizedString ApplicationQuit;

	public static ReasonStrings Instance => Game.Instance.BlueprintRoot.LocalizedTexts.Reasons;

	public string GetDisconnectCause(DisconnectCause cause)
	{
		return cause switch
		{
			DisconnectCause.ExceptionOnConnect => ExceptionOnConnect, 
			DisconnectCause.DnsExceptionOnConnect => DnsExceptionOnConnect, 
			DisconnectCause.ServerAddressInvalid => ServerAddressInvalid, 
			DisconnectCause.Exception => Exception, 
			DisconnectCause.ServerTimeout => ServerTimeout, 
			DisconnectCause.ClientTimeout => ClientTimeout, 
			DisconnectCause.DisconnectByServerLogic => DisconnectByServerLogic, 
			DisconnectCause.DisconnectByServerReasonUnknown => DisconnectByServerReasonUnknown, 
			DisconnectCause.InvalidAuthentication => InvalidAuthentication, 
			DisconnectCause.CustomAuthenticationFailed => CustomAuthenticationFailed, 
			DisconnectCause.AuthenticationTicketExpired => AuthenticationTicketExpired, 
			DisconnectCause.MaxCcuReached => MaxCcuReached, 
			DisconnectCause.InvalidRegion => InvalidRegion, 
			DisconnectCause.OperationNotAllowedInCurrentState => OperationNotAllowedInCurrentState, 
			DisconnectCause.DisconnectByClientLogic => DisconnectByClientLogic, 
			DisconnectCause.DisconnectByOperationLimit => DisconnectByOperationLimit, 
			DisconnectCause.DisconnectByDisconnectMessage => DisconnectByDisconnectMessage, 
			DisconnectCause.ApplicationQuit => ApplicationQuit, 
			_ => string.Empty, 
		};
	}
}
