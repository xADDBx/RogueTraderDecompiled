using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Tutorial.Solvers;

[TypeId("d33d52c1ebf36d744b35d6ce77efd8dc")]
public class TutorialSolverItemFromTrigger : TutorialSolver
{
	public override bool Solve(TutorialContext context)
	{
		return context.SolutionItem != null;
	}
}
