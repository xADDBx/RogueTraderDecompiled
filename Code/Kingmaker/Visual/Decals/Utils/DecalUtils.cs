using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Visual.Decals.Utils;

public static class DecalUtils
{
	public static List<MeshRenderer> GetAffectedRenderers(Decal decal)
	{
		Bounds bounds = GetBounds(decal.transform);
		MeshRenderer[] array = Object.FindObjectsOfType<MeshRenderer>();
		List<MeshRenderer> list = new List<MeshRenderer>();
		MeshRenderer[] array2 = array;
		foreach (MeshRenderer meshRenderer in array2)
		{
			if (meshRenderer.gameObject.scene.name.EndsWith("_Static") && HasLayer(decal.AffectedLayers, meshRenderer.gameObject.layer) && meshRenderer.GetComponentInParent<Decal>() == null && bounds.Intersects(meshRenderer.bounds))
			{
				list.Add(meshRenderer);
			}
		}
		return list;
	}

	private static bool HasLayer(LayerMask mask, int layer)
	{
		return (mask.value & (1 << layer)) != 0;
	}

	private static Bounds GetBounds(Transform transform)
	{
		Vector3 lossyScale = transform.lossyScale;
		Vector3 vector = -lossyScale / 2f;
		Vector3 vector2 = lossyScale / 2f;
		Vector3[] source = new Vector3[8]
		{
			new Vector3(vector.x, vector.y, vector.z),
			new Vector3(vector2.x, vector.y, vector.z),
			new Vector3(vector.x, vector2.y, vector.z),
			new Vector3(vector2.x, vector2.y, vector.z),
			new Vector3(vector.x, vector.y, vector2.z),
			new Vector3(vector2.x, vector.y, vector2.z),
			new Vector3(vector.x, vector2.y, vector2.z),
			new Vector3(vector2.x, vector2.y, vector2.z)
		}.Select(transform.TransformDirection).ToArray();
		vector = source.Aggregate(Vector3.Min);
		vector2 = source.Aggregate(Vector3.Max);
		return new Bounds(transform.position, vector2 - vector);
	}
}
