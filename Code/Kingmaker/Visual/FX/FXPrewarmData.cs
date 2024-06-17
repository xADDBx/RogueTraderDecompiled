using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.FX;

[CreateAssetMenu(fileName = "FXPrewarmData", menuName = "Techart/FX Prewarm Data")]
public class FXPrewarmData : ScriptableObject
{
	public float Duration;

	public List<GameObject> Prefabs = new List<GameObject>();
}
