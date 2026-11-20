namespace Bison.Core;

public static class PictureLicenses
{
    public static readonly IReadOnlyList<(string Code, string Label)> Options =
        new List<(string Code, string Label)>
        {
            ("CC0", "CC0 1.0 (public domain dedication)"),
            ("CC BY", "CC BY 4.0 (attribution)"),
            ("CC BY-SA", "CC BY-SA 4.0 (attribution, share alike)"),
            ("CC BY-ND", "CC BY-ND 4.0 (attribution, no derivatives)"),
            ("CC BY-NC", "CC BY-NC 4.0 (attribution, non-commercial)"),
            ("CC BY-NC-SA", "CC BY-NC-SA 4.0 (attribution, non-commercial, share alike)"),
            ("CC BY-NC-ND", "CC BY-NC-ND 4.0 (attribution, non-commercial, no derivatives)"),
        };

    public static bool IsValid(string? code) =>
        string.IsNullOrWhiteSpace(code) || Options.Any(o => o.Code == code);

    public static string? LabelFor(string? code) =>
        code is null ? null : Options.FirstOrDefault(o => o.Code == code).Label;
}
