using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectsPCView : ColonyProjectsBaseView
{
	[SerializeField]
	private OwlcatButton m_StartProjectButton;

	[SerializeField]
	private TextMeshProUGUI m_StartProjectButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_ShowBlockedProjectsButton;

	[SerializeField]
	private TextMeshProUGUI m_ShowBlockedProjectsButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_ShowFinishedProjectsButton;

	[SerializeField]
	private TextMeshProUGUI m_ShowFinishedProjectsButtonLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	[UsedImplicitly]
	private ColonyProjectsNavigationPCView m_Navigation;

	[SerializeField]
	[UsedImplicitly]
	private ColonyProjectsPagePCView m_Page;

	protected override void InitializeImpl()
	{
		m_Page.Initialize();
		m_Navigation.Initialize();
		m_StartProjectButtonLabel.text = UIStrings.Instance.ColonyProjectsTexts.StartProjectButton.Text;
		m_ShowBlockedProjectsButtonLabel.text = UIStrings.Instance.ColonyProjectsTexts.ShowBlockedProjectsButton.Text;
		m_ShowFinishedProjectsButtonLabel.text = UIStrings.Instance.ColonyProjectsTexts.ShowFinishedProjectsButton.Text;
	}

	protected override void BindViewImplementation()
	{
		m_Navigation.Bind(base.ViewModel.NavigationVM);
		m_Page.Bind(base.ViewModel.ColonyProjectPageVM);
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(m_Navigation.ScrollMenu));
		AddDisposable(base.ViewModel.StartAvailable.Subscribe(m_StartProjectButton.gameObject.SetActive));
		AddDisposable(base.ViewModel.ShowBlockedProjects.Subscribe(delegate(bool show)
		{
			string activeLayer2 = (show ? "Checked" : "Unchecked");
			m_ShowBlockedProjectsButton.SetActiveLayer(activeLayer2);
		}));
		AddDisposable(base.ViewModel.ShowFinishedProjects.Subscribe(delegate(bool show)
		{
			string activeLayer = (show ? "Checked" : "Unchecked");
			m_ShowFinishedProjectsButton.SetActiveLayer(activeLayer);
		}));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleClose();
		}));
		AddDisposable(m_StartProjectButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleStartProject();
		}));
		AddDisposable(m_ShowBlockedProjectsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleShowBlockedProjects();
		}));
		AddDisposable(m_ShowFinishedProjectsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleShowFinishedProjects();
		}));
	}

	protected override void UpdateNavigationImpl()
	{
		m_NavigationBehaviour.SetEntitiesVertical(m_Navigation.GetNavigationEntities());
	}
}
