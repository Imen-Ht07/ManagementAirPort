namespace GestionAirPort.Constants
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Passenger = "Passenger";

        public static readonly string[] All = new[]
        {
            Administrator,
            Passenger
        };
    }
}