using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IWeaponSetChangedTooltipUpdateHandler : ISubscriber
{
	void OnWeaponChangeTooltipUpdate();
}
