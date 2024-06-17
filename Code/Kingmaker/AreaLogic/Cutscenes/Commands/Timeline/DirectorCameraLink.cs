using Kingmaker.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

public class DirectorCameraLink : MonoBehaviour
{
	public enum ListenerControlType
	{
		None,
		FixedToCamera,
		TimelineAnimated,
		FixedToCameraOnZeroPos
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Timeline");

	private GameObject m_Camera;

	private Vector3 m_ListenerInitialPosition;

	private Quaternion m_ListenerInitialRotation;

	[InfoBox("None - DON'T USE IT, IF YOU SELECTED NONE, THEN THE GAME WILL USE FixedToCamera\nFixedToCamera - reparent current AudioListenerPositionController to this gameObject\nTimelineAnimated - reparent current AudioListenerPositionController to specified AudioListenerRoot\nFixedToCameraOnZeroPos - reparent current AudioListenerPositionController to this gameObject and set zero in local position")]
	public ListenerControlType ControlAudioListenerType;

	[ShowIf("NeedListenerRoot")]
	public Transform AudioListenerRoot;

	[ShowIf("NeedListenerRoot")]
	[InfoBox("If false rotation will be reset after reparent")]
	public bool KeepDefaultListenerRotation = true;

	private bool NeedListenerRoot => ControlAudioListenerType == ListenerControlType.TimelineAnimated;

	private void Awake()
	{
		if (ControlAudioListenerType == ListenerControlType.None)
		{
			ControlAudioListenerType = ListenerControlType.FixedToCamera;
		}
		UnityEngine.Camera componentInChildren = GetComponentInChildren<UnityEngine.Camera>(includeInactive: true);
		if ((bool)componentInChildren)
		{
			m_Camera = componentInChildren.gameObject;
			m_Camera.SetActive(value: false);
		}
	}

	public void Link()
	{
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			instance.gameObject.SetActive(value: false);
		}
		if ((bool)m_Camera)
		{
			m_Camera.gameObject.SetActive(value: true);
		}
		else if ((bool)instance)
		{
			m_Camera = Object.Instantiate(instance.Camera, base.transform).gameObject;
			m_Camera.hideFlags = HideFlags.DontSave;
			m_Camera.transform.localPosition = Vector3.zero;
			m_Camera.transform.localRotation = Quaternion.identity;
			m_Camera.GetComponent<UnityEngine.Camera>().fieldOfView = instance.CameraZoom.FovMax;
		}
		else
		{
			Logger.Error("Cant init cutscene camera. No predefined camera exists and no Camera rig");
		}
		if (instance.ListenerUpdater.GetComponent<AudioListenerPositionController>() != null)
		{
			instance.ListenerUpdater.GetComponent<AudioListenerPositionController>().FreezeXRotation = false;
		}
		switch (ControlAudioListenerType)
		{
		case ListenerControlType.FixedToCamera:
			instance.ChangeListenerParent(m_Camera.transform);
			break;
		case ListenerControlType.FixedToCameraOnZeroPos:
			instance.ChangeListenerParent(m_Camera.transform);
			m_ListenerInitialPosition = instance.ListenerUpdater.localPosition;
			instance.ListenerUpdater.localPosition = Vector3.zero;
			instance.ListenerUpdater.GetComponent<ListenerZoom>().enabled = false;
			if (instance.ListenerUpdater.GetComponent<AudioListenerPositionController>() != null)
			{
				instance.ListenerUpdater.GetComponent<AudioListenerPositionController>().FreezeXRotation = true;
			}
			break;
		case ListenerControlType.TimelineAnimated:
			instance.ChangeListenerParent(AudioListenerRoot ? AudioListenerRoot : m_Camera.transform);
			m_ListenerInitialPosition = instance.ListenerUpdater.localPosition;
			m_ListenerInitialRotation = instance.ListenerUpdater.localRotation;
			instance.ListenerUpdater.localPosition = Vector3.zero;
			if (!KeepDefaultListenerRotation)
			{
				instance.ListenerUpdater.localRotation = Quaternion.identity;
			}
			instance.ListenerUpdater.GetComponent<ListenerZoom>().enabled = false;
			break;
		}
	}

	public void UnLink()
	{
		if ((bool)m_Camera)
		{
			m_Camera.SetActive(value: false);
		}
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			instance.gameObject.SetActive(value: true);
		}
		if (ControlAudioListenerType == ListenerControlType.None)
		{
			return;
		}
		instance.ChangeListenerParent();
		if (ControlAudioListenerType == ListenerControlType.TimelineAnimated)
		{
			instance.ListenerUpdater.localPosition = m_ListenerInitialPosition;
			instance.ListenerUpdater.localRotation = m_ListenerInitialRotation;
			instance.ListenerUpdater.GetComponent<ListenerZoom>().enabled = true;
		}
		if (ControlAudioListenerType == ListenerControlType.FixedToCameraOnZeroPos)
		{
			instance.ListenerUpdater.localPosition = m_ListenerInitialPosition;
			instance.ListenerUpdater.GetComponent<ListenerZoom>().enabled = true;
			if (instance.ListenerUpdater.GetComponent<AudioListenerPositionController>() != null)
			{
				instance.ListenerUpdater.GetComponent<AudioListenerPositionController>().FreezeXRotation = false;
			}
		}
	}
}
