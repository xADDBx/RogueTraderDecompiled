using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public class PlayerStashView : ViewBase<PlayerStashVM>
{
	[SerializeField]
	public LootObjectView m_LootView;

	[SerializeField]
	private TextMeshProUGUI m_HeaderTitle;

	[SerializeField]
	protected OwlcatButton m_CloseButton;

	[SerializeField]
	protected OwlcatButton m_LootManagerButton;

	[SerializeField]
	protected TextMeshProUGUI m_LootManagerButtonLable;

	public void Initialize()
	{
		m_LootView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_LootView.Bind(base.ViewModel.ContextLoot[0]);
		m_HeaderTitle.text = base.ViewModel.LootDisplayName;
		AddDisposable(m_CloseButton.Or(null)?.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
		AddDisposable(m_LootManagerButton.Or(null)?.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ChangeView();
		}));
		if (m_LootManagerButtonLable != null)
		{
			m_LootManagerButtonLable.text = UIStrings.Instance.LootWindow.LootManager;
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
