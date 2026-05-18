import pathlib

REPLACEMENTS = [
    ("</motion>", "</motion>"),
    ("<motion ", "<motion "),
    ("createElement('motion')", "createElement('label')"),
]

# Use explicit div tags
REPLACEMENTS = [
    ("</" + "motion>", "</" + "motion>"),
]
