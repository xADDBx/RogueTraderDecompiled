using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Core.Overlays;
using UnityEngine;

namespace Kingmaker.Utility.Performance;

public class ObjectLimitsOverlay : MonoBehaviour
{
	private class LimitInfo
	{
		private readonly ObjectLimits.Entry m_Entry;

		public int Value { get; private set; }

		public string Name => m_Entry.Name;

		public int Threshold => m_Entry.Threshold;

		public Label.Severity Severity
		{
			get
			{
				if (Value > Threshold)
				{
					if (!((double)Value <= (double)Threshold * 1.1))
					{
						return Label.Severity.Error;
					}
					return Label.Severity.Warning;
				}
				return Label.Severity.Info;
			}
		}

		public LimitInfo(ObjectLimits.Entry entry)
		{
			m_Entry = entry;
		}

		public void UpdateValue()
		{
			Value = m_Entry.Getter();
		}
	}

	public const string OverlayName = "Limits";

	private readonly List<LimitInfo> m_Limits = new List<LimitInfo>();

	private void Start()
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			Object.Destroy(this);
			return;
		}
		List<OverlayElement> list = new List<OverlayElement>();
		ObjectLimits.Entry[] entries = ObjectLimits.Entries;
		foreach (ObjectLimits.Entry entry in entries)
		{
			LimitInfo info = new LimitInfo(entry);
			m_Limits.Add(info);
			list.Add(new Label(entry.Name, TextGetter, SeverityGetter));
			Label.Severity SeverityGetter()
			{
				return info.Severity;
			}
			string TextGetter()
			{
				return $"{info.Value}/{info.Threshold}";
			}
		}
		Overlay o = new Overlay("Limits", list.ToArray());
		OverlayService.Instance?.RegisterOverlay(o);
	}

	private void LateUpdate()
	{
		foreach (LimitInfo limit in m_Limits)
		{
			limit.UpdateValue();
		}
	}

	private void OnGUI()
	{
		Overlay overlay = OverlayService.Instance?.Get("Limits");
		if (overlay == null || OverlayService.Instance?.Current == overlay)
		{
			return;
		}
		float num = (float)(Camera.main?.pixelHeight ?? 0) - 35f;
		foreach (Label item in Enumerable.Reverse(overlay.Labels))
		{
			Label.Severity? severity = item.GetSeverity?.Invoke();
			bool flag;
			if (severity.HasValue)
			{
				Label.Severity valueOrDefault = severity.GetValueOrDefault();
				if ((uint)(valueOrDefault - 1) <= 1u)
				{
					flag = false;
					goto IL_00a5;
				}
			}
			flag = true;
			goto IL_00a5;
			IL_00a5:
			if (!flag)
			{
				item.Draw(num);
				num -= 19f;
			}
		}
	}
}
