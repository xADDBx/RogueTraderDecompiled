using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbyWaitingPartBaseView : ViewBase<NetLobbyVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_ConnectingText;

	private readonly BoolReactiveProperty m_IsWaitingPartActive = new BoolReactiveProperty();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_ConnectingText.text = UIStrings.Instance.NetLobbyTexts.ConnectingLabel;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.NetGameCurrentState.CombineLatest(base.ViewModel.ReadyToHostOrJoin, base.ViewModel.IsInRoom, (NetGame.State state, bool ready, bool inRoom) => new { state, ready, inRoom }).Subscribe(value =>
		{
			bool isConnectingNetGameCurrentState = base.ViewModel.IsConnectingNetGameCurrentState;
			base.gameObject.SetActive(isConnectingNetGameCurrentState);
			m_IsWaitingPartActive.Value = isConnectingNetGameCurrentState;
			if (isConnectingNetGameCurrentState)
			{
				m_ConnectingText.gameObject.SetActive(value: false);
			}
			else
			{
				isConnectingNetGameCurrentState = !value.inRoom && !value.ready;
				base.gameObject.SetActive(isConnectingNetGameCurrentState);
				m_IsWaitingPartActive.Value = isConnectingNetGameCurrentState;
				m_ConnectingText.gameObject.SetActive(!value.ready);
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
