namespace Application.Common.Utility.Validator;

public static class ValidatorMassege
{
    public static string GetMaxLengthErrorMessage(string Name, int maximum)
    {
        return @$"{Name} نمی تواند بیشتر از {maximum} کاراکتر باشد.";
    }

    public static string GetMinLengthErrorMessage(string Name, int min)
    {
        return @$"{Name} باید حداقل {min} کاراکتر باشد.";
    }

    public static string GetNotEmptyErrorMessage(string Name)
    {
        return @$"{Name} اجباری می باشد.";
    }

    public static string GetGreaterThanErrorMessage(string Name, long value)
    {
        return @$"{Name} نمی تواند بزرگ تر از {value} باشد.";
    }

    public static string GetLessThanErrorMessage(string Name, long value)
    {
        return @$"{Name} نمی تواند کوچک تر از {value} باشد.";
    }
}