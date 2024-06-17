using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public class CareerPathsListsCommonView : CharInfoComponentView<UnitProgressionVM>
{
	[SerializeField]
	private RankExpCounterCommonView m_RankExpCounterCommonView;

	[SerializeField]
	private UnitBackgroundBlockCommonView m_UnitBackgroundBlockCommonView;

	[SerializeField]
	protected List<CareerPathsListCommonView> m_CareerPathsLists;

	protected readonly ReactiveProperty<bool> m_IsShown = new ReactiveProperty<bool>();

	public override void Initialize()
	{
		base.Initialize();
		foreach (CareerPathsListCommonView careerPathsList in m_CareerPathsLists)
		{
			careerPathsList.Initialize();
		}
		if (m_UnitBackgroundBlockCommonView != null)
		{
			m_UnitBackgroundBlockCommonView.Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_RankExpCounterCommonView != null)
		{
			m_RankExpCounterCommonView.Bind(base.ViewModel.CharInfoExperienceVM);
		}
		if (m_UnitBackgroundBlockCommonView != null)
		{
			m_UnitBackgroundBlockCommonView.Bind(base.ViewModel.UnitBackgroundBlockVM);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected override void RefreshView()
	{
		for (int i = 0; i < m_CareerPathsLists.Count; i++)
		{
			CareerPathsListVM careerPathsListVM = base.ViewModel.CareerPathsList.ElementAtOrDefault(i);
			m_CareerPathsLists[i].Bind(careerPathsListVM);
			m_CareerPathsLists[i].gameObject.SetActive(careerPathsListVM != null);
		}
	}

	public void SetVisibility(bool visible)
	{
		m_FadeAnimator.PlayAnimation(visible);
		m_IsShown.Value = visible;
	}
}
