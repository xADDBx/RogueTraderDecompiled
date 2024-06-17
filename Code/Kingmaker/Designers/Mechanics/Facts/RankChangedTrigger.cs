using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("1e5d3dccdc3e487e94d2a5ba37a598e3")]
public class RankChangedTrigger : UnitFactComponentDelegate, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public int PrevRankValue;
	}

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ContextValue m_RankValue = 1;

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_Facts;

	public ContextValue RankValue => m_RankValue;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		Update();
	}

	protected override void OnFactDetached()
	{
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			base.Owner.Facts.Remove(fact);
		}
	}

	private void Update()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		int rank = base.Fact.GetRank();
		int prevRankValue = componentData.PrevRankValue;
		if (rank == prevRankValue)
		{
			return;
		}
		componentData.PrevRankValue = rank;
		int num = m_RankValue.Calculate(base.Context);
		if (rank > prevRankValue && rank >= num && m_Restrictions.IsPassed((MechanicEntityFact)base.Fact, (RulebookEvent)null, (AbilityData)null))
		{
			foreach (EntityFact item in TryAddFacts())
			{
				item.AddSource(base.Fact, this);
			}
			return;
		}
		if (rank < num)
		{
			TryRemoveFacts();
		}
	}

	private void TryRemoveFacts()
	{
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			if (base.Owner.Facts.Contains(fact))
			{
				base.Owner.Facts.Remove(fact);
			}
		}
	}

	private List<EntityFact> TryAddFacts()
	{
		List<EntityFact> list = TempList.Get<EntityFact>();
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			if (!base.Owner.Facts.Contains(fact))
			{
				EntityFact entityFact = base.Owner.AddFact(fact);
				if (entityFact == null)
				{
					break;
				}
				list.Add(entityFact);
			}
		}
		return list;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
