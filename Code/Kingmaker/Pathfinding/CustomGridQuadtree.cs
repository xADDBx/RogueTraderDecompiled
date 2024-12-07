using Pathfinding;

namespace Kingmaker.Pathfinding;

public class CustomGridQuadtree
{
	public delegate bool CheckPairwiseConsolidationDelegate(CustomGridQuadtree leaf1, CustomGridQuadtree leaf2);

	public delegate bool CheckConsolidationDelegate(CustomGridQuadtree leaf1, CustomGridQuadtree leaf2, CustomGridQuadtree lb, CustomGridQuadtree rb);

	public CustomGridGraph Graph { get; }

	public IntRect Rect { get; }

	public CustomGridQuadtree LeftTopChild { get; set; }

	public CustomGridQuadtree RightTopChild { get; set; }

	public CustomGridQuadtree LeftBottomChild { get; set; }

	public CustomGridQuadtree RightBottomChild { get; set; }

	public bool IsFullyNotWalkable { get; set; }

	public bool IsLeaf
	{
		get
		{
			if (LeftTopChild == null && RightTopChild == null && LeftBottomChild == null)
			{
				return RightBottomChild == null;
			}
			return false;
		}
	}

	public CustomGridQuadtree(CustomGridGraph graph, bool shouldDivideDown = true)
	{
		Graph = graph;
		Rect = new IntRect(0, 0, graph.Width - 1, graph.Depth - 1);
		if (shouldDivideDown)
		{
			DivideDown();
		}
	}

	public CustomGridQuadtree(CustomGridGraph graph, IntRect rect)
	{
		Graph = graph;
		Rect = rect;
	}

	public void DivideDown()
	{
		if (Rect.Width <= 1 && Rect.Height <= 1)
		{
			IsFullyNotWalkable = !(Graph.GetNode(Rect.xmin, Rect.ymin)?.StaticWalkable ?? false);
			return;
		}
		if (Rect.Width <= 1)
		{
			int num = Rect.Height / 2;
			IntRect intRect = new IntRect(Rect.xmin, Rect.ymin, Rect.xmax, Rect.ymin + num - 1);
			IntRect intRect2 = new IntRect(Rect.xmin, Rect.ymin + num, Rect.xmax, Rect.ymax);
			LeftTopChild = (Rect.Contains(intRect) ? new CustomGridQuadtree(Graph, intRect) : null);
			RightTopChild = (Rect.Contains(intRect2) ? new CustomGridQuadtree(Graph, intRect2) : null);
		}
		else if (Rect.Height <= 1)
		{
			int num2 = Rect.Width / 2;
			IntRect intRect3 = new IntRect(Rect.xmin, Rect.ymin, Rect.xmin + num2 - 1, Rect.ymax);
			IntRect intRect4 = new IntRect(Rect.xmin + num2, Rect.ymin, Rect.xmax, Rect.ymax);
			LeftTopChild = (Rect.Contains(intRect3) ? new CustomGridQuadtree(Graph, intRect3) : null);
			RightTopChild = (Rect.Contains(intRect4) ? new CustomGridQuadtree(Graph, intRect4) : null);
		}
		else
		{
			int num3 = Rect.Width / 2;
			int num4 = Rect.Height / 2;
			LeftTopChild = new CustomGridQuadtree(Graph, new IntRect(Rect.xmin, Rect.ymin, Rect.xmin + num3 - 1, Rect.ymin + num4 - 1));
			RightTopChild = new CustomGridQuadtree(Graph, new IntRect(Rect.xmin + num3, Rect.ymin, Rect.xmax, Rect.ymin + num4 - 1));
			LeftBottomChild = new CustomGridQuadtree(Graph, new IntRect(Rect.xmin, Rect.ymin + num4, Rect.xmin + num3 - 1, Rect.ymax));
			RightBottomChild = new CustomGridQuadtree(Graph, new IntRect(Rect.xmin + num3, Rect.ymin + num4, Rect.xmax, Rect.ymax));
		}
		LeftTopChild?.DivideDown();
		RightTopChild?.DivideDown();
		LeftBottomChild?.DivideDown();
		RightBottomChild?.DivideDown();
	}

	public void ConsolidateUp(CheckConsolidationDelegate checkDelegate, int maxRectSide = 0)
	{
		if (IsLeaf)
		{
			return;
		}
		LeftTopChild.ConsolidateUp(checkDelegate, maxRectSide);
		RightTopChild.ConsolidateUp(checkDelegate, maxRectSide);
		LeftBottomChild.ConsolidateUp(checkDelegate, maxRectSide);
		RightBottomChild.ConsolidateUp(checkDelegate, maxRectSide);
		if ((maxRectSide <= 0 || (Rect.Width < maxRectSide && Rect.Height < maxRectSide)) && LeftTopChild.IsLeaf && RightTopChild.IsLeaf && LeftBottomChild.IsLeaf && RightBottomChild.IsLeaf)
		{
			if (LeftTopChild.IsFullyNotWalkable && RightTopChild.IsFullyNotWalkable && LeftBottomChild.IsFullyNotWalkable && RightBottomChild.IsFullyNotWalkable)
			{
				LeftTopChild = null;
				RightTopChild = null;
				LeftBottomChild = null;
				RightBottomChild = null;
				IsFullyNotWalkable = true;
			}
			if (checkDelegate(LeftTopChild, RightTopChild, LeftBottomChild, RightBottomChild))
			{
				LeftTopChild = null;
				RightTopChild = null;
				LeftBottomChild = null;
				RightBottomChild = null;
			}
		}
	}

	public void ConsolidateUpPairwise(CheckPairwiseConsolidationDelegate checkDelegate, int maxRectSide = 0)
	{
		if (IsLeaf)
		{
			return;
		}
		LeftTopChild?.ConsolidateUpPairwise(checkDelegate, maxRectSide);
		RightTopChild?.ConsolidateUpPairwise(checkDelegate, maxRectSide);
		LeftBottomChild?.ConsolidateUpPairwise(checkDelegate, maxRectSide);
		RightBottomChild?.ConsolidateUpPairwise(checkDelegate, maxRectSide);
		if (maxRectSide <= 0 || (Rect.Width < maxRectSide && Rect.Height < maxRectSide))
		{
			bool flag = false;
			bool flag2 = CanMerge(LeftTopChild, RightTopChild, checkDelegate);
			bool flag3 = CanMerge(LeftTopChild, LeftBottomChild, checkDelegate);
			bool flag4 = CanMerge(RightTopChild, RightBottomChild, checkDelegate);
			bool flag5 = CanMerge(LeftBottomChild, RightBottomChild, checkDelegate);
			if (flag2 && flag5)
			{
				LeftTopChild = Merge(LeftTopChild, RightTopChild);
				RightTopChild = Merge(LeftBottomChild, RightBottomChild);
				LeftBottomChild = null;
				RightBottomChild = null;
				flag = true;
			}
			else if (flag3 && flag4)
			{
				LeftTopChild = Merge(LeftTopChild, LeftBottomChild);
				RightTopChild = Merge(RightTopChild, RightBottomChild);
				LeftBottomChild = null;
				RightBottomChild = null;
				flag = true;
			}
			else if (flag2)
			{
				LeftTopChild = Merge(LeftTopChild, RightTopChild);
				RightTopChild = null;
			}
			else if (flag3)
			{
				LeftTopChild = Merge(LeftTopChild, LeftBottomChild);
				LeftBottomChild = null;
			}
			else if (flag4)
			{
				RightTopChild = Merge(RightTopChild, RightBottomChild);
				RightBottomChild = null;
			}
			else if (flag5)
			{
				LeftBottomChild = Merge(LeftBottomChild, RightBottomChild);
				RightBottomChild = null;
			}
			if (flag && CanMerge(LeftTopChild, RightTopChild, checkDelegate))
			{
				CustomGridQuadtree leftTopChild = LeftTopChild;
				IsFullyNotWalkable = (leftTopChild == null || leftTopChild.IsFullyNotWalkable) && (RightTopChild?.IsFullyNotWalkable ?? true);
				LeftTopChild = null;
				RightTopChild = null;
			}
		}
	}

	private bool CanMerge(CustomGridQuadtree tree1, CustomGridQuadtree tree2, CheckPairwiseConsolidationDelegate checkDelegate)
	{
		if (tree1 == null || tree2 == null)
		{
			return true;
		}
		if (!tree1.IsLeaf || !tree2.IsLeaf)
		{
			return false;
		}
		if (tree1.IsFullyNotWalkable && tree2.IsFullyNotWalkable)
		{
			return true;
		}
		return checkDelegate(tree1, tree2);
	}

	private CustomGridQuadtree Merge(CustomGridQuadtree tree1, CustomGridQuadtree tree2)
	{
		if (tree1 == null && tree2 == null)
		{
			return null;
		}
		if (tree1 == null)
		{
			return tree2;
		}
		if (tree2 == null)
		{
			return tree1;
		}
		return new CustomGridQuadtree(Graph, IntRect.Union(tree1.Rect, tree2.Rect))
		{
			IsFullyNotWalkable = (tree1.IsFullyNotWalkable && tree2.IsFullyNotWalkable)
		};
	}

	public CustomGridQuadtree Find(int x, int y)
	{
		if (!Contains(x, y))
		{
			return null;
		}
		if (IsLeaf)
		{
			return this;
		}
		object obj = LeftTopChild?.Find(x, y);
		if (obj == null)
		{
			obj = RightTopChild?.Find(x, y);
			if (obj == null)
			{
				obj = LeftBottomChild?.Find(x, y);
				if (obj == null)
				{
					CustomGridQuadtree rightBottomChild = RightBottomChild;
					if (rightBottomChild == null)
					{
						return null;
					}
					obj = rightBottomChild.Find(x, y);
				}
			}
		}
		return (CustomGridQuadtree)obj;
	}

	public bool Contains(int x, int y)
	{
		return Rect.Contains(x, y);
	}

	public override string ToString()
	{
		return string.Format("{0}Node: {1}", IsLeaf ? "Leaf" : "", Rect);
	}
}
