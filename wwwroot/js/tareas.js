const TAREA_STORAGE_KEY = 'tareas';

export function getTareas() {
    const data = localStorage.getItem(TAREA_STORAGE_KEY);
    return data ? JSON.parse(data) : [];
}

export function saveTareas(tareas) {
    localStorage.setItem(TAREA_STORAGE_KEY, JSON.stringify(tareas));
}

export function clearTareas() {
    localStorage.removeItem(TAREA_STORAGE_KEY);
}