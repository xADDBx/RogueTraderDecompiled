using Kingmaker.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsCompanyElement : CreditElement, ICreditsElement
{
	[SerializeField]
	protected TextMeshProUGUI m_Label;

	public void Initialize(string company, ICreditsView view)
	{
		base.gameObject.SetActive(value: true);
		m_Label.text = company;
		base.transform.SetParent(view.Content);
		base.transform.ResetAll();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}

	public void Ping()
	{
	}
}
