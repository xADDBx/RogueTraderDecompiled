using System;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIExplorationTexts
{
	public LocalizedString ExploObjectResources;

	public LocalizedString ExploObjectResourcesEmpty;

	public LocalizedString ExploPointsOfInterest;

	public LocalizedString ExploBeginScan;

	public LocalizedString ExploCancelScan;

	public LocalizedString ExploAlreadyExplored;

	public LocalizedString ExploNotExplored;

	public LocalizedString ExploNotInteractable;

	public LocalizedString AnomalyVisitUnknown;

	public LocalizedString AnomalyVisitExplored;

	public LocalizedString StatCheckLootCheckStatButton;

	public LocalizedString StatCheckLootConfirmSelectedUnitButton;

	public LocalizedString StatCheckLootSwitchUnitSubHeader;

	public LocalizedString ExpeditionHeader;

	public LocalizedString ExpeditionRewardsDescription;

	public LocalizedString ExpeditionSendButtonLabel;

	public LocalizedString ExpeditionSentDialogMessage;

	public LocalizedString ResourceMiner;

	public LocalizedString ResourceMinerDesc;

	public LocalizedString NotEnoughResourceMiners;

	public LocalizedString UseResourceMinerDialogMessage;

	public LocalizedString RemoveResourceMinerDialogMessage;

	public LocalizedString StartMiningNotificationText;

	public LocalizedString StopMiningNotificationText;

	public LocalizedString YourScannersFoundResources;

	public LocalizedString DefaultAnomalyTypeName;

	public LocalizedString ShipSignatureAnomalyTypeName;

	public LocalizedString EnemyAnomalyTypeName;

	public LocalizedString GasAnomalyTypeName;

	public LocalizedString WarpHtonAnomalyTypeName;

	public LocalizedString LootAnomalyTypeName;

	public LocalizedString NoAnomalyInSystem;

	public LocalizedString TitheGrade;

	public LocalizedString TitheGradeUndetermined;

	public LocalizedString Aestimare;

	public LocalizedString DiscoveredResources;

	public LocalizedString ResourceMining;

	[Header("Points of Interest")]
	public LocalizedString BookEventPoi;

	public LocalizedString CargoPoi;

	public LocalizedString ColonyTraitPoi;

	public LocalizedString ExpeditionPoi;

	public LocalizedString GroundOperationPoi;

	public LocalizedString LootPoi;

	public LocalizedString ResourcesPoi;

	public LocalizedString StatCheckLootPoi;

	public string GetAnomalyTypeName(BlueprintAnomaly.AnomalyObjectType type)
	{
		return type switch
		{
			BlueprintAnomaly.AnomalyObjectType.ShipSignature => ShipSignatureAnomalyTypeName, 
			BlueprintAnomaly.AnomalyObjectType.Enemy => EnemyAnomalyTypeName, 
			BlueprintAnomaly.AnomalyObjectType.Gas => GasAnomalyTypeName, 
			BlueprintAnomaly.AnomalyObjectType.WarpHton => WarpHtonAnomalyTypeName, 
			BlueprintAnomaly.AnomalyObjectType.Loot => LootAnomalyTypeName, 
			_ => DefaultAnomalyTypeName, 
		};
	}

	public string GetPointOfInterestTypeName(BlueprintPointOfInterest type)
	{
		if (!(type is BlueprintPointOfInterestBookEvent))
		{
			if (!(type is BlueprintPointOfInterestCargo))
			{
				if (!(type is BlueprintPointOfInterestColonyTrait))
				{
					if (!(type is BlueprintPointOfInterestExpedition))
					{
						if (!(type is BlueprintPointOfInterestGroundOperation))
						{
							if (!(type is BlueprintPointOfInterestLoot))
							{
								if (!(type is BlueprintPointOfInterestResources))
								{
									if (type is BlueprintPointOfInterestStatCheckLoot)
									{
										return StatCheckLootPoi;
									}
									return "NO TYPE";
								}
								return ResourcesPoi;
							}
							return LootPoi;
						}
						return GroundOperationPoi;
					}
					return ExpeditionPoi;
				}
				return ColonyTraitPoi;
			}
			return CargoPoi;
		}
		return BookEventPoi;
	}
}
