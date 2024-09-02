using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ScrollingCalendar : MonoBehaviour
{
	public RectTransform monthsScrollingPanel;

	public RectTransform yearsScrollingPanel;

	public RectTransform daysScrollingPanel;

	public GameObject yearsButtonPrefab;

	public GameObject monthsButtonPrefab;

	public GameObject daysButtonPrefab;

	private GameObject[] monthsButtons;

	private GameObject[] yearsButtons;

	private GameObject[] daysButtons;

	public RectTransform monthCenter;

	public RectTransform yearsCenter;

	public RectTransform daysCenter;

	private UIVerticalScroller yearsVerticalScroller;

	private UIVerticalScroller monthsVerticalScroller;

	private UIVerticalScroller daysVerticalScroller;

	public InputField inputFieldDays;

	public InputField inputFieldMonths;

	public InputField inputFieldYears;

	public Text dateText;

	private int daysSet;

	private int monthsSet;

	private int yearsSet;

	private void InitializeYears()
	{
		int[] array = new int[int.Parse(DateTime.Now.ToString("yyyy")) + 1 - 1900];
		yearsButtons = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 1900 + i;
			GameObject gameObject = UnityEngine.Object.Instantiate(yearsButtonPrefab, new Vector3(0f, i * 80, 0f), Quaternion.Euler(new Vector3(0f, 0f, 0f)));
			gameObject.transform.SetParent(yearsScrollingPanel);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.GetComponentInChildren<Text>().text = array[i].ToString() ?? "";
			gameObject.name = "Year_" + array[i];
			gameObject.AddComponent<CanvasGroup>();
			yearsButtons[i] = gameObject;
		}
	}

	private void InitializeMonths()
	{
		int[] array = new int[12];
		monthsButtons = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string text = "";
			array[i] = i;
			GameObject gameObject = UnityEngine.Object.Instantiate(monthsButtonPrefab, new Vector3(0f, i * 80, 0f), Quaternion.Euler(new Vector3(0f, 0f, 0f)));
			gameObject.transform.SetParent(monthsScrollingPanel);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			switch (i)
			{
			case 0:
				text = "Jan";
				break;
			case 1:
				text = "Feb";
				break;
			case 2:
				text = "Mar";
				break;
			case 3:
				text = "Apr";
				break;
			case 4:
				text = "May";
				break;
			case 5:
				text = "Jun";
				break;
			case 6:
				text = "Jul";
				break;
			case 7:
				text = "Aug";
				break;
			case 8:
				text = "Sep";
				break;
			case 9:
				text = "Oct";
				break;
			case 10:
				text = "Nov";
				break;
			case 11:
				text = "Dec";
				break;
			}
			gameObject.GetComponentInChildren<Text>().text = text;
			gameObject.name = "Month_" + array[i];
			gameObject.AddComponent<CanvasGroup>();
			monthsButtons[i] = gameObject;
		}
	}

	private void InitializeDays()
	{
		int[] array = new int[31];
		daysButtons = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i + 1;
			GameObject gameObject = UnityEngine.Object.Instantiate(daysButtonPrefab, new Vector3(0f, i * 80, 0f), Quaternion.Euler(new Vector3(0f, 0f, 0f)));
			gameObject.transform.SetParent(daysScrollingPanel);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.GetComponentInChildren<Text>().text = array[i].ToString() ?? "";
			gameObject.name = "Day_" + array[i];
			gameObject.AddComponent<CanvasGroup>();
			daysButtons[i] = gameObject;
		}
	}

	public void Awake()
	{
		InitializeYears();
		InitializeMonths();
		InitializeDays();
		monthsVerticalScroller = new UIVerticalScroller(monthsScrollingPanel, monthsButtons, monthCenter);
		yearsVerticalScroller = new UIVerticalScroller(yearsScrollingPanel, yearsButtons, yearsCenter);
		daysVerticalScroller = new UIVerticalScroller(daysScrollingPanel, daysButtons, daysCenter);
		monthsVerticalScroller.Start();
		yearsVerticalScroller.Start();
		daysVerticalScroller.Start();
	}

	public void SetDate()
	{
		daysSet = int.Parse(inputFieldDays.text) - 1;
		monthsSet = int.Parse(inputFieldMonths.text) - 1;
		yearsSet = int.Parse(inputFieldYears.text) - 1900;
		daysVerticalScroller.SnapToElement(daysSet);
		monthsVerticalScroller.SnapToElement(monthsSet);
		yearsVerticalScroller.SnapToElement(yearsSet);
	}

	private void Update()
	{
		monthsVerticalScroller.Update();
		yearsVerticalScroller.Update();
		daysVerticalScroller.Update();
		string results = daysVerticalScroller.GetResults();
		string results2 = monthsVerticalScroller.GetResults();
		string results3 = yearsVerticalScroller.GetResults();
		results = ((results.EndsWith("1") && results != "11") ? (results + "st") : ((results.EndsWith("2") && results != "12") ? (results + "nd") : ((!results.EndsWith("3") || !(results != "13")) ? (results + "th") : (results + "rd"))));
		dateText.text = results2 + " " + results + " " + results3;
	}

	public void DaysScrollUp()
	{
		daysVerticalScroller.ScrollUp();
	}

	public void DaysScrollDown()
	{
		daysVerticalScroller.ScrollDown();
	}

	public void MonthsScrollUp()
	{
		monthsVerticalScroller.ScrollUp();
	}

	public void MonthsScrollDown()
	{
		monthsVerticalScroller.ScrollDown();
	}

	public void YearsScrollUp()
	{
		yearsVerticalScroller.ScrollUp();
	}

	public void YearsScrollDown()
	{
		yearsVerticalScroller.ScrollDown();
	}
}
