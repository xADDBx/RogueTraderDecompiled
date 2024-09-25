using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.Controls.Toggles;

[AddComponentMenu("UI/Owlcat/Owlcat Toggle", 30)]
[RequireComponent(typeof(RectTransform))]
public class OwlcatToggle : MonoBehaviour, IConsoleEntityProxy, IConsoleEntity
{
	private const string OnLayer = "On";

	private const string OffLayer = "Off";

	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private OwlcatToggleGroup m_ToggleGroup;

	[SerializeField]
	private bool m_PrimaryIsOn;

	private readonly ReactiveProperty<bool> m_IsOn = new ReactiveProperty<bool>();

	private CompositeDisposable m_Subscriptions;

	public OwlcatToggleGroup Group
	{
		get
		{
			return m_ToggleGroup;
		}
		set
		{
			SetToggleGroup(value, setMemberValue: true);
		}
	}

	public IReadOnlyReactiveProperty<bool> IsOn => m_IsOn;

	public IConsoleEntity ConsoleEntityProxy => m_MultiButton;

	private void OnEnable()
	{
		m_IsOn.Value = m_PrimaryIsOn;
		m_Subscriptions?.Dispose();
		m_Subscriptions = new CompositeDisposable();
		m_Subscriptions.Add(m_MultiButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleClick();
		}));
		m_Subscriptions.Add(m_MultiButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			HandleClick();
		}));
		m_Subscriptions.Add(m_IsOn.Subscribe(delegate(bool value)
		{
			m_MultiButton.SetActiveLayer(value ? "On" : "Off");
		}));
		SetToggleGroup(m_ToggleGroup, setMemberValue: false);
	}

	private void OnDisable()
	{
		m_Subscriptions?.Dispose();
		m_Subscriptions = null;
		SetToggleGroup(null, setMemberValue: false);
	}

	private void SetToggleGroup(OwlcatToggleGroup newGroup, bool setMemberValue)
	{
		if (m_ToggleGroup != null)
		{
			m_ToggleGroup.UnregisterToggle(this);
		}
		if (setMemberValue)
		{
			m_ToggleGroup = newGroup;
		}
		if (newGroup != null)
		{
			newGroup.RegisterToggle(this);
		}
	}

	private void HandleClick()
	{
		Set(!m_IsOn.Value);
	}

	public void Set(bool value)
	{
		if (m_IsOn.Value != value)
		{
			m_IsOn.Value = value;
			if (!(m_ToggleGroup == null) && (m_IsOn.Value || (!m_ToggleGroup.AnyTogglesOn() && !m_ToggleGroup.AllowSwitchOff)))
			{
				m_IsOn.Value = true;
			}
		}
	}
}
