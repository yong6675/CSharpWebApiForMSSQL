# CSharpWebApi
# **C# .NET Web API 练习项目** 🚀  

本项目包含一系列 **ASP.NET Core Web API** 练习，涵盖 **CRUD、身份验证、日志记录、权限控制** 等核心功能，帮助你熟练掌握 **.NET 后端开发**。  

## **练习列表**  

### ✅ **练习 1：基础 CRUD API**  
📌 **目标**：实现产品管理 API，支持 **增删改查** 操作。  

🔹 **要求**：  
- 使用 **Entity Framework Core** 连接 **SQLite 或 SQL Server**。  
- 创建 `Product` 实体：
  - `Id` (int, 主键)  
  - `Name` (string, 必填)  
  - `Price` (decimal, 不能小于 0)  
- 使用 **EF Core Migrations** 生成数据库表。  
- 实现 API：  
  - `GET /products`：获取所有产品  
  - `GET /products/{id}`：获取单个产品  
  - `POST /products`：新增产品  
  - `PUT /products/{id}`：更新产品  
  - `DELETE /products/{id}`：删除产品  

✨ **加分项**：  
✅ 使用 **DTO** 避免直接暴露数据库实体。  
✅ 添加数据验证（`Name` 不能为空，`Price` 不能小于 0）。  
✅ 集成 **Swagger（Swashbuckle）** 方便测试 API。  

---

### 🔐 **练习 2：JWT 身份验证**  
📌 **目标**：使用 **JWT** 进行身份验证，实现用户注册、登录、获取当前用户信息。  

🔹 **要求**：  
- 创建 `User` 模型：
  - `Id` (int, 主键)  
  - `Username` (string, 必填)  
  - `PasswordHash` (string, 使用 **BCrypt** 存储哈希密码)  
- 实现 API：
  - `POST /auth/register`：用户注册  
  - `POST /auth/login`：用户登录，返回 JWT 令牌  
  - `GET /auth/me`：获取当前用户信息（需要 **[Authorize]** 认证）  

✨ **加分项**：  
✅ 使用 **Claims** 存储用户角色信息（如 `User`, `Admin`）。  
✅ 仅允许 `GET /auth/me` 在用户已登录时访问。  

---

### 📜 **练习 3：日志（Logging）**  
📌 **目标**：使用 **Serilog** 记录日志，监控 API 请求和错误。  

🔹 **要求**：  
- 集成 **Serilog**，日志输出到 **Console 和文件**。  
- 记录 API 请求：
  - `INFO`：成功操作日志  
  - `ERROR`：失败时记录异常  
- 添加 **Middleware** 记录：
  - 请求的 **URL、方法、请求体**（仅 `POST/PUT`）。  
  - 响应时间。  
  - 发生异常时记录 `500 Internal Server Error`。  

✨ **加分项**：  
✅ 配置 **Serilog.Sinks.Seq** 进行结构化日志分析。  
✅ 如果用户已登录，记录 **UserId**（从 JWT 解析）。  

---

### 🔒 **练习 4：权限控制**  
📌 **目标**：基于用户角色（`User`, `Admin`）控制 API 访问权限。  

🔹 **要求**：  
- 扩展 `User` 模型，添加 `Role`（`User` 或 `Admin`）。  
- 控制 API 访问：
  - `User` 角色只能 **读取** (`GET /products`)。  
  - `Admin` 角色可以 **创建/修改/删除** (`POST/PUT/DELETE`)。  

✨ **加分项**：  
✅ 使用 **[Authorize(Roles = "Admin")]** 限制管理员权限。  
✅ 允许 `Admin` 修改用户角色。  

---

### 📊 **练习 5：分页 & 排序**  
📌 **目标**：优化 `GET /products`，添加分页和排序功能。  

🔹 **要求**：  
- 实现分页：
  - `?page=1&pageSize=10`  
- 实现排序：
  - `?sortBy=price&order=desc`  

✨ **加分项**：  
✅ 使用 `IQueryable` 和 `AsNoTracking()` 提高查询性能。  
✅ 返回 `PagedResult<T>`，包含 `totalCount` 和 `totalPages` 信息。  

---

## **如何运行项目？**  
1️⃣ **克隆项目**：  
```bash
git clone https://github.com/yong6675/CSharpWebApiForMSSQL.git
cd CSharpWebApiForMSSQL
