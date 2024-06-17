using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype.PsychicPowers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
[TypeId("5e6356b32b3149b68d77a844fb2cbab5")]
public class EtudeBracketOverrideAreaVeil : EtudeBracketTrigger, IGlobalRulebookHandler<RuleCalculateVeilCount>, IRulebookHandler<RuleCalculateVeilCount>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public int Value;

	protected override void OnEnter()
	{
		VeilThicknessCounter veilThicknessCounter = Game.Instance.TurnController.VeilThicknessCounter;
		if (Value > veilThicknessCounter.Value)
		{
			veilThicknessCounter.Value = Value;
		}
	}

	protected override void OnExit()
	{
		Game.Instance.TurnController.VeilThicknessCounter.ResetValueOutOfCombat();
	}

	public void OnEventAboutToTrigger(RuleCalculateVeilCount evt)
	{
		evt.StartVeilModifiers.Add(Value, base.Fact, ModifierDescriptor.UntypedUnstackable);
	}

	public void OnEventDidTrigger(RuleCalculateVeilCount evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
