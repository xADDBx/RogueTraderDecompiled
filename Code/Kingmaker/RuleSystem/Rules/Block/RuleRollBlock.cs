using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules.Block;

public class RuleRollBlock : RulebookTargetEvent<UnitEntity, MechanicEntity>
{
	public RuleCalculateBlockChance ChancesRule { get; }

	public RuleRollChance RollChanceRule { get; private set; }

	public bool Result { get; private set; }

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

	public RuleRollBlock([NotNull] UnitEntity initiator, int blockChance, MechanicEntity target, [NotNull] AbilityData ability, bool isAutoBlock)
		: base(initiator, target)
	{
		ChancesRule = new RuleCalculateBlockChance(initiator, blockChance, target, ability, isAutoBlock);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(ChancesRule);
		RollChanceRule = Rulebook.Trigger(new RuleRollChance(Defender, ChancesRule.Result, null, RollChanceType.Untyped, null, Attacker));
		Result = RollChanceRule.Success;
	}
}
