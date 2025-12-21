/**
 * Service Worker for NovaCalc Hub PWA
 * Provides offline functionality and caching strategies
 */

const CACHE_NAME = 'novacalc-v1.0.0';
const RUNTIME_CACHE = 'novacalc-runtime';

// Assets to cache on install
const PRECACHE_ASSETS = [
    '/',
    '/css/site.css',
    '/js/theme.js',
    '/js/utils.js',
    '/manifest.json',
    '/images/icons/icon-192x192.png',
    '/images/icons/icon-512x512.png'
];

// Install event - cache core assets
self.addEventListener('install', (event) => {
    console.log('[SW] Installing service worker...');
    
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                console.log('[SW] Precaching assets');
                return cache.addAll(PRECACHE_ASSETS);
            })
            .then(() => self.skipWaiting())
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    console.log('[SW] Activating service worker...');
    
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames
                    .filter((cacheName) => {
                        return cacheName !== CACHE_NAME && cacheName !== RUNTIME_CACHE;
                    })
                    .map((cacheName) => {
                        console.log('[SW] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    })
            );
        }).then(() => self.clients.claim())
    );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }

    // Skip chrome extension requests
    if (url.protocol === 'chrome-extension:') {
        return;
    }

    // Handle API calls with network-first strategy
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(networkFirst(request));
        return;
    }

    // Handle navigation requests with network-first strategy
    if (request.mode === 'navigate') {
        event.respondWith(networkFirst(request));
        return;
    }

    // Handle static assets with cache-first strategy
    event.respondWith(cacheFirst(request));
});

/**
 * Cache-first strategy
 * Try to serve from cache first, fallback to network
 */
async function cacheFirst(request) {
    const cache = await caches.open(CACHE_NAME);
    const cached = await cache.match(request);
    
    if (cached) {
        console.log('[SW] Serving from cache:', request.url);
        return cached;
    }

    try {
        const response = await fetch(request);
        
        // Cache successful responses
        if (response.status === 200) {
            const runtimeCache = await caches.open(RUNTIME_CACHE);
            runtimeCache.put(request, response.clone());
        }
        
        return response;
    } catch (error) {
        console.error('[SW] Fetch failed:', error);
        
        // Return offline page for navigation requests
        if (request.mode === 'navigate') {
            return cache.match('/offline.html');
        }
        
        throw error;
    }
}

/**
 * Network-first strategy
 * Try network first, fallback to cache
 */
async function networkFirst(request) {
    try {
        const response = await fetch(request);
        
        // Cache successful responses
        if (response.status === 200) {
            const cache = await caches.open(RUNTIME_CACHE);
            cache.put(request, response.clone());
        }
        
        return response;
    } catch (error) {
        console.log('[SW] Network failed, trying cache:', request.url);
        
        const cache = await caches.open(RUNTIME_CACHE);
        const cached = await cache.match(request);
        
        if (cached) {
            return cached;
        }
        
        // Return offline page for navigation requests
        if (request.mode === 'navigate') {
            const mainCache = await caches.open(CACHE_NAME);
            return mainCache.match('/offline.html');
        }
        
        throw error;
    }
}

// Listen for messages from clients
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

// Background sync for future use
self.addEventListener('sync', (event) => {
    console.log('[SW] Background sync:', event.tag);
    
    if (event.tag === 'sync-data') {
        event.waitUntil(syncData());
    }
});

async function syncData() {
    // Implement background sync logic here
    console.log('[SW] Syncing data...');
}
