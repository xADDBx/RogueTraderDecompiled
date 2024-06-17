using System;
using System.Text;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.MessageBox;

public abstract class MessageBoxBaseView : ViewBase<MessageBoxVM>
{
	[SerializeField]
	private TextMeshProUGUI m_MessageText;

	[SerializeField]
	private TMPLinkHandler m_LinkHandler;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		m_MessageText.text = base.ViewModel.MessageText;
		AddDisposable(base.ViewModel.WaitTime.Subscribe(delegate(int value)
		{
			StringBuilder stringBuilder = new StringBuilder(base.ViewModel.AcceptText);
			if (value > 0)
			{
				stringBuilder.Append($" ({value})");
			}
			SetAcceptText(stringBuilder.ToString());
		}));
		SetDeclineText(base.ViewModel.DeclineText);
		AddDisposable(base.ViewModel.InputText.CombineLatest(base.ViewModel.WaitTime, delegate(string s, int i)
		{
			bool flag = i == 0;
			if (flag && base.ViewModel.BoxType == DialogMessageBoxBase.BoxType.TextField)
			{
				flag = base.ViewModel.InputText.Value.Length > 0;
			}
			return flag;
		}).Subscribe(SetAcceptInteractable));
		AddDisposable(m_LinkHandler.OnClickAsObservable().Subscribe(delegate(Tuple<PointerEventData, TMP_LinkInfo> value)
		{
			base.ViewModel.OnLinkInvoke(value.Item2);
		}));
		BindTextField();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.OnDeclinePressed();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		DestroyTextField();
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		base.gameObject.SetActive(value: false);
	}

	protected virtual void OnTextInputChanged(string value)
	{
		base.ViewModel.InputText.Value = value;
	}

	protected abstract void SetAcceptInteractable(bool interactable);

	protected abstract void SetAcceptText(string acceptText);

	protected abstract void SetDeclineText(string declineText);

	protected abstract void BindTextField();

	protected abstract void DestroyTextField();
}
