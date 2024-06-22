using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Tutorial.Solvers;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.UnityExtensions;

namespace Kingmaker.Utility;

public static class LinksHelper
{
	[CanBeNull]
	public static BaseUnitEntity GetUnit(string id)
	{
		return EntityService.Instance.GetEntity<BaseUnitEntity>(id);
	}

	[CanBeNull]
	public static ItemEntity GetItem(string id)
	{
		return EntityService.Instance.GetEntity<ItemEntity>(id);
	}

	[CanBeNull]
	public static BlueprintItem GetBlueprintItem(string id)
	{
		return Utilities.GetBlueprint<BlueprintItem>(id);
	}

	[CanBeNull]
	public static BlueprintCargo GetBlueprintCargo(string id)
	{
		return Utilities.GetBlueprint<BlueprintCargo>(id);
	}

	[CanBeNull]
	public static BlueprintUnitFact GetUnitFact(string id)
	{
		return ResourcesLibrary.TryGetBlueprint(id) as BlueprintUnitFact;
	}

	[CanBeNull]
	public static BlueprintAnswer GetAnswer(string id)
	{
		return ResourcesLibrary.TryGetBlueprint(id) as BlueprintAnswer;
	}

	[CanBeNull]
	public static AbilityData GetPartyAbility(string id)
	{
		IEnumerator<AbilityData> enumerator = PartySpellsEnumerator.Get(withAbilities: true);
		while (enumerator.MoveNext())
		{
			if (enumerator.Current?.UniqueId == id)
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	[CanBeNull]
	public static UIPropertySettings GetUISettings(string[] keys)
	{
		return GetUnitFact(keys[1])?.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings settings) => settings.LinkKey == keys[2]);
	}

	public static StatTooltipData GetStatData(string stat, string unitId)
	{
		if (string.IsNullOrEmpty(stat) || string.IsNullOrEmpty(unitId))
		{
			return null;
		}
		BaseUnitEntity unit = GetUnit(unitId);
		if (unit == null)
		{
			return null;
		}
		StatType? statType = UIUtility.TryGetStatType(stat);
		if (!statType.HasValue)
		{
			return null;
		}
		ModifiableValue stat2 = unit.Stats.GetStat(statType.Value);
		if (!(stat2 is ModifiableValueSavingThrow savingThrow))
		{
			if (!(stat2 is ModifiableValueSkill skill))
			{
				if (stat2 is ModifiableValueAttributeStat attribute)
				{
					return new StatTooltipData(attribute);
				}
				return new StatTooltipData(stat2);
			}
			return new StatTooltipData(skill);
		}
		return new StatTooltipData(savingThrow);
	}

	public static string GetUIPropertySettingsReferenceLink(string linkKey, BlueprintUnitFact blueprint)
	{
		if (linkKey.IsNullOrEmpty())
		{
			return string.Empty;
		}
		return "{uip|" + linkKey + "|" + blueprint.AssetGuid + "}";
	}
}
