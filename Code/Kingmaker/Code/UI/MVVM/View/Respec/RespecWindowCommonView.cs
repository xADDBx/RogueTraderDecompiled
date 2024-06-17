using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Retrain;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Respec;

public class RespecWindowCommonView : ViewBase<RespecVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_RespecCost;

	[SerializeField]
	private TextMeshProUGUI m_WarningLabel;

	[SerializeField]
	protected RespecCharactersSelectorView m_RespecCharactersSelectorView;

	[SerializeField]
	protected SystemMapSpaceResourcesPCView m_SystemMapSpaceResourcesView;

	public void Initialize()
	{
		m_Animator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_HeaderLabel.text = UIStrings.Instance.CharGen.RespecWindowHeader;
		m_WarningLabel.text = UIStrings.Instance.CharGen.RespecWindowWarning;
		m_Animator.AppearAnimation();
		m_RespecCharactersSelectorView.Bind(base.ViewModel.CharacterSelectionGroupRadioVM);
		m_SystemMapSpaceResourcesView.Or(null)?.Bind(base.ViewModel.SystemMapSpaceResourcesVM);
		AddDisposable(base.ViewModel.RespecCost.Subscribe(delegate(int value)
		{
			m_RespecCost.text = FormatCost(value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_Animator.DisappearAnimation();
	}

	protected void CloseWindow()
	{
		base.ViewModel.OnClose();
	}

	protected void OnConfirm()
	{
		base.ViewModel.OnConfirm();
	}

	private string FormatCost(int cost)
	{
		if (cost <= 0)
		{
			return "0";
		}
		return "-" + cost;
	}
}
