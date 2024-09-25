using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextAlignment
{
	[Header("WH Soul Marks")]
	public LocalizedString Imperialis;

	public LocalizedString Benevolentia;

	public LocalizedString Hereticus;

	public LocalizedString Reason;

	[Header("Soul Marks Ranks")]
	public LocalizedString SoulMarkRankTierNone;

	public LocalizedString SoulMarkRankTier1;

	public LocalizedString SoulMarkRankTier2;

	public LocalizedString SoulMarkRankTier3;

	public LocalizedString SoulMarkRankTier4;

	public LocalizedString SoulMarkRankTier5;

	[Header("Conviction")]
	public LocalizedString RadicalTitle;

	public LocalizedString RadicalDescription;

	public LocalizedString PuritanTitle;

	public LocalizedString PuritanDescription;

	public LocalizedString CurrentConvictionTitle;

	public LocalizedString CurrentConvictionDescription;
}
