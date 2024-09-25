using System;
using System.Collections.Generic;
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
	private OwlcatMultiButton m_Pause;

	[SerializeField]
	private List<Image> m_PauseMarks;

	[SerializeField]
	private Sprite m_PauseSprite;

	[SerializeField]
	private Sprite m_UnpauseSprite;

	[SerializeField]
	private OwlcatMultiButton m_NetRoles;

	[SerializeField]
	private Image m_NetRolesAttentionMark;

	private IDisposable m_PauseHintDisposable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetButtonsSounds();
		AddDisposable(m_Settings.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenEscMenu();
		}));
		AddDisposable(m_Settings.SetHint(UIStrings.Instance.MainMenu.Settings, "EscPressed"));
		AddDisposable(base.ViewModel.ShowPauseButton.Subscribe(m_Pause.transform.parent.gameObject.SetActive));
		AddDisposable(m_Pause.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Pause();
		}));
		SetButtonPauseSettings(base.ViewModel.IsPause.Value);
		AddDisposable(base.ViewModel.IsPause.Subscribe(SetButtonPauseSettings));
		AddDisposable(base.ViewModel.PlayerHaveRoles.CombineLatest(base.ViewModel.NetFirstLoadState, (bool haveRoles, bool netFirstLoadState) => new { haveRoles, netFirstLoadState }).Subscribe(value =>
		{
			m_NetRoles.transform.parent.gameObject.SetActive(value.netFirstLoadState);
			m_NetRolesAttentionMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
		}));
		AddDisposable(m_NetRoles.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OpenNetRoles();
		}));
		AddDisposable(m_NetRoles.SetHint(UIStrings.Instance.EscapeMenu.EscMenuRoles));
		AddDisposable(m_NetRolesAttentionMark.SetHint(UIStrings.Instance.NetRolesTexts.YouHaveNoRole));
	}

	protected override void DestroyViewImplementation()
	{
		m_PauseHintDisposable?.Dispose();
		m_PauseHintDisposable = null;
	}

	private void SetButtonPauseSettings(bool state)
	{
		m_PauseHintDisposable?.Dispose();
		m_PauseHintDisposable = null;
		m_Pause.SetActiveLayer(state ? "Paused" : "Normal");
		m_PauseHintDisposable = m_Pause.SetHint(state ? UIStrings.Instance.CommonTexts.Unpause : UIStrings.Instance.CommonTexts.Pause);
		m_PauseMarks.ForEach(delegate(Image m)
		{
			m.sprite = (state ? m_UnpauseSprite : m_PauseSprite);
		});
	}

	private void SetButtonsSounds()
	{
		UISounds.Instance.SetClickAndHoverSound(m_Settings, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_Pause, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_NetRoles, UISounds.ButtonSoundsEnum.PlastickSound);
	}
}
