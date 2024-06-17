using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Prerequisites;

[Serializable]
[TypeId("40a1bdb738834fb69a296a2677235b35")]
public class PrerequisiteFact : Prerequisite
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	public int MinRank;

	public BlueprintUnitFact Fact => m_Fact;

	protected override bool MeetsInternal(IBaseUnitEntity unit)
	{
		if (unit.ToBaseUnitEntity().Facts.Get(Fact) is Feature feature)
		{
			return feature.Rank >= MinRank;
		}
		return false;
	}

	protected override string GetCaptionInternal()
	{
		if (MinRank <= 1)
		{
			return $"has {Fact}";
		}
		return $"has {Fact} (at least rank {MinRank})";
	}
}
