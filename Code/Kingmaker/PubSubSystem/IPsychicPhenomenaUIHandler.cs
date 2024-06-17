using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPsychicPhenomenaUIHandler : ISubscriber
{
	void HandleVeilThicknessValueChanged(int delta, int value);
}
