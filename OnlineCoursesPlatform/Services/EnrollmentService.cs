using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Services.Interfaces;

namespace OnlineCoursesPlatform.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;

    public EnrollmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EnrollmentActionResultDto> EnrollAsync(int courseId, int studentId)
    {
        var course = await _context.Courses
            .Include(c => c.Sections)
                .ThenInclude(section => section.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
        {
            return new EnrollmentActionResultDto
            {
                Succeeded = false,
                Errors = ["The specified course does not exist."]
            };
        }

        if (course.Status != CourseStatus.Approved)
        {
            return new EnrollmentActionResultDto
            {
                Succeeded = false,
                Errors = ["This course is not available for enrollment yet."]
            };
        }

        if (course.InstructorId == studentId)
        {
            return new EnrollmentActionResultDto
            {
                Succeeded = false,
                Errors = ["You already own this course. Use the instructor tools to manage it instead."]
            };
        }

        var isAlreadyEnrolled = await _context.Enrollments.AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId);
        if (isAlreadyEnrolled)
        {
            return new EnrollmentActionResultDto
            {
                Succeeded = false,
                Errors = ["You are already enrolled in this course."]
            };
        }

        _context.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            ProgressPercentage = 0
        });

        await _context.SaveChangesAsync();

        var firstLessonId = course.Sections
            .OrderBy(section => section.OrderIndex)
            .SelectMany(section => section.Lessons.OrderBy(lesson => lesson.OrderIndex))
            .Select(lesson => (int?)lesson.Id)
            .FirstOrDefault();

        return new EnrollmentActionResultDto
        {
            Succeeded = true,
            Message = "Enrolled successfully! Happy learning.",
            FirstLessonId = firstLessonId
        };
    }

    public async Task<EnrollmentConfirmationResultDto> GetConfirmationAsync(int courseId, int studentId)
    {
        var course = await _context.Courses
            .Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null || course.Status != CourseStatus.Approved)
        {
            return new EnrollmentConfirmationResultDto
            {
                NotFound = true
            };
        }

        if (course.InstructorId == studentId)
        {
            return new EnrollmentConfirmationResultDto
            {
                RedirectToCourseDetails = true,
                MessageKey = "Error",
                Message = "You own this course already. Open it from your instructor workspace instead of enrolling."
            };
        }

        var isAlreadyEnrolled = await _context.Enrollments.AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId);
        if (isAlreadyEnrolled)
        {
            return new EnrollmentConfirmationResultDto
            {
                RedirectToCourseDetails = true,
                MessageKey = "Success",
                Message = "You are already enrolled in this course."
            };
        }

        return new EnrollmentConfirmationResultDto
        {
            Course = course
        };
    }
}
