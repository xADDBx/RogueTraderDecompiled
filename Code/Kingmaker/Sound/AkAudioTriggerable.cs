using System;
using System.Collections.Generic;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Sound;

[RequireComponent(typeof(AudioObject))]
public abstract class AkAudioTriggerable : MonoBehaviour, IRegistrationCallback
{
	[SerializeField]
	private bool m_LogTriggers;

	[SerializeField]
	private TriggerType m_Triggers;

	[SerializeField]
	[InfoBox(Text = "Ignores sequences of duplicate triggers.\nIgnores AreaLoad happened after Enabled if both selected.\nIgnores Disabled happened after AreaUnload if both selected")]
	private bool m_TriggerOnce;

	private TriggerType m_TriggersCached;

	private readonly List<TriggerObjectBase> m_TriggersList = new List<TriggerObjectBase>();

	private TriggerType m_LastTrigger;

	private AudioObject m_AudioObject;

	private readonly LinkedListNode<IRegistrationCallback> m_RegistrationNode;

	protected AkAudioTriggerable()
	{
		m_RegistrationNode = new LinkedListNode<IRegistrationCallback>(this);
	}

	private void Awake()
	{
		m_AudioObject = GetComponent<AudioObject>();
	}

	protected virtual void OnEnable()
	{
		if ((bool)m_AudioObject)
		{
			m_AudioObject.RequestRegister(m_RegistrationNode);
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)m_AudioObject)
		{
			m_AudioObject.CancelRegistrationRequest(m_RegistrationNode);
		}
	}

	private void CreateTriggers()
	{
		foreach (TriggerType value in Enum.GetValues(typeof(TriggerType)))
		{
			if ((m_Triggers & value) != 0 && (m_TriggersCached & value) == 0)
			{
				TriggerObjectBase item = value.CreateTrigger(this);
				m_TriggersList.Add(item);
			}
			if ((m_Triggers & value) == 0 && (m_TriggersCached & value) != 0)
			{
				TriggerObjectBase item2 = m_TriggersList.FirstOrDefault((TriggerObjectBase t) => t.Type == value);
				m_TriggersList.Remove(item2);
			}
		}
		m_TriggersCached = m_Triggers;
	}

	internal void TriggerAndLog(TriggerObjectBase trigger)
	{
		if (!m_TriggerOnce || (m_LastTrigger != trigger.Type && !ArePaired(m_LastTrigger, trigger.Type)))
		{
			if (m_LogTriggers)
			{
				AkAudioService.Log.Log(this, "Audio triggered: " + base.name);
			}
			m_LastTrigger = trigger.Type;
			OnTrigger();
		}
	}

	public abstract void OnTrigger();

	public void StopAndLog(int fade)
	{
		if (m_LogTriggers)
		{
			AkAudioService.Log.Log(this, "Audio stopped: " + base.name);
		}
		OnStop(fade);
	}

	protected virtual void OnStop(int fade)
	{
	}

	private static bool ArePaired(TriggerType firstType, TriggerType secondType)
	{
		TriggerType triggerType = firstType | secondType;
		if (triggerType != (TriggerType.AreaLoad | TriggerType.Enabled))
		{
			return triggerType == (TriggerType.AreaUnload | TriggerType.Disabled);
		}
		return true;
	}

	public void OnAfterRegister()
	{
		m_LastTrigger = TriggerType.None;
		if (m_TriggersCached != m_Triggers)
		{
			CreateTriggers();
		}
		foreach (TriggerObjectBase triggers in m_TriggersList)
		{
			triggers.Enable();
		}
	}

	public void OnBeforeUnregister()
	{
		foreach (TriggerObjectBase triggers in m_TriggersList)
		{
			triggers.Disable();
		}
		m_LastTrigger = TriggerType.None;
	}
}
