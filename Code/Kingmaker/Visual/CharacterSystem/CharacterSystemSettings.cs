using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterSystemSettings : ScriptableObject
{
	private static CharacterSystemSettings s_Instance;

	public Material DefataultMaterial;

	public Texture2D DefaultDiffuseTexture;

	public static CharacterSystemSettings Instance => s_Instance;

	public static string ConfigFilePath => "Assets/Editor Default Resources/CharacterSystemSettings.asset";
}
