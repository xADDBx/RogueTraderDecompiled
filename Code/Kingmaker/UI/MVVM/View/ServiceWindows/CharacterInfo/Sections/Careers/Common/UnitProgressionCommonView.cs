using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;

public class UnitProgressionCommonView : CharInfoComponentView<UnitProgressionVM>
{
	[Header("Breadcrumbs")]
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private GameObject m_BreadcrumbsSlashes;

	[SerializeField]
	private ProgressionBreadcrumbsItemCommonView m_ProgressionBreadcrumbsItemCommonView;

	[Header("Sub Views")]
	[SerializeField]
	protected CareerPathsListsCommonView m_CareerPathsListsCommonView;

	[SerializeField]
	protected CareerPathProgressionCommonView m_CareerPathProgressionCommonView;

	private GameObject m_SpawnedSlashed;

	private UnitProgressionWindowState m_CurrentState;

	private Action<UnitProgressionWindowState> m_OnWindowStateChange;

	public UnitProgressionWindowState CurrentState => m_CurrentState;

	public void Initialize(Action<UnitProgressionWindowState> onWindowStateChange)
	{
		base.Initialize();
		m_OnWindowStateChange = onWindowStateChange;
		m_CareerPathsListsCommonView.Initialize();
		m_CareerPathProgressionCommonView.Initialize(HandleReturnAction);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CareerPathsListsCommonView.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.CurrentCareer.Subscribe(BindPathProgression));
		AddDisposable(base.ViewModel.State.Subscribe(HandleState));
		AddDisposable(base.ViewModel.Breadcrumbs.ObserveCountChanged().Subscribe(delegate
		{
			DrawBreadcrumbs();
		}));
		DrawBreadcrumbs();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_CareerPathsListsCommonView.Unbind();
		m_CareerPathProgressionCommonView.Unbind();
		m_WidgetList.Clear();
	}

	private void DrawBreadcrumbs()
	{
		if ((bool)m_SpawnedSlashed)
		{
			UnityEngine.Object.Destroy(m_SpawnedSlashed);
		}
		m_WidgetList.DrawEntries(base.ViewModel.Breadcrumbs.ToArray(), m_ProgressionBreadcrumbsItemCommonView);
		m_SpawnedSlashed = UnityEngine.Object.Instantiate(m_BreadcrumbsSlashes, m_WidgetList.transform, worldPositionStays: false);
		m_SpawnedSlashed.transform.SetAsFirstSibling();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_WidgetList.transform as RectTransform);
	}

	private void HandleReturnAction(bool saveSelections = false)
	{
		base.ViewModel.SetPreviousState(saveSelections);
	}

	protected virtual void HandleState(UnitProgressionWindowState state)
	{
		m_CurrentState = state;
		m_OnWindowStateChange?.Invoke(state);
		m_CareerPathsListsCommonView.SetVisibility(state == UnitProgressionWindowState.CareerPathList);
		m_CareerPathProgressionCommonView.SetVisibility(state == UnitProgressionWindowState.CareerPathProgression);
	}

	protected virtual void BindPathProgression(CareerPathVM careerPathVM)
	{
		m_CareerPathProgressionCommonView.Bind(careerPathVM);
		m_CareerPathsListsCommonView.Bind((careerPathVM == null) ? base.ViewModel : null);
	}
}
