using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Sound;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenAppearancePhaseVoiceHandler : ISubscriber
{
	void HandleChangeVoice([NotNull] BlueprintUnitAsksList blueprint);
}
