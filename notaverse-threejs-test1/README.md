# Полный чек-лист правок

|#	 | Файл | Что исправил |
|----|----|----|
|3	 | MainWindow.xaml.cs → ShowViewPanel	| Добавил Cursor, ToolTip, MouseDown на Image |
|1	 | threeScene.js	| Новая функция applyRotationMode, window.setRotationMode, добавлена замена onClick на pointerdown/pointerup |
|1	| MainWindow.xaml	| ComboBox RotationModeCombo теперь в панели |
|1	| MainWindow.xaml.cs	| Новый обработчик RotationMode_Changed |
|1	| SceneConfig.cs	| Обновлено поле RotationMode |
|2	| SceneConfig.cs	| Доработаны поля ProjectId, ProjectName, CreatedAt, ModifiedAt |
|2	| DataService.cs	| Новая фича — работа с проектами |
|2	| MainWindow.xaml	| Новая панель проектов, окно OpenProjectPanel, удалены старые кнопки |
|2	| MainWindow.xaml.cs	| Добавлены и доработаны поля _currentProject, _isProjectDirty, хелперы, обработчики NewProject_Click, OpenProject_Click, ConfirmOpenProject_Click, CloseOpenProject_Click, DeleteProject_Click, SaveProject_Click |
|2	| MainWindow.xaml.cs	| InitializeWebViewAsync — сделан второй виртуальный хост, восстановление последнего проекта
|2	| MainWindow.xaml.cs	| OnNavigationCompleted — переделана загрузка модели/камеры/режима |
|2	| MainWindow.xaml.cs	| LoadModel_Click, CloseEditMenu_Click, OnClosing — обновление |
|2	| MainWindow.xaml.cs	| Удалил SaveData_Click, LoadData_Click |
|2	| threeScene.js	| Настроил экспорт window.clearModel |

Так же небольшие косметические исправления