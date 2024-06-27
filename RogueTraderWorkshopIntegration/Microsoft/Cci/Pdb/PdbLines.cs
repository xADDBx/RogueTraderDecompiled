namespace Microsoft.Cci.Pdb;

internal class PdbLines
{
	internal PdbSource file;

	internal PdbLine[] lines;

	internal PdbLines(PdbSource file, uint count)
	{
		this.file = file;
		lines = new PdbLine[count];
	}
}
