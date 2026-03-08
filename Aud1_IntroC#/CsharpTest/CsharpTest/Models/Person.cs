namespace CsharpTest.Models;

public class Person
{
    public String Name { get; set; }
    public String Surname { get;  }
    public int Age { get; }

    public Person(string name, string surname, int age)
    {
        Name = name;
        Surname = surname;
        Age = age;
    }
}