using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Settings.Difficulty;

[CreateAssetMenu(menuName = "Settings/Difficulty preset")]
public class DifficultyPresetAsset : ScriptableObject
{
	[Tooltip("Use for UI")]
	public bool IsCustomMode;

	[HideIf("IsCustomMode")]
	public DifficultyPreset Preset;

	[Header("UI Block")]
	[ValidateNotNull]
	public Sprite Icon;

	public LocalizedString LocalizedName;

	public LocalizedString LocalizedDescription;
}
