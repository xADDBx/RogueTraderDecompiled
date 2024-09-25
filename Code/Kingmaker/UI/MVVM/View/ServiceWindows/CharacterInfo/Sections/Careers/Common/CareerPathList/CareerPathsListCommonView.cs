using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public class CareerPathsListCommonView : ViewBase<CareerPathsListVM>
{
	private enum CareerState
	{
		Locked,
		Selected,
		HasUpgrades,
		Finished,
		Unlocked
	}

	[Header("Common")]
	[SerializeField]
	private OwlcatMultiSelectable m_MainSelectable;

	[Header("View")]
	[SerializeField]
	private TextMeshProUGUI m_LockPavelLabel;

	[SerializeField]
	private TextMeshProUGUI m_UnlockPavelLabel;

	[SerializeField]
	private OwlcatMultiSelectable m_CareerStateSelectable;

	[Header("Selected Career")]
	[SerializeField]
	private CareerPathListSelectedCareerCommonView m_SelectedCareerCommonView;

	[Header("Careers List")]
	[SerializeField]
	private CareerPathListItemCommonView m_CareerPathListItemCommonView;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private ReactiveProperty<int> m_CurrentRank;

	private AccessibilityTextHelper m_TextHelper;

	public IEnumerable<CareerPathListItemCommonView> ItemViews => m_WidgetList.Entries.Cast<CareerPathListItemCommonView>();

	public void Initialize()
	{
		m_SelectedCareerCommonView.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_LockPavelLabel, m_UnlockPavelLabel);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.SelectedCareer.Subscribe(delegate(CareerPathVM career)
		{
			m_SelectedCareerCommonView.Bind(career);
			if (career != null)
			{
				AddDisposable(career.OnUpdateSelected.Subscribe(delegate
				{
					UpdateSelectedCareerState();
				}));
				AddDisposable(career.CanUpgrade.Subscribe(delegate
				{
					UpdateSelectedCareerState();
				}));
			}
			string activeLayer = ((base.ViewModel.SelectedCareer.Value == null) ? "Default" : "Career");
			m_MainSelectable.SetActiveLayer(activeLayer);
			UpdateNavigation();
			UpdateSelectedCareerState();
		}));
		SetupView();
		DrawEntries();
		AddDisposable(base.ViewModel.IsUnlocked.Subscribe(delegate
		{
			UpdateSelectedCareerState();
		}));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_WidgetList.Clear();
		m_TextHelper.Dispose();
	}

	private void UpdateSelectedCareerState()
	{
		CareerPathVM careerPathVM = base.ViewModel.SelectedCareer?.Value;
		CareerState careerState = CareerState.Locked;
		if (careerPathVM != null)
		{
			careerState = CareerState.Selected;
			if (careerPathVM.IsFinished)
			{
				careerState = CareerState.Finished;
			}
			else if (careerPathVM.IsAvailableToUpgrade)
			{
				careerState = CareerState.HasUpgrades;
			}
		}
		else if (base.ViewModel.IsUnlocked.Value)
		{
			careerState = CareerState.Unlocked;
		}
		m_CareerStateSelectable.SetActiveLayer(careerState.ToString());
	}

	private void DrawEntries()
	{
		m_WidgetList.DrawEntries(base.ViewModel.CareerPathVMs, m_CareerPathListItemCommonView);
		UpdateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			m_NavigationBehaviour.Clear();
			if (base.ViewModel.SelectedCareer.Value != null)
			{
				m_NavigationBehaviour.AddEntityVertical(m_SelectedCareerCommonView);
			}
			else if (m_WidgetList.Entries != null && m_WidgetList.Entries.Any())
			{
				List<IConsoleEntity> entities = m_WidgetList.Entries.Cast<IConsoleEntity>().ToList();
				m_NavigationBehaviour.AddRow(entities);
			}
			UpdateCurrentEntity();
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation();
		}
		return m_NavigationBehaviour;
	}

	public void UpdateCurrentEntity()
	{
		foreach (IConsoleEntity item in m_WidgetList.Entries.Cast<IConsoleEntity>())
		{
			if ((item as CareerPathListItemCommonView)?.GetViewModel() is CareerPathVM careerPathVM && careerPathVM.IsSelected.Value)
			{
				m_NavigationBehaviour.SetCurrentEntity(item);
				break;
			}
		}
	}

	private void SetupView()
	{
		string text = base.ViewModel.GetLevelToUnlock() + " " + UIStrings.Instance.CharacterSheet.LvlShort.Text;
		m_LockPavelLabel.text = text;
		m_UnlockPavelLabel.text = text;
	}

	public void CreateLines(List<CareerPathListItemCommonView> allCareers)
	{
		ItemViews.ToList().ForEach(delegate(CareerPathListItemCommonView c)
		{
			c.CreateLines(allCareers);
		});
	}
}
