using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space;

public class ShipPositionRulersView : ViewBase<ShipPositionRulersVM>
{
	[SerializeField]
	private RectTransform m_HorizontalShipPosition;

	[SerializeField]
	private RectTransform m_VerticalShipPosition;

	[SerializeField]
	private RectTransform m_HorizontalLine;

	[SerializeField]
	private RectTransform m_VerticalLine;

	private RectTransform m_ParentRect;

	protected override void BindViewImplementation()
	{
		m_ParentRect = (RectTransform)base.transform;
		AddDisposable(base.ViewModel.IsSystemMap.Subscribe(base.gameObject.SetActive));
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			SetShipRulerPositions(CameraRig.Instance.WorldToViewport((Game.Instance.StarSystemMapController.StarSystemShip == null) ? Vector3.zero : Game.Instance.StarSystemMapController.StarSystemShip.ViewPosition));
		}));
	}

	private void SetShipRulerPositions(Vector3 pos)
	{
		if (base.ViewModel.IsSystemMap.Value)
		{
			float xPos = pos.x * m_ParentRect.rect.width;
			float yPos = pos.y * m_ParentRect.rect.height;
			m_HorizontalShipPosition.anchoredPosition = new Vector2(SetXPos(xPos), 0f);
			m_VerticalShipPosition.anchoredPosition = new Vector2(0f, SetYPos(yPos));
		}
	}

	private float SetXPos(float x)
	{
		return Mathf.Clamp(x, m_HorizontalLine.offsetMin.x, m_HorizontalLine.offsetMin.x + m_HorizontalLine.rect.width - m_HorizontalShipPosition.rect.width);
	}

	private float SetYPos(float y)
	{
		return Mathf.Clamp(y, m_VerticalLine.offsetMin.y, m_VerticalLine.offsetMin.y + m_VerticalLine.rect.height - m_VerticalShipPosition.rect.height);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
