using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.SurfaceCombat;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public abstract class CombatStartWindowView : ViewBase<CombatStartWindowVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_CanDeployLabel;

	[SerializeField]
	private TextMeshProUGUI m_CannotDeployLabel;

	[SerializeField]
	protected OwlcatButton m_StartBattleButton;

	[SerializeField]
	private TextMeshProUGUI m_ButtonLabel;

	[SerializeField]
	private CombatStartCoopProgressBaseView m_ProgressBaseView;

	[SerializeField]
	private Canvas m_Canvas;

	private IDisposable m_CantStartBattleHint;

	public void Initialize()
	{
		UITurnBasedTexts turnBasedTexts = UIStrings.Instance.TurnBasedTexts;
		m_ButtonLabel.text = turnBasedTexts.StartBattle.Text;
		m_CanDeployLabel.text = turnBasedTexts.DeploymentPhaseBattle.Text;
		m_CannotDeployLabel.text = turnBasedTexts.CannotDeploy.Text;
		if (m_ProgressBaseView != null)
		{
			m_ProgressBaseView.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		Appear();
		m_CanDeployLabel.gameObject.SetActive(base.ViewModel.CanDeploy);
		m_CannotDeployLabel.gameObject.SetActive(!base.ViewModel.CanDeploy);
		m_Canvas.sortingOrder = ((!base.ViewModel.CanDeploy) ? 100 : 0);
		AddDisposable(base.ViewModel.CanStartCombat.Subscribe(delegate(bool can)
		{
			m_StartBattleButton.Interactable = can;
			if (can)
			{
				m_CantStartBattleHint?.Dispose();
			}
			else
			{
				m_CantStartBattleHint = m_StartBattleButton.SetHint(UIStrings.Instance.TurnBasedTexts.CannotStartbattle.Text);
			}
		}));
		if (m_ProgressBaseView != null)
		{
			m_ProgressBaseView.Bind(base.ViewModel.CoopProgressVM);
		}
	}

	private void Appear()
	{
		m_FadeAnimator.AppearAnimation();
	}

	private void Disappear()
	{
		m_FadeAnimator.DisappearAnimation();
	}

	protected override void DestroyViewImplementation()
	{
		Disappear();
		m_CantStartBattleHint?.Dispose();
	}
}
