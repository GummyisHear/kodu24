namespace Kodu24.Api.Storage;

/// <summary>
/// Piltide failid kettal. Andmebaasi läheb ainult failinimi (<c>ListingImage.FileName</c>).
/// Liides on eraldi, et teenust saaks testida ilma päris failideta.
/// </summary>
public interface IImageStorage
{
    /// <summary>Salvestab pildi uue juhusliku nimega ja tagastab selle nime (nt "3f2a….jpg").</summary>
    Task<string> SaveAsync(Stream content, string extension);

    /// <summary>Kustutab faili. Puuduv fail ei ole viga.</summary>
    void Delete(string fileName);
}
