using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("8efa69193b5243b388e9ee4f6e8cc41e")]
[PlayerUpgraderAllowed(false)]
public class GainColonyResources : GameAction
{
	[NotNull]
	public ResourceData[] Resources;

	public override string GetCaption()
	{
		return "Add resources to pool";
	}

	protected override void RunAction()
	{
		ResourceData[] resources = Resources;
		foreach (ResourceData resourceData in resources)
		{
			Game.Instance.ColonizationController.AddResourceNotFromColonyToPool(resourceData.Resource, resourceData.Count);
		}
	}
}
