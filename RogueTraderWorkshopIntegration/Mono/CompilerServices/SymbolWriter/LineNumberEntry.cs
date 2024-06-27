using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class LineNumberEntry
{
	public sealed class LocationComparer : IComparer<LineNumberEntry>
	{
		public static readonly LocationComparer Default = new LocationComparer();

		public int Compare(LineNumberEntry l1, LineNumberEntry l2)
		{
			if (l1.Row != l2.Row)
			{
				int row = l1.Row;
				return row.CompareTo(l2.Row);
			}
			return l1.Column.CompareTo(l2.Column);
		}
	}

	public readonly int Row;

	public int Column;

	public int EndRow;

	public int EndColumn;

	public readonly int File;

	public readonly int Offset;

	public readonly bool IsHidden;

	public static readonly LineNumberEntry Null = new LineNumberEntry(0, 0, 0, 0);

	public LineNumberEntry(int file, int row, int column, int offset)
		: this(file, row, column, offset, is_hidden: false)
	{
	}

	public LineNumberEntry(int file, int row, int offset)
		: this(file, row, -1, offset, is_hidden: false)
	{
	}

	public LineNumberEntry(int file, int row, int column, int offset, bool is_hidden)
		: this(file, row, column, -1, -1, offset, is_hidden)
	{
	}

	public LineNumberEntry(int file, int row, int column, int end_row, int end_column, int offset, bool is_hidden)
	{
		File = file;
		Row = row;
		Column = column;
		EndRow = end_row;
		EndColumn = end_column;
		Offset = offset;
		IsHidden = is_hidden;
	}

	public override string ToString()
	{
		return $"[Line {File}:{Row},{Column}-{EndRow},{EndColumn}:{Offset}]";
	}
}
