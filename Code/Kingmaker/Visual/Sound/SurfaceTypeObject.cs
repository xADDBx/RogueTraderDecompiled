using System;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class SurfaceTypeObject : RegisteredBehaviour
{
	public const float TileSize = 0.2f;

	[SerializeField]
	[HideInInspector]
	private byte[] m_Data;

	[SerializeField]
	[HideInInspector]
	private TextAsset m_SoundCacheFile;

	private byte[] m_RuntimeData;

	public Bounds Bounds;

	[SerializeField]
	[Tooltip("Allows editing of Y extent")]
	private bool m_UnboundY;

	[SerializeField]
	[Tooltip("Will return SoundSurface only for objects within bounds")]
	private bool m_UseOnlyInBounds;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float m_RaycastThickness;

	public bool UseOnlyInBounds => m_UseOnlyInBounds;

	public int Width { get; private set; }

	public int Length { get; private set; }

	public TextAsset SoundCacheFile => m_SoundCacheFile;

	public float RaycastThickness => m_RaycastThickness;

	public static SurfaceType? GetSurfaceSoundTypeSwitch(Vector3 worldPos)
	{
		foreach (SurfaceTypeObject item in ObjectRegistry<SurfaceTypeObject>.Instance)
		{
			if (item.TryGetSurfaceType(worldPos, out var surfaceType))
			{
				return (SurfaceType)surfaceType;
			}
		}
		return null;
	}

	public bool TryGetSurfaceType(Vector3 worldPos, out byte surfaceType)
	{
		surfaceType = 0;
		if (m_RuntimeData == null)
		{
			return false;
		}
		if (!TryGetCoordinates(worldPos, out var x, out var z))
		{
			return false;
		}
		int index = GetIndex(x, z);
		surfaceType = Get(index);
		return true;
	}

	public byte Get(int index)
	{
		if (index < 0 || index >= m_RuntimeData.Length)
		{
			return 0;
		}
		return m_RuntimeData[index];
	}

	public bool TryGetCoordinates(Vector3 worldPos, out int x, out int z)
	{
		x = (int)((worldPos.x - Bounds.min.x) / 0.2f);
		z = (int)((worldPos.z - Bounds.min.z) / 0.2f);
		if (m_UseOnlyInBounds && !Bounds.Contains(worldPos))
		{
			return false;
		}
		if (x >= 0)
		{
			return z >= 0;
		}
		return false;
	}

	public int GetIndex(int x, int z)
	{
		return x + Width * z;
	}

	protected override void OnEnabled()
	{
		UpdateValues();
	}

	public void UpdateValues()
	{
		Width = Mathf.CeilToInt(Bounds.size.x / 0.2f);
		Length = Mathf.CeilToInt(Bounds.size.z / 0.2f);
		if ((bool)m_SoundCacheFile)
		{
			m_RuntimeData = m_SoundCacheFile.bytes;
		}
		else
		{
			m_RuntimeData = m_Data;
		}
	}

	public void SetData(TextAsset soundCacheFile)
	{
		m_Data = null;
		m_SoundCacheFile = soundCacheFile;
		m_RuntimeData = SoundCacheFile.bytes;
	}

	private void OnValidate()
	{
		if (!m_UnboundY)
		{
			Bounds.extents = new Vector3(Bounds.extents.x, Math.Max(Bounds.extents.x, Bounds.extents.z), Bounds.extents.z);
		}
	}
}
