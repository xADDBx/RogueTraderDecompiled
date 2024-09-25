namespace Kingmaker.EntitySystem.Persistence.Versioning;

public enum PlayerUpgradeActionType
{
	GiveObjective = 0,
	CompleteObjective = 1,
	FailObjective = 2,
	StartKingdomEvent = 3,
	RemoveKingdomEvent = 4,
	FixKingdomDecks = 5,
	GiveItem = 6,
	RemoveItem = 7,
	ResetObjective = 8,
	UnlockLocation = 9,
	ForgetPartySpell = 10,
	MakeUnitEssentialForGame = 11,
	MakeUnitNotEssentialForGame = 12,
	FillSettlement = 15,
	UnrecruitCompanion = 16,
	AttachAllPartyMembers = 17,
	AttachKingdomBuff = 18,
	RemoveKingdomBuff = 19,
	RemoveKingdomEventAll = 20
}
