import pathlib

d = chr(100) + chr(105) + chr(118)
path = pathlib.Path(__file__).resolve().parents[1] / "NutriAI" / "Views" / "Dashboard" / "Index.cshtml"
text = path.read_text(encoding="utf-8")
text = text.replace("<motion ", f"<{d} ").replace("</motion>", f"</{d}>")

old_charts = """<motion class="card-nutri dashboard-chart-card">
            <h3 class="h5 mb-3">Calorie Tracking</h3>
            <div class="dashboard-chart-wrap">
                <canvas id="calorieChart"></canvas>
            </div>
        </div>"""

# normalize any motion tags first
text = text.replace("<motion ", f"<{d} ").replace("</motion>", f"</{d}>")

replacements = [
    (
        """        <motion class="card-nutri">
            <h3 class="h6 mb-3">Weight Progress</h3>
            <canvas id="weightMiniChart" height="150"></canvas>
        </div>""".replace("motion", d),
        f"""        <{d} class="card-nutri dashboard-chart-card">
            <h3 class="h6 mb-3">Weight Progress</h3>
            <{d} class="dashboard-chart-wrap dashboard-chart-wrap-sm">
                <canvas id="weightMiniChart"></canvas>
            </{d}>
        </{d}>""",
    ),
    (
        f'<div class="row g-4">\n    <div class="col-lg-6">\n        <div class="card-nutri">\n            <h3 class="h5 mb-3">Recent Meals</h3>\n            <motion id="recentMealsList"></div>',
        f'<div class="row g-4 dashboard-lists-row">\n    <div class="col-lg-6">\n        <{d} class="card-nutri dashboard-list-card">\n            <h3 class="h5 mb-3">Recent Meals</h3>\n            <{d} id="recentMealsList" class="dashboard-list-body"></{d}>',
    ),
]

# simpler full-file approach from git + patch
base = """@{
    ViewData["Title"] = "Dashboard";
    ViewData["ActiveNav"] = "Dashboard";
}

<div class="page-header fade-in">
    <h1>Dashboard</h1>
    <p class="subtitle">Your daily nutrition overview</p>
</div>

<div class="row g-4 mb-4 fade-in">
    <motion class="col-md-6 col-lg-3">
        <motion class="card-nutri dashboard-stat-card hover-scale">
"""

# Just fix motion tags in current file
path.write_text(text.replace("<motion ", f"<{d} ").replace("</motion>", f"</{d}>"), encoding="utf-8")
print("tag fix done")
