using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Tutorial;
using Kingmaker.ElementsSystem;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("18579760b47a4a48824be9903e8e2550")]
public class ShowTutorial : GameAction
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Pages")]
	private BlueprintTutorialPageReference[] m_Pages = new BlueprintTutorialPageReference[0];

	public float Delay;

	public ReferenceArrayProxy<BlueprintTutorialPage> Pages
	{
		get
		{
			BlueprintReference<BlueprintTutorialPage>[] pages = m_Pages;
			return pages;
		}
	}

	public override string GetCaption()
	{
		IEnumerable<string> values = from p in Pages
			where p != null
			select p.ToString();
		string text = string.Join(", ", values);
		return "Show Tutorial (" + text + ")";
	}

	public override void RunAction()
	{
		if (Delay <= 0f)
		{
			ShowTutorialPage();
		}
		else
		{
			MainThreadDispatcher.StartCoroutine(ShowTutorialCoroutine());
		}
	}

	private IEnumerator ShowTutorialCoroutine()
	{
		yield return new WaitForSecondsRealtime(Delay);
		ShowTutorialPage();
	}

	private void ShowTutorialPage()
	{
		BlueprintTutorialPage blueprintTutorialPage = Pages.FirstOrDefault((BlueprintTutorialPage p) => p != null);
		if (blueprintTutorialPage != null && blueprintTutorialPage.Conditions.Check())
		{
			Pages.Where((BlueprintTutorialPage p) => p != null).ToArray();
		}
	}
}
