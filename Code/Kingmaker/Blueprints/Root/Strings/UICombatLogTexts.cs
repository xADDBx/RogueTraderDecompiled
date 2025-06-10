using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICombatLogTexts
{
	public LocalizedString MomentumChanged;

	public LocalizedString MomentumChangeReason;

	[Space]
	public LocalizedString MomentumTypeCustom;

	public LocalizedString MomentumTypeFallDeadOrUnconscious;

	public LocalizedString MomentumTypeKillEnemy;

	public LocalizedString MomentumTypeStartTurn;

	public LocalizedString MomentumTypeAbilityCost;

	public LocalizedString MomentumTypeWound;

	public LocalizedString MomentumTypeTrauma;

	public LocalizedString MomentumFlatBonus;

	public LocalizedString MomentumResolveLostBase;

	public LocalizedString MomentumSourceResolve;

	public LocalizedString MomentumTargetResolveGained;

	public LocalizedString MomentumFactor;

	[Space]
	public LocalizedString ScatterShotHits;

	public LocalizedString ScatterShotCoverHits;

	public LocalizedString ScatterShotMiss;

	[Space]
	public LocalizedString ShotDirectionDeviation;

	public LocalizedString CentralShotDirection;

	public LocalizedString SlightDeviationShotDirection;

	public LocalizedString StrongDeviationShotDirection;

	public LocalizedString DeviationDescription;

	public LocalizedString DeviationHeader;

	[Space]
	public LocalizedString ChangeSize;

	public LocalizedString ShowModePin;

	public LocalizedString ShowModeUnpin;

	public LocalizedString ShowUnit;

	[Space]
	public LocalizedString LanceResultTitle;

	[Space]
	public LocalizedString AlwaysSucceed;

	public LocalizedString AlwaysFail;

	[Space]
	public LocalizedString Reflected;
}
