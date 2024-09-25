using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEncyclopediaHandler : ISubscriber
{
	void HandleEncyclopediaPage(string pageKey);

	void HandleEncyclopediaPage(INode page);
}
