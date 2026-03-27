using AutoMapper;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Mappings;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<CreateCourseViewModel, Course>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Instructor, opt => opt.Ignore())
            .ForMember(dest => dest.Currency, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Sections, opt => opt.Ignore())
            .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore())
            .ForMember(dest => dest.CourseTags, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.CurrenciesList, opt => opt.Ignore())
            .ForMember(dest => dest.CategoriesList, opt => opt.Ignore())
            .ForMember(dest => dest.TagsList, opt => opt.Ignore())
            .ForMember(dest => dest.SelectedTagIds, opt => opt.Ignore());

        CreateMap<Course, CourseListViewModel>()
            .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.InstructorId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Title : "Uncategorized"))
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : "$"))
            .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src =>
                src.Instructor != null
                    ? $"{src.Instructor.FirstName} {src.Instructor.LastName}".Trim()
                    : "Unknown Instructor"))
            .ForMember(dest => dest.InstructorProfilePicture, opt => opt.MapFrom(src =>
                src.Instructor != null && !string.IsNullOrEmpty(src.Instructor.ProfilePicture)
                    ? src.Instructor.ProfilePicture
                    : "/images/profiles/default-avatar.png"));

        CreateMap<Course, CourseDetailsViewModel>()
            .ForMember(dest => dest.InstructorId, opt => opt.MapFrom(src => src.InstructorId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Title : "Uncategorized"))
            .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Name : "Unknown"))
            .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : "$"))
            .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src =>
                src.Instructor != null
                    ? $"{src.Instructor.FirstName} {src.Instructor.LastName}".Trim()
                    : "Unknown Instructor"))
            .ForMember(dest => dest.InstructorProfilePicture, opt => opt.MapFrom(src =>
                src.Instructor != null && !string.IsNullOrEmpty(src.Instructor.ProfilePicture)
                    ? src.Instructor.ProfilePicture
                    : "/images/profiles/default-avatar.png"))
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SectionCount, opt => opt.MapFrom(src => src.Sections != null ? src.Sections.Count : 0))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                src.Reviews != null && src.Reviews.Any()
                    ? Math.Round(src.Reviews.Average(r => r.Rating), 2)
                    : 0))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.CourseTags != null
                    ? src.CourseTags.Select(courseTag => courseTag.Tag != null ? courseTag.Tag.Name : string.Empty).ToList()
                    : new List<string>()))
            .ForMember(dest => dest.TotalLessons, opt => opt.MapFrom(src =>
                src.Sections != null ? src.Sections.SelectMany(section => section.Lessons).Count() : 0));

        CreateMap<Review, ReviewDetailsViewModel>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                src.Student != null
                    ? $"{src.Student.FirstName} {src.Student.LastName}".Trim()
                    : "Unknown Student"))
            .ForMember(dest => dest.StudentProfilePicture, opt => opt.MapFrom(src =>
                src.Student != null && !string.IsNullOrEmpty(src.Student.ProfilePicture)
                    ? $"/images/profiles/{src.Student.ProfilePicture}"
                    : "/images/profiles/default-avatar.png"));
    }
}
