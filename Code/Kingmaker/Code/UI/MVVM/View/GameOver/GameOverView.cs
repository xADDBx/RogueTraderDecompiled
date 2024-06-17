using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public abstract class GameOverView : CommonStaticComponentView<GameOverVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ResultText;

	[SerializeField]
	protected OwlcatButton m_QuickLoadButton;

	[SerializeField]
	protected OwlcatButton m_LoadButton;

	[SerializeField]
	protected OwlcatButton m_MainMenuButton;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		DelayedInvoker.InvokeInTime(OnActivate, 3f);
		AddDisposable(base.ViewModel.Reason.Subscribe(delegate(string value)
		{
			m_ResultText.text = value;
		}));
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.EscapeMenu);
		});
	}

	protected virtual void OnActivate()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.EscapeMenu);
		});
	}
}
