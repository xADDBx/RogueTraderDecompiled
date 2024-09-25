using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Kingmaker.Visual.VFXGraph;

[AddComponentMenu("VFX/Property Binders/Owlcat Multiple Position Binder")]
[VFXBinder("Point Cache/Owlcat Multiple Position Binder")]
[ExecuteInEditMode]
public class VFXOwlcatMultiplePositionBinder : VFXBinderBase
{
	private static readonly Vector3 kOutOfBoundsPosition = new Vector3(0f, 100000f, 0f);

	private static readonly Color kOutOfBoundsPositionColor = new Color(kOutOfBoundsPosition.x, kOutOfBoundsPosition.y, kOutOfBoundsPosition.z);

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	[FormerlySerializedAs("PositionMapParameter")]
	public ExposedProperty PositionMapProperty = "PositionMap";

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	public ExposedProperty OpacityMapProperty = "OpacityMap";

	[VFXPropertyBinding(new string[] { "System.Int32" })]
	[FormerlySerializedAs("PositionCountParameter")]
	public ExposedProperty PositionCountProperty = "PositionCount";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	[FormerlySerializedAs("BoundsCenterParameter")]
	public ExposedProperty BoundsCenterProperty = "BoundsCenter";

	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	[FormerlySerializedAs("BoundsSizeParameter")]
	public ExposedProperty BoundsSizeProperty = "BoundsSize";

	public Vector3 BoundsMargin = new Vector3(1f, 1f, 1f);

	public List<Transform> Targets;

	public int BindingTag;

	private Texture2D m_PositionMapTexture;

	private Texture2D m_OpacityMapTexture;

	private int m_ParticlesCount;

	private Vector3 m_ParticleSystemBoundsCenter;

	private Vector3 m_ParticleSystemBoundsSize;

	private NativeArray<ulong> m_OpacityMapBuffer;

	private bool m_OpacityMapBufferChanged;

	[UsedImplicitly]
	protected override void OnEnable()
	{
		Setup();
		if (!binder.m_Bindings.Contains(this))
		{
			binder.m_Bindings.Add(this);
		}
	}

	[UsedImplicitly]
	protected override void OnDisable()
	{
		base.OnDisable();
		Cleanup();
	}

	[UsedImplicitly]
	private void LateUpdate()
	{
		if (m_OpacityMapBufferChanged && m_OpacityMapTexture != null && m_OpacityMapBuffer.IsCreated)
		{
			m_OpacityMapTexture.LoadRawTextureData(m_OpacityMapBuffer);
			m_OpacityMapTexture.Apply(updateMipmaps: false);
		}
	}

	public void SetOpacity(int indexBegin, int indexEnd, float value)
	{
		if (m_OpacityMapBuffer.IsCreated && indexEnd > indexBegin)
		{
			bool value2 = value > 0.5f;
			SetBufferBits(m_OpacityMapBuffer, value2, indexBegin, indexEnd - indexBegin);
			m_OpacityMapBufferChanged = true;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (m_PositionMapTexture != null && m_OpacityMapTexture != null && m_OpacityMapBuffer.IsCreated && component.HasTexture(PositionMapProperty) && component.HasTexture(OpacityMapProperty) && component.HasInt(PositionCountProperty) && component.HasVector3(BoundsCenterProperty))
		{
			return component.HasVector3(BoundsSizeProperty);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		if (m_PositionMapTexture != null)
		{
			component.SetTexture(PositionMapProperty, m_PositionMapTexture);
		}
		if (m_OpacityMapTexture != null)
		{
			component.SetTexture(OpacityMapProperty, m_OpacityMapTexture);
		}
		component.SetInt(PositionCountProperty, m_ParticlesCount);
		component.SetVector3(BoundsCenterProperty, m_ParticleSystemBoundsCenter);
		component.SetVector3(BoundsSizeProperty, m_ParticleSystemBoundsSize);
	}

	public void SetTargets(List<Transform> value)
	{
		Targets = value;
		if (base.enabled)
		{
			Cleanup();
			Setup();
			if (TryGetComponent<VisualEffect>(out var component) && IsValid(component))
			{
				UpdateBinding(component);
				component.Reinit();
			}
		}
	}

	private void Setup()
	{
		List<Color> value;
		using (CollectionPool<List<Color>, Color>.Get(out value))
		{
			CollectPositions(value, out var bounds);
			CreatePositionMap(value);
			CreateOpacityMap(value.Count);
			m_ParticlesCount = value.Count;
			m_ParticleSystemBoundsSize = bounds.size;
			m_ParticleSystemBoundsCenter = bounds.center;
			UnityEngine.Debug.Log($"[CANDLES] VFXOwlcatMultiplePositionBinder setup completed. particlesCount:{m_ParticlesCount}, particleSystemBounds:({bounds})", this);
		}
	}

	private void CollectPositions(List<Color> positions, out Bounds bounds)
	{
		if (Targets == null || Targets.Count == 0)
		{
			UnityEngine.Debug.LogError($"[CANDLES] VFXOwlcatMultiplePositionBinder targets not specified. Using fallback setup (single particle at {kOutOfBoundsPosition})", this);
			positions.Add(kOutOfBoundsPositionColor);
			bounds = new Bounds(kOutOfBoundsPosition, new Vector3(1f, 1f, 1f));
			return;
		}
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		int num = 0;
		foreach (Transform target in Targets)
		{
			if (target != null)
			{
				Vector3 position = target.transform.position;
				vector = Vector3.Min(vector, position);
				vector2 = Vector3.Max(vector2, position);
				positions.Add(new Color(position.x, position.y, position.z));
			}
			else
			{
				positions.Add(kOutOfBoundsPositionColor);
				num++;
			}
		}
		if (vector.x <= vector2.x && vector.y <= vector2.y && vector.z <= vector2.z)
		{
			Vector3 vector3 = Vector3.Max(BoundsMargin, default(Vector3));
			vector -= vector3;
			vector2 += vector3;
			Vector3 vector4 = vector2 - vector;
			Vector3 center = vector + vector4 / 2f;
			bounds = new Bounds(center, vector4);
		}
		else
		{
			bounds = new Bounds(kOutOfBoundsPosition, new Vector3(1f, 1f, 1f));
		}
		if (num > 0)
		{
			UnityEngine.Debug.LogError($"[CANDLES] VFXOwlcatMultiplePositionBinder has invalid targets ({num}/{positions.Count})", this);
		}
	}

	private void Cleanup()
	{
		try
		{
			DestroyPositionMap();
			DestroyOpacityMap();
		}
		finally
		{
			m_ParticlesCount = 0;
			m_ParticleSystemBoundsSize = default(Vector3);
			m_ParticleSystemBoundsCenter = default(Vector3);
			m_PositionMapTexture = null;
			m_OpacityMapTexture = null;
			m_OpacityMapBuffer = default(NativeArray<ulong>);
			m_OpacityMapBufferChanged = false;
		}
	}

	private void CreatePositionMap(List<Color> positions)
	{
		m_PositionMapTexture = new Texture2D(positions.Count, 1, TextureFormat.RGBAFloat, mipChain: false);
		m_PositionMapTexture.hideFlags = HideFlags.DontSave;
		m_PositionMapTexture.name = base.gameObject.name + "_PositionMap";
		m_PositionMapTexture.filterMode = FilterMode.Point;
		m_PositionMapTexture.wrapMode = TextureWrapMode.Repeat;
		m_PositionMapTexture.SetPixels(positions.ToArray(), 0);
		m_PositionMapTexture.Apply();
	}

	private void DestroyPositionMap()
	{
		if (!(m_PositionMapTexture != null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy(m_PositionMapTexture);
			}
			else
			{
				Object.DestroyImmediate(m_PositionMapTexture);
			}
		}
	}

	private unsafe void CreateOpacityMap(int particlesCount)
	{
		int num = 8;
		int num2 = num * 8;
		int num3 = Mathf.CeilToInt((float)particlesCount / (float)num2);
		int num4 = num3 * num;
		m_OpacityMapBuffer = new NativeArray<ulong>(num3, Allocator.Persistent);
		UnsafeUtility.MemSet(m_OpacityMapBuffer.GetUnsafePtr(), byte.MaxValue, num4);
		m_OpacityMapTexture = new Texture2D(num4, 1, TextureFormat.R8, mipChain: false, linear: true);
		m_OpacityMapTexture.hideFlags = HideFlags.DontSave;
		m_OpacityMapTexture.filterMode = FilterMode.Point;
		m_OpacityMapTexture.anisoLevel = 0;
		m_OpacityMapTexture.wrapMode = TextureWrapMode.Clamp;
		m_OpacityMapTexture.LoadRawTextureData(m_OpacityMapBuffer);
		m_OpacityMapTexture.Apply(updateMipmaps: false);
	}

	private void DestroyOpacityMap()
	{
		if (m_OpacityMapBuffer.IsCreated)
		{
			m_OpacityMapBuffer.Dispose();
		}
		if (m_OpacityMapTexture != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(m_OpacityMapTexture);
			}
			else
			{
				Object.DestroyImmediate(m_OpacityMapTexture);
			}
		}
		m_OpacityMapBuffer = default(NativeArray<ulong>);
		m_OpacityMapTexture = null;
	}

	public override string ToString()
	{
		return $"Multiple Position Binder {m_ParticlesCount} positions";
	}

	private unsafe static void SetBufferBits(NativeArray<ulong> buffer, bool value, int pos, int numBits)
	{
		int b = buffer.Length * 8 * 8;
		ulong* unsafePtr = (ulong*)buffer.GetUnsafePtr();
		int num = Mathf.Min(pos + numBits, b);
		int num2 = pos >> 6;
		int num3 = pos & 0x3F;
		int num4 = num - 1 >> 6;
		int num5 = num & 0x3F;
		ulong num6 = (ulong)(-1L << num3);
		ulong num7 = ulong.MaxValue >> 64 - num5;
		ulong num8 = (ulong)(-(value ? 1 : 0));
		ulong num9 = num6 & num8;
		ulong num10 = num7 & num8;
		ulong num11 = ~num6;
		ulong num12 = ~num7;
		if (num2 == num4)
		{
			ulong num13 = ~(num6 & num7);
			ulong num14 = num9 & num10;
			unsafePtr[num2] = (unsafePtr[num2] & num13) | num14;
			return;
		}
		unsafePtr[num2] = (unsafePtr[num2] & num11) | num9;
		for (int i = num2 + 1; i < num4; i++)
		{
			unsafePtr[i] = num8;
		}
		unsafePtr[num4] = (unsafePtr[num4] & num12) | num10;
	}
}
