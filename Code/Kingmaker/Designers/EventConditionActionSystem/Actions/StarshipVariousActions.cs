using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/StarshipVariousActions")]
[AllowMultipleComponents]
[TypeId("01f2545a552d7a44aa449aae019d7388")]
public class StarshipVariousActions : ContextAction
{
	public enum ActionType
	{
		StartingDamage,
		ApplyBuffToPlayerEnemies,
		RemoveBuffFromPlayerEnemies,
		SetFactionUnitsAsOnlyLowPriorityToOwnerBrain,
		ReduceBuffStackDuration,
		ApplyBuffToPlayerStarship
	}

	public ActionType actionType;

	[SerializeField]
	[ShowIf("DemandsBuff")]
	private BlueprintBuffReference m_Buff;

	[ShowIf("DemandsDamageStats")]
	public int hpPctDmg;

	[ShowIf("DemandsDamageStats")]
	public int[] shieldsPctDmgMin = new int[4];

	[SerializeField]
	[ShowIf("DemandsFaction")]
	private BlueprintFactionReference m_Faction;

	[SerializeField]
	[ShowIf("DemandsIntValue")]
	private int Value;

	[SerializeField]
	[ShowIf("DemandsChances")]
	private int Chances;

	public bool DemandsBuff
	{
		get
		{
			if (actionType != ActionType.ApplyBuffToPlayerEnemies && actionType != ActionType.RemoveBuffFromPlayerEnemies && actionType != ActionType.ReduceBuffStackDuration)
			{
				return actionType == ActionType.ApplyBuffToPlayerStarship;
			}
			return true;
		}
	}

	public bool DemandsDamageStats => actionType == ActionType.StartingDamage;

	public bool DemandsFaction => actionType == ActionType.SetFactionUnitsAsOnlyLowPriorityToOwnerBrain;

	public bool DemandsIntValue => actionType == ActionType.ReduceBuffStackDuration;

	public bool DemandsChances => actionType == ActionType.ReduceBuffStackDuration;

	public BlueprintFaction Faction => m_Faction?.Get();

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetCaption()
	{
		return $"Perform action: [{actionType}]";
	}

	public override void RunAction()
	{
		switch (actionType)
		{
		case ActionType.StartingDamage:
			StartingDamage(base.Context.MaybeOwner as StarshipEntity);
			return;
		case ActionType.SetFactionUnitsAsOnlyLowPriorityToOwnerBrain:
			SetFactionUnitsAsOnlyLowPriorityToOwnerBrain(base.Context.MaybeOwner as StarshipEntity);
			return;
		case ActionType.ReduceBuffStackDuration:
			ReduceBuffStackDuration(base.Context.MaybeOwner as StarshipEntity);
			return;
		case ActionType.ApplyBuffToPlayerStarship:
			ApplyBuffToPlayerStarship();
			return;
		}
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		IEnumerable<AbstractUnitEntity> source = Game.Instance.State.AllAwakeUnits.Where((AbstractUnitEntity u) => u is StarshipEntity && u.IsEnemy(playerShip));
		source.ToList();
		switch (actionType)
		{
		case ActionType.ApplyBuffToPlayerEnemies:
		{
			BuffDuration buffDuration = new BuffDuration(null, BuffEndCondition.CombatEnd);
			source.ForEach(delegate(AbstractUnitEntity u)
			{
				u.Buffs.Add(Buff, base.Context, buffDuration);
			});
			break;
		}
		case ActionType.RemoveBuffFromPlayerEnemies:
			source.ForEach(delegate(AbstractUnitEntity u)
			{
				u.Buffs.Remove(Buff);
			});
			break;
		}
	}

	private void StartingDamage(StarshipEntity ship)
	{
		if (ship != null)
		{
			ship.Health.SetDamage(ship.Health.MaxHitPoints * hpPctDmg / 100);
			PartStarshipShields shields2 = ship.Shields;
			if (shields2 != null)
			{
				SetSector(shields2, StarshipHitLocation.Fore, 0);
				SetSector(shields2, StarshipHitLocation.Aft, 1);
				SetSector(shields2, StarshipHitLocation.Port, 2);
				SetSector(shields2, StarshipHitLocation.Starboard, 3);
			}
		}
		void SetSector(PartStarshipShields shields, StarshipHitLocation hitLocation, int index)
		{
			StarshipSectorShields shields3 = shields.GetShields(hitLocation);
			shields3.Damage = shields3.Max * shieldsPctDmgMin[index] / 100;
		}
	}

	private void SetFactionUnitsAsOnlyLowPriorityToOwnerBrain(StarshipEntity ship)
	{
		PartUnitBrain brain = ship?.GetOptional<PartUnitBrain>();
		if (brain != null)
		{
			brain.ClearCustomLowPriorityTargets();
			Game.Instance.State.AllAwakeUnits.Where((AbstractUnitEntity u) => u is StarshipEntity starshipEntity && starshipEntity.Faction.Blueprint == Faction && !u.LifeState.IsDead).ForEach(delegate(AbstractUnitEntity u)
			{
				brain.AddCustomLowPriorityTarget(u);
			});
		}
	}

	private void ReduceBuffStackDuration(StarshipEntity unit)
	{
		foreach (Buff item in unit.Buffs.RawFacts.Where((Buff bf) => bf.Blueprint == Buff))
		{
			if (PFStatefulRandom.SpaceCombat.Range(0, 100) < Chances)
			{
				item.IncreaseDuration(new Rounds(-Value));
			}
		}
	}

	private void ApplyBuffToPlayerStarship()
	{
		BuffDuration duration = new BuffDuration(null, BuffEndCondition.SpaceCombatExit);
		Game.Instance.Player.PlayerShip.Buffs.Add(Buff, duration);
	}
}
