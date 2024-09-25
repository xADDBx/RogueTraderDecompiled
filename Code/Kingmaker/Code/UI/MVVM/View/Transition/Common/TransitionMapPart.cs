using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Transition.Common;

[Serializable]
public class TransitionMapPart
{
	public BlueprintMultiEntrance.BlueprintMultiEntranceMap Map;

	public GameObject MapObject;

	public List<TransitionEntryBaseView> Entries;

	public WidgetListMVVM WidgetList;

	public RectTransform LightBeam;

	public CanvasGroup LightBeamCanvas;

	public List<PointOnMap> PointsOnMap;

	public bool CustomPantographMaxY;

	[ShowIf("CustomPantographMaxY")]
	public float CustomPantographMaxYValue;

	public OwlcatButton Close;

	public void Initialize()
	{
		MapObject.SetActive(value: false);
		Entries.ForEach(delegate(TransitionEntryBaseView v)
		{
			v.Initialize();
		});
	}
}
