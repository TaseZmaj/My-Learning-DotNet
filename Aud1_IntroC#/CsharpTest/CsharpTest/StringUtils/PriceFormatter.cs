namespace CsharpTest.StringUtils;

public class PriceFormatter
{
    public static string FormatPrice(decimal price)
    {
        return "$" + price + ".00";
    }
}