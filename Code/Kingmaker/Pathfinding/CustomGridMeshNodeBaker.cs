using UnityEngine;

namespace Kingmaker.Pathfinding;

internal readonly struct CustomGridMeshNodeBaker
{
	private struct AverageHeightCalculator
	{
		private float heightSum;

		private int heightSumDivisor;

		private IntermediateNodeData[] nodeDataArray;

		public AverageHeightCalculator(IntermediateNodeData[] nodeDataArray)
		{
			this = default(AverageHeightCalculator);
			this.nodeDataArray = nodeDataArray;
		}

		public void AddInfluence(float value)
		{
			heightSum += value;
			heightSumDivisor++;
		}

		public void AddInfluence(CustomGridNodeBase n)
		{
			if (n != null)
			{
				heightSum += n.Vector3Position.y;
				heightSumDivisor++;
			}
		}

		public void AddInfluence(in IntermediateNodeData n)
		{
			if (n.flat)
			{
				heightSum += n.height;
				heightSumDivisor++;
			}
		}

		public void AddInfluence(int index)
		{
			if (index >= 0)
			{
				ref IntermediateNodeData reference = ref nodeDataArray[index];
				if (reference.flat)
				{
					heightSum += reference.height;
					heightSumDivisor++;
				}
			}
		}

		public void Resolve(ref float result)
		{
			if (heightSumDivisor > 0)
			{
				result = heightSum / (float)heightSumDivisor;
			}
			heightSum = 0f;
			heightSumDivisor = 0;
		}
	}

	private struct IntermediateNodeData
	{
		public float height;

		public float cornerHeightSW;

		public float cornerHeightSE;

		public float cornerHeightNW;

		public float cornerHeightNE;

		public bool flat;

		public CornerInfluence cornerInfluenceSW;

		public CornerInfluence cornerInfluenceSE;

		public CornerInfluence cornerInfluenceNW;

		public CornerInfluence cornerInfluenceNE;
	}

	private struct CornerInfluence
	{
		public int influenceIndexH;

		public int influenceIndexV;

		public int influenceIndexD;
	}

	private readonly struct CustomGridNodeDirection
	{
		public static readonly CustomGridNodeDirection S = new CustomGridNodeDirection(0);

		public static readonly CustomGridNodeDirection E = new CustomGridNodeDirection(1);

		public static readonly CustomGridNodeDirection N = new CustomGridNodeDirection(2);

		public static readonly CustomGridNodeDirection W = new CustomGridNodeDirection(3);

		private readonly int m_Value;

		private CustomGridNodeDirection(int value)
		{
			m_Value = value;
		}

		public static implicit operator int(CustomGridNodeDirection value)
		{
			return value.m_Value;
		}

		public static CustomGridNodeDirection operator -(CustomGridNodeDirection value)
		{
			return new CustomGridNodeDirection((value.m_Value + 2) % 4);
		}
	}

	private readonly CustomGridGraph m_Graph;

	private readonly bool m_FlatFilterEnabled;

	private readonly float m_FlatFilterThreshold;

	public CustomGridMeshNodeBaker(CustomGridGraph graph, bool flatFilterEnabled = true, float flatFilterThreshold = 0.05f)
	{
		m_Graph = graph;
		m_FlatFilterEnabled = flatFilterEnabled;
		m_FlatFilterThreshold = flatFilterThreshold;
	}

	public CustomGridMeshNode[] Bake()
	{
		IntermediateNodeData[] array = new IntermediateNodeData[m_Graph.nodes.Length];
		PopulateIntermediateNodeData(array);
		if (m_FlatFilterEnabled)
		{
			FlattenCorners(array);
		}
		CustomGridMeshNode[] array2 = new CustomGridMeshNode[m_Graph.nodes.Length];
		int i = 0;
		for (int num = m_Graph.nodes.Length; i < num; i++)
		{
			ref IntermediateNodeData reference = ref array[i];
			float height = reference.height;
			_ = m_Graph.nodes[i];
			array2[i].Pack(height, reference.cornerHeightSW, reference.cornerHeightSE, reference.cornerHeightNW, reference.cornerHeightNE);
		}
		return array2;
	}

	private void GetInfluenceNodes(CustomGridNodeBase originNode, CustomGridNodeDirection hDirection, CustomGridNodeDirection vDirection, out CustomGridNodeBase influenceH, out CustomGridNodeBase influenceV, out CustomGridNodeBase influenceD)
	{
		CustomGridNodeBase hAdjacent = GetAdjacentNode(originNode, hDirection);
		CustomGridNodeBase vAdjacent = GetAdjacentNode(originNode, vDirection);
		CustomGridNodeBase dAdjacent = GetAdjacentNode(hAdjacent, vDirection) ?? GetAdjacentNode(vAdjacent, hDirection);
		influenceH = null;
		influenceV = null;
		influenceD = null;
		if (hAdjacent != null && HasOHConnection())
		{
			influenceH = hAdjacent;
			if (dAdjacent != null && HasHDConnection())
			{
				influenceD = dAdjacent;
				if (vAdjacent != null && HasVDConnection())
				{
					influenceV = vAdjacent;
					return;
				}
			}
		}
		if (vAdjacent == null || !HasOVConnection())
		{
			return;
		}
		influenceV = vAdjacent;
		if (influenceD == null && dAdjacent != null && HasVDConnection())
		{
			influenceD = dAdjacent;
			if (influenceH == null && hAdjacent != null && HasHDConnection())
			{
				influenceH = hAdjacent;
			}
		}
		static CustomGridNodeBase GetAdjacentNode(CustomGridNodeBase origin, int direction)
		{
			return origin?.GetNeighbourAlongDirection(direction, checkConnectivity: false);
		}
		bool HasHDConnection()
		{
			if (!hAdjacent.HasConnectionInDirection(vDirection))
			{
				return dAdjacent.HasConnectionInDirection(-vDirection);
			}
			return true;
		}
		bool HasOHConnection()
		{
			if (!originNode.HasConnectionInDirection(hDirection))
			{
				return hAdjacent.HasConnectionInDirection(-hDirection);
			}
			return true;
		}
		bool HasOVConnection()
		{
			if (!originNode.HasConnectionInDirection(vDirection))
			{
				return vAdjacent.HasConnectionInDirection(-vDirection);
			}
			return true;
		}
		bool HasVDConnection()
		{
			if (!vAdjacent.HasConnectionInDirection(hDirection))
			{
				return dAdjacent.HasConnectionInDirection(-hDirection);
			}
			return true;
		}
	}

	private void PopulateIntermediateNodeData(IntermediateNodeData[] intermediateNodeDataArray)
	{
		int i = 0;
		for (int num = m_Graph.nodes.Length; i < num; i++)
		{
			CustomGridNode customGridNode = m_Graph.nodes[i];
			ref IntermediateNodeData reference = ref intermediateNodeDataArray[i];
			AverageHeightCalculator averageHeightCalculator = default(AverageHeightCalculator);
			float num2 = (reference.height = customGridNode.Vector3Position.y);
			GetInfluenceNodes(customGridNode, CustomGridNodeDirection.W, CustomGridNodeDirection.S, out var influenceH, out var influenceV, out var influenceD);
			averageHeightCalculator.AddInfluence(num2);
			averageHeightCalculator.AddInfluence(influenceH);
			averageHeightCalculator.AddInfluence(influenceV);
			averageHeightCalculator.AddInfluence(influenceD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightSW);
			reference.cornerInfluenceSW.influenceIndexH = influenceH?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceSW.influenceIndexV = influenceV?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceSW.influenceIndexD = influenceD?.NodeInGridIndex ?? (-1);
			GetInfluenceNodes(customGridNode, CustomGridNodeDirection.E, CustomGridNodeDirection.S, out influenceH, out influenceV, out influenceD);
			averageHeightCalculator.AddInfluence(num2);
			averageHeightCalculator.AddInfluence(influenceH);
			averageHeightCalculator.AddInfluence(influenceV);
			averageHeightCalculator.AddInfluence(influenceD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightSE);
			reference.cornerInfluenceSE.influenceIndexH = influenceH?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceSE.influenceIndexV = influenceV?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceSE.influenceIndexD = influenceD?.NodeInGridIndex ?? (-1);
			GetInfluenceNodes(customGridNode, CustomGridNodeDirection.W, CustomGridNodeDirection.N, out influenceH, out influenceV, out influenceD);
			averageHeightCalculator.AddInfluence(num2);
			averageHeightCalculator.AddInfluence(influenceH);
			averageHeightCalculator.AddInfluence(influenceV);
			averageHeightCalculator.AddInfluence(influenceD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightNW);
			reference.cornerInfluenceNW.influenceIndexH = influenceH?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceNW.influenceIndexV = influenceV?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceNW.influenceIndexD = influenceD?.NodeInGridIndex ?? (-1);
			GetInfluenceNodes(customGridNode, CustomGridNodeDirection.E, CustomGridNodeDirection.N, out influenceH, out influenceV, out influenceD);
			averageHeightCalculator.AddInfluence(num2);
			averageHeightCalculator.AddInfluence(influenceH);
			averageHeightCalculator.AddInfluence(influenceV);
			averageHeightCalculator.AddInfluence(influenceD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightNE);
			reference.cornerInfluenceNE.influenceIndexH = influenceH?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceNE.influenceIndexV = influenceV?.NodeInGridIndex ?? (-1);
			reference.cornerInfluenceNE.influenceIndexD = influenceD?.NodeInGridIndex ?? (-1);
			bool num3 = Mathf.Abs(num2 - reference.cornerHeightSW) < m_FlatFilterThreshold;
			bool flag = Mathf.Abs(num2 - reference.cornerHeightSE) < m_FlatFilterThreshold;
			bool flag2 = Mathf.Abs(num2 - reference.cornerHeightNW) < m_FlatFilterThreshold;
			bool flag3 = Mathf.Abs(num2 - reference.cornerHeightNE) < m_FlatFilterThreshold;
			bool flag4 = num3 && flag2 && customGridNode.HasConnectionInDirection(CustomGridNodeDirection.W);
			bool flag5 = flag && flag3 && customGridNode.HasConnectionInDirection(CustomGridNodeDirection.E);
			bool flag6 = num3 && flag && customGridNode.HasConnectionInDirection(CustomGridNodeDirection.S);
			bool flag7 = flag2 && flag3 && customGridNode.HasConnectionInDirection(CustomGridNodeDirection.N);
			reference.flat = flag4 || flag5 || flag6 || flag7;
		}
	}

	private void FlattenCorners(IntermediateNodeData[] nodeDataArray)
	{
		int i = 0;
		for (int num = nodeDataArray.Length; i < num; i++)
		{
			ref IntermediateNodeData reference = ref nodeDataArray[i];
			AverageHeightCalculator averageHeightCalculator = new AverageHeightCalculator(nodeDataArray);
			averageHeightCalculator.AddInfluence(in reference);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceSW.influenceIndexH);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceSW.influenceIndexV);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceSW.influenceIndexD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightSW);
			averageHeightCalculator.AddInfluence(in reference);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceSE.influenceIndexH);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceSE.influenceIndexV);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceSE.influenceIndexD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightSE);
			averageHeightCalculator.AddInfluence(in reference);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceNW.influenceIndexH);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceNW.influenceIndexV);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceNW.influenceIndexD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightNW);
			averageHeightCalculator.AddInfluence(in reference);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceNE.influenceIndexH);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceNE.influenceIndexV);
			averageHeightCalculator.AddInfluence(reference.cornerInfluenceNE.influenceIndexD);
			averageHeightCalculator.Resolve(ref reference.cornerHeightNE);
		}
	}
}
