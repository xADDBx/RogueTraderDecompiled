using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kingmaker.QA.Arbiter;

internal class ReportEntityAggregator
{
	private int m_SceneCount;

	private IEnumerable<string> m_NotStarted;

	private IEnumerable<string> m_FromSkippedInstruction;

	private IEnumerable<string> m_FromFailedInstruction;

	private string m_EntityName;

	public ReportEntityAggregator(IEnumerable<AbstractReportEntity> reportEntities)
	{
		AbstractReportEntity abstractReportEntity = reportEntities.FirstOrDefault();
		if (abstractReportEntity is SceneReportEntity)
		{
			m_EntityName = "Сцены";
		}
		if (abstractReportEntity is AreaReportEntity)
		{
			m_EntityName = "Локации";
		}
		Aggregate(reportEntities.Select((AbstractReportEntity x) => x as AbstractAggregatedReportEntity));
	}

	private void Aggregate(IEnumerable<AbstractAggregatedReportEntity> reportEntities)
	{
		m_SceneCount = reportEntities.Count();
		m_NotStarted = from x in reportEntities
			where !x.IsStarted && !x.IsFromFailedInstruction && !x.IsFromSkippedInstruction
			select x.Name into x
			orderby x
			select x;
		m_FromSkippedInstruction = from x in reportEntities
			where x.IsFromSkippedInstruction
			select x.Name + " (" + x.ExternalLogId + ")" into x
			orderby x
			select x;
		m_FromFailedInstruction = from x in reportEntities
			where x.IsFromFailedInstruction
			select x.Name + " (" + x.ExternalLogId + ")" into x
			orderby x
			select x;
	}

	public void WriteTestcaseToXml(XmlWriter xmlWriter)
	{
		WriteCoverageToXml(xmlWriter);
		WriteSkippedToXml(xmlWriter);
		WriteFailedToXml(xmlWriter);
	}

	private void WriteCoverageToXml(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("testcase");
		xmlWriter.WriteAttributeString("name", "Покрытие (" + m_EntityName + ")");
		int num = m_NotStarted.Count();
		int num2 = m_SceneCount - num;
		if (m_NotStarted.Any())
		{
			xmlWriter.WriteStartElement("failure");
			xmlWriter.WriteString($"Покрытие арбитром: {num2} из {m_SceneCount}." + $"\nНе запускались в текущем прогоне ({num}):\n\t");
			xmlWriter.WriteString(string.Join("\n\t", m_NotStarted));
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}

	private void WriteSkippedToXml(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("testcase");
		xmlWriter.WriteAttributeString("name", "Фатальные ошибки (краши)");
		if (m_FromSkippedInstruction.Any())
		{
			xmlWriter.WriteStartElement("failure");
			xmlWriter.WriteString("При выполнении части инструкций игра зависла или упала." + $"\n{m_EntityName} из этих инструкций ({m_FromSkippedInstruction.Count()}):\n\t");
			xmlWriter.WriteString(string.Join("\n\t", m_FromSkippedInstruction));
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}

	private void WriteFailedToXml(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("testcase");
		xmlWriter.WriteAttributeString("name", "Ошибки (вылет в меню)");
		if (m_FromFailedInstruction.Any())
		{
			xmlWriter.WriteStartElement("failure");
			xmlWriter.WriteString("При выполнении части инструкций произошли ошибки" + $"\n{m_EntityName} из этих инструкций ({m_FromFailedInstruction.Count()}):\n\t");
			xmlWriter.WriteString(string.Join("\n\t", m_FromFailedInstruction));
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}
}
