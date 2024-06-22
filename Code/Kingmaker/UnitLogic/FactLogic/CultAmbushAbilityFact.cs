using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("bee5cf278ed04bff8dad8c8b34ccd050")]
public class CultAmbushAbilityFact : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, IUnitNodeChangedHandler, ISubscriber<IBaseUnitEntity>, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, IHashable
{
	[SerializeField]
	private int m_VisibilityDistanceInNode;

	private bool m_IsCombatPreparation;

	protected override void OnActivate()
	{
		base.OnActivate();
		base.Owner.GetOrCreate<UnitPartCultAmbush>()?.Use((base.Fact as Feature)?.Blueprint);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitPartCultAmbush>();
		base.OnDeactivate();
	}

	void IRulebookHandler<RulePerformAbility>.OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	void IRulebookHandler<RulePerformAbility>.OnEventDidTrigger(RulePerformAbility evt)
	{
		if (evt != null && base.Owner.TryGetUnitPartCultAmbush(out var ambush) && !ambush.IsAllVisibility)
		{
			ambush.Use(evt.Spell.Blueprint, evt.Spell.Weapon != null);
		}
	}

	void IUnitNodeChangedHandler.HandleUnitNodeChanged(GraphNode oldNode)
	{
		if (!m_IsCombatPreparation)
		{
			BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
			ChangeUnitPosition(baseUnitEntity);
		}
	}

	private void ChangeUnitPosition(BaseUnitEntity unit)
	{
		if (unit == null || !base.Owner.TryGetUnitPartCultAmbush(out var ambush) || ambush.IsAllVisibility)
		{
			return;
		}
		if (unit == base.Owner)
		{
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if (!partyAndPet.IsDeadOrUnconscious && !partyAndPet.IsDetached)
				{
					int num = WarhammerGeometryUtils.DistanceToInCells(partyAndPet.Position, partyAndPet.SizeRect, base.Owner.Position, base.Owner.SizeRect);
					if (m_VisibilityDistanceInNode >= num)
					{
						ambush.MarkAllAsVisibility();
						break;
					}
				}
			}
			return;
		}
		if (unit.IsInPlayerParty)
		{
			int num2 = WarhammerGeometryUtils.DistanceToInCells(unit.Position, unit.SizeRect, base.Owner.Position, base.Owner.SizeRect);
			if (m_VisibilityDistanceInNode >= num2)
			{
				ambush.MarkAllAsVisibility();
			}
		}
	}

	void IPreparationTurnBeginHandler.HandleBeginPreparationTurn(bool canDeploy)
	{
		if (!m_IsCombatPreparation)
		{
			m_IsCombatPreparation = true;
		}
	}

	void IPreparationTurnEndHandler.HandleEndPreparationTurn()
	{
		if (m_IsCombatPreparation)
		{
			ChangeUnitPosition(base.Owner);
			m_IsCombatPreparation = false;
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
