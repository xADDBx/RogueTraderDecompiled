using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.VFXGraph;

public class VFXOwlcatTransformTarget : MonoBehaviour
{
	public bool LightDependency = true;

	public List<Transform> Targets = new List<Transform>();
}
