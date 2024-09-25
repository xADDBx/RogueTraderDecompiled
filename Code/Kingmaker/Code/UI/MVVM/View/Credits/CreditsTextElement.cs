using Kingmaker.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsTextElement : CreditElement, ICreditsElement
{
	[SerializeField]
	protected TextMeshProUGUI m_MessageLabel;

	public void Initialize(string text, ICreditsView view)
	{
		base.gameObject.SetActive(value: true);
		m_MessageLabel.text = text;
		base.transform.SetParent(view.Content);
		base.transform.ResetAll();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	public void Ping()
	{
	}
}
