using CsharpTest.Models.Interfaces;
namespace CsharpTest.Models;
public class Course : ICourseEnrollment
{
    public string Name { get; }
    public int CourseId { get; }
    public int Credits { get; }
    
    public List<Student> Students { get; }

    public Dictionary<Student, int> Grades { get; }
    
    public Course(string name, int courseId, int credits)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name cannot be null or whitespace.", nameof(name));
            
        Name = name;
        CourseId = courseId;
        Credits = credits;
        Students = [];
        Grades = new Dictionary<Student, int>();
    }

    public void EnrollStudent(Student student)
    {
        Students.Add(student);
        student.EnrollInCourse(this);
    }

    public void UnenrollStudent(Student student)
    {
        Students.Remove(student);
        student.UnenrollFromCourse(this);
    }

    public List<Student> GetEnrolledStudents()
    {
        return Students;
    }

    public Student? GetTopPerformingStudent()
    {
        // return 
        //     Grades
        //         .OrderByDescending(g => g.Value)
        //         .First()
        //         .Key;

        //Ova ima POEFIKASNA VREMENSKA KOMPLEKSNOST negoli ova gore
        return Grades.MaxBy(g => g.Value).Key;
    }
}