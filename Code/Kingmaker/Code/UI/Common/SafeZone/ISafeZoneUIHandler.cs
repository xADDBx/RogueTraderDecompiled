using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.Common.SafeZone;

public interface ISafeZoneUIHandler : ISubscriber
{
	void OnSafeZoneChanged();
}
