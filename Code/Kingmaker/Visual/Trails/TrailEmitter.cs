using System;
using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Visual.Trails;

[Serializable]
public class TrailEmitter
{
	private List<TrailPoint> m_ControlPoints = new List<TrailPoint> { TrailPoint.Pop() };

	private float m_Length;

	private int m_PointsCount;

	private float m_Time;

	private CompositeAnimationCurve.Entry m_OffsetCurveRandom;

	[NonSerialized]
	public bool Enabled = true;

	[Header("Appearence")]
	public GameObject Spawner;

	public GameObject SecondSpawner;

	public bool Smooth;

	public float WidthFactor = 1f;

	public bool UseSpawnerScale;

	public WidthOffset WidthOffset;

	public float UvOffset;

	public float RandomUvOffset;

	private float m_UvOffsetByRandomOffset;

	[Header("Velocity")]
	public VelocityType VelocityType;

	public float RandomizeVelocity;

	[Header("Timing")]
	public float Delay;

	public bool UseUnscaledTime;

	[Tooltip("Dont destroy mesh if Spawners are disabled.")]
	public bool DontDestroyOnDisable;

	internal bool RandomInitialized { get; private set; }

	internal void Update(CompositeTrailRenderer context, bool instancing)
	{
		m_Time += (UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		TrailPoint trailPoint = m_ControlPoints[0];
		bool flag = false;
		SpawnType spawnType = GetSpawnType(instancing);
		if (spawnType != 0 && !instancing && !UseUnscaledTime)
		{
			TrailPoint trailPoint2 = m_ControlPoints[m_ControlPoints.Count - 1];
			flag = GameCameraCulling.IsCulled(GetPosition(context)) && GameCameraCulling.IsCulled(trailPoint2.Position);
		}
		if (flag || !Enabled || m_Time < Delay || spawnType == SpawnType.Invalid || (spawnType & SpawnType.Disabled) != 0)
		{
			if (UseUnscaledTime && Time.frameCount % 120 == 0)
			{
				PFLog.TechArt.Log($"TrailEmitter early exit: culled={flag}, enabled={Enabled}, time={m_Time:F2}, delay={Delay}, spawn={spawnType}");
			}
			if (DontDestroyOnDisable)
			{
				trailPoint.Velocity = GetVelocity(context);
				UpdatePoints(context);
			}
			else
			{
				Clear();
			}
			return;
		}
		TrailPoint trailPoint3 = ((m_ControlPoints.Count > 1) ? m_ControlPoints[1] : null);
		Vector3 value = GetPosition(context);
		Vector3 right = GetRight(context.Width, context);
		trailPoint.Position = value;
		trailPoint.Right = right;
		trailPoint.Lifetime = context.Lifetime;
		trailPoint.PositionInTrail = 0f;
		trailPoint.Velocity = default(Vector3);
		if (trailPoint3 != null)
		{
			MathHelper.Vector3Distance(ref trailPoint3.Position, ref value, out trailPoint.PositionFromStart);
			trailPoint.PositionFromStart += trailPoint3.PositionFromStart;
		}
		else
		{
			trailPoint.PositionFromStart = 0f;
		}
		TrailPoint trailPoint4 = null;
		if (trailPoint3 == null)
		{
			trailPoint4 = TrailPoint.Pop();
			trailPoint4.Position = value;
		}
		else
		{
			Vector3 value2 = trailPoint3.Position;
			MathHelper.Vector3Distance(ref value2, ref value, out var result);
			if (result > context.MinVertexDistance)
			{
				trailPoint4 = TrailPoint.Pop();
				MathHelper.Vector3Lerp(ref value2, ref value, 0.99f, out trailPoint4.Position);
				MathHelper.Vector3Distance(ref value2, ref value, out trailPoint4.PositionFromStart);
				trailPoint4.PositionFromStart += trailPoint3.PositionFromStart;
			}
		}
		if (trailPoint4 != null)
		{
			trailPoint4.Right = right;
			trailPoint4.Lifetime = context.Lifetime;
			trailPoint4.Velocity = GetVelocity(context);
			m_ControlPoints.Insert(1, trailPoint4);
		}
		UpdatePoints(context);
	}

	internal void Clear()
	{
		while (m_ControlPoints.Count > 1)
		{
			TrailPoint.Push(m_ControlPoints[0]);
			m_ControlPoints.RemoveAt(0);
		}
	}

	public void ResetRandomUvOffset()
	{
		m_UvOffsetByRandomOffset = PFStatefulRandom.Visuals.Fx.Range(0f, RandomUvOffset);
	}

	private void UpdatePoints(CompositeTrailRenderer context)
	{
		Camera camera = null;
		UIDollRooms instance = UIDollRooms.Instance;
		bool flag = (object)instance != null && instance.CharacterDollRoom?.IsVisible == true;
		if (flag)
		{
			camera = UIDollRooms.Instance.CharacterDollRoom.GetComponentInChildren<Camera>();
		}
		if (camera == null)
		{
			camera = Game.GetCamera();
		}
		bool flag2 = camera != null;
		Vector3 value = default(Vector3);
		if (flag2)
		{
			value = camera.transform.position;
		}
		float num = 0f;
		Vector3 result = Vector3.forward;
		for (int i = 0; i < m_ControlPoints.Count; i++)
		{
			TrailPoint trailPoint = m_ControlPoints[i];
			float num2 = (UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
			if (flag && UseUnscaledTime)
			{
				num2 = Mathf.Min(num2, 0.0167f);
			}
			MathHelper.Vector3Multiply(ref trailPoint.Velocity, num2, out var result2);
			MathHelper.Vector3Add(ref trailPoint.Position, ref result2, out trailPoint.Position);
			trailPoint.Lifetime -= num2;
			if (trailPoint.Lifetime <= 0f)
			{
				trailPoint.Lifetime = 0f;
				if (i > 0)
				{
					m_ControlPoints.RemoveAt(i);
					TrailPoint.Push(trailPoint);
					i--;
					continue;
				}
			}
			if (i > 0)
			{
				MathHelper.Vector3Subtract(ref trailPoint.Position, ref m_ControlPoints[i - 1].Position, out result);
			}
			else if (m_ControlPoints.Count > 1)
			{
				MathHelper.Vector3Subtract(ref m_ControlPoints[i + 1].Position, ref trailPoint.Position, out result);
			}
			MathHelper.Vector3Length(ref result, out var result3);
			if (i > 0)
			{
				num = (trailPoint.PositionInTrail = num + result3);
				if (context.MaxLength > 0f && trailPoint.PositionInTrail >= context.MaxLength)
				{
					m_ControlPoints.RemoveAt(i);
					TrailPoint.Push(trailPoint);
					i--;
					continue;
				}
			}
			else
			{
				trailPoint.PositionInTrail = 0f;
				num = 0f;
			}
			if (float.IsNaN(result3) || Mathf.Approximately(result3, 0f))
			{
				result = Vector3.forward;
			}
			else
			{
				result.x /= result3;
				result.y /= result3;
				result.z /= result3;
			}
			if (context.GetEffectiveAlignment() == CompositeTrailRenderer.TrailAlignment.View && flag2)
			{
				MathHelper.Vector3Subtract(ref value, ref trailPoint.Position, out trailPoint.Normal);
				MathHelper.Vector3Normalize(ref trailPoint.Normal, out trailPoint.Normal);
				MathHelper.Vector3Cross(ref trailPoint.Normal, ref result, out trailPoint.ViewRight);
				MathHelper.Vector3Normalize(ref trailPoint.ViewRight, out trailPoint.ViewRight);
				MathHelper.Vector3Cross(ref result, ref trailPoint.ViewRight, out trailPoint.Normal);
				trailPoint.ViewRight *= trailPoint.Right.magnitude;
			}
			else
			{
				MathHelper.Vector3Normalize(ref trailPoint.Right, out var result4);
				MathHelper.Vector3Cross(ref result, ref result4, out trailPoint.Normal);
			}
		}
		m_Length = num;
	}

	internal int CalculatePointsCount(float minDistance)
	{
		int num = 0;
		if (Smooth)
		{
			for (int i = 0; i < m_ControlPoints.Count - 1; i++)
			{
				float num2 = 0f;
				MathHelper.Vector3Distance(ref m_ControlPoints[i].Position, ref m_ControlPoints[i + 1].Position, out var result);
				while (num2 < result)
				{
					num2 += minDistance;
					num++;
				}
			}
		}
		else
		{
			num = m_ControlPoints.Count;
		}
		return num;
	}

	public void UpdateGeometry(CompositeTrailRenderer context)
	{
		if (m_ControlPoints.Count < 2)
		{
			return;
		}
		m_PointsCount = 0;
		int vertexOffset = context.VertexOffset;
		int indexOffset = context.IndexOffset;
		for (int i = 0; i < m_ControlPoints.Count; i++)
		{
			TrailPoint trailPoint = m_ControlPoints[i];
			if (Smooth)
			{
				TrailPoint prevPoint = GetPrevPoint(i);
				TrailPoint nextPoint = GetNextPoint(i);
				TrailPoint nextPoint2 = GetNextPoint(i + 1);
				float num = 0f;
				MathHelper.Vector3Distance(ref trailPoint.Position, ref nextPoint.Position, out var result);
				for (; num < result; num += context.MinVertexDistance)
				{
					float num2 = num / result;
					MathHelper.Vector3CatmullRom(ref prevPoint.Position, ref trailPoint.Position, ref nextPoint.Position, ref nextPoint2.Position, num2, out var result2);
					Vector3 result3;
					if (context.GetEffectiveAlignment() == CompositeTrailRenderer.TrailAlignment.View)
					{
						MathHelper.Vector3CatmullRom(ref prevPoint.ViewRight, ref trailPoint.ViewRight, ref nextPoint.ViewRight, ref nextPoint2.ViewRight, num2, out result3);
					}
					else
					{
						MathHelper.Vector3CatmullRom(ref prevPoint.Right, ref trailPoint.Right, ref nextPoint.Right, ref nextPoint2.Right, num2, out result3);
					}
					float posFromStart = Mathf.Lerp(trailPoint.PositionFromStart, nextPoint.PositionFromStart, num2);
					float num3 = Mathf.Lerp(trailPoint.Lifetime, nextPoint.Lifetime, num2);
					num3 /= context.Lifetime;
					float num4 = Mathf.Lerp(trailPoint.PositionInTrail, nextPoint.PositionInTrail, num2);
					float normPos = ((m_Length > 0f) ? (num4 / m_Length) : 0f);
					float num5 = context.CalculateWidthScale(num3, normPos, num4) * WidthFactor;
					if (UseSpawnerScale)
					{
						GameObject singleSpawner = GetSingleSpawner();
						if (singleSpawner != null)
						{
							num5 *= singleSpawner.transform.localScale.x;
						}
					}
					result3 *= num5;
					MathHelper.Vector3Lerp(ref trailPoint.Normal, ref nextPoint.Normal, num2, out var result4);
					UpdateGeometryByPoint(context, m_PointsCount, indexOffset, vertexOffset, ref result2, ref result3, ref result4, num3, normPos, num4, posFromStart);
				}
				continue;
			}
			Vector3 pos = trailPoint.Position;
			Vector3 right = ((context.GetEffectiveAlignment() == CompositeTrailRenderer.TrailAlignment.View) ? trailPoint.ViewRight : trailPoint.Right);
			float positionFromStart = trailPoint.PositionFromStart;
			float normLifetime = trailPoint.Lifetime / context.Lifetime;
			float normPos2 = ((m_Length > 0f) ? (trailPoint.PositionInTrail / m_Length) : 0f);
			float num6 = context.CalculateWidthScale(normLifetime, normPos2, trailPoint.PositionInTrail) * WidthFactor;
			if (UseSpawnerScale)
			{
				GameObject singleSpawner2 = GetSingleSpawner();
				if (singleSpawner2 != null)
				{
					num6 *= singleSpawner2.transform.localScale.x;
				}
			}
			right *= num6;
			UpdateGeometryByPoint(context, i, indexOffset, vertexOffset, ref pos, ref right, ref trailPoint.Normal, normLifetime, normPos2, trailPoint.PositionInTrail, positionFromStart);
		}
		context.VertexOffset = vertexOffset + m_PointsCount * 2;
		context.IndexOffset = indexOffset + (m_PointsCount - 1) * 6;
	}

	private void UpdateGeometryByPoint(CompositeTrailRenderer context, int pointIndex, int indexOffset, int vertexOffset, ref Vector3 pos, ref Vector3 right, ref Vector3 vertexNormal, float normLifetime, float normPos, float posInTrail, float posFromStart)
	{
		m_PointsCount++;
		int num = vertexOffset + pointIndex * 2;
		int num2 = num + 1;
		Vector3 value = context.CalculateOffset(ref right, normLifetime, normPos, m_OffsetCurveRandom);
		MathHelper.Vector3Add(ref pos, ref value, out pos);
		SpawnType spawnType = GetSpawnType();
		if ((spawnType & SpawnType.Single) != 0)
		{
			switch (WidthOffset)
			{
			case WidthOffset.Center:
				right *= 0.5f;
				MathHelper.Vector3Add(ref pos, ref right, out context.Vertices[num2]);
				MathHelper.Vector3Subtract(ref pos, ref right, out context.Vertices[num]);
				break;
			case WidthOffset.Right:
				MathHelper.Vector3Add(ref pos, ref right, out context.Vertices[num2]);
				context.Vertices[num] = pos;
				break;
			case WidthOffset.Left:
				context.Vertices[num2] = pos;
				MathHelper.Vector3Subtract(ref pos, ref right, out context.Vertices[num]);
				break;
			}
		}
		else if ((spawnType & SpawnType.Double) != 0)
		{
			MathHelper.Vector3Add(ref pos, ref right, out context.Vertices[num2]);
			MathHelper.Vector3Subtract(ref pos, ref right, out context.Vertices[num]);
		}
		float x = context.CalculateUv(normPos, posFromStart) + UvOffset + m_UvOffsetByRandomOffset;
		context.Uv[num] = new Vector2(x, 0f);
		context.Uv[num2] = new Vector2(x, 1f);
		context.Normals[num] = vertexNormal;
		context.Normals[num2] = vertexNormal;
		Color color = context.CalculateColor(normLifetime, normPos, posInTrail);
		context.Colors[num] = color;
		context.Colors[num2] = color;
		context.Indices[indexOffset + pointIndex * 6] = vertexOffset + pointIndex * 2;
		context.Indices[indexOffset + pointIndex * 6 + 1] = vertexOffset + (pointIndex + 1) * 2;
		context.Indices[indexOffset + pointIndex * 6 + 2] = vertexOffset + pointIndex * 2 + 1;
		context.Indices[indexOffset + pointIndex * 6 + 3] = vertexOffset + pointIndex * 2 + 1;
		context.Indices[indexOffset + pointIndex * 6 + 4] = vertexOffset + (pointIndex + 1) * 2;
		context.Indices[indexOffset + pointIndex * 6 + 5] = vertexOffset + (pointIndex + 1) * 2 + 1;
	}

	private TrailPoint GetNextPoint(int i)
	{
		i = Mathf.Min(m_ControlPoints.Count - 1, i + 1);
		return m_ControlPoints[i];
	}

	private TrailPoint GetPrevPoint(int i)
	{
		i = Mathf.Max(0, i - 1);
		return m_ControlPoints[i];
	}

	private Vector3 GetRight(float width, CompositeTrailRenderer context = null)
	{
		if (GetSpawnType() == SpawnType.Double)
		{
			return (SecondSpawner.transform.position - Spawner.transform.position) * 0.5f;
		}
		GameObject singleSpawner = GetSingleSpawner();
		if (singleSpawner != null)
		{
			return -singleSpawner.transform.right * width;
		}
		if (context != null)
		{
			return -context.transform.right * width;
		}
		return -Vector3.right * width;
	}

	public Vector3 GetPosition(CompositeTrailRenderer context = null)
	{
		if (GetSpawnType() == SpawnType.Double)
		{
			return (SecondSpawner.transform.position + Spawner.transform.position) * 0.5f;
		}
		GameObject singleSpawner = GetSingleSpawner();
		if (singleSpawner != null)
		{
			return singleSpawner.transform.position;
		}
		if (context != null)
		{
			return context.transform.position;
		}
		return Vector3.zero;
	}

	private Vector3 GetVelocity(CompositeTrailRenderer context)
	{
		Vector3 vector = default(Vector3);
		if (!Mathf.Approximately(RandomizeVelocity, 0f) && VelocityType != 0)
		{
			vector = PFStatefulRandom.Trails.onUnitSphere * RandomizeVelocity;
		}
		switch (VelocityType)
		{
		case VelocityType.None:
			return default(Vector3);
		case VelocityType.Self:
		{
			GameObject singleSpawner = GetSingleSpawner();
			if (singleSpawner != null)
			{
				return singleSpawner.transform.TransformDirection(context.PointVelocity) + vector;
			}
			return context.PointVelocity + vector;
		}
		case VelocityType.World:
			return context.PointVelocity + vector;
		default:
			return default(Vector3);
		}
	}

	public void DrawGizmos()
	{
		if (m_ControlPoints.Count < 2)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_ControlPoints.Count; i++)
		{
			Gizmos.color = ((num > 0) ? Color.black : Color.white);
			num = 1 - num;
			if (i < m_ControlPoints.Count - 1)
			{
				Gizmos.DrawLine(m_ControlPoints[i].Position, m_ControlPoints[i + 1].Position);
			}
			Gizmos.DrawWireSphere(m_ControlPoints[i].Position, 0.1f);
		}
	}

	private SpawnType GetSpawnType(bool instancing = false)
	{
		SpawnType spawnType = SpawnType.Invalid;
		if (Spawner != null && SecondSpawner != null)
		{
			spawnType = SpawnType.Double;
			if (instancing || !Spawner.activeInHierarchy || !SecondSpawner.activeInHierarchy)
			{
				spawnType |= SpawnType.Disabled;
			}
			return spawnType;
		}
		GameObject singleSpawner = GetSingleSpawner();
		if ((bool)singleSpawner)
		{
			spawnType = SpawnType.Single;
			if (!instancing && !singleSpawner.activeInHierarchy)
			{
				spawnType |= SpawnType.Disabled;
			}
		}
		if (spawnType == SpawnType.Invalid && UseUnscaledTime)
		{
			spawnType = SpawnType.Single;
		}
		return spawnType;
	}

	private GameObject GetSingleSpawner()
	{
		if (!(Spawner == null))
		{
			return Spawner;
		}
		return SecondSpawner;
	}

	internal void Randomize(CompositeTrailRenderer context)
	{
		if (m_OffsetCurveRandom == null)
		{
			m_OffsetCurveRandom = new CompositeAnimationCurve.Entry();
		}
		CompositeAnimationCurve.Entry offsetCurveRandom = context.OffsetCurveRandom;
		m_OffsetCurveRandom.Amplitude = PFStatefulRandom.Trails.Range(0f - offsetCurveRandom.Amplitude, offsetCurveRandom.Amplitude);
		m_OffsetCurveRandom.Frequency = PFStatefulRandom.Trails.Range(0f - offsetCurveRandom.Frequency, offsetCurveRandom.Frequency);
		m_OffsetCurveRandom.OffsetX = PFStatefulRandom.Trails.Range(0f - offsetCurveRandom.OffsetX, offsetCurveRandom.OffsetX);
		m_OffsetCurveRandom.OffsetY = PFStatefulRandom.Trails.Range(0f - offsetCurveRandom.OffsetY, offsetCurveRandom.OffsetY);
		m_OffsetCurveRandom.ScrollSpeed = PFStatefulRandom.Trails.Range(0f - offsetCurveRandom.ScrollSpeed, offsetCurveRandom.ScrollSpeed);
		RandomInitialized = true;
	}
}
