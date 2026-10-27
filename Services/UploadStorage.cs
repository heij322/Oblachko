namespace Oblachko.Services;

public class UploadStorage
{
    public string Folder { get; }

    public UploadStorage(IWebHostEnvironment env)
    {
        Folder = Directory.Exists("/data")
            ? "/data/uploads"
            : Path.Combine(env.WebRootPath, "uploads");

        Directory.CreateDirectory(Folder);
    }
}
