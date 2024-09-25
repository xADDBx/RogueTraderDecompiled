using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Settings.Graphics;

[CreateAssetMenu(menuName = "Settings/Graphics preset")]
public class GraphicsPresetAsset : ScriptableObject
{
	[Tooltip("Use for UI")]
	public bool IsCustomMode;

	[HideIf("IsCustomMode")]
	public GraphicsPreset Preset;
}
