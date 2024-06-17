using Kingmaker.Enums;
using Kingmaker.Enums.Sound;

namespace Kingmaker.Visual.FX;

public interface IFXSettings
{
	MappedAnimationEventType? AnimationEvent { get; }

	AbilityEventType? AbilityEvent { get; }

	FXTarget Target { get; }

	FXSettings Settings { get; }
}
