using System;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap;

public class AdditionalAnomaliesInfoSpaceSystemInformationWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly BlueprintAnomaly BlueprintAnomaly;

	public readonly BlueprintStarSystemMap BlueprintStarSystemMap;

	public AdditionalAnomaliesInfoSpaceSystemInformationWindowVM(BlueprintAnomaly blueprintAnomaly, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		Name = blueprintAnomaly.Name;
		Icon = blueprintAnomaly.Icon;
		BlueprintAnomaly = blueprintAnomaly;
		BlueprintStarSystemMap = blueprintStarSystemMap;
	}

	protected override void DisposeImplementation()
	{
	}
}
