using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using worksystem.Models;
using System.Text;

namespace worksystem.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly List<string> _invalidatedTokens = new();

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
       
        public string GenerateToken(Employee employee)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Role, employee.UserRole.ToString())
            };
            
            if (!Enum.IsDefined(typeof(UserRole), employee.UserRole))
                throw new InvalidOperationException("Invalid jogkör");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing")
                )
            );
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(
                Convert.ToDouble(
                    _configuration["Jwt:TokenLifetimeInMinutes"] ?? "60" // Default 60 minutes
                )
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void InvalidateToken(string token) 
        {
            _invalidatedTokens.Add(token);
        }

        public int? ValidateToken(string token)
        {
            if(_invalidatedTokens.Contains(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            }
            catch
            {
                return null;
            }
        }

        public (string Token, string RefreshToken) GenerateTokenPair(Employee employee)
        {
            var token = GenerateToken(employee);
            var refreshToken = Guid.NewGuid().ToString();
            return (token, refreshToken);
        }
    }
}