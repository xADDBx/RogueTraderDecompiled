using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class CharInfoNameAndPortraitPCView : CharInfoComponentWithLevelUpView<CharInfoNameAndPortraitVM>
{
	private enum PortraitSize
	{
		Small,
		Middle,
		Full
	}

	[SerializeField]
	private ScrambledTMP m_NameFieldScrambled;

	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[SerializeField]
	private CharInfoHitPointsPCView m_HitPointsView;

	[SerializeField]
	protected OwlcatButton m_NextButton;

	[SerializeField]
	protected OwlcatButton m_PrevButton;

	[SerializeField]
	private PortraitSize m_Size;

	public override void Initialize()
	{
		base.Initialize();
		m_HitPointsView?.Initialize();
		m_Portrait.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_NextButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_PrevButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SelectNextCharacter();
		}));
		AddDisposable(m_PrevButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SelectPrevCharacter();
		}));
		AddDisposable(base.ViewModel.UnitName.Subscribe(delegate
		{
			SetName();
		}));
		AddDisposable(base.ViewModel.GroupCount.Subscribe(delegate(int count)
		{
			m_NextButton.SetInteractable(count > 1);
			m_PrevButton.SetInteractable(count > 1);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Portrait.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		SetName();
		SetPortrait();
		SetHP();
	}

	private void SetName()
	{
		string value = base.ViewModel.UnitName.Value;
		if (m_NameFieldScrambled != null && m_NameFieldScrambled.Text != value)
		{
			m_NameFieldScrambled.SetText(string.Empty, value);
		}
	}

	private void SetPortrait()
	{
		switch (m_Size)
		{
		case PortraitSize.Small:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitSmall);
			break;
		case PortraitSize.Middle:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitHalf);
			break;
		case PortraitSize.Full:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitFull);
			break;
		}
	}

	private void SetHP()
	{
		m_HitPointsView?.Bind(base.ViewModel.HitPoints);
	}

	protected override void OnShow()
	{
		m_Portrait.gameObject.SetActive(value: true);
	}

	protected override void OnHide()
	{
		UISounds.Instance.Sounds.Character.CharacterStatsHide.Play();
		m_Portrait.gameObject.SetActive(value: false);
	}
}
