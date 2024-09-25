using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenSelectItemHandler : ISubscriber
{
	void HandleSelectItem(FeatureGroup featureGroup, BlueprintFeature blueprintFeature);
}
