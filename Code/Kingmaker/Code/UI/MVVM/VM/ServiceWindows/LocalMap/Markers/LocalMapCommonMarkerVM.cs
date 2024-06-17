using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;

public class LocalMapCommonMarkerVM : LocalMapMarkerVM
{
	private readonly ILocalMapMarker m_Marker;

	public LocalMapCommonMarkerVM(ILocalMapMarker marker)
	{
		m_Marker = marker;
		Position.Value = marker.GetPosition();
		IsVisible.Value = marker.IsVisible();
		Description.Value = marker.GetDescription();
		IsMapObject.Value = marker.IsMapObject();
		MarkerType = marker.GetMarkerType();
		if (MarkerType == LocalMapMarkType.Loot && Description.Value.Empty())
		{
			Description.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Loot;
		}
	}

	protected override void OnUpdateHandler()
	{
		if (m_Marker != null)
		{
			Position.Value = m_Marker.GetPosition();
			IsVisible.Value = m_Marker.IsVisible();
		}
	}

	public override Entity GetEntity()
	{
		return m_Marker.GetEntity();
	}
}
