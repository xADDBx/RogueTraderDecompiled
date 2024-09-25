using System;
using System.Xml;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

internal class InstructionReportEntity : AbstractReportEntity
{
	[JsonProperty]
	public InstructionStatus Status;

	[JsonProperty]
	public TimeSpan TestTime { get; set; }

	[JsonProperty]
	public string Error { get; set; }

	public override void WriteTestCaseToXml(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("testcase");
		xmlWriter.WriteAttributeString("name", base.Name);
		xmlWriter.WriteAttributeString("time", ((int)TestTime.TotalSeconds).ToString());
		if (Status == InstructionStatus.Error)
		{
			xmlWriter.WriteStartElement("failure");
			xmlWriter.WriteString(Error);
			xmlWriter.WriteString("\n\nЛог: " + base.ExternalLogId);
			xmlWriter.WriteEndElement();
		}
		else
		{
			InstructionStatus status = Status;
			if (status == InstructionStatus.NotStarted || status == InstructionStatus.Started || status == InstructionStatus.Restarted)
			{
				xmlWriter.WriteStartElement("skipped");
				xmlWriter.WriteEndElement();
			}
			else if (Status != InstructionStatus.Passed)
			{
				xmlWriter.WriteStartElement("failure");
				xmlWriter.WriteString("Неизвестная ошибка. Возможно зависание или вылет.");
				xmlWriter.WriteString("\nЛог: " + base.ExternalLogId);
				xmlWriter.WriteEndElement();
			}
		}
		xmlWriter.WriteEndElement();
	}
}
