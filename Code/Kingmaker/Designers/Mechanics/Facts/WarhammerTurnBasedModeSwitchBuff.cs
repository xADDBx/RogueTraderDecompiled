using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("ecd6ebb641dd01a45b44d8a85bf23691")]
public class WarhammerTurnBasedModeSwitchBuff : MechanicEntityFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_BuffAtTBStart;

	[SerializeField]
	private BlueprintBuffReference m_BuffAtTBEnd;

	public ContextDurationValue DurationValue;

	public BlueprintBuff BuffAtTBStart => m_BuffAtTBStart?.Get();

	public BlueprintBuff BuffAtTBEnd => m_BuffAtTBEnd?.Get();

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TimeSpan seconds = DurationValue.Calculate(base.Context).Seconds;
		if (isTurnBased)
		{
			base.Owner.Buffs.Add(BuffAtTBStart, base.Owner, seconds);
		}
		else
		{
			base.Owner.Buffs.Add(BuffAtTBEnd, base.Owner);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
