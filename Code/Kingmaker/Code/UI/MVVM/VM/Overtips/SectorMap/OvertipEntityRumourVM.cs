using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.SectorMap;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

public class OvertipEntityRumourVM : OvertipEntityVM
{
	public readonly SectorMapRumourEntity SectorMapRumour;

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>(initialValue: false);

	private Vector3 m_Position = Vector3.zero;

	protected override bool UpdateEnabled => SectorMapRumour.IsQuestObjectiveActive;

	protected override Vector3 GetEntityPosition()
	{
		return m_Position;
	}

	public OvertipEntityRumourVM(SectorMapRumourEntity sectorMapRumourEntity)
	{
		SectorMapRumour = sectorMapRumourEntity;
		SetPosition();
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		SectorMapView sectorMapView = SectorMapView.Instance.Or(null);
		bool flag = (object)sectorMapView != null && sectorMapView.LayersMask.HasFlag(SystemMapLayer.Rumors);
		IsVisible.Value = SectorMapRumour.IsQuestObjectiveActive && flag;
	}

	private void SetPosition()
	{
		Renderer unityObject = SectorMapRumour.View.Renderers.FirstOrDefault((Renderer renderer) => renderer is LineRenderer);
		m_Position = unityObject.Or(null)?.bounds.center ?? Vector3.zero;
	}
}
