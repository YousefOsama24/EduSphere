using Microsoft.AspNetCore.Http;

namespace EduSphere.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(
            IFormFile image,
            string folderName);

        void DeleteImage(
            string fileName,
            string folderName);
    }
}