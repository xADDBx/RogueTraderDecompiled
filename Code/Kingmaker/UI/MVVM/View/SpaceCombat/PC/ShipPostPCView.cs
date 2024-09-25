using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipPostPCView : ShipPostBaseView
{
	[Header("PC")]
	[SerializeField]
	private OwlcatButton m_MainButton;

	[SerializeField]
	private Color m_PortraitEmptyColor;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private AbilitiesGroupPCView m_AbilitiesGroupPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Portrait.color = ((m_Portrait.sprite == null) ? m_PortraitEmptyColor : Color.white);
		UISpaceCombatTexts.SpacePostStrings postStrings = UIStrings.Instance.SpaceCombatTexts.GetPostStrings(base.ViewModel.Index);
		m_MainButton.SetTooltip(new TooltipTemplateSimple(postStrings.Title, postStrings.Description));
		m_Title.text = postStrings.Title;
		m_AbilitiesGroupPCView.Bind(base.ViewModel.AbilitiesGroup);
	}
}
