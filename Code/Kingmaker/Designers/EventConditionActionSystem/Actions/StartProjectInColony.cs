using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("9d042b002321e7345999f9d905501662")]
[PlayerUpgraderAllowed(false)]
public class StartProjectInColony : GameAction
{
	[ValidateNotNull]
	public BlueprintColonyProjectReference Project;

	public override string GetCaption()
	{
		return "Add project " + Project.Get().Name + " to current colony in context";
	}

	protected override void RunAction()
	{
		Colony colony = Game.Instance.Player.ColoniesState.ColonyContextData.Colony;
		if (colony != null)
		{
			if (!colony.ProjectCanStart(Project))
			{
				Element.LogError("Cannot add project to colony");
			}
			else
			{
				Game.Instance.GameCommandQueue.StartColonyProject(colony.Blueprint.ToReference<BlueprintColonyReference>(), Project);
			}
		}
	}
}
