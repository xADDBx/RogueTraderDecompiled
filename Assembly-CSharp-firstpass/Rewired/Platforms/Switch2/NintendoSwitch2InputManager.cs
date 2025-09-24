using System;
using System.Collections.Generic;
using Rewired.Utils.Interfaces;
using UnityEngine;

namespace Rewired.Platforms.Switch2;

[AddComponentMenu("Rewired/Nintendo Switch 2 Input Manager")]
[RequireComponent(typeof(InputManager))]
public sealed class NintendoSwitch2InputManager : MonoBehaviour, IExternalInputManager
{
	[Serializable]
	private class UserData : IKeyedData<int>
	{
		private const int defaultAllowedNpadStyles = 31;

		[SerializeField]
		private int _allowedNpadStyles = 31;

		[SerializeField]
		private int _joyConGripStyle = 1;

		[SerializeField]
		private bool _adjustIMUsForGripStyle = true;

		[SerializeField]
		private int _handheldActivationMode;

		[SerializeField]
		private bool _assignJoysticksByNpadId = true;

		[SerializeField]
		private bool _useVibrationThread = true;

		[SerializeField]
		private bool _autoStartIMUs = true;

		[SerializeField]
		private bool _autoStartJoyConMouseSensors;

		[SerializeField]
		private bool _allowJoyConMouseRebindPolling;

		[SerializeField]
		private bool _initializeJcms;

		[SerializeField]
		private NpadSettings_Internal _npadNo1 = new NpadSettings_Internal(0);

		[SerializeField]
		private NpadSettings_Internal _npadNo2 = new NpadSettings_Internal(1);

		[SerializeField]
		private NpadSettings_Internal _npadNo3 = new NpadSettings_Internal(2);

		[SerializeField]
		private NpadSettings_Internal _npadNo4 = new NpadSettings_Internal(3);

		[SerializeField]
		private NpadSettings_Internal _npadNo5 = new NpadSettings_Internal(4);

		[SerializeField]
		private NpadSettings_Internal _npadNo6 = new NpadSettings_Internal(5);

		[SerializeField]
		private NpadSettings_Internal _npadNo7 = new NpadSettings_Internal(6);

		[SerializeField]
		private NpadSettings_Internal _npadNo8 = new NpadSettings_Internal(7);

		[SerializeField]
		private NpadSettings_Internal _npadHandheld = new NpadSettings_Internal(0);

		[SerializeField]
		private DebugPadSettings_Internal _debugPad = new DebugPadSettings_Internal(0);

		private Dictionary<int, object[]> __delegates;

		public int allowedNpadStyles
		{
			get
			{
				return _allowedNpadStyles;
			}
			set
			{
				_allowedNpadStyles = value;
			}
		}

		public int joyConGripStyle
		{
			get
			{
				return _joyConGripStyle;
			}
			set
			{
				_joyConGripStyle = value;
			}
		}

		public bool adjustIMUsForGripStyle
		{
			get
			{
				return _adjustIMUsForGripStyle;
			}
			set
			{
				_adjustIMUsForGripStyle = value;
			}
		}

		public int handheldActivationMode
		{
			get
			{
				return _handheldActivationMode;
			}
			set
			{
				_handheldActivationMode = value;
			}
		}

		public bool assignJoysticksByNpadId
		{
			get
			{
				return _assignJoysticksByNpadId;
			}
			set
			{
				_assignJoysticksByNpadId = value;
			}
		}

		public bool useVibrationThread
		{
			get
			{
				return _useVibrationThread;
			}
			set
			{
				_useVibrationThread = value;
			}
		}

		public bool autoStartIMUs
		{
			get
			{
				return _autoStartIMUs;
			}
			set
			{
				_autoStartIMUs = value;
			}
		}

		public bool autoStartJoyConMouseSensors
		{
			get
			{
				return _autoStartJoyConMouseSensors;
			}
			set
			{
				_autoStartJoyConMouseSensors = value;
			}
		}

		public bool allowJoyConMouseRebindPolling
		{
			get
			{
				return _allowJoyConMouseRebindPolling;
			}
			set
			{
				_allowJoyConMouseRebindPolling = value;
			}
		}

		private NpadSettings_Internal npadNo1 => _npadNo1;

		private NpadSettings_Internal npadNo2 => _npadNo2;

		private NpadSettings_Internal npadNo3 => _npadNo3;

		private NpadSettings_Internal npadNo4 => _npadNo4;

		private NpadSettings_Internal npadNo5 => _npadNo5;

		private NpadSettings_Internal npadNo6 => _npadNo6;

		private NpadSettings_Internal npadNo7 => _npadNo7;

		private NpadSettings_Internal npadNo8 => _npadNo8;

		private NpadSettings_Internal npadHandheld => _npadHandheld;

		public DebugPadSettings_Internal debugPad => _debugPad;

		private Dictionary<int, object[]> delegates
		{
			get
			{
				if (__delegates != null)
				{
					return __delegates;
				}
				return __delegates = new Dictionary<int, object[]>
				{
					{
						0,
						new object[2]
						{
							(Func<int>)(() => allowedNpadStyles),
							(Action<int>)delegate(int x)
							{
								allowedNpadStyles = x;
							}
						}
					},
					{
						1,
						new object[2]
						{
							(Func<int>)(() => joyConGripStyle),
							(Action<int>)delegate(int x)
							{
								joyConGripStyle = x;
							}
						}
					},
					{
						2,
						new object[2]
						{
							(Func<bool>)(() => adjustIMUsForGripStyle),
							(Action<bool>)delegate(bool x)
							{
								adjustIMUsForGripStyle = x;
							}
						}
					},
					{
						3,
						new object[2]
						{
							(Func<int>)(() => handheldActivationMode),
							(Action<int>)delegate(int x)
							{
								handheldActivationMode = x;
							}
						}
					},
					{
						4,
						new object[2]
						{
							(Func<bool>)(() => assignJoysticksByNpadId),
							(Action<bool>)delegate(bool x)
							{
								assignJoysticksByNpadId = x;
							}
						}
					},
					{
						5,
						new object[2]
						{
							(Func<object>)(() => npadNo1),
							null
						}
					},
					{
						6,
						new object[2]
						{
							(Func<object>)(() => npadNo2),
							null
						}
					},
					{
						7,
						new object[2]
						{
							(Func<object>)(() => npadNo3),
							null
						}
					},
					{
						8,
						new object[2]
						{
							(Func<object>)(() => npadNo4),
							null
						}
					},
					{
						9,
						new object[2]
						{
							(Func<object>)(() => npadNo5),
							null
						}
					},
					{
						10,
						new object[2]
						{
							(Func<object>)(() => npadNo6),
							null
						}
					},
					{
						11,
						new object[2]
						{
							(Func<object>)(() => npadNo7),
							null
						}
					},
					{
						12,
						new object[2]
						{
							(Func<object>)(() => npadNo8),
							null
						}
					},
					{
						13,
						new object[2]
						{
							(Func<object>)(() => npadHandheld),
							null
						}
					},
					{
						14,
						new object[2]
						{
							(Func<object>)(() => debugPad),
							null
						}
					},
					{
						15,
						new object[2]
						{
							(Func<bool>)(() => useVibrationThread),
							(Action<bool>)delegate(bool x)
							{
								useVibrationThread = x;
							}
						}
					},
					{
						16,
						new object[2]
						{
							(Func<bool>)(() => autoStartIMUs),
							(Action<bool>)delegate(bool x)
							{
								autoStartIMUs = x;
							}
						}
					},
					{
						17,
						new object[2]
						{
							(Func<bool>)(() => autoStartJoyConMouseSensors),
							(Action<bool>)delegate(bool x)
							{
								autoStartJoyConMouseSensors = x;
							}
						}
					},
					{
						18,
						new object[2]
						{
							(Func<bool>)(() => allowJoyConMouseRebindPolling),
							(Action<bool>)delegate(bool x)
							{
								allowJoyConMouseRebindPolling = x;
							}
						}
					},
					{
						19,
						new object[2]
						{
							(Func<bool>)(() => _initializeJcms),
							(Action<bool>)delegate(bool x)
							{
								_initializeJcms = x;
							}
						}
					}
				};
			}
		}

		bool IKeyedData<int>.TryGetValue<T>(int key, out T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				value = default(T);
				return false;
			}
			if (!(value2[0] is Func<T> func))
			{
				value = default(T);
				return false;
			}
			value = func();
			return true;
		}

		bool IKeyedData<int>.TrySetValue<T>(int key, T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (!(value2[1] is Action<T> action))
			{
				return false;
			}
			action(value);
			return true;
		}
	}

	[Serializable]
	private sealed class NpadSettings_Internal : IKeyedData<int>
	{
		[Tooltip("Determines whether this Npad id is allowed to be used by the system.")]
		[SerializeField]
		private bool _isAllowed = true;

		[Tooltip("The Rewired Player Id assigned to this Npad id.")]
		[SerializeField]
		private int _rewiredPlayerId;

		[Tooltip("Determines how Joy-Cons should be handled.\n\nUnmodified: Joy-Con assignment mode will be left at the system default.\nDual: Joy-Cons pairs are handled as a single controller.\nSingle: Joy-Cons are handled as individual controllers.")]
		[SerializeField]
		private int _joyConAssignmentMode = -1;

		private Dictionary<int, object[]> __delegates;

		private bool isAllowed
		{
			get
			{
				return _isAllowed;
			}
			set
			{
				_isAllowed = value;
			}
		}

		private int rewiredPlayerId
		{
			get
			{
				return _rewiredPlayerId;
			}
			set
			{
				_rewiredPlayerId = value;
			}
		}

		private int joyConAssignmentMode
		{
			get
			{
				return _joyConAssignmentMode;
			}
			set
			{
				_joyConAssignmentMode = value;
			}
		}

		private Dictionary<int, object[]> delegates
		{
			get
			{
				if (__delegates != null)
				{
					return __delegates;
				}
				return __delegates = new Dictionary<int, object[]>
				{
					{
						0,
						new object[2]
						{
							(Func<bool>)(() => isAllowed),
							(Action<bool>)delegate(bool x)
							{
								isAllowed = x;
							}
						}
					},
					{
						1,
						new object[2]
						{
							(Func<int>)(() => rewiredPlayerId),
							(Action<int>)delegate(int x)
							{
								rewiredPlayerId = x;
							}
						}
					},
					{
						2,
						new object[2]
						{
							(Func<int>)(() => joyConAssignmentMode),
							(Action<int>)delegate(int x)
							{
								joyConAssignmentMode = x;
							}
						}
					}
				};
			}
		}

		internal NpadSettings_Internal(int playerId)
		{
			_rewiredPlayerId = playerId;
		}

		bool IKeyedData<int>.TryGetValue<T>(int key, out T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				value = default(T);
				return false;
			}
			if (!(value2[0] is Func<T> func))
			{
				value = default(T);
				return false;
			}
			value = func();
			return true;
		}

		bool IKeyedData<int>.TrySetValue<T>(int key, T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (!(value2[1] is Action<T> action))
			{
				return false;
			}
			action(value);
			return true;
		}
	}

	[Serializable]
	private sealed class DebugPadSettings_Internal : IKeyedData<int>
	{
		[Tooltip("Determines whether the Debug Pad will be enabled.")]
		[SerializeField]
		private bool _enabled;

		[Tooltip("The Rewired Player Id to which the Debug Pad will be assigned.")]
		[SerializeField]
		private int _rewiredPlayerId;

		private Dictionary<int, object[]> __delegates;

		private int rewiredPlayerId
		{
			get
			{
				return _rewiredPlayerId;
			}
			set
			{
				_rewiredPlayerId = value;
			}
		}

		private bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		private Dictionary<int, object[]> delegates
		{
			get
			{
				if (__delegates != null)
				{
					return __delegates;
				}
				return __delegates = new Dictionary<int, object[]>
				{
					{
						0,
						new object[2]
						{
							(Func<bool>)(() => enabled),
							(Action<bool>)delegate(bool x)
							{
								enabled = x;
							}
						}
					},
					{
						1,
						new object[2]
						{
							(Func<int>)(() => rewiredPlayerId),
							(Action<int>)delegate(int x)
							{
								rewiredPlayerId = x;
							}
						}
					}
				};
			}
		}

		internal DebugPadSettings_Internal(int playerId)
		{
			_rewiredPlayerId = playerId;
		}

		bool IKeyedData<int>.TryGetValue<T>(int key, out T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				value = default(T);
				return false;
			}
			if (!(value2[0] is Func<T> func))
			{
				value = default(T);
				return false;
			}
			value = func();
			return true;
		}

		bool IKeyedData<int>.TrySetValue<T>(int key, T value)
		{
			if (!delegates.TryGetValue(key, out var value2))
			{
				return false;
			}
			if (!(value2[1] is Action<T> action))
			{
				return false;
			}
			action(value);
			return true;
		}
	}

	[SerializeField]
	private UserData _userData = new UserData();

	object IExternalInputManager.Initialize(Platform platform, object configVars)
	{
		return null;
	}

	void IExternalInputManager.Deinitialize()
	{
	}
}
