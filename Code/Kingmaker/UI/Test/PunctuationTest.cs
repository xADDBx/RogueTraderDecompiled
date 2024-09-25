using System.Collections.Generic;
using Kingmaker.Localization.Enums;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.Test;

public class PunctuationTest : MonoBehaviour
{
	[SerializeField]
	private List<TextMeshProUGUI> Fonts;

	[SerializeField]
	private bool ShowSpecialSymbols;

	[SerializeField]
	private Locale Locale;

	public void OnEnable()
	{
		UpdateText();
	}

	public void Start()
	{
		UpdateText();
	}

	public void UpdateText()
	{
	}
}
