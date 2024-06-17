using Kingmaker.Enums;
using Kingmaker.Visual.FX;

namespace Kingmaker.Visual.Sound;

public interface ISoundSettings
{
	UnitSoundAnimationEventType? AnimationEvent { get; }

	AbilityEventType? AbilityEvent { get; }

	FXTarget Target { get; }

	SoundSettings Settings { get; }
}
