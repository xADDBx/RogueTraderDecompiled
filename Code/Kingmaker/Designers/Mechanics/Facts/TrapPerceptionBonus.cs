using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.Traps;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Buff on spawned unit")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("3602d4095f735894e826fc3c1615a580")]
public class TrapPerceptionBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ModifierDescriptor Descriptor;

	public ContextValue Value;

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
		if (evt.Reason.SourceEntity is TrapObjectData)
		{
			evt.DifficultyModifiers.Add(Value.Calculate(base.Context), base.Fact, Descriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
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
