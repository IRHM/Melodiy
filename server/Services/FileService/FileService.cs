using melodiy.server.Data.File;
using melodiy.server.Services.AuthService;

namespace melodiy.server.Services.FileService
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IAuthService _authService;

        public FileService(IFileRepository fileRepository, IAuthService authService)
        {
            _fileRepository = fileRepository;
            _authService = authService;
        }

        public async Task<ServiceResponse<string>> UploadImage(IFormFile image)
        {
            string username = _authService.GetUsername();
            ServiceResponse<string> response = new();

            if (image == null || image.Length == 0 || !IsValidImageContentType(image.ContentType))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Invalid file type.";
                return response;
            }

            try
            {
                response = await _fileRepository.UploadImage(image, username);
            }
            catch
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Unexpected Server Error.";
            }

            return response;
        }

        public async Task<ServiceResponse<string>> UploadSong(IFormFile song)
        {
            string username = _authService.GetUsername();
            ServiceResponse<string> response = new();

            if (song == null || song.Length == 0 || !IsValidAudioContentType(song.ContentType))
            {
                response.Success = false;
                response.StatusCode = 400;
                response.Message = "Invalid file type.";
                return response;
            }

            try
            {
                response = await _fileRepository.UploadSong(song, username);
            }
            catch
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Unexpected Server Error.";
            }
            return response;
        }

        private static bool IsValidImageContentType(string contentType)
        {
            return contentType.StartsWith("image/");
        }

        private static bool IsValidAudioContentType(string contentType)
        {
            return contentType is "audio/wav" or "audio/mpeg";
        }

        public async Task<ServiceResponse<bool>> DeleteFile(string bucket, string path)
        {
            ServiceResponse<bool> response = new();
            try
            {
                _ = await _fileRepository.DeleteFile(bucket, path);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.StatusCode = 500;
            }

            return response;
        }

    }
}