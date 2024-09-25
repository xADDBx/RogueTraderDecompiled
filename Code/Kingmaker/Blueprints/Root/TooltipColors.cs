using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TooltipColors
{
	[Header("Tooltip")]
	public Color32 Default;

	public Color32 Bonus;

	public Color32 Penaty;

	public Color32 CanNotEquip;

	public Color32 Equiped;

	public Color32 HeroicActAbility;

	public Color32 DesperateMeasureAbility;

	public Color32 TooltipValue;

	[Header("Backgrounds")]
	public Color32 WhiteBackground;

	public Color32 RedBackground;

	public Color32 YellowBackground;

	public Color32 GrayBackground;

	[Header("Info")]
	public Color32 DefaultInfo;

	public Color32 CanNotEquipInfo;

	public Color32 EquipedInfo;

	[Header("HUDPoints")]
	public Color32 MovePoints;

	public Color32 ActionPoints;

	public Color32 NotEnoughPoints;

	[Header("VeilMomentumProgressbar")]
	public Color32 ProgressbarNeutral;

	public Color32 ProgressbarBonus;

	public Color32 ProgressbarPenalty;
}
