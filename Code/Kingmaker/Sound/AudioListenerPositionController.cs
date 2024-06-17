using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Sound;

public class AudioListenerPositionController : MonoBehaviour
{
	private DefaultListener m_Listener;

	public bool FreezeXRotation;

	private void OnEnable()
	{
		m_Listener = ObjectRegistry<DefaultListener>.Instance?.MaybeSingle;
		if (!m_Listener)
		{
			GameObject gameObject = new GameObject("wWiseListener");
			gameObject.AddComponent<AudioObject>();
			m_Listener = gameObject.AddComponent<DefaultListener>();
			FreezeXRotation = false;
		}
	}

	private void LateUpdate()
	{
		m_Listener.transform.SetPositionAndRotation(base.transform.position, FreezeXRotation ? Quaternion.Euler(78.068f, base.transform.rotation.eulerAngles.y, base.transform.rotation.eulerAngles.z) : base.transform.rotation);
	}
}
