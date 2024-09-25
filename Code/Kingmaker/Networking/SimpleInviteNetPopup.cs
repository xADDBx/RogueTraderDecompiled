using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Networking;

public class SimpleInviteNetPopup : MonoBehaviour, INetInviteHandler, ISubscriber
{
	private static SimpleInviteNetPopup s_Instance;

	private Action<bool> m_Callback;

	public static void OpenPopup()
	{
		if (!s_Instance)
		{
			PFLog.Net.Log("Opened draft invite popup.");
			GameObject obj = new GameObject("SimpleInviteNetPopup");
			s_Instance = obj.AddComponent<SimpleInviteNetPopup>();
			UnityEngine.Object.DontDestroyOnLoad(obj);
		}
	}

	private void Awake()
	{
		EventBus.Subscribe(this);
		base.gameObject.SetActive(value: false);
	}

	private void OnGUI()
	{
		float x = ((float)Screen.width - 300f) / 2f;
		float y = ((float)Screen.height - 80f) / 2f;
		Rect screenRect = new Rect(x, y, 300f, 80f);
		GUILayout.Window(101, screenRect, DrawWindow, "Net Invite Popup", GUI.skin.window);
	}

	private void DrawWindow(int id)
	{
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Точно перейти по приглашению?");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Да"))
		{
			m_Callback?.Invoke(obj: true);
			base.gameObject.SetActive(value: false);
		}
		if (GUILayout.Button("Нет"))
		{
			m_Callback?.Invoke(obj: false);
			base.gameObject.SetActive(value: false);
		}
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	public void HandleInvite(Action<bool> callback)
	{
		m_Callback = delegate(bool result)
		{
			callback?.Invoke(result);
			m_Callback = null;
		};
		base.gameObject.SetActive(value: true);
	}

	public void HandleInviteAccepted(bool accepted)
	{
	}
}
