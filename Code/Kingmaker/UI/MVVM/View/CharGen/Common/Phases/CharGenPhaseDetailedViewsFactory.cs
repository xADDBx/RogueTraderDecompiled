using System;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.BackgroundBase;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Career;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Pregen;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Ship;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Summary;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Career;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.DarkestHour;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld.ChildPhases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.MomentOfTriumph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Navigator;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation.ChildPhases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Ship;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.SoulMark;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Summary;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases;

[Serializable]
public class CharGenPhaseDetailedViewsFactory : ICharGenPhaseDetailedViewsFactory
{
	[SerializeField]
	private CharGenPregenPhaseDetailedView m_PregenPhaseDetailedView;

	[SerializeField]
	private CharGenAppearancePhaseDetailedView m_AppearancePhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_SoulMarkPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_HomeworldPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_ImperialHomeworldChildPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_ForgeHomeworldChildPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_OccupationPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_NavigatorPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_SanctionedPsykerPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_ArbitratorPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_DarkestHourPhaseDetailedView;

	[SerializeField]
	private CharGenBackgroundBasePhaseDetailedView m_MomentOfTriumphPhaseDetailedView;

	[SerializeField]
	private CharGenCareerPhaseDetailedView m_CareerPhaseDetailedView;

	[SerializeField]
	private CharGenAttributesPhaseDetailedView m_AttributesPhaseDetailedView;

	[SerializeField]
	private CharGenSummaryPhaseDetailedView m_SummaryPhaseDetailedView;

	[SerializeField]
	private CharGenShipPhaseDetailedView m_ShipPhaseDetailedView;

	private bool m_PaperHintsAdded;

	public void Initialize()
	{
		m_PaperHintsAdded = false;
		m_PregenPhaseDetailedView.Initialize();
		m_AppearancePhaseDetailedView.Initialize();
		m_SoulMarkPhaseDetailedView.Initialize();
		m_HomeworldPhaseDetailedView.Initialize();
		m_OccupationPhaseDetailedView.Initialize();
		m_DarkestHourPhaseDetailedView.Initialize();
		m_MomentOfTriumphPhaseDetailedView.Initialize();
		m_NavigatorPhaseDetailedView.Initialize();
		m_CareerPhaseDetailedView.Initialize();
		m_AttributesPhaseDetailedView.Initialize();
		m_SummaryPhaseDetailedView.Initialize();
		m_ShipPhaseDetailedView.Initialize();
		m_ImperialHomeworldChildPhaseDetailedView.Initialize();
		m_ForgeHomeworldChildPhaseDetailedView.Initialize();
		m_SanctionedPsykerPhaseDetailedView.Initialize();
	}

	public void SetPaperHints(PaperHints paperHints)
	{
		if (UINetUtility.IsControlMainCharacter() && !m_PaperHintsAdded)
		{
			m_PregenPhaseDetailedView.SetPaperHints(paperHints);
			m_AppearancePhaseDetailedView.SetPaperHints(paperHints);
			m_SoulMarkPhaseDetailedView.SetPaperHints(paperHints);
			m_HomeworldPhaseDetailedView.SetPaperHints(paperHints);
			m_OccupationPhaseDetailedView.SetPaperHints(paperHints);
			m_DarkestHourPhaseDetailedView.SetPaperHints(paperHints);
			m_MomentOfTriumphPhaseDetailedView.SetPaperHints(paperHints);
			m_NavigatorPhaseDetailedView.SetPaperHints(paperHints);
			m_CareerPhaseDetailedView.SetPaperHints(paperHints);
			m_AttributesPhaseDetailedView.SetPaperHints(paperHints);
			m_SummaryPhaseDetailedView.SetPaperHints(paperHints);
			m_ShipPhaseDetailedView.SetPaperHints(paperHints);
			m_ImperialHomeworldChildPhaseDetailedView.SetPaperHints(paperHints);
			m_ForgeHomeworldChildPhaseDetailedView.SetPaperHints(paperHints);
			m_SanctionedPsykerPhaseDetailedView.SetPaperHints(paperHints);
			m_PaperHintsAdded = true;
		}
	}

	public ICharGenPhaseDetailedView GetDetailedPhaseView(CharGenPhaseBaseVM viewModel)
	{
		if (!(viewModel is CharGenPregenPhaseVM viewModel2))
		{
			if (!(viewModel is CharGenAppearanceComponentAppearancePhaseVM viewModel3))
			{
				if (!(viewModel is CharGenHomeworldPhaseVM viewModel4))
				{
					if (!(viewModel is CharGenOccupationPhaseVM viewModel5))
					{
						if (!(viewModel is CharGenNavigatorPhaseVM viewModel6))
						{
							if (!(viewModel is CharGenDarkestHourPhaseVM viewModel7))
							{
								if (!(viewModel is CharGenMomentOfTriumphPhaseVM viewModel8))
								{
									if (!(viewModel is CharGenCareerPhaseVM viewModel9))
									{
										if (!(viewModel is CharGenAttributesPhaseVM viewModel10))
										{
											if (!(viewModel is CharGenSummaryPhaseVM viewModel11))
											{
												if (!(viewModel is CharGenShipPhaseVM viewModel12))
												{
													if (!(viewModel is CharGenSoulMarkPhaseVM viewModel13))
													{
														if (!(viewModel is CharGenImperialHomeworldChildPhaseVM viewModel14))
														{
															if (!(viewModel is CharGenForgeHomeworldChildPhaseVM viewModel15))
															{
																if (!(viewModel is CharGenSanctionedPsykerChildPhaseVM viewModel16))
																{
																	if (viewModel is CharGenArbitratorChildPhaseVM viewModel17)
																	{
																		m_ArbitratorPhaseDetailedView.Bind(viewModel17);
																		return m_ArbitratorPhaseDetailedView;
																	}
																	return null;
																}
																m_SanctionedPsykerPhaseDetailedView.Bind(viewModel16);
																return m_SanctionedPsykerPhaseDetailedView;
															}
															m_ForgeHomeworldChildPhaseDetailedView.Bind(viewModel15);
															return m_ForgeHomeworldChildPhaseDetailedView;
														}
														m_ImperialHomeworldChildPhaseDetailedView.Bind(viewModel14);
														return m_ImperialHomeworldChildPhaseDetailedView;
													}
													m_SoulMarkPhaseDetailedView.Bind(viewModel13);
													return m_SoulMarkPhaseDetailedView;
												}
												m_ShipPhaseDetailedView.Bind(viewModel12);
												return m_ShipPhaseDetailedView;
											}
											m_SummaryPhaseDetailedView.Bind(viewModel11);
											return m_SummaryPhaseDetailedView;
										}
										m_AttributesPhaseDetailedView.Bind(viewModel10);
										return m_AttributesPhaseDetailedView;
									}
									m_CareerPhaseDetailedView.Bind(viewModel9);
									return m_CareerPhaseDetailedView;
								}
								m_MomentOfTriumphPhaseDetailedView.Bind(viewModel8);
								return m_MomentOfTriumphPhaseDetailedView;
							}
							m_DarkestHourPhaseDetailedView.Bind(viewModel7);
							return m_DarkestHourPhaseDetailedView;
						}
						m_NavigatorPhaseDetailedView.Bind(viewModel6);
						return m_NavigatorPhaseDetailedView;
					}
					m_OccupationPhaseDetailedView.Bind(viewModel5);
					return m_OccupationPhaseDetailedView;
				}
				m_HomeworldPhaseDetailedView.Bind(viewModel4);
				return m_HomeworldPhaseDetailedView;
			}
			m_AppearancePhaseDetailedView.Bind(viewModel3);
			return m_AppearancePhaseDetailedView;
		}
		m_PregenPhaseDetailedView.Bind(viewModel2);
		return m_PregenPhaseDetailedView;
	}
}
