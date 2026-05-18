import pathlib

t = "div"
path = pathlib.Path(__file__).resolve().parents[1] / "NutriAI" / "Views" / "Dashboard" / "Index.cshtml"

content = f"""@{{
    ViewData["Title"] = "Dashboard";
    ViewData["ActiveNav"] = "Dashboard";
}}

<{t} class="page-header fade-in">
    <h1>Dashboard</h1>
    <p class="subtitle">Your daily nutrition overview</p>
</{t}>

<{t} class="row g-4 mb-4 fade-in">
    <{t} class="col-md-6 col-lg-3">
        <{t} class="card-nutri dashboard-stat-card hover-scale">
            <{t} class="d-flex align-items-center gap-3">
                <{t} class="stat-icon green"><i class="fas fa-fire"></i></{t}>
                <{t}>
                    <p class="card-title mb-0">Calories Today</p>
                    <p class="stat-value mb-0"><span id="caloriesConsumed">--</span> / <span id="caloriesGoal">--</span></p>
                </{t}>
            </{t}>
            <{t} class="progress progress-nutri mt-3">
                <{t} class="progress-bar" id="calorieProgress" style="width: 0%"></{t}>
            </{t}>
        </{t}>
    </{t}>
    <{t} class="col-md-6 col-lg-3">
        <{t} class="card-nutri dashboard-stat-card hover-scale">
            <{t} class="d-flex align-items-center gap-3">
                <{t} class="stat-icon blue"><i class="fas fa-weight-scale"></i></{t}>
                <{t}>
                    <p class="card-title mb-0">Current Weight</p>
                    <p class="stat-value mb-0"><span id="currentWeight">--</span> kg</p>
                </{t}>
            </{t}>
            <p class="small-text text-muted mt-2 mb-0">Goal: <span id="goalWeight">--</span> kg</p>
        </{t}>
    </{t}>
    <{t} class="col-md-6 col-lg-3">
        <{t} class="card-nutri dashboard-stat-card hover-scale">
            <{t} class="d-flex align-items-center gap-3">
                <{t} class="stat-icon blue"><i class="fas fa-droplet"></i></{t}>
                <{t}>
                    <p class="card-title mb-0">Water Intake</p>
                    <p class="stat-value mb-0"><span id="waterMl">--</span> ml</p>
                </{t}>
            </{t}>
            <{t} class="progress progress-nutri mt-3">
                <{t} class="progress-bar bg-info" id="waterProgress" style="width: 0%"></{t}>
            </{t}>
        </{t}>
    </{t}>
    <{t} class="col-md-6 col-lg-3">
        <{t} class="card-nutri dashboard-stat-card hover-scale">
            <{t} class="d-flex align-items-center gap-3">
                <{t} class="stat-icon orange"><i class="fas fa-calendar-check"></i></{t}>
                <{t}>
                    <p class="card-title mb-0">Weekly Streak</p>
                    <p class="stat-value mb-0"><span id="weeklyStreak">--</span> days</p>
                </{t}>
            </{t}>
        </{t}>
    </{t}>
</{t}>

<{t} class="row g-4 mb-4 dashboard-charts-row">
    <{t} class="col-lg-8">
        <{t} class="card-nutri dashboard-chart-card">
            <h3 class="h5 mb-3">Calorie Tracking</h3>
            <{t} class="dashboard-chart-wrap">
                <canvas id="calorieChart"></canvas>
            </{t}>
        </{t}>
    </{t}>
    <{t} class="col-lg-4">
        <{t} class="card-nutri insight-card mb-4">
            <h3 class="h6 mb-2"><i class="fas fa-robot text-success me-2"></i>AI Insight</h3>
            <p class="small-text mb-0" id="aiInsight">Loading insights...</p>
        </{t}>
        <{t} class="card-nutri dashboard-chart-card">
            <h3 class="h6 mb-3">Weight Progress</h3>
            <{t} class="dashboard-chart-wrap dashboard-chart-wrap-sm">
                <canvas id="weightMiniChart"></canvas>
            </{t}>
        </{t}>
    </{t}>
</{t}>

<{t} class="row g-4 dashboard-lists-row">
    <{t} class="col-lg-6">
        <{t} class="card-nutri dashboard-list-card">
            <h3 class="h5 mb-3">Recent Meals</h3>
            <{t} id="recentMealsList" class="dashboard-list-body"></{t}>
        </{t}>
    </{t}>
    <{t} class="col-lg-6">
        <{t} class="card-nutri dashboard-list-card">
            <h3 class="h5 mb-3">Saved Meal Plans</h3>
            <{t} id="savedPlansList" class="dashboard-list-body"></{t}>
        </{t}>
    </{t}>
</{t}>

<{t} class="row g-4 mt-2" id="weeklyReportRow" style="display:none;">
    <{t} class="col-12">
        <{t} class="card-nutri insight-card">
            <h3 class="h6 mb-2"><i class="fas fa-file-lines text-success me-2"></i>Latest Weekly AI Report</h3>
            <p class="small-text mb-0" id="weeklyReportSummary"></p>
        </{t}>
    </{t}>
</{t}>

@section Scripts {{
    <script src="~/js/dashboard.js" asp-append-version="true"></script>
}}
"""

path.write_text(content, encoding="utf-8")
print("wrote", path)
