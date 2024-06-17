using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public class SurfaceActionBarAbilitiesRowView : MonoBehaviour, IDisposable
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	private IDisposable m_Disposes;

	private RectTransform m_TooltipCustomPosition;

	private List<Vector2> m_TooltipCustomPivots;

	public void Initialize(RectTransform tooltipPosition, List<Vector2> tooltipPivots)
	{
		m_TooltipCustomPosition = tooltipPosition;
		m_TooltipCustomPivots = tooltipPivots;
	}

	public void DrawEntries(List<ActionBarSlotVM> slots, SurfaceActionBarSlotAbilityView prefab)
	{
		m_Disposes?.Dispose();
		m_Disposes = m_WidgetList.DrawEntries(slots.ToArray(), prefab, strictMatching: true);
		SetActionBarSlotsTooltipCustomPosition();
	}

	public void Dispose()
	{
		m_WidgetList.VisibleEntries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_Disposes?.Dispose();
		m_Disposes = null;
	}

	public List<IWidgetView> GetSlots()
	{
		return m_WidgetList.Entries?.ToList();
	}

	public List<IConsoleNavigationEntity> GetConsoleEntities()
	{
		return m_WidgetList.Entries.Cast<IConsoleNavigationEntity>().ToList();
	}

	public IConsoleNavigationEntity GetFirstValidEntity()
	{
		return GetConsoleEntities().FirstOrDefault((IConsoleNavigationEntity e) => e?.IsValid() ?? false);
	}

	private void SetActionBarSlotsTooltipCustomPosition()
	{
		if (m_TooltipCustomPosition == null || m_TooltipCustomPivots == null)
		{
			return;
		}
		foreach (IWidgetView visibleEntry in m_WidgetList.VisibleEntries)
		{
			if (visibleEntry is SurfaceActionBarSlotAbilityView surfaceActionBarSlotAbilityView)
			{
				surfaceActionBarSlotAbilityView.Initialize();
				surfaceActionBarSlotAbilityView.SetTooltipCustomPosition(m_TooltipCustomPosition, m_TooltipCustomPivots);
			}
		}
	}
}
