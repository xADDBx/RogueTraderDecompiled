using DG.Tweening;
using Kingmaker.UI.MVVM.VM.CircleArc;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMap;

public class CircleArcsView : ViewBase<BaseCircleArcsVM>
{
	public bool HasRandomizeImages;

	[ConditionalShow("HasRandomizeImages")]
	[SerializeField]
	private RandomizeImages m_RandomizeImages;

	public bool AnimateShadows;

	[ConditionalShow("AnimateShadows")]
	[SerializeField]
	private CanvasGroup m_Shadows;

	[Header("Circle Arts")]
	[SerializeField]
	private CircleArtRotation m_CircleArtRotation;

	[SerializeField]
	private bool m_WithSound;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsCorrectGameMode.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.ShouldMoveArcs.Subscribe(MoveArcs));
		if (HasRandomizeImages)
		{
			SetRandomImages();
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetRandomImages()
	{
		m_RandomizeImages.LeftTopImage.sprite = m_RandomizeImages.LeftTopSprites[Random.Range(0, m_RandomizeImages.LeftTopSprites.Length)];
		m_RandomizeImages.LeftBottomImage.sprite = m_RandomizeImages.LeftBottomSprites[Random.Range(0, m_RandomizeImages.LeftBottomSprites.Length)];
		m_RandomizeImages.RightBottomImage.sprite = m_RandomizeImages.RightBottomSprites[Random.Range(0, m_RandomizeImages.RightBottomSprites.Length)];
	}

	private void MoveArcs(bool move)
	{
		RectTransform circleArt = m_CircleArtRotation.CircleArt;
		float duration = (move ? m_CircleArtRotation.CircleArtAnimationDuration : m_CircleArtRotation.CircleArtCloseAnimationDuration);
		if (m_WithSound)
		{
			if (move)
			{
				UISounds.Instance.Play(UISounds.Instance.Sounds.SpaceExploration.CircleArcsShow, isButton: false, playAnyway: true);
			}
			else
			{
				UISounds.Instance.Sounds.SpaceExploration.CircleArcsHide.Play();
			}
		}
		circleArt.DOKill();
		circleArt.DOScale(move ? m_CircleArtRotation.CircleArtMoveScale : m_CircleArtRotation.CircleArtDefaultScale, duration);
		circleArt.DORotate(move ? new Vector3(0f, 0f, Random.Range(m_CircleArtRotation.CircleArtMinRotation, m_CircleArtRotation.CircleArtMaxRotation) * (float)((Random.Range(0, 2) != 0) ? 1 : (-1))) : Vector3.zero, duration);
		if (AnimateShadows)
		{
			m_Shadows.DOFade(move ? 1f : 0f, duration);
		}
	}
}
