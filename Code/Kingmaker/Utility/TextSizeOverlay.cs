using System.Collections.Generic;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Core.Overlays;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Utility;

public class TextSizeOverlay : MonoBehaviour
{
	[Range(1f, 60f)]
	public int TargetFontSize = 27;

	public const string OverlayName = "TextSize";

	private bool m_UICommonSceneLoaded;

	private bool m_UISurfaceSceneLoaded;

	private bool m_UISpaceSceneLoaded;

	private int m_TMProsCount;

	private int m_TMProsSmallerThanTargetCount;

	private List<TextMeshProUGUI> m_ActiveTMPros;

	private void Start()
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			Object.Destroy(this);
			return;
		}
		m_ActiveTMPros = new List<TextMeshProUGUI>();
		List<OverlayElement> list = new List<OverlayElement>
		{
			new Label("PRESS ALT+CTRL+T TO ANALYZE TEXT, CTRL+R TO RELOAD UI", () => string.Empty),
			new Label("UI_Common_Scene LOADED", () => m_UICommonSceneLoaded.ToString()),
			new Label("UI_Surface_Scene LOADED", () => m_UISurfaceSceneLoaded.ToString()),
			new Label("UI_Space_Scene LOADED", () => m_UISpaceSceneLoaded.ToString()),
			new Label("TextMeshProUGUI TOTAL COUNT", () => m_TMProsCount.ToString()),
			new Label("TextMeshProUGUI SMALLER THAN TARGET COUNT", () => m_TMProsSmallerThanTargetCount.ToString())
		};
		Overlay o = new Overlay("TextSize", list.ToArray());
		OverlayService.Instance?.RegisterOverlay(o);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.T))
		{
			m_ActiveTMPros.Clear();
			m_TMProsSmallerThanTargetCount = 0;
			CollectTMProsFromScene("UI_Common_Scene", out m_UICommonSceneLoaded);
			CollectTMProsFromScene("UI_Surface_Scene", out m_UISurfaceSceneLoaded);
			CollectTMProsFromScene("UI_Space_Scene", out m_UISpaceSceneLoaded);
			AnalyzeTMPros();
			m_TMProsCount = m_ActiveTMPros.Count;
		}
	}

	private void CollectTMProsFromScene(string sceneName, out bool sceneIsLoaded)
	{
		Scene sceneByName = SceneManager.GetSceneByName(sceneName);
		sceneIsLoaded = sceneByName.isLoaded;
		if (!sceneIsLoaded)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		sceneByName.GetRootGameObjects(list);
		foreach (GameObject item in list)
		{
			m_ActiveTMPros.AddRange(item.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: false));
		}
	}

	private void AnalyzeTMPros()
	{
		foreach (TextMeshProUGUI activeTMPro in m_ActiveTMPros)
		{
			if (!(activeTMPro.fontSize >= (float)TargetFontSize))
			{
				activeTMPro.color = Color.magenta;
				activeTMPro.text += $" {activeTMPro.fontSize}pt";
				m_TMProsSmallerThanTargetCount++;
			}
		}
	}
}
