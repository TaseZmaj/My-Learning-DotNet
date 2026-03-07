using CsharpTest.Utils;
using CsharpTest.StringUtils;

class Program 
{
    private static void Main(string[] args)
    {
        const string name = "Stefan";
        const string surname = "Tasevski";
        const int age = 21;
             
        Console.WriteLine($"My name is {name} {surname}, and im {age} years old.");
        Console.WriteLine($" 2 + 3 = {Matematika.Zbir(2,3)}");
        Console.WriteLine($" 5 - 3 = {Matematika.Razlika(5,3)}");
        
        Console.WriteLine("");
        Console.WriteLine($"The number 200, formated as a price is {PriceFormatter.FormatPrice(2000)}");
    }
}
