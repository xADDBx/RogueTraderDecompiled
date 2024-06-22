using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console;

public class NetLobbyCreateJoinPartConsoleView : NetLobbyCreateJoinPartBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private ConsoleHint m_SelectRegionHint;

	[SerializeField]
	private ConsoleHint m_PasteLobbyIdHint;

	[SerializeField]
	private ConsoleHint m_ShowLobbyCodeHint;

	[SerializeField]
	private ConsoleHint m_EnterLobbyIdHint;

	[SerializeField]
	private ConsoleHint m_JoinableUserTypesHint;

	[SerializeField]
	private ConsoleHint m_InvitableUserTypesHint;

	[SerializeField]
	private OwlcatMultiButton m_CreateBlockFocusButton;

	[SerializeField]
	private OwlcatMultiButton m_JoinBlockFocusButton;

	[SerializeField]
	private TextMeshProUGUI m_CreateLobbyLabel;

	private readonly BoolReactiveProperty m_InputFieldIsFocused = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CreateBlockIsFocused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ConsoleInputField.Bind(string.Empty, delegate
		{
		});
		m_CreateLobbyLabel.text = UIStrings.Instance.NetLobbyTexts.CreateLobby;
	}

	protected override void DestroyViewImplementation()
	{
		m_CreateBlockIsFocused.Value = false;
		m_InputFieldIsFocused.Value = false;
		base.DestroyViewImplementation();
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).ToReactiveProperty()), UIStrings.Instance.CharGen.Back));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(state: false);
		}, 9, m_InputFieldIsFocused.And(IsInCreateJoinPart).ToReactiveProperty()), UIStrings.Instance.SettingsUI.Cancel));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowNetLobbyTutorial();
		}, 19, base.ViewModel.IsAnyTutorialBlocks.And(IsInCreateJoinPart).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.NetLobbyTexts.HowToPlay));
		AddDisposable(m_SelectRegionHint.Bind(inputLayer.AddButton(delegate
		{
			m_RegionDropdown.SetState(value: true);
		}, 17, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).ToReactiveProperty())));
		m_SelectRegionHint.SetLabel(UIStrings.Instance.NetLobbyTexts.SelectRegion);
		AddDisposable(m_ShowLobbyCodeHint.Bind(inputLayer.AddButton(delegate
		{
			ShowLobbyCode.Value = !ShowLobbyCode.Value;
		}, 16, IsInCreateJoinPart, InputActionEventType.ButtonJustLongPressed)));
		m_ShowLobbyCodeHint.SetLabel(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode);
		AddDisposable(ShowLobbyCode.Subscribe(delegate(bool state)
		{
			m_ShowLobbyCodeHint.SetLabel(state ? UIStrings.Instance.NetLobbyTexts.HideLobbyCode : UIStrings.Instance.NetLobbyTexts.ShowLobbyCode);
		}));
		AddDisposable(m_PasteLobbyIdHint.Bind(inputLayer.AddButton(delegate
		{
			m_ConsoleInputField.Text = base.ViewModel.GetCopiedLobbyId();
		}, 16, IsInCreateJoinPart, InputActionEventType.ButtonJustReleased)));
		m_PasteLobbyIdHint.SetLabel(UIStrings.Instance.NetLobbyTexts.PasteLobbyId);
		AddDisposable(m_EnterLobbyIdHint.Bind(inputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(state: true);
		}, 10, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused.Not())
			.ToReactiveProperty())));
		m_EnterLobbyIdHint.SetLabel(UIStrings.Instance.NetLobbyTexts.JoinLobbyCodePlaceholder);
		AddDisposable(m_JoinableUserTypesHint.Bind(inputLayer.AddButton(delegate
		{
			m_JoinableUserTypesDropdown.SetState(value: true);
		}, 10, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused)
			.ToReactiveProperty())));
		AddDisposable(m_InvitableUserTypesHint.Bind(inputLayer.AddButton(delegate
		{
			m_InvitableUserTypesDropdown.SetState(value: true);
		}, 11, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused)
			.ToReactiveProperty())));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.CreateLobby();
		}, 8, m_InputFieldIsFocused.Not().And(IsInCreateJoinPart).And(m_CreateBlockIsFocused)
			.ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.CreateLobby));
		AddDisposable(hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			if (m_InputFieldIsFocused.Value)
			{
				ActivateDeactivateInputField(state: false);
			}
			base.ViewModel.JoinLobby();
		}, 8, ReadyToJoin.And(IsInCreateJoinPart).And(m_CreateBlockIsFocused.Not()).ToReactiveProperty()), UIStrings.Instance.NetLobbyTexts.JoinLobby));
	}

	public void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		List<IConsoleEntity> entities = new List<IConsoleEntity> { m_CreateBlockFocusButton, m_JoinBlockFocusButton };
		navigationBehaviour.AddRow(entities);
		AddDisposable(IsInCreateJoinPart.Subscribe(delegate(bool state)
		{
			if (state)
			{
				DelayedInvoker.InvokeInFrames(delegate
				{
					navigationBehaviour.FocusOnEntityManual(m_CreateBlockFocusButton);
				}, 1);
			}
			else
			{
				navigationBehaviour.UnFocusCurrentEntity();
			}
		}));
		AddDisposable(navigationBehaviour.Focus.Subscribe(OnFocusEntity));
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CreateBlockIsFocused.Value = entity as OwlcatMultiButton == m_CreateBlockFocusButton && IsInCreateJoinPart.Value;
	}

	private void ActivateDeactivateInputField(bool state)
	{
		if (state)
		{
			m_ConsoleInputField.Select();
		}
		else
		{
			m_ConsoleInputField.Abort();
		}
		m_InputFieldIsFocused.Value = state;
	}
}
