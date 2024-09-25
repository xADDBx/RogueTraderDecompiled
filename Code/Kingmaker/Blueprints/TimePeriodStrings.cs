using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
public class TimePeriodStrings
{
	[Header("Day strings")]
	public LocalizedString OneDay;

	public LocalizedString TwoDays;

	public LocalizedString ThreeDays;

	public LocalizedString FourDays;

	public LocalizedString Days;

	[Header("Hour strings")]
	public LocalizedString LessThanAnHour;

	public LocalizedString OneHour;

	public LocalizedString TwoHours;

	public LocalizedString ThreeHours;

	public LocalizedString FourHours;

	public LocalizedString Hours;

	public LocalizedString And;

	[Header("Period Acronyms")]
	public LocalizedString CompactDay;

	public LocalizedString CompactHour;

	public LocalizedString CompactMinute;

	public LocalizedString CompactSecond;
}
