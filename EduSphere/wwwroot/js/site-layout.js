// ============================================================================
// site-layout.js — public site shell behaviour (mobile nav, sticky topbar)
// ============================================================================
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", init);

  function init() {
    initMobileNav();
    initStickyTopbar();
  }

  function initMobileNav() {
    var toggle = document.getElementById("nav-toggle");
    var nav = document.getElementById("site-nav");
    var scrim = document.getElementById("nav-scrim");
    if (!toggle || !nav) return;

    function close() {
      nav.classList.remove("open");
      if (scrim) scrim.classList.remove("visible");
      toggle.setAttribute("aria-expanded", "false");
    }
    function open() {
      nav.classList.add("open");
      if (scrim) scrim.classList.add("visible");
      toggle.setAttribute("aria-expanded", "true");
    }

    toggle.addEventListener("click", function () {
      var isOpen = nav.classList.contains("open");
      isOpen ? close() : open();
    });

    if (scrim) scrim.addEventListener("click", close);

    // Close drawer when a nav link is clicked (single-page anchors too)
    nav.querySelectorAll("a").forEach(function (link) {
      link.addEventListener("click", close);
    });

    // Close on escape
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") close();
    });
  }

  function initStickyTopbar() {
    var topbar = document.getElementById("site-topbar");
    if (!topbar) return;

    window.addEventListener("scroll", function () {
      topbar.classList.toggle("is-scrolled", window.scrollY > 8);
    }, { passive: true });
  }
})();
