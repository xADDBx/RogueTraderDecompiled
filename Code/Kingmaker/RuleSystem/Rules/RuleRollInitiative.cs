using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollInitiative : RulebookEvent
{
	public readonly ValueModifiersManager Modifiers = new ValueModifiersManager();

	private readonly int? m_OverrideResult;

	public float Result { get; private set; }

	public RuleRollD10 ResultD10 { get; private set; }

	public int Modifier => Modifiers.Value;

	public bool IsOverriden => m_OverrideResult.HasValue;

	public RuleRollInitiative(MechanicEntity initiator, int? overrideResult = null)
		: base(initiator)
	{
		m_OverrideResult = overrideResult;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		ResultD10 = Dice.D10;
		if (base.Initiator is StarshipEntity)
		{
			int num = (base.ConcreteInitiator.Blueprint as BlueprintStarship)?.Initiative ?? 0;
			Result = Math.Max((int)ResultD10 - 1 + Modifiers.Value + num, 1);
			return;
		}
		int num2 = base.ConcreteInitiator.GetAttributeOptional(StatType.WarhammerAgility)?.WarhammerBonus ?? 0;
		if (num2 > 0)
		{
			Modifiers.Add(num2, this, StatType.WarhammerAgility);
		}
		int num3 = base.ConcreteInitiator.GetAttributeOptional(StatType.WarhammerPerception)?.WarhammerBonus ?? 0;
		if (num3 > 0)
		{
			Modifiers.Add(num3 / 2, this, StatType.WarhammerPerception);
		}
		Result = ((float?)m_OverrideResult) ?? Math.Max((float)((int)ResultD10 + Modifiers.Value) + (float)(num2 + num3 / 2) / 100f, 1f);
	}
}
