using Kingmaker.AreaLogic.Etudes;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEtudeCounterHandler : ISubscriber
{
	void ShowEtudeCounter(EtudeShowCounterUIStruct counterUIStruct);

	void HideEtudeCounter(string id);
}
