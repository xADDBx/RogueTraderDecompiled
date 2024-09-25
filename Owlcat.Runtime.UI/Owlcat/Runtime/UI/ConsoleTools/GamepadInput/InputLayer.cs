using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UI.Dependencies;
using Rewired;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class InputLayer
{
	private struct BindUpdateInfo
	{
		public BindDescription Bind;

		public IReadOnlyReactiveProperty<bool> LayerBinded;
	}

	public string ContextName;

	private string m_CurrentGroup;

	private int m_NextBindId;

	public BoolReactiveProperty LayerBinded = new BoolReactiveProperty();

	protected bool m_CursorEnabled;

	public Action OnUnbind;

	public Action OnBind;

	public static readonly InputLayer Empty = new InputLayer
	{
		ContextName = "Empty Layer"
	};

	private readonly Dictionary<int, BindDescription> m_ActionsUsed = new Dictionary<int, BindDescription>();

	private readonly List<BindDescription> m_Binds = new List<BindDescription>();

	private static Func<bool> s_CanReceiveInputPredicate = () => true;

	private Dictionary<BindDescription, IDisposable> m_BindSubscriptions = new Dictionary<BindDescription, IDisposable>();

	private IDisposable m_UpdateCorutine;

	private List<BindUpdateInfo> m_Updates = new List<BindUpdateInfo>();

	public bool CursorEnabled
	{
		get
		{
			return m_CursorEnabled;
		}
		set
		{
			SetupCursor(value);
		}
	}

	protected virtual void SetupCursor(bool value)
	{
		m_CursorEnabled = value;
		GamePad.Instance.TryRestoreCursorState();
	}

	public static void SetCanReceiveInputPredicate(Func<bool> predicate)
	{
		s_CanReceiveInputPredicate = predicate;
	}

	public static bool CanReceiveInput()
	{
		return s_CanReceiveInputPredicate();
	}

	private void UseAction(int actionId, BindDescription bind)
	{
		if (!m_ActionsUsed.ContainsKey(actionId))
		{
			m_ActionsUsed.Add(actionId, bind);
		}
	}

	private void UnuseAction(int actionId)
	{
		if (m_ActionsUsed.ContainsKey(actionId))
		{
			m_ActionsUsed.Remove(actionId);
		}
	}

	public InputBindStruct AddButton(Action<InputActionEventData> handler, int actionId, IReadOnlyReactiveProperty<bool> enabled, InputActionEventType eventType = InputActionEventType.ButtonJustPressed)
	{
		if (eventType == InputActionEventType.ButtonJustLongPressed)
		{
			return AddLongPressButton(handler, actionId, enabled, eventType);
		}
		return AddButton(handler, new int[1] { actionId }, enabled, eventType);
	}

	public InputBindStruct AddButton(Action<InputActionEventData> handler, int actionId, InputActionEventType eventType = InputActionEventType.ButtonJustPressed, bool enableDefaultSound = true, bool avoidInputBlock = false)
	{
		if (eventType == InputActionEventType.ButtonJustLongPressed)
		{
			return AddLongPressButton(handler, actionId, eventType, avoidInputBlock);
		}
		return AddButton(handler, new int[1] { actionId }, eventType, avoidInputBlock);
	}

	public InputBindStruct AddButton(Action<InputActionEventData> handler, int[] actionIds, IReadOnlyReactiveProperty<bool> enabled, InputActionEventType eventType = InputActionEventType.ButtonJustPressed, bool avoidInputBlock = false)
	{
		if (actionIds.Length == 0)
		{
			return null;
		}
		if (GamePad.Instance.SwapButtonsForJapanese)
		{
			if (actionIds[0] == 8)
			{
				actionIds[0] = 9;
			}
			else if (actionIds[0] == 9)
			{
				actionIds[0] = 8;
			}
		}
		BindDescription bindDescription = GetBindDescription(actionIds.First(), eventType, InputAction, enabled, m_CurrentGroup, m_NextBindId, avoidInputBlock);
		AddBind(bindDescription);
		return new InputBindStruct(this, bindDescription);
		void InputAction(InputActionEventData eventData)
		{
			bool flag = true;
			int[] array = actionIds;
			foreach (int actionId in array)
			{
				switch (eventType)
				{
				case InputActionEventType.ButtonJustReleased:
					flag = flag && eventData.player.GetButtonUp(actionId) && !eventData.player.GetButtonLongPressUp(actionId);
					break;
				case InputActionEventType.ButtonJustLongPressed:
					flag = flag && (eventData.player.GetButtonLongPressDown(actionId) || eventData.player.GetButtonLongPress(actionId));
					break;
				}
			}
			if (flag)
			{
				handler(eventData);
			}
		}
	}

	public InputBindStruct AddButton(Action<InputActionEventData> handler, int[] actionIds, InputActionEventType eventType = InputActionEventType.ButtonJustPressed, bool avoidInputBlock = false)
	{
		if (actionIds.Length == 0)
		{
			return null;
		}
		return AddButton(handler, actionIds, null, eventType, avoidInputBlock);
	}

	private InputBindStruct AddLongPressButton(Action<InputActionEventData> handler, int actionId, IReadOnlyReactiveProperty<bool> enabled, InputActionEventType eventType, bool avoidInputBlock = false)
	{
		if (GamePad.Instance.SwapButtonsForJapanese)
		{
			if (actionId == 8)
			{
				actionId = 9;
			}
			else if (actionId == 9)
			{
				actionId = 8;
			}
		}
		InputActionEventType inputActionEventType = InputActionEventType.ButtonPressed;
		InputActionEventType inputActionEventType2 = InputActionEventType.ButtonJustPressed;
		InputActionEventType inputActionEventType3 = InputActionEventType.ButtonJustReleased;
		switch (eventType)
		{
		case InputActionEventType.ButtonLongPressed:
		case InputActionEventType.ButtonJustLongPressed:
		case InputActionEventType.ButtonLongPressJustReleased:
			inputActionEventType = InputActionEventType.ButtonPressed;
			inputActionEventType2 = InputActionEventType.ButtonJustPressed;
			inputActionEventType3 = InputActionEventType.ButtonJustReleased;
			break;
		case InputActionEventType.NegativeButtonLongPressed:
		case InputActionEventType.NegativeButtonJustLongPressed:
		case InputActionEventType.NegativeButtonLongPressJustReleased:
			inputActionEventType = InputActionEventType.NegativeButtonPressed;
			inputActionEventType2 = InputActionEventType.NegativeButtonJustPressed;
			inputActionEventType3 = InputActionEventType.NegativeButtonJustReleased;
			break;
		default:
			UIKitLogger.Log($"Cannot use AddLongPressButton with eventType={eventType}");
			return null;
		}
		int bindId = m_NextBindId++;
		BindDescription bindDescription = GetBindDescription(actionId, eventType, handler, enabled, m_CurrentGroup, bindId, avoidInputBlock);
		InputBindStruct bindStruct = new InputBindStruct(this, bindDescription)
		{
			IsLongPress = true
		};
		AddBind(bindDescription);
		BindDescription bindDescription2 = GetBindDescription(actionId, inputActionEventType, CallbackAction, enabled, m_CurrentGroup, bindId, avoidInputBlock);
		AddBind(bindDescription2);
		bindStruct.Binds.Add(bindDescription2);
		BindDescription bindDescription3 = GetBindDescription(actionId, inputActionEventType2, delegate
		{
			bindStruct.Percentage.Value = 0f;
		}, enabled, m_CurrentGroup, bindId, avoidInputBlock);
		AddBind(bindDescription3);
		bindStruct.Binds.Add(bindDescription3);
		BindDescription bindDescription4 = GetBindDescription(actionId, inputActionEventType3, delegate
		{
			bindStruct.Percentage.Value = 0f;
		}, enabled, m_CurrentGroup, bindId, avoidInputBlock);
		AddBind(bindDescription4);
		bindStruct.Binds.Add(bindDescription4);
		return bindStruct;
		void CallbackAction(InputActionEventData eventData)
		{
			int behaviorId = ReInput.mapping.Actions[actionId].behaviorId;
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(0, behaviorId);
			double buttonTimePressed = eventData.GetButtonTimePressed();
			float buttonLongPressTime = inputBehavior.buttonLongPressTime;
			float value = Mathf.Min((float)(buttonTimePressed / (double)buttonLongPressTime), 1f);
			bindStruct.Percentage.Value = value;
		}
	}

	public InputBindStruct AddLongPressButton(Action<InputActionEventData> handler, int actionId, InputActionEventType eventType, bool avoidInputBlock = false)
	{
		return AddLongPressButton(handler, actionId, null, eventType, avoidInputBlock);
	}

	public InputBindStruct AddAxis(Action<InputActionEventData, float> handler, int actionId, IReadOnlyReactiveProperty<bool> enabled, bool repeat = false)
	{
		List<BindDescription> list = new List<BindDescription>();
		int bindId = m_NextBindId++;
		Action<InputActionEventData> handler2 = delegate(InputActionEventData eventData)
		{
			handler(eventData, eventData.GetAxis());
		};
		if (repeat)
		{
			list.Add(GetBindDescription(actionId, InputActionEventType.ButtonRepeating, handler2, enabled, m_CurrentGroup, bindId));
			list.Add(GetBindDescription(actionId, InputActionEventType.NegativeButtonRepeating, handler2, enabled, m_CurrentGroup, bindId));
		}
		else
		{
			list.Add(GetBindDescription(actionId, InputActionEventType.AxisActiveOrJustInactive, handler2, enabled, m_CurrentGroup, bindId));
		}
		list.ForEach(AddBind);
		return new InputBindStruct(this, list);
	}

	public InputBindStruct AddAxis(Action<InputActionEventData, float> handler, int actionId, bool repeat = false)
	{
		return AddAxis(handler, actionId, null, repeat);
	}

	public InputBindStruct AddAxis2D(Action<InputActionEventData, Vector2> handler, int actionIdX, int actionIdY, bool repeat = false)
	{
		return AddAxis2D(handler, actionIdX, actionIdY, null, repeat);
	}

	public InputBindStruct AddAxis2D(Action<InputActionEventData, Vector2> handler, int actionIdX, int actionIdY, IReadOnlyReactiveProperty<bool> enabled = null, bool repeat = false)
	{
		List<BindDescription> list2 = new List<BindDescription>();
		int bindId = m_NextBindId++;
		ReactiveCommand<InputActionEventData> a;
		if (repeat)
		{
			a = new ReactiveCommand<InputActionEventData>();
			a.Buffer(a.ThrottleFrame(1)).Subscribe(delegate(IList<InputActionEventData> list)
			{
				InputActionEventData arg = list.FirstOrDefault();
				handler(arg, arg.player.GetAxis2D(actionIdX, actionIdY));
			});
			list2.Add(GetBindDescription(actionIdX, InputActionEventType.ButtonRepeating, Action, enabled, m_CurrentGroup, bindId));
			list2.Add(GetBindDescription(actionIdX, InputActionEventType.NegativeButtonRepeating, Action, enabled, m_CurrentGroup, bindId));
			list2.Add(GetBindDescription(actionIdY, InputActionEventType.ButtonRepeating, Action, enabled, m_CurrentGroup, bindId));
			list2.Add(GetBindDescription(actionIdY, InputActionEventType.NegativeButtonRepeating, Action, enabled, m_CurrentGroup, bindId));
		}
		else
		{
			list2.Add(GetBindDescription(actionIdX, InputActionEventType.AxisActiveOrJustInactive, Action, enabled, m_CurrentGroup, bindId));
			list2.Add(GetBindDescription(actionIdY, InputActionEventType.AxisActiveOrJustInactive, Action, enabled, m_CurrentGroup, bindId));
		}
		list2.ForEach(AddBind);
		return new InputBindStruct(this, list2);
		void Action(InputActionEventData eventData)
		{
			a.Execute(eventData);
		}
		void Action(InputActionEventData eventData)
		{
			handler(eventData, eventData.player.GetAxis2D(actionIdX, actionIdY));
		}
	}

	public InputBindStruct AddCursor(Action<InputActionEventData, Vector2> handler, int actionIdX, int actionIdY, IReadOnlyReactiveProperty<bool> enabled = null)
	{
		Action<InputActionEventData> handler2 = delegate(InputActionEventData eventData)
		{
			handler(eventData, eventData.player.GetAxis2D(actionIdX, actionIdY));
		};
		Action<InputActionEventData> handler3 = delegate(InputActionEventData eventData)
		{
			if (eventData.player.GetAxisTimeActive(actionIdX).CompareTo(0.0) != 0)
			{
				handler(eventData, eventData.player.GetAxis2D(actionIdX, actionIdY));
			}
		};
		int bindId = m_NextBindId++;
		List<BindDescription> list = new List<BindDescription>();
		list.Add(GetBindDescription(actionIdX, InputActionEventType.AxisActiveOrJustInactive, handler2, enabled, m_CurrentGroup, bindId));
		list.Add(GetBindDescription(actionIdY, InputActionEventType.AxisActiveOrJustInactive, handler3, enabled, m_CurrentGroup, bindId));
		list.ForEach(AddBind);
		return new InputBindStruct(this, list);
	}

	public InputBindStruct AddCursorWithTriggerZone(Action<InputActionEventData, Vector2> handler, int actionIdX, int actionIdY, float triggerZone, IReadOnlyReactiveProperty<bool> enabled = null)
	{
		bool triggered = false;
		Action<InputActionEventData> checkTriggerAndInvokeHandler = delegate(InputActionEventData eventData)
		{
			Vector2 axis2D = eventData.player.GetAxis2D(actionIdX, actionIdY);
			float magnitude = eventData.player.GetAxis2DPrev(actionIdX, actionIdY).magnitude;
			float magnitude2 = axis2D.magnitude;
			if (triggered && magnitude2 < triggerZone)
			{
				handler(eventData, axis2D);
				triggered = false;
			}
			else if (!triggered && magnitude2 > triggerZone && magnitude < triggerZone)
			{
				triggered = true;
			}
			if (triggered)
			{
				handler(eventData, axis2D);
			}
		};
		Action<InputActionEventData> handler2 = delegate(InputActionEventData eventData)
		{
			checkTriggerAndInvokeHandler(eventData);
		};
		Action<InputActionEventData> handler3 = delegate(InputActionEventData eventData)
		{
			if (eventData.player.GetAxisTimeActive(actionIdX).CompareTo(0.0) != 0)
			{
				checkTriggerAndInvokeHandler(eventData);
			}
		};
		int bindId = m_NextBindId++;
		List<BindDescription> list = new List<BindDescription>();
		list.Add(GetBindDescription(actionIdX, InputActionEventType.AxisActiveOrJustInactive, handler2, enabled, m_CurrentGroup, bindId));
		list.Add(GetBindDescription(actionIdY, InputActionEventType.AxisActiveOrJustInactive, handler3, enabled, m_CurrentGroup, bindId));
		list.ForEach(AddBind);
		return new InputBindStruct(this, list);
	}

	internal void RemoveBinds(List<BindDescription> bindDescriptions)
	{
		foreach (BindDescription bindDescription in bindDescriptions)
		{
			RemoveBind(bindDescription);
		}
	}

	private void RemoveBind(BindDescription bindDescription)
	{
		Bind(bindDescription, enable: false);
		if (m_Binds.Contains(bindDescription))
		{
			m_Binds.Remove(bindDescription);
		}
		if (m_BindSubscriptions.ContainsKey(bindDescription))
		{
			m_BindSubscriptions[bindDescription].Dispose();
			m_BindSubscriptions.Remove(bindDescription);
		}
		m_Updates.RemoveAll((BindUpdateInfo updateInfo) => updateInfo.Bind == bindDescription);
	}

	private void AddBind(BindDescription bind)
	{
		m_Binds.Add(bind);
		if (!LayerBinded.Value)
		{
			return;
		}
		if (bind.Enabled == null)
		{
			Bind(bind, enable: true);
			return;
		}
		m_BindSubscriptions.Add(bind, bind.Enabled.Subscribe(delegate
		{
			QueueBind(bind, LayerBinded);
		}));
	}

	private bool CheckAction(BindDescription bind)
	{
		if (m_ActionsUsed.ContainsKey(bind.ActionId))
		{
			int actionId = bind.ActionId;
			if (m_ActionsUsed[actionId].BindId == bind.BindId)
			{
				return true;
			}
			if (m_ActionsUsed[actionId].EventType != bind.EventType)
			{
				return true;
			}
			UIKitLogger.Error(ReInput.mapping.Actions[actionId].descriptiveName + " already used in layer '" + ContextName + "'");
			return false;
		}
		return true;
	}

	public static InputLayer FromView(MonoBehaviour view)
	{
		InputLayer inputLayer = new InputLayer
		{
			ContextName = view.name
		};
		IControl[] components = view.GetComponents<IControl>();
		foreach (IControl control in components)
		{
			if (control is ControlAxis)
			{
				ControlAxis controlAxis = control as ControlAxis;
				inputLayer.AddAxis(controlAxis.Handler.Action, controlAxis.AxisId);
			}
			else if (control is ControlCursor)
			{
				ControlCursor controlCursor = control as ControlCursor;
				inputLayer.AddCursor(controlCursor.Handler.Action, controlCursor.AxisIdX, controlCursor.AxisIdY);
			}
			else if (control is ControlButton)
			{
				ControlButton controlButton = control as ControlButton;
				inputLayer.AddButton(controlButton.Handler.Action, controlButton.ActionIds, InputActionEventType.ButtonJustReleased);
			}
		}
		return inputLayer;
	}

	public void Bind()
	{
		if (LayerBinded.Value)
		{
			UIKitLogger.Warning("Layer '" + ContextName + "' already binded");
			return;
		}
		foreach (BindDescription bind in m_Binds)
		{
			if (bind.Enabled == null)
			{
				Bind(bind, enable: true);
				continue;
			}
			m_BindSubscriptions.Add(bind, bind.Enabled.Subscribe(delegate
			{
				QueueBind(bind, LayerBinded);
			}));
		}
		LayerBinded.Value = true;
	}

	private void UpdateBinds(long l)
	{
		foreach (BindUpdateInfo update in m_Updates)
		{
			if (!update.Bind.Enabled.Value || !update.LayerBinded.Value)
			{
				Bind(update.Bind, enable: false);
			}
		}
		foreach (BindUpdateInfo update2 in m_Updates)
		{
			if (update2.Bind.Enabled.Value && update2.LayerBinded.Value)
			{
				Bind(update2.Bind, enable: true);
			}
		}
		m_Updates.Clear();
		m_UpdateCorutine.Dispose();
		m_UpdateCorutine = null;
	}

	private void QueueBind(BindDescription bind, IReadOnlyReactiveProperty<bool> layerBinded)
	{
		m_Updates.Add(new BindUpdateInfo
		{
			Bind = bind,
			LayerBinded = layerBinded
		});
		if (m_UpdateCorutine == null)
		{
			m_UpdateCorutine = Observable.TimerFrame(0, FrameCountType.EndOfFrame).Subscribe(UpdateBinds);
		}
	}

	public void Unbind()
	{
		if (!LayerBinded.Value)
		{
			UIKitLogger.Warning("Layer '" + ContextName + "' already unbinded");
			return;
		}
		foreach (BindDescription bind in m_Binds)
		{
			Bind(bind, enable: false);
		}
		LayerBinded.Value = false;
		foreach (KeyValuePair<BindDescription, IDisposable> bindSubscription in m_BindSubscriptions)
		{
			bindSubscription.Value.Dispose();
		}
		m_BindSubscriptions.Clear();
		m_Updates.Clear();
		m_UpdateCorutine?.Dispose();
		m_UpdateCorutine = null;
		OnUnbind?.Invoke();
	}

	private void Bind(BindDescription bind, bool enable)
	{
		string descriptiveName = ReInput.mapping.Actions[bind.ActionId].descriptiveName;
		if (enable && !bind.Bound)
		{
			if (bind.Debug)
			{
				UIKitLogger.Log($"Binding {descriptiveName} in layer '{ContextName}' in group {bind.Group}, bindId={bind.BindId}");
			}
			if (GamePad.Instance.Player == null)
			{
				UIKitLogger.Log("GamePad.Instance.Player is NULL");
				return;
			}
			GamePad.Instance.Player.AddInputEventDelegate(bind.Handler, UpdateLoopType.Update, bind.EventType, bind.ActionId);
			UseAction(bind.ActionId, bind);
			bind.Bound = true;
			OnBind?.Invoke();
		}
		else if (!enable && bind.Bound)
		{
			if (bind.Debug)
			{
				UIKitLogger.Log($"Unbinding {descriptiveName} in layer '{ContextName}' in group {bind.Group}, bindId={bind.BindId}");
			}
			if (GamePad.Instance.Player == null)
			{
				UIKitLogger.Log("GamePad.Instance.Player is NULL");
				return;
			}
			GamePad.Instance.Player.RemoveInputEventDelegate(bind.Handler, UpdateLoopType.Update, bind.EventType, bind.ActionId);
			UnuseAction(bind.ActionId);
			bind.Bound = false;
		}
	}

	private BindDescription GetBindDescription(int actionId, InputActionEventType eventType, Action<InputActionEventData> handler, IReadOnlyReactiveProperty<bool> enabled, string group, int bindId, bool avoidInputBlock = false)
	{
		enabled = InputEnabledScope.GetInputEnabledProperty(enabled);
		return new BindDescription
		{
			ActionId = actionId,
			EventType = eventType,
			ActionHandler = PermittedHandler,
			Enabled = enabled,
			Group = group,
			BindId = bindId
		};
		void PermittedHandler(InputActionEventData evt)
		{
			if (avoidInputBlock || CanReceiveInput())
			{
				handler(evt);
			}
		}
	}

	public void MoveCursorToTransform(Transform transform)
	{
		if (!(transform == null) && CursorEnabled && UIKitRewiredCursorController.HasController)
		{
			UIKitRewiredCursorController.Cursor.transform.position = transform.position;
		}
	}

	public void SetCurrentGroup(string groupName)
	{
		m_CurrentGroup = groupName;
	}
}
