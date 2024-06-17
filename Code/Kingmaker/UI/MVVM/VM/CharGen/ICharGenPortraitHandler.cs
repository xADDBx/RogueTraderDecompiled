using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenPortraitHandler : ISubscriber
{
	void HandleSetPortrait([NotNull] BlueprintPortrait blueprintPortrait);
}
