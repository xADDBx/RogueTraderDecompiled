using System;
using System.Collections.Generic;
using Kingmaker.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CharScreenColors
{
	[Header("Strings")]
	public Color32 Label;

	public Color32 LabelHighlight;

	public Color32 LabelBackground;

	public Color32 LabelBackgroundHighlight;

	public Color32 Icon1;

	public Color32 Icon2;

	public Color32 DisableValue;

	public Color32 DisableValue2;

	public Color32 NormalValue;

	public Color32 PenaltyValue;

	public Color32 BonusValue;

	public Color32 PenaltyValue2;

	public Color32 BonusValue2;

	[Header("Alignment")]
	public List<Color32> Good;

	public List<Color32> Neutral;

	public List<Color32> Evil;

	[Header("Backgrounds")]
	public Color32 PenaltyBG;

	public Color32 BonusBG;

	public Color32 SkillPenaltyBG;

	public Color32 SkillBonusBG;

	[Header("Class Progression")]
	public Color32 GotLevel;

	public Color32 GotLevel2;

	public Color32 GotLevelHighlighted;

	public Color32 GotLevel2Highlighted;

	public Color32 FutureLevel;

	public Color32 DisableLevel;

	[Header("Chargen color: Normal")]
	public ColorBlock MenuButtonNormal;

	[Header("Chargen color: Locked")]
	public ColorBlock MenuButtonLocked;

	[Header("Chargen color: Selected")]
	public ColorBlock MenuButtonSelected;

	[Header("Chargen color: Done")]
	public ColorBlock MenuButtonDone;

	[Header("Chargen color: Attention")]
	public ColorBlock MenuButtonAttention;

	[Header("Factions Reputation")]
	public Color32 Drusians;

	public Color32 Explorators;

	public Color32 Kasballica;

	public Color32 Pirates;

	public Color32 ShipVendor;

	public Color32 GetFactionColor(FactionType factionType)
	{
		return factionType switch
		{
			FactionType.Drusians => Drusians, 
			FactionType.Explorators => Explorators, 
			FactionType.Kasballica => Kasballica, 
			FactionType.Pirates => Pirates, 
			FactionType.ShipVendor => ShipVendor, 
			_ => Color.black, 
		};
	}
}
