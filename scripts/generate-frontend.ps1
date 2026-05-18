$base = "d:\Courses\ITI\04- Courses\27- AI-2\04- Project\02- Repo\NutriAI\NutriAI"
$E = "ENDTAG"

function Save-View($folder, $name, $content) {
    $dir = Join-Path $base "Views\$folder"
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $fixed = $content.Replace($E, "div")
    Set-Content -Path (Join-Path $dir $name) -Value $fixed -Encoding utf8
}

function Save-Js($name, $content) {
    $dir = Join-Path $base "wwwroot\js"
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Set-Content -Path (Join-Path $dir $name) -Value $content -Encoding utf8
}

Save-View "Water" "Index.cshtml" @"
@{
    ViewData["Title"] = "Water Tracker";
    ViewData["ActiveNav"] = "Water";
}

<div class="page-header fade-in">
    <h1>Water Tracker</h1>
    <p class="subtitle">Stay hydrated throughout the day</p>
</$E>

<div class="row g-4">
    <div class="col-lg-5">
        <div class="card-nutri text-center">
            <h3 class="h6 mb-4">Daily Hydration</h3>
            <div class="water-progress-circle">
                <canvas id="waterCircle" width="180" height="180"></canvas>
                <div class="water-progress-label">
                    <p class="stat-value mb-0" id="waterPercent">0%</p>
                    <p class="small-text text-muted mb-0"><span id="waterCurrent">0</span> / <span id="waterGoal">2500</span> ml</p>
                </$E>
            </$E>
            <div class="progress progress-nutri mt-4">
                <div class="progress-bar bg-info" id="waterBar" style="width:0%"></$E>
            </$E>
        </$E>
    </$E>
    <div class="col-lg-7">
        <div class="card-nutri">
            <h3 class="h6 mb-3">Quick Add</h3>
            <div class="d-flex flex-wrap gap-2 mb-4">
                <button class="btn btn-secondary-nutri water-add-btn" data-ml="250">+ 250ml</button>
                <button class="btn btn-secondary-nutri water-add-btn" data-ml="500">+ 500ml</button>
                <button class="btn btn-primary-nutri water-add-btn" data-ml="1000">+ 1L</button>
            </$E>
            <form id="customWaterForm" class="row g-2">
                <div class="col-8">
                    <input type="number" class="form-control form-control-nutri" id="customWaterMl" placeholder="Custom amount (ml)" min="50" />
                </$E>
                <div class="col-4">
                    <button type="submit" class="btn btn-outline-nutri w-100">Add</button>
                </$E>
            </form>
        </$E>
    </$E>
</$E>

@section Scripts {
    <script src="~/js/water.js" asp-append-version="true"></script>
}
"@

Save-View "MealPlanner" "Index.cshtml" @"
@{
    ViewData["Title"] = "AI Meal Planner";
    ViewData["ActiveNav"] = "MealPlanner";
}

<div class="page-header fade-in">
    <h1>AI Meal Planner</h1>
    <p class="subtitle">Generate a personalized weekly meal plan</p>
</$E>

<div class="row g-4">
    <div class="col-lg-4">
        <div class="card-nutri">
            <h3 class="h6 mb-3">Your Goals</h3>
            <form id="plannerForm">
                <motion></motion>
                <div class="mb-3">
                    <label class="form-label">Goal Weight (kg)</label>
                    <input type="number" step="0.1" class="form-control form-control-nutri" id="goalWeight" value="72" required />
                </$E>
                <div class="mb-3">
                    <label class="form-label">Timeline (weeks)</label>
                    <input type="number" class="form-control form-control-nutri" id="timeline" value="8" min="1" required />
                </$E>
                <div class="mb-3">
                    <label class="form-label">Dietary Preference</label>
                    <select class="form-select form-control-nutri" id="dietaryPreference">
                        <option value="balanced">Balanced</option>
                        <option value="high-protein">High Protein</option>
                        <option value="vegetarian">Vegetarian</option>
                        <option value="mediterranean">Mediterranean</option>
                        <option value="low-carb">Low Carb</option>
                    </select>
                </$E>
                <button type="submit" class="btn btn-primary-nutri w-100">Generate Plan</button>
            </form>
            <div class="ai-loading mt-3" id="plannerLoading">
                <div class="spinner-nutri"></$E>
                <p class="small-text text-muted">Generating your meal plan...</p>
            </$E>
        </$E>
    </$E>
    <div class="col-lg-8">
        <div id="mealPlanResults"></$E>
    </$E>
</$E>

@section Scripts {
    <script src="~/js/mealplanner.js" asp-append-version="true"></script>
}
"@

Save-View "Recipe" "Index.cshtml" @"
@{
    ViewData["Title"] = "Recipe Analyzer";
    ViewData["ActiveNav"] = "Recipe";
}

<div class="page-header fade-in">
    <h1>Recipe Analyzer</h1>
    <p class="subtitle">Paste a recipe and get detailed nutrition analysis</p>
</$E>

<div class="row g-4">
    <div class="col-lg-5">
        <div class="card-nutri">
            <form id="recipeForm">
                <div class="mb-3">
                    <label class="form-label">Recipe Text</label>
                    <textarea class="form-control form-control-nutri" id="recipeText" rows="12" placeholder="Paste your recipe ingredients and instructions..." required></textarea>
                </$E>
                <button type="submit" class="btn btn-primary-nutri w-100">Analyze Recipe</button>
            </form>
            <div class="ai-loading mt-3" id="recipeLoading">
                <div class="spinner-nutri"></$E>
                <p class="small-text text-muted">Analyzing recipe...</p>
            </$E>
        </$E>
    </$E>
    <div class="col-lg-7">
        <div id="recipeResults" class="d-none">
            <div class="card-nutri mb-4">
                <h3 class="h5 mb-3">Nutrition Summary</h3>
                <motion></motion>
                <motion></motion>
                <div class="row g-3" id="recipeSummary"></$E>
            </$E>
            <div class="card-nutri mb-4">
                <h3 class="h6 mb-3">Ingredient Breakdown</h3>
                <div class="table-responsive">
                    <table class="table table-nutri table-hover mb-0" id="ingredientTable">
                        <thead><tr><th>Ingredient</th><th>Amount</th><th>Calories</th></tr></thead>
                        <tbody></tbody>
                    </table>
                </$E>
            </$E>
            <div class="card-nutri">
                <h3 class="h6 mb-3"><i class="fas fa-lightbulb text-warning me-2"></i>Healthier Alternatives</h3>
                <ul id="alternativesList" class="mb-0"></ul>
            </$E>
        </$E>
    </$E>
</$E>

@section Scripts {
    <script src="~/js/recipe.js" asp-append-version="true"></script>
}
"@

Save-View "Report" "Index.cshtml" @"
@{
    ViewData["Title"] = "Weekly Report";
    ViewData["ActiveNav"] = "Report";
}

<div class="page-header fade-in">
    <h1>Weekly Report</h1>
    <p class="subtitle">Your fitness analytics for the past 7 days</p>
</$E>

<div class="row g-4 mb-4" id="reportStats"></$E>

<div class="row g-4 mb-4">
    <div class="col-lg-6">
        <div class="card-nutri">
            <h3 class="h6 mb-3">Daily Calories</h3>
            <canvas id="reportCalorieChart" height="120"></canvas>
        </$E>
    </$E>
    <div class="col-lg-6">
        <div class="card-nutri">
            <h3 class="h6 mb-3">Weight Trend</h3>
            <canvas id="reportWeightChart" height="120"></canvas>
        </$E>
    </$E>
</$E>

<div class="row g-4">
    <div class="col-lg-6">
        <div class="card-nutri">
            <h3 class="h6 mb-3">Hydration Consistency</h3>
            <canvas id="reportHydrationChart" height="120"></canvas>
        </$E>
    </$E>
    <div class="col-lg-6">
        <motion></motion>
        <div class="card-nutri insight-card">
            <h3 class="h6 mb-3"><i class="fas fa-robot text-success me-2"></i>AI Recommendations</h3>
            <ul id="aiRecommendations" class="mb-0"></ul>
        </$E>
    </$E>
</$E>

@section Scripts {
    <script src="~/js/report.js" asp-append-version="true"></script>
}
"@

Save-View "Profile" "Index.cshtml" @"
@model NutriAI.Models.ProfileRequest
@{
    ViewData["Title"] = "Profile";
    ViewData["ActiveNav"] = "Profile";
}

<div class="page-header fade-in">
    <h1>Profile</h1>
    <p class="subtitle">Manage your personal information and goals</p>
</$E>

<div class="row justify-content-center">
    <div class="col-lg-8">
        <div class="card-nutri">
            <form id="profileForm" class="needs-validation" novalidate>
                <div class="row g-3">
                    <div class="col-md-6">
                        <label class="form-label">Name</label>
                        <input type="text" class="form-control form-control-nutri" id="name" value="@Model.Name" required />
                    </$E>
                    <div class="col-md-6">
                        <label class="form-label">Age</label>
                        <input type="number" class="form-control form-control-nutri" id="age" value="@Model.Age" required />
                    </$E>
                    <div class="col-md-6">
                        <label class="form-label">Gender</label>
                        <select class="form-select form-control-nutri" id="gender">
                            <option value="Male" selected="@(Model.Gender == "Male")">Male</option>
                            <option value="Female" selected="@(Model.Gender == "Female")">Female</option>
                            <option value="Other" selected="@(Model.Gender == "Other")">Other</option>
                        </select>
                    </$E>
                    <motion></motion>
                    <div class="col-md-6">
                        <label class="form-label">Height (cm)</label>
                        <input type="number" class="form-control form-control-nutri" id="height" value="@Model.Height" required />
                    </$E>
                    <div class="col-md-6">
                        <label class="form-label">Current Weight (kg)</label>
                        <input type="number" step="0.1" class="form-control form-control-nutri" id="currentWeight" value="@Model.CurrentWeight" required />
                    </$E>
                    <div class="col-md-6">
                        <label class="form-label">Goal Weight (kg)</label>
                        <input type="number" step="0.1" class="form-control form-control-nutri" id="goalWeight" value="@Model.GoalWeight" required />
                    </$E>
                    <div class="col-12">
                        <label class="form-label">Activity Level</label>
                        <select class="form-select form-control-nutri" id="activityLevel">
                            <option value="Sedentary">Sedentary</option>
                            <option value="Lightly Active">Lightly Active</option>
                            <option value="Moderately Active" selected>Moderately Active</option>
                            <option value="Very Active">Very Active</option>
                        </select>
                    </$E>
                </$E>
                <button type="submit" class="btn btn-primary-nutri mt-4">Save Profile</button>
            </form>
        </$E>
    </$E>
</$E>

@section Scripts {
    <script src="~/js/profile.js" asp-append-version="true"></script>
}
"@

Save-View "Admin" "Index.cshtml" @"
@{
    ViewData["Title"] = "Admin Dashboard";
    ViewData["ActiveNav"] = "Admin";
}

<div class="page-header fade-in">
    <h1>Admin Dashboard</h1>
    <p class="subtitle">Platform overview and user management</p>
</$E>

<div class="row g-4 mb-4" id="adminStats"></$E>

<div class="card-nutri">
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <h3 class="h5 mb-0">Users</h3>
        <input type="search" class="form-control form-control-nutri" id="userSearch" placeholder="Search users..." style="max-width:280px" />
    </$E>
    <div class="table-responsive">
        <table class="table table-nutri table-hover mb-0">
            <thead>
                <tr><th>ID</th><th>Name</th><th>Email</th><th>Status</th><th>Joined</th></tr>
            </thead>
            <tbody id="usersTableBody"></tbody>
        </table>
    </$E>
    <nav class="mt-3">
        <ul class="pagination justify-content-center mb-0" id="usersPagination"></ul>
    </nav>
</$E>

@section Scripts {
    <script src="~/js/admin.js" asp-append-version="true"></script>
}
"@

# Complete Weight view (full)
Save-View "Weight" "Index.cshtml" @"
@{
    ViewData["Title"] = "Weight Tracker";
    ViewData["ActiveNav"] = "Weight";
}

<div class="page-header fade-in">
    <h1>Weight Tracker</h1>
    <p class="subtitle">Track your weight and progress toward your goal</p>
</$E>

<div class="row g-4 mb-4">
    <div class="col-md-4">
        <div class="card-nutri text-center hover-scale">
            <p class="card-title">Current Weight</p>
            <p class="stat-value text-success" id="displayCurrentWeight">--</p>
            <span class="small-text text-muted">kg</span>
        </$E>
    </$E>
    <div class="col-md-4">
        <div class="card-nutri text-center hover-scale">
            <p class="card-title">Goal Weight</p>
            <p class="stat-value text-primary" id="displayGoalWeight">--</p>
            <span class="small-text text-muted">kg</span>
        </$E>
    </$E>
    <div class="col-md-4">
        <div class="card-nutri text-center hover-scale">
            <p class="card-title">To Go</p>
            <p class="stat-value text-warning" id="weightToGo">--</p>
            <span class="small-text text-muted">kg</span>
        </$E>
    </$E>
</$E>

<div class="row g-4">
    <div class="col-lg-8">
        <div class="card-nutri">
            <h3 class="h5 mb-3">Weight Progress</h3>
            <canvas id="weightChart" height="100"></canvas>
        </$E>
    </$E>
    <div class="col-lg-4">
        <div class="card-nutri mb-4">
            <h3 class="h6 mb-3">Add Weight</h3>
            <form id="weightForm">
                <div class="mb-3">
                    <label class="form-label">Weight (kg)</label>
                    <input type="number" step="0.1" class="form-control form-control-nutri" id="weightInput" required />
                </$E>
                <button type="submit" class="btn btn-primary-nutri w-100">Save</button>
            </form>
        </$E>
        <div class="card-nutri">
            <h3 class="h6 mb-3">History</h3>
            <motion></motion>
            <div id="weightHistoryList" class="small-text"></$E>
        </$E>
    </$E>
</$E>

@section Scripts {
    <script src="~/js/weight.js" asp-append-version="true"></script>
}
"@

Write-Host "Views generated. Fixing stray motion tags..."
Get-ChildItem (Join-Path $base "Views") -Recurse -Filter *.cshtml | ForEach-Object {
    $t = Get-Content $_.FullName -Raw
    $t = $t -replace '<motion></motion>\s*',''
    $t = $t -replace '<motion>',''
    $t = $t -replace '</motion>',''
    Set-Content $_.FullName $t -Encoding utf8
}

Write-Host "Done views."
