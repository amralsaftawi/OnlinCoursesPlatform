using AutoMapper;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Mappings;

public class LessonProfile : Profile
{
    public LessonProfile()
    {
        CreateMap<Section, SectionDetailsViewModel>()
            .ForMember(dest => dest.Lessons, opt => opt.MapFrom(src =>
                src.Lessons.OrderBy(lesson => lesson.OrderIndex)));

        CreateMap<Lesson, LessonDetailsViewModel>()
            .ForMember(dest => dest.SectionOrderIndex, opt => opt.MapFrom(src => src.Section.OrderIndex))
            .ForMember(dest => dest.SectionTitle, opt => opt.MapFrom(src => src.Section.Title))
            .ForMember(dest => dest.ArticleContent, opt => opt.Ignore())
            .ForMember(dest => dest.Quiz, opt => opt.Ignore());

        CreateMap<Lesson, LessonEditorResultDto>()
            .ForMember(dest => dest.LessonId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.QuizQuestionType, opt => opt.Ignore())
            .ForMember(dest => dest.QuizPrompt, opt => opt.Ignore())
            .ForMember(dest => dest.QuizOptionA, opt => opt.Ignore())
            .ForMember(dest => dest.QuizOptionB, opt => opt.Ignore())
            .ForMember(dest => dest.QuizOptionC, opt => opt.Ignore())
            .ForMember(dest => dest.QuizOptionD, opt => opt.Ignore())
            .ForMember(dest => dest.QuizCorrectOption, opt => opt.Ignore())
            .ForMember(dest => dest.QuizCorrectTrueFalse, opt => opt.Ignore())
            .ForMember(dest => dest.QuizReferenceAnswer, opt => opt.Ignore());
    }
}
