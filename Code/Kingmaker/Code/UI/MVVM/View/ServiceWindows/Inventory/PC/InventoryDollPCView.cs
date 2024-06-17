using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.PC;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.PC;

public class InventoryDollPCView : InventoryDollView<InventoryEquipSlotPCView>
{
	[Header("Character Visual Settings")]
	[SerializeField]
	private OwlcatButton m_VisualSettingsViewButton;

	[SerializeField]
	private CharacterVisualSettingsPCView m_VisualSettingsPCView;

	public override void Initialize()
	{
		base.Initialize();
		if (m_VisualSettingsPCView != null)
		{
			m_VisualSettingsPCView.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_VisualSettingsPCView != null)
		{
			UISounds.Instance.SetClickAndHoverSound(m_VisualSettingsViewButton, UISounds.ButtonSoundsEnum.PlastickSound);
			AddDisposable(m_VisualSettingsViewButton.SetHint(UIStrings.Instance.CharGen.ShowVisualSettings));
			AddDisposable(base.ViewModel.VisualSettingsVM.Subscribe(m_VisualSettingsPCView.Bind));
			AddDisposable(m_VisualSettingsViewButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.SwitchVisualSettings();
			}));
		}
	}
}
