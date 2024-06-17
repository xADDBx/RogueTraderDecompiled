using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class EquipSelectionSlotConsoleView : VirtualListElementViewBase<EquipSelectorSlotVM>, IHasTooltipTemplate, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	protected TMP_Text m_Text;

	[SerializeField]
	protected Image m_Icon;

	[Header("UsableStacks")]
	[SerializeField]
	private GameObject m_UsableStacksContainer;

	[SerializeField]
	private TextMeshProUGUI m_UsableStacksCount;

	public EquipSelectorSlotVM EquipVM => base.ViewModel;

	protected override void BindViewImplementation()
	{
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
		SetupName();
		SetupIcon();
		SetupUsableCharges();
	}

	private void SetupName()
	{
		if (!(m_Text == null))
		{
			m_Text.text = base.ViewModel.DisplayName;
		}
	}

	private void SetupIcon()
	{
		m_Icon.color = Color.white;
		m_Icon.sprite = base.ViewModel.Icon;
	}

	private void SetupUsableCharges()
	{
		int num = base.ViewModel.UsableCount?.Value ?? 0;
		m_UsableStacksContainer.Or(null)?.SetActive(num > 0);
		if ((bool)m_UsableStacksCount)
		{
			m_UsableStacksCount.text = num.ToString();
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
