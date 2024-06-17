using System;
using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateCanApplyBuff : RulebookEvent
{
	public readonly MechanicsContext Context;

	public BlueprintBuff Blueprint => AppliedBuff.Blueprint;

	public BuffDuration Duration { get; private set; }

	public bool CanApply { get; set; } = true;


	public bool IgnoreImmunity { get; set; }

	public bool Immunity { get; set; }

	public Buff AppliedBuff { get; }

	public RuleCalculateCanApplyBuff([NotNull] MechanicEntity initiator, MechanicsContext context, Buff buff)
		: base(initiator)
	{
		AppliedBuff = buff;
		Context = context;
		Duration = new BuffDuration(buff);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!CanApply)
		{
			return;
		}
		if (Context.MaybeCaster != null && Context.MaybeCaster != base.Initiator && Context.MaybeCaster.IsAttackingGreenNPC((MechanicEntity)base.Initiator) && Context.SpellDescriptor.IsNegativeEffect())
		{
			CanApply = false;
			return;
		}
		if (this.SkipBecauseOfShadow())
		{
			CanApply = false;
			return;
		}
		if (Immunity && !IgnoreImmunity)
		{
			CanApply = false;
			return;
		}
		if (base.Initiator.IsPlayerFaction && Blueprint.IsHardCrowdControl)
		{
			HardCrowdControlDurationLimit value = SettingsRoot.Difficulty.HardCrowdControlOnPartyMaxDurationRounds.GetValue();
			if (value != HardCrowdControlDurationLimit.Unlimited)
			{
				Rounds rounds = value.ToRounds();
				Duration = (Duration.Rounds.HasValue ? new Rounds(Math.Min(Duration.Rounds.Value.Value, rounds.Value)) : rounds);
			}
		}
		CanApply &= !Blueprint.RemoveOnResurrect || !base.ConcreteInitiator.Features.Immortality;
	}
}
