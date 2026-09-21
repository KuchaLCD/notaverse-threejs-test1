/**
 * Триггер для <input type="file">.
 */
window.triggerFileInput = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) element.click();
};

/**
 * Чтение файлов из <input type="file">.
 */
window.getSelectedFiles = function (elementId) {
    return new Promise((resolve, reject) => {
        const element = document.getElementById(elementId);
        if (!element) {
            reject(new Error("Элемент не найден: " + elementId));
            return;
        }
        const files = element.files;
        const result = [];
        let loaded = 0;
        if (files.length === 0) {
            resolve([]);
            return;
        }
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const reader = new FileReader();
            reader.onload = function (e) {
                const base64 = e.target.result.split(",")[1];
                result.push({ fileName: file.name, fileData: base64 });
                loaded++;
                if (loaded === files.length) resolve(result);
            };
            reader.onerror = function (e) {
                reject(e.target.error);
            };
            reader.readAsDataURL(file);
        }
    });
};

/**
 * Настройка drop-зоны для drag&drop файлов.
 * @param {string} elementId — ID DOM-элемента
 * @param {object} dotNetHelper — ссылка на C#-объект
 * @param {number} paramIndex — индекс параметра
 */
window.setupDropZone = function (elementId, paramIndex) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.warn("setupDropZone: элемент не найден", elementId);
        return;
    }

    element.addEventListener("dragover", (e) => {
        e.preventDefault();
        element.style.borderColor = "#ffa500";
    });

    element.addEventListener("dragleave", () => {
        element.style.borderColor = "#aaa";
    });

    element.addEventListener("drop", (e) => {
        e.preventDefault();
        element.style.borderColor = "#aaa";
        const files = e.dataTransfer.files;
        if (!files || files.length === 0) return;

        const result = [];
        let loaded = 0;
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const reader = new FileReader();
            reader.onload = function (ev) {
                const base64 = ev.target.result.split(",")[1];
                result.push({ fileName: file.name, fileData: base64 });
                loaded++;
                if (loaded === files.length) {
                    window.chrome.webview.postMessage({
                        type: "filesDropped",
                        payload: { paramIndex: paramIndex, files: result }
                    });
                }
            };
            reader.onerror = function (ev) {
                console.error("Ошибка чтения файла:", ev.target.error);
            };
            reader.readAsDataURL(file);
        }
    });
};