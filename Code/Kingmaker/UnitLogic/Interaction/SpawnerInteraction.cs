using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[RequireComponent(typeof(UnitSpawnerBase))]
[KnowledgeDatabaseID("95d03dcfa566f5a4cadd0988cd234972")]
public abstract class SpawnerInteraction : EntityPartComponent<SpawnerInteractionPart>
{
	[JsonProperty]
	public int OverrideDistance;

	[JsonProperty]
	public ConditionsReference Conditions;

	[JsonProperty]
	public bool TriggerOnApproach;

	[JsonProperty]
	[ConditionalShow("TriggerOnApproach")]
	public bool TriggerOnParty = true;

	[JsonProperty]
	[ConditionalShow("TriggerOnApproach")]
	public float Cooldown = 5f;

	[JsonConstructor]
	protected SpawnerInteraction()
	{
	}

	public abstract AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
