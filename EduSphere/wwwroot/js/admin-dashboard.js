// ============================================================================
// admin-dashboard.js — Super Admin dashboard interactions
// ============================================================================
(function () {
  "use strict";

  document.addEventListener("DOMContentLoaded", init);

  function init() {
    initSidebarActiveState();
    initSearch();
    initRowActions();
    initQuickActions();
  }

  // Highlight the current section in the sidebar based on data-page attribute
  function initSidebarActiveState() {
    var links = document.querySelectorAll(".admin-nav a");
    links.forEach(function (link) {
      link.addEventListener("click", function (e) {
        links.forEach(function (l) { l.classList.remove("active"); });
        link.classList.add("active");
      });
    });
  }

  // Simple client-side filter over the registry-log table rows
  function initSearch() {
    var input = document.getElementById("admin-search");
    var table = document.getElementById("activity-log");
    if (!input || !table) return;

    var rows = Array.prototype.slice.call(table.querySelectorAll("tbody tr"));

    input.addEventListener("input", debounce(function () {
      var term = input.value.trim().toLowerCase();
      rows.forEach(function (row) {
        var text = row.textContent.toLowerCase();
        row.style.display = term === "" || text.indexOf(term) !== -1 ? "" : "none";
      });
    }, 200));
  }

  // Approve / reject / view actions on each row of the activity log
  function initRowActions() {
    var table = document.getElementById("activity-log");
    if (!table) return;

    table.addEventListener("click", function (e) {
      var btn = e.target.closest("[data-action]");
      if (!btn) return;

      var action = btn.getAttribute("data-action");
      var row = btn.closest("tr");
      var pill = row ? row.querySelector(".status-pill") : null;

      if (action === "approve" && pill) {
        pill.textContent = "معتمد";
        pill.className = "status-pill approved";
        showToast("تم اعتماد الدورة بنجاح");
      } else if (action === "reject" && pill) {
        pill.textContent = "مرفوض";
        pill.className = "status-pill rejected";
        showToast("تم رفض الدورة");
      } else if (action === "view") {
        showToast("جارٍ فتح تفاصيل السجل...");
      }
    });
  }

  // Quick action buttons in the sidebar panel (e.g. export report, clear cache)
  function initQuickActions() {
    var buttons = document.querySelectorAll(".quick-actions [data-quick-action]");
    buttons.forEach(function (btn) {
      btn.addEventListener("click", function () {
        var label = btn.getAttribute("data-quick-action");
        showToast(label + " — تم التنفيذ");
      });
    });
  }

  // Minimal toast helper, no external dependency
  var toastTimer = null;
  function showToast(message) {
    var toast = document.getElementById("admin-toast");
    if (!toast) {
      toast = document.createElement("div");
      toast.id = "admin-toast";
      toast.className = "admin-toast";
      document.body.appendChild(toast);
    }
    toast.textContent = message;
    toast.classList.add("visible");

    clearTimeout(toastTimer);
    toastTimer = setTimeout(function () {
      toast.classList.remove("visible");
    }, 2200);
  }

  function debounce(fn, wait) {
    var timer = null;
    return function () {
      var args = arguments;
      clearTimeout(timer);
      timer = setTimeout(function () { fn.apply(null, args); }, wait);
    };
  }
})();
