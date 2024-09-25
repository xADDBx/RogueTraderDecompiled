using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("ad170ffaa2264cc9804e5005e7050adf")]
public class RemoveFact : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	[SerializeField]
	private BlueprintUnitFactReference m_ExceptHasFact;

	[SerializeField]
	private BlueprintUnitFactReference[] m_AdditionalExceptHasFacts;

	[SerializeField]
	private bool m_ExcludeExCompanions;

	[SerializeField]
	[InfoBox("Checks in Blueprint and OriginalBlueprint")]
	private BlueprintUnitReference m_TargetPartyUnit;

	public BlueprintUnitFact Fact => m_Fact;

	[CanBeNull]
	public BlueprintFact ExceptHasFact => (BlueprintUnitFact)m_ExceptHasFact;

	public ReferenceArrayProxy<BlueprintUnitFact> AdditionalExceptHasFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] additionalExceptHasFacts = m_AdditionalExceptHasFacts;
			return additionalExceptHasFacts;
		}
	}

	public BlueprintUnit TargetPartyUnit => m_TargetPartyUnit;

	public override string GetCaption()
	{
		string text = null;
		if (ExceptHasFact != null || AdditionalExceptHasFacts.Length > 0)
		{
			using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
			if (ExceptHasFact != null)
			{
				pooledStringBuilder.Builder.Append(ExceptHasFact);
			}
			for (int i = 0; i < AdditionalExceptHasFacts.Length; i++)
			{
				if (i != 0 || ExceptHasFact != null)
				{
					pooledStringBuilder.Builder.Append(", ");
				}
				BlueprintUnitFact blueprintUnitFact = AdditionalExceptHasFacts[i];
				pooledStringBuilder.Builder.Append(blueprintUnitFact);
			}
			text = pooledStringBuilder.Builder.ToString();
		}
		string text2 = ((text != null) ? $"Remove {Fact} if unit has no {text}" : $"Remove {Fact}");
		if (TargetPartyUnit != null)
		{
			text2 += $" for {TargetPartyUnit}";
		}
		return text2;
	}

	protected override void RunActionOverride()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			Upgrade(allCharacter);
		}
	}

	private void Upgrade(BaseUnitEntity unit)
	{
		if ((TargetPartyUnit == null || TargetPartyUnit == unit.Blueprint || TargetPartyUnit == unit.OriginalBlueprint) && (ExceptHasFact == null || !unit.Facts.Contains(ExceptHasFact)) && !AdditionalExceptHasFacts.Any(unit.Facts.Contains))
		{
			UnitPartCompanion optional = unit.Parts.GetOptional<UnitPartCompanion>();
			if (!m_ExcludeExCompanions || optional == null || optional.State != CompanionState.ExCompanion)
			{
				unit.Facts.Remove(Fact);
			}
		}
	}
}
