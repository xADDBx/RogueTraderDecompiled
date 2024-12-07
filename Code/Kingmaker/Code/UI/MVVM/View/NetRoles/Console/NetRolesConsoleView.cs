using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.View.Common.InputField;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.EscMenu;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NetRoles.Console;

public class NetRolesConsoleView : NetRolesBaseView, INetRolesConsoleHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private List<NetRolesPlayerConsoleView> m_Players;

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_PlayerListNavigationBehaviour;

	private InputLayer m_GamersTagsInputLayer;

	private GridConsoleNavigationBehaviour m_GamersTagsNavigationBehavior;

	private readonly BoolReactiveProperty m_GamersTagsMode = new BoolReactiveProperty();

	public static readonly string InputLayerGamersTagsContextName = "GamersTags";

	public override void Initialize()
	{
		base.Initialize();
		m_Players.ForEach(delegate(NetRolesPlayerConsoleView p)
		{
			p.Initialize();
		});
	}

	protected override void BindViewImplementation()
	{
		for (int i = 0; i < m_Players.Count; i++)
		{
			m_Players[i].Bind((base.ViewModel.PlayerVms.Count > i) ? base.ViewModel.PlayerVms[i] : null);
		}
		base.BindViewImplementation();
		CreateInput();
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
	}

	protected override void DestroyViewImplementation()
	{
		if (m_GamersTagsNavigationBehavior != null)
		{
			m_GamersTagsNavigationBehavior.UnFocusCurrentEntity();
			m_GamersTagsNavigationBehavior.Clear();
		}
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour.Clear();
		base.DestroyViewImplementation();
	}

	private void CreateInput()
	{
		AddDisposable(m_PlayerListNavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_PlayerListNavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "NetRoles"
		});
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		SetPlayerListNavigation();
		m_PlayerListNavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_CommonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 8), base.ViewModel.IsRoomOwner ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9));
		if (m_Players == null || !m_Players.Any())
		{
			return;
		}
		m_Players.ForEach(delegate(NetRolesPlayerConsoleView p)
		{
			p.Characters.ForEach(delegate(NetRolesPlayerCharacterConsoleView c)
			{
				c.AddPlayerInput(inputLayer);
			});
		});
	}

	private void SetPlayerListNavigation()
	{
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour.Clear();
		List<IConsoleEntity> charactersEntities = new List<IConsoleEntity>();
		if (m_Players?.FirstOrDefault() == null)
		{
			return;
		}
		int? num = m_Players?.FirstOrDefault()?.Characters?.Count;
		int i;
		for (i = 0; i < num; i++)
		{
			m_Players?.Where((NetRolesPlayerConsoleView p) => p.IsBinded).ToList().ForEach(delegate(NetRolesPlayerConsoleView p)
			{
				if (p.Characters[i].IsValid())
				{
					charactersEntities.Add(p.Characters[i]);
				}
			});
		}
		if (charactersEntities.Any())
		{
			m_PlayerListNavigationBehaviour.SetEntitiesHorizontal(charactersEntities);
		}
	}

	public void HandleUpdateCharactersNavigation(UnitReference focusCharacter)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			SetPlayerListNavigation();
			m_PlayerListNavigationBehaviour.FocusOnEntityManual(m_PlayerListNavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is NetRolesPlayerCharacterConsoleView netRolesPlayerCharacterConsoleView && netRolesPlayerCharacterConsoleView.Character == focusCharacter));
		}, 1);
	}

	private void AddGamersTagsInput(ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(m_GamersTagsNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_GamersTagsInputLayer = m_GamersTagsNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerGamersTagsContextName
		});
		m_Players.ForEach(delegate(NetRolesPlayerConsoleView p)
		{
			p.AddGamerTagInput(m_GamersTagsInputLayer, hintsWidget, CloseGamersTagsMode);
		});
	}

	private void ShowGamersTagsMode()
	{
		m_GamersTagsMode.Value = true;
		SetGamersTagsNavigation();
		NetRolesPlayerConsoleView focusedPlayer = m_Players.FirstOrDefault((NetRolesPlayerConsoleView p) => p.Characters.FirstOrDefault((NetRolesPlayerCharacterConsoleView c) => c.IsFocused.Value));
		GamerTagAndNameBaseView gamerTagAndNameBaseView = m_GamersTagsNavigationBehavior.Entities.OfType<GamerTagAndNameBaseView>().FirstOrDefault((GamerTagAndNameBaseView e) => e.GetUserId() == focusedPlayer.Or(null)?.GetUserId());
		GamePad.Instance.PushLayer(m_GamersTagsInputLayer);
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		if (gamerTagAndNameBaseView != null)
		{
			m_GamersTagsNavigationBehavior.FocusOnEntityManual(gamerTagAndNameBaseView);
		}
		else
		{
			m_GamersTagsNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseGamersTagsMode()
	{
		m_GamersTagsNavigationBehavior?.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour?.FocusOnCurrentEntity();
		m_GamersTagsMode.Value = false;
		GamePad.Instance.PopLayer(m_GamersTagsInputLayer);
	}

	private void SetGamersTagsNavigation()
	{
		m_GamersTagsNavigationBehavior.Clear();
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		list.AddRange(m_Players.Select((NetRolesPlayerConsoleView player) => player.GamerTagAndName));
		if (list.Any())
		{
			m_GamersTagsNavigationBehavior.SetEntitiesVertical(list);
		}
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != m_InputLayer && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == "BugReportDuplicatesViewInput") && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == EscMenuBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == InputLayerGamersTagsContextName))
		{
			instance.PopLayer(m_InputLayer);
			instance.PushLayer(m_InputLayer);
		}
	}
}
