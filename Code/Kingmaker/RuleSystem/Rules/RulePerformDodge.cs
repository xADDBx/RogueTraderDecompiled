using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Covers;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformDodge : RulebookTargetEvent<UnitEntity, MechanicEntity>
{
	public class DelayedDodge : ContextData<DelayedDodge>
	{
		public readonly List<(UnitEntity Unit, RulePerformDodge DodgeRule)> DodgedUnits = new List<(UnitEntity, RulePerformDodge)>();

		protected override void Reset()
		{
			DodgedUnits.Clear();
		}
	}

	public AbilityData Ability { get; }

	public LosCalculations.CoverType CoverType { get; }

	public HashSet<CustomGridNodeBase> DangerArea { get; }

	public int BurstIndex { get; }

	public RuleCalculateDodgeChance ChancesRule { get; }

	public RuleRollChance ResultChanceRule { get; private set; }

	public bool Result { get; private set; }

	public bool IsMelee => Ability.Weapon?.Blueprint.IsMelee ?? false;

	public bool IsRanged => Ability.Weapon?.Blueprint.IsRanged ?? false;

	public bool IsJumpAside { get; private set; }

	public MechanicEntity Attacker => base.Target;

	public UnitEntity Defender => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public new NotImplementedException Target
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public RulePerformDodge([NotNull] UnitEntity defender, [NotNull] MechanicEntity attacker, [NotNull] AbilityData ability, LosCalculations.CoverType coverType, HashSet<CustomGridNodeBase> dangerArea, int burstIndex)
		: base(defender, attacker)
	{
		Ability = ability;
		CoverType = coverType;
		DangerArea = dangerArea;
		BurstIndex = burstIndex;
		ChancesRule = new RuleCalculateDodgeChance(defender, attacker, Ability, CoverType, BurstIndex);
		IsJumpAside = false;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(ChancesRule);
		ResultChanceRule = Rulebook.Trigger(new RuleRollChance(Defender, ChancesRule.Result, RollType.Dodge, RollChanceType.Untyped, null, Attacker));
		Result = ResultChanceRule.Success;
		if (!Result || !UnitPartJumpAsideDodge.NeedStepAsideDodge(Defender, this))
		{
			return;
		}
		if (UnitPartJumpAsideDodge.ShouldDodge(Defender, this) && UnitPartJumpAsideDodge.CanDodge(Defender, this, out var safePath, out var pathCost))
		{
			List<(UnitEntity, RulePerformDodge)> list = ContextData<DelayedDodge>.Current?.DodgedUnits;
			if (list != null)
			{
				list.Add((Defender, this));
			}
			else
			{
				Defender.GetOrCreate<UnitPartJumpAsideDodge>().Dodge(safePath, pathCost);
			}
			IsJumpAside = true;
		}
		else
		{
			Result = false;
		}
	}
}
