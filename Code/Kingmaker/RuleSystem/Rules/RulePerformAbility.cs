using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformAbility : RulebookEvent
{
	[NotNull]
	public readonly AbilityData Spell;

	[NotNull]
	public readonly TargetWrapper SpellTarget;

	public readonly AbilityExecutionContext Context;

	public bool Success { get; private set; }

	[CanBeNull]
	public AbilityExecutionProcess Result { get; private set; }

	[CanBeNull]
	public MechanicsContext ExecutionActionContext { get; set; }

	public bool IsCutscene { get; set; }

	public bool IgnoreCooldown { get; set; }

	public bool ForceFreeAction { get; set; }

	public RulePerformAbility([NotNull] Ability spell, [NotNull] TargetWrapper target)
		: this(spell.Data, target)
	{
	}

	public RulePerformAbility([NotNull] AbilityData spell, [NotNull] TargetWrapper target)
		: base(spell.Caster)
	{
		Spell = spell;
		SpellTarget = target;
		Context = spell.CreateExecutionContext(target);
	}

	public override IMechanicEntity GetRuleTarget()
	{
		return SpellTarget.Entity;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Context.IsForced = IsCutscene || base.ConcreteInitiator.IsCheater;
		if (!Context.IsForced && !Spell.IsValid(SpellTarget, out var unavailabilityReason))
		{
			PFLog.Default.ErrorWithReport($"Invalid target {SpellTarget} for spell '{Spell.Blueprint}' because {unavailabilityReason}");
			return;
		}
		Context.ShadowFactorPercents = 0;
		Success = true;
		using (ContextData<AbilityData.IgnoreCooldown>.RequestIf(IgnoreCooldown))
		{
			using (ContextData<AbilityData.ForceFreeAction>.RequestIf(ForceFreeAction))
			{
				Result = Spell.Cast(Context);
			}
		}
	}
}
