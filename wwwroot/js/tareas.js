const TAREA_STORAGE_KEY = 'tareas';

window.getTareas =  function () {
    const data = localStorage.getItem(TAREA_STORAGE_KEY);
    return data ? JSON.parse(data) : [];
}

window.saveTareas =  function (tareas) {
    localStorage.setItem(TAREA_STORAGE_KEY, JSON.stringify(tareas));
}

window.clearTareas = function () {
    localStorage.removeItem(TAREA_STORAGE_KEY);
}