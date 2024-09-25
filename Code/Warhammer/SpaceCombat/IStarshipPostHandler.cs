using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Warhammer.SpaceCombat;

public interface IStarshipPostHandler : ISubscriber
{
	void HandlePostBlocked(Post post);

	void HandleBuffDidAdded(Post post, Buff buff);

	void HandleBuffDidRemoved(Post post, Buff buff);
}
