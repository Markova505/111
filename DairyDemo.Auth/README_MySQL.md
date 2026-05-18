# Подключение MySQL к проекту DairyDemo.Auth

## Требования
1. Установленный MySQL Server (версия 5.7 или 8.0+)
2. phpMyAdmin (опционально, для управления БД)

## Настройка базы данных

### Вариант 1: Автоматическое создание (рекомендуется)
При первом запуске приложение автоматически:
- Создаст базу данных `dairy_auth` (если не существует)
- Создаст таблицу `users`
- Добавит тестовых пользователей (admin/admin и user/user)

### Вариант 2: Ручное создание через phpMyAdmin

1. Откройте phpMyAdmin в браузере (обычно http://localhost/phpmyadmin)
2. Войдите под пользователем root
3. Создайте новую базу данных с именем `dairy_auth`
4. Выполните SQL-запрос:

```sql
CREATE TABLE IF NOT EXISTS users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Login VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    IsBlocked BOOLEAN DEFAULT FALSE,
    FailedAttempts INT DEFAULT 0
);

INSERT INTO users (Login, PasswordHash, Role, IsBlocked, FailedAttempts) VALUES 
('admin', '$2a$11$...', 'Admin', FALSE, 0),
('user', '$2a$11$...', 'User', FALSE, 0);
```

## Настройка подключения

Откройте файл `Data/Db.MySql.cs` и измените строку подключения:

```csharp
private static readonly string ConnectionString = 
    "Server=localhost;Database=dairy_auth;Uid=root;Pwd=ВАШ_ПАРОЛЬ;SslMode=none;";
```

Параметры:
- `Server` - адрес сервера MySQL (обычно localhost)
- `Database` - имя базы данных
- `Uid` - пользователь MySQL (по умолчанию root)
- `Pwd` - пароль пользователя MySQL
- `SslMode` - режим SSL (none для локальной разработки)

## Установка пакета MySql.Data

В Visual Studio:
1. Правый клик на проекте → "Manage NuGet Packages"
2. Найдите и установите `MySql.Data`

Или через терминал:
```bash
dotnet add package MySql.Data
```

## Тестовые учетные данные
- Логин: `admin`, Пароль: `admin` (роль: Администратор)
- Логин: `user`, Пароль: `user` (роль: Пользователь)

## Возможные ошибки

### "Unable to connect to any of the specified MySQL hosts"
- Проверьте, запущен ли MySQL сервер
- Проверьте правильность параметров подключения

### "Access denied for user 'root'@'localhost'"
- Неверный пароль в строке подключения
- Проверьте права доступа пользователя

### "Unknown database 'dairy_auth'"
- База данных не создана
- Запустите приложение еще раз (создастся автоматически) или создайте вручную
