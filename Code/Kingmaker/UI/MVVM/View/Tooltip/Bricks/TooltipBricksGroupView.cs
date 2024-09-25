using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBricksGroupView : TooltipBaseBrickView<TooltipBricksGroupVM>, IConsoleTooltipBrick
{
	[SerializeField]
	private GameObject m_Background;

	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private VerticalLayoutGroup m_VerticalLayoutGroup;

	[SerializeField]
	private HorizontalLayoutGroup m_HorizontalLayoutGroup;

	[SerializeField]
	private GridLayoutGroup m_GridLayoutGroup;

	private bool m_IsDirty;

	private readonly List<IConsoleEntity> m_NavChildren = new List<IConsoleEntity>();

	private Vector2 m_InitSpacing;

	private float? m_InitCellWidth;

	private Color m_InitBgrColor;

	protected override void BindViewImplementation()
	{
		float valueOrDefault = m_InitCellWidth.GetValueOrDefault();
		if (!m_InitCellWidth.HasValue)
		{
			valueOrDefault = m_GridLayoutGroup.cellSize.x;
			m_InitCellWidth = valueOrDefault;
		}
		m_InitSpacing = m_GridLayoutGroup.spacing;
		if ((bool)m_BackgroundImage)
		{
			m_InitBgrColor = m_BackgroundImage.color;
		}
		if ((bool)m_Background)
		{
			m_Background.SetActive(base.ViewModel.HasBackground);
		}
		TooltipBricksGroupLayoutType? tooltipBricksGroupLayoutType = base.ViewModel.LayoutParams?.LayoutType;
		m_VerticalLayoutGroup.gameObject.SetActive(tooltipBricksGroupLayoutType == TooltipBricksGroupLayoutType.Vertical);
		m_HorizontalLayoutGroup.gameObject.SetActive(tooltipBricksGroupLayoutType == TooltipBricksGroupLayoutType.Horizontal);
		m_GridLayoutGroup.gameObject.SetActive(tooltipBricksGroupLayoutType == TooltipBricksGroupLayoutType.Grid);
		if (tooltipBricksGroupLayoutType == TooltipBricksGroupLayoutType.Grid)
		{
			m_GridLayoutGroup.constraintCount = base.ViewModel.LayoutParams.ColumnCount;
		}
		if (base.ViewModel.BackgroundColor.HasValue && (bool)m_BackgroundImage)
		{
			m_BackgroundImage.color = base.ViewModel.BackgroundColor.Value;
		}
		if (base.ViewModel.LayoutParams != null)
		{
			m_VerticalLayoutGroup.padding = base.ViewModel.LayoutParams.Padding;
			m_HorizontalLayoutGroup.padding = base.ViewModel.LayoutParams.Padding;
			m_GridLayoutGroup.padding = base.ViewModel.LayoutParams.Padding;
			m_GridLayoutGroup.spacing = ((base.ViewModel.LayoutParams.Spacing != default(Vector2)) ? base.ViewModel.LayoutParams.Spacing : m_InitSpacing);
			if (base.ViewModel.LayoutParams.CellSize.HasValue)
			{
				m_GridLayoutGroup.cellSize = base.ViewModel.LayoutParams.CellSize.Value;
			}
		}
		else
		{
			m_VerticalLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
			m_HorizontalLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
			m_GridLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
		}
		DelayedInvoker.InvokeInFrames(FixCellWidth, 1);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NavChildren.Clear();
		m_GridLayoutGroup.spacing = m_InitSpacing;
		if ((bool)m_BackgroundImage)
		{
			m_BackgroundImage.color = m_InitBgrColor;
		}
	}

	public void AddChild(RectTransform childTransform)
	{
		TooltipBricksGroupLayoutParams layoutParams = base.ViewModel.LayoutParams;
		if (layoutParams != null && layoutParams.LayoutType == TooltipBricksGroupLayoutType.Grid)
		{
			m_IsDirty = true;
		}
		Transform activeTransform = GetActiveTransform();
		childTransform.SetParent(activeTransform, worldPositionStays: false);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		TooltipBricksGroupLayoutParams layoutParams2 = base.ViewModel.LayoutParams;
		if (layoutParams2 != null && layoutParams2.PreferredElementHeight > 0f)
		{
			childTransform.GetComponent<IUpdateContainerElements>()?.UpdateElements(base.ViewModel.LayoutParams.PreferredElementHeight.Value);
		}
	}

	public void AddNavChild(IConsoleEntity entity)
	{
		m_NavChildren.Add(entity);
	}

	private Transform GetActiveTransform()
	{
		if (base.ViewModel.LayoutParams != null)
		{
			return base.ViewModel.LayoutParams.LayoutType switch
			{
				TooltipBricksGroupLayoutType.Vertical => m_VerticalLayoutGroup.transform, 
				TooltipBricksGroupLayoutType.Horizontal => m_HorizontalLayoutGroup.transform, 
				TooltipBricksGroupLayoutType.Grid => m_GridLayoutGroup.transform, 
				TooltipBricksGroupLayoutType.None => base.transform, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		return base.transform;
	}

	private void Update()
	{
		if (m_IsDirty && base.ViewModel != null)
		{
			m_IsDirty = false;
			TooltipBricksGroupLayoutParams layoutParams = base.ViewModel.LayoutParams;
			if (layoutParams != null && layoutParams.LayoutType == TooltipBricksGroupLayoutType.Grid && m_GridLayoutGroup.transform.childCount != 0 && !base.ViewModel.LayoutParams.CellSize.HasValue)
			{
				float childHeight = GetChildHeight(m_GridLayoutGroup.transform.GetChild(m_GridLayoutGroup.transform.childCount - 1) as RectTransform);
				m_GridLayoutGroup.cellSize = new Vector2(m_GridLayoutGroup.cellSize.x, childHeight);
			}
		}
	}

	private void FixCellWidth()
	{
		if (base.ViewModel == null)
		{
			return;
		}
		TooltipBricksGroupLayoutParams layoutParams = base.ViewModel.LayoutParams;
		if (layoutParams != null && layoutParams.CellSize.HasValue)
		{
			return;
		}
		RectTransform component = base.transform.parent.GetComponent<RectTransform>();
		if (component == null)
		{
			return;
		}
		float num = component.rect.width;
		LayoutElement component2 = component.GetComponent<LayoutElement>();
		if ((bool)component2 && component2.preferredWidth > 0f)
		{
			num = component2.preferredWidth;
		}
		else
		{
			LayoutGroup component3 = component.GetComponent<LayoutGroup>();
			if ((bool)component3)
			{
				num -= (float)(component3.padding.left + component3.padding.right);
			}
		}
		LayoutGroup component4 = GetComponent<LayoutGroup>();
		if ((bool)component4)
		{
			num -= (float)(component4.padding.left + component4.padding.right);
		}
		float x = (num - (float)m_GridLayoutGroup.padding.left - (float)m_GridLayoutGroup.padding.right - m_GridLayoutGroup.spacing.x * (float)(m_GridLayoutGroup.constraintCount - 1)) / (float)m_GridLayoutGroup.constraintCount;
		m_GridLayoutGroup.cellSize = new Vector2(x, m_GridLayoutGroup.cellSize.y);
	}

	private float GetChildHeight(RectTransform childTransform)
	{
		if (base.ViewModel.LayoutParams.PreferredElementHeight.HasValue)
		{
			return base.ViewModel.LayoutParams.PreferredElementHeight.Value;
		}
		LayoutElement component = childTransform.GetComponent<LayoutElement>();
		if (component == null)
		{
			return childTransform.rect.height;
		}
		if (component.preferredHeight > 0f)
		{
			return component.preferredHeight;
		}
		if (component.minHeight > 0f)
		{
			return component.minHeight;
		}
		return childTransform.rect.height;
	}

	public IConsoleEntity GetConsoleEntity()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		switch (base.ViewModel.LayoutParams?.LayoutType)
		{
		case TooltipBricksGroupLayoutType.Vertical:
			gridConsoleNavigationBehaviour.SetEntitiesVertical(m_NavChildren);
			break;
		case TooltipBricksGroupLayoutType.Horizontal:
			gridConsoleNavigationBehaviour.SetEntitiesHorizontal(m_NavChildren);
			break;
		case TooltipBricksGroupLayoutType.Grid:
			gridConsoleNavigationBehaviour.SetEntitiesGrid(m_NavChildren, base.ViewModel.LayoutParams.ColumnCount);
			break;
		case null:
		case TooltipBricksGroupLayoutType.None:
			gridConsoleNavigationBehaviour.SetEntitiesVertical(m_NavChildren);
			break;
		}
		return gridConsoleNavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
