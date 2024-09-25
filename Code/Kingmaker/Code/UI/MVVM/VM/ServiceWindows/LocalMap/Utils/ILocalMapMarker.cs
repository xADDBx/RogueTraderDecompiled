using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;

public interface ILocalMapMarker
{
	LocalMapMarkType GetMarkerType();

	string GetDescription();

	Vector3 GetPosition();

	bool IsVisible();

	bool IsMapObject();

	Entity GetEntity();
}
