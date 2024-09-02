using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.Settings;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Controllers.Combat;

public class PartUnitCombatState : BaseUnitPart, IRoundStartHandler, ISubscriber, IWarhammerAttackHandler, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, IStarshipAttackHandler, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitCombatState>, IEntityPartOwner
	{
		PartUnitCombatState CombatState { get; }
	}

	[JsonProperty]
	private bool m_InCombat;

	[JsonProperty]
	private bool m_LastCombatLeaveIgnoreLeaveTimer;

	[JsonProperty]
	private float? m_OverrideInitiative;

	[JsonProperty]
	public int DamageReceivedInCurrentFight;

	[JsonProperty]
	private EntityRef<MechanicEntity> m_LastTarget;

	[JsonProperty]
	private EntityRef<ItemEntityWeapon> m_LastAbilityWeapon;

	[JsonProperty]
	public Vector3? ReturnPosition { get; set; }

	[JsonProperty]
	public float? ReturnOrientation { get; set; }

	[JsonProperty]
	public int AttackInRoundCount { get; set; }

	[JsonProperty]
	public int AttackedInRoundCount { get; set; }

	[JsonProperty]
	public int HitInRoundCount { get; set; }

	[JsonProperty]
	public int GotHitInRoundCount { get; set; }

	[JsonProperty]
	public bool IsMovedThisTurn { get; set; }

	[JsonProperty]
	public bool SaveMPAfterUsingNextAbility { get; set; }

	[JsonProperty]
	public bool Surprised { get; private set; }

	[JsonProperty]
	public float ActionPointsBlue { get; private set; }

	[JsonProperty]
	public float MovedCellsThisTurn { get; private set; }

	[JsonProperty]
	public float ActionPointsBlueSpentThisTurn { get; private set; }

	[JsonProperty]
	public float ActionPointsBlueMax { get; set; }

	[JsonProperty]
	public int ActionPointsYellow { get; private set; }

	[JsonProperty]
	public int AttacksOfOpportunityMadeThisTurnCount { get; set; }

	[JsonProperty]
	public int ForceMovedDistanceInCells { get; set; }

	[JsonProperty]
	public int LastStraightMoveLength { get; set; }

	[JsonProperty]
	public int LastDiagonalCount { get; set; }

	[JsonProperty]
	[CanBeNull]
	public MechanicEntity ManualTarget { get; set; }

	[JsonProperty]
	public SpellDescriptor StoryModeBuffImmunity { get; set; }

	[JsonProperty]
	public bool StoryModeEnergyDrainImmunity { get; set; }

	[JsonProperty]
	public bool StartedCombatNearEnemy { get; set; }

	[JsonProperty]
	public Vector3? LastAttackPosition { get; private set; }

	public bool RecheckEquipmentRestrictionsAfterCombatEnd { get; set; }

	public float? OverrideInitiative
	{
		get
		{
			return m_OverrideInitiative ?? base.Owner.Blueprint.GetComponent<InitiativeModifier>()?.OverrideInitiative;
		}
		set
		{
			m_OverrideInitiative = value;
		}
	}

	[CanBeNull]
	public MechanicEntity LastTarget
	{
		get
		{
			return m_LastTarget;
		}
		set
		{
			if (!(m_LastTarget != value))
			{
				return;
			}
			EntityRef<MechanicEntity> prevTarget = m_LastTarget;
			m_LastTarget = value;
			using (ProfileScope.New("LastTarget change"))
			{
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<ICombatTargetChangeHandler>)delegate(ICombatTargetChangeHandler h)
				{
					h.HandleTargetChange(prevTarget);
				}, isCheckRuntime: true);
			}
		}
	}

	[CanBeNull]
	public ItemEntityWeapon LastAbilityWeapon
	{
		get
		{
			return m_LastAbilityWeapon;
		}
		private set
		{
			if (!(m_LastAbilityWeapon == value))
			{
				EntityRef<ItemEntityWeapon> prevWeapon = m_LastAbilityWeapon;
				m_LastAbilityWeapon = value;
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<ILastAbilityWeaponChangeHandler>)delegate(ILastAbilityWeaponChangeHandler h)
				{
					h.HandleLastAbilityWeaponChange(prevWeapon, value);
				}, isCheckRuntime: true);
			}
		}
	}

	public float InitiativeRoll => base.Owner.Initiative.Roll;

	public bool IsInCombat => m_InCombat;

	public bool LastCombatLeaveIgnoreLeaveTimer => m_LastCombatLeaveIgnoreLeaveTimer;

	public bool IsEngaged
	{
		get
		{
			if (m_InCombat)
			{
				return base.Owner.GetEngagedByUnits().Any();
			}
			return false;
		}
	}

	public bool IsEngagedInRealOrVirtualPosition
	{
		get
		{
			if (Game.Instance.VirtualPositionController == null || !Game.Instance.VirtualPositionController.TryGetVirtualPosition(base.Owner, out var virtualPosition))
			{
				return IsEngaged;
			}
			if (m_InCombat)
			{
				return base.Owner.IsEngagedInPosition(virtualPosition);
			}
			return false;
		}
	}

	public bool CanActInCombat => m_InCombat;

	public bool CanAttackOfOpportunity
	{
		get
		{
			if (!base.Owner.State.HasCondition(UnitCondition.DisableAttacksOfOpportunity) && !base.Owner.Passive && (AttacksOfOpportunityMadeThisTurnCount < MaxAttacksOfOpportunityPerRound || base.Owner.IsPlayerFaction || base.Owner.Blueprint.DifficultyType >= UnitDifficultyType.Elite) && base.Owner.CanAttack((BaseUnitEntity unit) => unit.GetThreatHand()?.Weapon))
			{
				return base.Owner.GetThreatHand()?.Weapon.Blueprint.AttackOfOpportunityAbility != null;
			}
			return false;
		}
	}

	public int MaxAttacksOfOpportunityPerRound => Math.Max(1, StatsContainer.GetStat(StatType.AttackOfOpportunityCount));

	private StatsContainer StatsContainer => base.Owner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValue WarhammerInitialAPBlue => StatsContainer.GetStat(StatType.WarhammerInitialAPBlue);

	public ModifiableValue WarhammerInitialAPYellow => StatsContainer.GetStat(StatType.WarhammerInitialAPYellow);

	public ModifiableValue InitiativeBonus => StatsContainer.GetStat(StatType.Initiative);

	protected override void OnAttach()
	{
		InitializeStats();
	}

	protected override void OnPrePostLoad()
	{
		InitializeStats();
	}

	private void InitializeStats()
	{
		StatsContainer.Register(StatType.WarhammerInitialAPBlue);
		StatsContainer.Register(StatType.WarhammerInitialAPYellow);
		StatsContainer.Register(StatType.AttackOfOpportunityCount);
		StatsContainer.Register(StatType.Initiative);
		StatsContainer.Register(StatType.AttackOfOpportunityCount).BaseValue = 1;
	}

	public void JoinCombat(bool surprised = false)
	{
		if (m_InCombat)
		{
			return;
		}
		Clear();
		base.Owner.CombatGroup.IsInCombat.Retain();
		m_InCombat = true;
		m_LastCombatLeaveIgnoreLeaveTimer = false;
		ResetActionPointsAll();
		AbstractUnitCommand current = base.Owner.Commands.Current;
		if (current != null && !SettingsRoot.Game.TurnBased.EnableTurnBasedMode && current.IsActed && !current.IsFinished)
		{
			return;
		}
		base.Owner.CachedPerceptionRoll = 0;
		ActionPointsYellow = 0;
		DamageReceivedInCurrentFight = 0;
		if (!base.Owner.Faction.IsPlayer)
		{
			UnitMoveTo currentMoveTo = base.Owner.Commands.CurrentMoveTo;
			if (currentMoveTo != null)
			{
				ReturnPosition = currentMoveTo.Target;
				ReturnOrientation = currentMoveTo.Orientation;
			}
			else
			{
				ReturnPosition = SizePathfindingHelper.FromViewToMechanicsPosition(base.Owner, base.Owner.Position, inBattle: true).GetNearestNodeXZUnwalkable()?.Vector3Position ?? base.Owner.CurrentNode.position;
				base.Owner.Position = ReturnPosition.Value;
				ReturnOrientation = base.Owner.DesiredOrientation;
			}
		}
		else
		{
			ReturnPosition = null;
			ReturnOrientation = null;
		}
		Surprised = surprised;
		CutsceneControlledUnit.UpdateActiveCutscene(base.Owner);
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitCombatHandler>)delegate(IUnitCombatHandler h)
		{
			h.HandleUnitJoinCombat();
		}, isCheckRuntime: true);
		if ((bool)base.Owner.View.Animator)
		{
			base.Owner.View.HandsEquipment?.SetCombatState(inCombat: true);
			if (base.Owner.View.gameObject.activeSelf)
			{
				base.Owner.View.UpdateCombatSwitch();
			}
		}
		base.Owner.Wake();
		PFLog.Default.Log(base.Owner.View, "Unit join combat: {0}", base.Owner);
		EventBus.RaiseEvent(delegate(IAnyUnitCombatHandler h)
		{
			h.HandleUnitJoinCombat(base.Owner);
		});
	}

	public void LeaveCombat(bool ignoreLeaveTimer = false)
	{
		if (!m_InCombat)
		{
			return;
		}
		m_InCombat = false;
		m_LastCombatLeaveIgnoreLeaveTimer = ignoreLeaveTimer;
		base.Owner.CombatGroup.IsInCombat.Release();
		ReturnToStartingPositionIfNeeded();
		Clear();
		if (!base.Owner.Destroyed)
		{
			base.Owner.Commands.InterruptAiCommands();
			CutsceneControlledUnit.UpdateActiveCutscene(base.Owner);
			if (base.Owner.View != null && base.Owner.View.Animator != null)
			{
				base.Owner.View.HandsEquipment.SetCombatState(inCombat: false);
				if (base.Owner.View.gameObject.activeSelf)
				{
					base.Owner.View.UpdateCombatSwitch();
				}
			}
		}
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitCombatHandler>)delegate(IUnitCombatHandler h)
		{
			h.HandleUnitLeaveCombat();
		}, isCheckRuntime: true);
		EventBus.RaiseEvent(delegate(IAnyUnitCombatHandler h)
		{
			h.HandleUnitLeaveCombat(base.Owner);
		});
		PFLog.Default.Log(base.Owner.View, "Unit leave combat: {0}", base.Owner);
		if (RecheckEquipmentRestrictionsAfterCombatEnd)
		{
			base.Owner.UnequipItemsWithFailedRestrictions();
			RecheckEquipmentRestrictionsAfterCombatEnd = false;
		}
	}

	public void ReturnToStartingPositionIfNeeded()
	{
		if (base.Owner.Blueprint.IsStayOnSameSpotAfterCombat || base.Owner.Commands.CurrentMoveTo != null || !ReturnPosition.HasValue || !(Vector3.Distance(ReturnPosition.Value, base.Owner.Position) > 0.1f))
		{
			return;
		}
		PathfindingService.Instance.FindPathRT_Delayed(base.Owner.MovementAgent, ReturnPosition.Value, 0.3f, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (base.Owner == null)
			{
				path.Claim(path);
				path.Release(path);
			}
			else
			{
				UnitMoveToParams cmdParams = new UnitMoveToParams(path, ReturnPosition.Value)
				{
					Orientation = ReturnOrientation,
					DoNotInterruptAfterFight = true
				};
				base.Owner.Commands.Run(cmdParams);
			}
		});
	}

	public bool CanEndTurn()
	{
		PartStarshipNavigation starshipNavigationOptional = base.Owner.GetStarshipNavigationOptional();
		if (starshipNavigationOptional != null)
		{
			if (!(ActionPointsBlue < (float)starshipNavigationOptional.FinishingTilesCount))
			{
				return !starshipNavigationOptional.HasAnotherPlaceToStand;
			}
			return true;
		}
		return true;
	}

	public void PrepareForNewTurn(bool isTurnBased)
	{
		LastStraightMoveLength = 0;
		LastDiagonalCount = 0;
		ActionPointsBlueSpentThisTurn = 0f;
		MovedCellsThisTurn = 0f;
		base.Owner.Initiative.PreparationInterrupted = false;
		if (isTurnBased)
		{
			int result = Rulebook.Trigger(new RuleCalculateActionPoints(base.Owner, isTurnBased: true)).Result;
			int result2 = Rulebook.Trigger(new RuleCalculateMovementPoints(base.Owner)).Result;
			ActionPointsYellow = result;
			float actionPointsBlueMax = (ActionPointsBlue = result2);
			ActionPointsBlueMax = actionPointsBlueMax;
		}
		AttacksOfOpportunityMadeThisTurnCount = 0;
		ForceMovedDistanceInCells = 0;
		if (isTurnBased && base.Owner is StarshipEntity starshipEntity)
		{
			starshipEntity.Hull.WeaponSlots.ForEach(delegate(WeaponSlot s)
			{
				s.Reload();
			});
		}
	}

	private void Clear()
	{
		Surprised = false;
		AttackInRoundCount = 0;
		AttackedInRoundCount = 0;
		HitInRoundCount = 0;
		GotHitInRoundCount = 0;
		ForceMovedDistanceInCells = 0;
		LastTarget = null;
		ManualTarget = null;
		LastAbilityWeapon = null;
		LastAttackPosition = null;
		StoryModeBuffImmunity = SpellDescriptor.None;
		StoryModeEnergyDrainImmunity = false;
		StartedCombatNearEnemy = false;
		SaveMPAfterUsingNextAbility = false;
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		AttackInRoundCount = 0;
		AttackedInRoundCount = 0;
		HitInRoundCount = 0;
		GotHitInRoundCount = 0;
		IsMovedThisTurn = false;
	}

	public void HandleAttack(RulePerformAttack rule)
	{
		if (rule.Initiator == base.Owner)
		{
			HandleAttackAsInitiator(rule.ResultIsHit, rule.Ability.Weapon);
		}
		else if (rule.Target == base.Owner)
		{
			HandleAttackAsTarget(rule.ResultIsHit, rule.ResultDamageRule?.Result);
		}
	}

	public void HandleAttack(RuleStarshipPerformAttack starshipAttack)
	{
		if (starshipAttack.Initiator == base.Owner)
		{
			HandleAttackAsInitiator(starshipAttack.ResultIsHit, null);
		}
		else if (starshipAttack.Target == base.Owner)
		{
			HandleAttackAsTarget(starshipAttack.ResultIsHit, starshipAttack.ResultDamage);
		}
	}

	private void HandleAttackAsInitiator(bool isAttackHit, ItemEntityWeapon weapon)
	{
		AttackInRoundCount++;
		if (isAttackHit)
		{
			HitInRoundCount++;
		}
		if (weapon != null)
		{
			LastAbilityWeapon = weapon;
		}
		LastAttackPosition = base.Owner.Position;
	}

	private void HandleAttackAsTarget(bool isAttackHit, int? resultDamage)
	{
		AttackedInRoundCount++;
		if (isAttackHit)
		{
			GotHitInRoundCount++;
			if (resultDamage.HasValue)
			{
				DamageReceivedInCurrentFight += resultDamage.Value;
			}
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor == base.Owner)
		{
			IsMovedThisTurn |= command.IsMoveUnit;
		}
	}

	public void ResetActionPointsAll()
	{
		ActionPointsBlueMax = (int)WarhammerInitialAPBlue;
		ActionPointsYellow = WarhammerInitialAPYellow;
		ActionPointsBlue = (int)WarhammerInitialAPBlue;
	}

	public void ResetActionPointsBlueCheat()
	{
		ActionPointsBlueSpentThisTurn = 0f;
		MovedCellsThisTurn = 0f;
		ActionPointsBlue = 100f;
	}

	public void SpendActionPoints(int? yellow = null, float? blue = null)
	{
		if (yellow > 0)
		{
			ActionPointsYellow = Math.Max(0, ActionPointsYellow - yellow.Value);
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentActionPoints>)delegate(IUnitSpentActionPoints h)
			{
				h.HandleUnitSpentActionPoints(yellow.Value);
			}, isCheckRuntime: true);
		}
		if (blue > 0f)
		{
			ActionPointsBlue = Math.Max(0f, ActionPointsBlue - blue.Value);
			ActionPointsBlueSpentThisTurn += blue.Value;
			RegisterMoveCells(blue.Value);
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentMovementPoints>)delegate(IUnitSpentMovementPoints h)
			{
				h.HandleUnitSpentMovementPoints(blue.Value);
			}, isCheckRuntime: true);
		}
	}

	public void RegisterMoveCells(float cells)
	{
		MovedCellsThisTurn += cells;
	}

	public void SpendActionPointsAll(bool yellow = false, bool blue = false)
	{
		if (yellow)
		{
			ActionPointsYellow = 0;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentActionPoints>)delegate(IUnitSpentActionPoints h)
			{
				h.HandleUnitSpentActionPoints(-1);
			}, isCheckRuntime: true);
		}
		if (blue)
		{
			ActionPointsBlue = 0f;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitSpentMovementPoints>)delegate(IUnitSpentMovementPoints h)
			{
				h.HandleUnitSpentMovementPoints(-1f);
			}, isCheckRuntime: true);
		}
	}

	public void GainYellowPoint(int value)
	{
		GainYellowPoint(value, null);
	}

	public void GainYellowPoint(int value, MechanicsContext context)
	{
		SetYellowPoint(ActionPointsYellow + Mathf.Max(0, value), context);
	}

	public void SetYellowPoint(int value)
	{
		SetYellowPoint(value, null);
	}

	public void SetYellowPoint(int value, MechanicsContext context)
	{
		if (value != ActionPointsYellow && value >= 0)
		{
			ActionPointsYellow = value;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitGainActionPoints>)delegate(IUnitGainActionPoints h)
			{
				h.HandleUnitGainActionPoints(ActionPointsYellow, context);
			}, isCheckRuntime: true);
		}
	}

	public void GainBluePoint(float value)
	{
		GainBluePoint(value, null);
	}

	public void GainBluePoint(float value, MechanicsContext context)
	{
		SetBluePoint(ActionPointsBlue + Mathf.Max(0f, value), context);
	}

	public void SetBluePoint(float value)
	{
		SetBluePoint(value, null);
	}

	public void SetBluePoint(float value, MechanicsContext context)
	{
		if (Math.Abs(value - ActionPointsBlue) > float.Epsilon && value >= 0f)
		{
			ActionPointsBlue = value;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitGainMovementPoints>)delegate(IUnitGainMovementPoints h)
			{
				h.HandleUnitGainMovementPoints(ActionPointsBlue, context);
			}, isCheckRuntime: true);
		}
	}

	public void GainActionPoints(int? yellow = null, float? blue = null, MechanicsContext context = null)
	{
		int? yellow2 = null;
		float? blue2 = null;
		if (yellow > 0)
		{
			yellow2 = ActionPointsYellow + yellow;
		}
		if (blue > 0f)
		{
			blue2 = ActionPointsBlue + blue;
		}
		SetActionPoints(yellow2, blue2, context);
	}

	public void SetActionPoints(int? yellow = null, float? blue = null, MechanicsContext context = null)
	{
		if (yellow >= 0)
		{
			ActionPointsYellow = yellow.Value;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitGainActionPoints>)delegate(IUnitGainActionPoints h)
			{
				h.HandleUnitGainActionPoints(yellow.Value, context);
			}, isCheckRuntime: true);
		}
		if (blue >= 0f)
		{
			ActionPointsBlue = blue.Value;
			EventBus.RaiseEvent((IMechanicEntity)base.Owner, (Action<IUnitGainMovementPoints>)delegate(IUnitGainMovementPoints h)
			{
				h.HandleUnitGainMovementPoints(blue.Value, context);
			}, isCheckRuntime: true);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_InCombat);
		result.Append(ref m_LastCombatLeaveIgnoreLeaveTimer);
		if (ReturnPosition.HasValue)
		{
			Vector3 val2 = ReturnPosition.Value;
			result.Append(ref val2);
		}
		if (ReturnOrientation.HasValue)
		{
			float val3 = ReturnOrientation.Value;
			result.Append(ref val3);
		}
		int val4 = AttackInRoundCount;
		result.Append(ref val4);
		int val5 = AttackedInRoundCount;
		result.Append(ref val5);
		int val6 = HitInRoundCount;
		result.Append(ref val6);
		int val7 = GotHitInRoundCount;
		result.Append(ref val7);
		bool val8 = IsMovedThisTurn;
		result.Append(ref val8);
		bool val9 = SaveMPAfterUsingNextAbility;
		result.Append(ref val9);
		bool val10 = Surprised;
		result.Append(ref val10);
		float val11 = ActionPointsBlue;
		result.Append(ref val11);
		float val12 = MovedCellsThisTurn;
		result.Append(ref val12);
		float val13 = ActionPointsBlueSpentThisTurn;
		result.Append(ref val13);
		float val14 = ActionPointsBlueMax;
		result.Append(ref val14);
		int val15 = ActionPointsYellow;
		result.Append(ref val15);
		int val16 = AttacksOfOpportunityMadeThisTurnCount;
		result.Append(ref val16);
		int val17 = ForceMovedDistanceInCells;
		result.Append(ref val17);
		int val18 = LastStraightMoveLength;
		result.Append(ref val18);
		int val19 = LastDiagonalCount;
		result.Append(ref val19);
		if (m_OverrideInitiative.HasValue)
		{
			float val20 = m_OverrideInitiative.Value;
			result.Append(ref val20);
		}
		result.Append(ref DamageReceivedInCurrentFight);
		Hash128 val21 = ClassHasher<MechanicEntity>.GetHash128(ManualTarget);
		result.Append(ref val21);
		SpellDescriptor val22 = StoryModeBuffImmunity;
		result.Append(ref val22);
		bool val23 = StoryModeEnergyDrainImmunity;
		result.Append(ref val23);
		bool val24 = StartedCombatNearEnemy;
		result.Append(ref val24);
		EntityRef<MechanicEntity> obj = m_LastTarget;
		Hash128 val25 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val25);
		EntityRef<ItemEntityWeapon> obj2 = m_LastAbilityWeapon;
		Hash128 val26 = StructHasher<EntityRef<ItemEntityWeapon>>.GetHash128(ref obj2);
		result.Append(ref val26);
		if (LastAttackPosition.HasValue)
		{
			Vector3 val27 = LastAttackPosition.Value;
			result.Append(ref val27);
		}
		return result;
	}
}
