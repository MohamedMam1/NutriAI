import re
import pathlib

root = pathlib.Path(__file__).resolve().parents[1] / "NutriAI"
html = (root / "nutriai-diet-landing.html").read_text(encoding="utf-8")
body = re.search(r"<body>\s*(.*?)\s*<script src", html, re.S).group(1)
body = body.replace(
    '<button class="btn-nav-ghost d-none d-md-block">Sign in</button>',
    '<a class="btn-nav-ghost d-none d-md-block text-decoration-none" asp-controller="Auth" asp-action="Login">Sign in</a>',
)
body = body.replace(
    '<button class="btn-nav-primary">Start free <i class="bi bi-arrow-right ms-1"></i></button>',
    '<a class="btn-nav-primary text-decoration-none" asp-controller="Auth" asp-action="Register">Start free <i class="bi bi-arrow-right ms-1"></i></a>',
)
body = body.replace('href="#" class="btn-hero"', 'asp-controller="Auth" asp-action="Register" class="btn-hero"')
body = body.replace('<button class="btn-pricing-outline">Get started free</button>', '<a class="btn-pricing-outline text-decoration-none d-block text-center" asp-controller="Auth" asp-action="Register">Get started free</a>')
body = body.replace('<button class="btn-pricing-solid">Start 14-day free trial</button>', '<a class="btn-pricing-solid text-decoration-none d-block text-center" asp-controller="Auth" asp-action="Register">Start 14-day free trial</a>')
body = body.replace('<a asp-controller="Home" asp-action="Privacy">', '<a asp-controller="Home" asp-action="Privacy">')  # noqa: placeholder
(body_path := root / "Views" / "Home" / "_LandingContent.cshtml").write_text(body, encoding="utf-8")
print(f"Wrote {body_path}")
