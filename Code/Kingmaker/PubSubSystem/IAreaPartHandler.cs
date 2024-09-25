using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAreaPartHandler : ISubscriber
{
	void OnAreaPartChanged(BlueprintAreaPart previous);
}
