using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickArmorStatsView : TooltipBaseBrickView<TooltipBrickArmorStatsVM>
{
	[Space]
	[SerializeField]
	private TextMeshProUGUI m_DeflectionLabel;

	[SerializeField]
	private TextMeshProUGUI m_DeflectionValue;

	[SerializeField]
	private Image m_DeflectionTooltip;

	[Space]
	[SerializeField]
	private TextMeshProUGUI m_AbsorptionLabel;

	[SerializeField]
	private TextMeshProUGUI m_AbsorptionValue;

	[SerializeField]
	private Image m_AbsorptionTooltip;

	[Space]
	[SerializeField]
	private TextMeshProUGUI m_DodgeLabel;

	[SerializeField]
	private TextMeshProUGUI m_DodgeValue;

	[SerializeField]
	private Image m_DodgeTooltip;

	protected override void BindViewImplementation()
	{
		m_DeflectionLabel.text = UIStrings.Instance.CharacterSheet.ArmorDeflection;
		m_AbsorptionLabel.text = UIStrings.Instance.CharacterSheet.ArmorAbsorption;
		m_DodgeLabel.text = UIStrings.Instance.CharacterSheet.Dodge;
		m_DeflectionValue.text = base.ViewModel.ArmorAbsorption;
		m_AbsorptionValue.text = base.ViewModel.ArmorDeflection;
		m_DodgeValue.text = base.ViewModel.Dodge;
		SetTooltips();
	}

	private void SetTooltips()
	{
		AddDisposable(m_DeflectionTooltip.SetGlossaryTooltip("Deflection"));
		AddDisposable(m_AbsorptionTooltip.SetGlossaryTooltip("Absorption"));
		AddDisposable(m_DodgeTooltip.SetTooltip(base.ViewModel.DodgeTooltip));
	}
}
