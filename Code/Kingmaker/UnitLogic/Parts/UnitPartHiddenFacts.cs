using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.QA;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartHiddenFacts : BaseUnitPart, IHashable
{
	private readonly List<IHiddenUnitFacts> m_HiddenFacts = new List<IHiddenUnitFacts>();

	[CanBeNull]
	public BlueprintRace ReplaceRace => m_HiddenFacts.FirstItem((IHiddenUnitFacts i) => i.ReplaceRace != null)?.ReplaceRace;

	public bool IsHidden(BlueprintFact fact)
	{
		return m_HiddenFacts.HasItem((IHiddenUnitFacts i) => i.Facts.Contains(fact));
	}

	public void Add(IHiddenUnitFacts hiddenUnitFacts)
	{
		if (m_HiddenFacts.Contains(hiddenUnitFacts))
		{
			PFLog.Default.ErrorWithReport("UnitPartHiddenFeatures.Add: can't add item twice");
		}
		else
		{
			m_HiddenFacts.Add(hiddenUnitFacts);
		}
	}

	public void Remove(IHiddenUnitFacts hiddenUnitFacts)
	{
		m_HiddenFacts.Remove(hiddenUnitFacts);
		if (m_HiddenFacts.Empty())
		{
			RemoveSelf();
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
