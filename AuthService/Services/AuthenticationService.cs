using AuthService.Domain;
using AuthService.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    public class AuthenticationService
    {
        // UserManager — встроенный сервис Identity, который управляет пользователями (создание, проверка пароля, добавление в роли и т.д.)
        private readonly UserManager<ApplicationUser> _userManager;

        // IConfiguration — позволяет получить значения из файла конфигурации (например, appsettings.json), в нашем случае — настройки JWT
        private readonly IConfiguration _configuration;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        // Метод регистрации нового пользователя
        public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
        {
            // Создаем нового пользователя на основе email
            var user = new ApplicationUser
            {
                UserName = dto.Email, // Имя пользователя — email
                Email = dto.Email     // Email
            };

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return IdentityResult.Failed(new IdentityError { Description = "Email already exists." });

            // Создаем пользователя с заданным паролем
            var result = await _userManager.CreateAsync(user, dto.Password);

            // Если регистрация прошла успешно, добавляем пользователя в роль "User"
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Возвращаем результат создания (успех/ошибки)
            return result;
        }

        // Метод входа в систему (аутентификация)
        public async Task<string?> LoginAsync(LoginDto dto)
        {
            // Ищем пользователя по email
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Проверяем, что пользователь найден и пароль верный
            if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                // Получаем все роли, в которых состоит пользователь (например, "User", "Admin")
                var userRoles = await _userManager.GetRolesAsync(user);

                // Создаем список claims — это "утверждения", которые будут зашиты в JWT-токен
                // ClaimTypes.Name — имя пользователя (в данном случае, email)
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

                // Добавляем в claims каждую роль, в которой состоит пользователь
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Получаем ключ из конфигурации (appsettings.json → Jwt:Key)
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                // Создаем подпись токена с использованием алгоритма HmacSha256
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Создаем JWT-токен с указанными параметрами:
                // - issuer: кто выпустил токен
                // - audience: кто может использовать токен
                // - claims: данные о пользователе
                // - expires: срок действия токена
                // - signingCredentials: чем подписан токен
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],          // от кого токен
                    audience: _configuration["Jwt:Audience"],      // для кого токен
                    claims: claims,                                 // список данных о пользователе
                    expires: DateTime.Now.AddMinutes(60),           // срок действия токена — 60 минут
                    signingCredentials: creds                       // подпись токена
                );

                // Возвращаем готовую строку токена
                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            // Если пользователь не найден или пароль неверный — возвращаем null
            return null;
        }
    }
}
