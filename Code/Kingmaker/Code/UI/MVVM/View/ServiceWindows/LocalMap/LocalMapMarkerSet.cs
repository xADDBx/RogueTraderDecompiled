using System;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap;

[Serializable]
public class LocalMapMarkerSet
{
	public LocalMapMarkType Type;

	public LocalMapMarkerPCView View;

	public RectTransform Container;
}
