using Kingmaker.Settings.Difficulty;
using Kingmaker.Settings.Graphics;
using UnityEngine;

namespace Kingmaker.Settings;

[CreateAssetMenu(menuName = "Settings/Settings values")]
public class SettingsValues : ScriptableObject
{
	public SettingsDefaultValues SettingsDefaultValues;

	public DifficultyPresetsList DifficultiesPresets;

	public GraphicsPresetsList GraphicsPresetsList;

	public TexturesQualityController.Settings TexturesQualityControllerSettings;

	private void OnValidate()
	{
		SettingsDefaultValues.OnValidate();
	}
}
