using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.SystemMechanics;

[TypeId("161d130e1ab5440890f214f98055360a")]
public class BlueprintDifficultyRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintDifficultyRoot>
	{
	}

	[Serializable]
	public class DifficultyTypeFacts
	{
		public UnitDifficultyType Type;

		[SerializeField]
		[ValidateNoNullEntries]
		private BlueprintUnitFactReference[] m_Facts;

		public ReferenceArrayProxy<BlueprintUnitFact> Facts
		{
			get
			{
				BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
				return facts;
			}
		}
	}

	public DifficultyTypeFacts[] UnitDifficultyFacts = new DifficultyTypeFacts[0];

	[NotNull]
	public IEnumerable<BlueprintUnitFact> GetDifficultyFacts(UnitDifficultyType type)
	{
		ReferenceArrayProxy<BlueprintUnitFact>? referenceArrayProxy = UnitDifficultyFacts.FirstItem((DifficultyTypeFacts i) => i.Type == type)?.Facts;
		if (!referenceArrayProxy.HasValue)
		{
			return Enumerable.Empty<BlueprintUnitFact>();
		}
		return referenceArrayProxy.GetValueOrDefault();
	}
}
