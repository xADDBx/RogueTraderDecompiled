using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenAttributesPhaseHandler : ISubscriber
{
	void HandleTryAdvanceStat(StatType statType, bool advance);
}
