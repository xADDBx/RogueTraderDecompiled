using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.IngameMenu.PC;

public class IngameMenuSettingsButtonPCView : IngameMenuBasePCView<IngameMenuSettingsButtonVM>
{
	[Header("Buttons Part")]
	[SerializeField]
	private OwlcatMultiButton m_Settings;

	[SerializeField]
	private OwlcatMultiButton m_NetRoles;

	[SerializeField]
	private Image m_NetRolesAttentionMark;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_Settings, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_Settings.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenEscMenu();
		}));
		AddDisposable(m_Settings.SetHint(UIStrings.Instance.MainMenu.Settings, "EscPressed"));
		AddDisposable(base.ViewModel.PlayerHaveRoles.CombineLatest(base.ViewModel.NetFirstLoadState, (bool haveRoles, bool netFirstLoadState) => new { haveRoles, netFirstLoadState }).Subscribe(value =>
		{
			m_NetRoles.transform.parent.gameObject.SetActive(value.netFirstLoadState);
			m_NetRolesAttentionMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
		}));
		UISounds.Instance.SetClickAndHoverSound(m_NetRoles, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_NetRoles.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenNetRoles();
		}));
		AddDisposable(m_NetRoles.SetHint(UIStrings.Instance.EscapeMenu.EscMenuRoles));
		AddDisposable(m_NetRolesAttentionMark.SetHint(UIStrings.Instance.NetRolesTexts.YouHaveNoRole));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
