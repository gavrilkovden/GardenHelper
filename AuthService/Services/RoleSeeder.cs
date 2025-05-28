using Microsoft.AspNetCore.Identity;

namespace AuthService.Services
{
    // Статический класс, отвечающий за начальное создание (сидирование) ролей в системе
    public static class RoleSeeder
    {
        // Метод для выполнения сидирования ролей (создания ролей при запуске приложения)
        public static async Task SeedRolesAsync(WebApplication app)
        {
            // Создаем область видимости (scope), чтобы получить доступ к сервисам внутри DI-контейнера (Dependency Injection)
            using var scope = app.Services.CreateScope();

            // Получаем сервис RoleManager, с помощью которого можно управлять ролями (создавать, проверять наличие и т.д.)
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Массив ролей, которые мы хотим создать, если их еще нет
            var roles = new[] { "User", "Admin" };

            // Проходимся по каждой роли и проверяем, существует ли она
            foreach (var role in roles)
            {
                // Если роли еще нет в базе данных — создаем её
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}

