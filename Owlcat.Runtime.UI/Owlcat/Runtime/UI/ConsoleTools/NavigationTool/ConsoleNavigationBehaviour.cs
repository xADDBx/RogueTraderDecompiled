using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Dependencies;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public abstract class ConsoleNavigationBehaviour : IConfirmClickHandler, IConsoleEntity, ILongConfirmClickHandler, IDeclineClickHandler, ILongDeclineClickHandler, IFunc01ClickHandler, ILongFunc01ClickHandler, IFunc02ClickHandler, ILongFunc02ClickHandler, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, INavigationDirectionsHandler, INavigationVerticalDirectionsHandler, INavigationUpDirectionHandler, INavigationDownDirectionHandler, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler, INavigationVectorDirectionHandler, IDisposable
{
	private BoolReactiveProperty m_CanClickConfirm = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanLongClickConfirm = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanClickDecline = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanLongClickDecline = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanClickFunc01 = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanLongClickFunc01 = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanClickFunc02 = new BoolReactiveProperty(initialValue: false);

	private BoolReactiveProperty m_CanLongClickFunc02 = new BoolReactiveProperty(initialValue: false);

	private List<IDisposable> m_InnerNavigationSubscriptions = new List<IDisposable>();

	private readonly ClickEvent m_ClickEvent = new ClickEvent();

	public string ContextName;

	[CanBeNull]
	protected IConsoleNavigationOwner m_Owner;

	[CanBeNull]
	private IConsoleNavigationScroll m_Scroll;

	private float m_DelayBeforeButtonRepeat = 0.4f;

	private float m_DelayBetweenButtonRepeat = 0.1f;

	private int m_MaxStepsInARowPossible = 1000;

	private float m_StepSpeedUp = 0.004f;

	private float m_MaxSpeedUp = 0.025f;

	private float m_PreviousTimeButtonPressed;

	private float m_TimeAtStartOfPress;

	private bool m_WasPressedAtPreviousFrame;

	private bool m_LeftIsPressed;

	private bool m_RightIsPressed;

	private bool m_UpIsPressed;

	private bool m_DownIsPressed;

	private Vector2 m_StickValue = Vector2.zero;

	private bool m_ScrollingLeft;

	private bool m_ScrollingRight;

	private bool m_ScrollingUp;

	private bool m_ScrollingDown;

	private bool m_ScrollingStick;

	private IDisposable m_UpdateSubscription;

	private CompositeDisposable m_PointerSubscriptions;

	private bool m_PositionForFloatNavigationHasBeenSet;

	private Vector2 m_PositionForFloatNavigation;

	private ReactiveProperty<IConsoleEntity> m_DeepestFocusAsObservable = new ReactiveProperty<IConsoleEntity>();

	private readonly List<IConsoleEntity> m_NestedFocuses = new List<IConsoleEntity>();

	private ReactiveProperty<IConsoleEntity> m_Focus = new ReactiveProperty<IConsoleEntity>();

	private IConsoleEntity m_CurrentEntity;

	private bool m_ScrollY;

	private bool m_ScrollX;

	private readonly bool m_PlaySoundOnSelect;

	public abstract IEnumerable<IConsoleEntity> Entities { get; }

	public IConsoleNavigationOwner Owner => m_Owner;

	private bool StickIsPressed => m_StickValue != Vector2.zero;

	private bool DPadIsPressed
	{
		get
		{
			if (!m_LeftIsPressed && !m_RightIsPressed && !m_UpIsPressed)
			{
				return m_DownIsPressed;
			}
			return true;
		}
	}

	private bool AnyButtonIsPressed
	{
		get
		{
			if (!DPadIsPressed)
			{
				return StickIsPressed;
			}
			return true;
		}
	}

	public List<IFloatConsoleNavigationEntity> NeighboursForFloatNavigation { get; } = new List<IFloatConsoleNavigationEntity>();


	public Vector2 PositionForFloatNavigation
	{
		private get
		{
			return m_PositionForFloatNavigation;
		}
		set
		{
			m_PositionForFloatNavigation = value;
			m_PositionForFloatNavigationHasBeenSet = true;
		}
	}

	public bool IsFocused { get; protected set; }

	public IConsoleEntity DeepestNestedFocus
	{
		get
		{
			IConsoleEntity consoleEntity = m_Focus.Value;
			while (consoleEntity is ConsoleNavigationBehaviour || consoleEntity is IConsoleEntityProxy)
			{
				if (consoleEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour)
				{
					consoleEntity = consoleNavigationBehaviour.DeepestNestedFocus;
				}
				if (consoleEntity is IConsoleEntityProxy consoleEntityProxy)
				{
					consoleEntity = consoleEntityProxy.ConsoleEntityProxy;
				}
			}
			m_DeepestFocusAsObservable.Value = consoleEntity;
			return consoleEntity;
		}
	}

	public ReactiveProperty<IConsoleEntity> DeepestFocusAsObservable => m_DeepestFocusAsObservable;

	public List<IConsoleEntity> NestedFocuses
	{
		get
		{
			m_NestedFocuses.Clear();
			if (m_Focus.Value == null)
			{
				return m_NestedFocuses;
			}
			m_NestedFocuses.Add(m_Focus.Value);
			IConsoleEntity consoleEntity = m_Focus.Value;
			while (consoleEntity is IConsoleEntityProxy consoleEntityProxy)
			{
				consoleEntity = consoleEntityProxy.ConsoleEntityProxy;
				m_NestedFocuses.Add(consoleEntity);
			}
			if (consoleEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour)
			{
				m_NestedFocuses.AddRange(consoleNavigationBehaviour.NestedFocuses);
			}
			return m_NestedFocuses;
		}
	}

	public ReactiveProperty<IConsoleEntity> Focus => m_Focus;

	public IConsoleEntity CurrentEntity
	{
		get
		{
			if (m_CurrentEntity == null)
			{
				SetFocusValue(m_CurrentEntity = Entities.FirstOrDefault((IConsoleEntity e) => e is ConsoleNavigationBehaviour consoleNavigationBehaviour && consoleNavigationBehaviour.Focus.Value != null));
			}
			return m_CurrentEntity;
		}
		private set
		{
			m_CurrentEntity = value;
			UpdateClickEnabledProperties();
		}
	}

	private void SetClicksInInputLayer(InputLayer il, IReadOnlyReactiveProperty<bool> enabled, InputBindStruct ibs, List<NavigationInputEventTypeConfig> inputConfig = null)
	{
		ibs.Binds.AddRange(il.AddButton(OnConfirmClick, 8, enabled.And(m_CanClickConfirm).ToReadOnlyReactiveProperty(), NavigationInputEventTypeHelper.GetEventType(inputConfig, 8)).Binds);
		ibs.Binds.AddRange(il.AddButton(OnLongConfirmClick, 8, enabled.And(m_CanLongClickConfirm).ToReadOnlyReactiveProperty(), InputActionEventType.ButtonJustLongPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnDeclineClick, 9, enabled.And(m_CanClickDecline).ToReadOnlyReactiveProperty(), NavigationInputEventTypeHelper.GetEventType(inputConfig, 9)).Binds);
		ibs.Binds.AddRange(il.AddButton(OnLongDeclineClick, 9, enabled.And(m_CanLongClickDecline).ToReadOnlyReactiveProperty(), InputActionEventType.ButtonJustLongPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnFunc01Click, 10, enabled.And(m_CanClickFunc01).ToReadOnlyReactiveProperty(), NavigationInputEventTypeHelper.GetEventType(inputConfig, 10)).Binds);
		ibs.Binds.AddRange(il.AddButton(OnLongFunc01Click, 10, enabled.And(m_CanLongClickFunc01).ToReadOnlyReactiveProperty(), InputActionEventType.ButtonJustLongPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnFunc02Click, 11, enabled.And(m_CanClickFunc02).ToReadOnlyReactiveProperty(), NavigationInputEventTypeHelper.GetEventType(inputConfig, 11)).Binds);
		ibs.Binds.AddRange(il.AddButton(OnLongFunc02Click, 11, enabled.And(m_CanLongClickFunc02).ToReadOnlyReactiveProperty(), InputActionEventType.ButtonJustLongPressed).Binds);
	}

	public IObservable<Unit> OnClickAsObservable()
	{
		return Observable.FromEvent((Func<Action, UnityAction>)((Action h) => h.Invoke), (Action<UnityAction>)m_ClickEvent.AddListener, (Action<UnityAction>)m_ClickEvent.RemoveListener);
	}

	private void OnConfirmClick(InputActionEventData data)
	{
		OnConfirmClick();
		m_ClickEvent.Invoke();
	}

	private void OnLongConfirmClick(InputActionEventData data)
	{
		OnLongConfirmClick();
		m_ClickEvent.Invoke();
	}

	private void OnDeclineClick(InputActionEventData data)
	{
		OnDeclineClick();
		m_ClickEvent.Invoke();
	}

	private void OnLongDeclineClick(InputActionEventData data)
	{
		OnLongDeclineClick();
		m_ClickEvent.Invoke();
	}

	private void OnFunc01Click(InputActionEventData data)
	{
		OnFunc01Click();
		m_ClickEvent.Invoke();
	}

	private void OnLongFunc01Click(InputActionEventData data)
	{
		OnLongFunc01Click();
		m_ClickEvent.Invoke();
	}

	private void OnFunc02Click(InputActionEventData data)
	{
		OnFunc02Click();
		m_ClickEvent.Invoke();
	}

	private void OnLongFunc02Click(InputActionEventData data)
	{
		OnLongFunc02Click();
		m_ClickEvent.Invoke();
	}

	public bool CanConfirmClick()
	{
		return CurrentEntity.CanConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return CurrentEntity.GetConfirmClickHint();
	}

	public void OnConfirmClick()
	{
		CurrentEntity.OnConfirmClick();
	}

	public bool CanLongConfirmClick()
	{
		return CurrentEntity.CanLongConfirmClick();
	}

	public string GetLongConfirmClickHint()
	{
		return CurrentEntity.GetLongConfirmClickHint();
	}

	public void OnLongConfirmClick()
	{
		CurrentEntity.OnLongConfirmClick();
	}

	public bool CanDeclineClick()
	{
		return CurrentEntity.CanDeclineClick();
	}

	public string GetDeclineClickHint()
	{
		return CurrentEntity.GetDeclineClickHint();
	}

	public void OnDeclineClick()
	{
		CurrentEntity.OnDeclineClick();
	}

	public bool CanLongDeclineClick()
	{
		return CurrentEntity.CanLongDeclineClick();
	}

	public string GetLongDeclineClickHint()
	{
		return CurrentEntity.GetLongDeclineClickHint();
	}

	public void OnLongDeclineClick()
	{
		CurrentEntity.OnLongDeclineClick();
	}

	public bool CanFunc01Click()
	{
		return CurrentEntity.CanFunc01Click();
	}

	public string GetFunc01ClickHint()
	{
		return CurrentEntity.GetFunc01ClickHint();
	}

	public void OnFunc01Click()
	{
		CurrentEntity.OnFunc01Click();
	}

	public bool CanLongFunc01Click()
	{
		return CurrentEntity.CanLongFunc01Click();
	}

	public string GetLongFunc01ClickHint()
	{
		return CurrentEntity.GetLongFunc01ClickHint();
	}

	public void OnLongFunc01Click()
	{
		CurrentEntity.OnLongFunc01Click();
	}

	public bool CanFunc02Click()
	{
		return CurrentEntity.CanFunc02Click();
	}

	public string GetFunc02ClickHint()
	{
		return CurrentEntity.GetFunc02ClickHint();
	}

	public void OnFunc02Click()
	{
		CurrentEntity.OnFunc02Click();
	}

	public bool CanLongFunc02Click()
	{
		return CurrentEntity.CanLongFunc02Click();
	}

	public string GetLongFunc02ClickHint()
	{
		return CurrentEntity.GetLongFunc02ClickHint();
	}

	public void OnLongFunc02Click()
	{
		CurrentEntity.OnLongFunc02Click();
	}

	private void UpdateClickEnabledProperties()
	{
		if (m_InnerNavigationSubscriptions.Count > 0)
		{
			foreach (IDisposable innerNavigationSubscription in m_InnerNavigationSubscriptions)
			{
				innerNavigationSubscription.Dispose();
			}
			m_InnerNavigationSubscriptions.Clear();
		}
		IConsoleEntity consoleEntity;
		for (consoleEntity = m_CurrentEntity; consoleEntity is IConsoleEntityProxy consoleEntityProxy; consoleEntity = consoleEntityProxy.ConsoleEntityProxy)
		{
		}
		if (consoleEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour)
		{
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanClickConfirm.Subscribe(delegate(bool value)
			{
				m_CanClickConfirm.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanLongClickConfirm.Subscribe(delegate(bool value)
			{
				m_CanLongClickConfirm.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanClickDecline.Subscribe(delegate(bool value)
			{
				m_CanClickDecline.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanLongClickDecline.Subscribe(delegate(bool value)
			{
				m_CanLongClickDecline.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanClickFunc01.Subscribe(delegate(bool value)
			{
				m_CanClickFunc01.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanLongClickFunc01.Subscribe(delegate(bool value)
			{
				m_CanLongClickFunc01.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanClickFunc02.Subscribe(delegate(bool value)
			{
				m_CanClickFunc02.Value = value;
			}));
			m_InnerNavigationSubscriptions.Add(consoleNavigationBehaviour.m_CanLongClickFunc02.Subscribe(delegate(bool value)
			{
				m_CanLongClickFunc02.Value = value;
			}));
		}
		m_CanClickConfirm.Value = CanConfirmClick();
		m_CanLongClickConfirm.Value = CanLongConfirmClick();
		m_CanClickDecline.Value = CanDeclineClick();
		m_CanLongClickDecline.Value = CanLongDeclineClick();
		m_CanClickFunc01.Value = CanFunc01Click();
		m_CanLongClickFunc01.Value = CanLongFunc01Click();
		m_CanClickFunc02.Value = CanFunc02Click();
		m_CanLongClickFunc02.Value = CanLongFunc02Click();
	}

	public void UpdateDeepestFocusObserve()
	{
		m_DeepestFocusAsObservable.Value = DeepestNestedFocus;
	}

	public bool IsInNestedFocuses(IConsoleEntity entity)
	{
		return NestedFocuses.Contains(entity);
	}

	protected ConsoleNavigationBehaviour([CanBeNull] IConsoleNavigationOwner owner = null, [CanBeNull] IConsoleNavigationScroll scroll = null, bool playSoundOnSelect = false)
	{
		m_Owner = owner;
		m_Scroll = scroll;
		m_PlaySoundOnSelect = playSoundOnSelect;
		InputBehavior inputBehavior = GamePad.Instance?.Player?.controllers.maps.InputBehaviors.FirstOrDefault();
		if (inputBehavior != null)
		{
			m_DelayBeforeButtonRepeat = inputBehavior.buttonRepeatDelay;
			m_DelayBetweenButtonRepeat = 1f / inputBehavior.buttonRepeatRate;
		}
	}

	public InputLayer GetInputLayer(InputLayer il = null, IReadOnlyReactiveProperty<bool> enabled = null, bool leftStick = true, bool rightStick = false, InputBindStruct ibs = null, List<NavigationInputEventTypeConfig> inputConfig = null)
	{
		il = il ?? new InputLayer
		{
			ContextName = "Navigation"
		};
		ibs = ibs ?? new InputBindStruct(il);
		ibs.Binds.AddRange(il.AddButton(OnLeftIsPressed, 4, enabled, InputActionEventType.ButtonPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnRightIsPressed, 5, enabled, InputActionEventType.ButtonPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnUpIsPressed, 6, enabled, InputActionEventType.ButtonPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnDownIsPressed, 7, enabled, InputActionEventType.ButtonPressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnLeftIsUnpressed, 4, enabled, InputActionEventType.ButtonUnpressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnRightIsUnpressed, 5, enabled, InputActionEventType.ButtonUnpressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnUpIsUnpressed, 6, enabled, InputActionEventType.ButtonUnpressed).Binds);
		ibs.Binds.AddRange(il.AddButton(OnDownIsUnpressed, 7, enabled, InputActionEventType.ButtonUnpressed).Binds);
		if (leftStick)
		{
			ibs.Binds.AddRange(il.AddAxis2D(OnStickPressed, 0, 1, enabled).Binds);
		}
		if (rightStick)
		{
			ibs.Binds.AddRange(il.AddAxis2D(OnStickPressed, 2, 3, enabled).Binds);
		}
		SetClicksInInputLayer(il, enabled, ibs, inputConfig);
		il.LayerBinded.Subscribe(SetActive);
		return il;
	}

	private void SetActive(bool active)
	{
		m_UpdateSubscription?.Dispose();
		m_UpdateSubscription = null;
		if (m_PointerSubscriptions == null)
		{
			m_PointerSubscriptions = new CompositeDisposable();
		}
		else
		{
			m_PointerSubscriptions.Clear();
		}
		if (active)
		{
			m_UpdateSubscription = ObservableExtensions.Subscribe(MainThreadDispatcher.LateUpdateAsObservable(), delegate
			{
				Tick();
			});
			RecursiveSubscribeToPointerDownEvent(Entities);
			return;
		}
		m_PointerSubscriptions?.Dispose();
		m_PointerSubscriptions = null;
		m_WasPressedAtPreviousFrame = false;
		m_LeftIsPressed = false;
		m_RightIsPressed = false;
		m_UpIsPressed = false;
		m_DownIsPressed = false;
		m_ScrollingLeft = false;
		m_ScrollingRight = false;
		m_ScrollingUp = false;
		m_ScrollingDown = false;
		m_StickValue = Vector2.zero;
		m_ScrollingStick = false;
	}

	private void RecursiveSubscribeToPointerDownEvent(IEnumerable<IConsoleEntity> entities)
	{
		foreach (IConsoleEntity entity in entities)
		{
			if (entity is ConsoleNavigationBehaviour consoleNavigationBehaviour)
			{
				RecursiveSubscribeToPointerDownEvent(consoleNavigationBehaviour.Entities);
				continue;
			}
			VirtualListElement virtualListElement = entity as VirtualListElement;
			if (virtualListElement != null)
			{
				if (virtualListElement.ConsoleEntityProxy != null)
				{
					SubscribeToPointerLeftClick(virtualListElement.ConsoleEntityProxy);
					continue;
				}
				IDisposable disposable = null;
				disposable = ObservableExtensions.Subscribe(virtualListElement.AttachViewCommand, delegate
				{
					m_PointerSubscriptions.Remove(disposable);
					SubscribeToPointerLeftClick(virtualListElement.ConsoleEntityProxy);
				});
				m_PointerSubscriptions.Add(disposable);
			}
			else
			{
				SubscribeToPointerLeftClick(entity);
			}
		}
	}

	private void SubscribeToPointerLeftClick(IConsoleEntity entity)
	{
		while (entity is IConsoleEntityProxy consoleEntityProxy)
		{
			entity = consoleEntityProxy.ConsoleEntityProxy;
		}
		if (entity is IConsolePointerLeftClickEvent consolePointerLeftClickEvent)
		{
			m_PointerSubscriptions.Add(ObservableExtensions.Subscribe(consolePointerLeftClickEvent.PointerLeftClickCommand, delegate
			{
				FocusOnEntityManual(entity);
			}));
		}
	}

	private void OnLeftIsPressed(InputActionEventData data)
	{
		m_LeftIsPressed = true;
	}

	private void OnRightIsPressed(InputActionEventData data)
	{
		m_RightIsPressed = true;
	}

	private void OnUpIsPressed(InputActionEventData data)
	{
		m_UpIsPressed = true;
	}

	private void OnDownIsPressed(InputActionEventData data)
	{
		m_DownIsPressed = true;
	}

	private void OnLeftIsUnpressed(InputActionEventData data)
	{
		if (!m_ScrollX)
		{
			m_LeftIsPressed = false;
			m_ScrollingLeft = false;
		}
	}

	private void OnRightIsUnpressed(InputActionEventData data)
	{
		if (!m_ScrollX)
		{
			m_RightIsPressed = false;
			m_ScrollingRight = false;
		}
	}

	private void OnUpIsUnpressed(InputActionEventData data)
	{
		if (!m_ScrollY)
		{
			m_UpIsPressed = false;
			m_ScrollingUp = false;
		}
	}

	private void OnDownIsUnpressed(InputActionEventData data)
	{
		if (!m_ScrollY)
		{
			m_DownIsPressed = false;
			m_ScrollingDown = false;
		}
	}

	private void OnStickPressed(InputActionEventData data, Vector2 vector)
	{
		if (vector.y > 0.5f)
		{
			OnDownIsUnpressed(data);
			OnUpIsPressed(data);
		}
		else if (vector.y < -0.5f)
		{
			OnUpIsUnpressed(data);
			OnDownIsPressed(data);
		}
		if (vector.x > 0.5f)
		{
			OnLeftIsUnpressed(data);
			OnRightIsPressed(data);
		}
		else if (vector.x < -0.5f)
		{
			OnRightIsUnpressed(data);
			OnLeftIsPressed(data);
		}
		m_ScrollY = vector.y > 0.5f || vector.y < -0.5f;
		m_ScrollX = vector.x > 0.5f || vector.x < -0.5f;
	}

	private void Tick()
	{
		if (!AnyButtonIsPressed)
		{
			m_WasPressedAtPreviousFrame = false;
			return;
		}
		if (TickScroll())
		{
			m_WasPressedAtPreviousFrame = false;
			return;
		}
		if (!m_WasPressedAtPreviousFrame)
		{
			m_TimeAtStartOfPress = Time.unscaledTime;
			m_WasPressedAtPreviousFrame = true;
		}
		float buttonTimePressed = Time.unscaledTime - m_TimeAtStartOfPress;
		if (buttonTimePressed < 0.0001f)
		{
			m_PreviousTimeButtonPressed = -1f;
		}
		float threshold = 0f;
		if (JustPassedTimeThreshold())
		{
			m_PreviousTimeButtonPressed = buttonTimePressed;
			if (HandlePressed())
			{
				UpdateDeepestFocusObserve();
			}
			return;
		}
		threshold += m_DelayBeforeButtonRepeat;
		float num = m_DelayBetweenButtonRepeat;
		if (JustPassedTimeThreshold())
		{
			m_PreviousTimeButtonPressed = buttonTimePressed;
			if (HandlePressed())
			{
				UpdateDeepestFocusObserve();
			}
			return;
		}
		if (threshold > buttonTimePressed)
		{
			m_PreviousTimeButtonPressed = buttonTimePressed;
			return;
		}
		for (int i = 0; i < m_MaxStepsInARowPossible; i++)
		{
			if (num > m_MaxSpeedUp)
			{
				num -= m_StepSpeedUp;
			}
			threshold += num;
			if (JustPassedTimeThreshold())
			{
				m_PreviousTimeButtonPressed = buttonTimePressed;
				if (HandlePressed())
				{
					UpdateDeepestFocusObserve();
				}
				break;
			}
			if (threshold > buttonTimePressed)
			{
				m_PreviousTimeButtonPressed = buttonTimePressed;
				break;
			}
		}
		bool JustPassedTimeThreshold()
		{
			if (threshold == 0f && buttonTimePressed < 0.001f && m_PreviousTimeButtonPressed < 0.001f)
			{
				return true;
			}
			if (m_PreviousTimeButtonPressed < threshold)
			{
				return buttonTimePressed >= threshold;
			}
			return false;
		}
	}

	private bool TickScroll()
	{
		if (m_CurrentEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour)
		{
			return consoleNavigationBehaviour.TickScroll();
		}
		if (m_Scroll == null)
		{
			return false;
		}
		if (m_ScrollingLeft && m_ScrollingRight)
		{
			m_ScrollingLeft = (m_ScrollingRight = false);
		}
		if (m_ScrollingUp && m_ScrollingDown)
		{
			m_ScrollingUp = (m_ScrollingDown = false);
		}
		if (m_ScrollingLeft)
		{
			m_Scroll.ScrollLeft();
			return !m_Scroll.CanFocusEntity(GetLeftValidEntity());
		}
		if (m_ScrollingRight)
		{
			m_Scroll.ScrollRight();
			return !m_Scroll.CanFocusEntity(GetRightValidEntity());
		}
		if (m_ScrollingUp)
		{
			m_Scroll.ScrollUp();
			return !m_Scroll.CanFocusEntity(GetUpValidEntity());
		}
		if (m_ScrollingDown)
		{
			m_Scroll.ScrollDown();
			return !m_Scroll.CanFocusEntity(GetDownValidEntity());
		}
		if (m_ScrollingStick)
		{
			m_Scroll.ScrollInDirection(m_StickValue);
			return !m_Scroll.CanFocusEntity(GetValidEntityInDirection(m_StickValue));
		}
		return false;
	}

	private bool HandlePressed()
	{
		if (m_UpIsPressed && m_DownIsPressed)
		{
			m_UpIsPressed = (m_DownIsPressed = false);
		}
		if (m_LeftIsPressed && m_RightIsPressed)
		{
			m_LeftIsPressed = (m_RightIsPressed = false);
		}
		if (DPadIsPressed)
		{
			m_StickValue = Vector2.zero;
		}
		if (m_LeftIsPressed)
		{
			return HandleLeft();
		}
		if (m_RightIsPressed)
		{
			return HandleRight();
		}
		if (m_UpIsPressed)
		{
			return HandleUp();
		}
		if (m_DownIsPressed)
		{
			return HandleDown();
		}
		if (StickIsPressed)
		{
			return HandleVector(m_StickValue);
		}
		return false;
	}

	public bool HandleLeft()
	{
		if (CurrentEntity == null || !CurrentEntity.IsValid())
		{
			return FocusOnLastValidEntity();
		}
		if (m_Focus.Value == null)
		{
			return FocusOnCurrentEntity();
		}
		if (CurrentEntity.HandleLeft())
		{
			return true;
		}
		IConsoleEntity leftValidEntity = GetLeftValidEntity();
		if (leftValidEntity == null)
		{
			if (m_Owner is INavigationLeftDirectionHandler navigationLeftDirectionHandler)
			{
				navigationLeftDirectionHandler.HandleLeft();
			}
			return false;
		}
		if (m_Scroll != null)
		{
			if (!m_Scroll.CanFocusEntity(leftValidEntity))
			{
				return HandleVector(Vector2.left);
			}
			if (m_ScrollingLeft)
			{
				m_WasPressedAtPreviousFrame = false;
			}
		}
		FocusOnEntity(leftValidEntity);
		return true;
	}

	public bool HandleRight()
	{
		if (CurrentEntity == null || !CurrentEntity.IsValid())
		{
			return FocusOnFirstValidEntity();
		}
		if (m_Focus.Value == null)
		{
			return FocusOnCurrentEntity();
		}
		if (CurrentEntity.HandleRight())
		{
			return true;
		}
		IConsoleEntity rightValidEntity = GetRightValidEntity();
		if (rightValidEntity == null)
		{
			if (m_Owner is INavigationRightDirectionHandler navigationRightDirectionHandler)
			{
				navigationRightDirectionHandler.HandleRight();
			}
			return false;
		}
		if (m_Scroll != null)
		{
			if (!m_Scroll.CanFocusEntity(rightValidEntity))
			{
				return HandleVector(Vector2.right);
			}
			if (m_ScrollingRight)
			{
				m_WasPressedAtPreviousFrame = false;
			}
		}
		FocusOnEntity(rightValidEntity);
		return true;
	}

	public bool HandleUp()
	{
		if (CurrentEntity == null || !CurrentEntity.IsValid())
		{
			FocusOnLastValidEntity();
			return true;
		}
		if (m_Focus.Value == null)
		{
			return FocusOnCurrentEntity();
		}
		if (CurrentEntity.HandleUp())
		{
			return true;
		}
		IConsoleEntity upValidEntity = GetUpValidEntity();
		if (upValidEntity == null)
		{
			if (m_Owner is INavigationUpDirectionHandler navigationUpDirectionHandler)
			{
				navigationUpDirectionHandler.HandleUp();
			}
			return false;
		}
		if (m_Scroll != null)
		{
			if (!m_Scroll.CanFocusEntity(upValidEntity))
			{
				return HandleVector(Vector2.up);
			}
			if (m_ScrollingUp)
			{
				m_WasPressedAtPreviousFrame = false;
			}
		}
		FocusOnEntity(upValidEntity);
		return true;
	}

	public bool HandleDown()
	{
		if (CurrentEntity == null || !CurrentEntity.IsValid())
		{
			FocusOnFirstValidEntity();
			return true;
		}
		if (m_Focus.Value == null)
		{
			return FocusOnCurrentEntity();
		}
		if (CurrentEntity.HandleDown())
		{
			return true;
		}
		IConsoleEntity downValidEntity = GetDownValidEntity();
		if (downValidEntity == null)
		{
			if (m_Owner is INavigationDownDirectionHandler navigationDownDirectionHandler)
			{
				navigationDownDirectionHandler.HandleDown();
			}
			return false;
		}
		if (m_Scroll != null)
		{
			if (!m_Scroll.CanFocusEntity(downValidEntity))
			{
				return HandleVector(Vector2.down);
			}
			if (m_ScrollingDown)
			{
				m_WasPressedAtPreviousFrame = false;
			}
		}
		FocusOnEntity(downValidEntity);
		return true;
	}

	public bool HandleVector(Vector2 vector)
	{
		if (CurrentEntity == null)
		{
			FocusOnFirstValidEntity();
			return true;
		}
		if (HandleDirectionForEntity(CurrentEntity, vector))
		{
			return true;
		}
		IConsoleEntity validEntityInDirection = GetValidEntityInDirection(vector);
		if (validEntityInDirection == null)
		{
			if (m_Owner != null)
			{
				HandleDirectionForEntity(m_Owner, vector);
			}
			return false;
		}
		if (m_Scroll != null && !m_Scroll.CanFocusEntity(validEntityInDirection))
		{
			if (validEntityInDirection != GetLastValidEntity() && validEntityInDirection != GetFirstValidEntity())
			{
				m_Scroll.ForceScrollToEntity(validEntityInDirection);
				FocusOnEntity(validEntityInDirection);
				return true;
			}
			m_Scroll.ForceScrollToEntity(validEntityInDirection);
		}
		FocusOnEntity(validEntityInDirection);
		return true;
	}

	private bool HandleDirectionForEntity(IConsoleEntity entity, Vector2 vector)
	{
		if (entity.HandleVector(vector))
		{
			return true;
		}
		if (Math.Abs(vector.x) >= Math.Abs(vector.y))
		{
			if (vector.x >= 0f)
			{
				if (entity is INavigationRightDirectionHandler navigationRightDirectionHandler)
				{
					return navigationRightDirectionHandler.HandleRight();
				}
			}
			else if (entity is INavigationLeftDirectionHandler navigationLeftDirectionHandler)
			{
				return navigationLeftDirectionHandler.HandleLeft();
			}
		}
		else if (vector.y >= 0f)
		{
			if (entity is INavigationUpDirectionHandler navigationUpDirectionHandler)
			{
				return navigationUpDirectionHandler.HandleUp();
			}
		}
		else if (entity is INavigationDownDirectionHandler navigationDownDirectionHandler)
		{
			return navigationDownDirectionHandler.HandleDown();
		}
		return false;
	}

	private void StartScrollInDirection(Vector2 direction)
	{
		m_ScrollingStick = true;
		m_Scroll?.ScrollInDirection(direction);
		m_ScrollingStick = false;
	}

	protected abstract IConsoleEntity GetLeftValidEntity();

	protected abstract IConsoleEntity GetRightValidEntity();

	protected abstract IConsoleEntity GetUpValidEntity();

	protected abstract IConsoleEntity GetDownValidEntity();

	protected abstract IConsoleEntity GetValidEntityInDirection(Vector2 direction);

	protected abstract IConsoleEntity GetFirstValidEntity();

	protected abstract IConsoleEntity GetLastValidEntity();

	public bool SetCurrentEntity(IConsoleEntity newCurrentEntity)
	{
		foreach (IConsoleEntity entity in Entities)
		{
			if (entity == newCurrentEntity)
			{
				CurrentEntity = entity;
				return true;
			}
			IConsoleEntity consoleEntity;
			for (consoleEntity = entity; consoleEntity is IConsoleEntityProxy consoleEntityProxy; consoleEntity = consoleEntityProxy.ConsoleEntityProxy)
			{
			}
			if (consoleEntity == newCurrentEntity)
			{
				CurrentEntity = entity;
				return true;
			}
			if (consoleEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour && consoleNavigationBehaviour.SetCurrentEntity(newCurrentEntity))
			{
				return true;
			}
		}
		return false;
	}

	private void SetFocusValue(IConsoleEntity value)
	{
		m_Focus.Value = value;
		UpdateDeepestFocusObserve();
	}

	public void ResetCurrentEntity(bool resetInChildNavigations = true)
	{
		if (resetInChildNavigations)
		{
			foreach (IConsoleEntity entity in Entities)
			{
				IConsoleEntity consoleEntity;
				for (consoleEntity = entity; consoleEntity is IConsoleEntityProxy consoleEntityProxy; consoleEntity = consoleEntityProxy.ConsoleEntityProxy)
				{
				}
				if (consoleEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour)
				{
					consoleNavigationBehaviour.ResetCurrentEntity();
				}
			}
		}
		CurrentEntity?.SetFocused(value: false);
		m_Owner?.EntityFocused(null);
		CurrentEntity = null;
		SetFocusValue(null);
	}

	private void UnFocusAllEntities()
	{
		Entities.ForEach(delegate(IConsoleEntity entity)
		{
			entity.SetFocused(value: false);
		});
	}

	public void UnFocusCurrentEntity()
	{
		CurrentEntity?.SetFocused(value: false);
		SetFocusValue(null);
	}

	public bool FocusOnCurrentEntity()
	{
		IConsoleEntity newFocus = ((CurrentEntity != null && CurrentEntity.IsValid()) ? CurrentEntity : GetFirstValidEntity());
		return FocusOnEntity(newFocus);
	}

	protected bool FocusOnEntity(IConsoleEntity newFocus)
	{
		if (newFocus == null || !newFocus.IsValid())
		{
			return false;
		}
		foreach (IConsoleEntity entity in Entities)
		{
			if (entity == newFocus)
			{
				if (m_PlaySoundOnSelect)
				{
					UIKitSoundManager.PlayButtonClickSound();
				}
				CurrentEntity?.SetFocused(value: false);
				newFocus.SetFocused(value: true);
				CurrentEntity = newFocus;
				SetFocusValue(CurrentEntity);
				m_Owner?.EntityFocused(CurrentEntity);
				return true;
			}
			IConsoleEntity consoleEntity;
			for (consoleEntity = entity; consoleEntity is IConsoleEntityProxy consoleEntityProxy; consoleEntity = consoleEntityProxy.ConsoleEntityProxy)
			{
			}
			if (consoleEntity == newFocus)
			{
				CurrentEntity?.SetFocused(value: false);
				newFocus.SetFocused(value: true);
				CurrentEntity = entity;
				SetFocusValue(CurrentEntity);
				m_Owner?.EntityFocused(CurrentEntity);
				return true;
			}
			if (consoleEntity is ConsoleNavigationBehaviour consoleNavigationBehaviour && consoleNavigationBehaviour.FocusOnEntity(newFocus))
			{
				CurrentEntity = entity;
				SetFocusValue(CurrentEntity);
				m_Owner?.EntityFocused(CurrentEntity);
				return true;
			}
		}
		return false;
	}

	public void SetFocus(bool value)
	{
		IsFocused = value;
		if (value)
		{
			FocusOnCurrentEntity();
		}
		else
		{
			UnFocusCurrentEntity();
		}
	}

	public bool FocusOnFirstValidEntity()
	{
		IConsoleEntity firstValidEntity = GetFirstValidEntity();
		return FocusOnEntity(firstValidEntity);
	}

	public bool FocusOnLastValidEntity()
	{
		IConsoleEntity lastValidEntity = GetLastValidEntity();
		return FocusOnEntity(lastValidEntity);
	}

	public void FocusOnEntityManual(IConsoleEntity entity, bool fullReset = true)
	{
		if (entity == null || !entity.IsValid())
		{
			ResetCurrentEntity();
			return;
		}
		if (fullReset)
		{
			UnFocusAllEntities();
		}
		FocusOnEntity(entity);
	}

	public bool IsValid()
	{
		return Entities.Any((IConsoleEntity entity) => entity.IsValid());
	}

	public Vector2 GetPosition()
	{
		if (!m_PositionForFloatNavigationHasBeenSet)
		{
			UIKitLogger.Error("You are using position, that has not been set");
		}
		return PositionForFloatNavigation;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return NeighboursForFloatNavigation;
	}

	public abstract void Clear();

	public void Dispose()
	{
	}
}
