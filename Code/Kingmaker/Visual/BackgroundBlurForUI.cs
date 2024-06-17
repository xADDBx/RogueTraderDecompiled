using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur;
using UnityEngine;

namespace Kingmaker.Visual;

[RequireComponent(typeof(Camera))]
public class BackgroundBlurForUI : MonoBehaviour
{
	public FullscreenBlurFeature BlurFeature;

	private readonly HashSet<BackgroundBlur> m_BlurObjects = new HashSet<BackgroundBlur>();

	public static BackgroundBlurForUI Instance { get; private set; }

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		if (!(BlurFeature == null))
		{
			BlurFeature.BlurSize = 0f;
		}
	}

	public void AddBlurComponent(BackgroundBlur blur)
	{
		m_BlurObjects.Add(blur);
		UpdateBlurState();
	}

	public void RemoveBlurComponent(BackgroundBlur blur)
	{
		m_BlurObjects.Remove(blur);
		UpdateBlurState();
	}

	public void UpdateBlurState()
	{
		if (BlurFeature == null)
		{
			return;
		}
		if (m_BlurObjects.Count == 0)
		{
			BlurFeature.BlurSize = 0f;
			return;
		}
		BackgroundBlur backgroundBlur = m_BlurObjects.ElementAtOrDefault(0);
		if (backgroundBlur == null || !backgroundBlur.gameObject.activeSelf)
		{
			m_BlurObjects.Remove(backgroundBlur);
			UpdateBlurState();
		}
		else
		{
			BlurFeature.BlurSize = backgroundBlur.BlurSize;
		}
	}
}
