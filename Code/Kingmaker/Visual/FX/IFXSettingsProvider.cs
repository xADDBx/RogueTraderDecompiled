using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.Enums.Sound;

namespace Kingmaker.Visual.FX;

public interface IFXSettingsProvider
{
	[CanBeNull]
	IEnumerable<IFXSettings> GetFXs(MappedAnimationEventType eventType);

	[CanBeNull]
	IEnumerable<IFXSettings> GetFXs(AbilityEventType eventType);
}
