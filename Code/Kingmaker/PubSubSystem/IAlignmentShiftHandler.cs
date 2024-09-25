using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface IAlignmentShiftHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAligmentShift(AlignmentShiftDirection direction, IAlignmentShiftProvider source);
}
