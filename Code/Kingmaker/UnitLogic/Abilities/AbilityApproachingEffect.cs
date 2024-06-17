using JetBrains.Annotations;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities;

public class AbilityApproachingEffect
{
	[NotNull]
	public readonly AbilityDeliveryTarget EffectTarget;

	[NotNull]
	public readonly AbilityDeliveryTarget DeliveryTarget;

	[CanBeNull]
	public readonly AbilityApplyEffect ApplyEffect;

	public readonly float ApproachSpeed;

	private float m_PassedDistance;

	public bool FinishedApproaching { get; private set; }

	public AbilityApproachingEffect([NotNull] AbilityDeliveryTarget effectTarget, [NotNull] AbilityDeliveryTarget deliveryTarget, [CanBeNull] AbilityApplyEffect applyEffect, float approachSpeedMps)
	{
		EffectTarget = effectTarget;
		DeliveryTarget = deliveryTarget;
		ApplyEffect = applyEffect;
		ApproachSpeed = approachSpeedMps;
	}

	public void TickApproaching()
	{
		m_PassedDistance += ApproachSpeed * Game.Instance.TimeController.DeltaTime;
		float magnitude = (EffectTarget.Target.Point - DeliveryTarget.Target.Point).magnitude;
		if (m_PassedDistance >= magnitude)
		{
			FinishedApproaching = true;
		}
	}
}
