# NutriAI — Project Index

Quick reference for controllers, routes, services, repositories, views, and client scripts.  
**Default route:** `{controller=Home}/{action=Index}/{id?}`  
**Global auth:** All controllers require login unless marked `[AllowAnonymous]` (see `Program.cs`).

---

## Table of contents

1. [Find something fast](#find-something-fast)
2. [Solution layout](#solution-layout)
3. [Controllers & routes](#controllers--routes)
4. [Client API map (JavaScript)](#client-api-map-javascript)
5. [Application services](#application-services)
6. [Repositories](#repositories)
7. [Domain entities](#domain-entities)
8. [DTOs & request models](#dtos--request-models)
9. [ViewModels & views](#viewmodels--views)
10. [Configuration & environment](#configuration--environment)
11. [Dependency injection](#dependency-injection)
12. [Alphabetical file index](#alphabetical-file-index)

---

## Find something fast

| I need… | Go to |
|--------|--------|
| Login / register / password | [AuthController](#authcontroller) |
| Dashboard data | [DashboardController](#dashboardcontroller) → `GetSummary` |
| Log a meal (AI) | [MealTrackerController](#mealtrackercontroller) → `Analyze` |
| Weight chart | [WeightController](#weightcontroller) |
| Water tracking | [WaterController](#watercontroller) |
| Weekly meal plan | [MealPlannerController](#mealplannercontroller) |
| Recipe nutrition | [RecipeController](#recipecontroller) |
| Weekly report | [ReportController](#reportcontroller) |
| User profile | [ProfileController](#profilecontroller) |
| Admin panel | [AdminController](#admincontroller) |
| OpenAI calls | [IAiNutritionService](#iaiutritionservice--openainutritionservice) |
| Database tables | [ApplicationDbContext](#applicationdbcontext) |
| `.env` variables | [Configuration](#configuration--environment) |
| Register a new service | [DependencyInjection.cs](#dependency-injection) |

---

## Solution layout

```
NutriAI/
├── NutriAI.sln
├── .env / .env.example          # Secrets (not committed)
├── PROJECT_INDEX.md             # This file
│
├── NutriAI/                     # Web (MVC + static files)
│   ├── Program.cs
│   ├── Controllers/
│   ├── Views/
│   ├── ViewModels/
│   ├── wwwroot/js/
│   └── appsettings.json
│
├── NutriAI.Application/         # Interfaces, DTOs, configuration models
├── NutriAI.Domain/              # Entities, constants
└── NutriAI.Infrastructure/      # EF Core, services, repos, AI, email
```

---

## Controllers & routes

Convention: `/ControllerName/ActionName`  
JSON actions return `application/json`. POST/DELETE from JS require header `X-CSRF-TOKEN` (see `wwwroot/js/site.js`).

### HomeController

**File:** `NutriAI/Controllers/HomeController.cs`  
**Auth:** `[AllowAnonymous]` on controller

| HTTP | Action | Route | Returns | Notes |
|------|--------|-------|---------|-------|
| GET | `Index` | `/` or `/Home` | View | Marketing landing (`_LandingLayout`, `_LandingContent`, `wwwroot/css/landing.css`, `wwwroot/js/landing.js`) |
| GET | `Privacy` | `/Home/Privacy` | View | Privacy policy |
| GET | `Error` | `/Home/Error` | View | Error page (no cache) |

**View:** `Views/Home/Index.cshtml`, `Privacy.cshtml`, `Shared/Error.cshtml`

---

### AuthController

**File:** `NutriAI/Controllers/AuthController.cs`  
**Service:** `IAuthService` → `AuthService`  
**Email:** `IEmailService` → `SmtpEmailService`

| HTTP | Action | Route | Auth | Returns |
|------|--------|-------|------|---------|
| GET | `Login` | `/Auth/Login` | Anonymous | View |
| POST | `Login` | `/Auth/Login` | Anonymous | Redirect → Dashboard |
| GET | `Register` | `/Auth/Register` | Anonymous | View |
| POST | `Register` | `/Auth/Register` | Anonymous | Redirect → RegisterConfirmation |
| GET | `RegisterConfirmation` | `/Auth/RegisterConfirmation` | Anonymous | View |
| POST | `Logout` | `/Auth/Logout` | Authorized | Redirect → Home |
| GET | `ForgotPassword` | `/Auth/ForgotPassword` | Anonymous | View |
| POST | `ForgotPassword` | `/Auth/ForgotPassword` | Anonymous | Redirect |
| GET | `ForgotPasswordConfirmation` | `/Auth/ForgotPasswordConfirmation` | Anonymous | View |
| GET | `ResetPassword` | `/Auth/ResetPassword?email&token` | Anonymous | View |
| POST | `ResetPassword` | `/Auth/ResetPassword` | Anonymous | Redirect → Login |
| GET | `ConfirmEmail` | `/Auth/ConfirmEmail?userId&token` | Anonymous | View |
| GET | `ResendConfirmation` | `/Auth/ResendConfirmation?email=` | Anonymous | View (optional email query) |
| POST | `ResendConfirmation` | `/Auth/ResendConfirmation` | Anonymous | Redirect (body: `email`; used from login with hidden field) |
| GET | `ChangePassword` | `/Auth/ChangePassword` | Authorized | View |
| POST | `ChangePassword` | `/Auth/ChangePassword` | Authorized | Redirect → Profile |
| GET | `AccessDenied` | `/Auth/AccessDenied` | Anonymous | View |

**Views:** `Views/Auth/*.cshtml`  
**JS:** `wwwroot/js/auth.js` (password toggle on login/register)

---

### DashboardController

**File:** `NutriAI/Controllers/DashboardController.cs`  
**Service:** `IDashboardService` → `DashboardService`

| HTTP | Action | Route | Returns |
|------|--------|-------|---------|
| GET | `Index` | `/Dashboard` | View |
| GET | `GetSummary` | `/Dashboard/GetSummary` | JSON `DashboardSummaryDto` |

**View:** `Views/Dashboard/Index.cshtml`  
**JS:** `wwwroot/js/dashboard.js`

---

### MealTrackerController

**File:** `NutriAI/Controllers/MealTrackerController.cs`  
**Service:** `IMealTrackerService` → `MealTrackerService`  
**AI:** `IAiNutritionService.AnalyzeMealAsync`

| HTTP | Action | Route | Body | Returns |
|------|--------|-------|------|---------|
| GET | `Index` | `/MealTracker` | — | View |
| GET | `GetMeals` | `/MealTracker/GetMeals` | — | JSON `MealLogDto[]` (today) |
| POST | `Analyze` | `/MealTracker/Analyze` | `MealAnalyzeRequestDto` | JSON `MealAnalyzeResponseDto` |
| DELETE | `Delete` | `/MealTracker/Delete?id={id}` | — | JSON `{ success: true }` |

**View:** `Views/MealTracker/Index.cshtml`  
**JS:** `wwwroot/js/mealtracker.js`

---

### WeightController

**File:** `NutriAI/Controllers/WeightController.cs`  
**Service:** `IWeightService` → `WeightService`

| HTTP | Action | Route | Body | Returns |
|------|--------|-------|------|---------|
| GET | `Index` | `/Weight` | — | View |
| GET | `GetData` | `/Weight/GetData` | — | JSON (history, goal, `aiInsight`) |
| POST | `Add` | `/Weight/Add` | `WeightAddRequest { weight }` | JSON |

**View:** `Views/Weight/Index.cshtml`  
**JS:** `wwwroot/js/weight.js`

---

### WaterController

**File:** `NutriAI/Controllers/WaterController.cs`  
**Service:** `IWaterService` → `WaterService`

| HTTP | Action | Route | Body | Returns |
|------|--------|-------|------|---------|
| GET | `Index` | `/Water` | — | View |
| GET | `GetStatus` | `/Water/GetStatus` | — | JSON (`currentMl`, `goalMl`, `percent`, `recommendation`) |
| POST | `Add` | `/Water/Add` | `WaterAddRequest { amountMl }` | JSON (same as status) |

**View:** `Views/Water/Index.cshtml`  
**JS:** `wwwroot/js/water.js`

---

### MealPlannerController

**File:** `NutriAI/Controllers/MealPlannerController.cs`  
**Service:** `IMealPlannerService` → `MealPlannerService`  
**AI:** `IAiNutritionService.GenerateMealPlanAsync`

| HTTP | Action | Route | Body | Returns |
|------|--------|-------|------|---------|
| GET | `Index` | `/MealPlanner` | — | View |
| POST | `Generate` | `/MealPlanner/Generate` | `MealPlanGenerateRequest` | JSON weekly plan |

**Request model (controller file):** `GoalWeight`, `TimelineWeeks`, `DietaryPreference`

**View:** `Views/MealPlanner/Index.cshtml`  
**JS:** `wwwroot/js/mealplanner.js`

---

### RecipeController

**File:** `NutriAI/Controllers/RecipeController.cs`  
**Service:** `IRecipeService` → `RecipeService`  
**AI:** `IAiNutritionService.AnalyzeRecipeAsync`

| HTTP | Action | Route | Body | Returns |
|------|--------|-------|------|---------|
| GET | `Index` | `/Recipe` | — | View |
| POST | `Analyze` | `/Recipe/Analyze` | `RecipeAnalyzeRequest { recipeText }` | JSON analysis |

**View:** `Views/Recipe/Index.cshtml`  
**JS:** `wwwroot/js/recipe.js`

---

### ReportController

**File:** `NutriAI/Controllers/ReportController.cs`  
**Service:** `IReportService` → `ReportService`

| HTTP | Action | Route | Returns |
|------|--------|-------|---------|
| GET | `Index` | `/Report` | View |
| GET | `GetWeeklyData` | `/Report/GetWeeklyData` | JSON (generates/serves weekly report, emails user) |

**View:** `Views/Report/Index.cshtml`  
**JS:** `wwwroot/js/report.js`

---

### ProfileController

**File:** `NutriAI/Controllers/ProfileController.cs`  
**Service:** `IProfileService` → `ProfileService`

| HTTP | Action | Route | Body | Returns |
|------|--------|-------|------|---------|
| GET | `Index` | `/Profile` | — | View (`ProfileViewModel`) |
| GET | `Get` | `/Profile/Get` | — | JSON `ProfileDto` |
| POST | `Save` | `/Profile/Save` | `ProfileDto` | JSON `{ success, message, errors }` |

**View:** `Views/Profile/Index.cshtml`  
**JS:** `wwwroot/js/profile.js`

---

### AdminController

**File:** `NutriAI/Controllers/AdminController.cs`  
**Auth:** `[Authorize(Roles = Roles.Admin)]`  
**Service:** `IAdminService` → `AdminService`

| HTTP | Action | Route | Query | Returns |
|------|--------|-------|-------|---------|
| GET | `Index` | `/Admin` | — | View |
| GET | `GetStats` | `/Admin/GetStats` | — | JSON platform health stats |
| GET | `GetUsers` | `/Admin/GetUsers` | `page`, `search` | JSON user list (paginated) |
| GET | `GetUser` | `/Admin/GetUser` | `id` | JSON single user |
| POST | `CreateUser` | `/Admin/CreateUser` | body | Create user |
| PUT | `UpdateUser` | `/Admin/UpdateUser/{id}` | body | Update user |
| DELETE | `DeleteUser` | `/Admin/DeleteUser/{id}` | — | Delete user |
| POST | `SetBan` | `/Admin/SetBan/{id}` | `{ banned }` | Ban/unban user |
| GET | `GetMealLogs` | `/Admin/GetMealLogs` | `page`, `userId?` | All meal logs with user info |
| GET | `GetRecipeAnalyses` | `/Admin/GetRecipeAnalyses` | `page`, `userId?` | Recipe analysis history |
| GET | `GetWeeklyReports` | `/Admin/GetWeeklyReports` | `page`, `userId?` | Weekly reports per user |

**Auth redirect:** Admins log in → `/Admin` (not user dashboard). User nav hidden for `Roles.Admin` only.

**View:** `Views/Admin/Index.cshtml`  
**JS:** `wwwroot/js/admin.js`

---

## Client API map (JavaScript)

| Script | Calls |
|--------|--------|
| `site.js` | `NutriAI.fetchJson`, `showToast`, CSRF for non-GET |
| `dashboard.js` | `GET /Dashboard/GetSummary` (dynamic weekly calories & weight trend) |
| `landing.js` | Landing scroll/reveal animations |
| `mealtracker.js` | `GET /MealTracker/GetMeals`, `POST /MealTracker/Analyze`, `DELETE /MealTracker/Delete?id=` |
| `weight.js` | `GET /Weight/GetData`, `POST /Weight/Add`, `DELETE /Weight/Delete?id=` |
| `water.js` | `GET /Water/GetStatus`, `POST /Water/Add` |
| `mealplanner.js` | `POST /MealPlanner/Generate` |
| `recipe.js` | `POST /Recipe/Analyze` |
| `report.js` | `GET /Report/GetWeeklyData` |
| `profile.js` | `POST /Profile/Save` |
| `admin.js` | Admin stats, user CRUD, ban, meal logs, recipes, weekly reports panels |
| `auth.js` | UI only (forms post to Auth actions) |

---

## Application services

| Interface | Implementation | Used by |
|-----------|----------------|---------|
| `IAuthService` | `AuthService` | `AuthController` |
| `IDashboardService` | `DashboardService` | `DashboardController` |
| `IMealTrackerService` | `MealTrackerService` | `MealTrackerController` |
| `IWeightService` | `WeightService` | `WeightController` |
| `IWaterService` | `WaterService` | `WaterController` |
| `IMealPlannerService` | `MealPlannerService` | `MealPlannerController` |
| `IRecipeService` | `RecipeService` | `RecipeController` |
| `IReportService` | `ReportService` | `ReportController` |
| `IProfileService` | `ProfileService` | `ProfileController` |
| `IAdminService` | `AdminService` | `AdminController` |
| `IEmailService` | `SmtpEmailService` | `AuthService`, `ReportService` |
| `IAiNutritionService` | `OpenAiNutritionService` | Meal/Planner/Recipe/Report/Water/Dashboard/Weight services |

### IAuthService → AuthService

**File:** `NutriAI.Infrastructure/Services/AuthService.cs`

| Method | Purpose |
|--------|---------|
| `RegisterAsync` | Create user, `UserGoal`, initial `WeightLog`, assign `User` role, send confirmation email |
| `LoginAsync` | Returns `ErrorCode` `EmailNotConfirmed` when email unconfirmed |
| `LoginAsync` | Password sign-in (requires confirmed email); `ErrorCode` `AccountBanned` when banned |
| `LogoutAsync` | Sign out |
| `ForgotPasswordAsync` | Send reset link email |
| `ResetPasswordAsync` | Reset password with token |
| `ConfirmEmailAsync` | Confirm email from link |
| `ResendConfirmationAsync` | Resend confirmation email |
| `ChangePasswordAsync` | Change password for logged-in user |

### IDashboardService → DashboardService

| Method | Purpose |
|--------|---------|
| `GetSummaryAsync` | Calories, weight, water, streak, AI insight, recent meals, plans, latest report days |

### IMealTrackerService → MealTrackerService

| Method | Purpose |
|--------|---------|
| `GetMealsAsync` | Today's meal logs |
| `AnalyzeMealAsync` | AI meal analysis + save log + `AIChat` history |
| `DeleteMealAsync` | Delete meal (owner only) |

### IWeightService → WeightService

| Method | Purpose |
|--------|---------|
| `GetDataAsync` | Weight history, goals, AI insight |
| `AddWeightAsync` | Log weight with validation (30–300 kg, max 3 kg/day change) |
| `DeleteWeightAsync` | Delete weight entry (owner only) |

### IWaterService → WaterService

| Method | Purpose |
|--------|---------|
| `GetStatusAsync` | Daily water total + AI hydration tip |
| `AddWaterAsync` | Add water log (1–5000 ml) |

### IMealPlannerService → MealPlannerService

| Method | Purpose |
|--------|---------|
| `GeneratePlanAsync` | AI 7-day plan (or fallback), persist `MealPlan` + items |

### IRecipeService → RecipeService

| Method | Purpose |
|--------|---------|
| `AnalyzeRecipeAsync` | AI recipe parse, save `Recipe` + `RecipeAnalysis` |

### IReportService → ReportService

| Method | Purpose |
|--------|---------|
| `GetWeeklyDataAsync` | Build/load `WeeklyReport`, AI tips, email user |

### IProfileService → ProfileService

| Method | Purpose |
|--------|---------|
| `GetProfileAsync` | User name + `UserGoal` fields |
| `SaveProfileAsync` | Update profile, recalc calorie/water targets |

### IAdminService → AdminService

| Method | Purpose |
|--------|---------|
| `GetStatsAsync` | User counts, activity stats |
| `GetUsersAsync` | Paginated user search |

### IEmailService → SmtpEmailService

| Method | Purpose |
|--------|---------|
| `SendEmailAsync` | SMTP HTML email (skips if not configured) |

### IAiNutritionService → OpenAiNutritionService

**File:** `NutriAI.Infrastructure/AI/OpenAiNutritionService.cs`  
**Config:** `OpenAI:ApiKey`, `OpenAI:Model`, `OpenAI:BaseUrl`

| Method | Purpose |
|--------|---------|
| `AnalyzeMealAsync` | Calories, macros, impact message |
| `GenerateMealPlanAsync` | 7-day plan with instructions |
| `AnalyzeRecipeAsync` | Ingredients, macros, alternatives |
| `GetWeeklyRecommendationsAsync` | 3 weekly tips |
| `GetHydrationRecommendationAsync` | Water tip |
| `GetDashboardInsightAsync` | Dashboard insight text |
| `GetWeightInsightAsync` | Weight progress insight |
| `IsConfigured` | True when API key is set |

**Helpers:** `NutritionContextHelper`, `NutritionTargetsCalculator` (same folder)

---

## Repositories

All implementations in `NutriAI.Infrastructure/Repositories/`.  
Registered in `DependencyInjection.cs`.

| Interface | Implementation | Key methods |
|-----------|----------------|-------------|
| `IGenericRepository<T>` | `GenericRepository<T>` | CRUD + `SaveChangesAsync` |
| `IMealLogRepository` | `MealLogRepository` | `GetByUserForDateAsync`, `GetRecentByUserAsync` |
| `IWeightLogRepository` | `WeightLogRepository` | `GetByUserAsync`, `GetLatestByUserAsync` |
| `IWaterLogRepository` | `WaterLogRepository` | `GetTotalForDateAsync`, `GetByUserForDateAsync` |
| `IUserGoalRepository` | `UserGoalRepository` | `GetByUserIdAsync` |
| `IMealPlanRepository` | `MealPlanRepository` | `GetWithItemsAsync`, `GetByUserAsync` |
| `IRecipeRepository` | `RecipeRepository` | `GetWithAnalysesAsync` |
| `IWeeklyReportRepository` | `WeeklyReportRepository` | `GetLatestByUserAsync` |
| `INotificationRepository` | `NotificationRepository` | `GetByUserAsync` *(unused in UI)* |
| `IAIChatRepository` | `AIChatRepository` | `GetByUserAndContextAsync` |
| `IUserRepository` | `UserRepository` | `SearchAsync`, `CountAsync`, `CountActiveAsync` |

---

## Domain entities

**Folder:** `NutriAI.Domain/Entities/`

| Entity | Table | Description |
|--------|-------|-------------|
| `ApplicationUser` | `AspNetUsers` | Identity user (`FullName`, goals, collections) |
| `UserGoal` | `UserGoals` | Age, gender, height, weights, activity, daily targets |
| `Meal` | `Meals` | Legacy/simple meal (linked to user) |
| `MealLog` | `MealLogs` | Daily meal entries + AI response |
| `WeightLog` | `WeightLogs` | Weight entries |
| `WaterLog` | `WaterLogs` | Water intake entries |
| `MealPlan` | `MealPlans` | Saved weekly plans |
| `MealPlanItem` | `MealPlanItems` | Per-day meals in a plan |
| `Recipe` | `Recipes` | User recipe text |
| `RecipeAnalysis` | `RecipeAnalyses` | Nutrition breakdown JSON |
| `WeeklyReport` | `WeeklyReports` | Cached weekly stats + tips |
| `AIChat` | `AIChats` | Meal tracker conversation log |
| `Notification` | `Notifications` | *(schema only, no feature yet)* |

**Constants:** `Domain/Constants/Roles.cs` — `Admin`, `User`

### ApplicationDbContext

**File:** `NutriAI.Infrastructure/Data/ApplicationDbContext.cs`  
**Migration:** `Migrations/20260518150814_InitialCreate.cs`  
**Seed:** `Identity/DatabaseSeeder.cs` (roles + optional admin from `AdminSeed`)

---

## DTOs & request models

**Folder:** `NutriAI.Application/DTOs/`

| Type | File | Used for |
|------|------|----------|
| `RegisterDto` (includes age, gender, height, weights, activity, daily water) | `AuthDtos.cs` | Auth registration |
| `LoginDto`, `ForgotPasswordDto`, `ResetPasswordDto`, `ChangePasswordDto` | `AuthDtos.cs` | Auth service |
| `ProfileDto` (email read-only on UI, daily water target) | `ProfileDtos.cs` | Profile API |
| `MealAnalyzeRequestDto`, `MealLogDto`, `MealAnalyzeResponseDto` | `MealDtos.cs` | Meal tracker |
| `DashboardSummaryDto`, `RecentMealDto`, `SavedPlanDto`, `DailyCaloriePointDto`, `DailyWeightPointDto` | `DashboardDtos.cs` | Dashboard (charts use weekly DB data) |
| `MealAnalysisResult`, `MealPlanDayResult`, `RecipeAnalysisResult`, `UserNutritionContext`, … | `AiNutritionDtos.cs` | AI service |

**Controller-local request types:**

| Type | File |
|------|------|
| `WeightAddRequest` | `WeightController.cs` |
| `WaterAddRequest` | `WaterController.cs` |
| `MealPlanGenerateRequest` | `MealPlannerController.cs` |
| `RecipeAnalyzeRequest` | `RecipeController.cs` |

**Common:** `Application/Common/ServiceResult.cs` — `Succeeded`, `Message`, `Errors`

---

## ViewModels & views

### ViewModels (`NutriAI/ViewModels/`)

| ViewModel | View |
|-----------|------|
| `LoginViewModel` | `Auth/Login` |
| `RegisterViewModel` | `Auth/Register` |
| `ForgotPasswordViewModel` | `Auth/ForgotPassword` |
| `ResetPasswordViewModel` | `Auth/ResetPassword` |
| `ChangePasswordViewModel` | `Auth/ChangePassword` |
| `ProfileViewModel` | `Profile/Index` |

### Views map

| View path | Controller.Action | Layout |
|-----------|-------------------|--------|
| `Home/Index` | `Home.Index` | `_LandingLayout` |
| `Home/_LandingContent` | partial | Landing sections |
| `Dashboard/Index` | `Dashboard.Index` | `_Layout` |
| `MealTracker/Index` | `MealTracker.Index` | `_Layout` |
| `Weight/Index` | `Weight.Index` | `_Layout` |
| `Water/Index` | `Water.Index` | `_Layout` |
| `MealPlanner/Index` | `MealPlanner.Index` | `_Layout` |
| `Recipe/Index` | `Recipe.Index` | `_Layout` |
| `Report/Index` | `Report.Index` | `_Layout` |
| `Profile/Index` | `Profile.Index` | `_Layout` |
| `Admin/Index` | `Admin.Index` | `_Layout` |
| `Auth/*` | Auth actions | `_AuthLayout` |
| `Shared/_Layout` | — | Main nav (all features) |
| `Shared/Error` | `Home.Error` | — |

### Extensions

| File | Member |
|------|--------|
| `Extensions/ClaimsPrincipalExtensions.cs` | `GetUserId()` — current user id from claims |

---

## Configuration & environment

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server |
| `AppSettings:BaseUrl` | Email links (confirm, reset) |
| `OpenAI:ApiKey` | OpenAI API |
| `OpenAI:Model` | Default `gpt-4o-mini` |
| `OpenAI:BaseUrl` | Default `https://api.openai.com/v1` |
| `EmailSettings:*` | SMTP sender |
| `AdminSeed:Email/Password/FullName` | First-run admin (optional) |

**Files:** `NutriAI/.env` (local), `NutriAI/.env.example`, `NutriAI/appsettings.json`  
**Load order:** `.env` (walk up directories) → `appsettings.json` → environment variables (`Program.cs`)

---

## Dependency injection

**File:** `NutriAI.Infrastructure/DependencyInjection.cs` — method `AddInfrastructure`

Registers: DbContext, Identity, all repositories, all services, `HttpClient` + `OpenAiNutritionService`, cookie auth, antiforgery header `X-CSRF-TOKEN`.

**Startup:** `NutriAI/Program.cs`

- `LoadEnvFile()` → `AddInfrastructure` → global `[Authorize]` + `AutoValidateAntiforgeryToken`
- `DatabaseSeeder.SeedAsync` on startup

---

## Alphabetical file index

### Controllers (`NutriAI/Controllers/`)

- `AdminController.cs`
- `AuthController.cs`
- `DashboardController.cs`
- `HomeController.cs`
- `MealPlannerController.cs`
- `MealTrackerController.cs`
- `ProfileController.cs`
- `RecipeController.cs`
- `ReportController.cs`
- `WaterController.cs`
- `WeightController.cs`

### Services (`NutriAI.Infrastructure/Services/`)

- `AdminService.cs`
- `AuthService.cs`
- `DashboardService.cs`
- `MealPlannerService.cs`
- `MealTrackerService.cs`
- `ProfileService.cs`
- `RecipeService.cs`
- `ReportService.cs`
- `WaterService.cs`
- `WeightService.cs`
- `Email/SmtpEmailService.cs`

### AI (`NutriAI.Infrastructure/AI/`)

- `OpenAiNutritionService.cs`
- `NutritionContextHelper.cs`
- `NutritionTargetsCalculator.cs`

### Repositories (`NutriAI.Infrastructure/Repositories/`)

- `AIChatRepository.cs`
- `GenericRepository.cs`
- `MealLogRepository.cs`
- `MealPlanRepository.cs`
- `NotificationRepository.cs`
- `RecipeRepository.cs`
- `UserGoalRepository.cs`
- `UserRepository.cs`
- `WaterLogRepository.cs`
- `WeeklyReportRepository.cs`
- `WeightLogRepository.cs`

### Interfaces (`NutriAI.Application/Interfaces/`)

- `Services/` — `IAuthService`, `IAdminService`, `IAiNutritionService`, `IDashboardService`, `IEmailService`, `IMealPlannerService`, `IMealTrackerService`, `IProfileService`, `IRecipeService`, `IReportService`, `IWaterService`, `IWeightService`
- `Repositories/` — one interface per repository above + `IGenericRepository<T>`

### Configuration (`NutriAI.Application/Configuration/`)

- `AdminSeedSettings.cs`
- `EmailSettings.cs`
- `OpenAiSettings.cs`

---

*Last indexed from solution structure. Update this file when adding controllers, actions, or services.*
