using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual;

[RequireComponent(typeof(LightProbeGroup))]
public class LightProbesGenerator : MonoBehaviour
{
	private LightProbeGroup m_LightProbeGroup;

	public Bounds Bounds;

	[Range(0.1f, 5f)]
	public float GridStep = 1f;

	[Range(1f, 5f)]
	public int LevelCount = 2;

	[Range(0.1f, 5f)]
	public float LevelHeight = 2f;

	public float OffsetY = 0.1f;

	public LayerMask RaycastMask;

	private void Awake()
	{
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		base.transform.localScale = Vector3.one;
	}

	public void GenerateProbes()
	{
		if (m_LightProbeGroup == null)
		{
			m_LightProbeGroup = GetComponent<LightProbeGroup>();
		}
		MeshRenderer[] array = Object.FindObjectsOfType<MeshRenderer>();
		List<MeshCollider> list = new List<MeshCollider>();
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if (meshRenderer.GetComponent<Collider>() == null)
			{
				list.Add(meshRenderer.gameObject.AddComponent<MeshCollider>());
			}
		}
		List<Vector3> list2 = new List<Vector3>();
		float num = Bounds.min.x;
		float num2 = Bounds.min.z;
		Vector3 max = Bounds.max;
		Ray ray = new Ray(default(Vector3), Vector3.down);
		for (; num2 < max.z; num2 += GridStep)
		{
			for (; num < max.x; num += GridStep)
			{
				ray.origin = new Vector3(num, max.y + 10f, num2);
				if (Physics.Raycast(ray, out var hitInfo, 100f, RaycastMask))
				{
					for (int j = 0; j < LevelCount; j++)
					{
						Vector3 point = hitInfo.point;
						point.y += OffsetY + (float)j * LevelHeight;
						list2.Add(point);
					}
				}
			}
			num = Bounds.min.x;
		}
		base.transform.position = default(Vector3);
		foreach (MeshCollider item in list)
		{
			Object.DestroyImmediate(item);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Bounds.center, Bounds.size);
	}
}
