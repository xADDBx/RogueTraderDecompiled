using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPartyLeaveAreaHandler : ISubscriber
{
	void HandlePartyLeaveArea(BlueprintArea currentArea, BlueprintAreaEnterPoint targetArea);
}
