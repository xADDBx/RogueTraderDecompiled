using System.Collections.Generic;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Spawners;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

public class SpawnerInteractionPart : ViewBasedPart, IUnitInitializer, IHashable
{
	public class Wrapper : IUnitInteraction
	{
		public SpawnerInteraction Source;

		public int Distance
		{
			get
			{
				if (Source.OverrideDistance != 0)
				{
					return Source.OverrideDistance;
				}
				return 2;
			}
		}

		public bool IsApproach => Source.TriggerOnApproach;

		public float ApproachCooldown => Source.Cooldown;

		public bool MainPlayerPreferred => true;

		public virtual bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
		{
			PartUnitState stateOptional = target.GetStateOptional();
			if (stateOptional != null && stateOptional.IsHelpless)
			{
				return false;
			}
			if (Source.TriggerOnApproach && Source.TriggerOnParty)
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
			if (Source.Conditions.Get() == null || !Source.Conditions.Get().Conditions.HasConditions)
			{
				return true;
			}
			using (ContextData<InteractingUnitData>.Request().Setup(initiator))
			{
				using (ContextData<ClickedUnitData>.Request().Setup(target))
				{
					return Source.Conditions.Get().Conditions.Check();
				}
			}
		}

		public AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
		{
			return Source.Interact(user, target);
		}
	}

	private readonly List<Wrapper> m_Components = new List<Wrapper>();

	public override bool ShouldCheckSourceComponent => false;

	public override void SetSource(IAbstractEntityPartComponent source)
	{
		base.SetSource(source);
		m_Components.Add(new Wrapper
		{
			Source = (SpawnerInteraction)source
		});
	}

	public void OnSpawn(AbstractUnitEntity unit)
	{
	}

	public void OnInitialize(AbstractUnitEntity unit)
	{
		UnitPartInteractions orCreate = unit.GetOrCreate<UnitPartInteractions>();
		foreach (Wrapper component in m_Components)
		{
			orCreate.AddInteraction(component);
		}
	}

	public void OnDispose(AbstractUnitEntity unit)
	{
		UnitPartInteractions orCreate = unit.GetOrCreate<UnitPartInteractions>();
		foreach (Wrapper component in m_Components)
		{
			orCreate.RemoveInteraction(component);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
