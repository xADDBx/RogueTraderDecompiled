using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class GamerTagAndNameBaseView : ViewBase<GamerTagAndNameVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_PLayerName;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	private readonly BoolReactiveProperty m_IsFocused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		m_PLayerName.text = string.Empty;
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_PLayerName.text = value;
			PFLog.Net.Log("GamerTagAndNameBaseView SET NAME " + value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void ShowOrHide(bool state)
	{
		base.gameObject.SetActive(state);
	}

	public void SetFocus(bool value)
	{
		m_IsFocused.Value = value;
		m_Selectable.SetFocus(value);
	}

	public bool IsValid()
	{
		if (base.gameObject.activeSelf)
		{
			return !string.IsNullOrWhiteSpace(base.ViewModel.Name.Value);
		}
		return false;
	}

	public void AddGamerTagInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action closeGamersTagModeAction, BoolReactiveProperty canConfirmLaunch = null)
	{
		if (canConfirmLaunch != null)
		{
			AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 9, m_IsFocused.And(canConfirmLaunch.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.Back));
			AddDisposable(inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 19, m_IsFocused.And(canConfirmLaunch.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased));
			AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				base.ViewModel.ShowGamerCard();
			}, 8, m_IsFocused.And(canConfirmLaunch.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.ShowGamerCard));
		}
		else
		{
			AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 9, m_IsFocused, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CharGen.Back));
			AddDisposable(inputLayer.AddButton(delegate
			{
				closeGamersTagModeAction();
			}, 19, m_IsFocused, InputActionEventType.ButtonJustReleased));
			AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
			{
				base.ViewModel.ShowGamerCard();
			}, 8, m_IsFocused, InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.ShowGamerCard));
		}
	}

	public string GetUserId()
	{
		return base.ViewModel.UserId.Value;
	}
}
