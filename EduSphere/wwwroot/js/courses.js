// ============================================================================
// courses.js — Registry filter/search behaviour for the courses landing page
// ============================================================================
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", init);

  function init() {
    var searchInput = document.getElementById("course-search");
    var categorySelect = document.getElementById("course-category");
    var levelSelect = document.getElementById("course-level");
    var applyBtn = document.getElementById("apply-filters");
    var grid = document.getElementById("course-grid");
    var emptyState = document.getElementById("archive-empty");
    var countTag = document.getElementById("result-count");

    if (!grid) return;

    var cards = Array.prototype.slice.call(grid.querySelectorAll(".course-card"));

    function normalize(text) {
      return (text || "").toString().trim().toLowerCase();
    }

    function applyFilters() {
      var term = normalize(searchInput ? searchInput.value : "");
      var category = categorySelect ? categorySelect.value : "all";
      var level = levelSelect ? levelSelect.value : "all";
      var visibleCount = 0;

      cards.forEach(function (card) {
        var title = normalize(card.getAttribute("data-title"));
        var instructor = normalize(card.getAttribute("data-instructor"));
        var cardCategory = card.getAttribute("data-category");
        var cardLevel = card.getAttribute("data-level");

        var matchesTerm = term === "" || title.indexOf(term) !== -1 || instructor.indexOf(term) !== -1;
        var matchesCategory = category === "all" || cardCategory === category;
        var matchesLevel = level === "all" || cardLevel === level;

        var isVisible = matchesTerm && matchesCategory && matchesLevel;
        card.style.display = isVisible ? "" : "none";
        if (isVisible) visibleCount++;
      });

      if (countTag) {
        countTag.textContent = visibleCount + " دورة مطابقة";
      }

      if (emptyState) {
        emptyState.classList.toggle("visible", visibleCount === 0);
      }
    }

    if (applyBtn) applyBtn.addEventListener("click", applyFilters);
    if (searchInput) {
      searchInput.addEventListener("input", debounce(applyFilters, 200));
      searchInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter") applyFilters();
      });
    }
    if (categorySelect) categorySelect.addEventListener("change", applyFilters);
    if (levelSelect) levelSelect.addEventListener("change", applyFilters);

    // Run once on load in case the server pre-filled a query string
    applyFilters();
  }

  function debounce(fn, wait) {
    var timer = null;
    return function () {
      var args = arguments;
      clearTimeout(timer);
      timer = setTimeout(function () {
        fn.apply(null, args);
      }, wait);
    };
  }
})();
