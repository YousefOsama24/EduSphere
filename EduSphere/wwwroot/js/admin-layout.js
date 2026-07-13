// ============================================================================
// admin-layout.js — sidebar drawer behaviour for the Super Admin shell
// (kept separate from admin-dashboard.js, which handles page-content actions)
// ============================================================================
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", init);

  function init() {
    initSidebarDrawer();
    markActiveNavFromUrl();
  }

  function initSidebarDrawer() {
    var toggle = document.getElementById("sidebar-toggle");
    var sidebar = document.getElementById("admin-sidebar");
    var scrim = document.getElementById("admin-scrim");
    if (!toggle || !sidebar) return;

    function close() {
      sidebar.classList.remove("open");
      if (scrim) scrim.classList.remove("visible");
      toggle.setAttribute("aria-expanded", "false");
    }
    function open() {
      sidebar.classList.add("open");
      if (scrim) scrim.classList.add("visible");
      toggle.setAttribute("aria-expanded", "true");
    }

    toggle.addEventListener("click", function () {
      sidebar.classList.contains("open") ? close() : open();
    });
    if (scrim) scrim.addEventListener("click", close);
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") close();
    });
  }

  // Highlight the sidebar link matching the current URL path
  function markActiveNavFromUrl() {
    var links = document.querySelectorAll(".admin-nav a[href]");
    var path = window.location.pathname.replace(/\/$/, "");

    links.forEach(function (link) {
      var linkPath = link.getAttribute("href").replace(/\/$/, "");
      if (linkPath && linkPath !== "#" && path.indexOf(linkPath) === 0) {
        links.forEach(function (l) { l.classList.remove("active"); });
        link.classList.add("active");
      }
    });
  }
})();
