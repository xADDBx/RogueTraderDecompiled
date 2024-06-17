using System.Collections;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Console.XBox;

public class EngagementScreen : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_Logo;

	[SerializeField]
	[UsedImplicitly]
	private Image m_Background;

	private bool m_Active = true;

	public void Show()
	{
		Object.DontDestroyOnLoad(this);
		m_Logo.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
		StartCoroutine(ShowAndHide());
	}

	private IEnumerator ShowAndHide()
	{
		LoadingProcess.Instance.StartLoadingProcess(DoNothing(), null, LoadingProcessTag.Save);
		Show(state: true);
		yield return new WaitForSecondsRealtime(0.5f);
		Object.Destroy(GetComponentInChildren<TextMeshProUGUI>());
		yield return null;
		Show(state: false);
	}

	private IEnumerator DoNothing()
	{
		while (m_Active)
		{
			yield return null;
		}
	}

	private void Show(bool state)
	{
		if (state)
		{
			m_Background.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
			m_Logo.DOFade(1f, 0.5f).SetUpdate(isIndependentUpdate: true).SetEase(Ease.InQuint);
			return;
		}
		m_Background.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true).SetEase(Ease.InQuart);
		m_Logo.DOFade(0f, 0.5f).SetUpdate(isIndependentUpdate: true).SetEase(Ease.InExpo)
			.OnComplete(delegate
			{
				Object.Destroy(base.gameObject);
			});
		m_Active = false;
	}
}
