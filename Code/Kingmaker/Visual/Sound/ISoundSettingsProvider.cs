using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Enums;

namespace Kingmaker.Visual.Sound;

public interface ISoundSettingsProvider
{
	[CanBeNull]
	IEnumerable<ISoundSettings> GetSounds(UnitSoundAnimationEventType eventType);

	[CanBeNull]
	IEnumerable<ISoundSettings> GetSounds(AbilityEventType soundEventType);
}
