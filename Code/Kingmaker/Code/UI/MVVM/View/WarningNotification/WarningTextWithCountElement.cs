using System;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.WarningNotification;

[Serializable]
public class WarningTextWithCountElement : WarningTextElement
{
	[SerializeField]
	private TextMeshProUGUI m_Count;

	public void SetText(string label, string counter)
	{
		SetText(label);
		m_Count.SetText(counter);
	}
}
