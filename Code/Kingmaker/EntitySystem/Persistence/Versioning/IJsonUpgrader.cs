using Newtonsoft.Json.Linq;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public interface IJsonUpgrader
{
	bool NeedsPlayerPriorityLoad { get; }

	bool WillUpgrade(string jsonName);

	void Upgrade(JObject root);
}
