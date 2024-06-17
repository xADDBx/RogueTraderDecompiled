using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISubtitleBarkHandler : ISubscriber
{
	void HandleOnShowBark(string text, float duration);

	void HandleOnHideBark();
}
