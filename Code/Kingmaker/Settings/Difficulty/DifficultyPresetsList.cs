using System.Collections.Generic;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Settings.Difficulty;

[CreateAssetMenu(menuName = "Settings/Difficulty presets list")]
public class DifficultyPresetsList : ScriptableObject
{
	[SerializeField]
	private DifficultyPresetAsset[] m_Difficulties;

	[ValidateNotNull]
	[SerializeField]
	private DifficultyPresetAsset m_CoreDifficulty;

	[ValidateNotNull]
	[SerializeField]
	private DifficultyPresetAsset m_HardDifficulty;

	[ValidateNotNull]
	[SerializeField]
	private DifficultyPresetAsset m_UnfairDifficulty;

	public IReadOnlyList<DifficultyPresetAsset> Difficulties => m_Difficulties;

	public DifficultyPresetAsset CoreDifficulty => m_CoreDifficulty;

	public DifficultyPresetAsset HardDifficulty => m_HardDifficulty;

	public DifficultyPresetAsset UnfairDifficulty => m_UnfairDifficulty;
}
