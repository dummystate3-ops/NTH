/* Performance Optimization Utilities */

// Lazy load images
document.addEventListener('DOMContentLoaded', function() {
    // Intersection Observer for lazy loading
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                    }
                    if (img.dataset.srcset) {
                        img.srcset = img.dataset.srcset;
                        img.removeAttribute('data-srcset');
                    }
                    img.classList.remove('lazy');
                    observer.unobserve(img);
                }
            });
        }, {
            rootMargin: '50px 0px',
            threshold: 0.01
        });

        document.querySelectorAll('img.lazy').forEach(img => {
            imageObserver.observe(img);
        });
    } else {
        // Fallback for older browsers
        document.querySelectorAll('img.lazy').forEach(img => {
            if (img.dataset.src) img.src = img.dataset.src;
            if (img.dataset.srcset) img.srcset = img.dataset.srcset;
        });
    }

    // Lazy load iframes (embeds, videos)
    if ('IntersectionObserver' in window) {
        const iframeObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const iframe = entry.target;
                    if (iframe.dataset.src) {
                        iframe.src = iframe.dataset.src;
                        iframe.removeAttribute('data-src');
                    }
                    observer.unobserve(iframe);
                }
            });
        }, {
            rootMargin: '100px 0px'
        });

        document.querySelectorAll('iframe[data-src]').forEach(iframe => {
            iframeObserver.observe(iframe);
        });
    }

    // Preconnect to external domains
    const preconnectDomains = [
        'https://fonts.googleapis.com',
        'https://fonts.gstatic.com',
        'https://www.googletagmanager.com',
        'https://pagead2.googlesyndication.com'
    ];

    preconnectDomains.forEach(domain => {
        const link = document.createElement('link');
        link.rel = 'preconnect';
        link.href = domain;
        link.crossOrigin = 'anonymous';
        document.head.appendChild(link);
    });
});

// Debounce function for performance
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Throttle function for performance
function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Optimize scroll events
window.addEventListener('scroll', throttle(function() {
    // Your scroll handling code here
}, 100));

// Cache DOM queries
const cache = new Map();
function querySelector(selector) {
    if (!cache.has(selector)) {
        cache.set(selector, document.querySelector(selector));
    }
    return cache.get(selector);
}

function querySelectorAll(selector) {
    if (!cache.has(selector)) {
        cache.set(selector, document.querySelectorAll(selector));
    }
    return cache.get(selector);
}

// Web Vitals reporting (optional - for monitoring)
if (typeof PerformanceObserver !== 'undefined') {
    // Largest Contentful Paint (LCP)
    new PerformanceObserver((entryList) => {
        const entries = entryList.getEntries();
        const lastEntry = entries[entries.length - 1];
        console.log('LCP:', lastEntry.renderTime || lastEntry.loadTime);
    }).observe({ entryTypes: ['largest-contentful-paint'] });

    // First Input Delay (FID)
    new PerformanceObserver((entryList) => {
        const entries = entryList.getEntries();
        entries.forEach(entry => {
            console.log('FID:', entry.processingStart - entry.startTime);
        });
    }).observe({ entryTypes: ['first-input'] });

    // Cumulative Layout Shift (CLS)
    let clsScore = 0;
    new PerformanceObserver((entryList) => {
        for (const entry of entryList.getEntries()) {
            if (!entry.hadRecentInput) {
                clsScore += entry.value;
                console.log('CLS:', clsScore);
            }
        }
    }).observe({ entryTypes: ['layout-shift'] });
}

// Resource hints
function addResourceHint(rel, href, as) {
    const link = document.createElement('link');
    link.rel = rel;
    link.href = href;
    if (as) link.as = as;
    document.head.appendChild(link);
}

// Prefetch next page (for navigation optimization)
function prefetchPage(url) {
    addResourceHint('prefetch', url);
}

// Critical CSS inline, defer non-critical
function optimizeStylesheets() {
    const stylesheets = document.querySelectorAll('link[rel="stylesheet"]:not([data-critical])');
    stylesheets.forEach(link => {
        link.media = 'print';
        link.onload = function() {
            this.media = 'all';
        };
    });
}

// Service Worker registration (PWA support)
if ('serviceWorker' in navigator && location.protocol === 'https:') {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/sw.js')
            .then(registration => {
                console.log('Service Worker registered:', registration.scope);
            })
            .catch(error => {
                console.log('Service Worker registration failed:', error);
            });
    });
}

// Network Information API (adaptive loading)
if ('connection' in navigator) {
    const connection = navigator.connection;
    if (connection.effectiveType === '4g') {
        // Load high-quality resources
        document.body.classList.add('high-speed');
    } else if (connection.effectiveType === '3g' || connection.effectiveType === '2g') {
        // Load lower-quality resources, defer non-essential
        document.body.classList.add('low-speed');
    }

    if (connection.saveData) {
        // User has data saver enabled
        document.body.classList.add('data-saver');
    }
}

// Request Idle Callback for non-critical tasks
if ('requestIdleCallback' in window) {
    requestIdleCallback(() => {
        // Non-critical tasks here (analytics, tracking, etc.)
    });
} else {
    setTimeout(() => {
        // Fallback
    }, 1);
}

// Export utilities
window.PerformanceUtils = {
    debounce,
    throttle,
    querySelector,
    querySelectorAll,
    prefetchPage,
    addResourceHint
};
