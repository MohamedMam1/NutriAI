// ── Navbar scroll effect ──
const nav = document.querySelector('.navbar-nutriai');
window.addEventListener('scroll', () => {
  nav.classList.toggle('scrolled', window.scrollY > 20);
});

// ── Scroll-reveal observer ──
const reveals = document.querySelectorAll('.reveal');
const observer = new IntersectionObserver((entries) => {
  entries.forEach(e => {
    if (e.isIntersecting) { e.target.classList.add('in'); observer.unobserve(e.target); }
  });
}, { threshold: 0.07, rootMargin: '0px 0px -40px 0px' });
reveals.forEach(el => observer.observe(el));

// ── Smooth anchor scrolling ──
document.querySelectorAll('a[href^="#"]').forEach(a => {
  a.addEventListener('click', e => {
    const target = document.querySelector(a.getAttribute('href'));
    if (target) {
      e.preventDefault();
      target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  });
});

// ── Live stats from database ──
(async function loadLandingStats() {
  try {
    const response = await fetch('/Home/GetLandingStats');
    if (!response.ok) return;
    const data = await response.json();

    // Update all elements that have a data-stat attribute
    document.querySelectorAll('[data-stat]').forEach(el => {
      const key = el.dataset.stat;
      if (data[key] !== undefined) {
        animateNumber(el, data[key]);
      }
    });
  } catch (err) {
    // Silently fail — stats will show the "—" placeholder
    console.warn('Could not load landing stats:', err);
  }
})();

/**
 * Animate a number counting up from 0 to the target value.
 * Formats large numbers with commas for readability.
 */
function animateNumber(el, target, duration = 1200) {
  if (target === 0) {
    el.textContent = '0';
    return;
  }
  const start = performance.now();
  const format = (n) => n.toLocaleString('en-US');

  function step(now) {
    const progress = Math.min((now - start) / duration, 1);
    // Ease-out cubic for a natural deceleration feel
    const eased = 1 - Math.pow(1 - progress, 3);
    const current = Math.round(eased * target);
    el.textContent = format(current);
    if (progress < 1) requestAnimationFrame(step);
  }
  requestAnimationFrame(step);
}
