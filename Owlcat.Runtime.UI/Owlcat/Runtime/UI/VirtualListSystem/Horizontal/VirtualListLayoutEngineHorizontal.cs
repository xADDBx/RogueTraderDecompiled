using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.Horizontal;

internal class VirtualListLayoutEngineHorizontal : IVirtualListLayoutEngine
{
	private VirtualListDistancesCalculator m_DistancesCalculator;

	private VirtualListLayoutSettingsHorizontal m_Settings;

	private VirtualListLayoutEngineContext m_EngineContext;

	public VirtualListLayoutEngineHorizontal(VirtualListLayoutSettingsHorizontal settings, VirtualListLayoutEngineContext engineContext, VirtualListDistancesCalculator distancesCalculator)
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
		float num = 0f;
		float y = 0f - m_Settings.Padding.Top;
		if (m_EngineContext.UpdateType == VirtualListUpdateType.FromTopToBottom)
		{
			num = element.OffsetMax.x;
			if (!forItself)
			{
				num += m_Settings.Spacing;
			}
		}
		else
		{
			num = element.OffsetMin.x;
			if (!forItself)
			{
				num -= m_Settings.Spacing;
			}
		}
		m_EngineContext.LastElementCornerPosition = new Vector2(num, y);
	}

	public void SetOffset(float position)
	{
		float y = 0f - m_Settings.Padding.Top;
		m_EngineContext.LastElementCornerPosition = new Vector2(position, y);
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
		Vector2 vector2 = m_EngineContext.LastElementCornerPosition - new Vector2(0f, height);
		if (m_EngineContext.UpdateType == VirtualListUpdateType.FromBottomToTop)
		{
			vector2 -= new Vector2(width, 0f);
		}
		Vector2 offsetMax = vector2 + new Vector2(width, height);
		element.OffsetMin = vector2;
		element.OffsetMax = offsetMax;
		element.Padding = padding;
		element.MarkUpdated();
	}

	public bool IsInFieldOfView(VirtualListElement element)
	{
		if (element.OffsetMin.x > m_DistancesCalculator.ViewportBottomBorder + m_Settings.VisibleZoneExpansion)
		{
			return false;
		}
		if (element.OffsetMax.x < m_DistancesCalculator.ViewportTopBorder - m_Settings.VisibleZoneExpansion)
		{
			return false;
		}
		return true;
	}
}
