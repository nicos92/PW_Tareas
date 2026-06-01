(function () {
    'use strict';

    function applyStoredTheme() {
        const theme = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-bs-theme', theme);
    }

    window.setTheme = function (theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
    };

    window.getTheme = function () {
        return localStorage.getItem('theme') || 'light';
    };

    applyStoredTheme();

    let enhancedHooked = false;

    function registerEnhancedNavigation() {
        if (enhancedHooked) {
            return true;
        }
        if (window.Blazor && typeof Blazor.addEventListener === 'function') {
            Blazor.addEventListener('enhancedload', applyStoredTheme);
            enhancedHooked = true;
            return true;
        }
        return false;
    }

    if (!registerEnhancedNavigation()) {
        document.addEventListener('DOMContentLoaded', function () {
            registerEnhancedNavigation();
        });
    }
})();
