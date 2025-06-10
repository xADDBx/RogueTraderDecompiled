namespace Kingmaker.View.MapObjects.InteractionComponentBase;

public interface IHideUnusedSettings
{
	bool ShouldShowUIType { get; }

	bool ShouldShowNotInCombat { get; }

	bool ShouldShowShowOvertip { get; }

	bool ShouldShowAlwaysDisabled { get; }

	bool ShouldShowProximityRadius { get; }

	bool ShouldShowType { get; }

	bool ShouldShowUseAnimationState { get; }

	bool ShouldShowDialog { get; }

	bool ShouldShowInteractionSound { get; }

	bool ShouldShowInteractionDisabledSound { get; }

	bool ShouldShowInteractionStopSound { get; }

	bool ShouldShowTrap { get; }

	bool ShouldShowDoNotNeedCollider { get; }

	bool ShouldShowUnlimitedInteractionsPerRound { get; }

	bool ShouldShowOverrideActionPointsCost { get; }

	bool ShouldShowInteractWithMeltaChargeFXData { get; }

	bool ShouldShowOvertipVerticalCorrection { get; }
}
