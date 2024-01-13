using System.Drawing.Imaging;

namespace hoa7mlishe.API.Helpers
{
    public static class Dictionaries
    {
        public static Dictionary<string, ImageFormat> ImageFormats => new()
        {
            { ".png", ImageFormat.Png },
            { ".jpg", ImageFormat.Jpeg },
            { ".jpeg", ImageFormat.Jpeg },
            { ".gif", ImageFormat.Jpeg },
        };

        public static Dictionary<string, string> ExtensionToDataFormat => new()
        {
            { "png", "image/png" },
            { "jpeg", "image/jpeg" },
            { "jpg", "image/jpeg" },
            { "gif", "image/gif" },
        };
    }
}
