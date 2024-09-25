using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.Controls;

[AddComponentMenu("Layout/Extended/Layout Element Extended")]
[RequireComponent(typeof(RectTransform))]
public class LayoutElementExtended : LayoutElement
{
	public LayoutElementExtendedValue MinWidthExtended = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue MinHeightExtended = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue PreferredWidthExtended = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue PreferredHeightExtended = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue FlexibleWidthExtended = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue FlexibleHeightExtended = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue MaxWidth = new LayoutElementExtendedValue();

	public LayoutElementExtendedValue MaxHeight = new LayoutElementExtendedValue();

	private RectTransform rectTransform;

	public RectTransform GetRectTransform
	{
		get
		{
			if (!rectTransform)
			{
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	public float Width
	{
		get
		{
			float result = 0f;
			if (MinWidthExtended.Enabled && !PreferredWidthExtended.Enabled)
			{
				result = MinWidth;
			}
			else if (!MinWidthExtended.Enabled && PreferredWidthExtended.Enabled)
			{
				result = PreferredWidth;
			}
			else if (MinWidthExtended.Enabled && PreferredWidthExtended.Enabled)
			{
				result = ((PreferredWidth > MinWidth) ? PreferredWidth : MinWidth);
			}
			return result;
		}
	}

	public float Height
	{
		get
		{
			float result = 0f;
			if (MinHeightExtended.Enabled && !PreferredHeightExtended.Enabled)
			{
				result = MinHeight;
			}
			else if (!MinHeightExtended.Enabled && PreferredHeightExtended.Enabled)
			{
				result = PreferredHeight;
			}
			else if (MinHeightExtended.Enabled && PreferredHeightExtended.Enabled)
			{
				result = ((PreferredHeight > MinHeight) ? PreferredHeight : MinHeight);
			}
			return result;
		}
	}

	private float MinWidth
	{
		get
		{
			float num = 0f;
			if (MinWidthExtended.Enabled)
			{
				MinWidthExtended.ProcessTargetValue(base.gameObject);
				num = MinWidthExtended.TargetValue;
				if (MaxWidth.Enabled)
				{
					MaxWidth.ProcessTargetValue(base.gameObject);
					if (num > MaxWidth.TargetValue)
					{
						num = MaxWidth.TargetValue;
					}
				}
			}
			return num;
		}
	}

	private float MinHeight
	{
		get
		{
			float num = 0f;
			if (MinHeightExtended.Enabled)
			{
				MinHeightExtended.ProcessTargetValue(base.gameObject);
				num = MinHeightExtended.TargetValue;
				if (MaxHeight.Enabled)
				{
					MaxHeight.ProcessTargetValue(base.gameObject);
					if (num > MaxHeight.TargetValue)
					{
						num = MaxHeight.TargetValue;
					}
				}
			}
			return num;
		}
	}

	private float PreferredWidth
	{
		get
		{
			float num = 0f;
			if (PreferredWidthExtended.Enabled)
			{
				PreferredWidthExtended.ProcessTargetValue(base.gameObject);
				num = PreferredWidthExtended.TargetValue;
				if (MaxWidth.Enabled)
				{
					MaxWidth.ProcessTargetValue(base.gameObject);
					if (num > MaxWidth.TargetValue)
					{
						num = MaxWidth.TargetValue;
					}
				}
			}
			return num;
		}
	}

	private float PreferredHeight
	{
		get
		{
			float num = 0f;
			if (PreferredHeightExtended.Enabled)
			{
				PreferredHeightExtended.ProcessTargetValue(base.gameObject);
				num = PreferredHeightExtended.TargetValue;
				if (MaxHeight.Enabled)
				{
					MaxHeight.ProcessTargetValue(base.gameObject);
					if (num > MaxHeight.TargetValue)
					{
						num = MaxHeight.TargetValue;
					}
				}
			}
			return num;
		}
	}

	private float FlexibleWidth
	{
		get
		{
			float result = 0f;
			if (FlexibleWidthExtended.Enabled)
			{
				FlexibleWidthExtended.ProcessTargetValue(base.gameObject);
				result = FlexibleWidthExtended.TargetValue;
			}
			return result;
		}
	}

	private float FlexibleHeight
	{
		get
		{
			float result = 0f;
			if (FlexibleHeightExtended.Enabled)
			{
				FlexibleHeightExtended.ProcessTargetValue(base.gameObject);
				result = FlexibleHeightExtended.TargetValue;
			}
			return result;
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
		minWidth = (MinWidthExtended.Enabled ? MinWidth : (-1f));
		preferredWidth = (PreferredWidthExtended.Enabled ? PreferredWidth : (-1f));
		flexibleWidth = (FlexibleWidthExtended.Enabled ? FlexibleWidth : (-1f));
		base.CalculateLayoutInputHorizontal();
	}

	public override void CalculateLayoutInputVertical()
	{
		minHeight = (MinHeightExtended.Enabled ? MinHeight : (-1f));
		preferredHeight = (PreferredHeightExtended.Enabled ? PreferredHeight : (-1f));
		flexibleHeight = (FlexibleHeightExtended.Enabled ? FlexibleHeight : (-1f));
		base.CalculateLayoutInputVertical();
	}
}
