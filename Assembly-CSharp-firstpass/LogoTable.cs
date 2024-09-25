using System;
using System.Collections.Generic;
using UnityEngine;

public class LogoTable : MonoBehaviour
{
	public enum LogoLetter
	{
		T,
		b,
		e
	}

	[Serializable]
	public class LogoLine
	{
		public LogoLetter letter1;

		public string letter2;

		public Color color;
	}

	private static LogoTable instance;

	public List<LogoLine> logoLines = new List<LogoLine>
	{
		new LogoLine
		{
			letter1 = LogoLetter.T,
			letter2 = "a",
			color = Color.red
		},
		new LogoLine
		{
			letter1 = LogoLetter.b,
			letter2 = "l",
			color = Color.green
		},
		new LogoLine
		{
			letter1 = LogoLetter.e,
			letter2 = "",
			color = Color.blue
		}
	};

	public static LogoTable Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<LogoTable>();
			}
			return instance;
		}
	}

	private void OnGUI()
	{
		GUILayout.Label("Select the Logo Table scene object to visualize the table in the editor");
	}
}
