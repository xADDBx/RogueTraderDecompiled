using Kingmaker.Enums;
using Kingmaker.Enums.Sound;

namespace Kingmaker.Visual.FX;

public interface IFXSettings
{
	MappedAnimationEventType? AnimationEvent { get; }

	AbilityEventType? AbilityEvent { get; }

	FXTarget Target { get; }

	bool OverrideTargetOrientationSource { get; }

	bool OrientationFromCasterToTarget { get; }

	OrientationSnapMode OrientationSnap { get; }

	FXSettings Settings { get; }
}
