// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    document.addEventListener('input', function (event) {
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || !input.hasAttribute('data-phone-number')) return;

        const caret = input.selectionStart ?? input.value.length;
        const digitsOnly = input.value.replace(/[^0-9]/g, '').slice(0, 10);
        if (input.value !== digitsOnly) {
            input.value = digitsOnly;
            const nextCaret = Math.min(caret, digitsOnly.length);
            input.setSelectionRange(nextCaret, nextCaret);
        }
    });

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
