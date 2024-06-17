using UnityEngine;

namespace Kingmaker.UnitLogic.Scene;

[CreateAssetMenu(menuName = "Import/Props Settings")]
public class PropsSettingsPreset : ScriptableObject
{
	public string[] Obstacles;

	public string[] Walkable;

	public string[] Visual;
}
