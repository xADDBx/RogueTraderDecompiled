using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public class CareerPathsListsCommonView : CharInfoComponentView<UnitProgressionVM>
{
	[SerializeField]
	private RankExpCounterCommonView m_RankExpCounterCommonView;

	[SerializeField]
	protected UnitBackgroundBlockCommonView m_UnitBackgroundBlockCommonView;

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
		m_RankExpCounterCommonView.Or(null)?.Bind(base.ViewModel.CharInfoExperienceVM);
		m_UnitBackgroundBlockCommonView.Or(null)?.Bind(base.ViewModel.UnitBackgroundBlockVM);
		CreateCharGenLines();
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

	[ContextMenu("TestMe")]
	public void CreateCharGenLines()
	{
		if (!base.ViewModel.IsCharGen)
		{
			return;
		}
		DelayedInvoker.InvokeInFrames(delegate
		{
			List<CareerPathListItemCommonView> allCareerViews = m_CareerPathsLists.SelectMany((CareerPathsListCommonView l) => l.ItemViews).ToList();
			m_CareerPathsLists.ForEach(delegate(CareerPathsListCommonView l)
			{
				l.CreateLines(allCareerViews);
			});
		}, 1);
	}
}
