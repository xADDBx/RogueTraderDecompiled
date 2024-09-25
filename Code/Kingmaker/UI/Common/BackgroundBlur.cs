using Kingmaker.Visual;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class BackgroundBlur : MonoBehaviour
{
	[SerializeField]
	private float m_BlurSize = 3f;

	public float BlurSize => m_BlurSize;

	protected virtual void OnEnable()
	{
		SetBlurState(state: true);
	}

	protected virtual void OnDisable()
	{
		SetBlurState(state: false);
	}

	protected void SetBlurState(bool state)
	{
		if (!(BackgroundBlurForUI.Instance == null))
		{
			if (state)
			{
				BackgroundBlurForUI.Instance.AddBlurComponent(this);
			}
			else
			{
				BackgroundBlurForUI.Instance.RemoveBlurComponent(this);
			}
		}
	}
}
