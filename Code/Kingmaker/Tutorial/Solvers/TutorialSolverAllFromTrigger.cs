using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Solvers;

[ClassInfoBox("Always successful. All arguments should be gathered by trigger.")]
[TypeId("5a73840501bdbaa408d63dfe55a931dd")]
public class TutorialSolverAllFromTrigger : TutorialSolver
{
	public override bool Solve(TutorialContext context)
	{
		return true;
	}
}
