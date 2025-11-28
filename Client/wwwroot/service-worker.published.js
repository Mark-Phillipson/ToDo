// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Additional runtime cache for sample data and API calls
const runtimeCacheName = 'runtime-cache-v1';

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash }));
    
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
    
    // Cache sample data and other runtime resources
    const runtimeCache = await caches.open(runtimeCacheName);
    try {
        await runtimeCache.addAll([
            'sample-data/todo.json',
            'manifest.json'
        ]);
    } catch (error) {
        console.warn('Failed to cache runtime resources:', error);
    }
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate';

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
        
        // If not in main cache, try runtime cache for sample data and other resources
        if (!cachedResponse) {
            const runtimeCache = await caches.open(runtimeCacheName);
            cachedResponse = await runtimeCache.match(request);
        }
        
        // If we're going to the network, try to update the cache
        if (!cachedResponse) {
            try {
                const networkResponse = await fetch(event.request);
                
                // Cache successful responses for future offline use
                if (networkResponse && networkResponse.status === 200) {
                    const responseToCache = networkResponse.clone();
                    // Cache sample-data and other runtime resources
                    if (event.request.url.includes('sample-data') || 
                        event.request.url.includes('manifest.json')) {
                        const runtimeCache = await caches.open(runtimeCacheName);
                        await runtimeCache.put(event.request, responseToCache);
                    }
                }
                
                return networkResponse;
            } catch (error) {
                console.log('Fetch failed; returning offline fallback if available:', error);
                
                // If we're offline and requesting sample data, return a minimal fallback
                if (event.request.url.includes('sample-data/todo.json')) {
                    return new Response('[]', {
                        headers: { 'Content-Type': 'application/json' }
                    });
                }
            }
        }
    }

    return cachedResponse || fetch(event.request);
}
