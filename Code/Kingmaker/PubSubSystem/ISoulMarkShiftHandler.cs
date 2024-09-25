using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface ISoulMarkShiftHandler : ISubscriber
{
	void HandleSoulMarkShift(ISoulMarkShiftProvider provider);
}
