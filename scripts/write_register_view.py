import pathlib

t = chr(100) + chr(105) + chr(118)
path = pathlib.Path(__file__).resolve().parents[1] / "NutriAI" / "Views" / "Auth" / "Register.cshtml"

content = f"""@model RegisterViewModel
@{{
    Layout = "_AuthLayout";
    ViewData["Title"] = "Register";
}}

<{t} class="auth-wrapper">
    <{t} class="auth-card register-card">
        <{t} class="brand">
            <h2><i class="fas fa-leaf"></i> NutriAI</h2>
            <p>Create your account and set up your nutrition profile.</p>
        </{t}>
        <form asp-action="Register" method="post" class="needs-validation" novalidate>
            @Html.AntiForgeryToken()
            <{t} asp-validation-summary="ModelOnly" class="text-danger mb-3"></{t}>
            <h3 class="h6 text-success mb-3">Account</h3>
            <{t} class="row g-3">
                <{t} class="col-md-6">
                    <label asp-for="FullName" class="form-label"></label>
                    <input asp-for="FullName" class="form-control form-control-nutri" />
                    <span asp-validation-for="FullName" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-6">
                    <label asp-for="Email" class="form-label"></label>
                    <input asp-for="Email" class="form-control form-control-nutri" />
                    <span asp-validation-for="Email" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-6">
                    <label asp-for="Password" class="form-label"></label>
                    <{t} class="input-group">
                        <input asp-for="Password" class="form-control form-control-nutri" id="password" />
                        <span class="input-group-text password-toggle" id="togglePassword"><i class="fas fa-eye"></i></span>
                    </{t}>
                    <span asp-validation-for="Password" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-6">
                    <label asp-for="ConfirmPassword" class="form-label"></label>
                    <{t} class="input-group">
                        <input asp-for="ConfirmPassword" class="form-control form-control-nutri" id="confirmPassword" />
                        <span class="input-group-text password-toggle" id="toggleConfirmPassword"><i class="fas fa-eye"></i></span>
                    </{t}>
                    <span asp-validation-for="ConfirmPassword" class="text-danger small-text"></span>
                </{t}>
            </{t}>

            <h3 class="h6 text-success mb-3 mt-2">Health profile</h3>
            <{t} class="row g-3">
                <{t} class="col-md-4">
                    <label asp-for="Age" class="form-label"></label>
                    <input asp-for="Age" class="form-control form-control-nutri" type="number" min="13" max="120" />
                    <span asp-validation-for="Age" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-4">
                    <label asp-for="Gender" class="form-label"></label>
                    <select asp-for="Gender" class="form-select form-control-nutri">
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                        <option value="Other">Other</option>
                    </select>
                </{t}>
                <{t} class="col-md-4">
                    <label asp-for="HeightCm" class="form-label"></label>
                    <input asp-for="HeightCm" class="form-control form-control-nutri" type="number" step="0.1" min="50" max="300" />
                    <span asp-validation-for="HeightCm" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-4">
                    <label asp-for="CurrentWeightKg" class="form-label"></label>
                    <input asp-for="CurrentWeightKg" class="form-control form-control-nutri" type="number" step="0.1" min="20" max="500" />
                    <span asp-validation-for="CurrentWeightKg" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-4">
                    <label asp-for="GoalWeightKg" class="form-label"></label>
                    <input asp-for="GoalWeightKg" class="form-control form-control-nutri" type="number" step="0.1" min="20" max="500" />
                    <span asp-validation-for="GoalWeightKg" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-md-4">
                    <label asp-for="DailyWaterTargetMl" class="form-label"></label>
                    <input asp-for="DailyWaterTargetMl" class="form-control form-control-nutri" type="number" min="500" max="6000" />
                    <span asp-validation-for="DailyWaterTargetMl" class="text-danger small-text"></span>
                </{t}>
                <{t} class="col-12">
                    <label asp-for="ActivityLevel" class="form-label"></label>
                    <select asp-for="ActivityLevel" class="form-select form-control-nutri">
                        <option value="Sedentary">Sedentary</option>
                        <option value="Lightly Active">Lightly Active</option>
                        <option value="Moderately Active">Moderately Active</option>
                        <option value="Very Active">Very Active</option>
                        <option value="Extra Active">Extra Active</option>
                    </select>
                </{t}>
            </{t}>

            <button type="submit" class="btn btn-primary-nutri w-100 mb-3 mt-3">Create Account</button>
            <p class="text-center small-text text-muted mb-0">
                Already have an account? <a asp-action="Login" class="text-success fw-semibold">Sign In</a>
            </p>
        </form>
    </{t}>
</{t}>

@section Scripts {{
    <partial name="_ValidationScriptsPartial" />
    <script src="~/js/auth.js" asp-append-version="true"></script>
}}
"""

path.write_text(content, encoding="utf-8")
print("wrote", path)
