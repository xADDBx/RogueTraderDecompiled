using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintCampaign))]
[AllowMultipleComponents]
[TypeId("fa4fb1db7ec84012a343068be067fd35")]
public abstract class BlueprintCampaignOverrideSetting : BlueprintComponent
{
	public virtual void Activate()
	{
	}

	public virtual void Deactivate()
	{
	}
}
