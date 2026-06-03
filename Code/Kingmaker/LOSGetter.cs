using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker;

[TypeId("867e1250463a04545bcc77a9c0aeba60")]
internal class LOSGetter : PropertyGetter
{
	private enum EntityType
	{
		Anyone,
		Allies,
		Enemies
	}

	[SerializeField]
	private bool HasLos;

	[SerializeField]
	private EntityType Alignment;

	[SerializeField]
	private BlueprintUnitFactReference[] m_HasFacts = Array.Empty<BlueprintUnitFactReference>();

	[SerializeField]
	private BlueprintUnitFactReference[] m_DoesntHaveFacts = Array.Empty<BlueprintUnitFactReference>();

	public ReferenceArrayProxy<BlueprintUnitFact> HasFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] hasFacts = m_HasFacts;
			return hasFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> DoesntHaveFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] doesntHaveFacts = m_DoesntHaveFacts;
			return doesntHaveFacts;
		}
	}

	protected override int GetBaseValue()
	{
		foreach (BaseUnitEntity item in base.CurrentEntity.GetVisionOptional().CanBeInRange)
		{
			if (item != base.CurrentEntity && UnitMatchesAlignment(item) && item.IsInCombat && item.IsVisibleForPlayer && item.IsConscious && !HasRestrictedFacts(item) && !LacksRequiredFacts(item) && base.CurrentEntity.HasLOS(item) == HasLos)
			{
				return 1;
			}
		}
		return 0;
	}

	private bool UnitMatchesAlignment(MechanicEntity unit)
	{
		return Alignment switch
		{
			EntityType.Anyone => true, 
			EntityType.Allies => unit.IsAlly(base.CurrentEntity), 
			EntityType.Enemies => unit.IsEnemy(base.CurrentEntity), 
			_ => true, 
		};
	}

	private bool HasRestrictedFacts(MechanicEntity unit)
	{
		foreach (BlueprintUnitFact doesntHaveFact in DoesntHaveFacts)
		{
			if (unit.Facts.Contains(doesntHaveFact))
			{
				return true;
			}
		}
		return false;
	}

	private bool LacksRequiredFacts(MechanicEntity unit)
	{
		foreach (BlueprintUnitFact hasFact in HasFacts)
		{
			if (!unit.Facts.Contains(hasFact))
			{
				return true;
			}
		}
		return false;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = string.Format("{0} LoS to {1}", HasLos ? "Has" : "Has no", Alignment);
		if (m_HasFacts.Length != 0)
		{
			text += $" with {m_HasFacts.Length} facts";
		}
		if (m_DoesntHaveFacts.Length != 0)
		{
			text += $" without {m_DoesntHaveFacts.Length} facts";
		}
		return text;
	}
}
