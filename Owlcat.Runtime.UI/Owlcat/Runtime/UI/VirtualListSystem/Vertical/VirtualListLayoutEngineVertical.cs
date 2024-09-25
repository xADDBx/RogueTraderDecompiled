using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.Vertical;

internal class VirtualListLayoutEngineVertical : IVirtualListLayoutEngine
{
	private VirtualListLayoutSettingsVertical m_Settings;

	private VirtualListLayoutEngineContext m_EngineContext;

	private VirtualListDistancesCalculator m_DistancesCalculator;

	public VirtualListLayoutEngineVertical(VirtualListLayoutSettingsVertical settings, VirtualListLayoutEngineContext engineContext, VirtualListDistancesCalculator distancesCalculator)
	{
		m_Settings = settings;
		m_EngineContext = engineContext;
		m_DistancesCalculator = distancesCalculator;
	}

	public void SetClear()
	{
		m_EngineContext.LastElementCornerPosition = new Vector2(m_Settings.Padding.Left, 0f - m_Settings.Padding.Top);
	}

	public void SetOffsetElement(VirtualListElement element, bool forItself = false)
	{
		float left = m_Settings.Padding.Left;
		float num;
		if (m_EngineContext.UpdateType == VirtualListUpdateType.FromTopToBottom)
		{
			num = element.OffsetMin.y;
			if (!forItself)
			{
				num -= m_Settings.Spacing;
			}
		}
		else
		{
			num = element.OffsetMax.y;
			if (!forItself)
			{
				num += m_Settings.Spacing;
			}
		}
		m_EngineContext.LastElementCornerPosition = new Vector2(left, num);
	}

	public void SetOffset(float position)
	{
		float left = m_Settings.Padding.Left;
		m_EngineContext.LastElementCornerPosition = new Vector2(left, position);
	}

	public void UpdatePosition(VirtualListElement element)
	{
		Vector2 anchorMax = (element.AnchorMin = new Vector2(0f, 1f));
		element.AnchorMax = anchorMax;
		float width = m_Settings.Width;
		float height = m_Settings.Height;
		VirtualListLayoutPadding padding = VirtualListLayoutPadding.Zero;
		if (element.HasLayoutSettings())
		{
			VirtualListLayoutElementSettings layoutSettings = element.LayoutSettings;
			if (layoutSettings.OverrideWidth)
			{
				width = layoutSettings.Width;
			}
			if (layoutSettings.OverrideHeight)
			{
				height = layoutSettings.Height;
			}
			if (layoutSettings.OverrideType == VirtualListLayoutElementSettings.LayoutOverrideType.Custom)
			{
				padding = layoutSettings.Padding;
			}
		}
		Vector2 lastElementCornerPosition = m_EngineContext.LastElementCornerPosition;
		if (m_EngineContext.UpdateType == VirtualListUpdateType.FromTopToBottom)
		{
			lastElementCornerPosition -= new Vector2(0f, height);
		}
		Vector2 offsetMax = lastElementCornerPosition + new Vector2(width, height);
		element.OffsetMin = lastElementCornerPosition;
		element.OffsetMax = offsetMax;
		element.Padding = padding;
		element.MarkUpdated();
	}

	public bool IsInFieldOfView(VirtualListElement element)
	{
		if (element.OffsetMin.y > m_DistancesCalculator.ViewportTopBorder + m_Settings.VisibleZoneExpansion)
		{
			return false;
		}
		if (element.OffsetMax.y < m_DistancesCalculator.ViewportBottomBorder - m_Settings.VisibleZoneExpansion)
		{
			return false;
		}
		return true;
	}
}
