using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitGetAbilityJump : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitJumpFinished();
}
public interface IUnitGetAbilityJump<TTag> : IUnitGetAbilityJump, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitGetAbilityJump, TTag>
{
}
