using Microsoft.AspNetCore.Hosting;

namespace OnlineCoursesPlatform.Infrastructure
{
    internal static class LessonContentStorage
    {
        public const string ArticleUploadFolder = "uploads/articles";
        public const string PdfUploadFolder = "uploads/pdfs";
        private const string ArticleUploadPrefix = "/uploads/articles/";
        private const string PdfUploadPrefix = "/uploads/pdfs/";

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

        public static string GetPdfUploadsPhysicalPath(IWebHostEnvironment environment)
        {
            return Path.Combine(GetWebRootPath(environment), "uploads", "pdfs");
        }

        public static string BuildRelativeArticlePath(string fileName)
        {
            return $"{ArticleUploadPrefix}{fileName}";
        }

        public static string BuildRelativePdfPath(string fileName)
        {
            return $"{PdfUploadPrefix}{fileName}";
        }

        public static bool IsLocalArticleUpload(string? contentUrl)
        {
            return !string.IsNullOrWhiteSpace(contentUrl)
                && contentUrl.StartsWith(ArticleUploadPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLocalPdfUpload(string? contentUrl)
        {
            return !string.IsNullOrWhiteSpace(contentUrl)
                && contentUrl.StartsWith(PdfUploadPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsManagedLessonUpload(string? contentUrl)
        {
            return IsLocalArticleUpload(contentUrl) || IsLocalPdfUpload(contentUrl);
        }

        public static async Task<string?> ReadLocalArticleContentAsync(IWebHostEnvironment environment, string? contentUrl)
        {
            var filePath = TryResolveManagedFilePath(environment, contentUrl, ArticleUploadPrefix, GetArticleUploadsPhysicalPath(environment));
            if (filePath == null || !File.Exists(filePath))
            {
                return null;
            }

            return await File.ReadAllTextAsync(filePath);
        }

        public static void DeleteLocalArticleFile(IWebHostEnvironment environment, string? contentUrl)
        {
            DeleteManagedLessonFile(environment, contentUrl);
        }

        public static void DeleteManagedLessonFile(IWebHostEnvironment environment, string? contentUrl)
        {
            if (string.IsNullOrWhiteSpace(contentUrl))
            {
                return;
            }

            string? uploadsRoot = null;
            string? uploadPrefix = null;

            if (IsLocalArticleUpload(contentUrl))
            {
                uploadsRoot = GetArticleUploadsPhysicalPath(environment);
                uploadPrefix = ArticleUploadPrefix;
            }
            else if (IsLocalPdfUpload(contentUrl))
            {
                uploadsRoot = GetPdfUploadsPhysicalPath(environment);
                uploadPrefix = PdfUploadPrefix;
            }

            if (uploadsRoot == null || uploadPrefix == null)
            {
                return;
            }

            var fullPath = TryResolveManagedFilePath(environment, contentUrl, uploadPrefix, uploadsRoot);
            if (fullPath != null && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private static string? TryResolveManagedFilePath(IWebHostEnvironment environment, string? contentUrl, string uploadPrefix, string uploadsRoot)
        {
            if (string.IsNullOrWhiteSpace(contentUrl) || !contentUrl.StartsWith(uploadPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var webRootPath = GetWebRootPath(environment);
            var relativePath = contentUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
            var normalizedUploadsRoot = Path.GetFullPath(uploadsRoot);

            if (!fullPath.StartsWith(normalizedUploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return fullPath;
        }
    }
}
