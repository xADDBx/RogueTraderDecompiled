using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalNavigationGroupBaseView : ViewBase<JournalNavigationGroupVM>, IWidgetView
{
	[Space]
	[SerializeField]
	[UsedImplicitly]
	private TextMeshProUGUI m_Label;

	[Header("Collapse")]
	[SerializeField]
	[UsedImplicitly]
	protected OwlcatMultiButton m_MultiButton;

	[Header("Elements")]
	[SerializeField]
	[UsedImplicitly]
	protected WidgetListMVVM m_WidgetList;

	[UsedImplicitly]
	private bool m_IsInit;

	public WidgetListMVVM WidgetList => m_WidgetList;

	protected bool ShowCompletedQuests => Game.Instance.Player.UISettings.JournalShowCompletedQuest;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((JournalNavigationGroupVM)vm);
	}

	protected override void BindViewImplementation()
	{
		m_Label.text = base.ViewModel.Title;
		AddDisposable(base.ViewModel.IsSelected.Subscribe(delegate(bool value)
		{
			m_MultiButton.Interactable = !value;
			m_MultiButton.SetActiveLayer(value ? "On" : "Off");
		}));
		PFLog.UI.Log(base.gameObject.transform.GetSiblingIndex() + " " + m_Label.text);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is JournalNavigationGroupVM;
	}
}
