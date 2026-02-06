using Microsoft.IdentityModel.Tokens;
using Poliview.crm.domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Poliview.crm.services
{
    public static class TokenService
    {

        public static string GenerateJwtToken(int codigoUsuario, string nomeUsuario, string email, string cpfcnpj, Jwt jwt)
        {
            return GenerateJwtToken(codigoUsuario, nomeUsuario, email, cpfcnpj, jwt.Subject, jwt.Issuer, jwt.Audience, jwt.key);
        }

        public static string GenerateJwtToken(int codigoUsuario, string nomeUsuario, string email, string cpfcnpj, string subject, string issuer, string audience, string key)
        {            
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, subject),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("id", codigoUsuario.ToString()),
                    new Claim("nome", nomeUsuario),
                    new Claim("email", email),
                    new Claim("cpfcnpj", cpfcnpj),

                };

			// var key = Encoding.ASCII.GetBytes("your-256-bit-secret");

			var keystring = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signin = new SigningCredentials(keystring, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                            issuer,
                            audience,
                            claims,
                            expires: DateTime.Now.AddDays(5),
                            signingCredentials: signin
                );
            var strToken = new JwtSecurityTokenHandler().WriteToken(token);
            return strToken;
        }

        public static int? ValidateJwtToken(string token, Jwt jwt)
        {
            return ValidateJwtToken(token, jwt.key);
        }

        public static int? ValidateJwtToken(string token, string key)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var keystring = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signin = new SigningCredentials(keystring, SecurityAlgorithms.HmacSha256);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = keystring,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                // return account id from JWT token if validation successful
                return accountId;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }

        public static string NomeUsuario(string token, Jwt jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keystring = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = keystring,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                }, out SecurityToken validatedToken); ;

                var jwtToken = (JwtSecurityToken)validatedToken;
                var nome = jwtToken.Claims.First(x => x.Type == "nome").Value;
                return nome;
            }
            catch (Exception ex)
            {
                // return null if validation fails                
                return ex.Message;
            }
        }

        public static UserJwt UserInfoJwtToken(string token, Jwt jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keystring = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.key));

            var ret = new UserJwt();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = keystring,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                }, out SecurityToken validatedToken); ;

                var jwtToken = (JwtSecurityToken)validatedToken;
                var nome = jwtToken.Claims.First(x => x.Type == "nome").Value;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value;
                var id = jwtToken.Claims.First(x => x.Type == "id").Value;
                var cpfcnpj = jwtToken.Claims.First(x => x.Type == "cpfcnpj").Value;

                ret.id = id;
                ret.nome = nome;
                ret.email = email;
                ret.cpfcnpj = cpfcnpj;

                return ret;
            }
            catch (Exception ex)
            {
                // return null if validation fails                
                return ret;
            }
        }
    }
}
