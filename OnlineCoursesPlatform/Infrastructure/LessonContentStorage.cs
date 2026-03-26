using Microsoft.AspNetCore.Hosting;

namespace OnlineCoursesPlatform.Infrastructure
{
    internal static class LessonContentStorage
    {
        public const string ArticleUploadFolder = "uploads/articles";
        private const string ArticleUploadPrefix = "/uploads/articles/";

        public static string GetWebRootPath(IWebHostEnvironment environment)
        {
            if (!string.IsNullOrWhiteSpace(environment.WebRootPath))
            {
                return environment.WebRootPath;
            }

            return Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        public static string GetArticleUploadsPhysicalPath(IWebHostEnvironment environment)
        {
            return Path.Combine(GetWebRootPath(environment), "uploads", "articles");
        }

        public static string BuildRelativeArticlePath(string fileName)
        {
            return $"{ArticleUploadPrefix}{fileName}";
        }

        public static bool IsLocalArticleUpload(string? contentUrl)
        {
            return !string.IsNullOrWhiteSpace(contentUrl)
                && contentUrl.StartsWith(ArticleUploadPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static void DeleteLocalArticleFile(IWebHostEnvironment environment, string? contentUrl)
        {
            if (!IsLocalArticleUpload(contentUrl))
            {
                return;
            }

            var webRootPath = GetWebRootPath(environment);
            var relativePath = contentUrl!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
            var uploadsRoot = Path.GetFullPath(GetArticleUploadsPhysicalPath(environment));

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
