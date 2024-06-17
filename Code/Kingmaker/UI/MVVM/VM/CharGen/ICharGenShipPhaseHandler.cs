using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenShipPhaseHandler : ISubscriber
{
	void HandleSetShip([NotNull] BlueprintStarship blueprintStarship);

	void HandleSetName([NotNull] string name);
}
