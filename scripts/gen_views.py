import os

BASE = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", "NutriAI"))
el = "div"
o = "<" + el
c = "</" + el + ">"

def tag(cls=None, id=None, close=False):
    if close:
        return c
    parts = [o]
    if cls:
        parts.append(' class="' + cls + '"')
    if id:
        parts.append(' id="' + id + '"')
    parts.append(">")
    return "".join(parts)

def w(rel, lines):
    path = os.path.join(BASE, rel.replace("/", os.sep))
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))

# Weight
w("Views/Weight/Index.cshtml", [
    "@{",
    '    ViewData["Title"] = "Weight Tracker";',
    '    ViewData["ActiveNav"] = "Weight";',
    "}",
    "",
    tag("page-header fade-in") + tag(close=True),
    "    <h1>Weight Tracker</h1>",
    '    <p class="subtitle">Track your weight and progress toward your goal</p>',
    c,
    "",
    tag("row g-4 mb-4") + tag(close=True),
    '    ' + tag("col-md-4") + tag(close=True),
    '        ' + tag("card-nutri text-center hover-scale") + tag(close=True),
    '            <p class="card-title">Current Weight</p>',
    '            <p class="stat-value text-success" id="displayCurrentWeight">--</p>',
    '            <span class="small-text text-muted">kg</span>',
    "        " + c,
    "    " + c,
    '    ' + tag("col-md-4") + tag(close=True),
    '        ' + tag("card-nutri text-center hover-scale") + tag(close=True),
    '            <p class="card-title">Goal Weight</p>',
    '            <p class="stat-value text-primary" id="displayGoalWeight">--</p>',
    '            <span class="small-text text-muted">kg</span>',
    "        " + c,
    "    " + c,
    '    ' + tag("col-md-4") + tag(close=True),
    '        ' + tag("card-nutri text-center hover-scale") + tag(close=True),
    '            <p class="card-title">To Go</p>',
    '            <p class="stat-value text-warning" id="weightToGo">--</p>',
    '            <span class="small-text text-muted">kg</span>',
    "        " + c,
    "    " + c,
    c,
    "",
    tag("row g-4") + tag(close=True),
    '    ' + tag("col-lg-8") + tag(close=True),
    '        ' + tag("card-nutri") + tag(close=True),
    '            <h3 class="h5 mb-3">Weight Progress</h3>',
    '            <canvas id="weightChart" height="100"></canvas>',
    "        " + c,
    "    " + c,
    '    ' + tag("col-lg-4") + tag(close=True),
    '        ' + tag("card-nutri mb-4") + tag(close=True),
    '            <h3 class="h6 mb-3">Add Weight</h3>',
    '            <form id="weightForm">',
    '                ' + tag("mb-3") + tag(close=True),
    '                    <label class="form-label">Weight (kg)</label>',
    '                    <input type="number" step="0.1" class="form-control form-control-nutri" id="weightInput" required />',
    "                " + c,
    '                <button type="submit" class="btn btn-primary-nutri w-100">Save</button>',
    "            </form>",
    "        " + c,
    '        ' + tag("card-nutri") + tag(close=True),
    '            <h3 class="h6 mb-3">History</h3>',
    '            <div id="weightHistoryList" class="small-text"></motion>',
    "        " + c,
    "    " + c,
    c,
    "",
    "@section Scripts {",
    '    <script src="~/js/weight.js" asp-append-version="true"></script>',
    "}",
])

print("Generated Weight view")
