namespace Kodu24.Api.Storage;

/// <summary>Hoiab pilte kaustas <c>uploads/</c> (API projekti juurkaustas, git-ignore'itud).</summary>
public class FileImageStorage : IImageStorage
{
    public const string FolderName = "uploads";

    private readonly string _folder;

    public FileImageStorage(IWebHostEnvironment env)
    {
        _folder = GetFolder(env);
        Directory.CreateDirectory(_folder);
    }

    public static string GetFolder(IWebHostEnvironment env) => Path.Combine(env.ContentRootPath, FolderName);

    public async Task<string> SaveAsync(Stream content, string extension)
    {
        // Kasutaja failinime ei kasutata kunagi: nii ei saa keegi teise faili üle kirjutada
        // ega kirjutada väljapoole kausta (nt "../../Program.cs").
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var file = File.Create(Path.Combine(_folder, fileName));
        await content.CopyToAsync(file);
        return fileName;
    }

    public void Delete(string fileName)
    {
        var path = Path.Combine(_folder, Path.GetFileName(fileName));
        if (File.Exists(path))
            File.Delete(path);
    }
}
