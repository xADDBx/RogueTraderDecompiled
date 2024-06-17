using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IScreenUIHandler : ISubscriber
{
	public enum ScreenType
	{
		None,
		VendorSelecting,
		Transition
	}

	void CloseScreen(ScreenType screen);
}
