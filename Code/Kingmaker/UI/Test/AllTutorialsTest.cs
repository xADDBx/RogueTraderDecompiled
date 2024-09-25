using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Tutorial;
using Kingmaker.UI.MVVM.View.Tutorial.PC;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Test;

public class AllTutorialsTest : MonoBehaviour
{
	private ReactiveProperty<BlueprintTutorial> m_CurrentTutorial;

	[SerializeField]
	private TutorialModalWindowPCView m_BigTutorial;

	[SerializeField]
	private TutorialHintWindowPCView m_SmallTutorial;

	[SerializeField]
	private TutorialTestButton m_Button;

	[SerializeField]
	private Transform m_ButtonsTransform;

	[SerializeField]
	private TextMeshProUGUI m_Index;

	[SerializeField]
	private OwlcatButton m_Next;

	[SerializeField]
	private OwlcatButton m_Back;

	private List<BlueprintTutorial> m_AllTutorials = new List<BlueprintTutorial>();

	private int m_CurrentIndex;

	private void Start()
	{
	}

	private void ChoosePrev()
	{
		if (m_CurrentIndex - 1 >= 0)
		{
			ChooseTutorial(m_CurrentIndex - 1);
		}
	}

	private void ChooseNext()
	{
		if (m_CurrentIndex + 1 < m_AllTutorials.Count)
		{
			ChooseTutorial(m_CurrentIndex + 1);
		}
	}

	public void ChooseTutorial(int i)
	{
		m_CurrentIndex = i;
		m_Index.text = m_CurrentIndex.ToString();
		TutorialData tutorialData = GetTutorialData(m_AllTutorials[i]);
		m_Index.text = i.ToString();
		if (tutorialData.Blueprint.Windowed)
		{
			m_BigTutorial.gameObject.SetActive(value: true);
			m_SmallTutorial.gameObject.SetActive(value: false);
			m_BigTutorial.Bind(new TutorialModalWindowVM(tutorialData, null));
		}
		else
		{
			m_BigTutorial.gameObject.SetActive(value: false);
			m_SmallTutorial.gameObject.SetActive(value: true);
			m_SmallTutorial.Bind(new TutorialHintWindowVM(tutorialData, null));
		}
	}

	private TutorialData GetTutorialData(BlueprintTutorial tutorial)
	{
		TutorialData tutorialData = new TutorialData(tutorial, null, null, solutionFound: false);
		tutorialData.AddPage(tutorial);
		foreach (ITutorialPage component in tutorial.GetComponents<ITutorialPage>())
		{
			tutorialData.AddPage(component);
		}
		return tutorialData;
	}
}
