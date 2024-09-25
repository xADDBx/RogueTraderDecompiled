using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Globalmap.Blueprints.SectorMap;

[TypeId("26ce44c3d7634d54afef5387f8b9b300")]
public class BlueprintSectorMapPointStarSystem : BlueprintSectorMapPoint
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintSectorMapPointStarSystem>
	{
	}

	[CanBeNull]
	public BlueprintAreaReference StarSystemToTransit;

	[CanBeNull]
	public BlueprintAreaEnterPointReference StarSystemAreaPoint;

	[CanBeNull]
	public ConditionsChecker BookEventConditions;

	[CanBeNull]
	public BlueprintDialogReference OverrideBookEvent;

	public bool IsFakeSystem;

	[CanBeNull]
	public ConditionsChecker ConditionsToVisitAutomatically;
}
