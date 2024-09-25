using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UI.Models.LevelUp;

public class DefaultBuildData : ContextData<DefaultBuildData>
{
	public BlueprintRace Race { get; private set; }

	public DefaultBuildData Setup([NotNull] BlueprintRace race)
	{
		Race = race;
		return this;
	}

	protected override void Reset()
	{
		Race = null;
	}
}
