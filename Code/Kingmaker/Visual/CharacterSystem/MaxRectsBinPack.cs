using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class MaxRectsBinPack
{
	public enum FreeRectChoiceHeuristic
	{
		RectBestShortSideFit,
		RectBestLongSideFit,
		RectBestAreaFit,
		RectBottomLeftRule,
		RectContactPointRule
	}

	public int binWidth;

	public int binHeight;

	public bool allowRotations;

	public List<Rect> usedRectangles = new List<Rect>();

	public List<Rect> freeRectangles = new List<Rect>();

	public MaxRectsBinPack(int width, int height, bool rotations)
	{
		Init(width, height, rotations);
	}

	public void Init(int width, int height, bool rotations)
	{
		binWidth = width;
		binHeight = height;
		allowRotations = rotations;
		Rect item = default(Rect);
		item.x = 0f;
		item.y = 0f;
		item.width = width;
		item.height = height;
		usedRectangles.Clear();
		freeRectangles.Clear();
		freeRectangles.Add(item);
	}

	public Rect Insert(int width, int height, FreeRectChoiceHeuristic method)
	{
		Rect usedNode = default(Rect);
		int bestShortSideFit = 0;
		int bestLongSideFit = 0;
		switch (method)
		{
		case FreeRectChoiceHeuristic.RectBestShortSideFit:
			usedNode = FindPositionForNewNodeBestShortSideFit(width, height, ref bestShortSideFit, ref bestLongSideFit);
			break;
		case FreeRectChoiceHeuristic.RectBottomLeftRule:
			usedNode = FindPositionForNewNodeBottomLeft(width, height, ref bestShortSideFit, ref bestLongSideFit);
			break;
		case FreeRectChoiceHeuristic.RectContactPointRule:
			usedNode = FindPositionForNewNodeContactPoint(width, height, ref bestShortSideFit);
			break;
		case FreeRectChoiceHeuristic.RectBestLongSideFit:
			usedNode = FindPositionForNewNodeBestLongSideFit(width, height, ref bestLongSideFit, ref bestShortSideFit);
			break;
		case FreeRectChoiceHeuristic.RectBestAreaFit:
			usedNode = FindPositionForNewNodeBestAreaFit(width, height, ref bestShortSideFit, ref bestLongSideFit);
			break;
		}
		if (usedNode.height == 0f)
		{
			return usedNode;
		}
		int num = freeRectangles.Count;
		for (int i = 0; i < num; i++)
		{
			if (SplitFreeNode(freeRectangles[i], ref usedNode))
			{
				freeRectangles.RemoveAt(i);
				i--;
				num--;
			}
		}
		PruneFreeList();
		usedRectangles.Add(usedNode);
		return usedNode;
	}

	public void Insert(List<Rect> rects, List<Rect> dst, FreeRectChoiceHeuristic method)
	{
		dst.Clear();
		while (rects.Count > 0)
		{
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int num3 = -1;
			Rect node = default(Rect);
			for (int i = 0; i < rects.Count; i++)
			{
				int score = 0;
				int score2 = 0;
				Rect rect = ScoreRect((int)rects[i].width, (int)rects[i].height, method, ref score, ref score2);
				if (score < num || (score == num && score2 < num2))
				{
					num = score;
					num2 = score2;
					node = rect;
					num3 = i;
				}
			}
			if (num3 == -1)
			{
				break;
			}
			PlaceRect(node);
			rects.RemoveAt(num3);
		}
	}

	private void PlaceRect(Rect node)
	{
		int num = freeRectangles.Count;
		for (int i = 0; i < num; i++)
		{
			if (SplitFreeNode(freeRectangles[i], ref node))
			{
				freeRectangles.RemoveAt(i);
				i--;
				num--;
			}
		}
		PruneFreeList();
		usedRectangles.Add(node);
	}

	private Rect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2)
	{
		Rect result = default(Rect);
		score1 = int.MaxValue;
		score2 = int.MaxValue;
		switch (method)
		{
		case FreeRectChoiceHeuristic.RectBestShortSideFit:
			result = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
			break;
		case FreeRectChoiceHeuristic.RectBottomLeftRule:
			result = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
			break;
		case FreeRectChoiceHeuristic.RectContactPointRule:
			result = FindPositionForNewNodeContactPoint(width, height, ref score1);
			score1 = -score1;
			break;
		case FreeRectChoiceHeuristic.RectBestLongSideFit:
			result = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
			break;
		case FreeRectChoiceHeuristic.RectBestAreaFit:
			result = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
			break;
		}
		if (result.height == 0f)
		{
			score1 = int.MaxValue;
			score2 = int.MaxValue;
		}
		return result;
	}

	public float Occupancy()
	{
		ulong num = 0uL;
		for (int i = 0; i < usedRectangles.Count; i++)
		{
			num += (uint)usedRectangles[i].width * (uint)usedRectangles[i].height;
		}
		return (float)num / (float)(binWidth * binHeight);
	}

	private Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
	{
		Rect result = default(Rect);
		bestY = int.MaxValue;
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			if (freeRectangles[i].width >= (float)width && freeRectangles[i].height >= (float)height)
			{
				int num = (int)freeRectangles[i].y + height;
				if (num < bestY || (num == bestY && freeRectangles[i].x < (float)bestX))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = width;
					result.height = height;
					bestY = num;
					bestX = (int)freeRectangles[i].x;
				}
			}
			if (allowRotations && freeRectangles[i].width >= (float)height && freeRectangles[i].height >= (float)width)
			{
				int num2 = (int)freeRectangles[i].y + width;
				if (num2 < bestY || (num2 == bestY && freeRectangles[i].x < (float)bestX))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = height;
					result.height = width;
					bestY = num2;
					bestX = (int)freeRectangles[i].x;
				}
			}
		}
		return result;
	}

	private Rect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
	{
		Rect result = default(Rect);
		bestShortSideFit = int.MaxValue;
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			if (freeRectangles[i].width >= (float)width && freeRectangles[i].height >= (float)height)
			{
				int a = Mathf.Abs((int)freeRectangles[i].width - width);
				int b = Mathf.Abs((int)freeRectangles[i].height - height);
				int num = Mathf.Min(a, b);
				int num2 = Mathf.Max(a, b);
				if (num < bestShortSideFit || (num == bestShortSideFit && num2 < bestLongSideFit))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = width;
					result.height = height;
					bestShortSideFit = num;
					bestLongSideFit = num2;
				}
			}
			if (allowRotations && freeRectangles[i].width >= (float)height && freeRectangles[i].height >= (float)width)
			{
				int a2 = Mathf.Abs((int)freeRectangles[i].width - height);
				int b2 = Mathf.Abs((int)freeRectangles[i].height - width);
				int num3 = Mathf.Min(a2, b2);
				int num4 = Mathf.Max(a2, b2);
				if (num3 < bestShortSideFit || (num3 == bestShortSideFit && num4 < bestLongSideFit))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = height;
					result.height = width;
					bestShortSideFit = num3;
					bestLongSideFit = num4;
				}
			}
		}
		return result;
	}

	private Rect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
	{
		Rect result = default(Rect);
		bestLongSideFit = int.MaxValue;
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			if (freeRectangles[i].width >= (float)width && freeRectangles[i].height >= (float)height)
			{
				int a = Mathf.Abs((int)freeRectangles[i].width - width);
				int b = Mathf.Abs((int)freeRectangles[i].height - height);
				int num = Mathf.Min(a, b);
				int num2 = Mathf.Max(a, b);
				if (num2 < bestLongSideFit || (num2 == bestLongSideFit && num < bestShortSideFit))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = width;
					result.height = height;
					bestShortSideFit = num;
					bestLongSideFit = num2;
				}
			}
			if (allowRotations && freeRectangles[i].width >= (float)height && freeRectangles[i].height >= (float)width)
			{
				int a2 = Mathf.Abs((int)freeRectangles[i].width - height);
				int b2 = Mathf.Abs((int)freeRectangles[i].height - width);
				int num3 = Mathf.Min(a2, b2);
				int num4 = Mathf.Max(a2, b2);
				if (num4 < bestLongSideFit || (num4 == bestLongSideFit && num3 < bestShortSideFit))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = height;
					result.height = width;
					bestShortSideFit = num3;
					bestLongSideFit = num4;
				}
			}
		}
		return result;
	}

	private Rect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
	{
		Rect result = default(Rect);
		bestAreaFit = int.MaxValue;
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			int num = (int)freeRectangles[i].width * (int)freeRectangles[i].height - width * height;
			if (freeRectangles[i].width >= (float)width && freeRectangles[i].height >= (float)height)
			{
				int a = Mathf.Abs((int)freeRectangles[i].width - width);
				int b = Mathf.Abs((int)freeRectangles[i].height - height);
				int num2 = Mathf.Min(a, b);
				if (num < bestAreaFit || (num == bestAreaFit && num2 < bestShortSideFit))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = width;
					result.height = height;
					bestShortSideFit = num2;
					bestAreaFit = num;
				}
			}
			if (allowRotations && freeRectangles[i].width >= (float)height && freeRectangles[i].height >= (float)width)
			{
				int a2 = Mathf.Abs((int)freeRectangles[i].width - height);
				int b2 = Mathf.Abs((int)freeRectangles[i].height - width);
				int num3 = Mathf.Min(a2, b2);
				if (num < bestAreaFit || (num == bestAreaFit && num3 < bestShortSideFit))
				{
					result.x = freeRectangles[i].x;
					result.y = freeRectangles[i].y;
					result.width = height;
					result.height = width;
					bestShortSideFit = num3;
					bestAreaFit = num;
				}
			}
		}
		return result;
	}

	private int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
	{
		if (i1end < i2start || i2end < i1start)
		{
			return 0;
		}
		return Mathf.Min(i1end, i2end) - Mathf.Max(i1start, i2start);
	}

	private int ContactPointScoreNode(int x, int y, int width, int height)
	{
		int num = 0;
		if (x == 0 || x + width == binWidth)
		{
			num += height;
		}
		if (y == 0 || y + height == binHeight)
		{
			num += width;
		}
		for (int i = 0; i < usedRectangles.Count; i++)
		{
			if (usedRectangles[i].x == (float)(x + width) || usedRectangles[i].x + usedRectangles[i].width == (float)x)
			{
				num += CommonIntervalLength((int)usedRectangles[i].y, (int)usedRectangles[i].y + (int)usedRectangles[i].height, y, y + height);
			}
			if (usedRectangles[i].y == (float)(y + height) || usedRectangles[i].y + usedRectangles[i].height == (float)y)
			{
				num += CommonIntervalLength((int)usedRectangles[i].x, (int)usedRectangles[i].x + (int)usedRectangles[i].width, x, x + width);
			}
		}
		return num;
	}

	private Rect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
	{
		Rect result = default(Rect);
		bestContactScore = -1;
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			if (freeRectangles[i].width >= (float)width && freeRectangles[i].height >= (float)height)
			{
				int num = ContactPointScoreNode((int)freeRectangles[i].x, (int)freeRectangles[i].y, width, height);
				if (num > bestContactScore)
				{
					result.x = (int)freeRectangles[i].x;
					result.y = (int)freeRectangles[i].y;
					result.width = width;
					result.height = height;
					bestContactScore = num;
				}
			}
			if (allowRotations && freeRectangles[i].width >= (float)height && freeRectangles[i].height >= (float)width)
			{
				int num2 = ContactPointScoreNode((int)freeRectangles[i].x, (int)freeRectangles[i].y, height, width);
				if (num2 > bestContactScore)
				{
					result.x = (int)freeRectangles[i].x;
					result.y = (int)freeRectangles[i].y;
					result.width = height;
					result.height = width;
					bestContactScore = num2;
				}
			}
		}
		return result;
	}

	private bool SplitFreeNode(Rect freeNode, ref Rect usedNode)
	{
		if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x || usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
		{
			return false;
		}
		if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
		{
			if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
			{
				Rect item = freeNode;
				item.height = usedNode.y - item.y;
				freeRectangles.Add(item);
			}
			if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
			{
				Rect item2 = freeNode;
				item2.y = usedNode.y + usedNode.height;
				item2.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
				freeRectangles.Add(item2);
			}
		}
		if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
		{
			if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
			{
				Rect item3 = freeNode;
				item3.width = usedNode.x - item3.x;
				freeRectangles.Add(item3);
			}
			if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
			{
				Rect item4 = freeNode;
				item4.x = usedNode.x + usedNode.width;
				item4.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
				freeRectangles.Add(item4);
			}
		}
		return true;
	}

	private void PruneFreeList()
	{
		for (int i = 0; i < freeRectangles.Count; i++)
		{
			for (int j = i + 1; j < freeRectangles.Count; j++)
			{
				if (IsContainedIn(freeRectangles[i], freeRectangles[j]))
				{
					freeRectangles.RemoveAt(i);
					i--;
					break;
				}
				if (IsContainedIn(freeRectangles[j], freeRectangles[i]))
				{
					freeRectangles.RemoveAt(j);
					j--;
				}
			}
		}
	}

	private bool IsContainedIn(Rect a, Rect b)
	{
		if (a.x >= b.x && a.y >= b.y && a.x + a.width <= b.x + b.width)
		{
			return a.y + a.height <= b.y + b.height;
		}
		return false;
	}
}
