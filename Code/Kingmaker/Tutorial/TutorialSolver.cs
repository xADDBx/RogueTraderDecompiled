using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Tutorial;

[AllowedOn(typeof(BlueprintTutorial))]
[TypeId("46f354bd82584df4b28b584ffdd55ed5")]
public abstract class TutorialSolver : BlueprintComponent
{
	public abstract bool Solve(TutorialContext context);
}
