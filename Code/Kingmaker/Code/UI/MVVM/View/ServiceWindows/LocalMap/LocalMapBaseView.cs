using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.LocalMapLegendBlock;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Kingmaker.Visual.LocalMap;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap;

public class LocalMapBaseView : ViewBase<LocalMapVM>
{
	private class PingData
	{
		public IDisposable PingDelay { get; set; }
	}

	[Header("Common Block")]
	[SerializeField]
	private ScrambledTMP m_Title;

	[Header("Map Block")]
	[SerializeField]
	protected RawImage m_Image;

	[SerializeField]
	protected RectTransform m_FrameBlock;

	[SerializeField]
	private RectTransform m_Frame;

	[SerializeField]
	private RectTransform m_LittleSkullCamera;

	public Vector2 MaxSize = new Vector2(1640f, 677f);

	private Vector2 m_ChangedMapSize;

	[Header("Markers Block")]
	[SerializeField]
	private List<LocalMapMarkerSet> m_MarkerSets = new List<LocalMapMarkerSet>();

	[SerializeField]
	private TextMeshProUGUI m_TitleLabelMap;

	[Header("Right Buttons")]
	[SerializeField]
	private OwlcatButton m_MapHistoryButton;

	[SerializeField]
	private LocalMapLegendBlockView m_LegendBlockView;

	[SerializeField]
	private RectTransform m_MapHistoryLittleSquare;

	[SerializeField]
	protected float m_ZoomStep;

	[SerializeField]
	protected float m_ZoomMaxSize;

	[SerializeField]
	protected float m_ZoomMinSize;

	[SerializeField]
	private float m_MoveMapSpeed;

	[SerializeField]
	private float m_MoveMapFrame;

	[SerializeField]
	private RectTransform m_MapBlock;

	[SerializeField]
	private RectTransform[] m_MarkersNeedToShowAlways;

	private bool m_SaveMarkerCoords;

	private Vector2 m_Size;

	protected readonly BoolReactiveProperty MaxZoom = new BoolReactiveProperty();

	protected readonly BoolReactiveProperty MinZoom = new BoolReactiveProperty();

	protected float CurrentZoom;

	[SerializeField]
	private List<FadeAnimator> m_TargetPingEntitys = new List<FadeAnimator>();

	[SerializeField]
	private Vector2 m_CorrectTargetPositionPoint;

	[SerializeField]
	private float m_CorrectBiggerX = 185f;

	[SerializeField]
	private float m_CorrectBiggerMinusX = 185f;

	[SerializeField]
	private float m_CorrectBiggerY = 125f;

	[SerializeField]
	private float m_CorrectBiggerMinusY = 90f;

	private readonly Dictionary<NetPlayer, PingData> m_PlayerPingData = new Dictionary<NetPlayer, PingData>();

	private static UIMeinMenuTexts UIMainMenuTexts => UIStrings.Instance.MainMenu;

	private Vector2 MaxPos
	{
		get
		{
			Vector2 vector = m_Image.rectTransform.localScale;
			Vector2 vector2 = vector - Vector2.one * m_ZoomMinSize;
			return m_Size / 5f * vector * vector2 * m_MoveMapFrame;
		}
	}

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
		m_TitleLabelMap.text = UIMainMenuTexts.LocalMap;
		foreach (FadeAnimator item in m_TargetPingEntitys.Where((FadeAnimator entity) => entity != null))
		{
			if (item.CanvasGroup != null)
			{
				item.CanvasGroup.alpha = 0f;
			}
			item.DisappearAnimation();
		}
		for (int i = 0; i < m_TargetPingEntitys.Count; i++)
		{
			Image component = m_TargetPingEntitys[i].GetComponent<Image>();
			if (!(component == null))
			{
				if (BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors.Count < i)
				{
					break;
				}
				component.color = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsColors[i];
			}
		}
		SetMaxSize();
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_Title.SetText(string.Empty, value);
		}));
		AddDisposable(base.ViewModel.DrawResult.Subscribe(SetDrawResult));
		AddDisposable(base.ViewModel.CompassAngle.Subscribe(SetFrameAngle));
		AddDisposable(m_MapHistoryButton.OnHoverAsObservable().Subscribe(ShowLocalMapHistory));
		AddDisposable(base.ViewModel.CoopPingPosition.Subscribe(delegate((NetPlayer, Vector3) value)
		{
			PingPosition(value.Item1, value.Item2);
		}));
		OpenLocalMapFirstZoomSettings();
		SetMarkersVM();
		m_LegendBlockView.Bind(base.ViewModel.LocalMapLegendBlockVM);
		SetMapRotation((float)base.ViewModel.LocalMapRotation);
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			HandleUpdate();
		}));
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.LocalMap);
		});
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		SetMapRotation(0f);
		UISounds.Instance.Sounds.LocalMap.MapClose.Play();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.LocalMap);
		});
	}

	private void HandleUpdate()
	{
		ShowMarkersAlways();
	}

	private void ShowMarkersAlways()
	{
		foreach (RectTransform item in m_Image.transform)
		{
			if (!m_MarkersNeedToShowAlways.Contains(item))
			{
				continue;
			}
			foreach (RectTransform item2 in item)
			{
				LocalMapMarkerPCView component = item2.GetComponent<LocalMapMarkerPCView>();
				component.LocalMapMarkersAlwaysInside();
				component.ShowHideArrow(state: false, component.RealPosition, item2.position);
				bool flag = component.RealPosition.x > m_MapBlock.sizeDelta.x / 2f - m_CorrectBiggerX;
				bool flag2 = component.RealPosition.x < (0f - m_MapBlock.sizeDelta.x) / 2f + m_CorrectBiggerMinusX;
				bool flag3 = component.RealPosition.y > m_MapBlock.sizeDelta.y / 2f - m_CorrectBiggerY;
				bool flag4 = component.RealPosition.y < (0f - m_MapBlock.sizeDelta.y) / 2f + m_CorrectBiggerMinusY;
				if (flag || flag2 || flag3 || flag4)
				{
					float x = (flag ? (m_MapBlock.sizeDelta.x / 2f - m_CorrectBiggerX) : (flag2 ? ((0f - m_MapBlock.sizeDelta.x) / 2f + m_CorrectBiggerMinusX) : component.RealPosition.x));
					float y = (flag3 ? (m_MapBlock.sizeDelta.y / 2f - m_CorrectBiggerY) : (flag4 ? ((0f - m_MapBlock.sizeDelta.y) / 2f + m_CorrectBiggerMinusY) : component.RealPosition.y));
					item2.position = new Vector3(x, y);
					component.ShowHideArrow(state: true, component.RealPosition, item2.position);
				}
			}
		}
	}

	private void SetMarkersVM()
	{
		foreach (LocalMapMarkerVM item in base.ViewModel.MarkersVm)
		{
			AddLocalMapMarker(item);
		}
		AddDisposable(base.ViewModel.MarkersVm.ObserveAdd().Subscribe(delegate(CollectionAddEvent<LocalMapMarkerVM> value)
		{
			AddLocalMapMarker(value.Value);
		}));
		m_FrameBlock.localScale = Vector3.one;
		foreach (RectTransform item2 in m_Image.transform)
		{
			if (item2 == m_FrameBlock)
			{
				continue;
			}
			foreach (RectTransform item3 in item2)
			{
				item3.localScale = Vector3.one;
			}
		}
	}

	private void OpenLocalMapFirstZoomSettings()
	{
		SetMapSize();
		m_Image.rectTransform.sizeDelta = m_Size;
		m_Image.rectTransform.localScale = Vector2.one * m_ZoomMinSize;
		UpdateMapPosition(Vector2.zero);
		InteractableRightButtons();
	}

	private void SetMapRotation(float z)
	{
		m_Image.rectTransform.eulerAngles = new Vector3(0f, 0f, z);
		foreach (RectTransform item in m_Image.transform)
		{
			if (item == m_FrameBlock)
			{
				continue;
			}
			foreach (RectTransform item2 in item)
			{
				item2.localRotation = Quaternion.Inverse(m_Image.rectTransform.rotation);
			}
		}
	}

	protected void ShowLocalMapHistory(bool state)
	{
		UISounds.Instance.Sounds.LocalMap.ShowHideLocalMapLegend.Play();
		m_LegendBlockView.ShowHide(state);
		m_MapHistoryLittleSquare.DOAnchorPosX(state ? 31f : (-3f), 0.1f).SetEase(Ease.Linear).SetUpdate(isIndependentUpdate: true);
	}

	private void SetFrameAngle(float z)
	{
		m_Frame.eulerAngles = new Vector3(0f, 0f, 0f - z);
	}

	private void AddLocalMapMarker(LocalMapMarkerVM localMapMarkerVM)
	{
		LocalMapMarkerSet localMapMarkerSet = m_MarkerSets.FirstOrDefault(delegate(LocalMapMarkerSet s)
		{
			if (localMapMarkerVM.MarkerType == LocalMapMarkType.Poi)
			{
				localMapMarkerVM.MarkerType = LocalMapMarkType.VeryImportantThing;
			}
			return s.Type == localMapMarkerVM.MarkerType;
		});
		if (localMapMarkerSet != null)
		{
			LocalMapMarkerPCView item = WidgetFactory.GetWidget(localMapMarkerSet.View);
			item.transform.SetParent(localMapMarkerSet.Container, worldPositionStays: false);
			item.Initialize(m_Image.rectTransform.sizeDelta, delegate
			{
				WidgetFactory.DisposeWidget(item);
			});
			item.Bind(localMapMarkerVM);
		}
	}

	private static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
	{
		return Vector2.Min(Vector2.Max(value, min), max);
	}

	private static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
	{
		return Vector3.Min(Vector3.Max(value, min), max);
	}

	protected void FindRogueTrader(bool smooth)
	{
		LocalMapMarkerSet localMapMarkerSet = m_MarkerSets.FirstOrDefault((LocalMapMarkerSet s) => s.Type == LocalMapMarkType.PlayerCharacter);
		if (localMapMarkerSet == null || localMapMarkerSet.Container.transform.childCount <= 0)
		{
			return;
		}
		RectTransform rectTransform = localMapMarkerSet.Container.transform.Cast<RectTransform>().FirstOrDefault((RectTransform character) => character.GetComponent<LocalMapCharacterMarkerPCView>()?.CharacterName == Game.Instance.Player.MainCharacter.Entity?.CharacterName);
		if ((bool)rectTransform)
		{
			Vector2 maxPos = MaxPos;
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			Vector2 vector = Clamp(base.ViewModel.LocalMapRotation switch
			{
				BlueprintAreaPart.LocalMapRotationDegree.Degree0 => new Vector2(0f - anchoredPosition.x, 0f - anchoredPosition.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree90 => new Vector2(anchoredPosition.y, 0f - anchoredPosition.x), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree180 => new Vector2(anchoredPosition.x, anchoredPosition.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree270 => new Vector2(0f - anchoredPosition.y, anchoredPosition.x), 
				_ => default(Vector2), 
			} * m_Image.rectTransform.localScale, -maxPos, maxPos);
			if (smooth)
			{
				m_Image.rectTransform.DOAnchorPos(vector, 1f).SetUpdate(isIndependentUpdate: true);
			}
			else
			{
				m_Image.rectTransform.anchoredPosition = vector;
			}
			base.ViewModel.ScrollCameraToRogueTrader();
		}
	}

	private void SetDrawResult(WarhammerLocalMapRenderer.DrawResults dr)
	{
		m_Size = new Vector2(dr.ColorRT.width, dr.ColorRT.height);
		SetMapSize();
		Vector2 vector = dr.ScreenRect.size * m_Size;
		m_FrameBlock.sizeDelta = new Vector2(Mathf.Max(vector.x, vector.y), Mathf.Min(vector.x, vector.y));
		m_FrameBlock.anchoredPosition = dr.ScreenRect.min * m_Size;
	}

	private void SetMaxSize()
	{
		BlueprintAreaPart.LocalMapRotationDegree localMapRotation = base.ViewModel.LocalMapRotation;
		m_ChangedMapSize = ((localMapRotation == BlueprintAreaPart.LocalMapRotationDegree.Degree0 || localMapRotation == BlueprintAreaPart.LocalMapRotationDegree.Degree180) ? new Vector2(MaxSize.x, MaxSize.y) : new Vector2(MaxSize.y, MaxSize.x));
	}

	private void SetMapSize()
	{
		Vector2 vector = m_Size / m_ChangedMapSize;
		float num = Mathf.Max(vector.x, vector.y);
		bool flag = num >= 1f;
		float num2 = (flag ? Mathf.Max(num, 1f) : (1f / num));
		m_Size = (flag ? (m_Size / num2) : (m_Size * num2));
	}

	protected void SetMapScale(float zoomDelta)
	{
		float num = m_ZoomStep * zoomDelta;
		Vector3 vector = new Vector3(num, num, 0f);
		Vector3 localScale = m_Image.rectTransform.localScale;
		Vector3 vector2 = Clamp(localScale + vector, new Vector3(m_ZoomMinSize, m_ZoomMinSize, float.MinValue), new Vector3(m_ZoomMaxSize, m_ZoomMaxSize, float.MaxValue));
		if (vector2 == localScale)
		{
			return;
		}
		m_Image.rectTransform.localScale = vector2;
		CurrentZoom = vector2.x;
		foreach (RectTransform item in m_Image.transform)
		{
			if (item == m_FrameBlock)
			{
				continue;
			}
			foreach (RectTransform item2 in item)
			{
				item2.localScale -= vector / 2f;
			}
		}
		UpdateMapPosition(Vector2.zero);
		InteractableRightButtons();
	}

	protected virtual void InteractableRightButtons()
	{
	}

	protected void UpdateMapPosition(Vector2 scrollDelta)
	{
		if (MinZoom.Value)
		{
			Vector2 anchoredPosition = m_Image.rectTransform.anchoredPosition;
			Vector2 maxPos = MaxPos;
			Vector2 vector = Clamp(anchoredPosition + scrollDelta, -maxPos, maxPos);
			if (vector != anchoredPosition)
			{
				m_Image.rectTransform.anchoredPosition = vector;
			}
		}
	}

	protected Vector2 GetViewportPos(Vector2 position)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Image.rectTransform, position, UICamera.Instance, out var localPoint);
		localPoint += Vector2.Scale(m_Size, m_Image.rectTransform.pivot);
		return localPoint / m_Size;
	}

	protected Vector2 GetViewportPos(PointerEventData eventData)
	{
		return GetViewportPos(eventData.position);
	}

	private void PingPosition(NetPlayer player, Vector3 position)
	{
		int playerIndex = player.Index - 1;
		if (m_PlayerPingData.ContainsKey(player))
		{
			m_PlayerPingData[player].PingDelay?.Dispose();
		}
		else
		{
			m_PlayerPingData[player] = new PingData();
		}
		Vector3 vector = WarhammerLocalMapRenderer.Instance.WorldToViewportPoint(position);
		PingData pingData = m_PlayerPingData[player];
		RectTransform rectTransform = m_TargetPingEntitys[playerIndex].transform as RectTransform;
		if (rectTransform != null)
		{
			rectTransform.anchoredPosition = new Vector2(m_Image.rectTransform.sizeDelta.x * vector.x, m_Image.rectTransform.sizeDelta.y * vector.y) - m_CorrectTargetPositionPoint;
		}
		m_TargetPingEntitys[playerIndex].AppearAnimation();
		pingData.PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingEntitys[playerIndex].DisappearAnimation();
		}, 7.5f);
	}
}
