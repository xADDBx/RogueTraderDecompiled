using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class FlexibleLensSelectorView : ViewBase<LensSelectorVM>
{
	[SerializeField]
	private OwlcatMultiButton[] m_Buttons;

	[SerializeField]
	private OwlcatButton[] m_OwlcatButtons;

	[SerializeField]
	private RectTransform m_Lens;

	[SerializeField]
	private float m_LensSwitchAnimationDuration = 0.75f;

	[SerializeField]
	private float m_Offset;

	[Header("Fake double buttons. Can be none")]
	[SerializeField]
	private OwlcatButton m_FakeButton;

	[SerializeField]
	private OwlcatMultiButton m_FakeSelectingButton;

	private int m_CurrentTabIndex;

	protected override void BindViewImplementation()
	{
		if ((m_Buttons.Length != 0 || m_OwlcatButtons.Length != 0) && !(m_Lens == null))
		{
			TryAddLensPositions();
			if (base.ViewModel.NeedToResetPosition)
			{
				ResetSelectorPosition();
			}
		}
	}

	public void ForceFocus(RectTransform buttonTransform)
	{
		SetLensPosition(buttonTransform);
	}

	private void ResetSelectorPosition()
	{
		if (m_Buttons.Length != 0)
		{
			SetLensPosition(m_Buttons[0].transform as RectTransform, withSound: false);
		}
		else
		{
			SetLensPosition(m_OwlcatButtons[0].transform as RectTransform, withSound: false);
		}
	}

	private void TryAddLensPositions()
	{
		if (m_Buttons.Length != 0)
		{
			m_Buttons.ForEach(delegate(OwlcatMultiButton b)
			{
				AddDisposable(ObservableExtensions.Subscribe(b.OnLeftClickAsObservable(), delegate
				{
					SetLensPosition(b.transform as RectTransform);
				}));
			});
		}
		else
		{
			m_OwlcatButtons.ForEach(delegate(OwlcatButton b)
			{
				AddDisposable(ObservableExtensions.Subscribe(b.OnLeftClickAsObservable(), delegate
				{
					SetLensPosition(b.transform as RectTransform);
				}));
			});
		}
		if ((bool)m_FakeButton && (bool)m_FakeSelectingButton)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_FakeButton.OnLeftClickAsObservable(), delegate
			{
				SetLensPosition(m_FakeSelectingButton.transform as RectTransform);
			}));
		}
	}

	private void SetLensPosition(RectTransform buttonTransform, bool withSound = true)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			Vector3 localPosition = buttonTransform.localPosition;
			UIUtility.MoveLensPosition(target: new Vector3(localPosition.x - m_Offset, localPosition.y, localPosition.z), lens: m_Lens, duration: m_LensSwitchAnimationDuration, withSound: withSound);
			if (m_Buttons.Length != 0)
			{
				OwlcatMultiButton[] buttons = m_Buttons;
				foreach (OwlcatMultiButton owlcatMultiButton in buttons)
				{
					if (!(owlcatMultiButton.transform as RectTransform != buttonTransform))
					{
						m_CurrentTabIndex = m_Buttons.IndexOf(owlcatMultiButton);
						break;
					}
				}
			}
			else
			{
				OwlcatButton[] owlcatButtons = m_OwlcatButtons;
				foreach (OwlcatButton owlcatButton in owlcatButtons)
				{
					if (!(owlcatButton.transform as RectTransform != buttonTransform))
					{
						m_CurrentTabIndex = m_OwlcatButtons.IndexOf(owlcatButton);
						break;
					}
				}
			}
		}, 1);
	}

	public void ChangeTab(int index, bool withSound = true)
	{
		if (m_Buttons.Length != 0)
		{
			if (index <= m_Buttons.Length - 1 && index >= 0)
			{
				RectTransform buttonTransform = m_Buttons[index].transform as RectTransform;
				SetLensPosition(buttonTransform, withSound);
			}
		}
		else if (index <= m_OwlcatButtons.Length - 1 && index >= 0)
		{
			RectTransform buttonTransform2 = m_OwlcatButtons[index].transform as RectTransform;
			SetLensPosition(buttonTransform2, withSound);
		}
	}

	public void SetNextTab()
	{
		if (m_Buttons.Length != 0)
		{
			int num = ((m_CurrentTabIndex + 1 < m_Buttons.Length) ? (m_CurrentTabIndex + 1) : 0);
			RectTransform buttonTransform = m_Buttons[num].transform as RectTransform;
			SetLensPosition(buttonTransform);
		}
		else
		{
			int num2 = ((m_CurrentTabIndex + 1 < m_OwlcatButtons.Length) ? (m_CurrentTabIndex + 1) : 0);
			RectTransform buttonTransform2 = m_OwlcatButtons[num2].transform as RectTransform;
			SetLensPosition(buttonTransform2);
		}
	}

	public void SetPrevTab()
	{
		if (m_Buttons.Length != 0)
		{
			int num = ((m_CurrentTabIndex == 0) ? (m_Buttons.Length - 1) : (m_CurrentTabIndex - 1));
			RectTransform buttonTransform = m_Buttons[num].transform as RectTransform;
			SetLensPosition(buttonTransform);
		}
		else
		{
			int num2 = ((m_CurrentTabIndex == 0) ? (m_OwlcatButtons.Length - 1) : (m_CurrentTabIndex - 1));
			RectTransform buttonTransform2 = m_OwlcatButtons[num2].transform as RectTransform;
			SetLensPosition(buttonTransform2);
		}
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		UISounds.Instance.Sounds.Selector.SelectorLoopStop.Play();
		m_CurrentTabIndex = 0;
	}
}
