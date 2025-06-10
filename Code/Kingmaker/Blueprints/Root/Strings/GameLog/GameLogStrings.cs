using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

public class GameLogStrings : StringsContainer
{
	public Color32 DefaultColor;

	public Color32 ColorMultiplier;

	[Header("Sprites")]
	public Sprite LeftArrowSprite;

	public Sprite RightArrowSprite;

	public Sprite DiesSprite;

	public Sprite MomentumSprite;

	public Sprite VeilThicknessSprite;

	public Sprite BuffSprite;

	public Sprite TraumaSprite;

	public Sprite FolderSprite;

	public Sprite EmptySprite;

	[Header("ColorText")]
	public ColorTextString MomentumColorText;

	public ColorTextString VeilThicknessColorText;

	public ColorTextString PushColorText;

	public ColorTextString OverpenetrationColorText;

	public ColorTextString CriticalHitColorText;

	[Header("Warhammer Prototype")]
	public GameLogMessage WarhammerDealDamage;

	public GameLogMessage WarhammerSourceDealDamage;

	public GameLogMessage WarhammerSourceDealCriticalDamage;

	public GameLogMessage WarhammerHitNoDamage;

	public GameLogMessage WarhammerRFHit;

	public GameLogMessage WarhammerMeleeHitSuperiority;

	public GameLogMessage WarhammerMeleeRFHitSuperiority;

	public GameLogMessage WarhammerCoverHit;

	public GameLogMessage WarhammerMiss;

	public GameLogMessage WarhammerDodge;

	public GameLogMessage WarhammerParry;

	public GameLogMessage WarhammerBlock;

	public GameLogMessage WarhammerDodgeAndParry;

	public GameLogMessage WarhammerDamageNegated;

	public GameLogMessage WarhammerParrySuperiority;

	public GameLogMessage WarhammerScatterHitFull;

	[Header("Starship")]
	public GameLogMessage WarhammerStarshipMiss;

	public GameLogMessage WarhammerStarshipTargetDisruptionMiss;

	public GameLogMessage WarhammerStarshipMissProximityMayFollow;

	public GameLogMessage WarhammerStarshipHit;

	public GameLogMessage WarhammerStarshipShieldHit;

	public GameLogMessage WarhammerStarshipShieldHullHit;

	public GameLogMessage WarhammerStarshipCriticalHit;

	public GameLogMessage WarhammerStarshipCriticalShieldHullHit;

	public GameLogMessage WarhammerStarshipLanceMiss;

	public GameLogMessage WarhammerStarshipLanceHit;

	public GameLogMessage WarhammerStarshipLanceCrit;

	public GameLogMessage WarhammerStarshipSteadyHandActivation;

	public GameLogMessage WarhammerStarshipAttackGroup;

	[Header("Warhammer Tooltip Bricks Attack Result")]
	public TooltipBrickAttackResultStrings AttackResultStrings;

	[Header("Warhammer Tooltip Bricks Attack")]
	public TooltipBrickStrings TooltipBrickStrings;

	[Header("Items")]
	public GameLogMessage ItemGained;

	public GameLogMessage ItemsGained;

	public GameLogMessage ItemLost;

	public GameLogMessage ItemsLost;

	public GameLogMessage ItemEquipped;

	public GameLogMessage ItemUnequipped;

	public GameLogMessage ItemIdentified;

	public GameLogMessage ItemUnidentified;

	public GameLogMessage FactionReputationGained;

	public GameLogMessage FactionReputationLost;

	public GameLogMessage ItemGroup;

	[Header("Cargo")]
	public GameLogMessage CargoCreated;

	public GameLogMessage CargoCreatedWithCapacity;

	public GameLogMessage CargoReplenished;

	public GameLogMessage CargoFormed;

	public GameLogMessage CargoRemoved;

	public GameLogMessage ItemSendToCargo;

	public GameLogMessage ItemSendToCargoAndCargoFormed;

	public GameLogMessage CargoGroup;

	[Header("Scrap")]
	public GameLogMessage ScrapGained;

	public GameLogMessage ScrapSpend;

	[Header("Combat")]
	public SavingThrowMessage SavingThrowSuccess;

	public SavingThrowMessage SavingThrowFail;

	public GameLogMessage SavingThrowGroup;

	public GameLogMessage UnitDeath;

	public GameLogMessage UnitFallsUnconscious;

	public DamageLogMessage Damage;

	public StatDamageLogMessage StatDamage;

	public StatDamageLogMessage StatDrain;

	public GameLogMessage UnitMissedTurn;

	public GameLogMessage TargetContextActionKill;

	public GameLogMessage SourceContextActionKill;

	public GameLogMessage InterruptTurn;

	[Header("Healing")]
	public HealLogMessage HealDamage;

	public StatDamageLogMessage HealStatDamage;

	public StatDamageLogMessage HealStatDrain;

	[Header("Spells")]
	public GameLogMessage UseAbility;

	public GameLogMessage UseAbilityOnTarget;

	public GameLogMessage UseItem;

	public GameLogMessage SpellImmunity;

	public GameLogMessage StatusEffect;

	public GameLogMessage BuffWound;

	public GameLogMessage BuffTrauma;

	public GameLogMessage HealsWound;

	public GameLogMessage HealsTrauma;

	public GameLogMessage GroupStatusEffect;

	public GameLogMessage PerilsOfTheWarp;

	public GameLogMessage TemporaryHitPointsAdd;

	public GameLogMessage TemporaryHitPointsRemove;

	[Header("Checks")]
	public GameLogMessage SkillCheckSuccess;

	public GameLogMessage SkillCheckFail;

	public GameLogMessage PartySkillCheckSuccess;

	public GameLogMessage PartySkillCheckFail;

	[Header("Thievery")]
	public GameLogMessage ThieveryMissing;

	public GameLogMessage LockIsJammed;

	public GameLogMessage CantDisarmTrap;

	public GameLogMessage PickLockSuccess;

	public GameLogMessage PickLockFail;

	public GameLogMessage PickLockCriticalFail;

	public GameLogMessage DisarmTrapSuccess;

	public GameLogMessage DisarmTrapFail;

	public GameLogMessage DisarmTrapCriticalFail;

	[Header("Perception")]
	public GameLogMessage TrapSpotted;

	public GameLogMessage DoorSpotted;

	[Header("Barks")]
	public GameLogMessage UnitBark;

	public GameLogMessage ObjectBark;

	[Header("Events")]
	public GameLogMessage DialogStarted;

	public GameLogMessage DialogEnded;

	public GameLogMessage BookStarted;

	public GameLogMessage BookEnded;

	public GameLogMessage CombatStarted;

	public GameLogMessage CombatEnded;

	[Header("Misc")]
	public GameLogMessage XpGain;

	public GameLogMessage TimePassed;

	[Header("Encumbrance Texts")]
	public GameLogMessage UnitEncumbranceLight;

	public GameLogMessage UnitEncumbranceMedium;

	public GameLogMessage UnitEncumbranceHeavy;

	public GameLogMessage UnitEncumbranceOverload;

	public GameLogMessage PartyEncumbranceLight;

	public GameLogMessage PartyEncumbranceMedium;

	public GameLogMessage PartyEncumbranceHeavy;

	public GameLogMessage PartyEncumbranceOverload;

	[Header("Space Exploration")]
	public GameLogMessage AnomalyCheckSuccess;

	public GameLogMessage AnomalyCheckFail;

	public GameLogMessage AnomalyInteractionSuccess;

	public GameLogMessage AnomalyInteractionFail;

	[Header("Colonization")]
	public GameLogMessage CreateColony;

	public GameLogMessage ProjectStarted;

	public GameLogMessage ProjectFinished;

	public GameLogMessage ContentmentChangedInColony;

	public GameLogMessage ContentmentChangedInAllColonies;

	public GameLogMessage EfficiencyChangedInColony;

	public GameLogMessage EfficiencyChangedInAllColonies;

	public GameLogMessage SecurityChangedInColony;

	public GameLogMessage SecurityChangedInAllColonies;

	public GameLogMessage ResourceGained;

	public GameLogMessage ResourceLost;

	public GameLogMessage ProfitFactorGained;

	public GameLogMessage ProfitFactorLost;

	public GameLogMessage NavigatorResourceGained;

	public GameLogMessage NavigatorResourceLost;

	public GameLogMessage ChronicleFinished;

	[Header("Starship Progression")]
	public GameLogMessage StarshipLevelUp;

	public GameLogMessage StarshipExpToNextLevel;

	public GameLogMessage StarshipExperiencePerArea;

	[Header("Veil thickness")]
	public GameLogMessage VeilThicknessValueChanged;

	public GameLogMessage VeilThicknessStateChanged;

	[Header("Momentum")]
	public GameLogMessage MomentumValueChanged;

	[Header("Separator")]
	public GameLogMessage SeparatorStart;

	public GameLogMessage SeparatorFinish;

	public static GameLogStrings Instance => BlueprintRoot.Instance.LocalizedTexts.GameLog;
}
