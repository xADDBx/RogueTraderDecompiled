using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.UI.ControlMapper;

[Serializable]
public class LanguageData : LanguageDataBase
{
	[Serializable]
	protected class CustomEntry
	{
		public string key;

		public string value;

		public CustomEntry()
		{
		}

		public CustomEntry(string key, string value)
		{
			this.key = key;
			this.value = value;
		}

		public static Dictionary<string, string> ToDictionary(CustomEntry[] array)
		{
			if (array == null)
			{
				return new Dictionary<string, string>();
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && !string.IsNullOrEmpty(array[i].key) && !string.IsNullOrEmpty(array[i].value))
				{
					if (dictionary.ContainsKey(array[i].key))
					{
						Debug.LogError("Key \"" + array[i].key + "\" is already in dictionary!");
					}
					else
					{
						dictionary.Add(array[i].key, array[i].value);
					}
				}
			}
			return dictionary;
		}
	}

	[Serializable]
	protected class ModifierKeys
	{
		public string control = "Control";

		public string alt = "Alt";

		public string shift = "Shift";

		public string command = "Command";

		public string separator = " + ";
	}

	[SerializeField]
	private string _yes = "Yes";

	[SerializeField]
	private string _no = "No";

	[SerializeField]
	private string _add = "Add";

	[SerializeField]
	private string _replace = "Replace";

	[SerializeField]
	private string _remove = "Remove";

	[SerializeField]
	private string _swap = "Swap";

	[SerializeField]
	private string _cancel = "Cancel";

	[SerializeField]
	private string _none = "None";

	[SerializeField]
	private string _okay = "Okay";

	[SerializeField]
	private string _done = "Done";

	[SerializeField]
	private string _default = "Default";

	[SerializeField]
	private string _assignControllerWindowTitle = "Choose Controller";

	[SerializeField]
	private string _assignControllerWindowMessage = "Press any button or move an axis on the controller you would like to use.";

	[SerializeField]
	private string _controllerAssignmentConflictWindowTitle = "Controller Assignment";

	[SerializeField]
	[Tooltip("{0} = Joystick Name\n{1} = Other Player Name\n{2} = This Player Name")]
	private string _controllerAssignmentConflictWindowMessage = "{0} is already assigned to {1}. Do you want to assign this controller to {2} instead?";

	[SerializeField]
	private string _elementAssignmentPrePollingWindowMessage = "First center or zero all sticks and axes and press any button or wait for the timer to finish.";

	[SerializeField]
	[Tooltip("{0} = Action Name")]
	private string _joystickElementAssignmentPollingWindowMessage = "Now press a button or move an axis to assign it to {0}.";

	[SerializeField]
	[Tooltip("This text is only displayed when split-axis fields have been disabled and the user clicks on the full-axis field. Button/key/D-pad input cannot be assigned to a full-axis field.\n{0} = Action Name")]
	private string _joystickElementAssignmentPollingWindowMessage_fullAxisFieldOnly = "Now move an axis to assign it to {0}.";

	[SerializeField]
	[Tooltip("{0} = Action Name")]
	private string _keyboardElementAssignmentPollingWindowMessage = "Press a key to assign it to {0}. Modifier keys may also be used. To assign a modifier key alone, hold it down for 1 second.";

	[SerializeField]
	[Tooltip("{0} = Action Name")]
	private string _mouseElementAssignmentPollingWindowMessage = "Press a mouse button or move an axis to assign it to {0}.";

	[SerializeField]
	[Tooltip("This text is only displayed when split-axis fields have been disabled and the user clicks on the full-axis field. Button/key/D-pad input cannot be assigned to a full-axis field.\n{0} = Action Name")]
	private string _mouseElementAssignmentPollingWindowMessage_fullAxisFieldOnly = "Move an axis to assign it to {0}.";

	[SerializeField]
	private string _elementAssignmentConflictWindowMessage = "Assignment Conflict";

	[SerializeField]
	[Tooltip("{0} = Element Name")]
	private string _elementAlreadyInUseBlocked = "{0} is already in use cannot be replaced.";

	[SerializeField]
	[Tooltip("{0} = Element Name")]
	private string _elementAlreadyInUseCanReplace = "{0} is already in use. Do you want to replace it?";

	[SerializeField]
	[Tooltip("{0} = Element Name")]
	private string _elementAlreadyInUseCanReplace_conflictAllowed = "{0} is already in use. Do you want to replace it? You may also choose to add the assignment anyway.";

	[SerializeField]
	private string _mouseAssignmentConflictWindowTitle = "Mouse Assignment";

	[SerializeField]
	[Tooltip("{0} = Other Player Name\n{1} = This Player Name")]
	private string _mouseAssignmentConflictWindowMessage = "The mouse is already assigned to {0}. Do you want to assign the mouse to {1} instead?";

	[SerializeField]
	private string _calibrateControllerWindowTitle = "Calibrate Controller";

	[SerializeField]
	private string _calibrateAxisStep1WindowTitle = "Calibrate Zero";

	[SerializeField]
	[Tooltip("{0} = Axis Name")]
	private string _calibrateAxisStep1WindowMessage = "Center or zero {0} and press any button or wait for the timer to finish.";

	[SerializeField]
	private string _calibrateAxisStep2WindowTitle = "Calibrate Range";

	[SerializeField]
	[Tooltip("{0} = Axis Name")]
	private string _calibrateAxisStep2WindowMessage = "Move {0} through its entire range then press any button or wait for the timer to finish.";

	[SerializeField]
	private string _inputBehaviorSettingsWindowTitle = "Sensitivity Settings";

	[SerializeField]
	private string _restoreDefaultsWindowTitle = "Restore Defaults";

	[SerializeField]
	[Tooltip("Message for a single player game.")]
	private string _restoreDefaultsWindowMessage_onePlayer = "This will restore the default input configuration. Are you sure you want to do this?";

	[SerializeField]
	[Tooltip("Message for a multi-player game.")]
	private string _restoreDefaultsWindowMessage_multiPlayer = "This will restore the default input configuration for all players. Are you sure you want to do this?";

	[SerializeField]
	private string _actionColumnLabel = "Actions";

	[SerializeField]
	private string _keyboardColumnLabel = "Keyboard";

	[SerializeField]
	private string _mouseColumnLabel = "Mouse";

	[SerializeField]
	private string _controllerColumnLabel = "Controller";

	[SerializeField]
	private string _removeControllerButtonLabel = "Remove";

	[SerializeField]
	private string _calibrateControllerButtonLabel = "Calibrate";

	[SerializeField]
	private string _assignControllerButtonLabel = "Assign Controller";

	[SerializeField]
	private string _inputBehaviorSettingsButtonLabel = "Sensitivity";

	[SerializeField]
	private string _doneButtonLabel = "Done";

	[SerializeField]
	private string _restoreDefaultsButtonLabel = "Restore Defaults";

	[SerializeField]
	private string _playersGroupLabel = "Players:";

	[SerializeField]
	private string _controllerSettingsGroupLabel = "Controller:";

	[SerializeField]
	private string _assignedControllersGroupLabel = "Assigned Controllers:";

	[SerializeField]
	private string _settingsGroupLabel = "Settings:";

	[SerializeField]
	private string _mapCategoriesGroupLabel = "Categories:";

	[SerializeField]
	private string _calibrateWindow_deadZoneSliderLabel = "Dead Zone:";

	[SerializeField]
	private string _calibrateWindow_zeroSliderLabel = "Zero:";

	[SerializeField]
	private string _calibrateWindow_sensitivitySliderLabel = "Sensitivity:";

	[SerializeField]
	private string _calibrateWindow_invertToggleLabel = "Invert";

	[SerializeField]
	private string _calibrateWindow_calibrateButtonLabel = "Calibrate";

	[SerializeField]
	private ModifierKeys _modifierKeys;

	[SerializeField]
	private CustomEntry[] _customEntries;

	private bool _initialized;

	private Dictionary<string, string> customDict;

	public override string yes => _yes;

	public override string no => _no;

	public override string add => _add;

	public override string replace => _replace;

	public override string remove => _remove;

	public override string swap => _swap;

	public override string cancel => _cancel;

	public override string none => _none;

	public override string okay => _okay;

	public override string done => _done;

	public override string default_ => _default;

	public override string assignControllerWindowTitle => _assignControllerWindowTitle;

	public override string assignControllerWindowMessage => _assignControllerWindowMessage;

	public override string controllerAssignmentConflictWindowTitle => _controllerAssignmentConflictWindowTitle;

	public override string elementAssignmentPrePollingWindowMessage => _elementAssignmentPrePollingWindowMessage;

	public override string elementAssignmentConflictWindowMessage => _elementAssignmentConflictWindowMessage;

	public override string mouseAssignmentConflictWindowTitle => _mouseAssignmentConflictWindowTitle;

	public override string calibrateControllerWindowTitle => _calibrateControllerWindowTitle;

	public override string calibrateAxisStep1WindowTitle => _calibrateAxisStep1WindowTitle;

	public override string calibrateAxisStep2WindowTitle => _calibrateAxisStep2WindowTitle;

	public override string inputBehaviorSettingsWindowTitle => _inputBehaviorSettingsWindowTitle;

	public override string restoreDefaultsWindowTitle => _restoreDefaultsWindowTitle;

	public override string actionColumnLabel => _actionColumnLabel;

	public override string keyboardColumnLabel => _keyboardColumnLabel;

	public override string mouseColumnLabel => _mouseColumnLabel;

	public override string controllerColumnLabel => _controllerColumnLabel;

	public override string removeControllerButtonLabel => _removeControllerButtonLabel;

	public override string calibrateControllerButtonLabel => _calibrateControllerButtonLabel;

	public override string assignControllerButtonLabel => _assignControllerButtonLabel;

	public override string inputBehaviorSettingsButtonLabel => _inputBehaviorSettingsButtonLabel;

	public override string doneButtonLabel => _doneButtonLabel;

	public override string restoreDefaultsButtonLabel => _restoreDefaultsButtonLabel;

	public override string controllerSettingsGroupLabel => _controllerSettingsGroupLabel;

	public override string playersGroupLabel => _playersGroupLabel;

	public override string assignedControllersGroupLabel => _assignedControllersGroupLabel;

	public override string settingsGroupLabel => _settingsGroupLabel;

	public override string mapCategoriesGroupLabel => _mapCategoriesGroupLabel;

	public override string restoreDefaultsWindowMessage
	{
		get
		{
			if (ReInput.players.playerCount > 1)
			{
				return _restoreDefaultsWindowMessage_multiPlayer;
			}
			return _restoreDefaultsWindowMessage_onePlayer;
		}
	}

	public override string calibrateWindow_deadZoneSliderLabel => _calibrateWindow_deadZoneSliderLabel;

	public override string calibrateWindow_zeroSliderLabel => _calibrateWindow_zeroSliderLabel;

	public override string calibrateWindow_sensitivitySliderLabel => _calibrateWindow_sensitivitySliderLabel;

	public override string calibrateWindow_invertToggleLabel => _calibrateWindow_invertToggleLabel;

	public override string calibrateWindow_calibrateButtonLabel => _calibrateWindow_calibrateButtonLabel;

	public override void Initialize()
	{
		if (!_initialized)
		{
			customDict = CustomEntry.ToDictionary(_customEntries);
			_initialized = true;
		}
	}

	public override string GetCustomEntry(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		if (!customDict.TryGetValue(key, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	public override bool ContainsCustomEntryKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		return customDict.ContainsKey(key);
	}

	public override string GetControllerAssignmentConflictWindowMessage(string joystickName, string otherPlayerName, string currentPlayerName)
	{
		return string.Format(_controllerAssignmentConflictWindowMessage, joystickName, otherPlayerName, currentPlayerName);
	}

	public override string GetJoystickElementAssignmentPollingWindowMessage(string actionName)
	{
		return string.Format(_joystickElementAssignmentPollingWindowMessage, actionName);
	}

	public override string GetJoystickElementAssignmentPollingWindowMessage_FullAxisFieldOnly(string actionName)
	{
		return string.Format(_joystickElementAssignmentPollingWindowMessage_fullAxisFieldOnly, actionName);
	}

	public override string GetKeyboardElementAssignmentPollingWindowMessage(string actionName)
	{
		return string.Format(_keyboardElementAssignmentPollingWindowMessage, actionName);
	}

	public override string GetMouseElementAssignmentPollingWindowMessage(string actionName)
	{
		return string.Format(_mouseElementAssignmentPollingWindowMessage, actionName);
	}

	public override string GetMouseElementAssignmentPollingWindowMessage_FullAxisFieldOnly(string actionName)
	{
		return string.Format(_mouseElementAssignmentPollingWindowMessage_fullAxisFieldOnly, actionName);
	}

	public override string GetElementAlreadyInUseBlocked(string elementName)
	{
		return string.Format(_elementAlreadyInUseBlocked, elementName);
	}

	public override string GetElementAlreadyInUseCanReplace(string elementName, bool allowConflicts)
	{
		if (!allowConflicts)
		{
			return string.Format(_elementAlreadyInUseCanReplace, elementName);
		}
		return string.Format(_elementAlreadyInUseCanReplace_conflictAllowed, elementName);
	}

	public override string GetMouseAssignmentConflictWindowMessage(string otherPlayerName, string thisPlayerName)
	{
		return string.Format(_mouseAssignmentConflictWindowMessage, otherPlayerName, thisPlayerName);
	}

	public override string GetCalibrateAxisStep1WindowMessage(string axisName)
	{
		return string.Format(_calibrateAxisStep1WindowMessage, axisName);
	}

	public override string GetCalibrateAxisStep2WindowMessage(string axisName)
	{
		return string.Format(_calibrateAxisStep2WindowMessage, axisName);
	}

	public override string GetPlayerName(int playerId)
	{
		return (ReInput.players.GetPlayer(playerId) ?? throw new ArgumentException("Invalid player id: " + playerId)).descriptiveName;
	}

	public override string GetControllerName(Controller controller)
	{
		if (controller == null)
		{
			throw new ArgumentNullException("controller");
		}
		return controller.name;
	}

	public override string GetElementIdentifierName(ActionElementMap actionElementMap)
	{
		if (actionElementMap == null)
		{
			throw new ArgumentNullException("actionElementMap");
		}
		if (actionElementMap.controllerMap.controllerType == ControllerType.Keyboard)
		{
			return GetElementIdentifierName(actionElementMap.keyCode, actionElementMap.modifierKeyFlags);
		}
		return GetElementIdentifierName(actionElementMap.controllerMap.controller, actionElementMap.elementIdentifierId, actionElementMap.axisRange);
	}

	public override string GetElementIdentifierName(Controller controller, int elementIdentifierId, AxisRange axisRange)
	{
		if (controller == null)
		{
			throw new ArgumentNullException("controller");
		}
		ControllerElementIdentifier elementIdentifierById = controller.GetElementIdentifierById(elementIdentifierId);
		if (elementIdentifierById == null)
		{
			throw new ArgumentException("Invalid element identifier id: " + elementIdentifierId);
		}
		Controller.Element elementById = controller.GetElementById(elementIdentifierId);
		if (elementById == null)
		{
			return string.Empty;
		}
		return elementById.type switch
		{
			ControllerElementType.Axis => elementIdentifierById.GetDisplayName(elementById.type, axisRange), 
			ControllerElementType.Button => elementIdentifierById.name, 
			_ => elementIdentifierById.name, 
		};
	}

	public override string GetElementIdentifierName(KeyCode keyCode, ModifierKeyFlags modifierKeyFlags)
	{
		if (modifierKeyFlags != 0)
		{
			return $"{ModifierKeyFlagsToString(modifierKeyFlags)}{_modifierKeys.separator}{Keyboard.GetKeyName(keyCode)}";
		}
		return Keyboard.GetKeyName(keyCode);
	}

	public override string GetActionName(int actionId)
	{
		return (ReInput.mapping.GetAction(actionId) ?? throw new ArgumentException("Invalid action id: " + actionId)).descriptiveName;
	}

	public override string GetActionName(int actionId, AxisRange axisRange)
	{
		InputAction action = ReInput.mapping.GetAction(actionId);
		if (action == null)
		{
			throw new ArgumentException("Invalid action id: " + actionId);
		}
		switch (axisRange)
		{
		case AxisRange.Full:
			return action.descriptiveName;
		case AxisRange.Positive:
			if (string.IsNullOrEmpty(action.positiveDescriptiveName))
			{
				return action.descriptiveName + " +";
			}
			return action.positiveDescriptiveName;
		case AxisRange.Negative:
			if (string.IsNullOrEmpty(action.negativeDescriptiveName))
			{
				return action.descriptiveName + " -";
			}
			return action.negativeDescriptiveName;
		default:
			throw new NotImplementedException();
		}
	}

	public override string GetMapCategoryName(int id)
	{
		return (ReInput.mapping.GetMapCategory(id) ?? throw new ArgumentException("Invalid map category id: " + id)).descriptiveName;
	}

	public override string GetActionCategoryName(int id)
	{
		return (ReInput.mapping.GetActionCategory(id) ?? throw new ArgumentException("Invalid action category id: " + id)).descriptiveName;
	}

	public override string GetLayoutName(ControllerType controllerType, int id)
	{
		return (ReInput.mapping.GetLayout(controllerType, id) ?? throw new ArgumentException("Invalid " + controllerType.ToString() + " layout id: " + id)).descriptiveName;
	}

	public override string ModifierKeyFlagsToString(ModifierKeyFlags flags)
	{
		int num = 0;
		string text = string.Empty;
		if (Keyboard.ModifierKeyFlagsContain(flags, ModifierKey.Control))
		{
			text += _modifierKeys.control;
			num++;
		}
		if (Keyboard.ModifierKeyFlagsContain(flags, ModifierKey.Command))
		{
			if (num > 0 && !string.IsNullOrEmpty(_modifierKeys.separator))
			{
				text += _modifierKeys.separator;
			}
			text += _modifierKeys.command;
			num++;
		}
		if (Keyboard.ModifierKeyFlagsContain(flags, ModifierKey.Alt))
		{
			if (num > 0 && !string.IsNullOrEmpty(_modifierKeys.separator))
			{
				text += _modifierKeys.separator;
			}
			text += _modifierKeys.alt;
			num++;
		}
		if (num >= 3)
		{
			return text;
		}
		if (Keyboard.ModifierKeyFlagsContain(flags, ModifierKey.Shift))
		{
			if (num > 0 && !string.IsNullOrEmpty(_modifierKeys.separator))
			{
				text += _modifierKeys.separator;
			}
			text += _modifierKeys.shift;
			num++;
		}
		return text;
	}
}
