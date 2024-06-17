using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SystemMap;

public class SystemScannerObjectVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Sprite Icon;

	public readonly Vector3 Position;

	public readonly Color Color;

	public SystemScannerObjectVM(AnomalyEntityData anomalyEntityData)
	{
		Color = UIConfig.Instance.AnomalyColor.GetAnomalyColor(anomalyEntityData.Blueprint.AnomalyType);
		Icon = UIConfig.Instance.AnomalyIcons.GetAnomalyIcon(anomalyEntityData.Blueprint.AnomalyType);
		Position = CameraRig.Instance.WorldToViewport(anomalyEntityData.Position);
	}

	protected override void DisposeImplementation()
	{
	}
}
