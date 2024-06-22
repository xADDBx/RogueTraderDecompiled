using Kingmaker.Enums;
using Kingmaker.Enums.Sound;

namespace Kingmaker.Visual.FX;

public interface IFXSettings
{
	MappedAnimationEventType? AnimationEvent { get; }

	AbilityEventType? AbilityEvent { get; }

	FXTarget Target { get; }

	bool OverrideTargetOrientationSource { get; }

	FXSettings Settings { get; }
}
