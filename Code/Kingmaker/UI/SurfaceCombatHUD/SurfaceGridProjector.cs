using Kingmaker.Pathfinding;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public class SurfaceGridProjector
{
	private Material m_Material;

	private ComputeBuffer m_GridBuffer;

	private Vector4[] m_GridData;

	private Vector2Int m_GridCenter;

	private Texture2D m_HeightTexture;

	private static readonly float IMPASSABLE_TERRAIN_HEIGHT = 20000f;

	private Texture2DArray m_DecalArray;

	private Color[] m_DecalColorsArray;

	private Color[] m_AreaBorderLineColorsArray = new Color[16];

	private static readonly uint NO_DATA = 0u;

	private static readonly int DecalsArray = Shader.PropertyToID("_DecalsArray");

	private static readonly int DecalColors = Shader.PropertyToID("_DecalColors");

	private static readonly int CombatGridBuffer = Shader.PropertyToID("_CombatGridBuffer");

	private static readonly int GridCellSize = Shader.PropertyToID("_gridCellSize");

	private static readonly int LineColors = Shader.PropertyToID("_LineColors");

	private static readonly int GridHeightTex = Shader.PropertyToID("_GridHeightTex");

	private static readonly int GridHeightSize = Shader.PropertyToID("_GridHeightSize");

	private static readonly int GridHeightTexSize = Shader.PropertyToID("_GridHeightTexSize");

	private static readonly int GridHeightCenter = Shader.PropertyToID("_GridHeightCenter");

	private static readonly int GridCenterX = Shader.PropertyToID("_gridCenterX");

	private static readonly int GridCenterZ = Shader.PropertyToID("_gridCenterZ");

	private readonly float m_GridCellSize;

	public SurfaceGridProjector(float gridCellSize, Material material)
	{
		m_GridCellSize = gridCellSize;
		m_Material = material;
		m_GridBuffer = new ComputeBuffer(4096, 16, ComputeBufferType.Constant);
		m_GridData = new Vector4[4096];
		for (int i = 0; i < 128; i++)
		{
			for (int j = 0; j < 128; j++)
			{
				SetGridData(i, j, NO_DATA);
			}
		}
		m_GridBuffer.SetData(m_GridData);
		m_Material.SetConstantBuffer(CombatGridBuffer, m_GridBuffer, 0, m_GridBuffer.count * 4 * 4);
		m_Material.SetFloat(GridCellSize, m_GridCellSize);
	}

	private bool SetGridData(int i, int j, uint data)
	{
		if (i >= 0 && j >= 0 && i < 128 && j < 128)
		{
			m_GridData[(i * 128 + j) / 4][(i * 128 + j) % 4] = math.asfloat(data);
			return true;
		}
		return false;
	}

	private bool SetGridData(Vector2 cell, uint data)
	{
		int i = (int)Mathf.Floor(cell.x / m_GridCellSize) - m_GridCenter.x + 64;
		int j = (int)Mathf.Floor(cell.y / m_GridCellSize) - m_GridCenter.y + 64;
		return SetGridData(i, j, data);
	}

	private bool GetGridData(int i, int j, ref uint data)
	{
		if (i >= 0 && j >= 0 && i < 128 && j < 128)
		{
			data = math.asuint(m_GridData[(i * 128 + j) / 4][(i * 128 + j) % 4]);
			return true;
		}
		data = NO_DATA;
		return false;
	}

	private bool GetGridData(Vector2 cell, ref uint data)
	{
		int i = (int)Mathf.Floor(cell.x / m_GridCellSize) - m_GridCenter.x + 64;
		int j = (int)Mathf.Floor(cell.y / m_GridCellSize) - m_GridCenter.y + 64;
		return GetGridData(i, j, ref data);
	}

	public void UpdateDecalsTextureArray(Texture2DArray decalsArray)
	{
		m_DecalArray = decalsArray;
		m_Material.SetTexture(DecalsArray, m_DecalArray);
	}

	public void UpdateColorsArray(Color[] colorsArray)
	{
		m_DecalColorsArray = colorsArray;
		m_Material.SetColorArray(DecalColors, m_DecalColorsArray);
	}

	public void UpdateHeightTexture(CustomGridGraph graph)
	{
		m_HeightTexture = GridHeightTextureGeneratorUtil.Generate(graph, IMPASSABLE_TERRAIN_HEIGHT);
		m_Material.SetTexture(GridHeightTex, m_HeightTexture);
		m_Material.SetVector(GridHeightSize, graph.size);
		m_Material.SetVector(GridHeightCenter, graph.center);
		m_Material.SetVector(GridHeightTexSize, new Vector4(graph.width, graph.depth, 1f / (float)graph.width, 1f / (float)graph.depth));
	}

	public void UpdateAreaBorderLineColor(Color areaBorderLineColor)
	{
		m_AreaBorderLineColorsArray[0] = areaBorderLineColor;
		m_Material.SetColorArray(LineColors, m_AreaBorderLineColorsArray);
	}

	public void UpdateAreaBorderLineColorMinMaxRane(Color minRangeColor, Color maxRangeColor)
	{
		m_AreaBorderLineColorsArray[0] = maxRangeColor;
		m_AreaBorderLineColorsArray[1] = minRangeColor;
		m_Material.SetColorArray(LineColors, m_AreaBorderLineColorsArray);
	}

	public void PushGridDataToShader()
	{
		m_GridBuffer.SetData(m_GridData);
	}

	public void UpdateGridCenterIndex(Vector2Int gridCenter)
	{
		m_GridCenter = gridCenter;
		m_Material.SetInt(GridCenterX, m_GridCenter.x);
		m_Material.SetInt(GridCenterZ, m_GridCenter.y);
	}

	public void SetupCell(Vector2 cell, uint textureIndex, uint colorIndex)
	{
		uint data = NO_DATA;
		if (GetGridData(cell, ref data))
		{
			SetTextureByte(ref data, textureIndex);
			SetColorByte(ref data, colorIndex);
			SetGridData(cell, data);
		}
	}

	public void SetupCellBorders(Vector2 cell, bool right, bool up, bool left, bool down)
	{
		int i = (int)Mathf.Floor(cell.x / GraphParamsMechanicsCache.GridCellSize) - m_GridCenter.x + 64;
		int j = (int)Mathf.Floor(cell.y / GraphParamsMechanicsCache.GridCellSize) - m_GridCenter.y + 64;
		SetupCellBorders(i, j, right, up, left, down);
	}

	public void SetupCellBorders(int i, int j, bool right, bool up, bool left, bool down)
	{
		if (i >= 0 && j >= 0 && i < 128 && j < 128)
		{
			uint data = NO_DATA;
			GetGridData(i, j, ref data);
			if (right)
			{
				SetBit(ref data, bitval: true, 0);
			}
			if (up)
			{
				SetBit(ref data, bitval: true, 1);
			}
			if (left)
			{
				SetBit(ref data, bitval: true, 2);
			}
			if (down)
			{
				SetBit(ref data, bitval: true, 3);
			}
			SetGridData(i, j, data);
		}
	}

	private void SetupCellBorderBit(int i, int j, bool value, int bit)
	{
		uint data = NO_DATA;
		GetGridData(i, j, ref data);
		SetBit(ref data, value, bit);
		SetGridData(i, j, data);
	}

	public void SetupCellBorderR(int i, int j, bool value)
	{
		SetupCellBorderBit(i, j, value, 0);
	}

	public void SetupCellBorderU(int i, int j, bool value)
	{
		SetupCellBorderBit(i, j, value, 1);
	}

	public void SetupCellBorderL(int i, int j, bool value)
	{
		SetupCellBorderBit(i, j, value, 2);
	}

	public void SetupCellBorderD(int i, int j, bool value)
	{
		SetupCellBorderBit(i, j, value, 3);
	}

	public void SetupCellBorderColorIndex(int i, int j, uint borderColorIndex)
	{
		uint data = NO_DATA;
		GetGridData(i, j, ref data);
		SetBorderColorIndex(ref data, borderColorIndex);
		SetGridData(i, j, data);
	}

	public void ClearGrid()
	{
		for (int i = 0; i < 128; i++)
		{
			for (int j = 0; j < 128; j++)
			{
				SetGridData(i, j, NO_DATA);
			}
		}
		m_GridBuffer.SetData(m_GridData);
	}

	public static bool GetBit(uint b, int bitNumber)
	{
		return (b & (uint)(1 << bitNumber)) != 0;
	}

	public static void SetBit(ref uint v, bool bitval, int bitpos)
	{
		if (!bitval)
		{
			v &= (uint)(~(1 << bitpos));
		}
		else
		{
			v |= (uint)(1 << bitpos);
		}
	}

	public static void SetTextureByte(ref uint v, uint b)
	{
		v |= math.clamp(b, 0u, 255u) << 16;
	}

	public static void SetColorByte(ref uint v, uint b)
	{
		v |= math.clamp(b, 0u, 255u) << 8;
	}

	public static void SetBorderColorIndex(ref uint v, uint colorIndex)
	{
		colorIndex &= 0xFu;
		colorIndex <<= 4;
		v |= colorIndex;
	}

	public void Dispose()
	{
		m_GridBuffer?.Dispose();
	}
}
