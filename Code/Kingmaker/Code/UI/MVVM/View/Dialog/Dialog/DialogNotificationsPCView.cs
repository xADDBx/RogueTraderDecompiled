using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogNotificationsPCView : ViewBase<DialogNotificationsVM>, ISettingsFontSizeUIHandler, ISubscriber
{
	[SerializeField]
	private TextMeshProUGUI m_NotificationPhraseLocations;

	[SerializeField]
	private TextMeshProUGUI m_NotificationPhraseItemsReceived;

	[SerializeField]
	private TextMeshProUGUI m_NotificationPhraseItemsLost;

	[SerializeField]
	private TextMeshProUGUI m_NotificationProfitFactorChanged;

	[SerializeField]
	private TextMeshProUGUI m_NotificationXpGained;

	[SerializeField]
	private TextMeshProUGUI m_NotificationCargoAdded;

	[SerializeField]
	private TextMeshProUGUI m_NotificationCargoLost;

	[SerializeField]
	private TextMeshProUGUI m_NotificationDamageDealt;

	[SerializeField]
	private TextMeshProUGUI m_NotificationNavigatorResourceAdded;

	[SerializeField]
	private TextMeshProUGUI m_NotificationFactionReputationReceived;

	[SerializeField]
	private TextMeshProUGUI m_NotificationFactionReputationLost;

	[SerializeField]
	private TextMeshProUGUI m_NotificationFactionVendorDiscountReceived;

	[SerializeField]
	private TextMeshProUGUI m_NotificationFactionVendorDiscountLost;

	[SerializeField]
	private TextMeshProUGUI m_NotificationAbilityAdded;

	[SerializeField]
	private TextMeshProUGUI m_NotificationBuffAdded;

	[SerializeField]
	private TextMeshProUGUI m_NotificatoinSoulMarksShift;

	[SerializeField]
	private bool m_IsSpaceEvent;

	[SerializeField]
	private float m_DefaultFontSize = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 24f;

	private DialogColors m_DialogColors;

	private bool m_IsInit;

	private CompositeDisposable m_Disposable;

	public void Initialize(DialogColors dialogColors)
	{
		if (!m_IsInit)
		{
			m_DialogColors = dialogColors;
			base.gameObject.SetActive(value: false);
			Clear();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.OnUpdateCommand.Subscribe(OnUpdateHandler));
		SetTextFontSize(base.ViewModel.FontSizeMultiplier);
	}

	private void SetTextFontSize(float multiplier)
	{
		float fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * multiplier;
		m_NotificationPhraseLocations.fontSize = fontSize;
		m_NotificationPhraseItemsReceived.fontSize = fontSize;
		m_NotificationPhraseItemsLost.fontSize = fontSize;
		m_NotificationXpGained.fontSize = fontSize;
		m_NotificationCargoAdded.fontSize = fontSize;
		m_NotificationCargoLost.fontSize = fontSize;
		m_NotificationDamageDealt.fontSize = fontSize;
		m_NotificationNavigatorResourceAdded.fontSize = fontSize;
		m_NotificationProfitFactorChanged.fontSize = fontSize;
		m_NotificationFactionReputationReceived.fontSize = fontSize;
		m_NotificationFactionReputationLost.fontSize = fontSize;
		m_NotificationFactionVendorDiscountReceived.fontSize = fontSize;
		m_NotificationFactionVendorDiscountLost.fontSize = fontSize;
		m_NotificationAbilityAdded.fontSize = fontSize;
		m_NotificationBuffAdded.fontSize = fontSize;
		m_NotificatoinSoulMarksShift.fontSize = fontSize;
	}

	private void Clear()
	{
		m_NotificationPhraseLocations.gameObject.SetActive(value: false);
		m_NotificationPhraseItemsReceived.gameObject.SetActive(value: false);
		m_NotificationPhraseItemsLost.gameObject.SetActive(value: false);
		m_NotificationXpGained.gameObject.SetActive(value: false);
		m_NotificationCargoAdded.gameObject.SetActive(value: false);
		m_NotificationCargoLost.gameObject.SetActive(value: false);
		m_NotificationDamageDealt.Or(null)?.gameObject.SetActive(value: false);
		m_NotificationNavigatorResourceAdded.gameObject.SetActive(value: false);
		m_NotificationProfitFactorChanged.gameObject.SetActive(value: false);
		m_NotificationFactionReputationReceived.gameObject.SetActive(value: false);
		m_NotificationFactionReputationLost.gameObject.SetActive(value: false);
		m_NotificationFactionVendorDiscountReceived.gameObject.SetActive(value: false);
		m_NotificationFactionVendorDiscountLost.gameObject.SetActive(value: false);
		m_NotificationAbilityAdded.gameObject.SetActive(value: false);
		m_NotificationBuffAdded.gameObject.SetActive(value: false);
		m_NotificatoinSoulMarksShift.gameObject.SetActive(value: false);
	}

	private void OnUpdateHandler(bool show)
	{
		base.gameObject.SetActive(show);
		if (!show)
		{
			return;
		}
		UISounds.Instance.Sounds.Journal.NewInformation.Play();
		m_Disposable?.Dispose();
		m_Disposable = null;
		m_Disposable = new CompositeDisposable();
		if (m_Disposable.Any())
		{
			m_Disposable?.ForEach(delegate(IDisposable d)
			{
				d.Dispose();
			});
			m_Disposable?.Clear();
		}
		Clear();
		SetItemReceivedOrLostNotification();
		SetXpGains();
		SetCustomNotifications();
		SetCargoAdded();
		SetCargoLost();
		SetDamageDealt();
		SetNavigatorResourceAdded();
		SetSoulMarkShift();
		SetProfitFactorChanged();
		SetFactionReputationChanged();
		SetFactionVendorDiscountChanged();
		SetAbilityAdded();
		SetBuffAdded();
	}

	private void SetItemReceivedOrLostNotification()
	{
		List<KeyValuePair<string, int>> list = base.ViewModel.ItemsChanged.Where((KeyValuePair<string, int> k) => k.Value > 0).ToList();
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				SmartAppend(list[i], stringBuilder);
			}
			m_NotificationPhraseItemsReceived.text = string.Format(UINotificationTexts.Instance.ItemsRecievedFormat, stringBuilder);
			m_NotificationPhraseItemsReceived.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationPhraseItemsReceived.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
		List<KeyValuePair<string, int>> list2 = base.ViewModel.ItemsChanged.Where((KeyValuePair<string, int> k) => k.Value < 0).ToList();
		if (list2.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int j = 0; j < list2.Count; j++)
		{
			if (j > 0)
			{
				stringBuilder2.Append(", ");
			}
			SmartAppend(list2[j], stringBuilder2);
		}
		m_NotificationPhraseItemsLost.text = string.Format(UINotificationTexts.Instance.ItemsLostFormat, stringBuilder2);
		m_NotificationPhraseItemsLost.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationPhraseItemsLost.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetXpGains()
	{
		if (base.ViewModel.XpGains.Count > 0)
		{
			int num = base.ViewModel.XpGains.Sum((int x) => x);
			string format = base.ViewModel.LinkGenerate(UINotificationTexts.Instance.XPGainedFormat, "Encyclopedia:ExperiencePoints");
			m_NotificationXpGained.text = string.Format(format, num);
			m_NotificationXpGained.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationXpGained.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
	}

	private void SetCargoAdded()
	{
		List<string> list = base.ViewModel.CargoAdded.ToList();
		if (list.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(list[i]);
		}
		m_NotificationCargoAdded.text = string.Format(UINotificationTexts.Instance.CargoAddedFormat, stringBuilder);
		m_NotificationCargoAdded.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationCargoAdded.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetCargoLost()
	{
		List<string> list = base.ViewModel.CargoLost.ToList();
		if (list.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(list[i]);
		}
		m_NotificationCargoLost.text = string.Format(UINotificationTexts.Instance.CargoLostFormat, stringBuilder);
		m_NotificationCargoLost.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationCargoLost.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetDamageDealt()
	{
		List<KeyValuePair<string, int>> list = base.ViewModel.DamageDealt.Where((KeyValuePair<string, int> k) => k.Value > 0).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			DamageDealtAppend(list[i], stringBuilder);
		}
		if ((bool)m_NotificationDamageDealt)
		{
			string format = (m_IsSpaceEvent ? UINotificationTexts.Instance.VoidshipDamagedFormat : UINotificationTexts.Instance.DamageDealtFormat);
			m_NotificationDamageDealt.text = string.Format(format, stringBuilder);
			m_NotificationDamageDealt.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationDamageDealt.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
	}

	private void SetNavigatorResourceAdded()
	{
		if (base.ViewModel.NavigatorResourceAdded.Count > 0)
		{
			int num = base.ViewModel.NavigatorResourceAdded.Sum((int x) => x);
			string arg = base.ViewModel.LinkGenerate($"{Math.Abs(num)}", "Encyclopedia:NavigatorsInsight");
			m_NotificationNavigatorResourceAdded.text = string.Format((num < 0) ? UINotificationTexts.Instance.NavigatorResourceLostFormat : UINotificationTexts.Instance.NavigatorResourceAddedFormat, arg);
			m_NotificationNavigatorResourceAdded.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationNavigatorResourceAdded.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
	}

	private void SetProfitFactorChanged()
	{
		if (base.ViewModel.ProfitFactorChanged.Count > 0)
		{
			float num = base.ViewModel.ProfitFactorChanged.Sum((float x) => x);
			string arg = base.ViewModel.LinkGenerate($"{Math.Abs(num)}", "Encyclopedia:ProfitFactor");
			m_NotificationProfitFactorChanged.text = string.Format((num < 0f) ? UINotificationTexts.Instance.LostProfitFactor : UINotificationTexts.Instance.GainedProfitFactor, arg);
			m_NotificationProfitFactorChanged.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationProfitFactorChanged.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
	}

	private void SetFactionReputationChanged()
	{
		List<KeyValuePair<FactionType, int>> list = base.ViewModel.FactionReputationChanged.Where((KeyValuePair<FactionType, int> k) => k.Value > 0).ToList();
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				FactionReputationAppend(list[i], stringBuilder);
			}
			m_NotificationFactionReputationReceived.text = string.Format(UINotificationTexts.Instance.FactionReputationReceivedFormat, stringBuilder);
			m_NotificationFactionReputationReceived.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationFactionReputationReceived.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
		List<KeyValuePair<FactionType, int>> list2 = base.ViewModel.FactionReputationChanged.Where((KeyValuePair<FactionType, int> k) => k.Value < 0).ToList();
		if (list2.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int j = 0; j < list2.Count; j++)
		{
			if (j > 0)
			{
				stringBuilder2.Append(", ");
			}
			FactionReputationAppend(list2[j], stringBuilder2);
		}
		m_NotificationFactionReputationLost.text = string.Format(UINotificationTexts.Instance.FactionReputationLostFormat, stringBuilder2);
		m_NotificationFactionReputationLost.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationFactionReputationLost.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetFactionVendorDiscountChanged()
	{
		List<KeyValuePair<FactionType, int>> list = base.ViewModel.FactionVendorDiscountChanged.Where((KeyValuePair<FactionType, int> k) => k.Value > 0).ToList();
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				FactionVendorDiscountAppend(list[i], stringBuilder, added: true);
			}
			m_NotificationFactionVendorDiscountReceived.text = stringBuilder.ToString();
			m_NotificationFactionVendorDiscountReceived.gameObject.SetActive(value: true);
			m_Disposable.Add(m_NotificationFactionVendorDiscountReceived.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		}
		List<KeyValuePair<FactionType, int>> list2 = base.ViewModel.FactionVendorDiscountChanged.Where((KeyValuePair<FactionType, int> k) => k.Value < 0).ToList();
		if (list2.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int j = 0; j < list2.Count; j++)
		{
			if (j > 0)
			{
				stringBuilder2.Append("<br>");
			}
			FactionVendorDiscountAppend(list2[j], stringBuilder2, added: false);
		}
		m_NotificationFactionVendorDiscountLost.text = stringBuilder2.ToString();
		m_NotificationFactionVendorDiscountLost.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationFactionVendorDiscountLost.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetAbilityAdded()
	{
		List<EntityFact> list = base.ViewModel.AbilityAdded.ToList();
		if (list.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append("<br>");
			}
			AbilityAppend(list[i], stringBuilder, added: true);
		}
		m_NotificationAbilityAdded.text = stringBuilder.ToString();
		m_NotificationAbilityAdded.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationAbilityAdded.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetBuffAdded()
	{
		List<EntityFact> list = base.ViewModel.BuffAdded.ToList();
		if (list.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append("<br>");
			}
			BuffAppend(list[i], stringBuilder, added: true);
		}
		m_NotificationBuffAdded.text = stringBuilder.ToString();
		m_NotificationBuffAdded.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificationBuffAdded.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetSoulMarkShift()
	{
		if (base.ViewModel.SoulMarkShifts.Count <= 0 || !m_NotificatoinSoulMarksShift)
		{
			return;
		}
		string text = "";
		for (int i = 0; i < base.ViewModel.SoulMarkShifts.Count; i++)
		{
			var (soulMarkDirection2, num2) = (KeyValuePair<SoulMarkDirection, int>)(ref base.ViewModel.SoulMarkShifts.ElementAt(i));
			if (i > 0)
			{
				text += ", ";
			}
			string arg = base.ViewModel.LinkGenerate(UIUtility.GetSoulMarkDirectionText(soulMarkDirection2).Text, $"SoulMarkShiftDirection:{soulMarkDirection2}");
			text += string.Format(UINotificationTexts.Instance.SoulMarksShiftFormat, arg, num2);
		}
		m_NotificatoinSoulMarksShift.text = text;
		m_NotificatoinSoulMarksShift.gameObject.SetActive(value: true);
		m_Disposable.Add(m_NotificatoinSoulMarksShift.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	private void SetCustomNotifications()
	{
		_ = base.ViewModel.CustomNotifications.Count;
	}

	private static void SmartAppend(KeyValuePair<string, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<string, int> keyValuePair = pair;
		var (text2, num2) = (KeyValuePair<string, int>)(ref keyValuePair);
		stringBuilder.Append((num2 > 1) ? $"{text2} x{num2}" : text2);
	}

	private void DamageDealtAppend(KeyValuePair<string, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<string, int> keyValuePair = pair;
		keyValuePair.Deconstruct(out var key, out var value);
		string text = key;
		int value2 = value;
		string text2 = base.ViewModel.LinkGenerate($"{Math.Abs(value2)}", "Encyclopedia:Damage");
		stringBuilder.Append(text + " (" + text2 + ")");
	}

	private void FactionReputationAppend(KeyValuePair<FactionType, int> pair, StringBuilder stringBuilder)
	{
		KeyValuePair<FactionType, int> keyValuePair = pair;
		keyValuePair.Deconstruct(out var key, out var value);
		FactionType factionType = key;
		int value2 = value;
		string text = base.ViewModel.LinkGenerate($"{Math.Abs(value2)}", "Encyclopedia:Reputation");
		stringBuilder.Append(UIStrings.Instance.CharacterSheet.GetFactionLabel(factionType) + " : " + text);
	}

	private void FactionVendorDiscountAppend(KeyValuePair<FactionType, int> pair, StringBuilder stringBuilder, bool added)
	{
		KeyValuePair<FactionType, int> keyValuePair = pair;
		keyValuePair.Deconstruct(out var key, out var value);
		FactionType factionType = key;
		int value2 = value;
		string arg = base.ViewModel.LinkGenerate($"{Math.Abs(value2)}", "Encyclopedia:Reputation");
		stringBuilder.Append(string.Format(added ? UINotificationTexts.Instance.FactionVendorDiscountReceivedFormat : UINotificationTexts.Instance.FactionVendorDiscountLostFormat, arg, UIStrings.Instance.CharacterSheet.GetFactionLabel(factionType)));
	}

	private void AbilityAppend(EntityFact ability, StringBuilder stringBuilder, bool added)
	{
		if (ability is Ability || ability is Feature)
		{
			string text = ability.Name;
			IEntity owner = ability.Owner;
			string text2 = ((owner is StarshipEntity starshipEntity) ? starshipEntity.CharacterName : ((!(owner is BaseUnitEntity baseUnitEntity)) ? string.Empty : baseUnitEntity.CharacterName));
			string arg = text2;
			string arg2 = base.ViewModel.LinkGenerate(text ?? "", "f:" + ability.Blueprint.AssetGuid);
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.AbilityAddedFormat, arg2, arg));
		}
	}

	private void BuffAppend(EntityFact buff, StringBuilder stringBuilder, bool added)
	{
		if (buff is Buff buff2)
		{
			string text = buff.Name;
			string arg = (buff.Owner as BaseUnitEntity)?.CharacterName;
			string arg2;
			if (buff2.IsPermanent)
			{
				arg2 = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.CharacterSheet.Permanent.Text;
			}
			else
			{
				string arg3 = ((buff2.ExpirationInRounds == 1) ? Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Round.Text : Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.TurnBasedTexts.Rounds.Text);
				arg2 = $"{buff2.ExpirationInRounds} {arg3}";
			}
			string arg4 = base.ViewModel.LinkGenerate(text ?? "", "f:" + buff.Blueprint.AssetGuid);
			stringBuilder.Append(string.Format(UINotificationTexts.Instance.BuffAddedFormat, arg4, arg, arg2));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetTextFontSize(size);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	public List<TextMeshProUGUI> GetAllNotifications()
	{
		return new List<TextMeshProUGUI>
		{
			m_NotificationPhraseLocations, m_NotificationPhraseItemsReceived, m_NotificationPhraseItemsLost, m_NotificationProfitFactorChanged, m_NotificationXpGained, m_NotificationCargoAdded, m_NotificationCargoLost, m_NotificationDamageDealt, m_NotificationNavigatorResourceAdded, m_NotificationFactionReputationReceived,
			m_NotificationFactionReputationLost, m_NotificationFactionVendorDiscountReceived, m_NotificationFactionVendorDiscountLost, m_NotificationAbilityAdded, m_NotificationBuffAdded, m_NotificatoinSoulMarksShift
		};
	}
}
