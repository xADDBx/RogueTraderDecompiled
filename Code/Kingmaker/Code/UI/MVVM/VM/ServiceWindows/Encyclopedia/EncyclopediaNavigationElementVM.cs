using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;

public class EncyclopediaNavigationElementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly BoolReactiveProperty IsSelected = new BoolReactiveProperty();

	public string Title;

	private List<EncyclopediaNavigationElementVM> m_ChildsVM;

	public readonly BoolReactiveProperty IsAvailablePage = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsUncommitedPlanetsLittleIcon = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsUncommitedPlanetsBigIcon = new BoolReactiveProperty(initialValue: true);

	private List<IPage> m_Childs = new List<IPage>();

	public IPage Page { get; }

	public List<EncyclopediaNavigationElementVM> ChildsVM => m_ChildsVM;

	public List<EncyclopediaNavigationElementVM> GetOrCreateChildsVM()
	{
		if (m_ChildsVM == null)
		{
			m_ChildsVM = new List<EncyclopediaNavigationElementVM>();
		}
		if (m_ChildsVM.Count == 0)
		{
			m_Childs = Page.GetChilds();
			foreach (IPage child in m_Childs)
			{
				if (child == null)
				{
					UberDebug.LogError($"{Page} has empty page");
				}
				else if (string.IsNullOrEmpty(child.GetTitle()))
				{
					UberDebug.LogError($"{Page} has element {child} with empty Title");
				}
				else if (!(child is IEncyclopediaPageWithAvailability { IsAvailable: false }))
				{
					EncyclopediaNavigationElementVM encyclopediaNavigationElementVM = new EncyclopediaNavigationElementVM(child);
					AddDisposable(encyclopediaNavigationElementVM);
					m_ChildsVM.Add(encyclopediaNavigationElementVM);
				}
			}
		}
		m_ChildsVM.Sort((EncyclopediaNavigationElementVM p, EncyclopediaNavigationElementVM q) => string.Compare(p.Title, q.Title, StringComparison.Ordinal));
		return m_ChildsVM;
	}

	public EncyclopediaNavigationElementVM(IPage page)
	{
		Page = page;
		Title = page.GetTitle();
		ShowEncyclopediaPlanetChapterAndPages();
	}

	private void ShowEncyclopediaPlanetChapterAndPages()
	{
		IsUncommitedPlanetsBigIcon.Value = false;
		IsUncommitedPlanetsLittleIcon.Value = false;
		List<PlanetExplorationInfo> scannedPlanets = Game.Instance.Player.StarSystemsState.ScannedPlanets;
		if (Page == UIConfig.Instance.PlanetTypeChapter.Get())
		{
			foreach (IPage child in Page.GetChilds())
			{
				if (!(child is BlueprintEncyclopediaPlanetTypePage blueprintEncyclopediaPlanetTypePage))
				{
					continue;
				}
				IsAvailablePage.Value = blueprintEncyclopediaPlanetTypePage.IsAvailable;
				if (IsAvailablePage.Value)
				{
					IsAvailablePage.Value = true;
					IsUncommitedPlanetsBigIcon.Value = scannedPlanets.Any((PlanetExplorationInfo planet) => !planet.IsReportedToAdministratum);
					break;
				}
			}
		}
		IPage page = Page;
		BlueprintEncyclopediaPlanetTypePage planetTypePage = page as BlueprintEncyclopediaPlanetTypePage;
		if (planetTypePage == null)
		{
			return;
		}
		IsAvailablePage.Value = planetTypePage.IsAvailable;
		if (IsAvailablePage.Value)
		{
			int num = scannedPlanets.Count((PlanetExplorationInfo planet) => planet.Planet.Type == planetTypePage.PlanetType);
			IsUncommitedPlanetsLittleIcon.Value = scannedPlanets.Any((PlanetExplorationInfo planet) => planet.Planet.Type == planetTypePage.PlanetType && !planet.IsReportedToAdministratum);
			Title = $"{Page.GetTitle()} [{num}]";
		}
	}

	public void SelectPage()
	{
		EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
		{
			x.HandleEncyclopediaPage(Page);
		});
		if (!(Page is GlossaryLetterIndexPage) && Page is BlueprintEncyclopediaChapter)
		{
			m_ChildsVM.FirstOrDefault((EncyclopediaNavigationElementVM p) => p.IsAvailablePage.Value)?.SelectPage();
		}
	}

	public bool SetSelection(IPage page)
	{
		bool flag = Page == page;
		List<EncyclopediaNavigationElementVM> orCreateChildsVM = GetOrCreateChildsVM();
		if (!flag)
		{
			foreach (EncyclopediaNavigationElementVM item in orCreateChildsVM)
			{
				flag = item.SetSelection(page) || flag;
			}
		}
		IsSelected.Value = flag;
		return flag;
	}

	protected override void DisposeImplementation()
	{
	}
}
