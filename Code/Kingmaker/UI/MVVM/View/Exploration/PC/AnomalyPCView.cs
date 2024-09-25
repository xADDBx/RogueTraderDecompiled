using Kingmaker.Code.UI.MVVM.View.StatCheckLoot.PC;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class AnomalyPCView : AnomalyBaseView<StatCheckLootPCView>
{
	[SerializeField]
	private TextMeshProUGUI m_AnomalyName;

	[SerializeField]
	private TextMeshProUGUI m_AnomalyDescription;

	[SerializeField]
	private GameObject m_FullyScannedContainer;

	[SerializeField]
	private TextMeshProUGUI m_VisitAnomalyButtonText;

	[SerializeField]
	private OwlcatButton m_VisitAnomalyButton;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private FadeAnimator m_Animator;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsFullyScanned.Subscribe(delegate(bool val)
		{
			m_FullyScannedContainer.SetActive(val);
		}));
		AddDisposable(base.ViewModel.AnomalyName.Subscribe(delegate(string val)
		{
			m_AnomalyName.text = val;
		}));
		AddDisposable(base.ViewModel.AnomalyDescription.Subscribe(delegate(string val)
		{
			m_AnomalyDescription.text = val;
		}));
		AddDisposable(base.ViewModel.VisitButtonLabel.Subscribe(delegate(string val)
		{
			m_VisitAnomalyButtonText.text = val;
		}));
		AddDisposable(m_VisitAnomalyButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			VisitAnomaly();
		}));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HideWindow();
		}));
		AddDisposable(base.ViewModel.Show.Subscribe(delegate
		{
			ShowWindow();
		}));
		AddDisposable(base.ViewModel.Hide.Subscribe(delegate
		{
			HideWindow();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void VisitAnomaly()
	{
		HideWindow();
		base.ViewModel.VisitAnomaly();
	}

	private void ShowWindow()
	{
		EscHotkeyManager.Instance.Subscribe(HideWindow);
		m_Animator.AppearAnimation();
	}

	private void HideWindow()
	{
		EscHotkeyManager.Instance.Unsubscribe(HideWindow);
		m_Animator.DisappearAnimation();
	}
}
