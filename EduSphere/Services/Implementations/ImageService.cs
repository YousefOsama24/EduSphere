using EduSphere.Services.Interfaces;

namespace EduSphere.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;

        private readonly string[] _allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private const long MaxFileSize = 2 * 1024 * 1024;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> UploadImageAsync(
            IFormFile image,
            string folderName)
        {
            if (image == null || image.Length == 0)
                return null;

            string extension =
                Path.GetExtension(image.FileName)
                .ToLower();

            if (!_allowedExtensions.Contains(extension))
                throw new Exception("Unsupported image format.");

            if (image.Length > MaxFileSize)
                throw new Exception("Image size must not exceed 2 MB.");

            string uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    folderName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName =
                $"{Guid.NewGuid()}{extension}";

            string fullPath =
                Path.Combine(
                    uploadsFolder,
                    fileName);

            using FileStream stream =
                new(fullPath, FileMode.Create);

            await image.CopyToAsync(stream);

            return fileName;
        }

        public void DeleteImage(
            string fileName,
            string folderName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            string path =
                Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    folderName,
                    fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}