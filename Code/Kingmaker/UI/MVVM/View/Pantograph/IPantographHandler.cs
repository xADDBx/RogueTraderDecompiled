using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.View.Pantograph;

public interface IPantographHandler : ISubscriber
{
	void Bind(PantographConfig config);

	void Unbind();

	void SetFocus(bool focused);
}
