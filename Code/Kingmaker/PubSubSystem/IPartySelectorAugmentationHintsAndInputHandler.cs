using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UniRx;

namespace Kingmaker.PubSubSystem;

public interface IPartySelectorAugmentationHintsAndInputHandler : ISubscriber
{
	void CreateInputImpl(InputLayer inputLayer, ReactiveProperty<bool> enable);

	void DisposeInputImpl();
}
