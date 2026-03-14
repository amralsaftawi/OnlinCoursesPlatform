using OnlineCoursesPlatform.Models;
using System.Collections.Generic;

namespace OnlineCoursesPlatform.ViewModels
{
    public class ManageCourseContentViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();
    }
}
