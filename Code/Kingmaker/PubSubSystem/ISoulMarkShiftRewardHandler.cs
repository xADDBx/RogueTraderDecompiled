using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface ISoulMarkShiftRewardHandler : ISubscriber
{
	void HandleSoulMarkShift(SoulMarkShift shift);
}
