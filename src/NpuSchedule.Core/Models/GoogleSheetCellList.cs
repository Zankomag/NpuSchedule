using System.Collections.Generic;

namespace NpuSchedule.Core.Models; 

public class GoogleSheetCellList {
	public IList<Sheet>? Sheets { get; set; } = null!;
}

public class Sheet {
	public IList<Data>? Data { get; set; } = null!;
}

public class Data {
	public IList<RowData>? RowData { get; set; } = null!;
}


public class RowData {

	public IList<Value>? Values { get; set; } = null!;

}


public class Value {
	public EffectiveValue EffectiveValue { get; set; } = null!;

	public string? Hyperlink { get; set; }
}


public class EffectiveValue {

	public string StringValue { get; set; } = null!;

}