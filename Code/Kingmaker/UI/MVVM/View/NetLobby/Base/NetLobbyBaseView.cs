using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbyBaseView : ViewBase<NetLobbyVM>, IInitializable
{
	[Header("Base Part")]
	[SerializeField]
	private TextMeshProUGUI m_Header;

	[Space]
	[SerializeField]
	protected NetLobbySaveSlotCollectionBaseView m_SlotCollectionView;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_SlotCollectionView.Initialize();
		m_Header.text = UIStrings.Instance.NetLobbyTexts.NetHeader;
	}

	protected override void BindViewImplementation()
	{
		SetVisibility(state: true);
	}

	protected override void DestroyViewImplementation()
	{
		SetVisibility(state: false);
	}

	private void SetVisibility(bool state)
	{
		if (state)
		{
			UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
		}
		else
		{
			UISounds.Instance.Sounds.LocalMap.MapClose.Play();
		}
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state, FullScreenUIType.NewGame);
		});
		base.gameObject.SetActive(state);
		Game.Instance.RequestPauseUi(state);
	}
}
