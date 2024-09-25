using System;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap;

public class OtherObjectsInfoSpaceSystemInformationWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Name;

	public readonly Sprite Icon;

	public readonly BlueprintArtificialObject BlueprintOtherObject;

	public readonly BlueprintStarSystemMap BlueprintStarSystemMap;

	public OtherObjectsInfoSpaceSystemInformationWindowVM(BlueprintArtificialObject blueprintOtherObject, BlueprintStarSystemMap blueprintStarSystemMap)
	{
		Name = blueprintOtherObject.Name;
		Icon = blueprintOtherObject.Icon;
		BlueprintOtherObject = blueprintOtherObject;
		BlueprintStarSystemMap = blueprintStarSystemMap;
	}

	protected override void DisposeImplementation()
	{
	}
}
