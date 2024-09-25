using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("62dba2a910a946a7b8092daa3e399e8a")]
[PlayerUpgraderAllowed(false)]
public class RemoveColonyResources : GameAction
{
	[NotNull]
	public ResourceData[] Resources;

	public override string GetCaption()
	{
		return "Remove resources from pool";
	}

	protected override void RunAction()
	{
		ResourceData[] resources = Resources;
		foreach (ResourceData resourceData in resources)
		{
			Game.Instance.ColonizationController.UseResourceFromPool(resourceData.Resource.Get(), resourceData.Count);
		}
	}
}
