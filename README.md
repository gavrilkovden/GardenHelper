🌱 Smart Garden Backend (Microservices Architecture)

Проект — интеллектуальная система помощи дачнику на основе микросервисной архитектуры. Система анализирует данные о растениях, почве и погоде, чтобы автоматически рекомендовать действия: полив, удобрение, защита от заморозков и др.

---

🧱 Архитектура

![Architecture Diagram](link-to-image-if-you-have-one)

Состоит из следующих микросервисов:

- **DataInputService** — ввод данных о растениях и почве
- **WeatherService** — получает прогноз погоды по координатам пользователя (через OpenWeather API), кэширует в Redis
- **AnalysisService** — анализирует данные из очереди RabbitMQ и передаёт в AI
- **AIAnalysisService** — обращается к AI-модели и генерирует рекомендации
- **RecommendationService** — рассылает рекомендации (Email/SMS/API)
- **AuthService** — регистрация и вход пользователей через ASP.NET Identity + JWT
- **API Gateway** — маршрутизация через Ocelot

---

⚙️ Технологии

- ASP.NET Core Web API
- Docker + Docker Compose
- SQL Server, Redis
- RabbitMQ
- Ocelot API Gateway
- ASP.NET Core Identity + JWT
- OpenWeatherMap API
- AI (DeepSeek/OpenRouter)
