using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.PubSubSystem;

public interface IUnitDirectHoverUIHandler : ISubscriber
{
	void HandleHoverChange([NotNull] AbstractUnitEntityView unitEntityView, bool isHover);
}
