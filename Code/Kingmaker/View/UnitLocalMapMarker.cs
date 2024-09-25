using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.View;

public class UnitLocalMapMarker : ILocalMapMarker
{
	private readonly UnitEntityView m_Unit;

	public UnitLocalMapMarker(UnitEntityView unit)
	{
		m_Unit = unit;
	}

	public LocalMapMarkType GetMarkerType()
	{
		if (!(m_Unit != null))
		{
			return LocalMapMarkType.Invalid;
		}
		if (!m_Unit.EntityData.IsDeadAndHasLoot)
		{
			return LocalMapMarkType.Unit;
		}
		return LocalMapMarkType.Loot;
	}

	public string GetDescription()
	{
		return m_Unit.EntityData.CharacterName;
	}

	public Vector3 GetPosition()
	{
		if (!(m_Unit != null))
		{
			return Vector3.zero;
		}
		return m_Unit.ViewTransform.position;
	}

	public bool IsVisible()
	{
		return m_Unit.IsVisible;
	}

	public bool IsMapObject()
	{
		return false;
	}

	public Entity GetEntity()
	{
		return m_Unit.Data;
	}
}
