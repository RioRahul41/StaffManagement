// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    const body = document.body;
    const toggle = document.getElementById('darkModeToggle');
    const label = document.getElementById('darkModeLabel');
    const storageKey = 'staff-management-dark-mode';

    function setDarkMode(enabled) {
        body.classList.toggle('dark-mode', enabled);
        if (label) label.textContent = enabled ? 'Light mode' : 'Dark mode';
    }

    setDarkMode(localStorage.getItem(storageKey) === 'true');
    if (toggle) {
        toggle.addEventListener('click', function () {
            const enabled = !body.classList.contains('dark-mode');
            localStorage.setItem(storageKey, enabled.toString());
            setDarkMode(enabled);
        });
    }
})();
