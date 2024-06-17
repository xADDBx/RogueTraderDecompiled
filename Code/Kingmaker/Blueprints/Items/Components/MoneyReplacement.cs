using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints.Items.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("d6f0c3aef90114545b6ae50863a93075")]
public class MoneyReplacement : BlueprintComponent
{
	public long Cost = 1L;
}
