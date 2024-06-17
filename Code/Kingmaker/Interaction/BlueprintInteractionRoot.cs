using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Interaction;

[TypeId("146c0886bade0a44a9c5a356d35a8775")]
public class BlueprintInteractionRoot : BlueprintScriptableObject
{
	[Serializable]
	public class GlobalInteractionSettings
	{
		[SerializeField]
		[ValidateNotNull]
		private StatType[] m_InteractOnlyByNotInteractedUnitSkills;

		public bool CheckInteractOnlyByNotInteractedUnit(StatType skill)
		{
			return m_InteractOnlyByNotInteractedUnitSkills.Any((StatType x) => x == skill);
		}
	}

	[Serializable]
	public class Referense : BlueprintReference<BlueprintInteractionRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private GlobalInteractionSettings m_GlobalInteractionSkillCheckSettings;

	[SerializeField]
	[ValidateNotNull]
	private GlobalInteractionSettings m_GlobalSkillCheckRestrictionSettings;

	[SerializeField]
	private int m_InteractionDCVariation = 2;

	[SerializeField]
	[InfoBox("Used to calculate result count after item destruction: Item.Cost / MagicPowerCost = `count of MagicPowerItem`")]
	private int m_MagicPowerCost = 100;

	[SerializeField]
	[InfoBox("Will drop from destroyed items to indicate their cost")]
	private BlueprintItemReference m_MagicPowerItem;

	[SerializeField]
	private PrefabLink m_DestructionFx;

	public GlobalInteractionSettings GlobalInteractionSkillCheckSettings => m_GlobalInteractionSkillCheckSettings;

	public GlobalInteractionSettings GlobalSkillCheckRestrictionSettings => m_GlobalSkillCheckRestrictionSettings;

	public int InteractionDCVariation => m_InteractionDCVariation;

	public BlueprintItem MagicPowerItem => m_MagicPowerItem;

	public int MagicPowerCost => m_MagicPowerCost;

	public PrefabLink DestructionFx => m_DestructionFx;
}
