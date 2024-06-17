using UnityEngine;

namespace Kingmaker;

[CreateAssetMenu(menuName = "ScriptableObjects/SolarSystemOrbitVisualConfig")]
public class SolarSystemOrbitVisualConfig : ScriptableObject
{
	public LineRenderer OrbitVisualPrefab;

	public LineRenderer OrbitVisualPrefabThin;
}
