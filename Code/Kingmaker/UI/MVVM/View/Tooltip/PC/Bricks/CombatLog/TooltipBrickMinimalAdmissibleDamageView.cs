using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickMinimalAdmissibleDamageView : TooltipBaseBrickView<TooltipBrickMinimalAdmissibleDamageVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	[SerializeField]
	private TextMeshProUGUI m_ResultValueText;

	[SerializeField]
	private TextMeshProUGUI m_MinimalAdmissibleDamageText;

	[SerializeField]
	private TextMeshProUGUI m_MinimalAdmissibleDamageValueText;

	[SerializeField]
	private TextMeshProUGUI m_ReasonsText;

	[SerializeField]
	private TextMeshProUGUI m_GameDifficultyText;

	protected override void BindViewImplementation()
	{
		m_HeaderText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamageHeader.Text;
		TextMeshProUGUI resultValueText = m_ResultValueText;
		int minimalAdmissibleDamage = base.ViewModel.MinimalAdmissibleDamage;
		resultValueText.text = "=" + minimalAdmissibleDamage;
		m_MinimalAdmissibleDamageText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamage.Text;
		TextMeshProUGUI minimalAdmissibleDamageValueText = m_MinimalAdmissibleDamageValueText;
		minimalAdmissibleDamage = base.ViewModel.MinimalAdmissibleDamage;
		minimalAdmissibleDamageValueText.text = minimalAdmissibleDamage.ToString();
		m_ReasonsText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamageReason.Text;
		GameDifficultyOption currentGameDifficulty = SettingsRoot.Difficulty.GameDifficulty.GetValue();
		DifficultyPresetAsset difficultyPresetAsset = BlueprintRoot.Instance.DifficultyList.Difficulties.First((DifficultyPresetAsset asset) => asset.Preset.GameDifficulty == currentGameDifficulty);
		m_GameDifficultyText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamageReasonValue.Text + " <b>" + difficultyPresetAsset.LocalizedName.Text + "</b>";
	}
}
