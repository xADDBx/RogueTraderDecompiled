using System;
using DG.Tweening;
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
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common;

public class CharGenRoadmapMenuView : ViewBase<SelectionGroupRadioVM<CharGenPhaseBaseVM>>
{
	[SerializeField]
	protected OwlcatMultiSelectable m_BackgroundFrame;

	[SerializeField]
	private RectTransform m_FrameStart;

	[SerializeField]
	private RectTransform m_FrameEnd;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	[Space]
	[SerializeField]
	protected CharGenPregenPhaseRoadmapView m_PregenPhaseRoadmapView;

	[SerializeField]
	protected CharGenAppearancePhaseRoadmapView m_AppearancePhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_SoulMarkPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_HomeworldPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_ImperialHomeworldChildPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_ForgeHomeworldChildPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_SanctionedPsykerChildPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_OccupationPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_NavigatorPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_MomentOfTriumphPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_DarkestHourPhaseRoadmapView;

	[SerializeField]
	protected CharGenCareerPhaseRoadmapView m_CareerPhaseRoadmapView;

	[SerializeField]
	private CharGenAttributesPhaseRoadmapView m_AttributesPhaseRoadmapView;

	[SerializeField]
	private CharGenShipPhaseRoadmapView m_ShipPhaseRoadmapView;

	[SerializeField]
	private CharGenSummaryPhaseRoadmapView m_SummaryPhaseRoadmapView;

	[SerializeField]
	private RectTransform m_Selector;

	[SerializeField]
	private float m_AnimationDuration = 0.55f;

	private IDisposable m_DelayedSelectorMoveDisposable;

	private ICharGenPhaseRoadmapView m_SelectedView;

	private bool m_SelectorSoundPlaying;

	private CharGenPhaseBaseVM m_PrevEntity;

	public void Initialize()
	{
		m_PregenPhaseRoadmapView.Initialize(CharGenPhaseType.Pregen);
		m_AppearancePhaseRoadmapView.Initialize(CharGenPhaseType.Appearance);
		m_SoulMarkPhaseRoadmapView.Initialize(CharGenPhaseType.SoulMark);
		m_HomeworldPhaseRoadmapView.Initialize(CharGenPhaseType.Homeworld);
		m_OccupationPhaseRoadmapView.Initialize(CharGenPhaseType.Occupation);
		m_NavigatorPhaseRoadmapView.Initialize(CharGenPhaseType.Navigator);
		m_MomentOfTriumphPhaseRoadmapView.Initialize(CharGenPhaseType.MomentOfTriumph);
		m_DarkestHourPhaseRoadmapView.Initialize(CharGenPhaseType.DarkestHour);
		m_CareerPhaseRoadmapView.Initialize(CharGenPhaseType.Career);
		m_AttributesPhaseRoadmapView.Initialize(CharGenPhaseType.Attributes);
		m_ShipPhaseRoadmapView.Initialize(CharGenPhaseType.Ship);
		m_SummaryPhaseRoadmapView.Initialize(CharGenPhaseType.Summary);
		m_ImperialHomeworldChildPhaseRoadmapView.Initialize(CharGenPhaseType.ImperialHomeworldChild);
		m_ForgeHomeworldChildPhaseRoadmapView.Initialize(CharGenPhaseType.ForgeHomeworldChild);
		m_SanctionedPsykerChildPhaseRoadmapView.Initialize(CharGenPhaseType.SanctionedPsyker);
	}

	protected override void BindViewImplementation()
	{
		foreach (CharGenPhaseBaseVM item in base.ViewModel.EntitiesCollection)
		{
			CreateRoadmapPhaseView(item);
		}
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveAdd().Subscribe(CreateRoadmapPhaseView));
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveMove().Subscribe(delegate
		{
			DelayedMoveSelector(base.ViewModel.SelectedEntity.Value);
		}));
		AddDisposable(base.ViewModel.SelectedEntity.Subscribe(DelayedMoveSelector));
		DelayedInvoker.InvokeInFrames(delegate
		{
			UpdateBackgroundFrameSize();
		}, 2);
	}

	private void DelayedMoveSelector(CharGenPhaseBaseVM selectedEntity)
	{
		m_SelectedView = GetRoadmapPhaseView(selectedEntity);
		if (m_SelectedView == null)
		{
			return;
		}
		m_DelayedSelectorMoveDisposable?.Dispose();
		m_DelayedSelectorMoveDisposable = DelayedInvoker.InvokeInFrames(delegate
		{
			m_ScrollRectExtended.EnsureVisibleHorizontal(m_SelectedView.ViewRectTransform, 160f);
			m_Selector.DOLocalMoveX(m_SelectedView.ViewRectTransform.localPosition.x, m_AnimationDuration).OnStart(delegate
			{
				if (m_PrevEntity != selectedEntity)
				{
					UISounds.Instance.Sounds.Selector.SelectorStart.Play();
					UISounds.Instance.Sounds.Selector.SelectorLoopStart.Play();
					m_SelectorSoundPlaying = true;
					m_PrevEntity = selectedEntity;
				}
			}).OnComplete(delegate
			{
				ShutUpSelector();
			})
				.OnKill(delegate
				{
					ShutUpSelector();
				})
				.SetUpdate(isIndependentUpdate: true);
		}, 2);
	}

	public void SetBackgroundFrameState(bool isCustom)
	{
		m_BackgroundFrame.SetActiveLayer(isCustom ? "Custom" : "Pregen");
	}

	private void UpdateBackgroundFrameSize()
	{
		RectTransform rectTransform = m_BackgroundFrame.transform as RectTransform;
		float x = m_FrameStart.localPosition.x;
		rectTransform.localPosition = new Vector3(x, rectTransform.localPosition.y, 0f);
		float x2 = m_FrameEnd.localPosition.x - x;
		rectTransform.sizeDelta = new Vector2(x2, rectTransform.sizeDelta.y);
	}

	public void ShutUpSelector()
	{
		if (m_SelectorSoundPlaying)
		{
			UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		}
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		m_SelectorSoundPlaying = false;
	}

	public void KillSelectorTween()
	{
		m_Selector.DOKill();
		DOTween.Kill(m_Selector);
	}

	protected override void DestroyViewImplementation()
	{
		KillSelectorTween();
		ShutUpSelector();
	}

	private void CreateRoadmapPhaseView(CollectionAddEvent<CharGenPhaseBaseVM> addEvent)
	{
		CreateRoadmapPhaseView(addEvent.Value);
	}

	private void CreateRoadmapPhaseView(CharGenPhaseBaseVM phaseVM)
	{
		if (!(phaseVM is CharGenPregenPhaseVM viewModel))
		{
			if (!(phaseVM is CharGenAppearanceComponentAppearancePhaseVM viewModel2))
			{
				if (!(phaseVM is CharGenHomeworldPhaseVM viewModel3))
				{
					if (!(phaseVM is CharGenOccupationPhaseVM viewModel4))
					{
						if (!(phaseVM is CharGenNavigatorPhaseVM viewModel5))
						{
							if (!(phaseVM is CharGenDarkestHourPhaseVM viewModel6))
							{
								if (!(phaseVM is CharGenMomentOfTriumphPhaseVM viewModel7))
								{
									if (!(phaseVM is CharGenCareerPhaseVM viewModel8))
									{
										if (!(phaseVM is CharGenAttributesPhaseVM viewModel9))
										{
											if (!(phaseVM is CharGenSummaryPhaseVM viewModel10))
											{
												if (!(phaseVM is CharGenShipPhaseVM viewModel11))
												{
													if (!(phaseVM is CharGenSoulMarkPhaseVM viewModel12))
													{
														if (!(phaseVM is CharGenImperialHomeworldChildPhaseVM viewModel13))
														{
															if (!(phaseVM is CharGenForgeHomeworldChildPhaseVM viewModel14))
															{
																if (phaseVM is CharGenSanctionedPsykerChildPhaseVM viewModel15)
																{
																	m_SanctionedPsykerChildPhaseRoadmapView.Bind(viewModel15);
																}
															}
															else
															{
																m_ForgeHomeworldChildPhaseRoadmapView.Bind(viewModel14);
															}
														}
														else
														{
															m_ImperialHomeworldChildPhaseRoadmapView.Bind(viewModel13);
														}
													}
													else
													{
														m_SoulMarkPhaseRoadmapView.Bind(viewModel12);
													}
												}
												else
												{
													m_ShipPhaseRoadmapView.Bind(viewModel11);
												}
											}
											else
											{
												m_SummaryPhaseRoadmapView.Bind(viewModel10);
											}
										}
										else
										{
											m_AttributesPhaseRoadmapView.Bind(viewModel9);
										}
									}
									else
									{
										m_CareerPhaseRoadmapView.Bind(viewModel8);
									}
								}
								else
								{
									m_MomentOfTriumphPhaseRoadmapView.Bind(viewModel7);
								}
							}
							else
							{
								m_DarkestHourPhaseRoadmapView.Bind(viewModel6);
							}
						}
						else
						{
							m_NavigatorPhaseRoadmapView.Bind(viewModel5);
						}
					}
					else
					{
						m_OccupationPhaseRoadmapView.Bind(viewModel4);
					}
				}
				else
				{
					m_HomeworldPhaseRoadmapView.Bind(viewModel3);
				}
			}
			else
			{
				m_AppearancePhaseRoadmapView.Bind(viewModel2);
			}
		}
		else
		{
			m_PregenPhaseRoadmapView.Bind(viewModel);
		}
	}

	private ICharGenPhaseRoadmapView GetRoadmapPhaseView(CharGenPhaseBaseVM phaseVM)
	{
		if (!(phaseVM is CharGenPregenPhaseVM))
		{
			if (!(phaseVM is CharGenAppearanceComponentAppearancePhaseVM))
			{
				if (!(phaseVM is CharGenHomeworldPhaseVM))
				{
					if (!(phaseVM is CharGenOccupationPhaseVM))
					{
						if (!(phaseVM is CharGenNavigatorPhaseVM))
						{
							if (!(phaseVM is CharGenDarkestHourPhaseVM))
							{
								if (!(phaseVM is CharGenMomentOfTriumphPhaseVM))
								{
									if (!(phaseVM is CharGenCareerPhaseVM))
									{
										if (!(phaseVM is CharGenAttributesPhaseVM))
										{
											if (!(phaseVM is CharGenSummaryPhaseVM))
											{
												if (!(phaseVM is CharGenShipPhaseVM))
												{
													if (!(phaseVM is CharGenSoulMarkPhaseVM))
													{
														if (!(phaseVM is CharGenImperialHomeworldChildPhaseVM))
														{
															if (!(phaseVM is CharGenForgeHomeworldChildPhaseVM))
															{
																if (phaseVM is CharGenSanctionedPsykerChildPhaseVM)
																{
																	return m_SanctionedPsykerChildPhaseRoadmapView;
																}
																return null;
															}
															return m_ForgeHomeworldChildPhaseRoadmapView;
														}
														return m_ImperialHomeworldChildPhaseRoadmapView;
													}
													return m_SoulMarkPhaseRoadmapView;
												}
												return m_ShipPhaseRoadmapView;
											}
											return m_SummaryPhaseRoadmapView;
										}
										return m_AttributesPhaseRoadmapView;
									}
									return m_CareerPhaseRoadmapView;
								}
								return m_MomentOfTriumphPhaseRoadmapView;
							}
							return m_DarkestHourPhaseRoadmapView;
						}
						return m_NavigatorPhaseRoadmapView;
					}
					return m_OccupationPhaseRoadmapView;
				}
				return m_HomeworldPhaseRoadmapView;
			}
			return m_AppearancePhaseRoadmapView;
		}
		return m_PregenPhaseRoadmapView;
	}

	public void SelectPrevPhase()
	{
		base.ViewModel.SelectPrevValidEntity();
	}

	public void SelectNextPhase()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void SelectLastValidPhase()
	{
		base.ViewModel.TrySelectLastValidEntity();
	}

	public void SelectFirstValidPhase()
	{
		base.ViewModel.TrySelectFirstValidEntity();
	}
}
