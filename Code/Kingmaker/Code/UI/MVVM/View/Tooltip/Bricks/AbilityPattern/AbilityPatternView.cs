using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Pathfinding;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks.AbilityPattern;

public class AbilityPatternView : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_PatternContainer;

	[SerializeField]
	private float m_PatternContainerSize;

	[SerializeField]
	private float m_Spacing;

	[SerializeField]
	private int m_MinSizeInCells = 3;

	[SerializeField]
	private AbilityPatternCell m_AbilityPatternCellPrefab;

	private UIUtilityItem.UIPatternData m_PatternData;

	private readonly List<AbilityPatternCell> m_AbilityPatternCells = new List<AbilityPatternCell>();

	[SerializeField]
	private int m_ScaleMaxSize = 12;

	[SerializeField]
	private int m_ScaleThreshold = 20;

	public void Initialize(UIUtilityItem.UIPatternData patternData)
	{
		m_PatternData = patternData;
		UIUtilityItem.UIPatternData patternData2 = m_PatternData;
		int num;
		if (patternData2 != null)
		{
			PatternGridData patternCells = patternData2.PatternCells;
			num = ((patternCells.Count > 1) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		if (flag)
		{
			DrawCells();
		}
		base.gameObject.SetActive(flag);
	}

	public void Destroy()
	{
		ClearCells();
	}

	private void ClearCells()
	{
		m_AbilityPatternCells.ForEach(WidgetFactory.DisposeWidget);
		m_AbilityPatternCells.Clear();
	}

	[UnityEngine.ContextMenu("TestMe")]
	public void TestMe()
	{
		ClearCells();
		UIUtilityItem.UIPatternData patternData = m_PatternData;
		if (patternData != null)
		{
			PatternGridData patternCells = patternData.PatternCells;
			if (patternCells.Count > 1)
			{
				DrawCells();
			}
		}
	}

	private void DrawCells()
	{
		ClearCells();
		IntRect intRect = m_PatternData.PatternCells.Bounds;
		Vector2Int? ownerCell = m_PatternData.OwnerCell;
		if (ownerCell.HasValue)
		{
			Vector2Int valueOrDefault = ownerCell.GetValueOrDefault();
			intRect = intRect.ExpandToContain(valueOrDefault.x, valueOrDefault.y);
		}
		int num = Math.Max(intRect.Width, intRect.Height);
		if (num > m_ScaleThreshold)
		{
			int num2 = Math.Max(m_MinSizeInCells, num);
			float num3 = Math.Min(1f * (float)m_ScaleMaxSize / (float)num2, 1f);
			intRect.xmin = (int)Math.Floor((float)intRect.xmin * num3);
			intRect.xmax = (int)Math.Floor((float)intRect.xmax * num3);
			intRect.ymin = (int)Math.Floor((float)intRect.ymin * num3);
			intRect.ymax = (int)Math.Floor((float)intRect.ymax * num3);
		}
		int width = intRect.Width;
		int height = intRect.Height;
		int num4 = Math.Max(m_MinSizeInCells, Math.Max(intRect.Width, intRect.Height));
		float num5 = (m_PatternContainerSize - m_Spacing * (float)(num4 - 1)) / (float)num4;
		int num6 = num4 / 2 - width / 2;
		int num7 = num4 / 2 - height / 2;
		Vector2Int vector2Int = new Vector2Int(intRect.xmin - num6, intRect.ymin - num7);
		PatternGridData patternGridData = m_PatternData.PatternCells.Move(-vector2Int);
		Vector2Int? vector2Int2 = m_PatternData.OwnerCell - vector2Int;
		Vector2 vector = new Vector2(-0.5f * (float)((num4 - width) % 2), 0.5f * (float)((num4 - height) % 2));
		Vector2 zero = Vector2.zero;
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < num4; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				Vector2Int vector2Int3 = new Vector2Int(i, j);
				if (patternGridData.Contains(vector2Int3) || vector2Int3 == vector2Int2)
				{
					AbilityPatternCell widget = WidgetFactory.GetWidget(m_AbilityPatternCellPrefab);
					AbilityPatternCellType cellType = GetCellType(vector2Int3 + vector2Int);
					widget.Initialize(cellType);
					RectTransform rectTransform = (RectTransform)widget.transform;
					rectTransform.SetParent(m_PatternContainer, worldPositionStays: false);
					rectTransform.anchoredPosition = (new Vector2(vector2Int3.x, vector2Int3.y) + vector) * num5 + zero;
					rectTransform.sizeDelta = new Vector2(num5, num5);
					switch (cellType)
					{
					case AbilityPatternCellType.Start:
						list.Insert(0, rectTransform);
						break;
					case AbilityPatternCellType.Caster:
						list.Add(rectTransform);
						break;
					}
					m_AbilityPatternCells.Add(widget);
				}
				zero.y += m_Spacing;
			}
			foreach (Transform item in list)
			{
				item.SetAsLastSibling();
			}
			list.Clear();
			zero.x += m_Spacing;
			zero.y = 0f;
		}
	}

	private AbilityPatternCellType GetCellType(Vector2Int position)
	{
		AbilityPatternCellType result = AbilityPatternCellType.Common;
		if (m_PatternData.OwnerCell.HasValue && m_PatternData.OwnerCell.Value == position)
		{
			result = AbilityPatternCellType.Caster;
		}
		else if (m_PatternData.MainCellsIndexes.Contains(position))
		{
			result = AbilityPatternCellType.Start;
		}
		return result;
	}
}
