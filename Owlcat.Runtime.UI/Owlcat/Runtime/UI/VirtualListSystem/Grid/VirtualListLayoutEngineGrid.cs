using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.Grid;

internal class VirtualListLayoutEngineGrid : IVirtualListLayoutEngine
{
	private VirtualListLayoutSettingsGrid m_Settings;

	private VirtualListLayoutEngineContext m_EngineContext;

	private VirtualListDistancesCalculator m_DistancesCalculator;

	public VirtualListLayoutEngineGrid(VirtualListLayoutSettingsGrid settings, VirtualListLayoutEngineContext engineContext, VirtualListDistancesCalculator distancesCalculator)
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
		int num = m_EngineContext.CurrentElementIndex % m_Settings.ElementsInRow;
		float num2;
		float num3;
		if (m_EngineContext.UpdateType == VirtualListUpdateType.FromTopToBottom)
		{
			if (!forItself && m_Settings.IsEdgeIndex(num + 1))
			{
				num2 = m_Settings.Padding.Left;
				num3 = element.OffsetMax.y - m_Settings.Spacing.y - m_Settings.Height;
			}
			else
			{
				num2 = m_Settings.Padding.Left + (m_Settings.Width + m_Settings.Spacing.x) * (float)num + m_Settings.Width;
				num3 = element.OffsetMax.y;
				if (forItself)
				{
					num3 -= m_Settings.Height;
				}
				else
				{
					num2 += m_Settings.Spacing.x;
				}
			}
		}
		else if (!forItself && m_Settings.IsEdgeIndex(num))
		{
			num2 = m_Settings.Padding.Left + (m_Settings.Width + m_Settings.Spacing.x) * (float)m_Settings.ElementsInRow - m_Settings.Spacing.x;
			num3 = element.OffsetMin.y + m_Settings.Spacing.y + m_Settings.Height;
		}
		else
		{
			num2 = m_Settings.Padding.Left + (m_Settings.Width + m_Settings.Spacing.x) * (float)num;
			num3 = element.OffsetMin.y;
			if (forItself)
			{
				num3 += m_Settings.Height;
			}
			else
			{
				num2 -= m_Settings.Spacing.x;
			}
		}
		m_EngineContext.LastElementCornerPosition = new Vector2(num2, num3);
	}

	public void SetOffset(float position)
	{
		int num = m_EngineContext.CurrentElementIndex % m_Settings.ElementsInRow;
		float num2 = m_Settings.Padding.Left + (m_Settings.Width + m_Settings.Spacing.x) * (float)num;
		if (m_EngineContext.UpdateType == VirtualListUpdateType.FromBottomToTop)
		{
			num2 += m_Settings.Width;
		}
		m_EngineContext.LastElementCornerPosition = new Vector2(num2, position);
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
		else
		{
			lastElementCornerPosition -= new Vector2(width, 0f);
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
