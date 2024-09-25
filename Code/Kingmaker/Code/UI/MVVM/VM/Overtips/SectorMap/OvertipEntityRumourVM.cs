using System;
using JetBrains.Annotations;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.SectorMap;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

[Serializable]
public class OvertipEntityRumourVM : OvertipEntityVM
{
	[CanBeNull]
	public readonly SectorMapRumourEntity SectorMapRumour;

	[CanBeNull]
	public readonly SectorMapRumourGroupView.SectorMapRumourGroupEntity SectorMapRumourGroup;

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>(initialValue: false);

	private Vector3 m_Position = Vector3.zero;

	protected override bool UpdateEnabled
	{
		get
		{
			SectorMapRumourEntity sectorMapRumour = SectorMapRumour;
			int num;
			if (sectorMapRumour == null)
			{
				if (SectorMapRumourGroup == null)
				{
					num = 0;
					goto IL_0037;
				}
				num = ((!SectorMapRumourGroup.View.ActiveQuestObjectives.Empty()) ? 1 : 0);
			}
			else
			{
				num = (sectorMapRumour.IsQuestObjectiveActive ? 1 : 0);
			}
			if (num == 0)
			{
				goto IL_0037;
			}
			goto IL_0043;
			IL_0043:
			return (byte)num != 0;
			IL_0037:
			IsVisible.Value = false;
			goto IL_0043;
		}
	}

	protected override Vector3 GetEntityPosition()
	{
		return m_Position;
	}

	public OvertipEntityRumourVM(SectorMapRumourEntity sectorMapRumourEntity)
	{
		SectorMapRumour = sectorMapRumourEntity;
		SectorMapRumourGroup = null;
		SectorMapRumourEntity sectorMapRumour = SectorMapRumour;
		if (sectorMapRumour == null || !sectorMapRumour.View.HasParent)
		{
			SetPosition();
		}
	}

	public OvertipEntityRumourVM(SectorMapRumourGroupView.SectorMapRumourGroupEntity sectorMapRumourGroupEntity)
	{
		SectorMapRumour = null;
		SectorMapRumourGroup = sectorMapRumourGroupEntity;
		SetPosition();
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		SectorMapView sectorMapView = SectorMapView.Instance.Or(null);
		bool flag = (object)sectorMapView != null && sectorMapView.LayersMask.HasFlag(SystemMapLayer.Rumors);
		IsVisible.Value = UpdateEnabled && flag;
	}

	private void SetPosition()
	{
		Renderer renderer2 = new Renderer();
		if (SectorMapRumour != null)
		{
			renderer2 = SectorMapRumour.View.Renderers.FirstOrDefault((Renderer renderer) => renderer is LineRenderer);
		}
		if (SectorMapRumourGroup != null)
		{
			renderer2 = SectorMapRumourGroup.View.Renderers.FirstOrDefault((Renderer renderer) => renderer is LineRenderer);
			if (renderer2 == null)
			{
				throw new Exception("OvertipEntityRumourVM[" + SectorMapRumourGroup.View.name + "]: bad Renderer");
			}
		}
		m_Position = renderer2.Or(null)?.bounds.center ?? Vector3.zero;
	}
}
