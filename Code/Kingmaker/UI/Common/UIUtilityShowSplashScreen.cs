using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Legacy.MainMenuUI;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class UIUtilityShowSplashScreen
{
	public static void ShowSplashScreen(SplashScreenController.ScreenUnit su, Sequence sequence, GameObject soundGameObject, CanvasGroup additionalCanvasGroup = null)
	{
		CanvasGroup cg = su.CanvasGroup;
		PFLog.UI.Log("SplashScreenController: Activating " + cg.name);
		cg.gameObject.SetActive(value: true);
		cg.alpha = 0f;
		sequence.AppendCallback(delegate
		{
		});
		sequence.Append(cg.DOFade(1f, su.InTime).SetEase(su.Ease).OnStart(delegate
		{
			if (su.VideoPlayer != null)
			{
				PFLog.System.Log("SplashScreenController: Playing " + cg.name);
				su.VideoPlayer.Play();
			}
			if (!FirstLaunchSettingsVM.HasShown && su.FirstLaunchAnotherSoundEvent && !string.IsNullOrEmpty(su.AKFirstLaunchSoundEvent))
			{
				su.FirstLaunchSoundId = SoundEventsManager.PostEvent(su.AKFirstLaunchSoundEvent, soundGameObject, canBeStopped: true);
				PFLog.System.Log($"Play splash screen audio event={su.AKFirstLaunchSoundEvent}, soundId={su.FirstLaunchSoundId}");
			}
			else if (!string.IsNullOrEmpty(su.AKSoundEvent))
			{
				su.SoundId = SoundEventsManager.PostEvent(su.AKSoundEvent, soundGameObject, canBeStopped: true);
				PFLog.System.Log($"Play splash screen audio event={su.AKSoundEvent}, soundId={su.SoundId}");
			}
		}));
		if (additionalCanvasGroup != null)
		{
			sequence.Join(additionalCanvasGroup.DOFade(1f, su.InTime).SetEase(su.Ease));
		}
		sequence.AppendInterval(su.DelayTime);
		sequence.Append(cg.DOFade(0f, su.OutTime).SetEase(su.Ease));
		sequence.AppendCallback(delegate
		{
			if (su.VideoPlayer != null)
			{
				PFLog.System.Log("SplashScreenController: Stopping " + cg.name);
				su.VideoPlayer.Stop();
			}
			su.SoundId = 0u;
			su.FirstLaunchSoundId = 0u;
			Object.Destroy(cg.gameObject);
		});
	}
}
