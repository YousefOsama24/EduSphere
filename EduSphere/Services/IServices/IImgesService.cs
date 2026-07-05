

namespace EduSphere.Services.IServices
{
    public interface IImgesService
    {
        //Task<string> CreateFileAsync(IFormFile Img, MovieImgType movieImgType = MovieImgType.MainImg);

        //string GetOldFilePath(string oldFileName, MovieImgType movieImgType = MovieImgType.MainImg);

         Task<string> CreateFileAsync(IFormFile file, string folder);
         string GetOldFilePath(string fileName, string folder);
        
    }
}
