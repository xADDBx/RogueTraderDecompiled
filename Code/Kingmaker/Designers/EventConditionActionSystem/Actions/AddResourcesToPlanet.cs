using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SystemMap;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("92acbbc7e734d4140adb8d6b54cbd58b")]
[PlayerUpgraderAllowed(true)]
public class AddResourcesToPlanet : GameAction
{
	[NotNull]
	public ResourceData[] Resources;

	[NotNull]
	public BlueprintStarSystemObjectReference StarSystemObject;

	public override string GetCaption()
	{
		return "Add resources to planet";
	}

	protected override void RunAction()
	{
		StarSystemObjectEntity starSystemObjectEntity = Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity sso) => sso.Blueprint == StarSystemObject.Get());
		if (starSystemObjectEntity != null)
		{
			ResourceData[] resources = Resources;
			foreach (ResourceData resourceData in resources)
			{
				starSystemObjectEntity.AddResource(resourceData.Resource.Get(), resourceData.Count);
			}
		}
	}
}
