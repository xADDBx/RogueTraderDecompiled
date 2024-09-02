using Kingmaker.Sound;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.Visual;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
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

	private Vector3 m_ListenerInitialPosition;

	private Quaternion m_ListenerInitialRotation;

	private UnityEngine.Camera m_DirectorCamera;

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
		m_DirectorCamera = GetComponentInChildren<UnityEngine.Camera>(includeInactive: true);
		if ((bool)m_DirectorCamera)
		{
			m_DirectorCamera.EnsureComponent<CameraOverlayConnector>().enabled = true;
			m_DirectorCamera.gameObject.SetActive(value: false);
		}
	}

	public void Link()
	{
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance)
		{
			instance.gameObject.SetActive(value: false);
		}
		if (m_DirectorCamera != null)
		{
			m_DirectorCamera.gameObject.SetActive(value: true);
		}
		else if ((bool)instance)
		{
			GameObject gameObject = Object.Instantiate(instance.Camera, base.transform).gameObject;
			gameObject.hideFlags = HideFlags.DontSave;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			m_DirectorCamera = gameObject.GetComponent<UnityEngine.Camera>();
			m_DirectorCamera.EnsureComponent<CameraOverlayConnector>().ForceUpdateState();
			m_DirectorCamera.fieldOfView = instance.CameraZoom.FovMax;
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
			instance.ChangeListenerParent(m_DirectorCamera.transform);
			break;
		case ListenerControlType.FixedToCameraOnZeroPos:
			instance.ChangeListenerParent(m_DirectorCamera.transform);
			m_ListenerInitialPosition = instance.ListenerUpdater.localPosition;
			instance.ListenerUpdater.localPosition = Vector3.zero;
			instance.ListenerUpdater.GetComponent<ListenerZoom>().enabled = false;
			if (instance.ListenerUpdater.GetComponent<AudioListenerPositionController>() != null)
			{
				instance.ListenerUpdater.GetComponent<AudioListenerPositionController>().FreezeXRotation = true;
			}
			break;
		case ListenerControlType.TimelineAnimated:
			instance.ChangeListenerParent(AudioListenerRoot ? AudioListenerRoot : m_DirectorCamera.transform);
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
		if (m_DirectorCamera != null)
		{
			m_DirectorCamera.gameObject.SetActive(value: false);
		}
		CameraRig instance = CameraRig.Instance;
		if (instance != null)
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
