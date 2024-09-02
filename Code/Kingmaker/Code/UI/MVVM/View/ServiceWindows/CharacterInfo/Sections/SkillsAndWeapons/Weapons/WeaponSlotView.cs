using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class WeaponSlotView : ViewBase<WeaponSlotVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_Slot;

	[SerializeField]
	private Image m_ItemImage;

	public OwlcatMultiButton Slot => m_Slot;

	protected override void BindViewImplementation()
	{
		m_ItemImage.sprite = base.ViewModel.Icon;
		m_ItemImage.gameObject.SetActive(base.ViewModel.Icon != null);
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_Slot.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Slot.IsValid();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}
