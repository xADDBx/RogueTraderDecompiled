using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Progression;

public interface IFeaturePrerequisite
{
	bool Meet(IBaseUnitEntity unit);
}
