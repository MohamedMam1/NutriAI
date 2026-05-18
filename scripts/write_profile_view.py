import pathlib

t = "motion"
t = chr(100) + chr(105) + chr(118)
path = pathlib.Path(__file__).resolve().parents[1] / "NutriAI" / "Views" / "Profile" / "Index.cshtml"

content = f"""@model ProfileViewModel
@{{
    ViewData["Title"] = "Profile";
    ViewData["ActiveNav"] = "Profile";
}}

<{t} class="page-header fade-in">
    <h1>Profile</h1>
    <p class="subtitle">Manage your personal information and goals</p>
</{t}>

@if (!Model.HasCompletedProfile)
{{
    <{t} class="alert alert-warning">Complete your health profile so calorie, water, and AI targets stay accurate.</{t}>
}}

<{t} class="row justify-content-center">
    <{t} class="col-lg-8">
        <{t} class="card-nutri mb-4">
            <form id="profileForm" class="needs-validation" novalidate>
                <{t} class="row g-3">
                    <{t} class="col-12">
                        <label class="form-label">Email</label>
                        <input type="email" class="form-control form-control-nutri" value="@Model.Email" readonly disabled />
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Name</label>
                        <input type="text" class="form-control form-control-nutri" id="name" value="@Model.Name" required />
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Age</label>
                        <input type="number" class="form-control form-control-nutri" id="age" value="@Model.Age" required />
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Gender</label>
                        <select class="form-select form-control-nutri" id="gender">
                            <option value="Male" selected="@(Model.Gender == "Male")">Male</option>
                            <option value="Female" selected="@(Model.Gender == "Female")">Female</option>
                            <option value="Other" selected="@(Model.Gender == "Other")">Other</option>
                        </select>
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Height (cm)</label>
                        <input type="number" class="form-control form-control-nutri" id="height" value="@Model.Height" required />
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Current Weight (kg)</label>
                        <input type="number" step="0.1" class="form-control form-control-nutri" id="currentWeight" value="@Model.CurrentWeight" required />
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Goal Weight (kg)</label>
                        <input type="number" step="0.1" class="form-control form-control-nutri" id="goalWeight" value="@Model.GoalWeight" required />
                    </{t}>
                    <{t} class="col-md-6">
                        <label class="form-label">Daily Water Goal (ml)</label>
                        <input type="number" class="form-control form-control-nutri" id="dailyWater" value="@Model.DailyWaterTargetMl" min="500" max="6000" required />
                    </{t}>
                    <{t} class="col-12">
                        <label class="form-label">Activity Level</label>
                        <select class="form-select form-control-nutri" id="activityLevel">
                            <option value="Sedentary" selected="@(Model.ActivityLevel == "Sedentary")">Sedentary</option>
                            <option value="Lightly Active" selected="@(Model.ActivityLevel == "Lightly Active")">Lightly Active</option>
                            <option value="Moderately Active" selected="@(Model.ActivityLevel == "Moderately Active")">Moderately Active</option>
                            <option value="Very Active" selected="@(Model.ActivityLevel == "Very Active")">Very Active</option>
                            <option value="Extra Active" selected="@(Model.ActivityLevel == "Extra Active")">Extra Active</option>
                        </select>
                    </{t}>
                </{t}>
                <button type="submit" class="btn btn-primary-nutri mt-4">Save Profile</button>
            </form>
        </{t}>

        <{t} class="card-nutri">
            <h3 class="h5 mb-3">Change Password</h3>
            <form asp-controller="Auth" asp-action="ChangePassword" method="post" class="needs-validation" novalidate>
                @Html.AntiForgeryToken()
                <{t} class="mb-3">
                    <label class="form-label">Current Password</label>
                    <input type="password" name="CurrentPassword" class="form-control form-control-nutri" required autocomplete="current-password" />
                </{t}>
                <{t} class="mb-3">
                    <label class="form-label">New Password</label>
                    <input type="password" name="NewPassword" class="form-control form-control-nutri" required minlength="8" autocomplete="new-password" />
                </{t}>
                <{t} class="mb-3">
                    <label class="form-label">Confirm New Password</label>
                    <input type="password" name="ConfirmPassword" class="form-control form-control-nutri" required minlength="8" autocomplete="new-password" />
                </{t}>
                <button type="submit" class="btn btn-outline-nutri">Update Password</button>
            </form>
        </{t}>
    </{t}>
</{t}>

@section Scripts {{
    <script src="~/js/profile.js" asp-append-version="true"></script>
}}
"""

path.write_text(content, encoding="utf-8")
print("wrote", path)
