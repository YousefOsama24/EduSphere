using EduSphere.Services.IServices;
using System.Threading.Tasks;

namespace EduSphere.Services
{
    public enum MovieImgType
    {
        MainImg,
        SubImg

    }
    public class ImgesService : IImgesService
    {
        public async Task<string> CreateFileAsync(IFormFile file, string folder)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fullPath = Path.Combine(path, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        public string GetOldFilePath(string fileName, string folder)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder, fileName);
        }
    }
}
