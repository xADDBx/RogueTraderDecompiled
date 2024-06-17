using Kingmaker.Blueprints.Root;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Tutorial.UI.Temp;

public class TutorialTestUIManager : MonoBehaviour
{
	private static TutorialTestUIManager s_Instance;

	private TutorialData m_Data;

	private bool m_Destroy;

	private bool m_BigWindow;

	private int m_Page;

	private Vector2 m_ScrollPosition;

	private static GUIStyle s_WrappedTextStyle;

	private static TestUIStylesRoot Styles => BlueprintRoot.Instance.TestUIStyles;

	public static void Show(TutorialData tutorialData, bool bigWindow)
	{
		if (s_Instance == null)
		{
			s_Instance = new GameObject("TutorialTestUIManager").AddComponent<TutorialTestUIManager>();
		}
		s_Instance.m_Data = tutorialData;
		s_Instance.m_BigWindow = bigWindow;
		s_Instance.m_Page = 0;
		s_Instance.m_ScrollPosition = Vector2.zero;
		s_WrappedTextStyle = new GUIStyle(Styles.Label)
		{
			wordWrap = true
		};
	}

	public static void Hide()
	{
		if (s_Instance != null)
		{
			Object.Destroy(s_Instance.gameObject);
		}
	}

	private void OnGUI()
	{
		float num = CameraRig.Instance.Camera.pixelWidth;
		float num2 = CameraRig.Instance.Camera.pixelHeight;
		float num3 = (m_BigWindow ? 1024 : 500);
		float num4 = (m_BigWindow ? 768 : 600);
		Rect clientRect = (m_BigWindow ? new Rect(num / 2f - num3 / 2f, num2 / 2f - num4 / 2f, num3, num4) : new Rect(num - num3 - 10f, 10f, num3, num4));
		GUI.Window(GetInstanceID(), clientRect, DrawWindow, "", Styles.Window);
		if (m_Destroy)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void DrawWindow(int id)
	{
		TutorialData.Page page = m_Data.Pages.Get(m_Page);
		if (page == null)
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			GUILayout.Label(page.Title, Styles.Label);
			GUILayout.FlexibleSpace();
			if (!Game.Instance.Player.Tutorial.IsTagBanned(m_Data.Blueprint.Tag) && GUILayout.Button("Ban", Styles.Button))
			{
				Game.Instance.Player.Tutorial.BanTag(m_Data.Blueprint.Tag);
			}
			GUILayout.Space(5f);
			if (GUILayout.Button("X", Styles.Button))
			{
				m_Destroy = true;
			}
		}
		GUILayout.Space(25f);
		if (m_BigWindow)
		{
			SpriteLink picture = page.Picture;
			if ((object)picture != null && picture.Exists())
			{
				GUILayout.Label(page.Picture.Load().texture);
				GUILayout.Space(25f);
			}
		}
		using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope(m_ScrollPosition))
		{
			m_ScrollPosition = scrollViewScope.scrollPosition;
			GUILayout.Label(page.Description, s_WrappedTextStyle);
			GUILayout.Space(25f);
			if (!page.TriggerText.IsNullOrEmpty())
			{
				GUILayout.Label(page.TriggerText, s_WrappedTextStyle);
				GUILayout.Space(25f);
			}
			if (!page.SolutionText.IsNullOrEmpty())
			{
				GUILayout.Label(page.SolutionText, s_WrappedTextStyle);
				GUILayout.Space(25f);
			}
			GUILayout.FlexibleSpace();
		}
		if (m_Data.Pages.Count <= 1)
		{
			return;
		}
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("<<", Styles.Button) && m_Page > 0)
			{
				m_Page--;
			}
			GUILayout.Space(10f);
			GUILayout.Label($"{m_Page + 1}/{m_Data.Pages.Count}", Styles.Label);
			GUILayout.Space(10f);
			if (GUILayout.Button(">>", Styles.Button) && m_Page < m_Data.Pages.Count - 1)
			{
				m_Page++;
			}
			GUILayout.FlexibleSpace();
		}
	}
}
