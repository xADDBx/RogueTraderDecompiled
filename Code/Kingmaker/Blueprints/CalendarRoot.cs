using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.UI.Common;

namespace Kingmaker.Blueprints;

public class CalendarRoot : StringsContainer
{
	public string StartDate = "01.01.2018";

	public string StartTime = "12:00";

	public int YearsShift = 2700;

	public TextHolder[] Months;

	public TextHolder[] DaysOfWeek;

	public FormatEntry[] Formats;

	public TimePeriodStrings TimePeriodStrings;

	[NonSerialized]
	private bool m_Initialized;

	[NonSerialized]
	private DateTime m_StartDate;

	[NonSerialized]
	private TimeSpan m_StartTime;

	public TimeSpan GetStartTime()
	{
		InitDates();
		return m_StartTime;
	}

	public DateTime GetStartDate()
	{
		InitDates();
		if (Game.Instance.Player.StartDate.HasValue)
		{
			return Game.Instance.Player.StartDate.Value;
		}
		return m_StartDate;
	}

	public string GetDateText(TimeSpan gameTime, GameDateFormat formatType = GameDateFormat.Full, bool monthWithRomanNumber = true)
	{
		FormatEntry formatEntry = Formats.FirstOrDefault((FormatEntry f) => f.Type == formatType);
		if (formatEntry == null)
		{
			return $"<unknown date format {formatType}>";
		}
		return GetDateText(gameTime, formatEntry.Format, monthWithRomanNumber);
	}

	public string GetDateText(TimeSpan gameTime, string format, bool monthWithRomanNumber = true)
	{
		InitDates();
		DateTime dateTime = GetStartDate() + gameTime;
		int num = dateTime.Year + YearsShift;
		string monthText = GetMonthText(dateTime.Month, monthWithRomanNumber);
		int day = dateTime.Day;
		string dayWithEndingText = GetDayWithEndingText(day);
		string dayOfWeekText = GetDayOfWeekText(dateTime.DayOfWeek);
		return format.Replace("(day)", day.ToString()).Replace("(day_with_ending)", dayWithEndingText).Replace("(day_of_week)", dayOfWeekText)
			.Replace("(month)", monthText)
			.Replace("(year)", num.ToString());
	}

	public string GetCurrentWHDateText()
	{
		return $"{Game.Instance.Player.GetVVYears()}VV. " + $"{Game.Instance.Player.GetAMRCYears()}AMRC. " + $"M{Game.Instance.Player.GetMillennium()}";
	}

	public string GetCurrentDateText(GameDateFormat formatType = GameDateFormat.Full)
	{
		return GetDateText(Game.Instance.TimeController.GameTime, formatType);
	}

	public string GetCurrentDayOfWeak()
	{
		return GetDateText(Game.Instance.TimeController.GameTime, GameDateFormat.DayOfWeek);
	}

	public string GetPeriodString(TimeSpan period, string daysColor = "", string hoursColor = "")
	{
		int days = period.Days;
		int hours = period.Hours;
		string text = ((daysColor != "") ? ("<color=#" + daysColor + ">" + days + "</color>") : days.ToString());
		string text2 = ((hoursColor != "") ? ("<color=#" + hoursColor + ">" + hours + "</color>") : hours.ToString());
		string text3 = "";
		if (days >= 1)
		{
			text3 += text;
			if (LocalizationManager.Instance.CurrentLocale != Locale.zhCN)
			{
				text3 += " ";
			}
			text3 = ((days >= 10 && (LocalizationManager.Instance.CurrentLocale != Locale.ruRU || days / 10 == 1)) ? (text3 + TimePeriodStrings.Days) : ((days % 10) switch
			{
				1 => text3 + TimePeriodStrings.OneDay, 
				2 => text3 + TimePeriodStrings.TwoDays, 
				3 => text3 + TimePeriodStrings.ThreeDays, 
				4 => text3 + TimePeriodStrings.FourDays, 
				_ => text3 + TimePeriodStrings.Days, 
			}));
			if (hours > 0)
			{
				text3 = ((!TimePeriodStrings.And.IsSet()) ? (text3 + " ") : string.Concat(text3, " ", TimePeriodStrings.And, " "));
			}
		}
		if (hours > 0)
		{
			text3 += text2;
			if (LocalizationManager.Instance.CurrentLocale != Locale.zhCN)
			{
				text3 += " ";
			}
			text3 = ((hours >= 10 && (LocalizationManager.Instance.CurrentLocale != Locale.ruRU || hours / 10 == 1)) ? (text3 + TimePeriodStrings.Hours) : ((hours % 10) switch
			{
				1 => text3 + TimePeriodStrings.OneHour, 
				2 => text3 + TimePeriodStrings.TwoHours, 
				3 => text3 + TimePeriodStrings.ThreeHours, 
				4 => text3 + TimePeriodStrings.FourHours, 
				_ => text3 + TimePeriodStrings.Hours, 
			}));
		}
		if (days == 0 && hours == 0)
		{
			text3 = TimePeriodStrings.LessThanAnHour;
		}
		return text3;
	}

	public string GetCompactPeriodString(TimeSpan period)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		if (period.Days > 0)
		{
			stringBuilder.Append(period.Days).Append(TimePeriodStrings.CompactDay);
			num++;
		}
		if (num < 2 && (period.Hours > 0 || num > 0))
		{
			if (num > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(period.Hours).Append(TimePeriodStrings.CompactHour);
			num++;
		}
		if (num < 2 && (period.Minutes > 0 || num > 0))
		{
			if (num > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(period.Minutes).Append(TimePeriodStrings.CompactMinute);
			num++;
		}
		if (num < 2 && (period.Seconds > 0 || num > 0))
		{
			if (num > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(period.Seconds).Append(TimePeriodStrings.CompactSecond);
		}
		return stringBuilder.ToString();
	}

	public string GetMonthText(int month, bool monthWithRomanNumber = false)
	{
		if (month <= 0 || month > Months.Length)
		{
			return $"<uknown month {month}>";
		}
		LocalizedString localizedString = Months[month - 1].Name;
		if (monthWithRomanNumber)
		{
			return $"{localizedString} ({UIUtility.ArabicToRoman(month)})";
		}
		return localizedString;
	}

	public string GetDayOfWeekText(DayOfWeek day)
	{
		if (day < DayOfWeek.Sunday || (int)day >= DaysOfWeek.Length)
		{
			return $"<uknown day of week {day}>";
		}
		return DaysOfWeek[(int)day].Name;
	}

	private string GetDayWithEndingText(int day)
	{
		if (LocalizationManager.Instance.CurrentLocale != 0)
		{
			return day.ToString();
		}
		string text = "th";
		switch (day)
		{
		case 1:
		case 21:
		case 31:
			text = "st";
			break;
		case 2:
		case 22:
			text = "nd";
			break;
		case 3:
		case 23:
			text = "rd";
			break;
		}
		return day + text;
	}

	private void InitDates()
	{
		if (!m_Initialized)
		{
			try
			{
				m_StartDate = DateTime.ParseExact(StartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
				m_StartDate = m_StartDate.Date;
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			try
			{
				m_StartTime = TimeSpan.Parse(StartTime, CultureInfo.InvariantCulture);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2);
			}
			m_Initialized = true;
		}
	}
}
