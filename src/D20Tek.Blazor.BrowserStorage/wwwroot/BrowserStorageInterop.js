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
