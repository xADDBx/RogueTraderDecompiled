using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[ComponentName("Clockwork/Conditional Command List")]
[AllowedOn(typeof(BlueprintClockworkScenario))]
[AllowMultipleComponents]
[TypeId("4c915c5106886254db42a38b6538bfda")]
public class ConditionalCommandList : BlueprintComponent
{
	[SerializeReference]
	public Condition Condition;

	public ClockworkCommandList CommandList;
}
