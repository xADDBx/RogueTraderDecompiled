namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(InputField))]
[AddComponentMenu("UI/Extensions/InputFocus")]
public class InputFocus : MonoBehaviour
{
	protected InputField _inputField;

	public bool _ignoreNextActivation;

	private void Start()
	{
		_inputField = GetComponent<InputField>();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Return) && !_inputField.isFocused)
		{
			if (_ignoreNextActivation)
			{
				_ignoreNextActivation = false;
				return;
			}
			_inputField.Select();
			_inputField.ActivateInputField();
		}
	}

	public void buttonPressed()
	{
		bool num = _inputField.text == "";
		_inputField.text = "";
		if (!num)
		{
			_inputField.Select();
			_inputField.ActivateInputField();
		}
	}

	public void OnEndEdit(string textString)
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			bool num = _inputField.text == "";
			_inputField.text = "";
			if (num)
			{
				_ignoreNextActivation = true;
			}
		}
	}
}
