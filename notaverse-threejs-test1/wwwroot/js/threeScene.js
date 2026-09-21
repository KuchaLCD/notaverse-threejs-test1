import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";

// ======================== ИНИЦИАЛИЗАЦИЯ СЦЕНЫ ========================
const scene = new THREE.Scene();
scene.background = new THREE.Color(0xe1e9f7);

const camera = new THREE.PerspectiveCamera(
    45,
    window.innerWidth / window.innerHeight,
    0.1,
    1000
);
camera.position.set(5, 5, 5);
camera.lookAt(0, 0, 0);

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.2;
renderer.outputColorSpace = THREE.SRGBColorSpace;

const container = document.getElementById("three-container");
container.appendChild(renderer.domElement);

// ======================== КАМЕРА И НАВИГАЦИЯ ========================
const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.dampingFactor = 0.05;
controls.screenSpacePanning = true;
// Настройки под Blender-стиль (СКМ — вращение, Shift+СКМ — панорама)
controls.mouseButtons = {
    LEFT: null,       // ЛКМ зарезервирована под выбор объектов
    MIDDLE: THREE.MOUSE.ROTATE,  // СКМ — вращение
    RIGHT: THREE.MOUSE.PAN       // ПКМ — панорама (или редактирование)
};

// ======================== ОСВЕЩЕНИЕ ========================
const dirLight = new THREE.DirectionalLight(0xffeedd, 2.5);
dirLight.position.set(10, 20, 10);
dirLight.castShadow = true;
dirLight.shadow.mapSize.width = 4096;
dirLight.shadow.mapSize.height = 4096;
dirLight.shadow.bias = -0.001;
const d = 25;
dirLight.shadow.camera.left = -d;
dirLight.shadow.camera.right = d;
dirLight.shadow.camera.top = d;
dirLight.shadow.camera.bottom = -d;
dirLight.shadow.camera.near = 1;
dirLight.shadow.camera.far = 50;
scene.add(dirLight);

const hemiLight = new THREE.HemisphereLight(0x87ceeb, 0x3a3a3a, 0.6);
scene.add(hemiLight);

const ambientLight = new THREE.AmbientLight(0x404060, 0.4);
scene.add(ambientLight);

// ======================== СОСТОЯНИЕ ========================
let currentMode = "view";          // "view" | "edit"
let selectedObject = null;
let hoveredObject = null;
let originalColors = new Map();     // Mesh → HEX-цвет (для сброса подсветки)
let objectDataMap = new Map();      // objectId → SceneObjectData (из C#)
let dotNetHelper = null;            // Ссылка на C#-объект (через interop)
const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();
let currentModel = null;

// ======================== ЗАГРУЗКА МОДЕЛЕЙ ========================
function clearModel() {
    if (currentModel) {
        scene.remove(currentModel);
        currentModel = null;
    }
    // Сбрасываем подсветку и выделение
    clearHighlight();
    selectedObject = null;
    hoveredObject = null;
}

/**
 * Загрузка модели из массива байт (Uint8Array).
 * Вызывается из C# через ExecuteScriptAsync.
 */
window.loadModelFromBytes = function (bytes, fileName) {
    clearModel();

    // bytes приходит как Uint8Array из C# через CallWithBase64Async
    const arrayBuffer = bytes.buffer
        ? bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength)
        : bytes;

    const extension = fileName.split(".").pop().toLowerCase();

    if (extension === "glb" || extension === "gltf") {
        const loader = new GLTFLoader();
        loader.parse(
            arrayBuffer,
            "",
            (gltf) => {
                const model = gltf.scene || gltf;
                addModelToScene(model);
                notifyCSharp("modelLoaded", { fileName, status: "success" });
            },
            (error) => {
                console.error("Ошибка загрузки GLB/GLTF:", error);
                notifyCSharp("modelLoaded", { fileName, status: "error", message: String(error) });
            }
        );
    } else {
        notifyCSharp("modelLoaded", { fileName, status: "error", message: "Неподдерживаемый формат" });
    }
};

/**
 * Загрузка модели по URL (из виртуального хоста WebView2).
 */
window.loadModelFromUrl = function (url, fileName) {
    clearModel();

    const extension = fileName.split(".").pop().toLowerCase();

    if (extension === "glb" || extension === "gltf") {
        const loader = new GLTFLoader();
        loader.load(
            url,
            (gltf) => {
                const model = gltf.scene || gltf;
                addModelToScene(model);
                notifyCSharp("modelLoaded", { fileName, status: "success" });
            },
            undefined,
            (error) => {
                console.error("Ошибка загрузки GLB/GLTF:", error);
                notifyCSharp("modelLoaded", { fileName, status: "error", message: error.message });
            }
        );
    }
};

function addModelToScene(model) {
    let meshIndex = 0;
    model.traverse((child) => {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;

            // Стабильный ID: используем имя из Blender, если оно осмысленное,
            // иначе — индекс в порядке обхода
            if (!child.userData.objectId) {
                const name = (child.name || "").trim();
                child.userData.objectId = name && name !== "Object"
                    ? name
                    : `mesh_${meshIndex}`;
            }
            meshIndex++;
        }
    });

    scene.add(model);
    currentModel = model;

    // Центрирование и настройка камеры под размер модели
    const box = new THREE.Box3().setFromObject(model);
    const size = box.getSize(new THREE.Vector3()).length();
    const center = box.getCenter(new THREE.Vector3());
    model.position.sub(center);

    const dist = size / (2 * Math.tan((camera.fov * Math.PI) / 360));
    camera.position.set(dist * 1.2, dist * 0.8, dist * 1.2);
    controls.target.set(0, 0, 0);
    controls.update();

    // Подстраиваем теневую камеру под размер модели
    try {
        const d = size * 0.6;
        dirLight.shadow.camera.left = -d;
        dirLight.shadow.camera.right = d;
        dirLight.shadow.camera.top = d;
        dirLight.shadow.camera.bottom = -d;
        dirLight.shadow.camera.near = 0.1;
        dirLight.shadow.camera.far = size * 2.5;
        dirLight.shadow.camera.updateProjectionMatrix();  // ← исправлено
    } catch (shadowErr) {
        console.warn("Не удалось обновить настройки тени:", shadowErr);
        // Не прерываем загрузку модели — тени не критичны
    }

    // После загрузки модели запрашиваем у C# данные объектов
    notifyCSharp("modelReady", { meshCount: countMeshes(model) });
}

function countMeshes(obj) {
    let count = 0;
    obj.traverse((child) => { if (child.isMesh) count++; });
    return count;
}

// ======================== АНИМАЦИЯ ========================
function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}
animate();

// ======================== RESIZE ========================
window.addEventListener("resize", () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});

// ======================== ПОДСВЕТКА ========================
function clearHighlight() {
    if (hoveredObject) {
        const originalColor = originalColors.get(hoveredObject);
        if (originalColor !== undefined && hoveredObject.material) {
            hoveredObject.material.color.setHex(originalColor);
        }
        originalColors.delete(hoveredObject);
        hoveredObject = null;
    }
    renderer.domElement.style.cursor = "default";
}

function applyHighlight(mesh) {
    if (!mesh.material) return;
    const originalColor = mesh.material.color.getHex();
    originalColors.set(mesh, originalColor);
    mesh.material.color.setHex(0xffa500); // Оранжевый
    renderer.domElement.style.cursor = "pointer";
}

// ======================== ОБРАБОТЧИКИ МЫШИ ========================
renderer.domElement.addEventListener("mousemove", onMouseMove);
function onMouseMove(event) {
    if (currentMode !== "edit") {
        if (hoveredObject) clearHighlight();
        return;
    }

    const rect = renderer.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);

    const meshes = [];
    scene.traverse((child) => { if (child.isMesh) meshes.push(child); });

    const intersects = raycaster.intersectObjects(meshes);

    let newHovered = null;
    if (intersects.length > 0) {
        newHovered = intersects[0].object;
    }

    if (newHovered !== hoveredObject) {
        clearHighlight();
        if (newHovered) {
            applyHighlight(newHovered);
            hoveredObject = newHovered;
        }
    }
}

renderer.domElement.addEventListener("click", onClick);
function onClick(event) {
    const rect = renderer.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);

    const meshes = [];
    scene.traverse((child) => { if (child.isMesh) meshes.push(child); });

    const intersects = raycaster.intersectObjects(meshes);

    if (intersects.length === 0) {
        if (currentMode === "view" && selectedObject) {
            selectedObject = null;
            notifyCSharp("objectDeselected", {});
        }
        return;
    }

    const clickedObject = intersects[0].object;
    const objectId = clickedObject.userData.objectId || clickedObject.uuid;
    const objectData = objectDataMap.get(objectId) || null;

    if (currentMode === "view") {
        selectedObject = clickedObject;
        notifyCSharp("objectSelected", { objectId, data: objectData });
    } else if (currentMode === "edit") {
        if (hoveredObject === clickedObject) {
            notifyCSharp("objectEdit", { objectId, data: objectData });
        }
    }
}

// ======================== УПРАВЛЕНИЕ ИЗ C# ========================

/**
 * Установка режима: "view" или "edit".
 */
window.setMode = function (mode) {
    currentMode = mode;
    clearHighlight();
    selectedObject = null;
    console.log("Режим изменён на:", mode);
};

/**
 * Установка данных объектов (из C#).
 * data — массив SceneObjectData.
 */
window.setObjectData = function (data) {
    objectDataMap.clear();
    if (data && Array.isArray(data)) {
        data.forEach((item) => {
            objectDataMap.set(item.id, item);
        });
    }
    console.log("Данные объектов обновлены. Записей:", objectDataMap.size);
};

/**
 * Обновление данных одного объекта (после редактирования в WPF).
 */
window.updateObjectData = function (payload) {
    if (!payload || !payload.objectId) {
        console.warn("updateObjectData: некорректный payload", payload);
        return;
    }
    if (payload.data) {
        objectDataMap.set(payload.objectId, payload.data);
    } else {
        objectDataMap.delete(payload.objectId);
    }
    console.log("Данные объекта обновлены:", payload.objectId);
};

/**
 * Регулировка яркости.
 */
window.setBrightness = function (value) {
    dirLight.intensity = value;
};

// ======================== МОСТ С C# ========================
function notifyCSharp(type, payload) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage({
            type: type,
            payload: payload || {}
        });
    } else {
        console.warn("chrome.webview не доступен — приложение запущено не в WebView2?");
    }
}

window.initializeDotNetHelper = function (helper) {
    dotNetHelper = helper;
    console.log("DotNetHelper инициализирован");
};

// Обработка drop-события в JS threeScene.js
document.addEventListener("DOMContentLoaded", () => {
    // Drop-зона для загрузки .glb
    const container = document.getElementById("three-container");

    container.addEventListener("dragover", (e) => {
        e.preventDefault();
        container.style.border = "3px dashed #ffa500";
    });

    container.addEventListener("dragleave", () => {
        container.style.border = "none";
    });

    container.addEventListener("drop", async (e) => {
        e.preventDefault();
        container.style.border = "none";

        const files = e.dataTransfer.files;
        if (!files || files.length === 0) return;

        const file = files[0];
        const ext = file.name.split(".").pop().toLowerCase();

        if (ext === "glb" || ext === "gltf") {
            const arrayBuffer = await file.arrayBuffer();
            const bytes = new Uint8Array(arrayBuffer);
            window.loadModelFromBytes(bytes, file.name);
        } else {
            console.warn("Неподдерживаемый формат:", ext);
        }
    });
});