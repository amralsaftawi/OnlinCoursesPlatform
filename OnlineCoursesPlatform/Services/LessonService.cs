using Microsoft.EntityFrameworkCore;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineCoursesPlatform.Services
{
    public class LessonService : ILessonService
    {
        private readonly IRepository<Section> _sectionRepository;
        private readonly IRepository<Lesson> _lessonRepository;

        public LessonService(IRepository<Section> sectionRepository, IRepository<Lesson> lessonRepository)
        {
            _sectionRepository = sectionRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<Section> AddSectionAsync(int courseId, string title)
        {
            // Calculate the next OrderIndex for the section
            var existingSectionsCount = await _sectionRepository.GetQueryable()
                .Where(s => s.CourseId == courseId)
                .CountAsync();

            var section = new Section
            {
                CourseId = courseId,
                Title = title,
                OrderIndex = existingSectionsCount + 1
            };

            var createdSection = await _sectionRepository.AddAsync(section);
            await _sectionRepository.SaveAsync();

            return createdSection;
        }

        public async Task<Lesson> AddLessonAsync(int sectionId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree)
        {
            // Calculate the next OrderIndex for the lesson in the section
            var existingLessonsCount = await _lessonRepository.GetQueryable()
                .Where(l => l.SectionId == sectionId)
                .CountAsync();

            var lesson = new Lesson
            {
                SectionId = sectionId,
                Title = title,
                Type = type,
                ContentUrl = contentUrl,
                Duration = duration,
                IsFree = isFree,
                OrderIndex = existingLessonsCount + 1
            };

            var createdLesson = await _lessonRepository.AddAsync(lesson);
            await _lessonRepository.SaveAsync();

            return createdLesson;
        }

        public async Task<IEnumerable<Section>> GetSectionsByCourseIdAsync(int courseId)
        {
            return await _sectionRepository.GetQueryable()
                .Where(s => s.CourseId == courseId)
                .Include(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .OrderBy(s => s.OrderIndex)
                .ToListAsync();
        }

        public async Task<Lesson> GetLessonByIdAsync(int lessonId)
        {
            return await _lessonRepository.GetByIdAsync(lessonId);
        }

        public async Task<Lesson> UpdateLessonAsync(int lessonId, string title, Models.Enums.LessonType type, string contentUrl, int duration, bool isFree)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return null;

            lesson.Title = title;
            lesson.Type = type;
            lesson.ContentUrl = contentUrl;
            lesson.Duration = duration;
            lesson.IsFree = isFree;

            var updatedLesson = await _lessonRepository.UpdateAsync(lesson);
            await _lessonRepository.SaveAsync();

            return updatedLesson;
        }

        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            var result = await _lessonRepository.DeleteAsync(lessonId);
            if (result)
            {
                await _lessonRepository.SaveAsync();
            }
            return result;
        }
    }
}
