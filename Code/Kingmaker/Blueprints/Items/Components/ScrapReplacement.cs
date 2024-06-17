using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("0926042f5249455a8b09cf6a241c5b2b")]
public class ScrapReplacement : BlueprintComponent
{
	public int Cost = 1;
}
