using System.Collections.Generic;
using Kingmaker;
using UnityEngine;

namespace Owlcat.Runtime.Visual.SceneHelpers;

public class StaticPrefab : MonoBehaviour
{
	public GameObject LightsRoot;

	public GameObject VisualRoot;

	public List<ShadowProxy> ShadowProxies;

	public List<SurfaceHitObject> SurfaceHitObjects;
}
