using System.Diagnostics;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.Data;

public class FileDatabaseClientData
{
	private static FileDatabaseClientData m_instance;

	public bool IndexingClientReadWriteLogEnabled;

	public ProcessWindowStyle BlueprintIndexingServerProcessWindowStyle;

	public static FileDatabaseClientData Instance => m_instance ?? (m_instance = new FileDatabaseClientData());
}
