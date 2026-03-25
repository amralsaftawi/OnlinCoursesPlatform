namespace OnlineCoursesPlatform.Dtos
{
    public class AdminRevenueDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
    }
}
