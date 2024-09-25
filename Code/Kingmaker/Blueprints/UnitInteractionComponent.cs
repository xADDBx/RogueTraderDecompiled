using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("e090ed34b87f41e4e826b4d6dff90462")]
public abstract class UnitInteractionComponent : BlueprintComponent, IUnitInteraction
{
	[JsonProperty]
	[SerializeField]
	private int m_OverrideDistance;

	[JsonProperty]
	public ConditionsChecker Conditions;

	[JsonProperty]
	public bool TriggerOnApproach;

	[JsonProperty]
	[ShowIf("TriggerOnApproach")]
	public bool TriggerOnParty = true;

	[JsonProperty]
	[ShowIf("TriggerOnApproach")]
	public float Cooldown = 5f;

	public int Distance
	{
		get
		{
			if (m_OverrideDistance != 0)
			{
				return m_OverrideDistance;
			}
			return 2;
		}
	}

	public bool IsApproach => TriggerOnApproach;

	public float ApproachCooldown => Cooldown;

	public bool MainPlayerPreferred => true;

	[JsonConstructor]
	protected UnitInteractionComponent()
	{
	}

	public virtual bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		PartUnitState stateOptional = target.GetStateOptional();
		if (stateOptional != null && stateOptional.IsHelpless)
		{
			return false;
		}
		if (TriggerOnApproach && TriggerOnParty)
		{
			if (!initiator.IsDirectlyControllable)
			{
				return false;
			}
			if (initiator.IsPet)
			{
				return false;
			}
			if (initiator.GetOptional<UnitPartSummonedMonster>() != null)
			{
				return false;
			}
		}
		if (!Conditions.HasConditions)
		{
			return true;
		}
		using (ContextData<InteractingUnitData>.Request().Setup(initiator))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				return Conditions.Check();
			}
		}
	}

	public abstract AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
