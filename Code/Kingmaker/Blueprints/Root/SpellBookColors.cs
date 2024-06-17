using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SpellBookColors
{
	[Header("Spell BookView")]
	public Color32 DefaultSlotColot;

	public Color32 FavoriteSlotColor;

	public Color32 OppositeSlotcolor;

	public Color32 DomainSlotColor;

	[Header("SpellDecoration")]
	public Color32[] Colors;

	public Sprite[] Borders;

	[Header("Metamagic")]
	public Sprite MetamagicEmpower;

	public Sprite MetamagicMaximize;

	public Sprite MetamagicQuicken;

	public Sprite MetamagicExtend;

	public Sprite MetamagicHeighten;

	public Sprite MetamagicReach;

	public Sprite MetamagicCompletelyNormal;

	public Sprite MetamagicSelective;
}
