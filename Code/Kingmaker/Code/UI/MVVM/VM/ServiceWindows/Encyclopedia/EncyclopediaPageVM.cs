using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;

public class EncyclopediaPageVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Title;

	public readonly string GlossaryText;

	public readonly List<EncyclopediaPageBlockVM> BlockVMs = new List<EncyclopediaPageBlockVM>();

	private readonly List<EncyclopediaPageImageVM> m_ImageVMs = new List<EncyclopediaPageImageVM>();

	private readonly ReactiveProperty<EncyclopediaPageImageVM> m_FullscreenImageVM = new ReactiveProperty<EncyclopediaPageImageVM>();

	public readonly float FontMultiplier = FontSizeMultiplier;

	public IPage Page { get; }

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public EncyclopediaPageVM(IPage page)
	{
		BlueprintEncyclopediaGlossaryEntry blueprintEncyclopediaGlossaryEntry = null;
		if (page is BlueprintEncyclopediaGlossaryEntry)
		{
			blueprintEncyclopediaGlossaryEntry = (BlueprintEncyclopediaGlossaryEntry)page;
			page = ((BlueprintEncyclopediaGlossaryChapter)blueprintEncyclopediaGlossaryEntry.Parent).GetLetterIndexPage(page.GetTitle().Substring(0, 1));
		}
		Page = page;
		Title = page.GetTitle();
		BlueprintEncyclopediaPlanetTypePage planetTypePage = page as BlueprintEncyclopediaPlanetTypePage;
		if (planetTypePage != null)
		{
			int num = Game.Instance.Player.StarSystemsState.ScannedPlanets.Count((PlanetExplorationInfo planet) => planet.Planet.Type == planetTypePage.PlanetType);
			Title = $"{page.GetTitle()} [{num}]";
		}
		if (page is BlueprintEncyclopediaPage { GlossaryEntry: not null } blueprintEncyclopediaPage)
		{
			string text = (blueprintEncyclopediaPage.GlossaryEntry as BlueprintEncyclopediaGlossaryEntry)?.GetDescription();
			GlossaryText = ((!string.IsNullOrWhiteSpace(text)) ? text : string.Empty);
		}
		foreach (IBlock block9 in page.GetBlocks())
		{
			EncyclopediaPageBlockVM encyclopediaPageBlockVM = null;
			try
			{
				if (!(block9 is BlueprintEncyclopediaBlockText block))
				{
					if (!(block9 is BlueprintEncyclopediaBlockImage block2))
					{
						if (!(block9 is BlueprintEncyclopediaSkillPage.SkillTable block3))
						{
							if (!(block9 is BlueprintEncyclopediaBlockBestiaryUnit block4))
							{
								if (!(block9 is BlueprintEncyclopediaBlockPages block5))
								{
									if (!(block9 is BlueprintEncyclopediaBookEventPage.BookEventLogBlock block6))
									{
										if (!(block9 is GlossaryEntryBlock glossaryEntryBlock))
										{
											if (!(block9 is BlueprintEncyclopediaPlanetTypePage.PlanetBlock block7))
											{
												if (block9 is BlueprintEncyclopediaAstropathBriefPage.AstropathBriefBlock block8)
												{
													encyclopediaPageBlockVM = new EncyclopediaPageBlockAstropathBriefVM(block8);
												}
											}
											else
											{
												encyclopediaPageBlockVM = new EncyclopediaPageBlockPlanetVM(block7);
											}
										}
										else
										{
											bool marked = blueprintEncyclopediaGlossaryEntry == glossaryEntryBlock.Entry;
											encyclopediaPageBlockVM = new EncyclopediaPageBlockGlossaryEntryVM(glossaryEntryBlock, marked);
										}
									}
									else
									{
										encyclopediaPageBlockVM = new EncyclopediaPageBlockBookEventVM(block6);
									}
								}
								else
								{
									encyclopediaPageBlockVM = new EncyclopediaPageBlockChildPagesVM(block5);
								}
							}
							else
							{
								encyclopediaPageBlockVM = new EncyclopediaPageBlockUnitVM(block4);
							}
						}
						else
						{
							encyclopediaPageBlockVM = new EncyclopediaPageBlockClassProgressionVM(block3);
						}
					}
					else
					{
						encyclopediaPageBlockVM = new EncyclopediaPageBlockImageVM(block2);
					}
				}
				else
				{
					encyclopediaPageBlockVM = new EncyclopediaPageBlockTextVM(block);
				}
			}
			catch (Exception ex)
			{
				PFLog.UI.Exception(ex, "Can't create block: {0}", block9);
				continue;
			}
			if (encyclopediaPageBlockVM != null)
			{
				AddDisposable(encyclopediaPageBlockVM);
				BlockVMs.Add(encyclopediaPageBlockVM);
			}
		}
		Action<EncyclopediaPageImageVM> zoomAction = null;
		if (page is BlueprintEncyclopediaBookEventPage)
		{
			zoomAction = ShowFullscreenImage;
		}
		foreach (SpriteLink item in from spriteLink in page.GetImages()
			where spriteLink.Exists()
			select spriteLink)
		{
			EncyclopediaPageImageVM encyclopediaPageImageVM = new EncyclopediaPageImageVM(item.Load(), zoomAction);
			AddDisposable(encyclopediaPageImageVM);
			m_ImageVMs.Add(encyclopediaPageImageVM);
		}
	}

	protected override void DisposeImplementation()
	{
		DisposeFullscreenImage();
	}

	private void ShowFullscreenImage(EncyclopediaPageImageVM imageVm)
	{
		if (m_FullscreenImageVM.Value == null)
		{
			m_FullscreenImageVM.Value = imageVm;
		}
	}

	private void DisposeFullscreenImage()
	{
		m_FullscreenImageVM.Value = null;
	}
}
