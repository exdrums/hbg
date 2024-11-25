using System;
using System.IO;
using System.Threading.Tasks;

namespace API.Files.Model;

public abstract class BaseImage
{
    public int Id { get; set; }
    public string FileName { get; set; } // The name of the file on the server
    public string OriginalName { get; set; } // The original name of the file uploaded
    public string FileType { get; set; } // e.g., "image/jpeg"
    public long FileSize { get; set; } // File size in bytes
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } // UserId of the uploader

    public abstract string GetFolderPath(); // Define folder path dynamically in derived classes

    public async Task SaveToFileSystemAsync(Stream fileStream)
    {
        var folderPath = GetFolderPath();
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var filePath = Path.Combine(folderPath, FileName);
        await using var output = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(output);
    }

    public FileStream GetFileStream()
    {
        var folderPath = GetFolderPath();
        var filePath = Path.Combine(folderPath, FileName);
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public bool DeleteFromFileSystem()
    {
        var folderPath = GetFolderPath();
        var filePath = Path.Combine(folderPath, FileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }
}

public class ProjectPlanImage : BaseImage
{
    public int ProjectId { get; set; } // The ID of the project the image belongs to
    public override string GetFolderPath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "Uploads", $"Projects/{ProjectId}/Plan/{Id}");
    }
}