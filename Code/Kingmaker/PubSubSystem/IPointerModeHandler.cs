using Kingmaker.Controllers.Clicks;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPointerModeHandler : ISubscriber
{
	void OnPointerModeChanged(PointerMode oldMode, PointerMode newMode);
}
