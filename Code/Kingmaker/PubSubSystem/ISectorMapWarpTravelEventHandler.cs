using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISectorMapWarpTravelEventHandler : ISubscriber
{
	void HandleStartEventInTheMiddleOfJump(BlueprintComponentReference<EtudeTriggerActionInWarpDelayed> etudeTrigger);
}
