using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.NetRoles.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
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
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_CommonHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 8), base.ViewModel.IsRoomOwner ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.CommonTexts.CloseWindow));
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

	protected override void DestroyViewImplementation()
	{
		m_PlayerListNavigationBehaviour.UnFocusCurrentEntity();
		m_PlayerListNavigationBehaviour.Clear();
		base.DestroyViewImplementation();
	}
}
