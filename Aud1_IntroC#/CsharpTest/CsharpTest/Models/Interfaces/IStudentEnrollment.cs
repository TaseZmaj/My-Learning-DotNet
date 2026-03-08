namespace CsharpTest.Models.Interfaces;

public interface IStudentEnrollment
{
    void EnrollInCourse(Course course);
    void UnenrollFromCourse(Course course);
    
    List<Course> GetEnrolledCourses();
    bool IsEnrolledIn(Course course);
}