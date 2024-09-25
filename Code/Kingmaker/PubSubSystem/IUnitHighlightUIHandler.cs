using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.PubSubSystem;

public interface IUnitHighlightUIHandler : ISubscriber
{
	void HandleHighlightChange([NotNull] AbstractUnitEntityView unit);
}
