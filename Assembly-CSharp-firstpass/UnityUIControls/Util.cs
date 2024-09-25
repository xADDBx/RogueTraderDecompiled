using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUIControls;

public class Util : MonoBehaviour
{
	public enum DateInterval
	{
		Millisecond,
		Second,
		Minute,
		Hour,
		Day,
		Weekday,
		Month,
		Year
	}

	public class Timer
	{
		private float fStartTime;

		private float fStopTime;

		private int intStartTime;

		private int intStopTime;

		private bool blnIsRunning;

		public int GetTime
		{
			get
			{
				if (blnIsRunning)
				{
					return (int)Time.time - intStartTime;
				}
				return intStopTime - intStartTime;
			}
		}

		public float GetFloatTime
		{
			get
			{
				if (blnIsRunning)
				{
					return Time.time - fStartTime;
				}
				return fStopTime - fStartTime;
			}
		}

		public bool IsRunning => blnIsRunning;

		public void StartTimer()
		{
			fStartTime = Time.time;
			intStartTime = (int)Time.time;
			blnIsRunning = true;
		}

		public void StopTimer()
		{
			fStopTime = Time.time;
			intStopTime = (int)Time.time;
			blnIsRunning = false;
		}
	}

	public class CoroutineWithData
	{
		private IEnumerator target;

		public object result;

		public Coroutine coroutine { get; private set; }

		public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
		{
			this.target = target;
			coroutine = owner.StartCoroutine(Run());
		}

		private IEnumerator Run()
		{
			while (target.MoveNext())
			{
				result = target.Current;
				yield return result;
			}
		}
	}

	public static string StringCheck(string strInput = "")
	{
		if (strInput == null)
		{
			return "";
		}
		return Regex.Replace(Regex.Replace(Regex.Replace(strInput, "[^\\u0000-\\u007F]", string.Empty), "\\\\", "\\\\"), "--", "-");
	}

	public static bool IsInt(string strInput)
	{
		strInput = StringCheck(strInput);
		int result;
		return int.TryParse(strInput, out result);
	}

	public static bool IsDecimal(string strInput)
	{
		strInput = StringCheck(strInput);
		double result;
		return double.TryParse(strInput, out result);
	}

	public static bool IsDate(string strInput)
	{
		strInput = StringCheck(strInput);
		DateTime result;
		return DateTime.TryParse(strInput, out result);
	}

	private static long Fix(double Number)
	{
		float f = ConvertToFloat(Number.ToString());
		if (Number >= 0.0)
		{
			return (long)Mathf.Floor(f);
		}
		return (long)Mathf.Ceil(f);
	}

	public static DateTime ConvertToDate(string strInput)
	{
		strInput = StringCheck(strInput);
		try
		{
			return DateTime.Parse(strInput);
		}
		catch
		{
			return DateTime.Parse("01/01/0001");
		}
	}

	public static string PlusMinus(int intInput)
	{
		string text = intInput.ToString();
		if (intInput >= 0)
		{
			text = "+" + text;
		}
		return text;
	}

	public static int ConvertToInt(string strInput)
	{
		strInput = StringCheck(strInput);
		try
		{
			return Convert.ToInt32(strInput);
		}
		catch
		{
			return 0;
		}
	}

	public static int ConvertToInt(bool blnInput)
	{
		if (blnInput)
		{
			return 1;
		}
		return 0;
	}

	public static int ConvertToInt(float dblInput)
	{
		return (int)Mathf.Ceil(dblInput);
	}

	public static int ConvertToInt(double dblInput)
	{
		return (int)Math.Ceiling(dblInput);
	}

	public static long ConvertToLong(string strInput)
	{
		strInput = StringCheck(strInput);
		try
		{
			return Convert.ToInt64(strInput);
		}
		catch
		{
			return 0L;
		}
	}

	public static float ConvertToFloat(string strInput)
	{
		strInput = StringCheck(strInput);
		try
		{
			return Convert.ToSingle(strInput);
		}
		catch
		{
			return 0f;
		}
	}

	public static float ConvertToFloat(int intInput)
	{
		return Convert.ToSingle(intInput);
	}

	public static bool ConvertToBoolean(string strInput)
	{
		if (strInput.ToLowerInvariant().Trim() == "true" || strInput == "1")
		{
			return true;
		}
		return false;
	}

	public static string ConvertToMoneyString(float dblInput, bool blnShowSign = false)
	{
		bool flag = dblInput < 0f;
		dblInput = Mathf.Abs(dblInput);
		string text = dblInput.ToString();
		if (text.IndexOf(".") < 0)
		{
			text += ".00";
		}
		if (text.Length - text.IndexOf(".") < 3)
		{
			text += "0";
		}
		text = "$" + text;
		if (dblInput > 0f)
		{
			if (flag)
			{
				text = "- " + text;
			}
			else if (blnShowSign)
			{
				text = "+ " + text;
			}
		}
		return text;
	}

	public static string ConvertToFloatString(float dblInput, bool blnShowSign = false)
	{
		bool flag = dblInput < 0f;
		dblInput = Mathf.Abs(dblInput);
		string text = dblInput.ToString();
		if (text.IndexOf(".") < 0)
		{
			text += ".00";
		}
		if (text.Length - text.IndexOf(".") < 3)
		{
			text += "0";
		}
		if (dblInput > 0f)
		{
			if (flag)
			{
				text = "- " + text;
			}
			else if (blnShowSign)
			{
				text = "+ " + text;
			}
		}
		return text;
	}

	public static string ConvertToFloatString(float dblInput, bool blnAddSign, int intPlaces)
	{
		bool flag = dblInput < 0f;
		dblInput = Mathf.Abs(dblInput);
		string text = (dblInput * Mathf.Pow(10f, intPlaces)).ToString();
		if (text.Contains("."))
		{
			text = text.Substring(0, text.IndexOf('.'));
		}
		dblInput = float.Parse(text) / Mathf.Pow(10f, intPlaces);
		string text2 = dblInput.ToString();
		if (text2.IndexOf(".") < 0)
		{
			text2 = text2 + "." + "00000000000000000000".Substring(0, intPlaces);
		}
		int num = text2.Length - text2.IndexOf(".") - 1;
		if (num < intPlaces)
		{
			text2 += "00000000000000000000".Substring(0, intPlaces - num);
		}
		text = text2.Substring(0, text2.LastIndexOf('.'));
		string text3 = "";
		for (int i = 1; i <= text.Length; i++)
		{
			text3 = text.Substring(text.Length - i, 1) + text3;
			if (i % 3 == 0)
			{
				text3 = "," + text3;
			}
		}
		if (text3.StartsWith(","))
		{
			text3 = text3.Substring(1);
		}
		text2 = text3 + text2.Substring(text2.LastIndexOf('.'));
		if (intPlaces == 0)
		{
			text2 = text2.Substring(0, text2.LastIndexOf('.'));
		}
		if (dblInput > 0f && blnAddSign)
		{
			text2 = ((!flag) ? ("+" + text2) : ("-" + text2));
		}
		else if (flag)
		{
			text2 = "-" + text2;
		}
		return text2;
	}

	public static string ConvertToFloatString(string strInput, bool blnAddSign)
	{
		int intPlaces = 2;
		return ConvertToFloatString(strInput, blnAddSign, intPlaces);
	}

	public static string ConvertToFloatString(string strInput, bool blnAddSign, int intPlaces)
	{
		return ConvertToFloatString(ConvertToFloat(strInput), blnAddSign, intPlaces);
	}

	public static int ConvertToFullPercent(int intA, int intB)
	{
		float num = ConvertToFloat(intA);
		float num2 = ConvertToFloat(intB);
		return ConvertToInt(num / num2 * 100f);
	}

	public static long DateDiff(DateInterval interval, DateTime dateStart, DateTime dateEnd)
	{
		TimeSpan timeSpan = dateEnd - dateStart;
		switch (interval)
		{
		case DateInterval.Year:
			if (dateEnd.Month < dateStart.Month)
			{
				return dateEnd.Year - dateStart.Year - 1;
			}
			return dateEnd.Year - dateStart.Year;
		case DateInterval.Month:
			return dateEnd.Month - dateStart.Month + 12 * (dateEnd.Year - dateStart.Year);
		case DateInterval.Weekday:
			return Fix(timeSpan.TotalDays) / 7;
		case DateInterval.Day:
			return Fix(timeSpan.TotalDays);
		case DateInterval.Hour:
			return Fix(timeSpan.TotalHours);
		case DateInterval.Minute:
			return Fix(timeSpan.TotalMinutes);
		case DateInterval.Second:
			return Fix(timeSpan.TotalSeconds);
		case DateInterval.Millisecond:
			return Fix(timeSpan.TotalMilliseconds);
		default:
			return 0L;
		}
	}

	public static Vector2 ConvertToVector2(string strInput)
	{
		Vector2 zero = Vector2.zero;
		strInput = strInput.Replace("(", "").Replace(")", "");
		string[] array = strInput.Split(',');
		zero.x = ConvertToFloat(array[0].Trim());
		zero.y = ConvertToFloat(array[1].Trim());
		return zero;
	}

	public static Vector3 ConvertToVector3(string strInput)
	{
		Vector3 zero = Vector3.zero;
		strInput = strInput.Replace("(", "").Replace(")", "");
		string[] array = strInput.Split(',');
		zero.x = ConvertToFloat(array[0].Trim());
		zero.y = ConvertToFloat(array[1].Trim());
		zero.z = ConvertToFloat(array[2].Trim());
		return zero;
	}

	public static Vector4 ConvertToVector4(string strInput)
	{
		Vector4 zero = Vector4.zero;
		strInput = strInput.Replace("(", "").Replace(")", "");
		string[] array = strInput.Split(',');
		zero.x = ConvertToFloat(array[0].Trim());
		zero.y = ConvertToFloat(array[1].Trim());
		zero.z = ConvertToFloat(array[2].Trim());
		zero.w = ConvertToFloat(array[3].Trim());
		return zero;
	}

	public static Quaternion ConvertToQuaternion(string strInput)
	{
		Quaternion result = Quaternion.Euler(Vector3.zero);
		try
		{
			strInput = strInput.Replace("(", "").Replace(")", "");
			string[] array = strInput.Split(',');
			result.x = ConvertToFloat(array[0].Trim());
			result.y = ConvertToFloat(array[1].Trim());
			result.z = ConvertToFloat(array[2].Trim());
			result.w = ConvertToFloat(array[3].Trim());
		}
		catch
		{
			Debug.LogError("Error in ConvertToQuaternion. " + strInput);
		}
		return result;
	}

	public static string ConvertSecondsToTime(float intInput, bool blnLongDisplay = false)
	{
		string text = "";
		intInput = ConvertToFloat(Mathf.FloorToInt(intInput));
		int num = Mathf.FloorToInt(intInput / 3600f);
		int num2 = Mathf.FloorToInt(intInput % 3600f / 60f);
		int num3 = Mathf.FloorToInt(intInput % 60f);
		if (blnLongDisplay)
		{
			if (num > 0)
			{
				text = num + "hr";
			}
			if (num2 > 0)
			{
				if (num > 0)
				{
					text += ", ";
				}
				text = text + num2 + "min";
			}
			if (num3 > 0)
			{
				if (num > 0 || num2 > 0)
				{
					text += ", ";
				}
				text = text + num3 + "sec";
			}
		}
		else
		{
			if (num > 0)
			{
				text = num.ToString().PadLeft(2, '0') + ":";
			}
			text = text + num2.ToString().PadLeft(2, '0') + ":";
			text += num3.ToString().PadLeft(2, '0');
		}
		return text;
	}

	public static void ConvertToTransform(ref Transform t, string strInput)
	{
		try
		{
			string[] array = strInput.Split('|');
			t.position = ConvertToVector3(array[0]);
			t.rotation = ConvertToQuaternion(array[1]);
		}
		catch
		{
			t.position = Vector3.zero;
			t.rotation = Quaternion.identity;
		}
	}

	public static string ConvertFromTransform(Transform t)
	{
		return t.position.ToString() + "|" + t.rotation.ToString();
	}

	private static long Fix(float Number)
	{
		if (Number >= 0f)
		{
			return (long)Mathf.Floor(Number);
		}
		return (long)Mathf.Ceil(Number);
	}

	public static float Normalize(float value)
	{
		if (value > 0f)
		{
			return 1f;
		}
		if (value < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	public static byte[] GetBytes(string str)
	{
		byte[] array = new byte[str.Length * 2];
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	public static string GetString(byte[] bytes)
	{
		char[] array = new char[bytes.Length / 2];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return new string(array);
	}

	public static string ReadTextFile(string strFileName, bool blnStripCRLF = false)
	{
		string text = "";
		string text2 = ApplicationPaths.persistentDataPath + "/" + strFileName;
		try
		{
			StreamReader streamReader = new StreamReader(text2);
			text = streamReader.ReadToEnd();
			streamReader.Close();
			if (blnStripCRLF)
			{
				text = text.Replace("\n\n", "\n");
			}
		}
		catch
		{
			Debug.LogError("Unable to read \"" + text2 + "\".");
		}
		return text;
	}

	public static bool WriteTextFile(string strDirectory, string strFilename, string strText)
	{
		try
		{
			strDirectory = Application.dataPath + "/" + strDirectory;
			if (!Directory.Exists(strDirectory))
			{
				try
				{
					Directory.CreateDirectory(strDirectory);
				}
				catch
				{
					Debug.LogError("Unable to Create directory \"" + strDirectory + "\".");
				}
			}
			if (Directory.Exists(strDirectory))
			{
				File.WriteAllText(strDirectory + "/" + strFilename, strText);
				return true;
			}
		}
		catch
		{
			Debug.LogError("Unable to write \"" + strFilename + "\" to \"" + strDirectory + "\"");
		}
		return false;
	}

	public static bool FileExists(string strDirectory, string strFilename)
	{
		strDirectory = Application.dataPath + "/" + strDirectory;
		if (!Directory.Exists(strDirectory))
		{
			return false;
		}
		if (File.Exists(strDirectory + "/" + strFilename))
		{
			return true;
		}
		return false;
	}

	private static double RadiansToDegrees(double rad)
	{
		return rad / 3.1415927410125732 * 180.0;
	}

	private static double DegreesToRadians(double deg)
	{
		return deg * 3.1415927410125732 / 180.0;
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angle)
	{
		return angle * (point - pivot) + pivot;
	}

	public static void OrbitCameraUpdate(GameObject goOrbitingObject, GameObject goOrbitTarget = null, bool blnUseCamera = false, float fOrbitDegrees = 1f, float fRotationSpeed = 25f)
	{
		if (!(goOrbitingObject == null))
		{
			if (goOrbitTarget != null)
			{
				goOrbitingObject.transform.position = RotatePointAroundPivot(goOrbitingObject.transform.position, goOrbitTarget.transform.position, Quaternion.Euler(0f, fOrbitDegrees * fRotationSpeed * Time.deltaTime, 0f));
				goOrbitingObject.transform.LookAt(goOrbitTarget.transform.position);
			}
			else if (goOrbitingObject.transform.parent != null)
			{
				goOrbitingObject.transform.position = RotatePointAroundPivot(goOrbitingObject.transform.position, goOrbitingObject.transform.parent.position, Quaternion.Euler(0f, fOrbitDegrees * fRotationSpeed * Time.deltaTime, 0f));
			}
		}
	}

	public static void AlwaysFaceObject(GameObject goFacee, GameObject goScale = null, GameObject goTarget = null, float fScaleModifier = 1f)
	{
		if (goScale == null)
		{
			goScale = goFacee;
		}
		if (goFacee != null)
		{
			if (goTarget != null)
			{
				goFacee.transform.LookAt(goTarget.transform.position);
				return;
			}
			goFacee.transform.rotation = Camera.main.transform.rotation;
			float num = Vector3.Distance(goFacee.transform.position, Camera.main.transform.position);
			num *= fScaleModifier;
			goScale.transform.localScale = new Vector3(num, num, num);
		}
	}

	public static Vector3 SetVectorX(Vector3 v, float x)
	{
		v.x = x;
		return v;
	}

	public static Vector3 SetVectorY(Vector3 v, float y)
	{
		v.y = y;
		return v;
	}

	public static Vector3 SetVectorZ(Vector3 v, float z)
	{
		v.z = z;
		return v;
	}

	public static Vector3 SetVectorXZ(Vector3 v, float x, float z)
	{
		v.x = x;
		v.z = z;
		return v;
	}

	public static Vector3 AddVectorX(Vector3 v, float x)
	{
		v.x += x;
		return v;
	}

	public static Vector3 AddVectorY(Vector3 v, float y)
	{
		v.y += y;
		return v;
	}

	public static Vector3 AddVectorZ(Vector3 v, float z)
	{
		v.z += z;
		return v;
	}

	public static Vector3 VectorXZ(Vector3 v)
	{
		return new Vector3(v.x, 0f, v.z);
	}

	public static float Distance2D(Vector3 v1, Vector2 v2)
	{
		v1.y = (v2.y = 0f);
		return Vector3.Distance(v1, v2);
	}

	public static Vector3 MakeUniformScale(float scale)
	{
		return new Vector3(scale, scale, scale);
	}

	public static void LookAt2D(Transform transform, Vector3 point)
	{
		Vector3 eulerAngles = transform.eulerAngles;
		transform.LookAt(point);
		eulerAngles.y = transform.eulerAngles.y;
		transform.eulerAngles = eulerAngles;
	}

	public static float CalculateMass(float radius)
	{
		return Mathf.Min(radius * 0.05f, 10f);
	}

	public static void AutoResize(int screenWidth, int screenHeight)
	{
		Vector2 vector = new Vector2((float)Screen.width / (float)screenWidth, (float)Screen.height / (float)screenHeight);
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(vector.x, vector.y, 1f));
	}

	public static void CalcWidth(Image pnl, int intCur, int intMax, int intMaxWidth, int intMaxHeight)
	{
		float num = ConvertToFloat(intCur);
		float num2 = ConvertToFloat(intMax);
		pnl.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2((float)intMaxWidth * (num / num2), intMaxHeight);
	}

	public static Vector3 GetModelCenter(GameObject go)
	{
		Vector3 position = go.transform.position;
		return new Vector3(position.x, position.y + 1.5f, position.z);
	}

	public static void CopyComponent(GameObject go, Component orig)
	{
		Component component = go.GetComponent(orig.GetType());
		if (component == null)
		{
			component = go.AddComponent(orig.GetType());
		}
		component.gameObject.SetActive(orig.gameObject.activeSelf);
		FieldInfo[] fields = orig.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			try
			{
				fieldInfo.SetValue(component, fieldInfo.GetValue(orig));
			}
			catch
			{
			}
		}
	}

	public static void CopyComponent(Component target, Component orig)
	{
		GameObject gameObject = target.gameObject;
		Component component;
		if (target.GetType() != orig.GetType())
		{
			component = gameObject.GetComponent(orig.GetType());
			if (component == null)
			{
				component = gameObject.AddComponent(orig.GetType());
			}
		}
		else
		{
			component = target;
		}
		component.gameObject.SetActive(orig.gameObject.activeSelf);
		FieldInfo[] fields = orig.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			try
			{
				fieldInfo.SetValue(component, fieldInfo.GetValue(orig));
			}
			catch
			{
			}
		}
	}

	public static GameObject GetChildGameObject(GameObject fromGameObject, string strObjectName)
	{
		Transform[] componentsInChildren = fromGameObject.transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != null && transform.gameObject != null && transform.gameObject.name != null && transform.gameObject.name == strObjectName)
			{
				return transform.gameObject;
			}
		}
		return null;
	}

	public static GameObject GetTrueParent(GameObject goChild)
	{
		GameObject gameObject = goChild;
		while (gameObject.transform.parent != null && gameObject.transform.parent.name != "Canvas" && !gameObject.transform.parent.CompareTag("Game") && !gameObject.transform.parent.CompareTag("Container"))
		{
			gameObject = gameObject.transform.parent.gameObject;
		}
		return gameObject;
	}

	public static GameObject GetParentWithTag(GameObject goChild, string strTag)
	{
		GameObject gameObject = goChild;
		while (gameObject.transform.parent.gameObject != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
			if (gameObject.transform.CompareTag(strTag))
			{
				return gameObject;
			}
		}
		return gameObject;
	}

	public static GameObject GetClosestObjectByTag(GameObject goSource, string strTag)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag(strTag);
		GameObject gameObject = null;
		GameObject[] array2 = array;
		foreach (GameObject gameObject2 in array2)
		{
			if (!gameObject)
			{
				gameObject = gameObject2;
			}
			if (Vector3.Distance(goSource.transform.position, gameObject2.transform.position) <= Vector3.Distance(goSource.transform.position, gameObject.transform.position))
			{
				gameObject = gameObject2;
			}
		}
		return gameObject;
	}

	public static Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		return new Color32(r, g, b, byte.MaxValue);
	}

	public static Sprite GetSprite(string strDirectory, string strSpriteName)
	{
		if (!strDirectory.EndsWith("/"))
		{
			strDirectory += "/";
		}
		strSpriteName = strSpriteName.Replace("/", "");
		return Resources.Load<Sprite>(strDirectory + strSpriteName);
	}

	public static Vector2 MultiplyBy(Vector2 v1, Vector2 v2)
	{
		Vector2 result = default(Vector2);
		result.x = v1.x * v2.x;
		result.y = v1.y * v2.y;
		return result;
	}

	public static Vector3 MultiplyBy(Vector3 v1, Vector3 v2)
	{
		Vector3 result = default(Vector3);
		result.x = v1.x * v2.x;
		result.y = v1.y * v2.y;
		result.z = v1.z * v2.z;
		return result;
	}

	public static Vector2 MultiplyBy(Vector2 v1, Vector3 v2)
	{
		Vector2 result = default(Vector2);
		result.x = v1.x * v2.x;
		result.y = v1.y * v2.y;
		return result;
	}

	public static Vector2 MultiplyBy(Vector2 v1, float f)
	{
		Vector2 result = default(Vector2);
		result.x = v1.x * f;
		result.y = v1.y * f;
		return result;
	}

	public static Vector3 MultiplyBy(Vector3 v1, float f)
	{
		Vector3 result = default(Vector3);
		result.x = v1.x * f;
		result.y = v1.y * f;
		result.z = v1.z * f;
		return result;
	}

	public static Vector2 DivideBy(Vector2 v1, Vector2 v2)
	{
		Vector2 result = default(Vector2);
		result.x = v1.x / v2.x;
		result.y = v1.y / v2.y;
		return result;
	}

	public static Vector3 DivideBy(Vector3 v1, Vector3 v2)
	{
		Vector3 result = default(Vector3);
		result.x = v1.x / v2.x;
		result.y = v1.y / v2.y;
		result.z = v1.z / v2.z;
		return result;
	}

	public static Vector2 DivideBy(Vector2 v1, Vector3 v2)
	{
		Vector2 result = default(Vector2);
		result.x = v1.x / v2.x;
		result.y = v1.y / v2.y;
		return result;
	}

	public static Vector2 DivideBy(Vector2 v1, float f)
	{
		Vector2 result = default(Vector2);
		result.x = v1.x / f;
		result.y = v1.y / f;
		return result;
	}

	public static Vector3 DivideBy(Vector3 v1, float f)
	{
		Vector3 result = default(Vector3);
		result.x = v1.x / f;
		result.y = v1.y / f;
		result.z = v1.z / f;
		return result;
	}

	public static string GetSpeedLabel(char ch)
	{
		return ch switch
		{
			'N' => "kts", 
			'M' => "mph", 
			'K' => "kph", 
			_ => "mph", 
		};
	}

	public static string GetDistanceLabel(char ch)
	{
		return ch switch
		{
			'N' => "nm", 
			'M' => "miles", 
			'K' => "km", 
			_ => "miles", 
		};
	}

	public static void CopyToClipboard(string strText)
	{
		GUIUtility.systemCopyBuffer = strText;
	}

	public static string PasteFromClipboard()
	{
		return GUIUtility.systemCopyBuffer;
	}
}
