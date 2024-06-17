using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAlignmentChangeHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAlignmentChange(Alignment newAlignment, Alignment prevAlignment);
}
