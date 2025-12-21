/**
 * Theme Manager for NovaCalc Hub
 * Handles light/dark mode toggle and persistence
 */

class ThemeManager {
    constructor() {
        this.theme = this.getStoredTheme() || 'light';
        this.init();
    }

    init() {
        // Apply theme on load
        this.applyTheme(this.theme);
        
        // Set up toggle button listeners
        this.setupToggleListeners();
        
        // Watch for system preference changes
        this.watchSystemPreference();
    }

    getStoredTheme() {
        return localStorage.getItem('theme');
    }

    setStoredTheme(theme) {
        localStorage.setItem('theme', theme);
    }

    applyTheme(theme) {
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        
        // Update toggle button icon
        this.updateToggleIcon(theme);
    }

    toggleTheme() {
        this.theme = this.theme === 'light' ? 'dark' : 'light';
        this.setStoredTheme(this.theme);
        this.applyTheme(this.theme);
    }

    setupToggleListeners() {
        const toggleButtons = document.querySelectorAll('.theme-toggle');
        toggleButtons.forEach(button => {
            button.addEventListener('click', () => this.toggleTheme());
        });
    }

    updateToggleIcon(theme) {
        const toggleButtons = document.querySelectorAll('.theme-toggle');
        toggleButtons.forEach(button => {
            const sunIcon = button.querySelector('.sun-icon');
            const moonIcon = button.querySelector('.moon-icon');
            
            if (sunIcon && moonIcon) {
                if (theme === 'dark') {
                    sunIcon.classList.remove('hidden');
                    moonIcon.classList.add('hidden');
                } else {
                    sunIcon.classList.add('hidden');
                    moonIcon.classList.remove('hidden');
                }
            }
        });
    }

    watchSystemPreference() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        mediaQuery.addEventListener('change', (e) => {
            if (!this.getStoredTheme()) {
                this.theme = e.matches ? 'dark' : 'light';
                this.applyTheme(this.theme);
            }
        });
    }
}

// Initialize theme manager when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.themeManager = new ThemeManager();
});
