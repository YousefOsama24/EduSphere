document.addEventListener("DOMContentLoaded", function () {
    // 1. إضافة أنيميشن تدريجي لصفوف الجدول عند التحميل
    const tableRows = document.querySelectorAll(".table tbody tr");
    tableRows.forEach((row, index) => {
        row.style.opacity = "0";
        row.style.transform = "translateY(10px)";
        row.style.transition = "all 0.3s ease";
        
        setTimeout(() => {
            row.style.opacity = "1";
            row.style.transform = "translateY(0)";
        }, index * 50); // بيعمل تأثير تتابع (Stagger effect)
    });

    // 2. تلوين حواف الـ Inputs عند الـ Focus بشكل جمالي
    const formInputs = document.querySelectorAll(".form-control, .form-select");
    formInputs.forEach(input => {
        input.addEventListener("focus", function () {
            this.parentElement.classList.add("focused");
        });
        input.addEventListener("blur", function () {
            this.parentElement.classList.remove("focused");
        });
    });
});