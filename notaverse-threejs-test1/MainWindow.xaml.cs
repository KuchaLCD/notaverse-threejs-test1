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

            // Асинхронная инициализация WebView2
            InitializeWebViewAsync();
        }
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

        // ==================== ЗАГРУЗКА МОДЕЛИ ====================
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

        // ==================== СОХРАНЕНИЕ/ЗАГРУЗКА ДАННЫХ ====================
        private async void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _sceneConfig.Objects = _objectsData;

                // Синхронизируем камеру из JS
                var cameraJson = await WebView.CoreWebView2.ExecuteScriptAsync(
                    "JSON.stringify({" +
                    "positionX: camera.position.x," +
                    "positionY: camera.position.y," +
                    "positionZ: camera.position.z," +
                    "targetX: controls.target.x," +
                    "targetY: controls.target.y," +
                    "targetZ: controls.target.z" +
                    "})");

                // ExecuteScriptAsync возвращает JSON-строку в кавычках с экранированием
                var unescaped = JsonSerializer.Deserialize<string>(cameraJson);
                if (!string.IsNullOrEmpty(unescaped))
                {
                    var cam = JsonSerializer.Deserialize<CameraConfig>(unescaped);
                    if (cam != null) _sceneConfig.Camera = cam;
                }

                await _dataService.SaveSceneConfigAsync(_sceneConfig);

                MessageBox.Show("Данные сцены сохранены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Читаем конфиг из JSON
                _sceneConfig = await _dataService.LoadSceneConfigAsync();
                _objectsData = _sceneConfig.Objects;

                // 2. Передаём данные объектов в JS
                await _bridge.CallWithJsonAsync("setObjectData", _objectsData);

                // 3. Восстанавливаем камеру
                if (_sceneConfig.Camera != null)
                {
                    var cam = _sceneConfig.Camera;
                    await WebView.CoreWebView2.ExecuteScriptAsync(
                        $"camera.position.set({cam.PositionX}, {cam.PositionY}, {cam.PositionZ});" +
                        $"controls.target.set({cam.TargetX}, {cam.TargetY}, {cam.TargetZ});" +
                        "controls.update();");
                }

                // 4. Загружаем модель, если она была сохранена
                if (!string.IsNullOrEmpty(_sceneConfig.ModelFilePath))
                {
                    var absPath = _fileService.GetModelAbsolutePath(_sceneConfig.ModelFilePath);

                    if (File.Exists(absPath))
                    {
                        var fileNameOnly = Path.GetFileName(_sceneConfig.ModelFilePath);
                        var url = $"https://models.local/{fileNameOnly}";
                        await _bridge.CallAsync("loadModelFromUrl", url, fileNameOnly);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Файл модели не найден: {_sceneConfig.ModelFilePath}\n" +
                            "Загрузите .glb вручную через кнопку «Загрузить модель».",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show("Сцена и данные загружены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
                    // Изображение
                    var img = new Image
                    {
                        MaxWidth = 200,
                        MaxHeight = 200,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    try
                    {
                        var absPath = _fileService.GetAbsolutePath(field.FilePaths[0]);
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
                    catch { /* игнорируем */ }

                    valueElement = img;
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