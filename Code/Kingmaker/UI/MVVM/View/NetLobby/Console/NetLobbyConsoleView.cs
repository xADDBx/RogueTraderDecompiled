using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console;

public class NetLobbyConsoleView : NetLobbyBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private NetLobbyCreateJoinPartConsoleView m_CreateJoinBlock;

	[SerializeField]
	private NetLobbyWaitingPartConsoleView m_WaitingBlock;

	[SerializeField]
	private NetLobbyLobbyPartConsoleView m_LobbyBlock;

	[SerializeField]
	private NetLobbyTutorialPartConsoleView m_TutorialBlock;

	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public static readonly string InputLayerName = "NetLobby";

	private NetLobbySaveSlotCollectionConsoleView SlotCollectionView => m_SlotCollectionView as NetLobbySaveSlotCollectionConsoleView;

	public override void Initialize()
	{
		base.Initialize();
		m_CreateJoinBlock.Initialize();
		m_WaitingBlock.Initialize();
		m_LobbyBlock.Initialize();
		m_TutorialBlock.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CreateJoinBlock.Bind(base.ViewModel);
		m_WaitingBlock.Bind(base.ViewModel);
		m_LobbyBlock.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.SaveSlotCollectionVm.Subscribe(SlotCollectionView.Bind));
		CreateInput();
		AddDisposable(base.ViewModel.NetLobbyTutorialPartVM.Subscribe(m_TutorialBlock.Bind));
	}

	protected override void DestroyViewImplementation()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
		base.DestroyViewImplementation();
	}

	private void CreateInput()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerName
		});
		BuildNavigationImpl(m_NavigationBehaviour);
		CreateInputImpl(m_InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_CreateJoinBlock.BuildNavigationImpl(navigationBehaviour);
		m_LobbyBlock.BuildNavigationImpl(navigationBehaviour);
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		m_CreateJoinBlock.CreateInputImpl(inputLayer, m_CommonHintsWidget);
		m_WaitingBlock.CreateInputImpl(inputLayer, m_CommonHintsWidget);
		m_LobbyBlock.CreateInputImpl(inputLayer, m_CommonHintsWidget);
	}
}
