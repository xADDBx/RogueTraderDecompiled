using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IChangeChapterHandler : ISubscriber
{
	void HandleChangeChapter();
}
