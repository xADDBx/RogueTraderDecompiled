using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Sound;

internal class EnvironmentData
{
	private readonly List<AudioEnvironment> m_ActiveEnvironments = new List<AudioEnvironment>();

	private readonly AkAuxSendArray m_AuxSendValues = new AkAuxSendArray();

	private Vector3 m_LastPosition = Vector3.zero;

	private bool m_HasEnvironmentListChanged = true;

	private bool m_HasSentZero;

	public bool Changed => m_HasEnvironmentListChanged;

	private void AddHighestPriorityEnvironments(Vector3 position)
	{
		if (m_AuxSendValues.isFull || m_AuxSendValues.Count() >= m_ActiveEnvironments.Count)
		{
			return;
		}
		for (int i = 0; i < m_ActiveEnvironments.Count; i++)
		{
			AudioEnvironment audioEnvironment = m_ActiveEnvironments[i];
			uint valueHash = audioEnvironment.Bus.ValueHash;
			if ((!audioEnvironment.IsDefault || i == 0) && !m_AuxSendValues.Contains(valueHash))
			{
				m_AuxSendValues.Add(valueHash, audioEnvironment.GetAuxSendValueForPosition(position));
				if (audioEnvironment.ExcludeOthers || m_AuxSendValues.isFull)
				{
					break;
				}
			}
		}
	}

	public void UpdateAuxSend(GameObject gameObject, Vector3 position)
	{
		if (m_HasEnvironmentListChanged || !(m_LastPosition == position))
		{
			m_AuxSendValues.Reset();
			AddHighestPriorityEnvironments(position);
			bool flag = m_AuxSendValues.Count() == 0;
			if (!m_HasSentZero || !flag)
			{
				AkSoundEngine.SetGameObjectAuxSendValues(gameObject, m_AuxSendValues, (uint)m_AuxSendValues.Count());
			}
			m_HasSentZero = flag;
			m_LastPosition = position;
			m_HasEnvironmentListChanged = false;
		}
	}

	public void TryAddEnvironment(AudioEnvironment env)
	{
		if (env != null)
		{
			int num = m_ActiveEnvironments.BinarySearch(env, AudioEnvironment.Comparer);
			if (num < 0)
			{
				m_ActiveEnvironments.Insert(~num, env);
			}
			m_HasEnvironmentListChanged = true;
		}
	}

	public void RemoveEnvironment(AudioEnvironment env)
	{
		m_ActiveEnvironments.Remove(env);
		m_HasEnvironmentListChanged = true;
	}
}
