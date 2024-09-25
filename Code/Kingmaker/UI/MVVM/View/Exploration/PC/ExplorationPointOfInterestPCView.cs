using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationPointOfInterestPCView : ExplorationPointOfInterestBaseView
{
	[SerializeField]
	private OwlcatButton m_Button;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(HandleClick));
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(HandleClick));
		AddDisposable(m_Button.OnHoverAsObservable().Subscribe(base.AnimateHover));
		AddDisposable(m_Button.OnFocusAsObservable().Subscribe(base.AnimateHover));
	}

	protected override void SetFocusImpl(bool value)
	{
		m_Button.SetFocus(value);
	}

	private void HandleClick()
	{
		base.ViewModel.Interact();
	}

	protected override void SetHintImpl(string stateText)
	{
		string text = ((base.ViewModel.PointOfInterestBlueprintType.Value is BlueprintPointOfInterestGroundOperation) ? base.ViewModel.Name.Value : $"{stateText}\n<b><color=#{ColorUtility.ToHtmlStringRGB(m_HeaderColor)}>{base.ViewModel.Name}");
		Hint = m_Icon.SetHint(text);
	}
}
