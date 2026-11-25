namespace Bison.Core;





// Decides from file extension to upload image, vid or audio
public static class TypeOfMedia 
{

    public static string For(string path?) 
    {
        var extension = System.IO.Path.GetExtension( path ?? "").ToLowerInvariant();
        return extension switch

        {
            ".mp4" or ".webm" => "video",
            ".mp3" or ".m4a" or ".ogg" => "audio",
            _ => "image"
        };

    }
}