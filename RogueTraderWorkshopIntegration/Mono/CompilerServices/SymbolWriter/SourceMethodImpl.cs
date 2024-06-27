namespace Mono.CompilerServices.SymbolWriter;

internal class SourceMethodImpl : IMethodDef
{
	private string name;

	private int token;

	private int namespaceID;

	public string Name => name;

	public int NamespaceID => namespaceID;

	public int Token => token;

	public SourceMethodImpl(string name, int token, int namespaceID)
	{
		this.name = name;
		this.token = token;
		this.namespaceID = namespaceID;
	}
}
