// wwwroot/sw.js
self.addEventListener("install", (event) => {
  console.log("Service Worker instalado");
  self.skipWaiting();
});

self.addEventListener("fetch", (event) => {
  // Por ahora no cacheamos nada offline, solo dejamos pasar la petición para cumplir el requisito PWA
  event.respondWith(fetch(event.request));
});
