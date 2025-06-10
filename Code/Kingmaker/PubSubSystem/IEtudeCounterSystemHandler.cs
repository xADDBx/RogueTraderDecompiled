using Kingmaker.AreaLogic.Etudes;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEtudeCounterSystemHandler : ISubscriber
{
	void ShowEtudeCounterSystem(EtudeUICounterSystemTypes type);

	void HideEtudeCounterSystem(EtudeUICounterSystemTypes type);
}
