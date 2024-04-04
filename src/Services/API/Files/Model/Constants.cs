using System.IO;

namespace API.Files.Model
{
    public static class Constants
    {
        public static string BASE_PATH { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "volume");
        public static readonly string[] DocuTemplateExtensions = new string[] { ".docx", ".docm", ".pptx", ".pptm", ".xlsx", ".xlsm" };
        public static readonly string[] ImageExtensions = new string[] { ".jpg", "jpeg" };
        public static readonly string[] ConfigExtensions = new string[] { ".json" };
    }
}