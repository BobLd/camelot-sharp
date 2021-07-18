NuGet packages available on the [releases](https://github.com/BobLd/camelot-sharp/releases) page and on www.nuget.org:
- [Camelot](https://www.nuget.org/packages/Camelot)
- [Camelot.ImageProcessing.OpenCvSharp4](https://www.nuget.org/packages/Camelot.ImageProcessing.OpenCvSharp4)

# camelot-sharp
A C# library to extract tabular data from PDFs (port of camelot Python version using PdfPig).

Original Python source code available here: [camelot-dev/camelot](https://github.com/camelot-dev/camelot).

[![Windows](https://github.com/BobLd/camelot-sharp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/BobLd/camelot-sharp/actions/workflows/dotnet.yml)

# Usage
## Stream mode 
```csharp
using (PdfDocument doc = PdfDocument.Open(@"Files\foo.pdf", new ParsingOptions() { ClipPaths = true }))
{
	Stream stream = new Stream();
	var tables = stream.ExtractTables(doc.GetPage(1));

	Assert.Single(tables);
	Assert.Equal((612, 792), stream.Dimensions);
	Assert.Equal(612, stream.PdfWidth);
	Assert.Equal(792, stream.PdfHeight);
	//Assert.Equal(84, stream.HorizontalText.Count);

	var parsingReport = tables[0].ParsingReport();
	//   parsing_report = {"accuracy": 99.02, "whitespace": 12.24, "order": 1, "page": 1}
	parsingReport["order"] = 1;
	parsingReport["page"] = 1;
}
```

## Lattice mode
```csharp
using (var doc = PdfDocument.Open(@"Files\column_span_2.pdf", new ParsingOptions() { ClipPaths = true }))
{
	var page = doc.GetPage(1);

	Lattice lattice = new Lattice(new OpenCvImageProcesser(), new BasicSystemDrawingProcessor(), line_scale: 40);
	var tables = lattice.ExtractTables(page,
		layout_kwargs: new DlaOptions[]
		{
			new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
			{
				WithinLineMultiplier = 2
			}
		});
	Assert.Single(tables);
	Assert.Equal(DataLatticeShiftTextLeftTop.Length, tables[0].Cells.Count);
	Assert.Equal(DataLatticeShiftTextLeftTop, tables[0].Data().Select(r => r.Select(c => c).ToArray()).ToArray());
}

```
