using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public class WeatherEffectController : IWeatherEntityController, IDisposable
{
	private Transform m_Root;

	private WeatherEffect m_Effect;

	private VisualEffect[,] m_Cells;

	private Bounds m_AreaBounds;

	private float m_LastDistanceFromCamera;

	private float m_VisibleSize;

	private int2 m_GridSize;

	private float3 m_CellSize;

	private int m_CurrentX = -10000;

	private int m_CurrentY = -10000;

	public IEnumerable<VisualEffect> EnumerateCells()
	{
		for (int y = 0; y < m_GridSize.y; y++)
		{
			for (int x = 0; x < m_GridSize.x; x++)
			{
				yield return m_Cells[x, y];
			}
		}
	}

	public WeatherEffectController(Transform root, WeatherEffect effect)
	{
		m_Root = root;
		m_Effect = effect;
	}

	public void Update(Camera camera, float weatherIntensity, Vector2 windDirection, float windIntensity)
	{
		ValidateCells(camera);
		weatherIntensity = m_Effect.EffectIntensityOverLayerIntensity.Evaluate(weatherIntensity);
		UpdateCells(camera, weatherIntensity, windDirection, windIntensity);
	}

	private void UpdateCells(Camera camera, float intensity, Vector2 windDirection, float windIntensity)
	{
		switch (m_Effect.PositioningMode)
		{
		case WeatherEffect.PositionType.Grid:
			UpdateGrid(camera, intensity, windDirection, windIntensity);
			break;
		case WeatherEffect.PositionType.Single:
			UpdateSingle(camera, intensity, windDirection, windIntensity);
			break;
		}
	}

	private void UpdateSingle(Camera camera, float intensity, Vector2 windDirection, float windIntensity)
	{
		Vector3 centerInHorizontalCrossSectionInDistance = GetCenterInHorizontalCrossSectionInDistance(camera, m_LastDistanceFromCamera);
		VisualEffect visualEffect = m_Cells[0, 0];
		visualEffect.transform.position = centerInHorizontalCrossSectionInDistance;
		UpdateVFXParameters(visualEffect, intensity, windDirection, windIntensity);
	}

	private void UpdateGrid(Camera camera, float intensity, Vector2 windDirection, float windIntensity)
	{
		Vector3 vector = camera.transform.position + camera.transform.forward * m_LastDistanceFromCamera;
		Vector3 vector2 = vector - new Vector3(m_VisibleSize / 2f, 0f, m_VisibleSize / 2f);
		int num = Mathf.RoundToInt(vector2.x / m_CellSize.x);
		int num2 = Mathf.RoundToInt(vector2.z / m_CellSize.z);
		for (int i = 0; i < m_GridSize.y; i++)
		{
			for (int j = 0; j < m_GridSize.x; j++)
			{
				VisualEffect vfx = m_Cells[j, i];
				UpdateVFXParameters(vfx, intensity, windDirection, windIntensity);
			}
		}
		if (num == m_CurrentX && num2 == m_CurrentY)
		{
			return;
		}
		for (int k = 0; k < m_GridSize.y; k++)
		{
			for (int l = 0; l < m_GridSize.x; l++)
			{
				int num3 = num + l;
				int num4 = num2 + k;
				if (num3 >= m_CurrentX && num3 < m_CurrentX + m_GridSize.x && num4 >= m_CurrentY && num4 < m_CurrentY + m_GridSize.y)
				{
					continue;
				}
				int2 @int = new int2(num3 % m_GridSize.x, num4 % m_GridSize.y);
				@int.x = ((@int.x < 0) ? (@int.x + m_GridSize.x) : @int.x);
				@int.y = ((@int.y < 0) ? (@int.y + m_GridSize.y) : @int.y);
				VisualEffect visualEffect = m_Cells[@int.x, @int.y];
				float2 @float = new float2((float)num3 * m_CellSize.x, (float)num4 * m_CellSize.z);
				if (m_Effect.SnapToGround)
				{
					float2 float2 = new float2(m_CellSize.x * 0.5f, m_CellSize.z * 0.5f);
					float num5 = 0f;
					float num6 = 0f;
					if (Physics.Raycast(new Ray(new Vector3(@float.x, vector.y, @float.y), Vector3.down), out var hitInfo, 50f))
					{
						num5 += 1f;
						num6 += hitInfo.point.y;
					}
					if (Physics.Raycast(new Ray(new Vector3(@float.x - float2.x, vector.y, @float.y - float2.y), Vector3.down), out hitInfo, 50f))
					{
						num5 += 1f;
						num6 += hitInfo.point.y;
					}
					if (Physics.Raycast(new Ray(new Vector3(@float.x + float2.x, vector.y, @float.y - float2.y), Vector3.down), out hitInfo, 50f))
					{
						num5 += 1f;
						num6 += hitInfo.point.y;
					}
					if (Physics.Raycast(new Ray(new Vector3(@float.x + float2.x, vector.y, @float.y + float2.y), Vector3.down), out hitInfo, 50f))
					{
						num5 += 1f;
						num6 += hitInfo.point.y;
					}
					if (Physics.Raycast(new Ray(new Vector3(@float.x - float2.x, vector.y, @float.y + float2.y), Vector3.down), out hitInfo, 50f))
					{
						num5 += 1f;
						num6 += hitInfo.point.y;
					}
					num6 = ((!(num5 > 0f)) ? vector.y : (num6 / num5));
					visualEffect.transform.position = new Vector3(@float.x, num6, @float.y);
				}
				else
				{
					visualEffect.transform.position = new Vector3(@float.x, vector.y, @float.y);
				}
			}
		}
		m_CurrentX = num;
		m_CurrentY = num2;
	}

	private void UpdateVFXParameters(VisualEffect vfx, float intensity, Vector2 windDirection, float windIntensity)
	{
		if (vfx.HasFloat(VFXWeatherParameters.Intensity))
		{
			vfx.SetFloat(VFXWeatherParameters.Intensity, intensity);
		}
		if (vfx.HasVector2(VFXWeatherParameters.WindDirection))
		{
			vfx.SetVector2(VFXWeatherParameters.WindDirection, windDirection);
		}
		if (vfx.HasFloat(VFXWeatherParameters.WindIntensity))
		{
			vfx.SetFloat(VFXWeatherParameters.WindIntensity, windIntensity);
		}
	}

	private void ValidateCells(Camera camera)
	{
		if (m_Effect.DistanceFromCamera == m_LastDistanceFromCamera)
		{
			return;
		}
		VisualEffect component = m_Effect.VisualEffectPrefab.GetComponent<VisualEffect>();
		m_CellSize = component.GetVector3(VFXWeatherParameters.BoundsSize);
		int2 gridSize = new int2(1, 1);
		if (m_Effect.PositioningMode == WeatherEffect.PositionType.Grid)
		{
			m_VisibleSize = CalculateVisibleSize(camera, m_Effect.DistanceFromCamera);
			gridSize = new int2(Mathf.CeilToInt(m_VisibleSize / m_CellSize.x), Mathf.CeilToInt(m_VisibleSize / m_CellSize.z));
		}
		if (gridSize.x != m_GridSize.x || gridSize.y != m_GridSize.y)
		{
			Destroy();
			m_GridSize = gridSize;
			m_Cells = new VisualEffect[m_GridSize.x, m_GridSize.y];
			for (int i = 0; i < m_GridSize.y; i++)
			{
				for (int j = 0; j < m_GridSize.x; j++)
				{
					m_Cells[j, i] = CreateCell(m_Effect.VisualEffectPrefab);
					UpdateCellBakedGroundParameters(m_Cells[j, i]);
				}
			}
			if (m_Effect.UseBakedLocationData)
			{
				for (int k = 0; k < m_GridSize.y; k++)
				{
					for (int l = 0; l < m_GridSize.x; l++)
					{
						UpdateCellBakedGroundParameters(m_Cells[l, k]);
					}
				}
			}
		}
		m_LastDistanceFromCamera = m_Effect.DistanceFromCamera;
	}

	public void SetAreaBounds(Bounds bounds)
	{
		m_AreaBounds = bounds;
		if (m_Cells == null || m_Cells.Length == 0)
		{
			return;
		}
		VisualEffect[,] cells = m_Cells;
		foreach (VisualEffect vfx in cells)
		{
			UpdateCellBakedGroundParameters(vfx);
		}
	}

	private VisualEffect CreateCell(GameObject prefab)
	{
		return UnityEngine.Object.Instantiate(prefab, m_Root.transform, worldPositionStays: true).GetComponent<VisualEffect>();
	}

	public void UpdateBakedGroundParameters()
	{
		if (!m_Effect.UseBakedLocationData)
		{
			return;
		}
		for (int i = 0; i < m_GridSize.y; i++)
		{
			for (int j = 0; j < m_GridSize.x; j++)
			{
				UpdateCellBakedGroundParameters(m_Cells[j, i]);
			}
		}
	}

	private void UpdateCellBakedGroundParameters(VisualEffect vfx)
	{
		if (VFXTotalLocationWeatherData.HasAreas)
		{
			if (vfx.HasTexture(VFXWeatherParameters.LocationDataTexture))
			{
				vfx.SetTexture(VFXWeatherParameters.LocationDataTexture, VFXTotalLocationWeatherData.Texture);
			}
			if (vfx.HasVector2(VFXWeatherParameters.LocationDataTextureSize) && VFXTotalLocationWeatherData.Texture != null)
			{
				vfx.SetVector2(VFXWeatherParameters.LocationDataTextureSize, new Vector2(VFXTotalLocationWeatherData.Texture.width, VFXTotalLocationWeatherData.Texture.height));
			}
			if (vfx.HasVector3(VFXWeatherParameters.LocationDataBoundsCenter) && vfx.HasVector3(VFXWeatherParameters.LocationDataBoundsSize))
			{
				vfx.SetVector3(VFXWeatherParameters.LocationDataBoundsCenter, m_AreaBounds.center);
				vfx.SetVector3(VFXWeatherParameters.LocationDataBoundsSize, m_AreaBounds.size);
			}
		}
	}

	public void Destroy()
	{
		if (m_Cells == null)
		{
			return;
		}
		for (int i = 0; i < m_GridSize.y; i++)
		{
			for (int j = 0; j < m_GridSize.x; j++)
			{
				if (m_Cells[j, i] != null)
				{
					UnityEngine.Object.Destroy(m_Cells[j, i].gameObject);
				}
			}
		}
	}

	private static Vector3 GetCenterInHorizontalCrossSectionInDistance(Camera camera, float distanceFromCamera)
	{
		Transform transform = camera.transform;
		Vector3 vector = Vector3.Project(transform.forward, Vector3.down);
		Vector3 normalized = (transform.forward - vector).normalized;
		float num = math.acos(Vector3.Dot(Vector3.down, transform.forward));
		float num2 = math.radians(camera.fieldOfView * 0.5f);
		Vector3 vector2 = normalized * (vector.magnitude * math.tan(num + num2));
		Vector3 vector3 = normalized * (vector.magnitude * math.tan(num - num2));
		Vector3 vector4 = (vector2 + vector3) * 0.5f;
		return transform.position + (vector + vector4) * distanceFromCamera;
	}

	private static float CalculateVisibleSize(Camera camera, float distanceFromCamera)
	{
		float num = math.abs(math.acos(math.dot(-camera.transform.forward, new float3(0f, 1f, 0f)))) + math.radians(90f);
		float num2 = math.radians(camera.fieldOfView * 0.5f);
		float x = math.radians(180f) - (num2 + num);
		float num3 = math.radians(180f) - num;
		float num4 = num2;
		float x2 = math.radians(180f) - (num4 + num3);
		float num5 = distanceFromCamera * math.sin(num2) / math.sin(x);
		float num6 = distanceFromCamera * math.sin(num4) / math.sin(x2);
		float num7 = num5 + num6;
		return math.length(new float2(num7 * camera.aspect, num7));
	}

	public void Dispose()
	{
		Destroy();
	}
}
