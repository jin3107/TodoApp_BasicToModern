# TodoApp - Basic to Modern

A comprehensive full-stack Todo application built with modern technologies, featuring a React TypeScript frontend and .NET Core backend with advanced features like caching, background jobs, and detailed reporting.

## 🚀 Features

### Core Functionality
- ✅ **Todo Lists Management** - Create, edit, delete, and organize todo lists
- ✅ **Todo Items Management** - Full CRUD operations with rich metadata
- ✅ **Priority System** - High, Medium, Low priority levels
- ✅ **Status Tracking** - Track completion status with timestamps
- ✅ **Due Dates** - Set and manage due dates for tasks
- ✅ **Search & Filter** - Advanced search across all todo items
- ✅ **Pagination** - Efficient data loading with pagination

### Advanced Features
- 📊 **Dashboard Analytics** - Visual insights into productivity
- 📈 **Progress Reports** - Detailed completion statistics
- 📅 **Daily Trends** - Track completion patterns over time
- 🔄 **Background Jobs** - Automated tasks using Quartz.NET
- 📧 **Email Reports** - Scheduled daily/weekly email summaries
- ⚡ **Redis Caching** - High-performance data caching
- 🐳 **Docker Support** - Containerized deployment
- 🔒 **Nginx Reverse Proxy** - Production-ready web server
- 🎨 **Modern UI/UX** - Clean, responsive design with Ant Design

## 🛠️ Tech Stack

### Frontend
- **React 18** - Modern React with hooks
- **TypeScript** - Type-safe development
- **Vite** - Fast build tool and dev server
- **Ant Design** - Professional UI component library
- **Axios** - HTTP client with interceptors
- **Day.js** - Modern date manipulation library
- **SCSS** - Enhanced styling capabilities

### Backend
- **.NET 8** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API development
- **Entity Framework Core** - ORM with Code First approach
- **MySQL** - Primary database
- **Redis** - Caching and session storage
- **Quartz.NET** - Background job scheduling
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging

### Architecture
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling
- **CQRS** - Command Query Responsibility Segregation
- **Caching Strategy** - Multi-level caching with Redis

## 📁 Project Structure

```
TodoApp_BasicToModern/
├── TodoApp.Client/                 # React TypeScript Frontend
│   ├── src/
│   │   ├── components/            # Reusable UI components
│   │   ├── pages/                # Page components
│   │   ├── apis/                 # API service layers
│   │   ├── interfaces/           # TypeScript interfaces
│   │   ├── helpers/              # Utility functions
│   │   └── scss/                 # Global styles
│   └── public/                   # Static assets
│
└── TodoApp.Server/               # .NET Core Backend
    └── src/
        ├── Todo.API/             # Web API controllers
        ├── Todo.Services/        # Business logic layer
        ├── Todo.Repositories/    # Data access layer
        ├── Todo.Models/          # Entity models
        ├── Todo.DTOs/           # Data transfer objects
        └── Todo.Commons/        # Shared utilities
```

## 🚦 Getting Started

### Prerequisites

#### For Development
- **Node.js** 18+ 
- **.NET 8 SDK**
- **MySQL 8.0+**
- **Redis** (optional, for caching)
- **Visual Studio 2022** or **VS Code**

#### For Docker Deployment
- **Docker** 20.10+
- **Docker Compose** 2.0+

### Backend Setup

1. **Clone and navigate to server directory**
   ```bash
   git clone <repository-url>
   cd TodoApp_BasicToModern/TodoApp.Server/src
   ```

2. **Configure database connection**
   ```json
   // appsettings.json
   {
     "ConnectionString": "Server=localhost;Database=TodoAppDB;Uid=root;Pwd=your_password;"
   }
   ```

3. **Run database migrations**
   ```bash
   cd Todo.API
   dotnet ef database update
   ```

4. **Install dependencies and run**
   ```bash
   dotnet restore
   dotnet run
   ```
   
   🌐 **API will be available at:** `https://localhost:7196`
   📚 **Swagger documentation:** `https://localhost:7196/swagger`

### Frontend Setup

1. **Navigate to client directory**
   ```bash
   cd TodoApp_BasicToModern/TodoApp.Client
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure API endpoint (if needed)**
   ```typescript
   // src/configs/axios.ts
   const api = axios.create({
     baseURL: "https://localhost:7196",
   });
   ```

4. **Start development server**
  # Docker Deployment (Recommended for Production)

1. **Create environment file**
   ```bash
   # Copy and edit .env file
   cp .env.example .env
   nano .env
   ```
   
   **Configure these variables:**
   ```bash
   # Email Settings (for scheduled reports)
   SMTP_USERNAME=your-email@gmail.com
   SMTP_PASSWORD=your-gmail-app-password
   RECIPIENT_EMAIL=recipient@example.com
   
   # Redis Password
   REDIS_reports/progress` - Get progress report
- `POST /reports/snapshot` - Create daily snapshot
- `GET /reports/daily-completion-trend` - Daily completion statistics
- `GET /reports/priority-distribution` - Priority distribution data

### Jobs (Admin Only)
- `POST /jobs/daily-report/trigger` - Manually trigger daily email report
- `POST /jobs/weekly-summary/trigger` - Manually trigger weekly summary
- `POST /jobs/task-reminder/trigger` - Manually trigger task reminder
2. **Configure Nginx (Optional)**
   ```bash
   # Copy and edit nginx config
   cp nginx/conf.d/todoapp.conf.example nginx/conf.d/todoapp.conf
   nano nginx/conf.d/todoapp.conf
   
   # Update:
   # - server_name: your domain
   # - allow: your IP address
   ```

3. **Start all services**
   ```bash
   docker-compose up -d
   ```
   
   🌐 **Services will be available at:**
   - Frontend: `http://localhost` (port 80)
   - Backend API: `http://localhost/api`
   - MySQL: `localh (Quartz.NET Scheduler)
```csharp
// Automated email reports
- Daily Task Report: 6:00 PM every day
- Weekly Summary: 9:00 AM every Monday  
- Task Reminder: 8:00 AM every day
```

### Email Configuration
Configure SMTP settings for email reports:

**Development Mode** (`appsettings.Development.json`):
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "RecipientEmail": "recipient@example.com"
  }
}
```

**Production Mode** (Docker - `.env` file):
```bash
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
RECIPIENT_EMAIL=recipient@example.com
```

> **Note:** For Gmail, use [App Passwords](https://support.google.com/accounts/answer/185833) instead of regular password.
   ```bash
   docker-compose logs -f
   ```

5. *� Deployment to VPS

1. **Prepare VPS**
   ```bash
   # Install Docker and Docker Compose
   curl -fsSL https://get.docker.com -o get-docker.sh
   sh get-docker.sh
   ```

2. **Clone repository**
   ```bash
   git clone https://github.com/jin3107/TodoApp_BasicToModern.git
   cd TodoApp_BasicToModern
   ```

3. **Create environment file**
   ```bash
   nano .env
   # Add your SMTP and Redis credentials
   ```

4. **Configure Nginx (Optional)**
   ```bash
   cp nginx/conf.d/todoapp.conf.example nginx/conf.d/todoapp.conf
   nano nginx/conf.d/todoapp.conf
   # Update domain and IP whitelist
   ```

5. **Start services**
   ```bash
   docker-compose up -d
   ```

6. **Check logs**
   ```bash
   docker-compose logs -f backend
   ```

> **Security Note:** Files `.env` and `nginx/conf.d/todoapp.conf` are gitignored. Create them manually on the server.

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection**
   - Ensure MySQL is running
   - Check connection string format
   - Verify credentials

2. **CORS Issues**
   - Confirm API URL in axios config
   - Check CORS policy in backend

3. **Cache Issues**
   - Restart Redis server
   - Clear cache keys manually if needed

4. **Docker Issues**
   - Check if ports 80, 3306, 6379 are available
   - Verify `.env` file exists and has correct values
   - Run `docker-compose logs` to see errors

5. **Email Not Sending**
   - Verify SMTP credentials in `.env` (Production) or `appsettings.Development.json` (Development)
   - For Gmail, ensure you're using App Password, not regular password
   - Check Quartz job logs: `docker-compose logs backend | grep Email`

6 `DELETE /todo-lists/{id}` - Delete todo list
- `POST /todo-lists/search` - Search todo lists

### Todo Items
- `GET /todo-items/{id}` - Get todo item by ID
- `POST /todo-items` - Create new todo item
- `PUT /todo-items` - Update todo item
- `DELETE /todo-items/{id}` - Delete todo item
- `POST /todo-items/search` - Search todo items

### Reports (5-30 min TTL)
- **Pagination** for large datasets
- **Connection Pooling** for database connections
- **Lazy Loading** in React components
- **Bundle Optimization** with Vite
- **Nginx Reverse Proxy** with rate limiting (10 req/s)
- **Docker Multi-stage Builds** for smaller images
## 🔧 Configuration

### Database Configuration
```json
{
- **Rate Limiting** via Nginx (10 requests/second)
- **IP Whitelisting** for admin endpoints (/jobs, /swagger)
- **Environment Variables** for sensitive data (not committed to Git)
  "ConnectionString": "Server=localhost;Database=TodoApp;Uid=root;Pwd=password;",
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### Caching Strategy
- **Search Results**: 5-minute expiration
- **Individual Records**: 10-minute expiration
- **Cache Keys**: Structured with operation and filters
- **Cache Invalidation**: Automatic on CUD operations

### Background Jobs
- **Daily Reports**: Generate daily completion statistics
- **Cleanup Tasks**: Remove old completed items
- **Cache Warming**: Pre-load frequently accessed data

## 🎯 Key Features Implementation

### Smart Caching
```csharp
// Cache key includes filters for accurate invalidation
private string GenerateSearchCacheKey(SearchRequest request)
{
    return $"todo-item:search:page:{request.PageIndex}:filters:{SerializeFilters(request.Filters)}";
}
```

### Advanced Filtering
```typescript
// Client-side filter building
const filters: Filter[] = [
  {
    fieldName: "TodoListId",
    value: todoListId,
    operation: "Equals"
  }
];
```

### Real-time Updates
```typescript
// Automatic refresh after operations
const onItemsChange = useCallback(() => {
  loadTodoLists(currentPage, searchText);
}, [currentPage, searchText]);
```

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection**
   - Ensure MySQL is running
   - Check connection string format
   - Verify credentials

2. **CORS Issues**
   - Confirm API URL in axios config
   - Check CORS policy in backend

3. **Cache Issues**
   - Restart Redis server
   - Clear cache keys manually if needed

4. **Build Errors**
   - Run `dotnet restore` for backend
   - Run `npm install` for frontend
   - Check .NET and Node versions

## 📈 Performance Optimizations

- **Database Indexing** on frequently queried fields
- **Redis Caching** for expensive queries
- **Pagination** for large datasets
- **Connection Pooling** for database connections
- **Lazy Loading** in React components
- **Bundle Optimization** with Vite

## 🔒 Security Features

- **Input Validation** on all endpoints
- **CORS Configuration** for cross-origin requests
- **Error Handling** with proper status codes
- **SQL Injection Prevention** with parameterized queries

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## 👨‍💻 Author

Built with ❤️ by Rainy

---

⭐ **If you found this project helpful, please give it a star!**
