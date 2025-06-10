using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UINotificationTexts
{
	public LocalizedString ItemsLostFormat;

	public LocalizedString ItemsRecievedFormat;

	public LocalizedString ColonyResourceReceivedFormat;

	public LocalizedString XPGainedFormat;

	public LocalizedString CargoAddedFormat;

	public LocalizedString CargoLostFormat;

	public LocalizedString VoidshipDamagedFormat;

	public LocalizedString DamageDealtFormat;

	public LocalizedString NavigatorResourceAddedFormat;

	public LocalizedString NavigatorResourceLostFormat;

	public LocalizedString SoulMarksShiftFormat;

	public LocalizedString GainedProfitFactor;

	public LocalizedString LostProfitFactor;

	public LocalizedString FactionReputationLostFormat;

	public LocalizedString FactionReputationReceivedFormat;

	public LocalizedString FactionVendorDiscountLostFormat;

	public LocalizedString FactionVendorDiscountReceivedFormat;

	public LocalizedString AbilityAddedFormat;

	public LocalizedString BuffAddedFormat;

	public static UINotificationTexts Instance => UIStrings.Instance.NotificationTexts;
}
