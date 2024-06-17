using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Modding;

[RequireComponent(typeof(RectTransform))]
public class OwlcatModificationsWindow : MonoBehaviour, IDisposable
{
	private enum Action
	{
		None,
		OpenSettings,
		CloseSettings
	}

	private const int ID = 2129345167;

	private const float WidthToHeightRatio = 1.3333334f;

	private static OwlcatModificationsWindow s_Instance;

	private Vector2 m_ScrollPosition;

	private OwlcatModification m_SelectedModification;

	private IDisposable m_KeyBindSubscription;

	public static bool IsActive
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.gameObject.activeSelf;
			}
			return false;
		}
	}

	public IDisposable Bind()
	{
		m_KeyBindSubscription = Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.OpenModificationsWindow.name, SwitchVisible);
		return this;
	}

	private static void Show()
	{
		if (s_Instance == null)
		{
			s_Instance = Resources.FindObjectsOfTypeAll<OwlcatModificationsWindow>().FirstOrDefault();
		}
		if (!(s_Instance != null))
		{
			return;
		}
		s_Instance.gameObject.SetActive(value: true);
		if (s_Instance.m_SelectedModification != null)
		{
			try
			{
				s_Instance.m_SelectedModification.OnShowGUI?.Invoke();
			}
			catch (Exception ex)
			{
				PFLog.Mods.Exception(ex);
			}
		}
	}

	private static void Hide()
	{
		if (s_Instance == null)
		{
			return;
		}
		s_Instance.Or(null)?.gameObject.SetActive(value: false);
		if (s_Instance.Or(null)?.m_SelectedModification == null)
		{
			return;
		}
		try
		{
			s_Instance.m_SelectedModification.OnHideGUI?.Invoke();
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
	}

	private void SwitchVisible()
	{
		if (s_Instance == null || !s_Instance.gameObject.activeSelf)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void OnGUI()
	{
		Camera main = Camera.main;
		if (!(main == null))
		{
			float num = main.pixelWidth;
			float num2 = main.pixelHeight;
			float num3 = num2 * 0.9f;
			float num4 = num3 * 1.3333334f;
			float x = (num - num4) / 2f;
			float y = (num2 - num3) / 2f;
			GUI.Window(2129345167, new Rect(x, y, num4, num3), DrawWindow, "Owlcat Modifications UI");
			GUI.FocusWindow(2129345167);
		}
	}

	private void DrawWindow(int id)
	{
		bool flag = false;
		using (new GUILayout.VerticalScope())
		{
			using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(m_ScrollPosition))
			{
				m_ScrollPosition = scrollViewScope.scrollPosition;
				using (new GUILayout.VerticalScope())
				{
					if (m_SelectedModification == null)
					{
						DrawModificationsList();
					}
					else
					{
						DrawModificationSettings(m_SelectedModification);
					}
				}
			}
			if (GUILayout.Button("Close"))
			{
				flag = true;
			}
		}
		if (flag)
		{
			Hide();
		}
	}

	private static Action DrawModificationHeader(OwlcatModification modification, bool detailed)
	{
		Action result = Action.None;
		GUILayout.Label((!string.IsNullOrEmpty(modification.Manifest.DisplayName)) ? modification.Manifest.DisplayName : modification.Manifest.UniqueName);
		GUILayout.Space(5f);
		GUILayout.Label("(" + modification.Manifest.Version + ")");
		GUILayout.FlexibleSpace();
		bool? flag = null;
		try
		{
			flag = modification.IsEnabled?.Invoke();
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
		bool flag2 = flag.HasValue && modification.OnSetEnabled != null;
		bool flag3 = GUI.enabled;
		try
		{
			GUI.enabled = flag2;
			bool? flag4 = flag;
			int num;
			object text;
			if (!flag4.HasValue)
			{
				num = 1;
			}
			else
			{
				num = (flag4.GetValueOrDefault() ? 1 : 0);
				if (num == 0)
				{
					text = "Disabled";
					goto IL_00ec;
				}
			}
			text = "Enabled";
			goto IL_00ec;
			IL_00ec:
			GUILayout.Label((string)text);
			GUILayout.Space(3f);
			bool flag5 = GUILayout.Toggle((byte)num != 0, GUIContent.none);
			if (flag.HasValue && flag5 != flag && flag2)
			{
				try
				{
					modification.OnSetEnabled(flag5);
				}
				catch (Exception ex2)
				{
					PFLog.Mods.Exception(ex2);
				}
			}
		}
		finally
		{
			GUI.enabled = flag3;
		}
		GUILayout.Space(15f);
		if (!detailed && modification.OnDrawGUI != null && GUILayout.Button("Settings"))
		{
			result = Action.OpenSettings;
		}
		if (detailed && GUILayout.Button("Back"))
		{
			result = Action.CloseSettings;
		}
		return result;
	}

	private void DrawModificationsList()
	{
		OwlcatModification[] appliedModifications = OwlcatModificationsManager.Instance.AppliedModifications;
		foreach (OwlcatModification owlcatModification in appliedModifications)
		{
			using (new GUILayout.HorizontalScope("box"))
			{
				if (DrawModificationHeader(owlcatModification, detailed: false) == Action.OpenSettings)
				{
					m_SelectedModification = owlcatModification;
					try
					{
						owlcatModification.OnShowGUI?.Invoke();
					}
					catch (Exception ex)
					{
						PFLog.Mods.Exception(ex);
					}
				}
			}
		}
	}

	private void DrawModificationSettings([NotNull] OwlcatModification modification)
	{
		using (new GUILayout.HorizontalScope("box"))
		{
			if (DrawModificationHeader(modification, detailed: true) == Action.CloseSettings)
			{
				m_SelectedModification = null;
				try
				{
					modification.OnHideGUI?.Invoke();
				}
				catch (Exception ex)
				{
					PFLog.Mods.Exception(ex);
				}
			}
		}
		using (new GUILayout.VerticalScope("box"))
		{
			try
			{
				modification.OnDrawGUI?.Invoke();
			}
			catch (Exception ex2)
			{
				GUILayout.TextArea(ex2.Message + "\n" + ex2.StackTrace);
			}
		}
	}

	public void Dispose()
	{
		m_KeyBindSubscription.Dispose();
		Hide();
	}
}
