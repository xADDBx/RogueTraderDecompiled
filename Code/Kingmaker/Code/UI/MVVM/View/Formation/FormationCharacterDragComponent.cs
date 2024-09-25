using Kingmaker.UI.InputSystems;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Formation;

public class FormationCharacterDragComponent : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	public const int OneStepSize = 23;

	public const float OneStepSizeConsole = 23f;

	public const int Scaler = 40;

	private RectTransform m_ParentArea;

	private RectTransform m_RectTransform;

	public bool IsInteractable;

	public readonly ReactiveCommand DragCommand = new ReactiveCommand();

	private Vector2 m_MinPosition;

	private Vector2 m_MaxPosition;

	public void Initialize(RectTransform parentArea)
	{
		m_ParentArea = parentArea;
		m_RectTransform = base.transform as RectTransform;
		m_MinPosition = m_ParentArea.rect.min - m_RectTransform.rect.min;
		m_MaxPosition = m_ParentArea.rect.max - m_RectTransform.rect.max;
	}

	public void OnDrag(PointerEventData data)
	{
		if (!(m_ParentArea == null) && IsInteractable)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentArea, data.position, data.pressEventCamera, out var localPoint))
			{
				SetTargetLocalPosition(localPoint);
			}
			ClampToWindow();
			DragCommand.Execute();
		}
	}

	private void SetTargetLocalPosition(Vector2 localPointerPosition)
	{
		if (KeyboardAccess.IsShiftHold())
		{
			base.transform.localPosition = localPointerPosition;
			return;
		}
		Vector2 vector = localPointerPosition;
		vector.x -= vector.x % 23f;
		vector.y -= vector.y % 23f;
		base.transform.localPosition = vector;
	}

	private void ClampToWindow()
	{
		Vector3 localPosition = m_RectTransform.localPosition;
		localPosition.x = Mathf.Clamp(localPosition.x, m_MinPosition.x, m_MaxPosition.x);
		localPosition.y = Mathf.Clamp(localPosition.y, m_MinPosition.y, m_MaxPosition.y);
		m_RectTransform.localPosition = localPosition;
	}

	public Vector2 ClampPosition(Vector2 vec)
	{
		return new Vector2(Mathf.Clamp(vec.x, m_MinPosition.x, m_MaxPosition.x), Mathf.Clamp(vec.y, m_MinPosition.y, m_MaxPosition.y));
	}
}
