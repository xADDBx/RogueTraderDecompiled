using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("e86225224a394ea8a20cfd197baaf46a")]
public class BlueprintVendorFactionsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintVendorFactionsRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private FactionReputationSettings m_DefaultFactionReputation;

	[SerializeField]
	[ValidateNotNull]
	private List<BlueprintVendorFaction.Reference> m_VendorFactions;

	public FactionReputationSettings DefaultFactionReputation => m_DefaultFactionReputation;

	public IReadOnlyList<BlueprintVendorFaction.Reference> VendorFactions => m_VendorFactions;
}
