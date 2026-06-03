using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;

public class EncyclopediaNavigationElementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEncyclopediaNodeViewedHandler, ISubscriber
{
	public readonly BoolReactiveProperty IsSelected = new BoolReactiveProperty();

	public string Title;

	private List<EncyclopediaNavigationElementVM> m_ChildsVM;

	public readonly BoolReactiveProperty IsAvailablePage = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsUncommitedPlanetsLittleIcon = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsUncommitedPlanetsBigIcon = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsViewed = new BoolReactiveProperty(initialValue: true);

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
		AddDisposable(EventBus.Subscribe(this));
		ShowEncyclopediaPlanetChapterAndPages();
		IsViewed.Value = GetIsViewed();
	}

	protected override void DisposeImplementation()
	{
	}

	private bool GetIsViewed()
	{
		if (Page.IsChilds())
		{
			return Page.GetChilds().All(delegate(IPage c)
			{
				if (c is GlossaryLetterIndexPage glossaryLetterIndexPage2)
				{
					return glossaryLetterIndexPage2.GetBlocks().All((IBlock b) => !(b is GlossaryEntryBlock glossaryEntryBlock2) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(glossaryEntryBlock2.Entry));
				}
				if (c is BlueprintEncyclopediaAstropathBriefPage blueprintEncyclopediaAstropathBriefPage2)
				{
					return blueprintEncyclopediaAstropathBriefPage2.GetBlocks().All((IBlock b) => !(b is BlueprintEncyclopediaAstropathBriefPage.AstropathBriefBlock astropathBriefBlock2) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(astropathBriefBlock2.Entry));
				}
				if (c is BlueprintEncyclopediaPlanetTypePage blueprintEncyclopediaPlanetTypePage2)
				{
					return blueprintEncyclopediaPlanetTypePage2.GetBlocks().All((IBlock b) => !(b is BlueprintEncyclopediaPlanetTypePage.PlanetBlock planetBlock2) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(planetBlock2.Entry));
				}
				return !(c is BlueprintEncyclopediaNode node2) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(node2);
			});
		}
		IPage page = Page;
		if (!(page is GlossaryLetterIndexPage glossaryLetterIndexPage))
		{
			if (!(page is BlueprintEncyclopediaAstropathBriefPage blueprintEncyclopediaAstropathBriefPage))
			{
				if (!(page is BlueprintEncyclopediaPlanetTypePage blueprintEncyclopediaPlanetTypePage))
				{
					if (page is BlueprintEncyclopediaNode node)
					{
						return Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(node);
					}
					return true;
				}
				return blueprintEncyclopediaPlanetTypePage.GetBlocks().All((IBlock b) => !(b is BlueprintEncyclopediaPlanetTypePage.PlanetBlock planetBlock) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(planetBlock.Entry));
			}
			return blueprintEncyclopediaAstropathBriefPage.GetBlocks().All((IBlock b) => !(b is BlueprintEncyclopediaAstropathBriefPage.AstropathBriefBlock astropathBriefBlock) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(astropathBriefBlock.Entry));
		}
		return glossaryLetterIndexPage.GetBlocks().All((IBlock b) => !(b is GlossaryEntryBlock glossaryEntryBlock) || Game.Instance.Player.UISettings.EncyclopediaData.IsViewed(glossaryEntryBlock.Entry));
	}

	public void SetIsViewed()
	{
		IPage page = Page;
		if (!(page is BlueprintEncyclopediaNode node))
		{
			if (!(page is GlossaryLetterIndexPage glossaryLetterIndexPage))
			{
				return;
			}
			{
				foreach (IBlock block in glossaryLetterIndexPage.GetBlocks())
				{
					if (block is GlossaryEntryBlock glossaryEntryBlock)
					{
						Game.Instance.Player.UISettings.EncyclopediaData.MarkViewed(glossaryEntryBlock.Entry);
					}
				}
				return;
			}
		}
		Game.Instance.Player.UISettings.EncyclopediaData.MarkViewed(node);
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

	public void HandleEncyclopediaNodeViewed(BlueprintEncyclopediaNode node)
	{
		IsViewed.Value = GetIsViewed();
	}
}
