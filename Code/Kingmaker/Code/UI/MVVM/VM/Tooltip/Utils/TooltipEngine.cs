using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Tooltip;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

public static class TooltipEngine
{
	private static readonly List<Vector2> DefaultPivots = new List<Vector2>
	{
		new Vector2(0.5f, 1f),
		new Vector2(0.5f, 0f),
		new Vector2(0f, 0.5f),
		new Vector2(1f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	public static MonoBehaviour GetBrickView(TooltipBricksView config, TooltipBaseBrickVM vm)
	{
		if (!(vm is TooltipBrickTimerVM viewModel))
		{
			if (!(vm is TooltipBrickTextVM viewModel2))
			{
				if (!(vm is TooltipBrickSeparatorVM viewModel3))
				{
					if (!(vm is TooltipBrickSpaceVM viewModel4))
					{
						if (!(vm is TooltipBrickTitleVM viewModel5))
						{
							if (!(vm is TooltipBrickPictureVM viewModel6))
							{
								if (!(vm is TooltipBrickPortraitAndNameVM viewModel7))
								{
									if (!(vm is TooltipBrickItemIconAndNameVM viewModel8))
									{
										if (!(vm is TooltipBrickResourceIconAndNameVM viewModel9))
										{
											if (!(vm is TooltipBrickColonyProjectProgressVM viewModel10))
											{
												if (!(vm is TooltipBrickPFIconAndNameVM viewModel11))
												{
													if (!(vm is TooltipBrickIconAndNameVM viewModel12))
													{
														if (!(vm is TooltipBrickFactionStatusVM viewModel13))
														{
															if (!(vm is TooltipBrickItemFooterVM viewModel14))
															{
																if (!(vm is TooltipBrickTripleTextVM viewModel15))
																{
																	if (!(vm is TooltipBrickDoubleTextVM viewModel16))
																	{
																		if (!(vm is TooltipBrickIconValueStatVM viewModel17))
																		{
																			if (!(vm is TooltipBrickIconStatValueVM viewModel18))
																			{
																				if (!(vm is TooltipBrickTwoColumnsStatVM viewModel19))
																				{
																					if (!(vm is TooltipBrickValueStatFormulaVM viewModel20))
																					{
																						if (!(vm is TooltipBrickEntityHeaderVM viewModel21))
																						{
																							if (!(vm is TooltipBrickBuffVM viewModel22))
																							{
																								if (!(vm is TooltipBrickFeatureVM tooltipBrickFeatureVM))
																								{
																									if (!(vm is TooltipBrickAbilityScoresBlockVM viewModel23))
																									{
																										if (!(vm is TooltipBrickAbilityScoresVM viewModel24))
																										{
																											if (!(vm is TooltipBrickEncumbranceVM viewModel25))
																											{
																												if (!(vm is TooltipBrickButtonVM viewModel26))
																												{
																													if (!(vm is TooltipBrickHistoryManagementVM viewModel27))
																													{
																														if (!(vm is TooltipBrickNonStackVm viewModel28))
																														{
																															if (!(vm is TooltipBrickPrerequisiteVM viewModel29))
																															{
																																if (!(vm is TooltipBrickRateVM viewModel30))
																																{
																																	if (!(vm is TooltipBrickFeatureDescriptionVM viewModel31))
																																	{
																																		if (!(vm is TooltipBrickSkillsVM viewModel32))
																																		{
																																			if (!(vm is TooltipBrickAbilityTargetVM viewModel33))
																																			{
																																				if (!(vm is TooltipBrickHintVM viewModel34))
																																				{
																																					if (!(vm is TooltipBrickPlanetInfoVM viewModel35))
																																					{
																																						if (!(vm is TooltipBrickOtherObjectsInfoVM viewModel36))
																																						{
																																							if (!(vm is TooltipBrickAnomalyInfoVM viewModel37))
																																							{
																																								if (!(vm is TooltipBrickResourceInfoVM viewModel38))
																																								{
																																									if (!(vm is TooltipBrickUnifiedStatusVM viewModel39))
																																									{
																																										if (!(vm is TooltipBrickProfitFactorVM viewModel40))
																																										{
																																											if (!(vm is TooltipBrickIconPatternVM viewModel41))
																																											{
																																												if (!(vm is TooltipBricksGroupVM tooltipBricksGroupVM))
																																												{
																																													if (!(vm is TooltipBrickCargoCapacityVM viewModel42))
																																													{
																																														if (!(vm is TooltipBrickGlobalMapPositionVM viewModel43))
																																														{
																																															if (!(vm is TooltipBrickMomentumPortraitVM viewModel44))
																																															{
																																																if (!(vm is TooltipBricksMomentumPortraitsVM viewModel45))
																																																{
																																																	if (!(vm is TooltipBrickPortraitFeaturesVM viewModel46))
																																																	{
																																																		if (!(vm is TooltipBrickSliderVM viewModel47))
																																																		{
																																																			if (!(vm is TooltipBrickWeaponSetVM viewModel48))
																																																			{
																																																				if (!(vm is TooltipBrickArmorStatsVM viewModel49))
																																																				{
																																																					if (!(vm is TooltipBrickEventVM viewModel50))
																																																					{
																																																						if (!(vm is TooltipBrickIconAndTextWithCustomColorsVM viewModel51))
																																																						{
																																																							if (!(vm is TooltipBrickChanceVM viewModel52))
																																																							{
																																																								if (!(vm is TooltipBrickShotDirectionVM viewModel53))
																																																								{
																																																									if (!(vm is TooltipBrickShotDirectionWithNameVM viewModel54))
																																																									{
																																																										if (!(vm is TooltipBrickIconTextVM viewModel55))
																																																										{
																																																											if (!(vm is TooltipBrickTextValueVM viewModel56))
																																																											{
																																																												if (!(vm is TooltipBrickTextSignatureValueVM viewModel57))
																																																												{
																																																													if (!(vm is TooltipBrickDamageRangeVM viewModel58))
																																																													{
																																																														if (!(vm is TooltipBrickWidgetVM viewModel59))
																																																														{
																																																															if (!(vm is TooltipBrickIconTextValueVM viewModel60))
																																																															{
																																																																if (!(vm is TooltipBrickTriggeredAutoVM viewModel61))
																																																																{
																																																																	if (!(vm is TooltipBrickMinimalAdmissibleDamageVM viewModel62))
																																																																	{
																																																																		if (!(vm is TooltipBrickCalculatedFormulaVM viewModel63))
																																																																		{
																																																																			if (!(vm is TooltipBrickDamageNullifierVM viewModel64))
																																																																			{
																																																																				if (!(vm is TooltipBrickTitleWithIconVM viewModel65))
																																																																				{
																																																																					if (!(vm is TooltipBrickRankEntrySelectionVM viewModel66))
																																																																					{
																																																																						if (!(vm is TooltipBrickTextBackgroundVM viewModel67))
																																																																						{
																																																																							if (!(vm is TooltipBrickNestedMessageVM viewModel68))
																																																																							{
																																																																								if (!(vm is TooltipBrickAttributeVM viewModel69))
																																																																								{
																																																																									if (vm is TooltipBrickItemHeaderVM viewModel70)
																																																																									{
																																																																										TooltipBrickItemHeaderView widget = WidgetFactory.GetWidget(config.BrickItemHeaderView, activate: true, strictMatching: true);
																																																																										widget.Bind(viewModel70);
																																																																										return widget;
																																																																									}
																																																																									return null;
																																																																								}
																																																																								TooltipBrickAttributeView widget2 = WidgetFactory.GetWidget(config.BrickAttributeView, activate: true, strictMatching: true);
																																																																								widget2.Bind(viewModel69);
																																																																								return widget2;
																																																																							}
																																																																							TooltipBrickNestedMessageView widget3 = WidgetFactory.GetWidget(config.BrickNestedMessageView, activate: true, strictMatching: true);
																																																																							widget3.Bind(viewModel68);
																																																																							return widget3;
																																																																						}
																																																																						TooltipBrickTextBackgroundView widget4 = WidgetFactory.GetWidget(config.BrickTextBackgroundView, activate: true, strictMatching: true);
																																																																						widget4.Bind(viewModel67);
																																																																						return widget4;
																																																																					}
																																																																					TooltipBrickRankEntrySelectionView widget5 = WidgetFactory.GetWidget(config.BrickRankEntrySelectionView, activate: true, strictMatching: true);
																																																																					widget5.Bind(viewModel66);
																																																																					return widget5;
																																																																				}
																																																																				TooltipBrickTitleWithIconView widget6 = WidgetFactory.GetWidget(config.BrickTitleWithIconView, activate: true, strictMatching: true);
																																																																				widget6.Bind(viewModel65);
																																																																				return widget6;
																																																																			}
																																																																			TooltipBrickDamageNullifierView widget7 = WidgetFactory.GetWidget(config.BrickDamageNullifierView, activate: true, strictMatching: true);
																																																																			widget7.Bind(viewModel64);
																																																																			return widget7;
																																																																		}
																																																																		TooltipBrickCalculatedFormulaView widget8 = WidgetFactory.GetWidget(config.CalculatedFormulaView, activate: true, strictMatching: true);
																																																																		widget8.Bind(viewModel63);
																																																																		return widget8;
																																																																	}
																																																																	TooltipBrickMinimalAdmissibleDamageView widget9 = WidgetFactory.GetWidget(config.BrickMinimalAdmissibleDamageView, activate: true, strictMatching: true);
																																																																	widget9.Bind(viewModel62);
																																																																	return widget9;
																																																																}
																																																																TooltipBrickTriggeredAutoView widget10 = WidgetFactory.GetWidget(config.BrickTriggeredAutoView, activate: true, strictMatching: true);
																																																																widget10.Bind(viewModel61);
																																																																return widget10;
																																																															}
																																																															TooltipBrickIconTextValueView widget11 = WidgetFactory.GetWidget(config.BrickIconTextValueView, activate: true, strictMatching: true);
																																																															widget11.Bind(viewModel60);
																																																															return widget11;
																																																														}
																																																														TooltipBrickWidgetView widget12 = WidgetFactory.GetWidget(config.BrickWidgetView, activate: true, strictMatching: true);
																																																														widget12.Bind(viewModel59);
																																																														return widget12;
																																																													}
																																																													TooltipBrickDamageRangeView widget13 = WidgetFactory.GetWidget(config.BrickDamageRangeView, activate: true, strictMatching: true);
																																																													widget13.Bind(viewModel58);
																																																													return widget13;
																																																												}
																																																												TooltipBrickTextSignatureValueView widget14 = WidgetFactory.GetWidget(config.BrickTextSignatureValueView, activate: true, strictMatching: true);
																																																												widget14.Bind(viewModel57);
																																																												return widget14;
																																																											}
																																																											TooltipBrickTextValueView widget15 = WidgetFactory.GetWidget(config.BrickTextValueView, activate: true, strictMatching: true);
																																																											widget15.Bind(viewModel56);
																																																											return widget15;
																																																										}
																																																										TooltipBrickIconTextView widget16 = WidgetFactory.GetWidget(config.BrickIconTextView, activate: true, strictMatching: true);
																																																										widget16.Bind(viewModel55);
																																																										return widget16;
																																																									}
																																																									TooltipBrickShotDeviationWithNameView widget17 = WidgetFactory.GetWidget(config.BrickShotDeviationWithNameView, activate: true, strictMatching: true);
																																																									widget17.Bind(viewModel54);
																																																									return widget17;
																																																								}
																																																								TooltipBrickShotDeviationView widget18 = WidgetFactory.GetWidget(config.BrickShotDeviationView, activate: true, strictMatching: true);
																																																								widget18.Bind(viewModel53);
																																																								return widget18;
																																																							}
																																																							TooltipBrickChanceView widget19 = WidgetFactory.GetWidget(config.BrickChanceView, activate: true, strictMatching: true);
																																																							widget19.Bind(viewModel52);
																																																							return widget19;
																																																						}
																																																						TooltipBrickIconAndTextWithCustomColorsView widget20 = WidgetFactory.GetWidget(config.IconAndTextWithCustomColorsView, activate: true, strictMatching: true);
																																																						widget20.Bind(viewModel51);
																																																						return widget20;
																																																					}
																																																					TooltipBrickEventsView widget21 = WidgetFactory.GetWidget(config.EventsView, activate: true, strictMatching: true);
																																																					widget21.Bind(viewModel50);
																																																					return widget21;
																																																				}
																																																				TooltipBrickArmorStatsView widget22 = WidgetFactory.GetWidget(config.ArmorStatsView, activate: true, strictMatching: true);
																																																				widget22.Bind(viewModel49);
																																																				return widget22;
																																																			}
																																																			TooltipBrickWeaponSetView widget23 = WidgetFactory.GetWidget(config.WeaponSetView, activate: true, strictMatching: true);
																																																			widget23.Bind(viewModel48);
																																																			return widget23;
																																																		}
																																																		TooltipBrickSliderView widget24 = WidgetFactory.GetWidget(config.SliderView, activate: true, strictMatching: true);
																																																		widget24.Bind(viewModel47);
																																																		return widget24;
																																																	}
																																																	TooltipBrickPortraitFeaturesView widget25 = WidgetFactory.GetWidget(config.PortraitFeaturesView, activate: true, strictMatching: true);
																																																	widget25.Bind(viewModel46);
																																																	return widget25;
																																																}
																																																TooltipBricksMomentumPortraitsView widget26 = WidgetFactory.GetWidget(config.MomentumPortraitsView, activate: true, strictMatching: true);
																																																widget26.Bind(viewModel45);
																																																return widget26;
																																															}
																																															TooltipBrickMomentumPortraitView widget27 = WidgetFactory.GetWidget(config.MomentumPortraitView, activate: true, strictMatching: true);
																																															widget27.Bind(viewModel44);
																																															return widget27;
																																														}
																																														TooltipBrickGlobalMapPositionView widget28 = WidgetFactory.GetWidget(config.GlobalMapPositionView, activate: true, strictMatching: true);
																																														widget28.Bind(viewModel43);
																																														return widget28;
																																													}
																																													TooltipBrickCargoCapacityView widget29 = WidgetFactory.GetWidget(config.CargoCapacityView, activate: true, strictMatching: true);
																																													widget29.Bind(viewModel42);
																																													return widget29;
																																												}
																																												if (tooltipBricksGroupVM.Type == TooltipBricksGroupType.Start)
																																												{
																																													TooltipBricksGroupView widget30 = WidgetFactory.GetWidget(config.BricksGroupView, activate: true, strictMatching: true);
																																													widget30.Bind(tooltipBricksGroupVM);
																																													return widget30;
																																												}
																																												return null;
																																											}
																																											TooltipBrickIconPatternView widget31 = WidgetFactory.GetWidget(config.IconPatternView, activate: true, strictMatching: true);
																																											widget31.Bind(viewModel41);
																																											return widget31;
																																										}
																																										TooltipBrickProfitFactorView widget32 = WidgetFactory.GetWidget(config.ProfitFactorView, activate: true, strictMatching: true);
																																										widget32.Bind(viewModel40);
																																										return widget32;
																																									}
																																									TooltipBrickUnifiedStatusView widget33 = WidgetFactory.GetWidget(config.UnifiedStatusView, activate: true, strictMatching: true);
																																									widget33.Bind(viewModel39);
																																									return widget33;
																																								}
																																								TooltipBrickResourceInfoView widget34 = WidgetFactory.GetWidget(config.ResourceInfoView, activate: true, strictMatching: true);
																																								widget34.Bind(viewModel38);
																																								return widget34;
																																							}
																																							TooltipBrickAnomalyInfoView widget35 = WidgetFactory.GetWidget(config.AnomalyInfoView, activate: true, strictMatching: true);
																																							widget35.Bind(viewModel37);
																																							return widget35;
																																						}
																																						TooltipBrickOtherObjectsInfoView widget36 = WidgetFactory.GetWidget(config.OtherObjectsInfoView, activate: true, strictMatching: true);
																																						widget36.Bind(viewModel36);
																																						return widget36;
																																					}
																																					TooltipBrickPlanetInfoView widget37 = WidgetFactory.GetWidget(config.PlanetInfoView, activate: true, strictMatching: true);
																																					widget37.Bind(viewModel35);
																																					return widget37;
																																				}
																																				TooltipBrickHintView widget38 = WidgetFactory.GetWidget(config.BrickHintView, activate: true, strictMatching: true);
																																				widget38.Bind(viewModel34);
																																				return widget38;
																																			}
																																			TooltipBrickAbilityTargetView widget39 = WidgetFactory.GetWidget(config.BrickAbilityTargetView, activate: true, strictMatching: true);
																																			widget39.Bind(viewModel33);
																																			return widget39;
																																		}
																																		TooltipBrickSkillsView widget40 = WidgetFactory.GetWidget(config.BrickSkillsView, activate: true, strictMatching: true);
																																		widget40.Bind(viewModel32);
																																		return widget40;
																																	}
																																	TooltipBrickFeatureShortDescriptionView widget41 = WidgetFactory.GetWidget(config.BrickFeatureShortDescriptionView, activate: true, strictMatching: true);
																																	widget41.Bind(viewModel31);
																																	return widget41;
																																}
																																TooltipBrickRateView widget42 = WidgetFactory.GetWidget(config.BrickRateView, activate: true, strictMatching: true);
																																widget42.Bind(viewModel30);
																																return widget42;
																															}
																															TooltipBrickPrerequisiteView widget43 = WidgetFactory.GetWidget(config.BrickPrerequisiteView, activate: true, strictMatching: true);
																															widget43.Bind(viewModel29);
																															return widget43;
																														}
																														TooltipBrickNonStackView widget44 = WidgetFactory.GetWidget(config.BrickNonStackView, activate: true, strictMatching: true);
																														widget44.Bind(viewModel28);
																														return widget44;
																													}
																													TooltipBrickHistoryManagementView widget45 = WidgetFactory.GetWidget(config.BrickHistoryManagementView, activate: true, strictMatching: true);
																													widget45.Bind(viewModel27);
																													return widget45;
																												}
																												TooltipBrickButtonView widget46 = WidgetFactory.GetWidget(config.BrickButtonView, activate: true, strictMatching: true);
																												widget46.Bind(viewModel26);
																												return widget46;
																											}
																											TooltipBrickEncumbranceView widget47 = WidgetFactory.GetWidget(config.BrickEncumbranceView, activate: true, strictMatching: true);
																											widget47.Bind(viewModel25);
																											return widget47;
																										}
																										TooltipBrickAbilityScoresView widget48 = WidgetFactory.GetWidget(config.BrickAbilityScoresView, activate: true, strictMatching: true);
																										widget48.Bind(viewModel24);
																										return widget48;
																									}
																									TooltipBrickAbilityScoresBlockView widget49 = WidgetFactory.GetWidget(config.BrickAbilityScoresBlockView, activate: true, strictMatching: true);
																									widget49.Bind(viewModel23);
																									return widget49;
																								}
																								if (tooltipBrickFeatureVM.IsHeader)
																								{
																									TooltipBrickFeatureHeaderView widget50 = WidgetFactory.GetWidget(config.BrickFeatureHeaderView, activate: true, strictMatching: true);
																									widget50.Bind(tooltipBrickFeatureVM);
																									return widget50;
																								}
																								TooltipBrickFeatureView widget51 = WidgetFactory.GetWidget(config.BrickFeatureView, activate: true, strictMatching: true);
																								widget51.Bind(tooltipBrickFeatureVM);
																								return widget51;
																							}
																							TooltipBrickBuffView widget52 = WidgetFactory.GetWidget(config.BrickBuffView, activate: true, strictMatching: true);
																							widget52.Bind(viewModel22);
																							return widget52;
																						}
																						TooltipBrickEntityHeaderView widget53 = WidgetFactory.GetWidget(config.BrickEntityHeaderView, activate: true, strictMatching: true);
																						widget53.Bind(viewModel21);
																						return widget53;
																					}
																					TooltipBrickValueStatFormulaView widget54 = WidgetFactory.GetWidget(config.BrickValueStatFormulaView, activate: true, strictMatching: true);
																					widget54.Bind(viewModel20);
																					return widget54;
																				}
																				TooltipBrickTwoColumnsStatView widget55 = WidgetFactory.GetWidget(config.BrickTwoColumnsStatView, activate: true, strictMatching: true);
																				widget55.Bind(viewModel19);
																				return widget55;
																			}
																			TooltipBrickIconStatValueView widget56 = WidgetFactory.GetWidget(config.BrickIconStatValueView, activate: true, strictMatching: true);
																			widget56.Bind(viewModel18);
																			return widget56;
																		}
																		TooltipBrickIconValueStatView widget57 = WidgetFactory.GetWidget(config.BrickIconValueStatView, activate: true, strictMatching: true);
																		widget57.Bind(viewModel17);
																		return widget57;
																	}
																	TooltipBrickDoubleTextView widget58 = WidgetFactory.GetWidget(config.BrickDoubleTextView, activate: true, strictMatching: true);
																	widget58.Bind(viewModel16);
																	return widget58;
																}
																TooltipBrickTripleTextView widget59 = WidgetFactory.GetWidget(config.BrickTripleTextView, activate: true, strictMatching: true);
																widget59.Bind(viewModel15);
																return widget59;
															}
															TooltipBrickItemFooterView widget60 = WidgetFactory.GetWidget(config.BrickItemFooterView, activate: true, strictMatching: true);
															widget60.Bind(viewModel14);
															return widget60;
														}
														TooltipBrickFactionStatusView widget61 = WidgetFactory.GetWidget(config.FactionStatusView, activate: true, strictMatching: true);
														widget61.Bind(viewModel13);
														return widget61;
													}
													TooltipBrickIconAndNameView widget62 = WidgetFactory.GetWidget(config.BrickIconAndNameView, activate: true, strictMatching: true);
													widget62.Bind(viewModel12);
													return widget62;
												}
												TooltipBrickPFIconAndNameView widget63 = WidgetFactory.GetWidget(config.BrickPFIconAndNameView, activate: true, strictMatching: true);
												widget63.Bind(viewModel11);
												return widget63;
											}
											TooltipBrickColonyProjectProgressView widget64 = WidgetFactory.GetWidget(config.BrickColonyProjectProgressView, activate: true, strictMatching: true);
											widget64.Bind(viewModel10);
											return widget64;
										}
										TooltipBrickResourceIconAndNameView widget65 = WidgetFactory.GetWidget(config.BrickResourceIconAndNameView, activate: true, strictMatching: true);
										widget65.Bind(viewModel9);
										return widget65;
									}
									TooltipBrickItemIconAndNameView widget66 = WidgetFactory.GetWidget(config.BrickItemIconAndNameView, activate: true, strictMatching: true);
									widget66.Bind(viewModel8);
									return widget66;
								}
								TooltipBrickPortraitAndNameView widget67 = WidgetFactory.GetWidget(config.BrickPortraitAndNameView, activate: true, strictMatching: true);
								widget67.Bind(viewModel7);
								return widget67;
							}
							TooltipBrickPictureView widget68 = WidgetFactory.GetWidget(config.BrickPictureView, activate: true, strictMatching: true);
							widget68.Bind(viewModel6);
							return widget68;
						}
						TooltipBrickTitleView widget69 = WidgetFactory.GetWidget(config.BrickTitleView, activate: true, strictMatching: true);
						widget69.Bind(viewModel5);
						return widget69;
					}
					TooltipBrickSpaceView widget70 = WidgetFactory.GetWidget(config.BrickSpaceView, activate: true, strictMatching: true);
					widget70.Bind(viewModel4);
					return widget70;
				}
				TooltipBrickSeparatorView widget71 = WidgetFactory.GetWidget(config.BrickSeparatorView, activate: true, strictMatching: true);
				widget71.Bind(viewModel3);
				return widget71;
			}
			TooltipBrickTextView widget72 = WidgetFactory.GetWidget(config.BrickTextView, activate: true, strictMatching: true);
			widget72.Bind(viewModel2);
			return widget72;
		}
		TooltipBrickTimerView widget73 = WidgetFactory.GetWidget(config.BrickTimerView, activate: true, strictMatching: true);
		widget73.Bind(viewModel);
		return widget73;
	}

	public static void DestroyBrickView(MonoBehaviour view)
	{
		WidgetFactory.DisposeWidget(view);
	}

	public static void SetPivots(List<Vector2> pivots, List<Vector2> priorityPivots)
	{
		if (priorityPivots != null && priorityPivots.Count >= 0)
		{
			pivots.AddRange(priorityPivots);
		}
		if (pivots.Count <= 0)
		{
			pivots.AddRange(DefaultPivots);
		}
		float num = 10f;
		for (float num2 = 0f; num2 <= 1f; num2 += 1f)
		{
			for (float num3 = 0f; num3 <= num; num3 += 1f)
			{
				Vector2 item = new Vector2(num2, num3 / num);
				if (!pivots.Contains(item))
				{
					pivots.Add(new Vector2(num2, num3 / num));
				}
			}
		}
		for (float num4 = 0f; num4 <= 1f; num4 += 1f)
		{
			for (float num5 = 0f; num5 <= num; num5 += 1f)
			{
				Vector2 item2 = new Vector2(num5 / num, num4);
				if (!pivots.Contains(item2))
				{
					pivots.Add(new Vector2(num5 / num, num4));
				}
			}
		}
	}
}
