// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const layout = document.getElementById("appLayout");
    const toggle = document.getElementById("sidebarToggle");

    if (!layout || !toggle) return;

    const mobile = window.matchMedia("(max-width: 768px)");
    const storageKey = "rangrasiya-sidebar-collapsed";

    if (!mobile.matches &&
        localStorage.getItem(storageKey) === "true") {
        layout.classList.add("sidebar-collapsed");
    }

    function updateButton() {
        const isMobile = mobile.matches;
        const collapsed = layout.classList.contains("sidebar-collapsed");
        const open = layout.classList.contains("mobile-sidebar-open");

        toggle.textContent = isMobile
            ? (open ? "✕" : "☰")
            : (collapsed ? "☰" : "⇤");

        toggle.title = isMobile
            ? (open ? "Close sidebar" : "Open sidebar")
            : (collapsed ? "Expand sidebar" : "Collapse sidebar");

        toggle.setAttribute("aria-label", toggle.title);
    }

    toggle.addEventListener("click", function () {
        if (mobile.matches) {
            layout.classList.toggle("mobile-sidebar-open");
        } else {
            layout.classList.toggle("sidebar-collapsed");

            localStorage.setItem(
                storageKey,
                layout.classList.contains("sidebar-collapsed")
                    ? "true"
                    : "false"
            );
        }

        updateButton();
    });

    mobile.addEventListener("change", function () {
        layout.classList.remove("mobile-sidebar-open");
        updateButton();
    });

    updateButton();
});
