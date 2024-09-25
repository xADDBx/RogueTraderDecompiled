using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

[ExecuteInEditMode]
public class ScreenSpaceDecal : MonoBehaviour
{
	public enum DecalType
	{
		Default,
		GUI
	}

	private static HashSet<ScreenSpaceDecal> s_All = new HashSet<ScreenSpaceDecal>();

	private MaterialPropertyBlock m_MaterialProperties;

	private float m_CreationTime;

	[SerializeField]
	[HideInInspector]
	protected Material m_Material;

	[SerializeField]
	private int m_Layer;

	[SerializeField]
	private bool m_ValidateHeight;

	[SerializeField]
	private LayerMask m_RaycastMask;

	[NonSerialized]
	[HideInInspector]
	public BoundingSphere BoundingSphere;

	private Renderer[] m_Renderers = Array.Empty<Renderer>();

	public static IEnumerable<ScreenSpaceDecal> All
	{
		get
		{
			foreach (ScreenSpaceDecal item in s_All)
			{
				yield return item;
			}
		}
	}

	public virtual DecalType Type => DecalType.Default;

	public virtual bool IsVisible => true;

	public virtual bool IsFullScreen => false;

	public MaterialPropertyBlock MaterialProperties => m_MaterialProperties ?? (m_MaterialProperties = new MaterialPropertyBlock());

	public Material SharedMaterial => m_Material;

	public int Layer
	{
		get
		{
			return m_Layer;
		}
		set
		{
			m_Layer = value;
		}
	}

	public float CreationTime => m_CreationTime;

	protected virtual void Awake()
	{
		m_CreationTime = Time.timeSinceLevelLoad;
	}

	protected virtual void OnEnable()
	{
		s_All.Add(this);
		m_Renderers = GetComponentsInChildren<Renderer>();
	}

	protected virtual void OnDisable()
	{
		s_All.Remove(this);
		m_Renderers = Array.Empty<Renderer>();
	}

	protected virtual void Update()
	{
		if (m_ValidateHeight && !IsFullScreen)
		{
			ValidateHeight();
		}
		if (base.transform.hasChanged)
		{
			BoundingSphere.position = base.transform.position;
			BoundingSphere.radius = base.transform.lossyScale.magnitude * 0.5f;
			base.transform.hasChanged = false;
		}
		if (m_MaterialProperties != null)
		{
			Renderer[] renderers = m_Renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].SetPropertyBlock(m_MaterialProperties);
			}
		}
	}

	private void ValidateHeight()
	{
		Vector3 localScale = base.transform.localScale;
		float num = Mathf.Max(localScale.x, localScale.z);
		Vector3 position = base.transform.position;
		float num2 = position.y - localScale.y * 0.5f;
		float num3 = position.y + localScale.y * 0.5f;
		float num4 = 100f;
		float num5 = num4 * 0.5f;
		Ray ray = default(Ray);
		ray.direction = Vector3.down;
		ray.origin = new Vector3(position.x + num, position.y + num5, position.z + num);
		if (Physics.Raycast(ray, out var hitInfo, num4, m_RaycastMask))
		{
			Vector3 point = hitInfo.point;
			if (num2 > point.y)
			{
				num2 = point.y;
			}
			if (num3 < point.y)
			{
				num3 = point.y;
			}
		}
		ray.origin = new Vector3(position.x - num, position.y + num5, position.z + num);
		if (Physics.Raycast(ray, out hitInfo, num4, m_RaycastMask))
		{
			Vector3 point2 = hitInfo.point;
			if (num2 > point2.y)
			{
				num2 = point2.y;
			}
			if (num3 < point2.y)
			{
				num3 = point2.y;
			}
		}
		ray.origin = new Vector3(position.x + num, position.y + num5, position.z - num);
		if (Physics.Raycast(ray, out hitInfo, num4, m_RaycastMask))
		{
			Vector3 point3 = hitInfo.point;
			if (num2 > point3.y)
			{
				num2 = point3.y;
			}
			if (num3 < point3.y)
			{
				num3 = point3.y;
			}
		}
		ray.origin = new Vector3(position.x - num, position.y + num5, position.z - num);
		if (Physics.Raycast(ray, out hitInfo, num4, m_RaycastMask))
		{
			Vector3 point4 = hitInfo.point;
			if (num2 > point4.y)
			{
				num2 = point4.y;
			}
			if (num3 < point4.y)
			{
				num3 = point4.y;
			}
		}
		localScale.y = Mathf.Max(num3 - num2);
		base.transform.localScale = localScale;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, Quaternion.Euler(0f, 45f, 0f), Vector3.one);
		Gizmos.DrawWireSphere(default(Vector3), BoundingSphere.radius);
	}
}
