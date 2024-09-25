using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UI.Pointer;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;

public class LocalMapDestinationMarkerVM : LocalMapMarkerVM
{
	private readonly BaseUnitEntity m_Unit;

	public LocalMapDestinationMarkerVM(BaseUnitEntity unit)
	{
		m_Unit = unit;
		MarkerType = LocalMapMarkType.DestinationMark;
		IsVisible.Value = false;
		Position.Value = m_Unit.Position;
	}

	protected override void OnUpdateHandler()
	{
		if (m_Unit != null)
		{
			Dictionary<BaseUnitEntity, Vector3> unitMarksLocalMap = ClickPointerManager.Instance.UnitMarksLocalMap;
			IsVisible.Value = unitMarksLocalMap.ContainsKey(m_Unit);
			if (IsVisible.Value)
			{
				Position.Value = unitMarksLocalMap[m_Unit];
			}
		}
	}

	public override Entity GetEntity()
	{
		return m_Unit;
	}
}
