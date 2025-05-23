using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.CharGen;

namespace Kingmaker.PubSubSystem;

public interface IChangeAppearanceHandler : ISubscriber
{
	void HandleShowChangeAppearance(CharGenConfig config);
}
