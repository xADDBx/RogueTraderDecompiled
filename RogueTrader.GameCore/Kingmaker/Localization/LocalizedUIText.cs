using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.Stores.DlcInterfaces;
using TMPro;
using UnityEngine;

namespace Kingmaker.Localization;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedUIText : MonoBehaviour
{
	[Serializable]
	public class DlcText
	{
		[SerializeField]
		private BlueprintScriptableObjectReference m_Dlc;

		[StringCreateTemplate(StringCreateTemplateAttribute.StringType.UIText)]
		public SharedStringAsset Text;

		public IBlueprintDlc Dlc => m_Dlc?.Get() as IBlueprintDlc;
	}

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.UIText)]
	public SharedStringAsset Text;

	public bool IsSaber;

	[SerializeField]
	private List<DlcText> DlcTexts;

	private void Awake()
	{
		LocalizationManager.Instance.LocaleChanged += PackChangedEventHandler;
		UpdateText();
	}

	private void OnDestroy()
	{
		LocalizationManager.Instance.LocaleChanged -= PackChangedEventHandler;
	}

	private void PackChangedEventHandler(Locale _)
	{
		UpdateText();
	}

	public void UpdateText()
	{
		if (Text == null)
		{
			return;
		}
		TMP_Text component = GetComponent<TMP_Text>();
		if (component == null)
		{
			return;
		}
		LocalizedString @string = Text.String;
		foreach (DlcText dlcText in DlcTexts)
		{
			if ((dlcText?.Dlc?.IsAvailable).GetValueOrDefault())
			{
				@string = dlcText.Text.String;
				break;
			}
		}
		component.text = @string;
	}
}
