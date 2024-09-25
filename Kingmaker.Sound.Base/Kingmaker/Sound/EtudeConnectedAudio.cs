using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

public class EtudeConnectedAudio : RegisteredBehaviour
{
	[SerializeField]
	private AkBankReference m_LoadBank;

	public void Toggle(bool on)
	{
		if (on)
		{
			m_LoadBank?.Load();
		}
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(on);
		}
		if (!on)
		{
			m_LoadBank?.Unload();
		}
	}
}
