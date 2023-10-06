namespace API.Extensions
{
    public static class DatetimeExtensions
    {
        public static int CalcuateAge(this DateOnly dob)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            
            var age = today.Year - dob.Year;

            if (dob > today.AddDays(-age)) age--;

            return age;
        }
    }
}
