using DG.Tweening;
using Kingmaker.Utility.GameConst;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.WarningNotification.PC;

public class WarningsTextPCView : WarningsTextView
{
	[Header("PC Part")]
	[SerializeField]
	private RectTransform m_MoveTransform;

	[SerializeField]
	private float m_MoveShift = 150f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsEtudeCounterShowing.Skip(1).Subscribe(PlayAnimationMove));
	}

	private void PlayAnimationMove(bool direct)
	{
		m_MoveTransform.DOLocalMoveY(direct ? m_MoveShift : 0f, UIConsts.BarkMoveTime);
	}
}
