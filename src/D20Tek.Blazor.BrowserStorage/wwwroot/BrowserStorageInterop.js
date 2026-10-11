const listeners = new Map();
let nextId = 0;

export function addStorageListener(dotNetRef, storageName) {
    const id = nextId++;
    const target = storageName === 'localStorage' ? localStorage : sessionStorage;

    const handler = (e) => {
        if (e.storageArea === target) {
            dotNetRef.invokeMethodAsync('OnStorageChanged', e.key, e.oldValue, e.newValue);
        }
    };

    window.addEventListener('storage', handler);
    listeners.set(id, handler);
    return id;
}

export function removeStorageListener(id) {
    const handler = listeners.get(id);
    if (handler) {
        window.removeEventListener('storage', handler);
        listeners.delete(id);
    }
}

function parseCookies() {
    const result = new Map();
    const raw = document.cookie;
    if (!raw) return result;

    for (const part of raw.split('; ')) {
        const idx = part.indexOf('=');
        if (idx < 0) continue;
        const key = decodeURIComponent(part.substring(0, idx));
        const value = decodeURIComponent(part.substring(idx + 1));
        result.set(key, value);
    }

    return result;
}

export function getCookie(key) {
    const cookies = parseCookies();
    return cookies.has(key) ? cookies.get(key) : null;
}

export function setCookie(key, value, maxAgeSeconds, path, domain, sameSite, secure) {
    let cookie = `${encodeURIComponent(key)}=${encodeURIComponent(value)}`;

    if (maxAgeSeconds !== null && maxAgeSeconds !== undefined) {
        cookie += `;max-age=${maxAgeSeconds}`;
    }
    if (path) {
        cookie += `;path=${path}`;
    }
    if (domain) {
        cookie += `;domain=${domain}`;
    }
    if (sameSite) {
        cookie += `;samesite=${sameSite}`;
    }
    if (secure) {
        cookie += ';secure';
    }

    document.cookie = cookie;
}

export function removeCookie(key, path, domain) {
    let cookie = `${encodeURIComponent(key)}=;max-age=0`;
    if (path) {
        cookie += `;path=${path}`;
    }
    if (domain) {
        cookie += `;domain=${domain}`;
    }

    document.cookie = cookie;
}

export function getAllCookieKeys() {
    return Array.from(parseCookies().keys());
}

export function isCookieAvailable() {
    try {
        const probeKey = '__d20tek_cookie_probe__';
        document.cookie = `${probeKey}=1;max-age=60`;
        const available = document.cookie.indexOf(probeKey) !== -1;
        document.cookie = `${probeKey}=;max-age=0`;
        return available;
    } catch (e) {
        return false;
    }
}
