using CsharpTest.Models.Interfaces;
namespace CsharpTest.Models;

public abstract class Student : IStudentEnrollment
{
    public string Name { get; }
    public string Surname { get; }
    private DateTime? _yearOfEnrollment;
    
    public DateTime? YearOfEnrollment
    {
        get => _yearOfEnrollment ?? DateTime.Now;
        private set => _yearOfEnrollment = value;
    }
    
    
    public int Index { get; }
    public List<Course> Courses { get; }
    public Dictionary<Course, int> Grades { get; }

    protected Student(string name, string surname, DateTime yearOfEnrollment, int index)
    {
        Name = name;
        Surname = surname;
        _yearOfEnrollment = yearOfEnrollment;
        Index = index;
        Courses = [];
        Grades = [];
    }

    public void EnrollInCourse(Course course)
    {
        if (course == null) throw new ArgumentNullException(nameof(course));
        if ( GetEnrolledCourses().Contains(course)) return;
        
        Courses.Add(course);
        course.EnrollStudent(this);
    }

    public void UnenrollFromCourse(Course course)
    {
        Courses.Remove(course);
        course.UnenrollStudent(this);
    }

    public List<Course> GetEnrolledCourses()
    {
        return Courses;
    }

    public bool IsEnrolledIn(Course course)
    {
        return Courses.Contains(course);
    }
}