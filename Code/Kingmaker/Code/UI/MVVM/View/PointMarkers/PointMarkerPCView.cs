using System;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using Kingmaker.View;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.PointMarkers;

[RequireComponent(typeof(CanvasGroup))]
public class PointMarkerPCView : ViewBase<PointMarkerVM>
{
	[Header("General")]
	[SerializeField]
	private OwlcatButton m_MainButton;

	[SerializeField]
	private RectTransform m_ScaledGroup;

	[SerializeField]
	protected Image m_Portrait;

	[SerializeField]
	protected Image m_SubtypeIcon;

	[SerializeField]
	protected Image m_Frame;

	[SerializeField]
	protected Image m_Arrow;

	[SerializeField]
	private RectTransform m_ArrowTransform;

	[SerializeField]
	private PointMarkerLOSView m_LOSView;

	[Header("Visual Settings")]
	[SerializeField]
	protected PointMarkerRelationParams[] m_ParamsArray;

	[Header("Timings")]
	[SerializeField]
	private float m_AppearanceTime = 0.1f;

	[SerializeField]
	private float m_MarkerJumpTime = 0.1f;

	private PointMarkersPCView m_ParentView;

	private CanvasGroup m_CanvasGroup;

	private Tweener m_AnimationTween;

	private LineSegment2 m_UnitLine = new LineSegment2
	{
		PointA = Vector3.zero,
		PointB = Vector3.zero
	};

	private Vector3 m_RigTargetPosition;

	public void Initialize(PointMarkersPCView parentView)
	{
		m_ParentView = parentView;
		m_CanvasGroup = GetComponent<CanvasGroup>();
		if (m_LOSView != null)
		{
			m_LOSView.Initialize();
		}
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Portrait.Subscribe(SetPortrait));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(SetVisibility));
		AddDisposable(base.ViewModel.Relation.Subscribe(delegate(UnitRelation r)
		{
			if (base.ViewModel.Unit != null)
			{
				ApplyRelationParams(GetRelationParams(r));
			}
		}));
		AddDisposable(base.ViewModel.AnotherPointMarkObjectType.Subscribe(delegate(EntityPointMarkObjectType r)
		{
			if (base.ViewModel.Unit == null)
			{
				ApplyRelationParams(GetAnotherEntityTypeParams(r));
			}
		}));
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleClick();
		}));
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			HandleUpdate();
		}));
		if ((bool)m_LOSView)
		{
			AddDisposable(base.ViewModel.LineOfSight.Subscribe(m_LOSView.Bind));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	private PointMarkerRelationParams GetRelationParams(UnitRelation relation)
	{
		return m_ParamsArray.First((PointMarkerRelationParams item) => item.Relation == relation);
	}

	private PointMarkerRelationParams GetAnotherEntityTypeParams(EntityPointMarkObjectType type)
	{
		return m_ParamsArray.First((PointMarkerRelationParams item) => item.IsAnotherEntity && item.EntityPointMarkObjectType == type);
	}

	protected virtual void ApplyRelationParams(PointMarkerRelationParams relationParams)
	{
		if (relationParams != null)
		{
			SetPortrait(relationParams.IsAnotherEntity ? relationParams.Icon : base.ViewModel.Portrait.Value);
			m_Frame.color = relationParams.FrameColor;
			if (!relationParams.IsAnotherEntity)
			{
				m_Arrow.color = relationParams.FrameColor;
			}
			m_SubtypeIcon.color = relationParams.IconColor;
			m_ScaledGroup.localScale = new Vector3(relationParams.Scale, relationParams.Scale, 0f);
		}
	}

	protected virtual void HandleClick()
	{
		base.ViewModel.ScrollToUnit();
	}

	protected virtual void HandleUpdate()
	{
		if (base.ViewModel.IsVisible.Value)
		{
			CalculateMarkerPosition(smoothIgnore: false);
		}
	}

	protected virtual void SetPortrait(Sprite portrait)
	{
		m_Portrait.gameObject.SetActive(!base.ViewModel.UsedSubtypeIcons);
		m_SubtypeIcon.gameObject.SetActive(base.ViewModel.UsedSubtypeIcons);
		if (base.ViewModel.UsedSubtypeIcons)
		{
			m_SubtypeIcon.sprite = portrait;
		}
		else
		{
			m_Portrait.sprite = portrait;
		}
	}

	protected virtual void SetVisibility(bool isVisible)
	{
		m_AnimationTween?.Complete();
		if (isVisible)
		{
			CalculateMarkerPosition(smoothIgnore: true);
			base.gameObject.SetActive(value: true);
			m_AnimationTween = m_CanvasGroup.DOFade(1f, m_AppearanceTime).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_AnimationTween = m_CanvasGroup.DOFade(0f, m_AppearanceTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
		}
	}

	private void CalculateMarkerPosition(bool smoothIgnore)
	{
		CameraRig instance = CameraRig.Instance;
		m_UnitLine.PointA.x = (float)Screen.width / 2f;
		m_UnitLine.PointA.y = (float)Screen.height / 2f;
		m_UnitLine.PointA.z = 0f;
		m_UnitLine.PointB = instance.Camera.WorldToScreenPoint(base.ViewModel.Position);
		m_RigTargetPosition = instance.GetTargetPointPosition();
		Vector3 vector = instance.Camera.transform.position - m_RigTargetPosition;
		if (m_UnitLine.PointB.z <= 0f)
		{
			Vector3 vector2 = instance.Camera.WorldToScreenPoint(m_RigTargetPosition);
			m_UnitLine.PointB = instance.Camera.WorldToScreenPoint(base.ViewModel.Position + (m_UnitLine.PointB.z - 1f) * vector / vector2.z);
		}
		float num = float.MaxValue;
		float num2 = 0f;
		Vector2 vector3 = Vector2.zero;
		bool flag = false;
		foreach (LineSegment2 border in m_ParentView.Borders)
		{
			if (IsIntersecting(m_UnitLine, border))
			{
				Vector2 intersection = GetIntersection(m_UnitLine, border);
				Vector2 vector4 = new Vector2(intersection.x / m_ParentView.ScreenScale - m_ParentView.RectTransform.rect.width / 2f, intersection.y / m_ParentView.ScreenScale - m_ParentView.RectTransform.rect.height / 2f);
				float num3 = Mathf.Pow(Mathf.Pow(vector4.x, 2f) + Mathf.Pow(vector4.y, 2f), 0.5f);
				if (!(num3 >= num))
				{
					vector3 = vector4;
					num2 = Mathf.Atan2(vector3.x, vector3.y) * 57.29578f;
					num = num3;
					flag = true;
				}
			}
		}
		if (flag)
		{
			Vector2 vector5 = new Vector2(vector3.x + Mathf.Cos((-90f - num2) * (MathF.PI / 180f)) * m_ArrowTransform.rect.height, vector3.y + Mathf.Sin((-90f - num2) * (MathF.PI / 180f)) * m_ArrowTransform.rect.height);
			base.transform.localPosition = vector5;
			m_ArrowTransform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles.x, base.transform.rotation.eulerAngles.y, 0f - num2);
		}
	}

	private static bool IsIntersecting(LineSegment2 segment1, LineSegment2 segment2)
	{
		Vector2 vector = new Vector2(segment1.PointA.x, segment1.PointA.y);
		Vector2 vector2 = new Vector2(segment1.PointB.x, segment1.PointB.y);
		Vector2 vector3 = new Vector2(segment2.PointA.x, segment2.PointA.y);
		Vector2 vector4 = new Vector2(segment2.PointB.x, segment2.PointB.y);
		Vector2 vector5 = vector2 - vector;
		Vector2 vector6 = vector3 - vector4;
		Vector2 vector7 = vector - vector3;
		float num = vector6.y * vector7.x - vector6.x * vector7.y;
		float num2 = vector5.y * vector6.x - vector5.x * vector6.y;
		float num3 = vector5.x * vector7.y - vector5.y * vector7.x;
		float num4 = vector5.y * vector6.x - vector5.x * vector6.y;
		bool result = true;
		if (Math.Abs(num2) < Mathf.Epsilon || Math.Abs(num4) < Mathf.Epsilon)
		{
			result = false;
		}
		else
		{
			if (num2 > 0f)
			{
				if (num < 0f || num > num2)
				{
					result = false;
				}
			}
			else if (num > 0f || num < num2)
			{
				result = false;
			}
			if (num4 > 0f)
			{
				if (num3 < 0f || num3 > num4)
				{
					result = false;
				}
			}
			else if (num3 > 0f || num3 < num4)
			{
				result = false;
			}
		}
		return result;
	}

	private static Vector2 GetIntersection(LineSegment2 segment1, LineSegment2 segment2)
	{
		Vector2 vector = new Vector2(segment1.PointA.x, segment1.PointA.y);
		Vector2 vector2 = new Vector2(segment1.PointB.x, segment1.PointB.y);
		Vector2 vector3 = new Vector2(segment2.PointA.x, segment2.PointA.y);
		Vector2 vector4 = new Vector2(segment2.PointB.x, segment2.PointB.y);
		float num = vector2.y - vector.y;
		float num2 = vector.x - vector2.x;
		float num3 = num * vector.x + num2 * vector.y;
		float num4 = vector4.y - vector3.y;
		float num5 = vector3.x - vector4.x;
		float num6 = num4 * vector3.x + num5 * vector3.y;
		float num7 = num * num5 - num4 * num2;
		float x = (num5 * num3 - num2 * num6) / num7;
		float y = (num * num6 - num4 * num3) / num7;
		return new Vector2(x, y);
	}
}
