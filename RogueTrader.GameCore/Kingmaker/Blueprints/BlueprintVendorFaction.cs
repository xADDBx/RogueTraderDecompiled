using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("535b405b948543eca5e301737aece91b")]
public class BlueprintVendorFaction : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintVendorFaction>
	{
	}

	[SerializeField]
	private FactionType m_Faction;

	[SerializeField]
	private LocalizedString m_DisplayName;

	[SerializeField]
	private LocalizedString m_Description;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private ItemsItemOrigin[] m_CargoTypes;

	[SerializeField]
	private bool m_OverrideReputation;

	[SerializeField]
	[ValidateNotNull]
	[ShowIf("OverrideReputation")]
	private FactionReputationSettings m_Reputation;

	public bool OverrideReputation => m_OverrideReputation;

	public IReadOnlyList<FactionReputationSettings.FactionReputationLevel> ReputationLevelThresholds => m_Reputation.ReputationLevelThresholds;

	public FactionType FactionType => m_Faction;

	public LocalizedString DisplayName => m_DisplayName;

	public LocalizedString Description => m_Description;

	public Sprite Icon => m_Icon;

	public ItemsItemOrigin[] CargoTypes => m_CargoTypes;
}
