using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.PC;

public class NetLobbyPCView : NetLobbyBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_CloseButton;

	[Space]
	[SerializeField]
	private NetLobbyCreateJoinPartPCView m_CreateJoinBlock;

	[SerializeField]
	private NetLobbyWaitingPartPCView m_WaitingBlock;

	[SerializeField]
	private NetLobbyLobbyPartPCView m_LobbyBlock;

	[SerializeField]
	private NetLobbyTutorialPartPCView m_TutorialBlock;

	[SerializeField]
	private OwlcatButton[] m_TutorialButtons;

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
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnClose));
		m_TutorialButtons.ForEach(delegate(OwlcatButton b)
		{
			b.gameObject.SetActive(base.ViewModel.IsAnyTutorialBlocks.Value);
			AddDisposable(b.OnLeftClickAsObservable().Subscribe(base.ViewModel.ShowNetLobbyTutorial));
			AddDisposable(b.SetHint(UIStrings.Instance.NetLobbyTexts.HowToPlay));
		});
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			if (!base.ViewModel.NetLobbyTutorialOnScreen.Value)
			{
				base.ViewModel.OnClose();
			}
		}));
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.SaveSlotCollectionVm.Subscribe(m_SlotCollectionView.Bind));
		m_CreateJoinBlock.Bind(base.ViewModel);
		m_WaitingBlock.Bind(base.ViewModel);
		m_LobbyBlock.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.NetLobbyTutorialPartVM.Subscribe(m_TutorialBlock.Bind));
		AddDisposable(base.ViewModel.NetLobbyTutorialOnScreen.Subscribe(delegate(bool value)
		{
			m_CloseButton.gameObject.SetActive(!value);
		}));
	}
}
