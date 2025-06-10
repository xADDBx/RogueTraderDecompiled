using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickProtocolPetView : TooltipBaseBrickView<TooltipBrickProtocolPetVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	private Image m_protocolIcon;

	[SerializeField]
	private TextMeshProUGUI m_protocolName;

	[SerializeField]
	private OwlcatMultiButton m_Protocol;

	protected TooltipTemplateItem m_Tooltip;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_protocolIcon.sprite = base.ViewModel.ProtocolIcon;
		m_protocolName.text = base.ViewModel.ProtocolName;
		m_Tooltip = new TooltipTemplateItem(base.ViewModel.Item);
		AddDisposable(m_Protocol.SetTooltip(m_Tooltip));
	}

	public void SetFocus(bool value)
	{
		m_Protocol.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Protocol.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_Tooltip;
	}
}
