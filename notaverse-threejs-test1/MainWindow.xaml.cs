using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using notaverse_threejs_test1.Models;
using notaverse_threejs_test1.Services;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Path = System.IO.Path;

namespace notaverse_threejs_test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ==================== СЕРВИСЫ ====================
        private readonly DataService _dataService = new();
        private readonly FileService _fileService = new();
        private WebViewBridge _bridge;

        // ==================== ДАННЫЕ СЦЕНЫ ====================
        private SceneConfig _sceneConfig = new();
        private SceneObjectData _selectedObjectData = null;
        private List<SceneObjectData> _objectsData = new();

        // ==================== СОСТОЯНИЕ РЕДАКТИРОВАНИЯ ====================
        private bool _isEditMode = false;

        // ==================== ИНИЦИАЛИЗАЦИЯ ====================
        public MainWindow()
        {
            InitializeComponent();
            _bridge = new WebViewBridge(WebView);

            // Отслеживание изменения названия проекта
            TxtProjectName.TextChanged += (s, e) => _isProjectDirty = true;

            // Асинхронная инициализация WebView2
            InitializeWebViewAsync();
        }

        // ==================== СОСТОЯНИЕ ПРОЕКТА ====================
        private SceneConfig _currentProject = new();
        private bool _isProjectDirty = false;   // есть ли несохранённые изменения

        /// <summary>
        /// Скрывает обе панели (просмотра и редактирования).
        /// Вызывается, когда пользователь кликнул по пустому месту сцены
        /// (сообщение "objectDeselected" из JS).
        /// </summary>
        private void HidePanels()
        {
            ViewInfoPanel.Visibility = Visibility.Collapsed;
            EditMenuPanel.Visibility = Visibility.Collapsed;
            _selectedObjectData = null;
        }

        /*
        private async void InitializeWebViewAsync()
        {
            try
            {
                // 1. Создаём окружение WebView2
                var userDataFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "WebView2_Data");
                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: userDataFolder);

                await WebView.EnsureCoreWebView2Async(env);

                // 2. Настраиваем виртуальный хост для загрузки локальных файлов
                var wwwrootPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local",
                    wwwrootPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // 2.1 Настройка второй виртуальный хост в MainWindow.xaml.cs для папки Models/
                var modelsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
                if (!Directory.Exists(modelsPath))
                    Directory.CreateDirectory(modelsPath);

                // 2.2 Второй виртуальный хост для моделей — грузим .glb через URL
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "models.local",
                    modelsPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // 3. Навигация на локальную страницу
                WebView.CoreWebView2.Navigate("https://app.local/index.html");

                // 4. Подписываемся на события
                WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

                // 5. Разрешаем drag&drop файлов
                WebView.AllowExternalDrop = true;

                // 6. Загружаем сохранённую конфигурацию
                _sceneConfig = await _dataService.LoadSceneConfigAsync();
                _objectsData = _sceneConfig.Objects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации WebView2: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        private async void InitializeWebViewAsync()
        {
            try
            {
                var userDataFolder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "WebView2_Data");
                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: userDataFolder);

                await WebView.EnsureCoreWebView2Async(env);

                var wwwrootPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local", wwwrootPath, CoreWebView2HostResourceAccessKind.Allow);

                // Второй хост — для папки Models
                var modelsPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Models");
                Directory.CreateDirectory(modelsPath);
                WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "models.local", modelsPath, CoreWebView2HostResourceAccessKind.Allow);

                WebView.CoreWebView2.Navigate("https://app.local/index.html");

                WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
                WebView.AllowExternalDrop = true;

                // Пытаемся восстановить последний проект
                var lastId = await _dataService.GetCurrentProjectPointerAsync();
                if (!string.IsNullOrEmpty(lastId))
                {
                    _currentProject = await _dataService.LoadProjectAsync(lastId);
                }
                else
                {
                    _currentProject = new SceneConfig();
                }

                // Синхронизируем UI с загруженным проектом
                TxtProjectName.Text = _currentProject.ProjectName;
                _objectsData = _currentProject.Objects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации WebView2: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
        private async void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // БОЛЬШЕ НЕ НУЖНО создавать DotNetObjectReference!
                // Достаточно просто передать данные в JS.

                // Передаём данные объектов
                await _bridge.CallWithJsonAsync("setObjectData", _objectsData);

                // Устанавливаем начальный режим
                await _bridge.CallAsync("setMode", "view");
            }
        }
        */
        private async void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess) return;

            // 1. Передаём данные объектов
            await _bridge.CallWithJsonAsync("setObjectData", _objectsData);

            // 2. Устанавливаем режим вращения из проекта
            var rotationMode = string.IsNullOrEmpty(_currentProject.RotationMode)
                ? "skm" : _currentProject.RotationMode;
            await _bridge.CallAsync("setRotationMode", rotationMode);

            // Синхронизируем ComboBox
            foreach (ComboBoxItem item in RotationModeCombo.Items)
            {
                if (item.Tag as string == rotationMode)
                {
                    RotationModeCombo.SelectedItem = item;
                    break;
                }
            }

            // 3. Устанавливаем режим работы
            await _bridge.CallAsync("setMode", "view");

            // 4. Загружаем модель, если она была в проекте
            if (!string.IsNullOrEmpty(_currentProject.ModelFilePath))
            {
                await LoadModelFromProjectAsync();
            }

            // 5. Восстанавливаем камеру
            await RestoreCameraAsync();
        }

        /// <summary>
        /// Загрузка модели из текущего проекта (используется при старте и при открытии проекта).
        /// </summary>
        private async Task LoadModelFromProjectAsync()
        {
            if (string.IsNullOrEmpty(_currentProject.ModelFilePath)) return;

            var absPath = _fileService.GetModelAbsolutePath(_currentProject.ModelFilePath);
            if (!File.Exists(absPath))
            {
                MessageBox.Show(
                    $"Файл модели не найден:\n{_currentProject.ModelFilePath}\n\n" +
                    "Загрузите .glb вручную через кнопку «Загрузить модель».",
                    "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fileNameOnly = Path.GetFileName(_currentProject.ModelFilePath);
            var url = $"https://models.local/{fileNameOnly}";
            await _bridge.CallAsync("loadModelFromUrl", url, fileNameOnly);
        }

        /// <summary>
        /// Восстановление камеры из проекта.
        /// </summary>
        private async Task RestoreCameraAsync()
        {
            if (_currentProject.Camera == null) return;
            var cam = _currentProject.Camera;
            await WebView.CoreWebView2.ExecuteScriptAsync(
                $"camera.position.set({cam.PositionX}, {cam.PositionY}, {cam.PositionZ});" +
                $"controls.target.set({cam.TargetX}, {cam.TargetY}, {cam.TargetZ});" +
                "controls.update();");
        }

        /// <summary>
        /// Синхронизация камеры из JS в конфиг проекта.
        /// </summary>
        private async Task SyncCameraToProjectAsync()
        {
            var cameraJson = await WebView.CoreWebView2.ExecuteScriptAsync(
                "JSON.stringify({" +
                "positionX: camera.position.x," +
                "positionY: camera.position.y," +
                "positionZ: camera.position.z," +
                "targetX: controls.target.x," +
                "targetY: controls.target.y," +
                "targetZ: controls.target.z" +
                "})");

            var unescaped = JsonSerializer.Deserialize<string>(cameraJson);
            if (!string.IsNullOrEmpty(unescaped))
            {
                var cam = JsonSerializer.Deserialize<CameraConfig>(unescaped);
                if (cam != null) _currentProject.Camera = cam;
            }
        }

        /// <summary>
        /// Полное сохранение текущего проекта.
        /// </summary>
        private async Task SaveCurrentProjectAsync()
        {
            _currentProject.ProjectName = TxtProjectName.Text;
            _currentProject.Objects = _objectsData;
            await SyncCameraToProjectAsync();

            await _dataService.SaveProjectAsync(_currentProject);
            await _dataService.SaveCurrentProjectPointerAsync(_currentProject.ProjectId);
            _isProjectDirty = false;
        }

        /// <summary>
        /// Полная загрузка проекта — модель, объекты, камера.
        /// </summary>
        private async Task LoadProjectIntoSceneAsync(SceneConfig project)
        {
            _currentProject = project;
            _objectsData = project.Objects;
            TxtProjectName.Text = project.ProjectName;

            // Передаём объекты в JS
            await _bridge.CallWithJsonAsync("setObjectData", _objectsData);

            // Режим вращения
            var rotationMode = string.IsNullOrEmpty(project.RotationMode)
                ? "skm" : project.RotationMode;
            await _bridge.CallAsync("setRotationMode", rotationMode);
            foreach (ComboBoxItem item in RotationModeCombo.Items)
            {
                if (item.Tag as string == rotationMode)
                {
                    RotationModeCombo.SelectedItem = item;
                    break;
                }
            }

            // Модель
            await LoadModelFromProjectAsync();

            // Камера
            await RestoreCameraAsync();

            // Запоминаем текущий проект
            await _dataService.SaveCurrentProjectPointerAsync(project.ProjectId);
        }

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_isProjectDirty)
            {
                var res = MessageBox.Show(
                    "В текущем проекте есть несохранённые изменения. Сохранить перед выходом?",
                    "Выход",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (res == MessageBoxResult.Yes)
                {
                    e.Cancel = true; // откладываем закрытие
                    await SaveCurrentProjectAsync();
                    Close(); // закрываем повторно, теперь _isProjectDirty = false
                }
            }
            base.OnClosing(e);
        }

        // ==================== ПРОЕКТЫ ====================

        private async void NewProject_Click(object sender, RoutedEventArgs e)
        {
            if (_isProjectDirty)
            {
                var res = MessageBox.Show(
                    "В текущем проекте есть несохранённые изменения. Сохранить?",
                    "Новый проект",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.Yes) await SaveCurrentProjectAsync();
            }

            // Создаём новый проект
            _currentProject = new SceneConfig
            {
                ProjectName = "Новый проект",
                RotationMode = "skm"
            };
            _objectsData = new List<SceneObjectData>();

            // Очищаем сцену
            await _bridge.CallWithJsonAsync("setObjectData", _objectsData);
            await WebView.CoreWebView2.ExecuteScriptAsync("clearModel();");

            // Синхронизируем UI
            TxtProjectName.Text = _currentProject.ProjectName;
            HidePanels();
        }

        private async void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (_isProjectDirty)
            {
                var res = MessageBox.Show(
                    "В текущем проекте есть несохранённые изменения. Сохранить?",
                    "Открыть проект",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.Yes) await SaveCurrentProjectAsync();
            }

            // Загружаем список проектов
            var projects = await _dataService.ListProjectsAsync();
            ProjectsList.ItemsSource = projects;

            if (projects.Count == 0)
            {
                MessageBox.Show("Пока нет сохранённых проектов.",
                    "Открыть проект", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenProjectPanel.Visibility = Visibility.Visible;
        }

        private async void ConfirmOpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedItem is not ProjectInfo info)
            {
                MessageBox.Show("Выберите проект из списка.",
                    "Открыть проект", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var project = await _dataService.LoadProjectAsync(info.Id);

            // Очищаем сцену и загружаем новый проект
            await WebView.CoreWebView2.ExecuteScriptAsync("clearModel();");
            await LoadProjectIntoSceneAsync(project);

            OpenProjectPanel.Visibility = Visibility.Collapsed;
        }

        private void CloseOpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenProjectPanel.Visibility = Visibility.Collapsed;
        }

        private async void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectsList.SelectedItem is not ProjectInfo info) return;

            var res = MessageBox.Show(
                $"Удалить проект «{info.Name}»? Это действие нельзя отменить.",
                "Удаление проекта",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                _dataService.DeleteProject(info.Id);

                // Обновляем список
                var projects = await _dataService.ListProjectsAsync();
                ProjectsList.ItemsSource = projects;
            }
        }

        private async void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await SaveCurrentProjectAsync();
                MessageBox.Show($"Проект «{_currentProject.ProjectName}» сохранён!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения проекта: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== ПРИЁМ СООБЩЕНИЙ ИЗ JS ====================
        private async void OnWebMessageReceived(
            object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.WebMessageAsJson;
                var message = JsonSerializer.Deserialize<JsMessage>(json);

                if (message == null) return;

                switch (message.Type)
                {
                    case "modelReady":
                        // Модель загружена — обновляем данные объектов
                        await _bridge.CallWithJsonAsync(
                            "setObjectData", _objectsData);
                        break;

                    case "objectSelected":
                        // Пользователь кликнул по объекту в режиме просмотра
                        await HandleObjectSelected(message.Payload);
                        break;

                    case "objectEdit":
                        // Пользователь кликнул по объекту в режиме редактирования
                        await HandleObjectEdit(message.Payload);
                        break;

                    case "objectDeselected":
                        HidePanels();
                        break;

                    case "modelLoaded":
                        // Модель загружена (успех или ошибка)
                        var status = message.Payload?.GetProperty("status").GetString();
                        if (status == "success")
                        {
                            MessageBox.Show("Модель успешно загружена!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            var errMsg = message.Payload?.GetProperty("message").GetString();
                            MessageBox.Show($"Ошибка загрузки модели: {errMsg}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обработки сообщения из JS: {ex.Message}");
            }
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ JS ====================
        private async Task HandleObjectSelected(JsonElement? payload)
        {
            if (payload == null) return;

            var objectId = payload.Value.GetProperty("objectId").GetString();
            var dataElement = payload.Value.TryGetProperty("data", out var d) ? d : (JsonElement?)null;

            SceneObjectData data = null;
            if (dataElement.HasValue && dataElement.Value.ValueKind != JsonValueKind.Null)
            {
                data = JsonSerializer.Deserialize<SceneObjectData>(
                    dataElement.Value.GetRawText());
            }

            // Если данных нет — создаём временный объект с ID
            if (data == null)
            {
                data = new SceneObjectData { Id = objectId, Name = objectId };
            }

            // Сохраняем ссылку на выбранный объект
            _selectedObjectData = data;

            // Показываем панель просмотра
            ShowViewPanel(data);

            // Передаём данные в JS (на случай, если их не было)
            await _bridge.CallWithJsonAsync("updateObjectData",
                new { objectId, data });
        }

        private async Task HandleObjectEdit(JsonElement? payload)
        {
            if (payload == null) return;

            var objectId = payload.Value.GetProperty("objectId").GetString();
            var dataElement = payload.Value.TryGetProperty("data", out var d) ? d : (JsonElement?)null;

            SceneObjectData data = null;
            if (dataElement.HasValue && dataElement.Value.ValueKind != JsonValueKind.Null)
            {
                data = JsonSerializer.Deserialize<SceneObjectData>(
                    dataElement.Value.GetRawText());
            }

            if (data == null)
            {
                data = new SceneObjectData { Id = objectId, Name = objectId };
            }

            _selectedObjectData = data;

            // Показываем панель редактирования
            TxtObjectName.Text = data.Name;
            FieldsItemsControl.ItemsSource = null;
            FieldsItemsControl.ItemsSource = data.Parameters;
            EditMenuPanel.Visibility = Visibility.Visible;
            ViewInfoPanel.Visibility = Visibility.Collapsed;
        }

        // ==================== УПРАВЛЕНИЕ РЕЖИМАМИ ====================
        private async void Mode_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            _isEditMode = RadioEdit.IsChecked == true;

            if (_isEditMode)
            {
                // Режим редактирования
                await _bridge.CallAsync("setMode", "edit");
                ViewInfoPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Режим просмотра
                await _bridge.CallAsync("setMode", "view");
                EditMenuPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void RotationMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;

            if (RotationModeCombo.SelectedItem is ComboBoxItem item &&
                item.Tag is string mode)
            {
                // Сообщаем JS о смене режима
                await _bridge.CallAsync("setRotationMode", mode);

                // Запоминаем в конфиге для сохранения
                _sceneConfig.RotationMode = mode;

                Debug.WriteLine($"Режим вращения изменён на: {mode}");
            }
        }

        // ==================== ЗАГРУЗКА МОДЕЛИ ====================

        /*
        private async void LoadModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D модели (*.glb;*.gltf)|*.glb;*.gltf|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var sourcePath = openFileDialog.FileName;

                // 1. Копируем модель во внутреннюю папку приложения
                using var sourceStream = File.OpenRead(sourcePath);
                var relativeModelPath = await _fileService.SaveModelAsync(
                    sourceStream, Path.GetFileName(sourcePath));

                // 2. Сохраняем ОТНОСИТЕЛЬНЫЙ путь (переносимо)
                _sceneConfig.ModelFilePath = relativeModelPath;

                // 3. Сохраняем конфиг сразу — чтобы модель «прилипла» к базе
                await _dataService.SaveSceneConfigAsync(_sceneConfig);

                // 4. Загружаем модель в JS через виртуальный хост
                var fileNameOnly = Path.GetFileName(relativeModelPath);
                var url = $"https://models.local/{fileNameOnly}";
                await _bridge.CallAsync("loadModelFromUrl", url, fileNameOnly);

                // Данные объектов обновятся в событии modelReady
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки модели: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        private async void LoadModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D модели (*.glb;*.gltf)|*.glb;*.gltf|Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var sourcePath = openFileDialog.FileName;

                // Копируем модель во внутреннюю папку
                using var sourceStream = File.OpenRead(sourcePath);
                var relativeModelPath = await _fileService.SaveModelAsync(
                    sourceStream, Path.GetFileName(sourcePath));

                _currentProject.ModelFilePath = relativeModelPath;
                _isProjectDirty = true;

                // Загружаем в JS
                var fileNameOnly = Path.GetFileName(relativeModelPath);
                var url = $"https://models.local/{fileNameOnly}";
                await _bridge.CallAsync("loadModelFromUrl", url, fileNameOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки модели: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== СОХРАНЕНИЕ/ЗАГРУЗКА ДАННЫХ ====================
        //private async void SaveData_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        _sceneConfig.Objects = _objectsData;

        //        // Синхронизируем камеру из JS
        //        var cameraJson = await WebView.CoreWebView2.ExecuteScriptAsync(
        //            "JSON.stringify({" +
        //            "positionX: camera.position.x," +
        //            "positionY: camera.position.y," +
        //            "positionZ: camera.position.z," +
        //            "targetX: controls.target.x," +
        //            "targetY: controls.target.y," +
        //            "targetZ: controls.target.z" +
        //            "})");

        //        // ExecuteScriptAsync возвращает JSON-строку в кавычках с экранированием
        //        var unescaped = JsonSerializer.Deserialize<string>(cameraJson);
        //        if (!string.IsNullOrEmpty(unescaped))
        //        {
        //            var cam = JsonSerializer.Deserialize<CameraConfig>(unescaped);
        //            if (cam != null) _sceneConfig.Camera = cam;
        //        }

        //        await _dataService.SaveSceneConfigAsync(_sceneConfig);

        //        MessageBox.Show("Данные сцены сохранены!",
        //            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка сохранения: {ex.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //private async void LoadData_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(_sceneConfig.RotationMode))
        //    {
        //        var mode = _sceneConfig.RotationMode;
        //        await _bridge.CallAsync("setRotationMode", mode);

        //        // Синхронизируем UI
        //        foreach (ComboBoxItem item in RotationModeCombo.Items)
        //        {
        //            if (item.Tag as string == mode)
        //            {
        //                RotationModeCombo.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }
        //    try
        //    {
        //        // 1. Читаем конфиг из JSON
        //        _sceneConfig = await _dataService.LoadSceneConfigAsync();
        //        _objectsData = _sceneConfig.Objects;

        //        // 2. Передаём данные объектов в JS
        //        await _bridge.CallWithJsonAsync("setObjectData", _objectsData);

        //        // 3. Восстанавливаем камеру
        //        if (_sceneConfig.Camera != null)
        //        {
        //            var cam = _sceneConfig.Camera;
        //            await WebView.CoreWebView2.ExecuteScriptAsync(
        //                $"camera.position.set({cam.PositionX}, {cam.PositionY}, {cam.PositionZ});" +
        //                $"controls.target.set({cam.TargetX}, {cam.TargetY}, {cam.TargetZ});" +
        //                "controls.update();");
        //        }

        //        // 4. Загружаем модель, если она была сохранена
        //        if (!string.IsNullOrEmpty(_sceneConfig.ModelFilePath))
        //        {
        //            var absPath = _fileService.GetModelAbsolutePath(_sceneConfig.ModelFilePath);

        //            if (File.Exists(absPath))
        //            {
        //                var fileNameOnly = Path.GetFileName(_sceneConfig.ModelFilePath);
        //                var url = $"https://models.local/{fileNameOnly}";
        //                await _bridge.CallAsync("loadModelFromUrl", url, fileNameOnly);
        //            }
        //            else
        //            {
        //                MessageBox.Show(
        //                    $"Файл модели не найден: {_sceneConfig.ModelFilePath}\n" +
        //                    "Загрузите .glb вручную через кнопку «Загрузить модель».",
        //                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            }
        //        }

        //        MessageBox.Show("Сцена и данные загружены!",
        //            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        // ==================== ПАНЕЛЬ ПРОСМОТРА ====================
        private void ShowViewPanel(SceneObjectData data)
        {
            ViewFieldsStack.Children.Clear();
            LblObjectTitle.Text = data.Name;

            foreach (var field in data.Parameters)
            {
                var rowGrid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });

                // Название поля
                var txtName = new TextBlock
                {
                    Text = $"{field.Name}:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(txtName, 0);
                rowGrid.Children.Add(txtName);

                // Значение
                FrameworkElement valueElement;

                if (field.Type == "image" && field.FilePaths.Count > 0)
                {
                    // Изображение (олд)
                    //var img = new Image
                    //{
                    //    MaxWidth = 200,
                    //    MaxHeight = 200,
                    //    Stretch = Stretch.Uniform,
                    //    Margin = new Thickness(0, 5, 0, 5)
                    //};

                    //try
                    //{
                    //    var absPath = _fileService.GetAbsolutePath(field.FilePaths[0]);
                    //    if (File.Exists(absPath))
                    //    {
                    //        var bitmap = new BitmapImage();
                    //        bitmap.BeginInit();
                    //        bitmap.UriSource = new Uri(absPath);
                    //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    //        bitmap.EndInit();
                    //        img.Source = bitmap;
                    //    }
                    //}
                    //catch { /* игнорируем */ }

                    //valueElement = img;

                    // Изображения
                    var img = new Image
                    {
                        MaxWidth = 200,
                        MaxHeight = 200,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 5, 0, 5),
                        Cursor = Cursors.Hand,
                        ToolTip = "Нажмите, чтобы открыть изображение в системном просмотрщике"
                    };

                    try
                    {
                        var absPath = _fileService.GetAbsolutePath(field.FilePaths[0]);
                        if (File.Exists(absPath))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(absPath);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad; // чтобы не блокировать файл
                            bitmap.EndInit();
                            img.Source = bitmap;
                        }
                        else
                        {
                            img.Source = null;
                            img.ToolTip = "Файл не найден";
                        }
                    }
                    catch { /* игнорируем */ }

                    // Обработчик клика — открывает файл через системное приложение
                    var relPath = field.FilePaths[0]; // захватываем локально, иначе замыкание на изменяемую переменную
                    img.MouseDown += (s, ev) =>
                    {
                        try
                        {
                            _fileService.OpenFile(relPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Не удалось открыть изображение: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    valueElement = img;

                    // Для множества фоток в рамках одного параметра
                    /*
                    var imagesPanel = new StackPanel { Orientation = Orientation.Vertical };

                    foreach (var relPath in field.FilePaths)
                    {
                        var img = new Image
                        {
                            MaxWidth = 180,
                            MaxHeight = 180,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 3, 0, 3),
                            Cursor = Cursors.Hand,
                            ToolTip = "Нажмите, чтобы открыть в системном просмотрщике"
                        };

                        try
                        {
                            var absPath = _fileService.GetAbsolutePath(relPath);
                            if (File.Exists(absPath))
                            {
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.UriSource = new Uri(absPath);
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.EndInit();
                                img.Source = bitmap;
                            }
                        }
                        catch { }

                        var capturedPath = relPath;
                        img.MouseDown += (s, ev) =>
                        {
                            try { _fileService.OpenFile(capturedPath); }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось открыть изображение: {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        };

                        imagesPanel.Children.Add(img);
                    }

                    valueElement = imagesPanel;
                    */
                }
                else if (field.Type == "file" && field.FilePaths.Count > 0)
                {
                    // Файл — кликабельная ссылка
                    var link = new TextBlock
                    {
                        Text = Path.GetFileName(field.FilePaths[0]),
                        Foreground = Brushes.Blue,
                        TextDecorations = TextDecorations.Underline,
                        Cursor = Cursors.Hand,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    link.Tag = field.FilePaths[0];
                    link.MouseDown += (s, ev) =>
                    {
                        try
                        {
                            _fileService.OpenFile(link.Tag.ToString());
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Не удалось открыть файл: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };
                    valueElement = link;
                }
                else if (field.Type == "link")
                {
                    // Ссылка на сайт
                    var link = new TextBlock
                    {
                        Text = field.Value,
                        Foreground = Brushes.Blue,
                        TextDecorations = TextDecorations.Underline,
                        Cursor = Cursors.Hand,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    link.MouseDown += (s, ev) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = field.Value,
                                UseShellExecute = true
                            });
                        }
                        catch { /* игнорируем */ }
                    };
                    valueElement = link;
                }
                else
                {
                    // Обычный текст
                    valueElement = new TextBlock
                    {
                        Text = field.Value,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                }

                Grid.SetColumn(valueElement, 1);
                rowGrid.Children.Add(valueElement);

                // Кнопка копирования
                var btnCopy = new Button
                {
                    Content = "📋",
                    ToolTip = "Скопировать значение",
                    Width = 24,
                    Height = 24,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    FontSize = 12,
                    Tag = field.Type == "file" && field.FilePaths.Count > 0
                        ? field.FilePaths[0]
                        : field.Value
                };
                btnCopy.Click += CopyToClipboard_Click;

                Grid.SetColumn(btnCopy, 2);
                rowGrid.Children.Add(btnCopy);

                ViewFieldsStack.Children.Add(rowGrid);
            }

            if (data.Parameters.Count == 0)
            {
                ViewFieldsStack.Children.Add(new TextBlock
                {
                    Text = "Дополнительных полей нет.",
                    FontStyle = FontStyles.Italic
                });
            }

            ViewInfoPanel.Visibility = Visibility.Visible;
            EditMenuPanel.Visibility = Visibility.Collapsed;
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                var text = btn.Tag.ToString();
                if (text.StartsWith("AttachedFiles"))
                {
                    text = Path.GetFileName(text);
                }
                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetText(text);
                    btn.Content = "✓";
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(600)
                    };
                    timer.Tick += (s, args) => { btn.Content = "📋"; timer.Stop(); };
                    timer.Start();
                }
            }
        }

        private void CloseViewMenu_Click(object sender, RoutedEventArgs e)
        {
            ViewInfoPanel.Visibility = Visibility.Collapsed;
        }

        // ==================== ПАНЕЛЬ РЕДАКТИРОВАНИЯ ====================
        private void AddCustomField_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewFieldName.Text) ||
                _selectedObjectData == null) return;

            _selectedObjectData.Parameters.Add(new ObjectParameter
            {
                Name = TxtNewFieldName.Text,
                Type = "text",
                Value = ""
            });
            TxtNewFieldName.Clear();
            RefreshFieldsList();
        }

        private void AddLinkField_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewFieldName.Text) ||
                _selectedObjectData == null) return;

            _selectedObjectData.Parameters.Add(new ObjectParameter
            {
                Name = TxtNewFieldName.Text,
                Type = "link",
                Value = "https://"
            });
            TxtNewFieldName.Clear();
            RefreshFieldsList();
        }

        private async void AddImageField_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewFieldName.Text) ||
                _selectedObjectData == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                var param = new ObjectParameter
                {
                    Name = TxtNewFieldName.Text,
                    Type = "image"
                };

                foreach (var file in dialog.FileNames)
                {
                    using var stream = File.OpenRead(file);
                    var relativePath = await _fileService.SaveFileAsync(
                        stream, Path.GetFileName(file), _selectedObjectData.Id);
                    param.FilePaths.Add(relativePath);
                }

                _selectedObjectData.Parameters.Add(param);
                TxtNewFieldName.Clear();
                RefreshFieldsList();
            }
        }

        private async void AddFileField_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewFieldName.Text) ||
                _selectedObjectData == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Документы (*.docx;*.doc;*.xlsx;*.xls;*.pdf;*.txt)|" +
                         "*.docx;*.doc;*.xlsx;*.xls;*.pdf;*.txt|" +
                         "Все файлы (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                var param = new ObjectParameter
                {
                    Name = TxtNewFieldName.Text,
                    Type = "file"
                };

                foreach (var file in dialog.FileNames)
                {
                    using var stream = File.OpenRead(file);
                    var relativePath = await _fileService.SaveFileAsync(
                        stream, Path.GetFileName(file), _selectedObjectData.Id);
                    param.FilePaths.Add(relativePath);
                }

                _selectedObjectData.Parameters.Add(param);
                TxtNewFieldName.Clear();
                RefreshFieldsList();
            }
        }

        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ObjectParameter param &&
                _selectedObjectData != null)
            {
                // Удаляем прикреплённые файлы
                foreach (var path in param.FilePaths)
                {
                    _fileService.DeleteFile(path);
                }
                _selectedObjectData.Parameters.Remove(param);
                RefreshFieldsList();
            }
        }

        private void TypeCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshFieldsList();
        }

        private void RefreshFieldsList()
        {
            FieldsItemsControl.ItemsSource = null;
            FieldsItemsControl.ItemsSource = _selectedObjectData?.Parameters;
        }

        /*
        private async void CloseEditMenu_Click(object sender, RoutedEventArgs e)
        {
            //EditMenuPanel.Visibility = Visibility.Collapsed;
            if (_selectedObjectData != null)
            {
                _selectedObjectData.Name = TxtObjectName.Text;

                // Обновляем или добавляем объект в общий список
                var existing = _objectsData.FirstOrDefault(o => o.Id == _selectedObjectData.Id);
                if (existing != null) _objectsData.Remove(existing);
                _objectsData.Add(_selectedObjectData);

                // Сохраняем в JSON
                await _dataService.SaveObjectsAsync(_objectsData);

                // Обновляем карту в JS — отправляем ВЕСЬ список, а не один элемент
                await _bridge.CallWithJsonAsync("setObjectData", _objectsData);
            }

            EditMenuPanel.Visibility = Visibility.Collapsed;
        }
        */
        private async void CloseEditMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedObjectData != null)
            {
                _selectedObjectData.Name = TxtObjectName.Text;

                var existing = _objectsData.FirstOrDefault(o => o.Id == _selectedObjectData.Id);
                if (existing != null) _objectsData.Remove(existing);
                _objectsData.Add(_selectedObjectData);

                _currentProject.Objects = _objectsData;
                _isProjectDirty = true;

                await _bridge.CallWithJsonAsync("setObjectData", _objectsData);
            }

            EditMenuPanel.Visibility = Visibility.Collapsed;
        }

        private void TextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            // Заглушка для совместимости
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ====================
        private class JsMessage
        {
            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("payload")]
            public JsonElement? Payload { get; set; }
        }
    }
}