using System;
using System.Collections.Generic;
using Kingmaker.Sound.Base;
using UnityEngine;

namespace Kingmaker.Utility;

public class BakedPhysicsAnimator : MonoBehaviour
{
	public bool PlayOnEnable;

	[HideInInspector]
	private ColliderHitDetector m_ColliderDetector;

	[HideInInspector]
	public BakedAnimationParameters AnimParams;

	[HideInInspector]
	public bool Recording;

	private Component[] m_Heirarchy;

	private int m_CurrentFrame;

	private int m_KeyIndex;

	private const int NumberOfFrames = 1500;

	private const int RecordFreq = 5;

	public Vector3 GetInitalPos => base.transform.position;

	public Quaternion GetInitialRot => base.transform.rotation;

	public void ResetRecording()
	{
		m_CurrentFrame = 0;
		m_KeyIndex = 0;
		m_Heirarchy = base.transform.GetComponentsInChildren(typeof(Transform));
		m_Heirarchy = Array.FindAll(m_Heirarchy, (Component x) => x.GetComponent<Rigidbody>() != null);
		AnimParams.ObjectAnimHolder = new List<ObjectAnimationHolder>();
		int num = 300;
		for (int i = 0; i < m_Heirarchy.Length; i++)
		{
			Component component = m_Heirarchy[i];
			string objLocalPath = GetObjLocalPath(component.gameObject);
			ObjectAnimationHolder objectAnimationHolder = new ObjectAnimationHolder
			{
				RelativeObjName = objLocalPath,
				BakedAnimKeys = new List<BakedAnimationKey>()
			};
			for (int j = 0; j < num; j++)
			{
				objectAnimationHolder.BakedAnimKeys.Add(new BakedAnimationKey());
			}
			AnimParams.ObjectAnimHolder.Add(objectAnimationHolder);
		}
	}

	public void RemoveUnusedKeys()
	{
		foreach (ObjectAnimationHolder item in AnimParams.ObjectAnimHolder)
		{
			item.BakedAnimKeys.RemoveRange(m_KeyIndex, item.BakedAnimKeys.Count - m_KeyIndex);
		}
	}

	private void RegisterSoundCallBack(string soundCallback, float value)
	{
		ObjectAnimationHolder objectAnimationHolder = AnimParams.ObjectAnimHolder.Find((ObjectAnimationHolder x) => x.RelativeObjName.Contains("Pelvis"));
		if (objectAnimationHolder != null)
		{
			objectAnimationHolder.BakedAnimKeys[m_KeyIndex].IsSoundEvent = true;
			objectAnimationHolder.BakedAnimKeys[m_KeyIndex].SoundEvenString = soundCallback;
			objectAnimationHolder.BakedAnimKeys[m_KeyIndex].SoundEventFloat = value;
		}
	}

	private void OnEnable()
	{
		if (PlayOnEnable)
		{
			m_CurrentFrame = 0;
		}
	}

	private void FixedUpdate()
	{
		if (m_CurrentFrame < 1500 && m_KeyIndex < 1500 && PlayOnEnable)
		{
			Play();
			m_CurrentFrame++;
		}
	}

	private void Record()
	{
		if (m_KeyIndex >= AnimParams.ObjectAnimHolder[0].BakedAnimKeys.Count || m_CurrentFrame % 5 != 0)
		{
			return;
		}
		foreach (ObjectAnimationHolder item in AnimParams.ObjectAnimHolder)
		{
			Transform transform = base.transform.Find(item.RelativeObjName);
			item.BakedAnimKeys[m_KeyIndex].Position = transform.localPosition;
			item.BakedAnimKeys[m_KeyIndex].Rotation = transform.localRotation;
		}
		m_KeyIndex++;
	}

	private void Play()
	{
		if (m_KeyIndex + 1 >= AnimParams.ObjectAnimHolder[0].BakedAnimKeys.Count)
		{
			return;
		}
		float t = (float)(m_CurrentFrame % 5) / 5f;
		if (m_CurrentFrame != 0 && m_CurrentFrame % 5 == 0)
		{
			m_KeyIndex++;
		}
		foreach (ObjectAnimationHolder item in AnimParams.ObjectAnimHolder)
		{
			Transform obj = base.transform.Find(item.RelativeObjName);
			Vector3 localPosition = Vector3.Lerp(item.BakedAnimKeys[m_KeyIndex].Position, item.BakedAnimKeys[m_KeyIndex + 1].Position, t);
			Quaternion localRotation = Quaternion.Slerp(item.BakedAnimKeys[m_KeyIndex].Rotation, item.BakedAnimKeys[m_KeyIndex + 1].Rotation, t);
			obj.localPosition = localPosition;
			obj.localRotation = localRotation;
			if (item.BakedAnimKeys[m_KeyIndex].IsSoundEvent)
			{
				string soundEvenString = item.BakedAnimKeys[m_KeyIndex].SoundEvenString;
				float soundEventFloat = item.BakedAnimKeys[m_KeyIndex].SoundEventFloat;
				uint num = SoundEventsManager.PostEvent("VelocityRagdollBodyfall", base.gameObject);
				if (num != 0)
				{
					AkSoundEngine.SetRTPCValueByPlayingID(soundEvenString, soundEventFloat, num);
				}
			}
		}
	}

	private string GetObjLocalPath(GameObject obj)
	{
		string text = "/" + obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			text = "/" + obj.name + text;
		}
		return text;
	}

	public void OnDisable()
	{
	}
}
