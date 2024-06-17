using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SystemMap.PC;

public class OvertipAnomalyPCView : OvertipAnomalyView
{
	[SerializeField]
	private Image m_HintTaker;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AnomalyView anomalyView = base.ViewModel.SystemMapObject.View as AnomalyView;
		if (!(anomalyView == null))
		{
			BlueprintAnomaly.AnomalyObjectType anomalyType = anomalyView.Data.Blueprint.AnomalyType;
			AddDisposable(m_HintTaker.SetHint(UIStrings.Instance.ExplorationTexts.GetAnomalyTypeName(anomalyType)));
		}
	}
}
