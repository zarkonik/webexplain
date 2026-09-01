using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SkiaSharp;
using WebExplain.Api.Repositories;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace WebExplain.Api.Services;

public class GuideExportService(
    IGuideRepository guideRepository,
    ICaptureSessionRepository captureSessionRepository) : IGuideExportService
{
    private const long EmuPerInch = 914400;
    private const double MaxImageWidthInches = 6.2;

    public async Task<byte[]?> ExportToWordAsync(Guid guideId, Guid userId, CancellationToken cancellationToken = default)
    {
        var guide = await guideRepository.GetByIdAsync(guideId, userId);
        if (guide is null)
        {
            return null;
        }

        var screenshotPaths = new Dictionary<int, string>();
        if (guide.SourceCaptureSessionId is { } sessionId)
        {
            var session = await captureSessionRepository.GetByIdAsync(sessionId, userId);
            if (session is not null)
            {
                foreach (var page in session.Pages)
                {
                    screenshotPaths[page.Order] = page.ScreenshotFilePath;
                }
            }
        }

        using var stream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());
            AddHeading1Style(mainPart);

            body.AppendChild(CreateHeading(guide.Title));
            if (!string.IsNullOrWhiteSpace(guide.Description))
            {
                body.AppendChild(CreateParagraph(guide.Description, italic: true, color: "6B7280"));
            }
            body.AppendChild(CreateParagraph(guide.SourceUrl, color: "4F46E5"));
            body.AppendChild(CreateDivider());

            var orderedSteps = guide.Steps.OrderBy(s => s.Order).ToList();
            var isFirstStep = true;
            foreach (var step in orderedSteps)
            {
                // The description explains the action taken FOR this step, but the
                // screenshot that shows where to actually perform it is the state
                // BEFORE that action - i.e. the previous step's screenshot - not the
                // result captured right after clicking/typing.
                body.AppendChild(CreateStepHeading($"Step {step.Order}", pageBreakBefore: !isFirstStep));
                isFirstStep = false;

                var mainText = step.ElementDescription ?? "No description available for this step.";
                body.AppendChild(CreateParagraph(mainText, bold: true));

                if (screenshotPaths.TryGetValue(step.Order - 1, out var screenshotPath) && File.Exists(screenshotPath))
                {
                    var imageBytes = await File.ReadAllBytesAsync(screenshotPath, cancellationToken);
                    if (step.TargetWidth is > 0 && step.TargetHeight is > 0)
                    {
                        imageBytes = DrawHighlightBox(
                            imageBytes, step.TargetX ?? 0, step.TargetY ?? 0, step.TargetWidth.Value, step.TargetHeight.Value);
                    }
                    body.AppendChild(CreateImageParagraph(mainPart, imageBytes, $"step-{step.Order}"));
                }

                if (!string.IsNullOrWhiteSpace(step.Instruction))
                {
                    body.AppendChild(CreateParagraph(step.Instruction, italic: true, color: "4B5563"));
                }

                var pageUrl = step.PageUrl ?? guide.SourceUrl;
                body.AppendChild(CreateParagraph($"🔒 {pageUrl}", color: "6B7280", small: true));
            }

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static void AddHeading1Style(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        var normalStyle = new Style { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true };
        normalStyle.Append(new StyleName { Val = "Normal" });
        normalStyle.Append(new PrimaryStyle());
        styles.Append(normalStyle);

        var heading1Style = new Style { Type = StyleValues.Paragraph, StyleId = "Heading1" };
        heading1Style.Append(new StyleName { Val = "heading 1" });
        heading1Style.Append(new BasedOn { Val = "Normal" });
        heading1Style.Append(new NextParagraphStyle { Val = "Normal" });
        heading1Style.Append(new PrimaryStyle());
        heading1Style.Append(new StyleParagraphProperties(
            new KeepNext(),
            new SpacingBetweenLines { Before = "240", After = "160" },
            new OutlineLevel { Val = 0 }));
        heading1Style.Append(new StyleRunProperties(new Bold(), new FontSize { Val = "32" }));
        styles.Append(heading1Style);

        stylesPart.Styles = styles;
    }

    private static Paragraph CreateStepHeading(string text, bool pageBreakBefore)
    {
        var paragraphProperties = new ParagraphProperties();
        if (pageBreakBefore)
        {
            paragraphProperties.AppendChild(new PageBreakBefore());
        }
        paragraphProperties.AppendChild(new ParagraphStyleId { Val = "Heading1" });
        paragraphProperties.AppendChild(new Justification { Val = JustificationValues.Center });

        return new Paragraph(paragraphProperties, new Run(new RunProperties(new Bold()), new Text(text)));
    }

    private static Paragraph CreateHeading(string text)
    {
        var runProperties = new RunProperties(new Bold(), new FontSize { Val = "36" }); // 18pt
        var paragraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { Before = "240", After = "200" });

        return new Paragraph(paragraphProperties, new Run(runProperties, new Text(text)));
    }

    private static Paragraph CreateDivider()
    {
        var paragraphProperties = new ParagraphProperties(
            new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "E5E7EB", Space = 1 }),
            new SpacingBetweenLines { After = "300" });
        return new Paragraph(paragraphProperties);
    }

    private static Paragraph CreateParagraph(string text, bool bold = false, bool italic = false, string? color = null, bool small = false)
    {
        var runProperties = new RunProperties();
        if (bold) runProperties.AppendChild(new Bold());
        if (italic) runProperties.AppendChild(new Italic());
        if (color is not null) runProperties.AppendChild(new Color { Val = color });
        if (small) runProperties.AppendChild(new FontSize { Val = "18" }); // 9pt

        var run = new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        var paragraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "200" });
        return new Paragraph(paragraphProperties, run);
    }

    private static Paragraph CreateImageParagraph(MainDocumentPart mainPart, byte[] imageBytes, string relationshipTag)
    {
        var imagePart = mainPart.AddImagePart(ImagePartType.Png);
        using (var imageStream = new MemoryStream(imageBytes))
        {
            imagePart.FeedData(imageStream);
        }
        var relationshipId = mainPart.GetIdOfPart(imagePart);

        var (pixelWidth, pixelHeight) = ReadPngDimensions(imageBytes);
        var widthEmu = (long)(MaxImageWidthInches * EmuPerInch);
        var heightEmu = pixelWidth > 0
            ? (long)(widthEmu * ((double)pixelHeight / pixelWidth))
            : (long)(MaxImageWidthInches * EmuPerInch * 0.6);

        var drawing = new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = widthEmu, Cy = heightEmu },
                new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                new DW.DocProperties { Id = 1U, Name = relationshipTag },
                new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = relationshipTag },
                                new PIC.NonVisualPictureDrawingProperties()
                            ),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relationshipId },
                                new A.Stretch(new A.FillRectangle())
                            ),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0L, Y = 0L },
                                    new A.Extents { Cx = widthEmu, Cy = heightEmu }
                                ),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
                            )
                        )
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                )
            )
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            }
        );

        var paragraphProperties = new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "200" });
        return new Paragraph(paragraphProperties, new Run(drawing));
    }

    /// <summary>
    /// Draws a red highlight rectangle over the element this step acted on, so a reader of
    /// the exported document can see exactly where to click/type without relying on prose
    /// alone. The web viewer draws the same box as a CSS overlay instead, since it can be
    /// positioned live over the image - but a static Word document needs it baked into the
    /// picture itself.
    /// </summary>
    private static byte[] DrawHighlightBox(byte[] pngBytes, double x, double y, double width, double height)
    {
        using var original = SKBitmap.Decode(pngBytes);
        if (original is null)
        {
            return pngBytes;
        }

        using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height));
        var canvas = surface.Canvas;
        canvas.DrawBitmap(original, 0, 0, new SKSamplingOptions(), null);

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = new SKColor(239, 68, 68), // #EF4444
            StrokeWidth = 4,
            IsAntialias = true
        };

        var rect = SKRect.Create((float)x - 3, (float)y - 3, (float)width + 6, (float)height + 6);
        canvas.DrawRoundRect(rect, 5, 5, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static (int Width, int Height) ReadPngDimensions(byte[] pngBytes)
    {
        if (pngBytes.Length < 24)
        {
            return (0, 0);
        }

        // PNG signature (8 bytes) is followed by the IHDR chunk, which stores the image's
        // width and height as two big-endian 4-byte integers starting at byte 16.
        var width = (pngBytes[16] << 24) | (pngBytes[17] << 16) | (pngBytes[18] << 8) | pngBytes[19];
        var height = (pngBytes[20] << 24) | (pngBytes[21] << 16) | (pngBytes[22] << 8) | pngBytes[23];
        return (width, height);
    }
}
