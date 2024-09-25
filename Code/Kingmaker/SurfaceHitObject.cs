using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Visual.SceneHelpers;
using UnityEngine;

namespace Kingmaker;

public class SurfaceHitObject : MonoBehaviour
{
	public SurfaceType _SoundSurfaceType = SurfaceType.Metal;

	[InspectorReadOnly]
	public Collider[] _Colliders;

	private void OnValidate()
	{
	}

	public void Validate(StaticPrefab forcedStaticPrefab = null)
	{
		if (this == null || TryGetComponent<TerrainCollider>(out var _))
		{
			return;
		}
		StaticPrefab staticPrefab = ((forcedStaticPrefab != null) ? forcedStaticPrefab : GetComponentInParent<StaticPrefab>(includeInactive: true));
		if (staticPrefab == null)
		{
			Debug.LogError(base.name + ": Cannot find StaticPrefab!");
			return;
		}
		if (!staticPrefab.SurfaceHitObjects.Contains(this))
		{
			staticPrefab.SurfaceHitObjects.Add(this);
		}
		string text = $"{staticPrefab.name}_{_SoundSurfaceType}_SurfaceHit_Collider";
		if (base.gameObject.name != text)
		{
			base.gameObject.name = text;
		}
		if (_Colliders == null || _Colliders.Length == 0)
		{
			CollectColliders();
		}
		int num = LayerMask.NameToLayer("SurfaceHit");
		if (base.gameObject.layer != num)
		{
			base.gameObject.layer = num;
		}
	}

	public void CollectColliders()
	{
		_Colliders = GetComponents<Collider>();
	}

	public bool ContainsColliderForMesh(Mesh mesh, out MeshCollider meshCollider)
	{
		Collider[] colliders = _Colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i] is MeshCollider meshCollider2 && meshCollider2.sharedMesh.Equals(mesh))
			{
				meshCollider = meshCollider2;
				return true;
			}
		}
		meshCollider = null;
		return false;
	}
}
