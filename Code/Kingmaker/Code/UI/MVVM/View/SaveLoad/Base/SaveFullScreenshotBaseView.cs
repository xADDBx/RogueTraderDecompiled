using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

public class SaveFullScreenshotBaseView : ViewBase<SaveSlotVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private RawImage m_ScreenshotImage;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		m_FadeAnimator.AppearAnimation();
		AddDisposable(base.ViewModel.ScreenShotHighRes.Subscribe(SetScreenshot));
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator.DisappearAnimation();
		base.ViewModel.DisposeHighResScreenshot();
	}

	private void SetScreenshot(Texture2D screenshot)
	{
		if (screenshot != null && screenshot.width == 4 && screenshot.height == 4)
		{
			screenshot = null;
		}
		m_ScreenshotImage.gameObject.SetActive(screenshot != null);
		if (screenshot != null)
		{
			m_ScreenshotImage.texture = screenshot;
			m_ScreenshotImage.GetComponent<AspectRatioFitter>().aspectRatio = screenshot.GetAspect();
		}
	}
}
