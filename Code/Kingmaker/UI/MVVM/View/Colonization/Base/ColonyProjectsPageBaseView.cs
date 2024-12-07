using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsPageBaseView : ViewBase<ColonyProjectsPageVM>
{
	[Header("Header")]
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[Header("Rewards Group Objects")]
	[SerializeField]
	private TextMeshProUGUI m_RewardsTitle;

	[SerializeField]
	protected WidgetListMVVM m_RewardsWidgetList;

	[SerializeField]
	protected ScrollRectExtended m_RewardsScrollRect;

	[Header("Requirements Group Objects")]
	[SerializeField]
	private TextMeshProUGUI m_RequirementsTitle;

	[SerializeField]
	protected WidgetListMVVM m_RequirementsWidgetList;

	[SerializeField]
	protected ScrollRectExtended m_RequirementsScrollRect;

	public ScrollRectExtended RewardsScrollRect => m_RewardsScrollRect;

	public void Initialize()
	{
		m_RequirementsTitle.text = UIStrings.Instance.QuesJournalTexts.RequiredResources;
		m_RewardsTitle.text = UIStrings.Instance.QuesJournalTexts.RewardsResources;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string val)
		{
			m_Title.text = val;
		}));
		AddDisposable(base.ViewModel.Description.Subscribe(delegate(string val)
		{
			m_Description.text = val;
		}));
		AddDisposable(base.ViewModel.RefreshData.Subscribe(RefreshView));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public List<IConsoleNavigationEntity> GetRewardsNavigationEntities()
	{
		return m_RewardsWidgetList.GetNavigationEntities();
	}

	public List<IConsoleNavigationEntity> GetRequirementsNavigationEntities()
	{
		return m_RequirementsWidgetList.GetNavigationEntities();
	}

	public void ScrollList(IConsoleEntity entity)
	{
		if (entity == null)
		{
			ResetScrolls();
		}
		else
		{
			ScrollListInternal(entity);
		}
	}

	protected virtual void ScrollListInternal(IConsoleEntity entity)
	{
		if (!(entity is ColonyProjectsRewardElementBaseView colonyProjectsRewardElementBaseView))
		{
			if (entity is ColonyProjectsRequirementElementBaseView colonyProjectsRequirementElementBaseView)
			{
				m_RequirementsScrollRect.EnsureVisibleVertical(colonyProjectsRequirementElementBaseView.transform as RectTransform, 50f, smoothly: false, needPinch: false);
			}
		}
		else
		{
			m_RewardsScrollRect.EnsureVisibleVertical(colonyProjectsRewardElementBaseView.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	private void ResetScrolls()
	{
		m_RewardsScrollRect.ScrollToTop();
		m_RequirementsScrollRect.ScrollToTop();
	}

	private void RefreshView()
	{
		ResetScrolls();
		DrawRewards();
		DrawRequirements();
	}

	private void DrawRewards()
	{
		DrawRewardsImpl();
	}

	protected virtual void DrawRewardsImpl()
	{
	}

	private void DrawRequirements()
	{
		DrawRequirementsImpl();
	}

	protected virtual void DrawRequirementsImpl()
	{
	}
}
