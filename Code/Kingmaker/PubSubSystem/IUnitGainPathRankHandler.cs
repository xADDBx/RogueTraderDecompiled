using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.PubSubSystem;

public interface IUnitGainPathRankHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitGainPathRank(BlueprintPath path);
}
public interface IUnitGainPathRankHandler<TTag> : IUnitGainPathRankHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitGainPathRankHandler, TTag>
{
}
