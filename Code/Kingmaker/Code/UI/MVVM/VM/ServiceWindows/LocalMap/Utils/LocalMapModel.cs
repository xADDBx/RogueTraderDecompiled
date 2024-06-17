using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;

public static class LocalMapModel
{
	public static readonly HashSet<ILocalMapMarker> Markers = new HashSet<ILocalMapMarker>();

	public static bool IsInCurrentArea(Vector3 pos)
	{
		return Game.Instance.CurrentlyLoadedAreaPart.Bounds.LocalMapBounds.ContainsXZ(pos);
	}
}
