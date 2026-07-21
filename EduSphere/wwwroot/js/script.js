// Brightleaf Learning Center — shared UI behaviour
// Sidebar toggle (mobile), backdrop, and a couple of demo-only interactions.
// Everything here is presentation-only; wire real handlers up when this
// template is dropped into your MVC views.

document.addEventListener('DOMContentLoaded', function () {
  var sidebar   = document.getElementById('appSidebar');
  var backdrop  = document.getElementById('sidebarBackdrop');
  var toggles   = document.querySelectorAll('[data-toggle="sidebar"]');

  function openSidebar () {
    sidebar.classList.add('show');
    backdrop.classList.add('show');
  }
  function closeSidebar () {
    sidebar.classList.remove('show');
    backdrop.classList.remove('show');
  }

  toggles.forEach(function (btn) {
    btn.addEventListener('click', openSidebar);
  });
  if (backdrop) backdrop.addEventListener('click', closeSidebar);

  // Mark current nav link active based on data-page match (fallback to first)
  document.querySelectorAll('.sidebar-nav .nav-link').forEach(function (link) {
    link.addEventListener('click', function () {
      if (link.getAttribute('data-bs-toggle') || link.getAttribute('href') === '#') return;
    });
  });

  // Enable all Bootstrap tooltips on the page
  var tooltipEls = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
  tooltipEls.forEach(function (el) { new bootstrap.Tooltip(el); });
});
