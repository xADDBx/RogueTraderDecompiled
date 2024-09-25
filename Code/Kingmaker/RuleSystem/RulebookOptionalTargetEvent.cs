using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem;

public abstract class RulebookOptionalTargetEvent : RulebookEvent
{
	[CanBeNull]
	public readonly MechanicEntity MaybeTarget;

	public bool FakeRule { get; set; }

	public bool HasNoTarget { get; set; }

	public override IMechanicEntity GetRuleTarget()
	{
		return MaybeTarget;
	}

	protected RulebookOptionalTargetEvent([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target)
		: base(initiator)
	{
		MaybeTarget = target;
	}
}
public abstract class RulebookOptionalTargetEvent<TTarget> : RulebookOptionalTargetEvent where TTarget : MechanicEntity
{
	[CanBeNull]
	public new TTarget MaybeTarget => (TTarget)base.MaybeTarget;

	protected RulebookOptionalTargetEvent([NotNull] MechanicEntity initiator, [CanBeNull] TTarget target)
		: base(initiator, target)
	{
	}
}
public abstract class RulebookOptionalTargetEvent<TInitiator, TTarget> : RulebookOptionalTargetEvent where TInitiator : MechanicEntity where TTarget : MechanicEntity
{
	[NotNull]
	public new TInitiator Initiator => (TInitiator)base.Initiator;

	[CanBeNull]
	public new TTarget MaybeTarget => (TTarget)base.MaybeTarget;

	protected RulebookOptionalTargetEvent([NotNull] TInitiator initiator, [CanBeNull] TTarget target)
		: base(initiator, target)
	{
	}
}
