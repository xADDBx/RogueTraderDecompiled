using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting;

public class OccludedObjectDepthClipper : MonoBehaviour
{
	[SerializeField]
	public float m_Radius = 5f;

	[SerializeField]
	private float m_OffsetToCamera;

	public static HashSet<OccludedObjectDepthClipper> All { get; private set; }

	public float Radius
	{
		get
		{
			return m_Radius;
		}
		set
		{
			m_Radius = value;
		}
	}

	public float OffsetToCamera
	{
		get
		{
			return m_OffsetToCamera;
		}
		set
		{
			m_OffsetToCamera = value;
		}
	}

	static OccludedObjectDepthClipper()
	{
		All = new HashSet<OccludedObjectDepthClipper>();
	}

	private void OnEnable()
	{
		All.Add(this);
	}

	private void OnDisable()
	{
		All.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, m_Radius);
		Gizmos.color = color;
	}
}
