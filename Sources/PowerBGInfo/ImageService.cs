using ImagePlayground;

namespace PowerBGInfo;

public class ImageService
{
    public Image Load(string filePath) => Image.Load(filePath);
    public void Save(Image image, string filePath) => image.Save(filePath);
}
