using System.Collections;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMapNotification.Base;

public abstract class BaseSystemMapNotificationView<TViewModel> : ViewBase<TViewModel> where TViewModel : SystemMapNotificationBaseVM
{
	[Header("Common Part")]
	[SerializeField]
	protected TextMeshProUGUI m_Status;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	protected WindowAnimator m_Animator;

	public void Initialize()
	{
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ShowNotificationCommand.Subscribe(delegate
		{
			ShowNotification();
		}));
		AddDisposable(base.ViewModel.IsShowUp.Subscribe(delegate(bool value)
		{
			if (!value)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				UISounds.Instance.Sounds.Journal.NewQuest.Play();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void ShowNotification()
	{
		ShowNotificationImpl();
		ShowNextNotification();
	}

	protected virtual void ShowNotificationImpl()
	{
	}

	private void ShowNextNotification()
	{
		base.ViewModel.IsShowUp.Value = true;
		m_Animator.AppearAnimation(delegate
		{
			StartCoroutine(HideCurrentNotification());
		});
	}

	private IEnumerator HideCurrentNotification()
	{
		yield return new WaitForSecondsRealtime(UIConsts.QuestNotificationTime);
		Hide();
	}

	protected void Hide()
	{
		m_Animator.DisappearAnimation(delegate
		{
			base.ViewModel.IsShowUp.Value = false;
		});
	}
}
