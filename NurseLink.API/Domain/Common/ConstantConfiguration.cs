namespace NurseLink.API.Domain.Common
{
    public static class ConstantConfiguration
    {
        public static readonly string accessAllowed = "AllowAll";

        public static readonly string jwtKey = "Jwt:Key";
        public static readonly string jwtIssuer = "Jwt:Issuer";
        public static readonly string jwtAudience = "Jwt:Audience";
        public static readonly string jwtExpireMinutes = "Jwt:ExpireMinutes";

        public static readonly string defaultConnection = "DefaultConnection";
    }
}