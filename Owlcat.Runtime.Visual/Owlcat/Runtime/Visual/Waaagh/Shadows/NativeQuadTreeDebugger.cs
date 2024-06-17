using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

internal sealed class NativeQuadTreeDebugger
{
	private Texture2D m_AllocationTexture;

	public Texture2D AllocationTexture => m_AllocationTexture;

	public void Refresh(NativeQuadTree quadTree, Color partiallyOccupiedColor, Color occupiedColor, Color occupiedInHierarchyColor)
	{
		int num = (int)Mathf.Sqrt(quadTree.GetNodesCountOnLevel(quadTree.Levels - 1));
		if (m_AllocationTexture == null || m_AllocationTexture.width != num)
		{
			if (m_AllocationTexture != null)
			{
				Object.DestroyImmediate(m_AllocationTexture);
			}
			m_AllocationTexture = new Texture2D(num, num, TextureFormat.ARGB32, mipChain: true);
			m_AllocationTexture.filterMode = FilterMode.Point;
			m_AllocationTexture.name = "ShadowAtlasDebugTex";
		}
		Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		int levels = quadTree.Levels;
		for (int i = 0; i < levels; i++)
		{
			int miplevel = levels - i - 1;
			Color32[] pixels = m_AllocationTexture.GetPixels32(miplevel);
			int nodesCountOnLevel = quadTree.GetNodesCountOnLevel(i);
			int levelStartIndex = quadTree.GetLevelStartIndex(i);
			for (int j = 0; j < nodesCountOnLevel; j++)
			{
				int nodeIndex = levelStartIndex + j;
				NativeQuadTreeNode node = quadTree.GetNode(nodeIndex);
				Color32[] array = pixels;
				int num2 = j;
				array[num2] = node.State switch
				{
					NativeQuadTreeNodeState.PartiallyOccupied => partiallyOccupiedColor, 
					NativeQuadTreeNodeState.Occupied => occupiedColor, 
					NativeQuadTreeNodeState.OccupiedInHierarchy => occupiedInHierarchyColor, 
					_ => color, 
				};
			}
			m_AllocationTexture.SetPixels32(pixels, miplevel);
		}
		m_AllocationTexture.Apply(updateMipmaps: false);
	}
}
