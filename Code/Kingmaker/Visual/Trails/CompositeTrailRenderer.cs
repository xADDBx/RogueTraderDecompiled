using System;
using System.Collections.Generic;
using DG.DemiLib.Attributes;
using Kingmaker.QA.Profiling;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Trails;

[ExecuteInEditMode]
[ScriptExecutionOrder(10001)]
public class CompositeTrailRenderer : MonoBehaviour
{
	public enum TrailAlignment
	{
		View,
		World
	}

	public enum CurveMode
	{
		Lifetime,
		Length
	}

	private const float MIN_DIST = 0.001f;

	private List<TrailEmitter> m_ActiveTrails = new List<TrailEmitter>();

	private Mesh m_Mesh;

	private Dictionary<GameObject, bool> m_SpawnersEnabled = new Dictionary<GameObject, bool>();

	private CompositeAnimationCurve.Entry m_PrevOffsetRandom = new CompositeAnimationCurve.Entry
	{
		Amplitude = 0f,
		Frequency = 0f,
		OffsetX = 0f,
		OffsetY = 0f,
		ScrollSpeed = 0f
	};

	internal Vector3[] Vertices = new Vector3[0];

	internal Vector2[] Uv = new Vector2[0];

	internal Vector3[] Normals = new Vector3[0];

	internal Color[] Colors = new Color[0];

	internal int[] Indices = new int[0];

	private List<int> IndicesCurrent = new List<int>();

	internal int VertexOffset;

	internal int IndexOffset;

	public Material Material;

	public Transform ProbeAnchor;

	public bool UseLightProbes = true;

	public float Lifetime = 1f;

	public TrailAlignment Alignment;

	public Space UvSpace = Space.Self;

	[SerializeField]
	private float m_MinVertexDistance = 0.1f;

	public Vector3 PointVelocity = Vector3.up;

	public Gradient ColorOverLifetime;

	public Gradient ColorOverLength;

	public float MaxColorLenght;

	public Color MultiplyColor = Color.white;

	public float Width = 1f;

	public AnimationCurve WidthOverLifetime = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public AnimationCurve WidthOverLength = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public float MaxLength;

	public CurveMode OffsetCurveMode;

	public CompositeAnimationCurve.Entry OffsetCurveRandom = new CompositeAnimationCurve.Entry
	{
		Amplitude = 0f,
		Frequency = 0f,
		OffsetX = 0f,
		OffsetY = 0f,
		ScrollSpeed = 0f
	};

	public CompositeAnimationCurve OffsetCurve = new CompositeAnimationCurve();

	public List<TrailEmitter> Emitters = new List<TrailEmitter>();

	public Action WantRepaint;

	private int m_InstancingUpdateFrame = -1;

	private bool m_HasNoActiveEmitters;

	public Mesh Mesh => m_Mesh;

	public float MinVertexDistance
	{
		get
		{
			m_MinVertexDistance = Mathf.Max(m_MinVertexDistance, 0.001f);
			return m_MinVertexDistance;
		}
	}

	private void OnEnable()
	{
		if (m_SpawnersEnabled.Count > 0)
		{
			foreach (KeyValuePair<GameObject, bool> item in m_SpawnersEnabled)
			{
				if (item.Key != null)
				{
					item.Key.SetActive(item.Value);
				}
			}
		}
		foreach (TrailEmitter emitter in Emitters)
		{
			emitter.Clear();
			emitter.ResetRandomUvOffset();
		}
		if (m_Mesh == null)
		{
			m_Mesh = new Mesh
			{
				name = $"{base.gameObject.name}_TrailMesh"
			};
			m_Mesh.MarkDynamic();
		}
	}

	private void Start()
	{
		foreach (TrailEmitter emitter in Emitters)
		{
			if (emitter.Spawner != null)
			{
				m_SpawnersEnabled[emitter.Spawner] = emitter.Spawner.activeInHierarchy;
			}
			if (emitter.SecondSpawner != null)
			{
				m_SpawnersEnabled[emitter.SecondSpawner] = emitter.SecondSpawner.activeInHierarchy;
			}
		}
		LateUpdate();
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(m_Mesh);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(m_Mesh);
		}
	}

	private void LateUpdate()
	{
		using (Counters.Trails?.Measure())
		{
			if (!m_HasNoActiveEmitters && !(Game.GetCamera() == null))
			{
				int pointsCount = UpdateEmitters(instancing: false);
				UpdateGeometry(pointsCount);
				DrawMesh(Matrix4x4.identity);
			}
		}
	}

	public void InstancingUpdate()
	{
		if (Time.frameCount != m_InstancingUpdateFrame && !m_HasNoActiveEmitters)
		{
			m_InstancingUpdateFrame = Time.frameCount;
			int pointsCount = UpdateEmitters(instancing: true);
			UpdateGeometry(pointsCount);
		}
	}

	private int UpdateEmitters(bool instancing)
	{
		m_ActiveTrails.Clear();
		VertexOffset = 0;
		IndexOffset = 0;
		int num = 0;
		bool flag = m_PrevOffsetRandom.Amplitude != OffsetCurveRandom.Amplitude || m_PrevOffsetRandom.Frequency != OffsetCurveRandom.Frequency || m_PrevOffsetRandom.OffsetX != OffsetCurveRandom.OffsetX || m_PrevOffsetRandom.OffsetY != OffsetCurveRandom.OffsetY || m_PrevOffsetRandom.ScrollSpeed != OffsetCurveRandom.ScrollSpeed;
		m_PrevOffsetRandom.Amplitude = OffsetCurveRandom.Amplitude;
		m_PrevOffsetRandom.Frequency = OffsetCurveRandom.Frequency;
		m_PrevOffsetRandom.OffsetX = OffsetCurveRandom.OffsetX;
		m_PrevOffsetRandom.OffsetY = OffsetCurveRandom.OffsetY;
		m_PrevOffsetRandom.ScrollSpeed = OffsetCurveRandom.ScrollSpeed;
		m_HasNoActiveEmitters = true;
		foreach (TrailEmitter emitter in Emitters)
		{
			if (!emitter.RandomInitialized || flag)
			{
				emitter.Randomize(this);
			}
			emitter.Update(this, instancing);
			int num2 = emitter.CalculatePointsCount(MinVertexDistance);
			if (num2 > 1)
			{
				m_ActiveTrails.Add(emitter);
				num = Mathf.Max(num, num2);
			}
			m_HasNoActiveEmitters &= !emitter.Enabled;
		}
		m_HasNoActiveEmitters &= m_ActiveTrails.Count < 1;
		return num;
	}

	private void UpdateGeometry(int pointsCount)
	{
		if (m_ActiveTrails.Count <= 0)
		{
			return;
		}
		int num = pointsCount * 2 * Emitters.Count;
		int num2 = pointsCount * 6 * Emitters.Count;
		int num3 = Vertices.Length;
		if (Vertices.Length < num)
		{
			if (Vertices.Length != 0)
			{
				while (num3 < num)
				{
					num3 *= 2;
					num2 *= 2;
				}
			}
			else
			{
				num3 = num;
			}
			Array.Resize(ref Vertices, num3);
			Array.Resize(ref Normals, num3);
			Array.Resize(ref Uv, num3);
			Array.Resize(ref Colors, num3);
			Array.Resize(ref Indices, num2);
		}
		foreach (TrailEmitter activeTrail in m_ActiveTrails)
		{
			activeTrail.UpdateGeometry(this);
		}
		if (IndexOffset >= 3)
		{
			UpdateMesh();
		}
	}

	public void DrawMesh(Matrix4x4 matrix)
	{
		if (m_ActiveTrails.Count > 0 && IndexOffset >= 3)
		{
			Graphics.DrawMesh(Mesh, matrix, Material, base.gameObject.layer, null, 0, null, ShadowCastingMode.Off, receiveShadows: false, ProbeAnchor, UseLightProbes);
		}
	}

	private void UpdateMesh()
	{
		Mesh mesh = Mesh;
		mesh.Clear();
		mesh.vertices = Vertices;
		mesh.normals = Normals;
		mesh.uv = Uv;
		mesh.colors = Colors;
		IndicesCurrent.Clear();
		for (int i = 0; i < Indices.Length; i++)
		{
			IndicesCurrent.Add(Indices[i]);
		}
		IndicesCurrent.RemoveRange(IndexOffset, Indices.Length - IndexOffset);
		mesh.SetTriangles(IndicesCurrent, 0);
		mesh.RecalculateBounds();
	}

	private void OnDrawGizmosSelected()
	{
		if (WantRepaint != null)
		{
			WantRepaint();
		}
		foreach (TrailEmitter emitter in Emitters)
		{
			if (emitter.Spawner != null)
			{
				Gizmos.color = (emitter.Spawner.activeInHierarchy ? Color.red : new Color(1f, 0f, 0f, 0.5f));
				Gizmos.DrawWireSphere(emitter.Spawner.transform.position, 0.2f);
				emitter.DrawGizmos();
			}
		}
	}

	public float CalculateWidthScale(float normLifetime, float normPos, float pos)
	{
		float t = 1f - normLifetime;
		float num = WidthOverLifetime.EvaluateNormalized(t);
		float num2 = 1f;
		if (MaxLength > 0f)
		{
			num2 = WidthOverLength.EvaluateNormalized(pos / MaxLength);
		}
		else if (normPos > 0f)
		{
			num2 = WidthOverLength.EvaluateNormalized(normPos);
		}
		return num * num2;
	}

	public Color CalculateColor(float normLifetime, float normPos, float pos)
	{
		float time = 1f - normLifetime;
		Color color = ColorOverLifetime.Evaluate(time);
		time = ((!(MaxColorLenght > 0f)) ? normPos : Mathf.Clamp01(pos / MaxColorLenght));
		return color * (ColorOverLength.Evaluate(time) * MultiplyColor);
	}

	public float CalculateUv(float normPos, float posFromStart)
	{
		if (UvSpace == Space.Self)
		{
			return normPos;
		}
		return posFromStart;
	}

	public Vector3 CalculateOffset(ref Vector3 normal, float normLifetime, float normLength, CompositeAnimationCurve.Entry randomOffsets)
	{
		float time = 0f;
		if (OffsetCurveMode == CurveMode.Lifetime)
		{
			if (Lifetime > 0f)
			{
				time = 1f - normLifetime;
			}
		}
		else
		{
			time = normLength;
		}
		Vector3 value = normal.normalized;
		float num = 0f;
		num = ((randomOffsets == null) ? OffsetCurve.EvaluateNormalized(time) : OffsetCurve.EvaluateNormalizedWithOffsets(time, randomOffsets));
		MathHelper.Vector3Multiply(ref value, num, out value);
		return value;
	}

	public void SetEmittersEnabled(bool emit)
	{
		if (emit)
		{
			m_HasNoActiveEmitters = false;
		}
		foreach (TrailEmitter emitter in Emitters)
		{
			emitter.Enabled = emit;
		}
	}

	private void OnValidate()
	{
		m_MinVertexDistance = Mathf.Max(m_MinVertexDistance, 0.001f);
	}
}
