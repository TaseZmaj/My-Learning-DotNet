namespace CsharpTest.Models.Interfaces;

public interface ICourseEnrollment
{
    void EnrollStudent(Student student);
    void UnenrollStudent(Student student);

    List<Student> GetEnrolledStudents();
}