using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip;

public class TooltipBricksView : MonoBehaviour
{
	public TooltipBrickTextView BrickTextView;

	public TooltipBrickSeparatorView BrickSeparatorView;

	public TooltipBrickSpaceView BrickSpaceView;

	public TooltipBrickTitleView BrickTitleView;

	public TooltipBrickPictureView BrickPictureView;

	public TooltipBrickIconAndNameView BrickIconAndNameView;

	public TooltipBrickFactionStatusView FactionStatusView;

	public TooltipBrickDoubleTextView BrickDoubleTextView;

	public TooltipBrickTripleTextView BrickTripleTextView;

	public TooltipBrickIconValueStatView BrickIconValueStatView;

	public TooltipBrickIconStatValueView BrickIconStatValueView;

	public TooltipBrickTwoColumnsStatView BrickTwoColumnsStatView;

	public TooltipBrickValueStatFormulaView BrickValueStatFormulaView;

	public TooltipBrickTimerView BrickTimerView;

	public TooltipBrickEntityHeaderView BrickEntityHeaderView;

	public TooltipBrickFeatureView BrickFeatureView;

	public TooltipBrickFeatureHeaderView BrickFeatureHeaderView;

	public TooltipBrickBuffView BrickBuffView;

	public TooltipBrickBuffDOTView BrickBuffDOTView;

	public TooltipBrickWeaponDOTInitialDamageView BrickWeaponDOTInitialDamageView;

	public TooltipBrickItemFooterView BrickItemFooterView;

	public TooltipBrickAbilityScoresView BrickAbilityScoresView;

	public TooltipBrickEncumbranceView BrickEncumbranceView;

	public TooltipBrickPortraitAndNameView BrickPortraitAndNameView;

	public TooltipBrickItemIconAndNameView BrickItemIconAndNameView;

	public TooltipBrickResourceIconAndNameView BrickResourceIconAndNameView;

	public TooltipBrickColonyProjectProgressView BrickColonyProjectProgressView;

	public TooltipBrickPFIconAndNameView BrickPFIconAndNameView;

	public TooltipBrickButtonView BrickButtonView;

	public TooltipBrickHistoryManagementView BrickHistoryManagementView;

	public TooltipBrickNonStackView BrickNonStackView;

	public TooltipBrickPrerequisiteView BrickPrerequisiteView;

	public TooltipBrickRateView BrickRateView;

	public TooltipBrickFeatureShortDescriptionView BrickFeatureShortDescriptionView;

	public TooltipBrickAbilityScoresBlockView BrickAbilityScoresBlockView;

	public TooltipBrickSkillsView BrickSkillsView;

	public TooltipBrickAbilityTargetView BrickAbilityTargetView;

	public TooltipBrickHintView BrickHintView;

	public TooltipBrickPlanetInfoView PlanetInfoView;

	public TooltipBrickOtherObjectsInfoView OtherObjectsInfoView;

	public TooltipBrickAnomalyInfoView AnomalyInfoView;

	public TooltipBrickResourceInfoView ResourceInfoView;

	public TooltipBrickUnifiedStatusView UnifiedStatusView;

	public TooltipBrickProfitFactorView ProfitFactorView;

	public TooltipBrickIconPatternView IconPatternView;

	public TooltipBricksGroupView BricksGroupView;

	public TooltipBrickCargoCapacityView CargoCapacityView;

	public TooltipBrickGlobalMapPositionView GlobalMapPositionView;

	public TooltipBrickMomentumPortraitView MomentumPortraitView;

	public TooltipBricksMomentumPortraitsView MomentumPortraitsView;

	public TooltipBrickPortraitFeaturesView PortraitFeaturesView;

	public TooltipBrickSliderView SliderView;

	public TooltipBrickWeaponSetView WeaponSetView;

	public TooltipBrickArmorStatsView ArmorStatsView;

	public TooltipBrickEventsView EventsView;

	public TooltipBrickIconAndTextWithCustomColorsView IconAndTextWithCustomColorsView;

	public TooltipBrickWidgetView BrickWidgetView;

	public TooltipBrickCalculatedFormulaView CalculatedFormulaView;

	public TooltipBrickTitleWithIconView BrickTitleWithIconView;

	public TooltipBrickRankEntrySelectionView BrickRankEntrySelectionView;

	public TooltipBrickTextBackgroundView BrickTextBackgroundView;

	public TooltipBrickAttributeView BrickAttributeView;

	public TooltipBrickItemHeaderView BrickItemHeaderView;

	[Header("Combat Log")]
	public TooltipBrickChanceView BrickChanceView;

	public TooltipBrickShotDeviationView BrickShotDeviationView;

	public TooltipBrickShotDeviationWithNameView BrickShotDeviationWithNameView;

	public TooltipBrickIconTextView BrickIconTextView;

	public TooltipBrickIconTextValueView BrickIconTextValueView;

	public TooltipBrickTextValueView BrickTextValueView;

	public TooltipBrickTextSignatureValueView BrickTextSignatureValueView;

	public TooltipBrickDamageRangeView BrickDamageRangeView;

	public TooltipBrickMinimalAdmissibleDamageView BrickMinimalAdmissibleDamageView;

	public TooltipBrickTriggeredAutoView BrickTriggeredAutoView;

	public TooltipBrickDamageNullifierView BrickDamageNullifierView;

	public TooltipBrickNestedMessageView BrickNestedMessageView;
}
