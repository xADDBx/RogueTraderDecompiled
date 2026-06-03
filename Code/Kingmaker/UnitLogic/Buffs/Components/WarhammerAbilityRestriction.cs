using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4154c6f5c7b64b23a2102278fc83645b")]
public class WarhammerAbilityRestriction : UnitBuffComponentDelegate, IHashable
{
	[Tooltip("Определяет, должна ли выполняться проверка на блок абилки. Все проверки будут проводиться только в случае прохождения этого рестрикшна")]
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	[Tooltip("Абилка блочится, если рестрикшн проходится.")]
	private RestrictionCalculator m_AbilityRestrictions = new RestrictionCalculator();

	public bool AbilityIsRestricted(AbilityData abilityData)
	{
		PropertyContext context = new PropertyContext(abilityData);
		if (!Restrictions.IsPassed(context))
		{
			return false;
		}
		return m_AbilityRestrictions.IsPassed(context);
	}

	public string GetAbilityRestrictionUIText()
	{
		if (base.OwnerBlueprint is BlueprintUnitFact blueprintUnitFact)
		{
			LocalizedString hasForbiddenCondition = LocalizedTexts.Instance.Reasons.HasForbiddenCondition;
			string factName = UIUtilityTexts.GetBlueprintUnitFactNameText(blueprintUnitFact);
			return hasForbiddenCondition.ToString(delegate
			{
				GameLogContext.Text = factName;
			});
		}
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
