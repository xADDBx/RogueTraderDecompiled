using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Common.PC.InputField;

public class PCInputField : MonoBehaviour, IDisposable
{
	[SerializeField]
	protected OwlcatButton m_InputButton;

	[Header("Before focus")]
	[SerializeField]
	private TextMeshProUGUI m_EditLabel;

	[SerializeField]
	private TextMeshProUGUI m_FieldResult;

	[Header("When focus")]
	[SerializeField]
	protected TMP_InputField m_InputField;

	[SerializeField]
	private TextMeshProUGUI m_InputFieldWhenEmpty;

	protected string InputFieldResultText = string.Empty;

	private Action<string> m_OnEndEditAction;

	private List<IDisposable> m_Disposables = new List<IDisposable>();

	public void Initialize(string clickToEditText, string fieldPlaceHoldertext)
	{
		m_EditLabel.text = clickToEditText;
		m_InputFieldWhenEmpty.text = fieldPlaceHoldertext;
	}

	public IDisposable Bind(string defaultText, Action<string> onEndEditAction)
	{
		m_OnEndEditAction = onEndEditAction;
		m_InputField.text = (InputFieldResultText = defaultText);
		m_FieldResult.text = InputFieldResultText;
		AddDisposable(m_InputField.OnEndEditAsObservable().Subscribe(OnEndEdit));
		UpdatePlaceholder();
		AddDisposable(ObservableExtensions.Subscribe(m_InputButton.OnLeftClickAsObservable(), delegate
		{
			OnEdit();
		}));
		SetInputActive(state: false);
		return this;
	}

	private void UpdatePlaceholder()
	{
		m_OnEndEditAction(InputFieldResultText);
		m_InputField.text = InputFieldResultText;
		m_FieldResult.text = InputFieldResultText;
	}

	public void OnEdit()
	{
		SetInputActive(state: true);
		m_InputField.Select();
		m_InputField.ActivateInputField();
	}

	private void OnEndEdit(string text)
	{
		SetInputActive(state: false);
		if (!string.IsNullOrEmpty(text))
		{
			InputFieldResultText = text;
		}
		UpdatePlaceholder();
	}

	private void SetInputActive(bool state)
	{
		m_InputField.gameObject.SetActive(state);
		m_InputButton.gameObject.SetActive(!state);
	}

	private void AddDisposable(IDisposable d)
	{
		m_Disposables.Add(d);
	}

	public void Dispose()
	{
		SetInputActive(state: false);
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
	}
}
