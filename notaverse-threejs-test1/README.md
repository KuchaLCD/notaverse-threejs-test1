---
description: Это общая сборка всего текущего стека и писем из чата DeepSeek по созданию моего приложения 3д-движка на C# WPF + Three.js. Здесь будет полная история сообщений и текущая кодовая база приложения из сообщений.
---

Здесь находится полный пулл сообщений и вывода моего чата с DeepSeek по созданию моего приложения. Прочитай этот документ внимательно и ознакомся с текущими особенностями стека - это пригодится для формирования красивого README.md в моём репозитории GitHub под мой проект 3д-движка. **Очен важная оговорка!!!** Весь пулл вывода (ответов) DeepSeek был полностью перенесён в текущий код приложения - приложение полностью составлялось за счёт чата и ответов от DeepSeek! Никаких изменений без него я сам не вносил - весь стек делал с DeepSeek и ошибки тоже исправлял с ним.

# Входные данные перед первым запросом

## Наполнение HelixToolkitCodeBase.md
```
description: В чём заключается "специфическая логика просмотра" в приложении в релаизации C# .NET WPF проекта 3д-вьюера через HelixToolkit - дело в том, что это не просто 3д-вьюер для просмотра моделей, которые я загружаю в него - это полноценная система для хранения и обработки информации цифровых двойников предприятий! Что-то типо систем IoT или цифровых городов, только в масштабах пока что малых моделей с очень урезанной площадью и размером модели для большей оптимизации работы приложения. Если коротко, в моей программе существует 2 основных режима работы - просмотр (обычный просмотр объектов, в котором если нажать по объекту ЛКМ, то высветится окно о его конфигурации - название объекта, поля с дополнительной информации двух видов - обычная строка или ссылка на файл, который хранится в специальной папке, созданной приложением) и инспектор (режим редактирования) (в этом режиме на экране появляется прицел по направлению взгляда вьюпорта (камеры) приложения, и если он направлен на элемент модели (обычно составные примитивы модели - куб, треугольник и т.д.), то данный элемент подсвечивается и ему можно настроить окно, которое можно будет просмотреть в режиме просмотра - конфигурации названия объекта, полей с дополнительной информацией двух видов - обычная строка или ссылка на файл). Отдельно есть режимы навигации по модели, похожие на навигацию в blender - по умолчанию ([Shift + СКМ] - Сдвиг сцены, [Ctrl + F] - возврат на исходную позицию, [СКМ] - Вращени) и режим полёта ([Shift + ~] - Включить/Выключить режим полета, Q (строгое перемещение вниз) и E (строгое перемещение вверх), W (вперёд по направлению взгляда), A (влево по направлению взгляда), S (назад по направлению взгляда), D (вправо по направлению взгляда)). Данная реализация оказалась самой надёжной и рабочей, но запертой в Windows системе
```

### Кодовая база C# .NET WPF проекта 3д-вьюера 

#### Структура проекта

```
WpfApp1Unity
|--Зависимости
|--assets
|   |--logo.png
|--App.xaml
|   |--App.xaml.cs
|--AssemlyInfo.cs
|--DataModel.cs
|--MainWindow.xaml
|   |--MainWindow.xaml.cs
```

### Наполнение класса-модели данных DataModel.cs

```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp1Unity
{
    public class DataModel
    {
        public class CubeField
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool IsFileLink { get; set; } // Флаг: является ли поле ссылкой на локальный файл
        }

        public class ModelData
        {
            public string Id { get; set; }        // Уникальный ID 3D-модели на сцене
            public string Name { get; set; }      // Имя для вывода в карточке
            public string FilePath { get; set; }  // Путь к файлу .obj (чтобы загрузить при старте)
            public List<CubeField> DynamicFields { get; set; } = new List<CubeField>();
        }

        public class CameraConfig
        {
            public double Radius { get; set; }
            public double Theta { get; set; }
            public double Phi { get; set; }

            // Координаты вектора смещения центра вращения (Shift + СКМ)
            public double TargetOffsetX { get; set; }
            public double TargetOffsetY { get; set; }
            public double TargetOffsetZ { get; set; }

            // Направление взгляда и позиция для точного восстановления из Fly Mode
            public double PosX { get; set; }
            public double PosY { get; set; }
            public double PosZ { get; set; }
            public double LookX { get; set; }
            public double LookY { get; set; }
            public double LookZ { get; set; }
        }

        public class SceneConfig
        {
            public CameraConfig Camera { get; set; } = new CameraConfig();
            public List<ModelData> Models { get; set; } = new List<ModelData>();
        }
    }
}
```
### Наполнение страницы MainWindow.xaml

```xml
<Window x:Class="WpfApp1Unity.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:WpfApp1Unity"
        mc:Ignorable="d"
        Title="3D Viewer - NotaVerseBIM alpha 0.0.15" Height="450" Width="800" Icon="/assets/notaverseLogo.png">
    <Grid>
        <!-- 3D Сцена -->
        <Viewport3D Name="MainViewport" 
                    MouseDown="Viewport_MouseDown" 
                    MouseUp="Viewport_MouseUp" 
                    MouseMove="Viewport_MouseMove" 
                    MouseWheel="Viewport_MouseWheel">
            <Viewport3D.Camera>
                <PerspectiveCamera x:Name="TheCamera" Position="2,2,3" LookDirection="-2,-2,-3" UpDirection="0,1,0" FieldOfView="45" />
            </Viewport3D.Camera>

            <Viewport3D.Children>
                <ModelVisual3D>
                    <ModelVisual3D.Content>
                        <AmbientLight Color="White" />
                    </ModelVisual3D.Content>
                </ModelVisual3D>

                <!-- Оранжевая точка (метка), изначально скрыта (масштаб 0) -->
                <ModelVisual3D x:Name="MarkSphereVisual">
                    <ModelVisual3D.Transform>
                        <ScaleTransform3D x:Name="MarkScale" ScaleX="0" ScaleY="0" ScaleZ="0"/>
                    </ModelVisual3D.Transform>
                    <ModelVisual3D.Content>
                        <!-- Упрощенная геометрия маленького маркера в центре куба -->
                        <GeometryModel3D>
                            <GeometryModel3D.Geometry>
                                <MeshGeometry3D Positions="-0.08,-0.08,0.08 0.08,-0.08,0.08 0.08,0.08,0.08 -0.08,0.08,0.08 -0.08,-0.08,-0.08 0.08,-0.08,-0.08 0.08,0.08,-0.08 -0.08,0.08,-0.08"
                                                TriangleIndices="0,1,2 2,3,0  1,5,6 6,2,1  5,4,7 7,6,5  4,0,3 3,7,4  3,2,6 6,7,3  4,5,1 1,0,4"/>
                            </GeometryModel3D.Geometry>
                            <GeometryModel3D.Material>
                                <DiffuseMaterial Brush="Orange" />
                            </GeometryModel3D.Material>
                        </GeometryModel3D>
                    </ModelVisual3D.Content>
                </ModelVisual3D>
            </Viewport3D.Children>
        </Viewport3D>

        <!-- Оранжевый прицел по центру (имитация луча из центра вьюпорта) -->
        <Canvas x:Name="CenterTarget" IsHitTestVisible="False" Visibility="Collapsed">
            <Ellipse Width="10" Height="10" Fill="Orange" Opacity="0.7"/>
            <!-- Тонкое перекрестие луча -->
            <Line X1="0" Y1="250" X2="800" Y2="250" Stroke="Orange" StrokeDashArray="4 4" Opacity="0.3" x:Name="HorizontalLine"/>
            <Line X1="400" Y1="0" X2="400" Y2="600" Stroke="Orange" StrokeDashArray="4 4" Opacity="0.3" x:Name="VerticalLine"/>
        </Canvas>

        <!-- Верхняя панель управления режимами -->
        <Grid VerticalAlignment="Top" Background="#C0000000">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>

            <StackPanel Orientation="Vertical" Grid.Column="0">
                <StackPanel Orientation="Horizontal">
                    <RadioButton x:Name="RadioView" Content="👁 Просмотр (🎮: СКМ)" IsChecked="True" Foreground="White" Checked="Mode_Changed" Margin="0,0,20,0"/>
                    <RadioButton x:Name="RadioEdit" Content="✎ Инспектор (🎮: Прицел + ПКМ)" Foreground="White" Checked="Mode_Changed" Margin="0,0,20,0"/>
                </StackPanel>
                <Expander Header="🎮 Подсказки" Foreground="AntiqueWhite" Margin="0,5,0,0">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="*" />
                            <RowDefinition Height="Auto" />
                            <RowDefinition Height="Auto" />
                            <RowDefinition Height="Auto" />
                            <RowDefinition Height="Auto" />
                        </Grid.RowDefinitions>
                        <StackPanel Grid.Row="0" Orientation="Vertical" Margin="5,0,0,0">
                            <TextBlock Text="🕹️ Навигация: [Shift + СКМ] - Сдвиг сцены | [Ctrl + F] - возврат на исходную позицию | [Shift + ~] (WASDQE + Shift для турбо)" Foreground="Violet" FontSize="11" FontWeight="Bold" Margin="5,0,0,0"/>
                            <TextBlock Text="[СКМ] - Вращение | 👁 [ЛКМ по объекту] - Вызов окна | ✎ [ПКМ по объекту] - Редактирование окна" Foreground="Violet" FontSize="11" FontWeight="Bold" Margin="5,0,0,0"/>
                        </StackPanel>
                        <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="5,0,0,0">
                            <TextBlock Text="[Shift + ~] - Включить/Выключить режим полета" Foreground="Yellow" VerticalAlignment="Center" FontWeight="Bold" FontSize="10"/>
                            <TextBlock Text="Q (перемещение вниз) и E (перемещение вверх)" Foreground="Yellow" VerticalAlignment="Center" FontWeight="Bold" FontSize="10" Margin="10,0,0,0"/>
                        </StackPanel>
                        <StackPanel Grid.Row="2" Orientation="Horizontal" Margin="5,0,0,0">
                            <TextBlock Text="W (вперёд по направлению взгляда)" Foreground="Yellow" VerticalAlignment="Center" FontWeight="Bold" FontSize="10"/>
                            <TextBlock Text="A (влево по направлению взгляда)" Foreground="Yellow" VerticalAlignment="Center" FontWeight="Bold" FontSize="10" Margin="10,0,0,0"/>
                        </StackPanel>
                        <StackPanel Grid.Row="3" Orientation="Horizontal" Margin="5,0,0,0">
                            <TextBlock Text="S (назад по направлению взгляда)" Foreground="Yellow" VerticalAlignment="Center" FontWeight="Bold" FontSize="10"/>
                            <TextBlock Text="D (вправо по направлению взгляда)" Foreground="Yellow" VerticalAlignment="Center" FontWeight="Bold" FontSize="10" Margin="10,0,0,0"/>
                        </StackPanel>
                        <TextBlock Grid.Row="4" Text="ОЧЕНЬ ВАЖНО! Во время навигации старайтесь держать курсор мыши на объекте!" Foreground="Red" VerticalAlignment="Center" FontWeight="Bold" FontSize="10" Margin="5,0,0,0"/>
                    </Grid>
                </Expander>
                
            </StackPanel>

            <!-- Кнопки сохранения файлов -->
            <StackPanel Orientation="Horizontal" Grid.Column="1">
                <Button Content="Импортировать .OBJ" Click="ImportModel_Click" Margin="0,0,10,0" Padding="10,2" Background="#FF8E44AD" Foreground="White" BorderThickness="0" FontWeight="Bold"/>
                <Button Content="Сохранить данные" Click="SaveData_Click" Margin="0,0,10,0" Padding="10,2" Background="#FF27AE60" Foreground="White" BorderThickness="0"/>
                <Button Content="Загрузить данные" Click="LoadData_Click" Padding="10,2" Background="#FF2980B9" Foreground="White" BorderThickness="0"/>
            </StackPanel>
        </Grid>

        <!-- Подсказка по навигации полета (показывается только при активном полете) -->
        <Border x:Name="FlyNavigationHelp" Visibility="Collapsed" Background="#E0000000" CornerRadius="5" Padding="10" HorizontalAlignment="Left" VerticalAlignment="Top" Margin="10,50,0,0" Width="220">
            <StackPanel>
                <TextBlock Text="🚀 РЕЖИМ ПОЛЕТА АКТИВЕН" Foreground="Cyan" FontWeight="Bold" Margin="0,0,0,5"/>
                <TextBlock Text="• Мышь: Поворот камеры" Foreground="White"/>
                <TextBlock Text="• W / S: Вперед / Назад" Foreground="White"/>
                <TextBlock Text="• A / D: Смещение Влево / Вправо" Foreground="White"/>
                <TextBlock Text="• E / Q: Вверх / Вниз" Foreground="White"/>
                <TextBlock Text="• Shift + ~ : Выйти из полета" Foreground="Yellow" Margin="0,5,0,0" FontWeight="Bold"/>
            </StackPanel>
        </Border>

        <!-- ОКНО РЕДАКТИРОВАНИЯ ИНФОРМАЦИИ (По центру/Сбоку) -->
        <Border x:Name="EditMenuPanel" Visibility="Collapsed" Background="White" BorderBrush="Orange" BorderThickness="2" CornerRadius="5" Width="300" HorizontalAlignment="Right" VerticalAlignment="Center" Margin="20" Padding="15">
            <StackPanel>
                <TextBlock Text="Настройка аннотации" FontSize="16" FontWeight="Bold" Margin="0,0,0,10"/>
                <TextBlock Text="Название объекта:"/>
                <TextBox x:Name="TxtCubeName" Margin="0,0,0,10"/>

                <TextBlock Text="Дополнительные поля:" FontWeight="Bold" Margin="0,5,0,5"/>

                <ItemsControl x:Name="FieldsItemsControl">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Grid Margin="0,4">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="100"/>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="30"/>
                                </Grid.ColumnDefinitions>

                                <TextBox Grid.Column="0" Text="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" GotFocus="TextBox_GetFocus" Margin="0,0,5,0" Background="#FFF5F5F5" FontWeight="Bold"/>

                                <!-- Значение поля с триггером на тип (файл/текст) -->
                                <TextBox x:Name="ValueBox" Grid.Column="1" Text="{Binding Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" GotFocus="TextBox_GetFocus"/>

                                <Button Grid.Column="2" Content="×" Click="DeleteField_Click" Tag="{Binding}" Margin="5,0,0,0" Background="#FFE74C3C" Foreground="White" BorderThickness="0" FontWeight="Bold" Cursor="Hand"/>
                            </Grid>
                            <DataTemplate.Triggers>
                                <!-- Если это ссылка на файл, делаем её поле слегка фиолетовым и отключаем ручной ввод, чтобы путь не сломали -->
                                <DataTrigger Binding="{Binding IsFileLink}" Value="True">
                                    <Setter TargetName="ValueBox" Property="Background" Value="#FFF3E5F5"/>
                                    <Setter TargetName="ValueBox" Property="IsReadOnly" Value="True"/>
                                    <Setter TargetName="ValueBox" Property="ToolTip" Value="Кликните дважды в режиме просмотра, чтобы открыть файл"/>
                                </DataTrigger>
                            </DataTemplate.Triggers>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>

                <!-- Конструктор добавления новых полей -->
                <Border BorderBrush="Gray" BorderThickness="0,1,0,0" Margin="0,10,0,0" Padding="0,5,0,0">
                    <StackPanel>
                        <TextBox x:Name="TxtNewFieldName" ToolTip="Имя нового поля (напр. Вес, ID)"/>
                        <Grid>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="*"/>
                                <RowDefinition Height="*"/>
                            </Grid.RowDefinitions>
                            <Grid Grid.Row="0" Margin="0,5,0,10">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                <Button Content="+ Текст" Click="AddCustomField_Click" Grid.Column="0" Margin="0,0,2,0"/>
                                <Button Content="+ Файл📎" Click="AddFileField_Click" Grid.Column="1" Margin="2,0,0,0" Background="#FF9B59B6" Foreground="White" BorderThickness="0"/>
                            </Grid>
                            <TextBlock Grid.Row="1" Text="🛈 ВАЖНО! Прежде чем добавить файл, полю с файлом нужно указать ИМЯ!" Foreground="Fuchsia" VerticalAlignment="Center" FontWeight="Bold" FontSize="10" TextWrapping="Wrap" Margin="0,5,0,5"/>
                            </Grid>
                    </StackPanel>
                </Border>

                <Button Content="Сохранить и Закрыть" Click="CloseEditMenu_Click" Background="Orange" Foreground="White" FontWeight="Bold" Height="30"/>
            </StackPanel>
        </Border>

        <!-- ОКНО ПРОСМОТРА ИНФОРМАЦИИ (Вызывается по клику на оранжевую точку) -->
        <Border x:Name="ViewInfoPanel" Visibility="Collapsed" Background="#F0F4F8" BorderBrush="SteelBlue" BorderThickness="2" CornerRadius="5" Width="250" HorizontalAlignment="Left" VerticalAlignment="Center" Margin="20" Padding="15">
            <StackPanel>
                <TextBlock x:Name="LblCubeTitle" Text="Информация" FontSize="16" FontWeight="Bold" Foreground="SteelBlue" Margin="0,0,0,10"/>
                <StackPanel x:Name="ViewFieldsStack"/>
                <Button Content="Закрыть" Click="CloseViewMenu_Click" Margin="0,15,0,0"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```
### Наполнение класса страницы MainWindow.xaml.cs

```csharp
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfApp1Unity.DataModel;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using HelixToolkit.Wpf; // Обязательно добавьте этот using в самый верх файла
using Microsoft.Win32;  // Для OpenFileDialog

namespace WpfApp1Unity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Камера и навигация
        private double _radius = 4.0;
        private double _theta = 0.8;
        private double _phi = 1.0;
        private Point _lastMousePosition;
        private bool _isOrbiting = false;
        private bool _isPanning = false;
        private Vector3D _targetOffset = new Vector3D(0, 0, 0);

        // Режим полета
        private bool _isFlyMode = false;
        private Point _flyCenterPoint;
        private readonly HashSet<Key> _pressedKeys = new HashSet<Key>();
        private const double BaseFlySpeed = 0.01;
        private const double BoostFlySpeed = 0.08;

        // База данных моделей сцены и подсветка
        private List<ModelData> _sceneModels = new List<ModelData>();
        private ModelData _selectedModelData = null;
        private ModelVisual3D _hoveredVisual = null;
        private MaterialGroup _originalMaterialGroup = null;
        private bool _isHoveringCube = false;
        private const string SaveFileName = "cube_data.json";

        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            this.LostFocus += (s, e) => _pressedKeys.Clear();

            // Создаем стартовый куб программно, чтобы его можно было легко удалить
            CreateDefaultStartCube();
            ResetCameraToDefault();

            this.SizeChanged += (s, e) => {
                HorizontalLine.X2 = this.ActualWidth;
                HorizontalLine.Y1 = HorizontalLine.Y2 = this.ActualHeight / 2 - 20;
                VerticalLine.Y2 = this.ActualHeight;
                VerticalLine.X1 = VerticalLine.X2 = this.ActualWidth / 2 - 10;
            };

            CompositionTarget.Rendering += OnRenderFrame;
        }

        private bool IsEditMode => RadioEdit?.IsChecked == true;

        // Создание куба по умолчанию при старте
        private void CreateDefaultStartCube()
        {
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = Point3DCollection.Parse("-0.5,-0.5,0.5 0.5,-0.5,0.5 0.5,0.5,0.5 -0.5,0.5,0.5 -0.5,-0.5,-0.5 0.5,-0.5,-0.5 0.5,0.5,-0.5 -0.5,0.5,-0.5"),
                TriangleIndices = Int32Collection.Parse("0,1,2 2,3,0  1,5,6 6,2,1  5,4,7 7,6,5  4,0,3 3,7,4  3,2,6 6,7,3  4,5,1 1,0,4")
            };

            GeometryModel3D model3D = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.SteelBlue));
            Model3DGroup group = new Model3DGroup();
            group.Children.Add(model3D);

            ModelVisual3D visual = new ModelVisual3D { Content = group };
            string startCubeId = "default_start_cube";
            visual.SetName(startCubeId);

            _sceneModels.Add(new ModelData { Id = startCubeId, Name = "Стартовый Куб", FilePath = "" });
            MainViewport.Children.Add(visual);
        }

        private void ResetCameraToDefault()
        {
            _targetOffset = new Vector3D(0, 0, 0);
            _radius = 4.0;
            _theta = 0.8;
            _phi = 1.0;
            UpdateCameraPosition();
        }

        private void ClearSceneFromModels()
        {
            List<ModelVisual3D> toRemove = new List<ModelVisual3D>();
            foreach (var child in MainViewport.Children)
            {
                if (child != MarkSphereVisual && child is ModelVisual3D mv && mv.Content is Model3DGroup)
                    toRemove.Add(mv);
            }
            foreach (var rm in toRemove) MainViewport.Children.Remove(rm);
            _sceneModels.Clear();
        }

        // ПОШАГОВЫЙ ИМПОРТ И РАЗДЕЛЕНИЕ МОДЕЛЕЙ ИЗ BLENDER
        private void ImportModel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Wavefront OBJ (*.obj)|*.obj" };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Очищаем сцену от стартового куба или прошлых моделей
                    ResetHoveredMaterial();
                    ClearSceneFromModels();

                    // 2. Читаем .obj файл
                    ObjReader reader = new ObjReader();
                    Model3DGroup fullImportedGroup = reader.Read(openFileDialog.FileName);

                    // 3. Разделяем общую группу на отдельные независимые объекты
                    ProcessAndAddSubModels(fullImportedGroup, openFileDialog.FileName);

                    MessageBox.Show($"Сцена успешно импортирована! Разделено объектов: {_sceneModels.Count}", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка");
                }
            }
        }

        // Рекурсивный обход импортированной геометрии для выделения независимых объектов
        private void ProcessAndAddSubModels(Model3DGroup group, string fullPath, string parentName = "")
        {
            int objectCounter = 1;
            foreach (var child in group.Children)
            {
                if (child is GeometryModel3D geometryModel)
                {
                    // Создаем отдельный визуальный контейнер для ОДНОГО меша из Blender
                    Model3DGroup singleGroup = new Model3DGroup();
                    singleGroup.Children.Add(geometryModel);

                    ModelVisual3D visual = new ModelVisual3D { Content = singleGroup };
                    string uniqueId = Guid.NewGuid().ToString();
                    visual.SetName(uniqueId);

                    // Формируем понятное имя объекта
                    string objName = !string.IsNullOrEmpty(parentName)
                        ? $"{parentName}_Part_{objectCounter}"
                        : $"Object_{Guid.NewGuid().ToString().Substring(0, 4)}";

                    ModelData subModelData = new ModelData
                    {
                        Id = uniqueId,
                        Name = objName,
                        FilePath = fullPath // Путь к общему файлу для восстановления сцены
                    };

                    _sceneModels.Add(subModelData);
                    MainViewport.Children.Add(visual);
                    objectCounter++;
                }
                else if (child is Model3DGroup subGroup)
                {
                    // Если это вложенная группа (например, составной объект), вытаскиваем меши оттуда
                    string groupName = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                    ProcessAndAddSubModels(subGroup, fullPath, groupName);
                }
            }
        }

        // ТРАССИРОВКА ЛУЧА (RAYCAST) ПО ОТДЕЛЬНЫМ ОБЪЕКТАМ
        private void PerformRaycastFromCenter()
        {
            Point centerPoint = new Point(MainViewport.ActualWidth / 2, MainViewport.ActualHeight / 2);
            bool hitSomething = false;
            ModelVisual3D foundVisual = null;

            VisualTreeHelper.HitTest(MainViewport, null,
                new HitTestResultCallback(Result =>
                {
                    RayMeshGeometry3DHitTestResult meshResult = Result as RayMeshGeometry3DHitTestResult;
                    if (meshResult != null)
                    {
                        DependencyObject parent = meshResult.VisualHit;
                        while (parent != null && !(parent is ModelVisual3D))
                        {
                            parent = VisualTreeHelper.GetParent(parent);
                        }

                        if (parent is ModelVisual3D visual && visual != MarkSphereVisual && visual.Content is Model3DGroup)
                        {
                            foundVisual = visual;
                            hitSomething = true;
                            return HitTestResultBehavior.Stop;
                        }
                    }
                    return HitTestResultBehavior.Continue;
                }),
                new PointHitTestParameters(centerPoint));

            if (hitSomething && foundVisual != null)
            {
                if (_hoveredVisual != foundVisual)
                {
                    ResetHoveredMaterial();
                    _hoveredVisual = foundVisual;
                    ApplyHighlightMaterial(_hoveredVisual);
                }
                _isHoveringCube = true;
            }
            else
            {
                ResetHoveredMaterial();
                _hoveredVisual = null;
                _isHoveringCube = false;
            }
        }

        private void ApplyHighlightMaterial(ModelVisual3D visual)
        {
            if (visual?.Content is Model3DGroup group)
            {
                var highlight = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(120, 255, 165, 0)));
                ApplyMaterialToGroup(group, highlight);
            }
        }

        private void ApplyMaterialToGroup(Model3DGroup group, Material highlightMaterial)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryModel3D mesh)
                {
                    var newGroup = new MaterialGroup();
                    if (mesh.Material != null) newGroup.Children.Add(mesh.Material);
                    newGroup.Children.Add(highlightMaterial);

                    if (_originalMaterialGroup == null && mesh.Material is MaterialGroup mg) _originalMaterialGroup = mg;
                    else if (_originalMaterialGroup == null)
                    { _originalMaterialGroup = new MaterialGroup(); _originalMaterialGroup.Children.Add(mesh.Material); }
                    mesh.Material = newGroup;
                }
            }
        }
        private void ResetHoveredMaterial() { if (_hoveredVisual?.Content is Model3DGroup group) { ResetMaterialForGroup(group); } _originalMaterialGroup = null; }
        private void ResetMaterialForGroup(Model3DGroup group)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryModel3D mesh)
                {
                    if (mesh.Material is MaterialGroup mg && mg.Children.Count > 1)
                    {
                        mesh.Material = mg.Children[0]; 
                        // Возвращаем исходный материал
                    }
                }
            }
        }
        // НАЖАТИЕ КНОПОК МЫШИ (ПКМ / ЛКМ ПО ОТДЕЛЬНЫМ ОБЪЕКТАМ)
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isFlyMode) return;_lastMousePosition = e.GetPosition(this);
            if (e.ChangedButton == MouseButton.Middle)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) _isPanning = true;
                else _isOrbiting = true;
                MainViewport.CaptureMouse();
            }
            // ПКМ — Открытие меню редактирования НАВЕДЕННОГО объекта
            else if (e.ChangedButton == MouseButton.Right && IsEditMode && _isHoveringCube && _hoveredVisual != null)
            {
                string visualId = _hoveredVisual.GetName();
                _selectedModelData = _sceneModels.Find(m => m.Id == visualId);
                if (_selectedModelData != null)
                {
                    TxtCubeName.Text = _selectedModelData.Name;
                    FieldsItemsControl.ItemsSource = null;
                    FieldsItemsControl.ItemsSource = _selectedModelData.DynamicFields;
                    EditMenuPanel.Visibility = Visibility.Visible;
                    ViewInfoPanel.Visibility = Visibility.Collapsed;
                }
            }
            // ЛКМ — Просмотр карточки КЛИКНУТОГО объекта
            else if (e.ChangedButton == MouseButton.Left && !IsEditMode)
            {
                Point mousePos = e.GetPosition(MainViewport);
                ModelVisual3D clickedVisual = null;
                VisualTreeHelper.HitTest(MainViewport, null,new HitTestResultCallback(Result =>{RayMeshGeometry3DHitTestResult meshResult = Result as RayMeshGeometry3DHitTestResult;
                    if (meshResult != null)
                    {
                        DependencyObject parent = meshResult.VisualHit;
                        while (parent != null && !(parent is ModelVisual3D)) parent = VisualTreeHelper.GetParent(parent);
                        if (parent is ModelVisual3D visual && visual != MarkSphereVisual && visual.Content is Model3DGroup)
                        {
                            clickedVisual = visual;
                            return HitTestResultBehavior.Stop;
                        }
                    }
                    return HitTestResultBehavior.Continue;
                }),new PointHitTestParameters(mousePos));
                if (clickedVisual != null)
                {
                    string visualId = clickedVisual.GetName();
                    ModelData clickedModelData = _sceneModels.Find(m => m.Id == visualId);
                    if (clickedModelData != null) ShowCubeInfoPanel(clickedModelData);
                }
            }
        }
        // ОСТАЛЬНАЯ ЛОГИКА КАМЕРЫ И ИНТЕРФЕЙСА (БЕЗ ИЗМЕНЕНИЙ)
        private void UpdateCameraPosition()
        {
            if (_isFlyMode) return;
            _phi = Math.Clamp(_phi, 0.1, Math.PI - 0.1);
            if (_radius < 0.5) _radius = 0.5;
            double x = _targetOffset.X + _radius * Math.Sin(_phi) * Math.Sin(_theta);
            double y = _targetOffset.Y + _radius * Math.Cos(_phi);
            double z = _targetOffset.Z + _radius * Math.Sin(_phi) * Math.Cos(_theta);
            TheCamera.Position = new Point3D(x, y, z);
            TheCamera.LookDirection = new Vector3D(_targetOffset.X - x, _targetOffset.Y - y, _targetOffset.Z - z);
            if (IsEditMode) PerformRaycastFromCenter();
        }
        private void OnRenderFrame(object sender, EventArgs e)
        {
            if (!_isFlyMode) return;
            Vector3D lookDir = TheCamera.LookDirection; 
            lookDir.Normalize();
            Vector3D rightDir = Vector3D.CrossProduct(lookDir, new Vector3D(0, 1, 0)); 
            rightDir.Normalize();
            Vector3D upDir = new Vector3D(0, 1, 0);
            Point3D currentPos = TheCamera.Position;
            double currentSpeed = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) ? BoostFlySpeed : BaseFlySpeed;
            if (_pressedKeys.Contains(Key.W)) currentPos += lookDir * currentSpeed;
            if (_pressedKeys.Contains(Key.S)) currentPos -= lookDir * currentSpeed;
            if (_pressedKeys.Contains(Key.A)) currentPos -= rightDir * currentSpeed;
            if (_pressedKeys.Contains(Key.D)) currentPos += rightDir * currentSpeed;
            if (_pressedKeys.Contains(Key.E)) currentPos += upDir * currentSpeed;
            if (_pressedKeys.Contains(Key.Q)) currentPos -= upDir * currentSpeed;
            TheCamera.Position = currentPos;
            if (IsEditMode) PerformRaycastFromCenter();
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Вход/выход из полета (Shift + ~)
            if (e.Key == Key.OemTilde && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                ToggleFlyMode();
                e.Handled = true;
                return;
            }

            // ИСПРАВЛЕНО: Быстрый возврат камеры к объектам теперь по нажатию Ctrl + F
            if (!_isFlyMode && e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ResetCameraToDefault();
                e.Handled = true; // Сообщаем WPF, что событие обработано
                return;
            }

            // Если режим полета активен, собираем зажатые клавиши навигации (WASDQE)
            if (_isFlyMode)
            {
                _pressedKeys.Add(e.Key);
            }
        }
        private void MainWindow_KeyUp(object sender, KeyEventArgs e) 
        { 
            if (_isFlyMode) _pressedKeys.Remove(e.Key); 
        }
        private void ToggleFlyMode()
        {
            _isFlyMode = !_isFlyMode; 
            _pressedKeys.Clear();
            if (_isFlyMode)
            {
                FlyNavigationHelp.Visibility = Visibility.Visible;
                _flyCenterPoint = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
                Mouse.OverrideCursor = Cursors.None; 
                MainViewport.CaptureMouse();
            }
            else
            {
                FlyNavigationHelp.Visibility = Visibility.Collapsed; 
                Mouse.OverrideCursor = null; 
                MainViewport.ReleaseMouseCapture();
                Vector3D look = TheCamera.LookDirection; 
                _radius = 4.0; look.Normalize();
                Point3D targetPoint = TheCamera.Position + (look * _radius); 
                _targetOffset = (Vector3D)targetPoint;
                _theta = Math.Atan2(-look.X, -look.Z); 
                _phi = Math.Acos(-look.Y); 
                UpdateCameraPosition();
            }
        }
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(this);
            if (_isFlyMode)
            {
                double dx = currentPosition.X - _flyCenterPoint.X; 
                double dy = currentPosition.Y - _flyCenterPoint.Y;
                if (dx != 0 || dy != 0){double sensitivity = 0.2; 
                    Vector3D look = TheCamera.LookDirection;
                    Matrix3D rotationMatrix = new Matrix3D();
                    rotationMatrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), -dx * sensitivity));
                    Vector3D right = Vector3D.CrossProduct(look, new Vector3D(0, 1, 0));
                    if (right.Length > 0.01) { right.Normalize(); 
                        rotationMatrix.Rotate(new Quaternion(right, -dy * sensitivity)); 
                    }
                    TheCamera.LookDirection = rotationMatrix.Transform(look);
                    Point screenCenter = this.PointToScreen(_flyCenterPoint); 
                    NativeMethods.SetCursorPos((int)screenCenter.X, (int)screenCenter.Y);
                }
            }
            else if (_isPanning && e.MiddleButton == MouseButtonState.Pressed)
            {
                double dx = currentPosition.X - _lastMousePosition.X; 
                double dy = currentPosition.Y - _lastMousePosition.Y;
                double panSpeed = 0.005 * _radius;
                Vector3D lookDir = TheCamera.LookDirection; 
                lookDir.Normalize();
                Vector3D rightDir = Vector3D.CrossProduct(lookDir, new Vector3D(0, 1, 0)); 
                rightDir.Normalize();
                Vector3D upDir = Vector3Class.CrossProduct(rightDir, lookDir); 
                upDir.Normalize();_targetOffset -= rightDir * dx * panSpeed; _targetOffset += upDir * dy * panSpeed;UpdateCameraPosition();
            }
            else if (_isOrbiting && e.MiddleButton == MouseButtonState.Pressed)
            {
                double dx = currentPosition.X - _lastMousePosition.X; 
                double dy = currentPosition.Y - _lastMousePosition.Y;
                _theta -= dx * 0.005; _phi -= dy * 0.005; UpdateCameraPosition();
            }
            _lastMousePosition = currentPosition;
            if (IsEditMode && !_isFlyMode) PerformRaycastFromCenter();
        }
        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e) 
        { 
            if (e.ChangedButton == MouseButton.Middle) 
            { 
                _isOrbiting = false; 
                _isPanning = false; 
                MainViewport.ReleaseMouseCapture(); 
            } 
        }
        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e) 
        { 
            if (_isFlyMode) return; 
            if (Keyboard.Modifiers == ModifierKeys.Control) 
            { 
                if (e.Delta > 0) _radius -= 0.3; 
                else _radius += 0.3; 
                UpdateCameraPosition(); 
                e.Handled = true; 
            } 
        }
        private void Mode_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized || CenterTarget == null || MarkScale == null) return;
            if (IsEditMode) 
            { 
                CenterTarget.Visibility = Visibility.Visible; 
                MarkScale.ScaleX = MarkScale.ScaleY = MarkScale.ScaleZ = 0; 
                ViewInfoPanel.Visibility = Visibility.Collapsed; 
            }
            else 
            { 
                CenterTarget.Visibility = Visibility.Collapsed; 
                EditMenuPanel.Visibility = Visibility.Collapsed; 
                MarkScale.ScaleX = MarkScale.ScaleY = MarkScale.ScaleZ = 1; 
                ResetHoveredMaterial(); 
                _hoveredVisual = null; 
                _isHoveringCube = false; 
            }
        }
        // СЕРИАЛИЗАЦИЯ И ВОССТАНОВЛЕНИЕ СЦЕНЫ
        private void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем общую структуру сцены
                SceneConfig sceneConfig = new SceneConfig
                {
                    Models = _sceneModels,
                    Camera = new CameraConfig
                    {
                        Radius = _radius,
                        Theta = _theta,
                        Phi = _phi,
                        TargetOffsetX = _targetOffset.X,
                        TargetOffsetY = _targetOffset.Y,
                        TargetOffsetZ = _targetOffset.Z,
                        PosX = TheCamera.Position.X,
                        PosY = TheCamera.Position.Y,
                        PosZ = TheCamera.Position.Z,
                        LookX = TheCamera.LookDirection.X,
                        LookY = TheCamera.LookDirection.Y,
                        LookZ = TheCamera.LookDirection.Z
                    }
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(sceneConfig, options);
                File.WriteAllText(SaveFileName, jsonString);

                MessageBox.Show("Конфигурация сцены и положение камеры успешно сохранены!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении проекта: {ex.Message}", "Ошибка");
            }
        }
        private void LoadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(SaveFileName))
                {
                    ResetHoveredMaterial();
                    ClearSceneFromModels();

                    // 1. Читаем JSON в новую структуру-обертку
                    string jsonString = File.ReadAllText(SaveFileName);
                    SceneConfig sceneConfig = JsonSerializer.Deserialize<SceneConfig>(jsonString);

                    if (sceneConfig == null)
                    {
                        MessageBox.Show("Не удалось прочитать файл сохранения.", "Ошибка");
                        return;
                    }

                    // 2. Восстанавливаем список моделей данных
                    _sceneModels = sceneConfig.Models ?? new List<ModelData>();

                    // 3. Физически пересоздаем 3D-объекты на сцене
                    ObjReader reader = new ObjReader();
                    Dictionary<string, Model3DGroup> cache = new Dictionary<string, Model3DGroup>();
                    List<ModelData> restoredModelsData = new List<ModelData>();

                    foreach (var modelData in _sceneModels)
                    {
                        if (string.IsNullOrEmpty(modelData.FilePath)) continue;

                        if (File.Exists(modelData.FilePath))
                        {
                            if (!cache.ContainsKey(modelData.FilePath))
                            {
                                cache[modelData.FilePath] = reader.Read(modelData.FilePath);
                            }

                            Model3DGroup fullModelGroup = cache[modelData.FilePath];
                            ModelVisual3D generatedVisual = FindAndCreateVisualFromGroup(fullModelGroup, restoredModelsData.Count + 1, modelData.Id);

                            if (generatedVisual != null)
                            {
                                MainViewport.Children.Add(generatedVisual);
                                restoredModelsData.Add(modelData);
                            }
                        }
                    }
                    _sceneModels = restoredModelsData;

                    // 4. ВОССТАНОВЛЕНИЕ ПОЛОЖЕНИЯ ВЬЮПОРТА (КАМЕРЫ)
                    if (sceneConfig.Camera != null)
                    {
                        var cam = sceneConfig.Camera;

                        // Восстанавливаем внутренние углы и смещение для орбитального режима
                        _radius = cam.Radius;
                        _theta = cam.Theta;
                        _phi = cam.Phi;
                        _targetOffset = new Vector3D(cam.TargetOffsetX, cam.TargetOffsetY, cam.TargetOffsetZ);

                        // Жестко выставляем позицию и направление камеры в пространстве
                        TheCamera.Position = new Point3D(cam.PosX, cam.PosY, cam.PosZ);
                        TheCamera.LookDirection = new Vector3D(cam.LookX, cam.LookY, cam.LookZ);
                    }
                    else
                    {
                        // Если в старых сохранениях не было камеры — сбрасываем в дефолт
                        ResetCameraToDefault();
                    }

                    // Запускаем перерасчет и луч (если мы в режиме редактирования)
                    if (IsEditMode) PerformRaycastFromCenter();

                    MessageBox.Show($"Сцена и положение вьюпорта успешно восстановлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Файл сохранения cube_data.json не найден.", "Внимание");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении сцены: {ex.Message}", "Ошибка");
            }
        }

        // Вспомогательный метод для извлечения конкретного меша по его порядковому номеру при восстановлении
        private ModelVisual3D FindAndCreateVisualFromGroup(Model3DGroup group, int targetIndex, string originalId)
        {
            int currentIndex = 0;
            return ExtractVisualRecursively(group, targetIndex, originalId, ref currentIndex);
        }

        private ModelVisual3D ExtractVisualRecursively(Model3DGroup group, int targetIndex, string originalId, ref int currentIndex)
        {
            foreach (var child in group.Children)
            {
                if (child is GeometryModel3D geometryModel)
                {
                    currentIndex++;
                    if (currentIndex == targetIndex)
                    {
                        // Нашли нужный меш! Клонируем его геометрию и материалы, чтобы они не накладывались на другие объекты
                        GeometryModel3D clonedMesh = new GeometryModel3D(geometryModel.Geometry, geometryModel.Material);
                        if (geometryModel.BackMaterial != null) clonedMesh.BackMaterial = geometryModel.BackMaterial;

                        Model3DGroup singleGroup = new Model3DGroup();
                        singleGroup.Children.Add(clonedMesh);

                        ModelVisual3D visual = new ModelVisual3D { Content = singleGroup };
                        visual.SetName(originalId); // Присваиваем ему тот самый старый ID из JSON
                        return visual;
                    }
                }
                else if (child is Model3DGroup subGroup)
                {
                    var res = ExtractVisualRecursively(subGroup, targetIndex, originalId, ref currentIndex);
                    if (res != null) return res;
                }
            }
            return null;
        }

        private void AddCustomField_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtNewFieldName.Text) && _selectedModelData != null)
            {
                _selectedModelData.DynamicFields.Add(new CubeField { Name = TxtNewFieldName.Text, Value = "" });
                TxtNewFieldName.Clear();
                FieldsItemsControl.ItemsSource = null; 
                FieldsItemsControl.ItemsSource = _selectedModelData.DynamicFields;
            }
        }
        // ЛОГИКА УДАЛЕНИЯ ПОЛЯ ИЗ КАРТОЧКИ ОБЪЕКТА
        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, на которую нажали
            Button deleteButton = sender as Button;
            if (deleteButton == null) return;

            // Из свойства Tag достаем ссылку на объект поля CubeField, которое нужно удалить
            CubeField fieldToRemove = deleteButton.Tag as CubeField;

            if (fieldToRemove != null && _selectedModelData != null)
            {
                // Удаляем поле из динамического списка выбранной модели
                _selectedModelData.DynamicFields.Remove(fieldToRemove);

                // Обновляем отображение списка в интерфейсе
                FieldsItemsControl.ItemsSource = null;
                FieldsItemsControl.ItemsSource = _selectedModelData.DynamicFields;
            }
        }
        // Добавление поля-файла в режиме редактирования
        private void AddFileField_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewFieldName.Text) || _selectedModelData == null) return;

            // Вызываем системный проводник для выбора ЛЮБОГО файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл для прикрепления к объекту",
                Filter = "Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Создаем папку AttachedFiles рядом с .exe приложения, если её еще нет
                    string targetFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AttachedFiles");
                    if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                    // Формируем уникальное имя файла, чтобы избежать перезаписи при одинаковых именах
                    string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                    string uniqueFileName = $"{Guid.NewGuid().ToString().Substring(0, 6)}_{fileName}";
                    string targetPath = System.IO.Path.Combine(targetFolder, uniqueFileName);

                    // Копируем файл физически в папку проекта
                    File.Copy(openFileDialog.FileName, targetPath, true);

                    // Сохраняем ТОЛЬКО относительный путь для переносимости
                    string relativePath = System.IO.Path.Combine("AttachedFiles", uniqueFileName);

                    _selectedModelData.DynamicFields.Add(new CubeField
                    {
                        Name = TxtNewFieldName.Text,
                        Value = relativePath,
                        IsFileLink = true // Помечаем, что это файл
                    });

                    TxtNewFieldName.Clear();
                    FieldsItemsControl.ItemsSource = null;
                    FieldsItemsControl.ItemsSource = _selectedModelData.DynamicFields;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка");
                }
            }
        }
        // Метод очищает зажатые клавиши полета, когда пользователь кликает в любое текстовое поле,
        // чтобы камера случайно не улетела во время ввода текста
        private void TextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            _pressedKeys.Clear();
        }
        private void CloseEditMenu_Click(object sender, RoutedEventArgs e) 
        { 
            if (_selectedModelData != null) _selectedModelData.Name = TxtCubeName.Text; 
            EditMenuPanel.Visibility = Visibility.Collapsed; 
        }
        private void ShowCubeInfoPanel(ModelData data)
        {
            if (data == null) return;

            ViewFieldsStack.Children.Clear();
            LblCubeTitle.Text = data.Name;

            foreach (var field in data.DynamicFields)
            {
                // Создаем контейнер строки (Название | Значение/Ссылка | Кнопка Копировать)
                Grid rowGrid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });

                // 1. Название поля
                TextBlock txtName = new TextBlock
                {
                    Text = $"{field.Name}:",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(txtName, 0);
                rowGrid.Children.Add(txtName);

                // 2. Значение (Обычный текст ИЛИ синяя ссылка для файлов)
                FrameworkElement valueElement;

                if (field.IsFileLink)
                {
                    // Если это файл, создаем кликабельный TextBlock в виде гиперссылки
                    TextBlock txtLink = new TextBlock
                    {
                        Text = System.IO.Path.GetFileName(field.Value.Substring(7)), // Срезаем "AttachedFiles/" для красоты
                        Foreground = Brushes.Blue,
                        TextDecorations = TextDecorations.Underline,
                        Cursor = Cursors.Hand,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        ToolTip = "Нажмите, чтобы открыть файл в системе"
                    };

                    // Записываем полный относительный путь в Tag, чтобы считать при клике
                    txtLink.Tag = field.Value;
                    txtLink.MouseDown += AttachedFileLink_MouseDown; // Обработчик открытия файла

                    valueElement = txtLink;
                }
                else
                {
                    // Если обычный текст
                    TextBlock txtValue = new TextBlock
                    {
                        Text = field.Value,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    valueElement = txtValue;
                }

                Grid.SetColumn(valueElement, 1);
                rowGrid.Children.Add(valueElement);

                // 3. Кнопка копирования в буфер обмена «🖹» (Пункт 1)
                Button btnCopy = new Button
                {
                    Content = "🖹",
                    ToolTip = "Скопировать значение в буфер обмена",
                    Width = 22,
                    Height = 22,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    FontSize = 12,
                    Tag = field.Value // Кнопка запоминает, что именно ей копировать
                };
                btnCopy.Click += CopyToClipboard_Click;

                Grid.SetColumn(btnCopy, 2);
                rowGrid.Children.Add(btnCopy);

                ViewFieldsStack.Children.Add(rowGrid);
            }

            if (data.DynamicFields.Count == 0)
            {
                ViewFieldsStack.Children.Add(new TextBlock { Text = "Дополнительных полей нет.", FontStyle = FontStyles.Italic });
            }

            ViewInfoPanel.Visibility = Visibility.Visible;
            EditMenuPanel.Visibility = Visibility.Collapsed;
        }
        private void CloseViewMenu_Click(object sender, RoutedEventArgs e) 
        { 
            ViewInfoPanel.Visibility = Visibility.Collapsed; 
        }
        // МЕХАНИКА КОПИРОВАНИЯ В БУФЕР ОБМЕНА
        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string textToCopy = btn.Tag.ToString();

                // Если это файл, копируем имя файла, если текст — копируем сам текст
                if (textToCopy.StartsWith("AttachedFiles"))
                {
                    textToCopy = System.IO.Path.GetFileName(textToCopy);
                }

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    Clipboard.SetText(textToCopy);

                    // Небольшая визуальная подсказка: меняем иконку кнопки на галочку на полсекунды
                    btn.Content = "✓";
                    var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
                    timer.Tick += (s, args) => { btn.Content = "🖹"; timer.Stop(); };
                    timer.Start();
                }
            }
        }

        // МЕХАНИКА АВТОМАТИЧЕСКОГО ОТКРЫТИЯ ЛЮБОГО ФАЙЛА СИСТЕМОЙ
        private void AttachedFileLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock linkBlock && linkBlock.Tag != null)
            {
                string relativePath = linkBlock.Tag.ToString();
                // Собираем полный абсолютный путь к файлу в текущей системе
                string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                if (File.Exists(absolutePath))
                {
                    try
                    {
                        // Запуск процесса открытия файла через дефолтное системное приложение (PDF reader, Браузер, Excel и т.д.)
                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = absolutePath,
                            UseShellExecute = true // Обязательный флаг для .NET Core / .NET 5+, чтобы задействовать ассоциации файлов Windows
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка");
                    }
                }
                else
                {
                    MessageBox.Show("Файл не найден в папке AttachedFiles! Проверьте наличие директории.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    // Вспомогательный класс для обхода ошибок опечаток
    internal static class Vector3Class 
    { 
        public static Vector3D CrossProduct(Vector3D r, Vector3D l) { return Vector3D.CrossProduct(r, l); } 
    }
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    }
}
```

-----------------------

## Наполнение NET-MAUI-BlazorHybridCodeBase.md
```
description: Для проекта на C# .NET MAUI Blazor Hybrid в отличие от WPF решения всё намного проще - существует 2 режима работы. 1) Обычный "просмотр". По умолчанию - при клике на объект сцены появляется окно с настроенными данными объекта - по умолчанию только его имя, если в режиме "редактирования" не было настроено иное. В случае, если окно было модифицировано с помощью режима "редактирования" - помимо изменённого имени объекта ещё могут отображаться некоторые статичные параметры с возможностью их быстрого копирования в буфер обмена по нажатию соответствующей кнопки - это может быть обычный текст длиной до 2000 символов, ссылки на сайт, фото (.jpg, .jpeg, .png, .bmp и прочие) или файл (.docx\doc, .xlsx\xls, .pdf, .txt и прочие), в случае с фото и файлами - они хранятся в специальной внутренней папке приложения для быстрого доступа и для возможности безопасного переноса папки с программой с диска на диск (для целостности ссылок на фото и файлы). 2) Режим "редактирования". При наведении курсора мыши на объект сцены его грани подсвечивается оранжевым цветом (процесс выделения\выбора объекта сцены для его редактирования). Далее при клике на выделенный объект появляется окно с возможностью изменить текущее имя объекта, также настроить\добавить для его окна новые параметры, которые имеют 2 поля для настройки в режиме редактирования - имя параметра, и его значение в зависимости от выбранного типа параметра - "обычный текст" (текстовое поле для написания текста пользователем до 2000 символов), "ссылка на сайт" (текст ссылки для мгновенного открытия браузера при клике на ссылку для перехода на интернет-ресурс ссылки), "фото" (поле с кнопкой "прикрепить фото" с функцией drag&drop файлов с расширением .jpg, .jpeg, .png, .bmp и прочие файлы, подразумевающие изображение. Можно прикрепить несколько изображений!) и "файл" (поле с кнопкой "прикрепить файл" с функцией drag&drop файлов с расширением .docx\doc, .xlsx\xls, .pdf, .txt и прочие файлы. Можно прикрепить несколько файлов!)). Когда настраиваемое окно готово, внизу по кнопке "сохранить" данные объекта и его окна соответственно сохраняются в локальной базе данных в формате .JSON-нотаций, и при следующей загрузке модели в сцену (three-container) по кнопке "Выбор файла" все внесённые изменения в окна в режиме "редактирования" должны подгрузится вместе с моделью из локальной базы данных в в формате .JSON-нотаций. Для данной реализации всё намного проще - мы используем нативную навигацию Three.js и веб-стек под обёрткой MAUI для большей совместимости с разными платформами. Но такое решение оказалось не надёжным в плане реализации сбора данных в отличие от WPF, где такой проблемы нет в априори, т.к. WPF не требует специфических настроек для доступа к данным. Как итог - только наполовину работающий функционал
```

### Кодовая база C# .NET MAUI Blazor Hybrid проекта 3д-вьюера 

#### Структура проекта

```
MauiBlazorThreeJsApp
|--Properties
|--wwwroot
|   |--js
|   |   |--threeScene.js
|   |--lib
|   |--app.css
|   |--index.html
|--Зависимости
|--Components
|   |--Layout
|   |   |--MainLayout.razor
|   |   |--NavMenu.razor
|   |--Pages
|   |   |--Counter.razor
|   |   |--Home.razor
|   |   |--NotFound.razor
|   |   |--ObjectEditModal.razor
|   |   |--ObjectViewModal.razor
|   |   |--Weather.razor
|   |--_Imports.razor
|   |--Routes.razor
|   |--ThreeScene.razor
|--Models
|   |--SceneObjectData.cs
|--Platforms
|--Resources
|--Services
|   |--DataService.cs
|   |--FileService.cs
|--App.xaml
|   |--App.xaml.cs
|--MainPage.xaml
|   |--MainPage.xaml.cs
|--MauiProgram.cs
```

### Наполнение класса-модели данных сцены SceneObjectData.cs

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MauiBlazorThreeJsApp.Models
{
    public class SceneObjectData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Уникальный ID объекта

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Без имени";

        [JsonPropertyName("parameters")]
        public List<ObjectParameter> Parameters { get; set; } = new();
    }

    public class ObjectParameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "text"; // "text", "link", "image", "file"

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty; // Для текста и ссылок

        [JsonPropertyName("filePaths")] // Для хранения путей к файлам (изображениям или документам)
        public List<string> FilePaths { get; set; } = new();
    }
}
```

### Наполнение класса-сервиса данных объектов сцены DataService.cs

```csharp
using MauiBlazorThreeJsApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MauiBlazorThreeJsApp.Services
{
    public class DataService
    {
        private readonly string _dataFilePath;

        public DataService()
        {
            // Папка для данных приложения. Файлы будут сохранены здесь.
            var appDataPath = FileSystem.AppDataDirectory;
            _dataFilePath = Path.Combine(appDataPath, "scene_objects_data.json");
        }

        // Загрузка всех данных из JSON-файла
        public async Task<List<SceneObjectData>> LoadDataAsync()
        {
            if (!File.Exists(_dataFilePath))
                return new List<SceneObjectData>();

            var json = await File.ReadAllTextAsync(_dataFilePath);
            return JsonSerializer.Deserialize<List<SceneObjectData>>(json) ?? new List<SceneObjectData>();
        }

        // Сохранение данных в JSON-файл
        public async Task SaveDataAsync(List<SceneObjectData> objectsData)
        {
            var json = JsonSerializer.Serialize(objectsData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
    }
}
```

### Наполнение класса-сервиса обработки и загрузки данных FileService.cs

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Storage; // для FileSystem и Launcher

namespace MauiBlazorThreeJsApp.Services
{
    public class FileService
    {
        private readonly string _storageFolder;

        public FileService()
        {
            // Создаем папку для файлов внутри AppDataDirectory
            _storageFolder = Path.Combine(FileSystem.AppDataDirectory, "object_files");
            if (!Directory.Exists(_storageFolder))
                Directory.CreateDirectory(_storageFolder);
        }

        // Сохранение файла из потока в локальное хранилище
        public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string objectId)
        {
            // Генерируем уникальное имя, чтобы избежать конфликтов
            var extension = Path.GetExtension(originalFileName);
            var uniqueFileName = $"{objectId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_storageFolder, uniqueFileName);

            using var fileStreamOutput = File.Create(filePath);
            await fileStream.CopyToAsync(fileStreamOutput);

            return filePath; // Возвращаем полный путь к сохраненному файлу
        }

        // Получение потока файла для отображения или скачивания
        public Stream GetFileStream(string filePath)
        {
            return File.OpenRead(filePath);
        }

        // Удаление файла (например, при удалении параметра или объекта)
        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        // + Метод чтения файлов/фото
        public async Task<string> ReadFileAsBase64Async(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            var bytes = await File.ReadAllBytesAsync(filePath);
            return Convert.ToBase64String(bytes);
        }

        public async Task OpenFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден", filePath);

            var fileInfo = new FileInfo(filePath);
            var request = new OpenFileRequest(fileInfo.Name, new ReadOnlyFile(filePath));
            // Открываем файл через системный Launcher
            await Launcher.Default.OpenAsync(request);
        }

        private string GetContentType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
}
```

### Наполнение класса-инициализатора MauiProgram.cs

```csharp
using MauiBlazorThreeJsApp.Services;
using Microsoft.Extensions.Logging;

namespace MauiBlazorThreeJsApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<DataService>();
            builder.Services.AddSingleton<FileService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
```

### Наполнение модального окна просмотра ObjectViewModal.razor

```razor
@using MauiBlazorThreeJsApp.Models
@using MauiBlazorThreeJsApp.Services
@inject IJSRuntime JS
@inject FileService FileService

@if (IsVisible)
{
    <div class="modal-overlay" @onclick="Close">
        <div class="modal-content" @onclick:stopPropagation="true">
            <h3>Информация об объекте</h3>
            <div><strong>Имя:</strong> @(ObjectData?.Name ?? "Без имени")</div>
            @if (ObjectData?.Parameters != null && ObjectData.Parameters.Any())
            {
                <hr />
                <h4>Параметры:</h4>
                @foreach (var param in ObjectData.Parameters)
                {
                    <div style="margin: 8px 0; border-bottom: 1px solid #eee; padding-bottom: 4px;">
                        <strong>@param.Name:</strong>
                        @switch (param.Type)
                        {
                            case "text":
                                <span>@param.Value</span>
                                <button class="copy-btn" @onclick="() => CopyToClipboard(param.Value)">Копировать</button>
                                break;
                            case "link":
                                <a href="@param.Value" target="_blank">@param.Value</a>
                                <button class="copy-btn" @onclick="() => CopyToClipboard(param.Value)">Копировать</button>
                                break;
                            case "image":
                                @foreach (var filePath in param.FilePaths)
                                {
                                    var imgSrc = _imageCache.ContainsKey(filePath) ? _imageCache[filePath] : "";
                                    <div style="margin: 4px 0;">
                                        @if (!string.IsNullOrEmpty(imgSrc))
                                        {
                                            <img src="@imgSrc" style="max-width: 200px; max-height: 200px; border: 1px solid #ccc;" />
                                        }
                                        else
                                        {
                                            <span>Изображение не загружено</span>
                                        }
                                        <button class="copy-btn" @onclick="() => CopyToClipboard(filePath)">Копировать путь</button>
                                        <button class="open-btn" @onclick="() => OpenFile(filePath)">Открыть</button>
                                    </div>
                                }
                                break;
                            case "file":
                                @foreach (var filePath in param.FilePaths)
                                {
                                    <div style="margin: 4px 0;">
                                        <span>@Path.GetFileName(filePath)</span>
                                        <button class="copy-btn" @onclick="() => CopyToClipboard(filePath)">Копировать путь</button>
                                        <button class="open-btn" @onclick="() => OpenFile(filePath)">Открыть</button>
                                    </div>
                                }
                                break;
                            default:
                                <span>@param.Value</span>
                                break;
                        }
                    </div>
                }
            }
            else
            {
                <p>Нет дополнительных параметров.</p>
            }
            <div style="margin-top: 15px;">
                <button @onclick="Close">Закрыть</button>
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public SceneObjectData? ObjectData { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private Dictionary<string, string> _imageCache = new();

    protected override async Task OnParametersSetAsync()
    {
        if (ObjectData?.Parameters != null)
        {
            _imageCache.Clear();
            foreach (var param in ObjectData.Parameters)
            {
                if (param.Type == "image" && param.FilePaths != null)
                {
                    foreach (var path in param.FilePaths)
                    {
                        if (!_imageCache.ContainsKey(path))
                        {
                            try
                            {
                                var base64 = await FileService.ReadFileAsBase64Async(path);
                                if (!string.IsNullOrEmpty(base64))
                                {
                                    var contentType = GetContentType(path);
                                    _imageCache[path] = $"data:{contentType};base64,{base64}";
                                }
                            }
                            catch { /* игнорируем ошибки */ }
                        }
                    }
                }
            }
        }
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }

    private async Task CopyToClipboard(string text)
    {
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    private async Task OpenFile(string filePath)
    {
        try
        {
            await FileService.OpenFileAsync(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось открыть файл: {ex.Message}");
        }
    }

    private string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLower();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
```

### Наполнение модального окна редактирования ObjectEditModal.razor

```razor
@using MauiBlazorThreeJsApp.Models
@using MauiBlazorThreeJsApp.Services
@using System.IO
@using Microsoft.AspNetCore.Components;

@* ObjectEditModal.razor *@
@if (IsVisible && ObjectData != null)
{
    <div class="modal-overlay" @onclick="Close">
        <div class="modal-content" @onclick:stopPropagation="true" style="max-width: 600px;">
            <h3>Редактирование объекта</h3>
            <div>
                <label>Имя объекта:</label>
                <input @bind="ObjectData.Name" style="width: 100%; padding: 5px;" />
            </div>
            <hr />
            <h4>Параметры</h4>
            @for (int i = 0; i < ObjectData.Parameters.Count; i++)
            {
                var localIndex = i; // ★ захват
                var param = ObjectData.Parameters[i];
                <div style="border: 1px solid #ddd; padding: 10px; margin: 10px 0; border-radius: 4px;">
                    <div style="display: flex; gap: 10px; align-items: center; margin-bottom: 8px;">
                        <label>Имя параметра:</label>
                        <input @bind="param.Name" style="flex:1;" />
                        <label>Тип:</label>
                        <select @bind="param.Type" style="min-width: 120px;">
                            <option value="text">Текст</option>
                            <option value="link">Ссылка</option>
                            <option value="image">Фото</option>
                            <option value="file">Файл</option>
                        </select>
                        <button @onclick="() => RemoveParameter(localIndex)">Удалить</button>
                    </div>

                    @if (param.Type == "text" || param.Type == "link")
                    {
                        <div>
                            <label>Значение:</label>
                            <input @bind="param.Value" style="width: 100%;" />
                        </div>
                    }
                    else if (param.Type == "image" || param.Type == "file")
                    {
                        <div>
                            <label>Прикрепленные файлы:</label>
                            @if (param.FilePaths.Any())
                            {
                                <ul>
                                    @foreach (var path in param.FilePaths)
                                    {
                                        <li>@Path.GetFileName(path) <button @onclick="() => RemoveFileFromParameter(localIndex, path)">Удалить</button></li>
                                    }
                                </ul>
                            }
                            <div id="drop-zone-@localIndex" style="border: 2px dashed #aaa; padding: 15px; text-align: center; margin-top: 5px;"
                                 @ref="dropZones[localIndex]"
                                 @ondragover:preventDefault="true"
                                 @ondrop:preventDefault="true"
                                 @ondrop="() => OnDrop(localIndex, param.Type)">
                                Перетащите файлы сюда или <button @onclick="() => TriggerFileInput(localIndex, param.Type)">выберите</button>
                                <input type="file" id="file-input-@localIndex" style="display:none;" multiple accept="@GetAccept(param.Type)" @onchange="() => OnFileSelected(localIndex, param.Type)" />
                            </div>
                        </div>
                    }
                </div>
            }
            <button @onclick="AddParameter">Добавить параметр</button>
            <hr />
            <div style="display: flex; gap: 10px;">
                <button @onclick="Save">Сохранить</button>
                <button @onclick="Close">Отмена</button>
            </div>
        </div>
    </div>
}

@code {
    public class FileData
    {
        public string fileName { get; set; }   // совпадает с JS
        public string fileData { get; set; }   // совпадает с JS
    }

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public SceneObjectData ObjectData { get; set; } = new();
    [Parameter] public EventCallback<SceneObjectData> OnSave { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public DotNetObjectReference<Home>? DotNetHelper { get; set; }

    [Inject] IJSRuntime JS { get; set; }
    [Inject] FileService FileService { get; set; }

    private List<ElementReference> dropZones = new();
    private List<ElementReference> fileInputs = new();
    private DotNetObjectReference<Home>? _dotNetHelper;

    protected override void OnParametersSet()
    {
        if (ObjectData != null)
        {
            fileInputs = new List<ElementReference>(ObjectData.Parameters.Count);
            dropZones = new List<ElementReference>(ObjectData.Parameters.Count);
            for (int i = 0; i < ObjectData.Parameters.Count; i++)
            {
                fileInputs.Add(new ElementReference());
                dropZones.Add(new ElementReference());
            }
        }
    }

    [JSInvokable]
    public async Task OnFilesDropped(int index, FileData[] files)
    {
        await ProcessFiles(index, files);
    }

    private void AddParameter()
    {
        ObjectData.Parameters.Add(new ObjectParameter
        {
            Name = "Новый параметр",
            Type = "text",
            Value = ""
        });
        fileInputs.Add(new ElementReference());
        dropZones.Add(new ElementReference());
    }

    private void RemoveParameter(int index)
    {
        if (index < 0 || index >= ObjectData.Parameters.Count) return;
        var param = ObjectData.Parameters[index];
        foreach (var path in param.FilePaths)
            FileService.DeleteFile(path);
        ObjectData.Parameters.RemoveAt(index);
        if (index < fileInputs.Count) fileInputs.RemoveAt(index);
        if (index < dropZones.Count) dropZones.RemoveAt(index);
    }

    private async Task Save()
    {
        await OnSave.InvokeAsync(ObjectData);
        await Close();
        StateHasChanged();
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }

    // Drag & Drop
    private void OnDragOver(DragEventArgs e)
    {
        // Закомментировал, т.к. e.PreventDefault() вызывает ошибку: ""DragEventArgs" не содержит определения "PreventDefault", и не удалось найти доступный метод расширения "PreventDefault", принимающий тип "DragEventArgs" в качестве первого аргумента (возможно, пропущена директива using или ссылка на сборку)."
        //e.PreventDefault();
    }

    private async Task OnDrop(int index, string type)
    {
        // Ничего не делаем, вся логика в JS (вызовет OnFilesDropped)
        // Но можно добавить уведомление.
    }

    // Вызов диалога выбора файла
    private async Task TriggerFileInput(int index, string type)
    {
        await JS.InvokeVoidAsync("triggerFileInput", $"file-input-{index}");
    }

    private async Task OnFileSelected(int index, string type)
    {
        try
        {
            var files = await JS.InvokeAsync<FileData[]>("getSelectedFiles", fileInputs[index]);
            await ProcessFiles(index, files);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при выборе файлов: {ex.Message}");
        }
    }

    private async Task ProcessFiles(int index, FileData[] files)
    {
        if (files == null || files.Length == 0) return;
        foreach (var file in files)
        {
            var bytes = Convert.FromBase64String(file.fileData);
            using var stream = new MemoryStream(bytes);
            var savedPath = await FileService.SaveFileAsync(stream, file.fileName, ObjectData.Id);
            ObjectData.Parameters[index].FilePaths.Add(savedPath);
        }
        StateHasChanged();
    }

    private void RemoveFileFromParameter(int index, string path)
    {
        FileService.DeleteFile(path);
        ObjectData.Parameters[index].FilePaths.Remove(path);
    }

    private string GetAccept(string type)
    {
        return type == "image" ? "image/*" : ".doc,.docx,.xls,.xlsx,.pdf,.txt";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsVisible && ObjectData != null)
        {
            _dotNetHelper = DotNetHelper;
            for (int i = 0; i < ObjectData.Parameters.Count && i < dropZones.Count; i++)
            {
                var param = ObjectData.Parameters[i];
                if (param.Type == "image" || param.Type == "file")
                {
                    await JS.InvokeVoidAsync("setupDropZone", dropZones[i], _dotNetHelper, i, ObjectData.Id);
                }
            }
        }
    }

    // Чтобы работал drag&drop, нужно добавить JS-функции в отдельный файл или в threeScene.js
}
```

### Наполнение главного окна Home.razor

```razor
@page "/"
@using MauiBlazorThreeJsApp.Models
@using MauiBlazorThreeJsApp.Services
@inject IJSRuntime JS
@inject DataService DataService
@inject FileService FileService
@implements IAsyncDisposable

<PageTitle>3D Сцена</PageTitle>

<h3>Моя 3D сцена на Three.js</h3>

@* Кнопка для выбора файла *@
<InputFile OnChange="OnFileSelected" accept=".glb,.gltf,.obj" maxFileSize="524288000" style="margin-bottom: 10px;" /> <!-- 500 МБ лимит -->

@* Управление светом сцены *@
<label>Яркость: <input type="range" min="0.5" max="5" step="0.1" @oninput="OnBrightnessChange" /></label>

@* Управление режимами работы *@
<div style="margin: 10px 0; display: flex; gap: 20px; align-items: center;">
    <label>
        <input type="radio" name="mode" value="view" checked @onchange="OnModeChange" />
        Режим просмотра
    </label>
    <label>
        <input type="radio" name="mode" value="edit" @onchange="OnModeChange" />
        Режим редактирования
    </label>
    <span style="font-size: 0.9em; color: #666;">
        @(currentMode == "view" ? "Кликните на объект для просмотра информации" : "Наведите на объект для подсветки, кликните для редактирования")
    </span>
</div>

@* Модальное окно просмотра *@
<ObjectViewModal IsVisible="@showViewModal" ObjectData="@viewModalData" OnClose="CloseViewModal" />

@* Модальное окно редакирования *@
<ObjectEditModal IsVisible="@showEditModal" ObjectData="@editModalData" OnSave="SaveObjectData" OnClose="CloseEditModal" DotNetHelper="_dotNetHelper" />

@* Контейнер для Three.js *@
<div id="three-container" style="width: 100%; height: 500px; border: 1px solid #ccc;"></div>

@* Трейс-лог *@
@if (!string.IsNullOrEmpty(statusMessage))
{
    <div style="color: @(isError ? "red" : "green"); margin: 10px 0;">
        @statusMessage
    </div>
}

@code {
    //Для построения модели на сцене
    private IJSObjectReference? _threeJsModule;

    //Для трейс-лога
    private string statusMessage = "";
    private bool isError = false;

    // Для управления режимами
    private string currentMode = "view";

    // Данные всех объектов на сцене
    private List<SceneObjectData> _objectsData = new();

    // Хелпер для трейслога и вывода ошибок
    private DotNetObjectReference<Home>? _dotNetHelper;
    
    // Статус модального окна просмотра
    private bool showViewModal = false;
    private SceneObjectData? viewModalData = null;

    // Статус модального окна редактирования
    private bool showEditModal = false;
    private SceneObjectData? editModalData = null;

    // --- Жизненный цикл ---
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Добавляем параметр версии, чтобы избежать кэширования
                var version = DateTime.Now.Ticks;
                _threeJsModule = await JS.InvokeAsync<IJSObjectReference>("import", $"./js/threeScene.js?v={version}");
                _dotNetHelper = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("initializeDotNetHelper", _dotNetHelper);

                _objectsData = await DataService.LoadDataAsync();
                await JS.InvokeVoidAsync("setObjectData", _objectsData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации: {ex.Message}");
            }
        }
    }

    // --- Обработчики ошибок ---
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_threeJsModule is not null)
        {
            await _threeJsModule.DisposeAsync();
        }
        _dotNetHelper?.Dispose();
    }

    // --- Обработчики UI для режимов работы системы ---
    private async Task OnModeChange(ChangeEventArgs e)
    {
        currentMode = e.Value?.ToString() ?? "view";
        await JS.InvokeVoidAsync("setMode", currentMode);
    }

    // --- Обратные вызовы из JavaScript ---

    /*
    // --- Default ---
    [JSInvokable]
    public async Task OnObjectSelected(string objectId, SceneObjectData? data)
    {
        // Показать окно просмотра (можно реализовать как модальное окно или отдельный компонент)
        // Здесь мы просто покажем сообщение, но вы можете открыть модальное окно
        if (data == null)
        {
            // Данных нет, показываем только имя (или ID)
            statusMessage = $"Выбран объект: {objectId} (данные не найдены)";
        }
        else
        {
            statusMessage = $"Выбран объект: {data.Name}";
            // Здесь можно открыть модальное окно с данными
            // Например, вызвать метод для отображения параметров
        }
        isError = false;
        StateHasChanged();
    }
    */
    
    [JSInvokable]
    public async Task OnObjectSelected(string objectId, SceneObjectData? data)
    {
        Console.WriteLine($"OnObjectSelected: {objectId}, data: {data != null}");
        viewModalData = data ?? new SceneObjectData { Id = objectId, Name = objectId };
        showViewModal = true;
        StateHasChanged();
    }

    /*
    // --- Default ---
    [JSInvokable]
    public async Task OnObjectEdit(string objectId, SceneObjectData? data)
    {
        // Открыть окно редактирования
        // data может быть null, если объект еще не имеет данных
        var objectData = data ?? new SceneObjectData { Id = objectId, Name = objectId };
        // Открыть модальное окно редактирования с objectData
        // После сохранения вызвать SaveObjectData
        Console.WriteLine($"Редактирование объекта: {objectId}");
    }
    */

    [JSInvokable]
    public async Task OnObjectEdit(string objectId, SceneObjectData? data)
    {
        Console.WriteLine($"OnObjectEdit: {objectId}, data: {data != null}");
        editModalData = data ?? new SceneObjectData { Id = objectId, Name = objectId };
        showEditModal = true;
        StateHasChanged();
    }

    [JSInvokable]
    public async Task OnObjectDeselected()
    {
        statusMessage = "Выделение снято";
        StateHasChanged();
    }

    // --- Методы для сохранения данных ---

    public async Task SaveObjectData(SceneObjectData updatedData)
    {
        // Найти и обновить или добавить данные
        var existing = _objectsData.FirstOrDefault(d => d.Id == updatedData.Id);
        if (existing != null)
        {
            _objectsData.Remove(existing);
        }
        _objectsData.Add(updatedData);

        // Сохранить в JSON
        await DataService.SaveDataAsync(_objectsData);

        // Обновить данные в JS
        await JS.InvokeVoidAsync("setObjectData", _objectsData);

        statusMessage = $"Данные для объекта '{updatedData.Name}' сохранены";
        StateHasChanged();
    }

    // --- Обработка выбранного файла ---
    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;

        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 500 * 1024 * 1024); //500 МБ лимит
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            await JS.InvokeVoidAsync("loadModel", fileBytes, file.Name);
            statusMessage = $"Модель '{file.Name}' успешно загружена!";
            isError = false;
        }
        catch (Exception ex)
        {
            statusMessage = $"Ошибка: {ex.Message}";
            isError = true;
            //Для трейс-лога
            Console.WriteLine($"Ошибка загрузки файла: {ex.Message}");
        }

        // 1. Принудительно обновляем UI, чтобы пользователь УВИДЕЛ сообщение успехов/ошибки
        StateHasChanged();

        // 2. Ждем 7 секунд в фоне
        await Task.Delay(7000);

        // 3. Очищаем сообщение
        statusMessage = string.Empty;

        // 4. Снова принудительно обновляем UI, чтобы сообщение ИСЧЕЗЛО с экрана
        StateHasChanged();

    }

    // --- Для смены освещения сцены ---
    private async Task OnBrightnessChange(ChangeEventArgs e)
    {
        var val = float.Parse(e.Value.ToString().Replace('.',','));
        await JS.InvokeVoidAsync("setBrightness", val);
    }

    // --- Для обработки закрытия модального окна просмотра ---
    private void CloseViewModal()
    {
        showViewModal = false;
        StateHasChanged();
    }

    // --- Для обработки закрытия модального окна редактирования ---
    private void CloseEditModal()
    {
        showEditModal = false;
        StateHasChanged();
    }
}
```

### Наполнение js-логики Three.js-сцены threeScene.js

```js
import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";
import { OBJLoader } from "three/addons/loaders/OBJLoader.js";

const scene = new THREE.Scene();
scene.background = new THREE.Color(0xCBD5E1);  //Цвет сцены в HEX

const camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 1000);
camera.position.set(5, 5, 5);
camera.lookAt(0, 0, 0);

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.2;
renderer.outputColorSpace = THREE.SRGBColorSpace;

const container = document.getElementById('three-container');
container.appendChild(renderer.domElement);

const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.dampingFactor = 0.05;
controls.screenSpacePanning = true;

// ------------------- ОСВЕЩЕНИЕ (улучшенное) -------------------
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

// Пол (можно оставить для ориентации, но для большой модели он может быть не нужен)
const planeGeometry = new THREE.PlaneGeometry(5, 5);
const planeMaterial = new THREE.MeshStandardMaterial({ color: 0x336699, side: THREE.DoubleSide });
const plane = new THREE.Mesh(planeGeometry, planeMaterial);
plane.rotation.x = -Math.PI / 2;
plane.position.y = -0.51;
plane.receiveShadow = true;
scene.add(plane);

// ------------------- СОСТОЯНИЕ -------------------
let currentMode = 'view'; // 'view' или 'edit'
let selectedObject = null; // Текущий выбранный объект (для режима просмотра)
let hoveredObject = null; // Объект под курсором (для режима редактирования)
let originalColors = new Map(); // Хранилище исходных цветов для сброса подсветки
let objectDataMap = new Map(); // Хранилище данных объектов (имя, параметры), загруженных из C#
let dotNetHelper = null; // Ссылка на C#-объект для обратных вызовов
const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();

// ------------------- ЗАГРУЗКА МОДЕЛЕЙ -------------------
let currentModel = null;

function clearModel() {
    if (currentModel) {
        scene.remove(currentModel);
        currentModel = null;
    }
}

window.loadModel = function (data, fileName) {
    clearModel();
    const extension = fileName.split('.').pop().toLowerCase();
    let loader;
    let arrayBuffer;
    if (data instanceof Uint8Array) {
        arrayBuffer = data.buffer;
    } else {
        arrayBuffer = data;
    }

    if (extension === 'glb' || extension === 'gltf') {
        loader = new GLTFLoader();
        loader.parse(arrayBuffer, '', (gltf) => {
            const model = gltf.scene || gltf;
            addModelToScene(model);
        }, (error) => {
            console.error('Ошибка загрузки GLB/GLTF:', error);
        });
    } else if (extension === 'obj') {
        loader = new OBJLoader();
        const blob = new Blob([data], { type: 'text/plain' });
        const url = URL.createObjectURL(blob);
        loader.load(url, (object) => {
            addModelToScene(object);
            URL.revokeObjectURL(url);
        }, undefined, (error) => {
            console.error('Ошибка загрузки OBJ:', error);
            URL.revokeObjectURL(url);
        });
    } else {
        console.error('Неподдерживаемый формат:', extension);
    }
};

function addModelToScene(model) {
    model.traverse((child) => {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;
        }
    });
    scene.add(model);
    currentModel = model;

    // Центрирование и настройка камеры под размер модели
    const box = new THREE.Box3().setFromObject(model);
    const size = box.getSize(new THREE.Vector3()).length();
    const center = box.getCenter(new THREE.Vector3());
    model.position.sub(center);

    const dist = size / (2 * Math.tan(camera.fov * Math.PI / 360));
    camera.position.set(dist * 1.2, dist * 0.8, dist * 1.2);
    controls.target.set(0, 0, 0);
    controls.update();

    // Подстраиваем теневую камеру под размер модели
    const d = size * 0.6;
    dirLight.shadow.camera.left = -d;
    dirLight.shadow.camera.right = d;
    dirLight.shadow.camera.top = d;
    dirLight.shadow.camera.bottom = -d;
    dirLight.shadow.camera.near = 0.1;
    dirLight.shadow.camera.far = size * 2.5;
    dirLight.shadow.updateProjectionMatrix();

    console.log('Модель загружена, размер:', size);
}

// ------------------- АНИМАЦИЯ -------------------
function animate() {
    requestAnimationFrame(animate);
    controls.update();
    renderer.render(scene, camera);
}
animate();

// ------------------- RESIZE -------------------
window.addEventListener('resize', onWindowResize, false);
function onWindowResize() {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}

// Обработчик изменения освещения сцены
window.setBrightness = function (value) {
    dirLight.intensity = value;
};

console.log('Three.js готов к загрузке больших моделей.');

// Сброс подсветки
function clearHighlight() {
    if (hoveredObject) {
        const originalColor = originalColors.get(hoveredObject);
        if (originalColor && hoveredObject.material) {
            hoveredObject.material.color.setHex(originalColor);
        }
        originalColors.delete(hoveredObject);
        hoveredObject = null;
    }
}

// Обработчик движения мыши (для подсветки в режиме редактирования)
renderer.domElement.addEventListener('mousemove', onMouseMove);
function onMouseMove(event) {
    if (currentMode !== 'edit') {
        // Если мы не в режиме редактирования, убираем подсветку, если она была
        if (hoveredObject) {
            clearHighlight();
        }
        return;
    }

    // Вычисление позиции мыши в нормализованных координатах
    const rect = renderer.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);

    // Собираем все видимые меши для проверки пересечений
    const meshes = [];
    scene.traverse((child) => {
        if (child.isMesh) {
            meshes.push(child);
        }
    });

    const intersects = raycaster.intersectObjects(meshes);

    let newHoveredObject = null;
    if (intersects.length > 0) {
        newHoveredObject = intersects[0].object;
    }

    // Если наводим на новый объект
    if (newHoveredObject !== hoveredObject) {
        // Сбрасываем подсветку старого
        clearHighlight();

        if (newHoveredObject) {
            // Сохраняем и меняем цвет нового
            if (newHoveredObject.material) {
                const originalColor = newHoveredObject.material.color.getHex();
                originalColors.set(newHoveredObject, originalColor);
                newHoveredObject.material.color.setHex(0xFFA500); // Оранжевый
            }
            hoveredObject = newHoveredObject;
            // Меняем курсор
            renderer.domElement.style.cursor = 'pointer';
        } else {
            renderer.domElement.style.cursor = 'default';
        }
    }
}

renderer.domElement.addEventListener('click', onClick);
function onClick(event) {
    const rect = renderer.domElement.getBoundingClientRect();
    mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    raycaster.setFromCamera(mouse, camera);

    const meshes = [];
    scene.traverse((child) => {
        if (child.isMesh) {
            meshes.push(child);
        }
    });

    const intersects = raycaster.intersectObjects(meshes);

    if (intersects.length === 0) {
        if (currentMode === 'view' && selectedObject) {
            selectedObject = null;
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnObjectDeselected');
            }
        }
        return;
    }

    const clickedObject = intersects[0].object;
    // Используем uuid как стабильный идентификатор
    const objectId = clickedObject.uuid;

    console.log('Клик по объекту:', clickedObject);
    console.log('objectId:', objectId);

    if (currentMode === 'view') {
        selectedObject = clickedObject;
        const objectData = objectDataMap.get(objectId);
        console.log('Данные для просмотра:', objectData);
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnObjectSelected', objectId, objectData)
                .catch(err => console.error('Ошибка вызова OnObjectSelected:', err));
        }
    } else if (currentMode === 'edit') {
        if (hoveredObject === clickedObject) {
            const objectData = objectDataMap.get(objectId);
            console.log('Данные для редактирования:', objectData);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnObjectEdit', objectId, objectData)
                    .catch(err => console.error('Ошибка вызова OnObjectEdit:', err));
            }
        }
    }
}

// Установка режима работы
window.setMode = function (mode) {
    currentMode = mode;
    clearHighlight();
    if (selectedObject) {
        selectedObject = null;
    }
    console.log('Режим изменен на:', mode);
};

// Функция для передачи ссылки на C#-объект
window.initializeDotNetHelper = function (helper) {
    dotNetHelper = helper;
    console.log('DotNetHelper инициализирован');
};

// Обнуление данных (заглушка)
window.setObjectData = function (data) {
    objectDataMap.clear();
    if (data && Array.isArray(data)) {
        data.forEach(item => {
            objectDataMap.set(item.id, item);
        });
    }
    console.log('Данные объектов обновлены');
};

// --- Триггер для file input ---
window.triggerFileInput = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) element.click();
};

// + ------------------- РАБОТА С ФАЙЛАМИ -------------------
// Чтение файлов из <input type="file">
window.getSelectedFiles = function (element) {
    return new Promise((resolve, reject) => {
        const element = document.getElementById(elementId);
        if (!element) {
            reject(new Error('Элемент не найден'));
            return;
        }
        const files = element.files;
        const result = [];
        let loaded = 0;
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const reader = new FileReader();
            reader.onload = function (e) {
                // Убираем префикс "data:image/png;base64," и т.п., оставляем только base64
                const base64 = e.target.result.split(',')[1];
                result.push({
                    fileName: file.name,
                    fileData: base64
                });
                loaded++;
                if (loaded === files.length) {
                    resolve(result);
                }
            };
            reader.onerror = function (e) {
                reject(e.target.error);
            };
            reader.readAsDataURL(file);
        }
    });
};

// Чтение файлов из события Drag&Drop
// (будет вызываться из C# с передачей события, но проще использовать отдельный обработчик)
// Для упрощения пока оставим заглушку, но можно реализовать как глобальный обработчик.
// Мы реализуем через отдельную функцию, которая подписывается на drop для конкретного элемента.
window.setupDropZone = function (elementId, dotNetHelper, paramIndex, objectId) {
    const element = document.getElementById(elementId);
    if (!element || typeof element.addEventListener !== 'function') {
        console.warn('setupDropZone: элемент не является DOM-элементом', element);
        return;
    }
    if (!element) {
        console.error('Элемент с id', elementId, 'не найден');
        return;
    }
    element.addEventListener('dragover', function (e) {
        e.preventDefault();
    });
    element.addEventListener('drop', function (e) {
        e.preventDefault();
        const files = e.dataTransfer.files;
        if (!files || files.length === 0) return;
        const result = [];
        let loaded = 0;
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const reader = new FileReader();
            reader.onload = function (ev) {
                const base64 = ev.target.result.split(',')[1];
                result.push({
                    fileName: file.name,
                    fileData: base64
                });
                loaded++;
                if (loaded === files.length) {
                    dotNetHelper.invokeMethodAsync('OnFilesDropped', paramIndex, result)
                        .catch(err => console.error('Ошибка вызова OnFilesDropped:', err));
                }
            };
            reader.onerror = function (ev) {
                console.error('Ошибка чтения файла:', ev.target.error);
            };
            reader.readAsDataURL(file);
        }
    });
};
```

--------------------

# Пулл чата с DeepSeek по разработке 3д-движка NotaVerse - BIM

## Сообщение 1

Привет! Нужна твоя помощь в создании 3д-движка на C# WPF с использованием Three.js для отображения моделей в 3д-сцене. У меня есть 2 проекта - я хочу внедрить логику Three.js из одного проекта в другой на C# WPF. Для этого ознакомься с проектами HelixToolkitCodeBase.md (C# WPF проект - основной для интеграции) и NET-MAUI-BlazorHybridCodeBase.md (.NET MAUI Blazor Hybrid проект - интегрируемый в C# WPF). От тебя требуется подробное описание по шагам по реализации моего приложения с подробным расписыванием каждого шага - что делать, как делать, кодом модулей и т.д. с учётом того, что я работаю в Visual Studio Community 2026. По интеграции - навигацию оставляем нативную от Three.js с динамичными настройками сцены от второго проекта, а платформу и бизнес-логику хранения информации оставляем от основного C# WPF. В итоге: должно получится решение на C# WPF c современным Three.js для просмотра, загрузки и сохранения параметров сцены загружаемой из blender модели .glb в 2х режимах работы: 1) Обычный "просмотр". По умолчанию - при клике на объект сцены появляется окно с настроенными данными объекта - по умолчанию только его имя, если в режиме "редактирования" не было настроено иное. В случае, если окно было модифицировано с помощью режима "редактирования" - помимо изменённого имени объекта ещё могут отображаться некоторые статичные параметры с возможностью их быстрого копирования в буфер обмена по нажатию соответствующей кнопки - это может быть обычный текст длиной до 2000 символов, ссылки на сайт, фото (.jpg, .jpeg, .png, .bmp и прочие) или файл (.docx\doc, .xlsx\xls, .pdf, .txt и прочие), в случае с фото и файлами - они хранятся в специальной внутренней папке приложения для быстрого доступа и для возможности безопасного переноса папки с программой с диска на диск (для целостности ссылок на фото и файлы). 2) Режим "редактирования". При наведении курсора мыши на объект сцены его грани подсвечивается оранжевым цветом (процесс выделения\выбора объекта сцены для его редактирования). Далее при клике на выделенный объект появляется окно с возможностью изменить текущее имя объекта, также настроить\добавить для его окна новые параметры, которые имеют 2 поля для настройки в режиме редактирования - имя параметра, и его значение в зависимости от выбранного типа параметра - "обычный текст" (текстовое поле для написания текста пользователем до 2000 символов), "ссылка на сайт" (текст ссылки для мгновенного открытия браузера при клике на ссылку для перехода на интернет-ресурс ссылки), "фото" (поле с кнопкой "прикрепить фото" с функцией drag&drop файлов с расширением .jpg, .jpeg, .png, .bmp и прочие файлы, подразумевающие изображение. Можно прикрепить несколько изображений!) и "файл" (поле с кнопкой "прикрепить файл" с функцией drag&drop файлов с расширением .docx\doc, .xlsx\xls, .pdf, .txt и прочие файлы. Можно прикрепить несколько файлов!)). Когда настраиваемое окно готово, внизу по кнопке "сохранить" данные объекта и его окна соответственно сохраняются в локальной базе данных в формате .JSON-нотаций, и при следующей загрузке модели в сцену (three-container) по кнопке "Выбор файла" все внесённые изменения в окна в режиме "редактирования" должны подгрузится вместе с моделью из локальной базы данных в в формате .JSON-нотаций.

## Ответ 1

Пошаговое руководство по интеграции Three.js в C# WPF проект
Общая архитектура решения

Прежде чем перейти к пошаговой реализации, важно понять итоговую архитектуру вашего приложения. Вы объединяете два подхода:

* От WPF-проекта (HelixToolkitCodeBase.md) — берёте всю бизнес-логику: модель данных DataModel.cs, систему хранения параметров объектов, работу с файлами (папка AttachedFiles), сериализацию сцены в JSON, UI-панели (просмотр/редактирование), логику копирования в буфер обмена.

* От MAUI Blazor Hybrid проекта (NET-MAUI-BlazorHybridCodeBase.md) — берёте 3D-движок на Three.js: threeScene.js с навигацией (OrbitControls), загрузку .glb/.gltf через GLTFLoader, raycasting для выбора объектов, подсветку граней оранжевым, режимы view/edit.

Ключевое архитектурное решение: вместо Viewport3D из WPF вы используете контрол WebView2, который загружает локальную HTML-страницу с Three.js. Вся 3D-сцена рендерится в браузерном движке (Chromium), а WPF-приложение выступает в роли «хоста» — оно предоставляет окна для отображения/редактирования данных, хранит JSON-базу и управляет файлами.

Схема взаимодействия:
```
┌─────────────────────────────────────────────────────┐
│                   WPF Приложение                     │
│  ┌───────────────────────────────────────────────┐  │
│  │              MainWindow.xaml                   │  │
│  │  ┌─────────────────┐  ┌─────────────────────┐ │  │
│  │  │   WebView2      │  │  Панель просмотра/  │ │  │
│  │  │  (Three.js)     │  │  редактирования     │ │  │
│  │  │                 │  │  (WPF UI)           │ │  │
│  │  │  - Орбит        │  │                     │ │  │
│  │  │  - Зум          │  │  - TxtCubeName      │ │  │
│  │  │  - Raycast      │  │  - FieldsItemsCtrl  │ │  │
│  │  │  - Подсветка    │  │  - Кнопки файлов    │ │  │
│  │  └────────┬────────┘  └─────────────────────┘ │  │
│  └───────────┼───────────────────────────────────┘  │
│              │  postMessage / ExecuteScriptAsync     │
│              ▼                                       │
│  ┌───────────────────────────────────────────────┐  │
│  │            C# сервисы (бизнес-логика)          │  │
│  │  - DataService (JSON-база)                     │  │
│  │  - FileService (AttachedFiles)                 │  │
│  │  - SceneConfig (сериализация камеры + данных)  │  │
│  └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```
Шаг 1. Создание нового WPF-проекта и установка зависимостей
1.1. Создание проекта

* Откройте Visual Studio Community 2026.

* Создайте новый проект: Файл → Создать → Проект.

* Выберите шаблон «Приложение WPF» (C#) — именно WPF, не .NET MAUI.

* Назовите проект, например, NotaVerseBIM.WebView.

* Целевая платформа: .NET 8.0 (или .NET 9.0, если доступна в вашей версии VS).

1.2. Установка NuGet-пакетов

Откройте Обозреватель решений → правой кнопкой по проекту → Управление NuGet-пакетами и установите:
| Пакет	| Назначение |
|---|---|
| Microsoft.Web.WebView2	| Встраивание Chromium-движка в WPF для рендеринга Three.js |
| System.Text.Json	| Сериализация/десериализация JSON-базы (уже входит в .NET 8, но проверьте версию) |
    Примечание: HelixToolkit.Wpf больше не нужен — мы отказываемся от нативного WPF 3D в пользу Three.js. Однако если вы хотите сохранить его для каких-то вспомогательных целей, можете оставить.

1.3. Структура проекта после настройки

```
NotaVerseBIM.WebView/
├── Assets/
│   └── logo.png
├── wwwroot/                    ← НОВАЯ ПАПКА (копируется в выходной каталог)
│   ├── index.html              ← HTML-контейнер для Three.js
│   ├── css/
│   │   └── app.css
│   └── js/
│       ├── threeScene.js       ← Основная логика Three.js
│       └── interop.js          ← Мост между JS и C#
├── Services/
│   ├── DataService.cs          ← Работа с JSON-базой
│   ├── FileService.cs          ← Работа с AttachedFiles
│   └── WebViewBridge.cs        ← Обёртка над postMessage/ExecuteScriptAsync
├── Models/
│   ├── SceneObjectData.cs      ← Модель данных объекта (из MAUI-проекта)
│   └── SceneConfig.cs          ← Конфигурация сцены (из WPF-проекта)
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
└── MainWindow.xaml.cs
```

1.4. Настройка копирования wwwroot в выходной каталог

В `.csproj` файле добавьте:

```xml
<ItemGroup>
  <Content Include="wwwroot\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

Это гарантирует, что HTML/CSS/JS-файлы будут доступны рядом с `.exe` при запуске.

Шаг 2. Создание HTML-контейнера и подключение Three.js

2.1. Файл `wwwroot/index.html`

```html
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>3D Viewer — NotaVerseBIM</title>
    <link rel="stylesheet" href="css/app.css">
    <!-- Three.js загружается как ES-модуль через importmap -->
    <script type="importmap">
    {
        "imports": {
            "three": "https://cdn.jsdelivr.net/npm/three@0.170.0/build/three.module.js",
            "three/addons/": "https://cdn.jsdelivr.net/npm/three@0.170.0/examples/jsm/"
        }
    }
    </script>
</head>
<body>
    <div id="three-container"></div>
    <script type="module" src="js/threeScene.js"></script>
    <script src="js/interop.js"></script>
</body>
</html>
```
    Важно: Мы используем CDN для Three.js, чтобы не усложнять сборку. Если вам нужна автономность (работа без интернета), скачайте three.module.js и папку addons локально в wwwroot/lib/three/ и укажите локальные пути в importmap.

2.2. Файл `wwwroot/css/app.css`

```css
* { margin: 0; padding: 0; box-sizing: border-box; }

body, html {
    width: 100%;
    height: 100%;
    overflow: hidden;
    background: #1a1a2e;
    font-family: 'Segoe UI', sans-serif;
}

#three-container {
    width: 100vw;
    height: 100vh;
    display: block;
}

/* Стили для модальных окон, если они будут рендериться в WebView */
.modal-overlay {
    position: fixed;
    top: 0; left: 0;
    width: 100%; height: 100%;
    background: rgba(0,0,0,0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
}

.modal-content {
    background: white;
    border-radius: 8px;
    padding: 20px;
    max-width: 500px;
    max-height: 80vh;
    overflow-y: auto;
    box-shadow: 0 4px 20px rgba(0,0,0,0.3);
}
```

Шаг 3. Перенос логики Three.js из MAUI-проекта

3.1. Файл `wwwroot/js/threeScene.js`

Этот файл — адаптированная версия `threeScene.js` из MAUI-проекта. Основные изменения:

* Убираем импорт `OrbitControls`, `GLTFLoader`, `OBJLoader` — они подключаются через **importmap**.

* Добавляем обработку .glb через `GLTFLoader` (MAUI-проект использовал и **.glb**, и **.obj**, и **.gltf** — оставляем **.glb** как основной формат).

* Сохраняем логику **raycasting**, подсветки, режимов **view/edit**.

* Добавляем мост для приёма данных из C# и отправки событий в C#.

```js
import * as THREE from "three";
import { OrbitControls } from "three/addons/controls/OrbitControls.js";
import { GLTFLoader } from "three/addons/loaders/GLTFLoader.js";

// ======================== ИНИЦИАЛИЗАЦИЯ СЦЕНЫ ========================
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x1a1a2e);

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

    const extension = fileName.split(".").pop().toLowerCase();
    const arrayBuffer = bytes.buffer ? bytes.buffer : bytes;

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
                notifyCSharp("modelLoaded", { fileName, status: "error", message: error.message });
            }
        );
    } else {
        console.error("Неподдерживаемый формат:", extension);
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
    model.traverse((child) => {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;
            // Назначаем уникальный ID каждому мешу
            if (!child.userData.objectId) {
                child.userData.objectId = child.uuid;
            }
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
    const d = size * 0.6;
    dirLight.shadow.camera.left = -d;
    dirLight.shadow.camera.right = d;
    dirLight.shadow.camera.top = d;
    dirLight.shadow.camera.bottom = -d;
    dirLight.shadow.camera.near = 0.1;
    dirLight.shadow.camera.far = size * 2.5;
    dirLight.shadow.updateProjectionMatrix();

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
window.updateObjectData = function (objectId, data) {
    if (data) {
        objectDataMap.set(objectId, data);
    } else {
        objectDataMap.delete(objectId);
    }
};

/**
 * Регулировка яркости.
 */
window.setBrightness = function (value) {
    dirLight.intensity = value;
};

// ======================== МОСТ С C# ========================
function notifyCSharp(type, payload) {
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync("OnJsMessage", type, payload)
            .catch((err) => console.error("Ошибка вызова OnJsMessage:", err));
    }
}

window.initializeDotNetHelper = function (helper) {
    dotNetHelper = helper;
    console.log("DotNetHelper инициализирован");
};
```

3.2. Файл `wwwroot/js/interop.js`

Этот файл содержит вспомогательные функции для работы с файлами (**drag&drop, file input**) — они будут вызываться из C# через `ExecuteScriptAsync` или из компонентов **WebView**.

```js
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
window.setupDropZone = function (elementId, dotNetHelper, paramIndex) {
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
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync("OnFilesDropped", paramIndex, result)
                            .catch((err) => console.error("Ошибка OnFilesDropped:", err));
                    }
                }
            };
            reader.onerror = function (ev) {
                console.error("Ошибка чтения файла:", ev.target.error);
            };
            reader.readAsDataURL(file);
        }
    });
};
```

Шаг 4. Создание C#-моделей данных

4.1. Файл `Models/SceneObjectData.cs`

Эта модель пришла из **MAUI**-проекта. Она используется для объектов, которые **Three.js** видит на сцене. Каждый объект — это отдельный меш (или группа мешей), которому соответствует запись в **JSON**-базе.

```csharp
using System.Text.Json.Serialization;

namespace NotaVerseBIM.WebView.Models
{
    public class SceneObjectData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Без имени";

        [JsonPropertyName("parameters")]
        public List<ObjectParameter> Parameters { get; set; } = new();
    }

    public class ObjectParameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// "text", "link", "image", "file"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Пути к файлам (относительно папки AttachedFiles).
        /// </summary>
        [JsonPropertyName("filePaths")]
        public List<string> FilePaths { get; set; } = new();
    }
}
```

4.2. Файл `Models/SceneConfig.cs`

Эта модель пришла из **WPF**-проекта. Она хранит конфигурацию камеры и общие настройки сцены.

```csharp
using System.Text.Json.Serialization;

namespace NotaVerseBIM.WebView.Models
{
    public class SceneConfig
    {
        [JsonPropertyName("camera")]
        public CameraConfig Camera { get; set; } = new();

        [JsonPropertyName("objects")]
        public List<SceneObjectData> Objects { get; set; } = new();

        [JsonPropertyName("modelFilePath")]
        public string ModelFilePath { get; set; } = string.Empty;
    }

    public class CameraConfig
    {
        [JsonPropertyName("positionX")]
        public double PositionX { get; set; } = 5;

        [JsonPropertyName("positionY")]
        public double PositionY { get; set; } = 5;

        [JsonPropertyName("positionZ")]
        public double PositionZ { get; set; } = 5;

        [JsonPropertyName("targetX")]
        public double TargetX { get; set; } = 0;

        [JsonPropertyName("targetY")]
        public double TargetY { get; set; } = 0;

        [JsonPropertyName("targetZ")]
        public double TargetZ { get; set; } = 0;
    }
}
```
    Почему CameraConfig отличается от WPF-версии? В Three.js камера управляется через OrbitControls, который оперирует position и target. Поэтому мы храним именно эти значения, а не Radius/Theta/Phi (как в WPF-версии с ручной орбитальной математикой).

Шаг 5. Создание C#-сервисов

5.1. Файл `Services/DataService.cs`

Этот сервис отвечает за сохранение и загрузку JSON-базы объектов сцены. Он практически идентичен MAUI-версии, но с одним отличием: путь к файлу данных — рядом с `.exe`, а не в `AppDataDirectory`.

```csharp
using System.IO;
using System.Text.Json;
using NotaVerseBIM.WebView.Models;

namespace NotaVerseBIM.WebView.Services
{
    public class DataService
    {
        private readonly string _dataFilePath;

        public DataService()
        {
            // Папка рядом с .exe — для переносимости
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = Path.Combine(baseDir, "Data");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _dataFilePath = Path.Combine(dataFolder, "scene_config.json");
        }

        public string DataFilePath => _dataFilePath;

        /// <summary>
        /// Загрузка всей конфигурации сцены (камера + объекты).
        /// </summary>
        public async Task<SceneConfig> LoadSceneConfigAsync()
        {
            if (!File.Exists(_dataFilePath))
                return new SceneConfig();

            var json = await File.ReadAllTextAsync(_dataFilePath);
            return JsonSerializer.Deserialize<SceneConfig>(json) ?? new SceneConfig();
        }

        /// <summary>
        /// Сохранение всей конфигурации сцены.
        /// </summary>
        public async Task SaveSceneConfigAsync(SceneConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_dataFilePath, json);
        }

        /// <summary>
        /// Загрузка только списка объектов (для передачи в JS).
        /// </summary>
        public async Task<List<SceneObjectData>> LoadObjectsAsync()
        {
            var config = await LoadSceneConfigAsync();
            return config.Objects;
        }

        /// <summary>
        /// Сохранение только списка объектов.
        /// </summary>
        public async Task SaveObjectsAsync(List<SceneObjectData> objects)
        {
            var config = await LoadSceneConfigAsync();
            config.Objects = objects;
            await SaveSceneConfigAsync(config);
        }
    }
}
```

5.2. Файл `Services/FileService.cs`

Сервис для работы с файлами. Он копирует файлы в папку `AttachedFiles` рядом с `.exe`, сохраняет относительные пути и обеспечивает открытие файлов системными приложениями.

```csharp
using System.IO;
using System.Diagnostics;

namespace NotaVerseBIM.WebView.Services
{
    public class FileService
    {
        private readonly string _storageFolder;

        public FileService()
        {
            _storageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AttachedFiles");
            if (!Directory.Exists(_storageFolder))
                Directory.CreateDirectory(_storageFolder);
        }

        /// <summary>
        /// Сохранение файла из потока в локальное хранилище.
        /// Возвращает ОТНОСИТЕЛЬНЫЙ путь (например, "AttachedFiles/abc123_photo.jpg").
        /// </summary>
        public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string objectId)
        {
            var extension = Path.GetExtension(originalFileName);
            var uniqueFileName = $"{objectId}_{Guid.NewGuid().ToString()[..8]}{extension}";
            var filePath = Path.Combine(_storageFolder, uniqueFileName);

            using var output = File.Create(filePath);
            await fileStream.CopyToAsync(output);

            // Возвращаем относительный путь для переносимости
            return Path.Combine("AttachedFiles", uniqueFileName);
        }

        /// <summary>
        /// Получение абсолютного пути по относительному.
        /// </summary>
        public string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        /// <summary>
        /// Проверка существования файла.
        /// </summary>
        public bool FileExists(string relativePath)
        {
            return File.Exists(GetAbsolutePath(relativePath));
        }

        /// <summary>
        /// Удаление файла.
        /// </summary>
        public void DeleteFile(string relativePath)
        {
            var absPath = GetAbsolutePath(relativePath);
            if (File.Exists(absPath))
                File.Delete(absPath);
        }

        /// <summary>
        /// Открытие файла системным приложением.
        /// </summary>
        public void OpenFile(string relativePath)
        {
            var absPath = GetAbsolutePath(relativePath);
            if (!File.Exists(absPath))
                throw new FileNotFoundException("Файл не найден", absPath);

            Process.Start(new ProcessStartInfo
            {
                FileName = absPath,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// Чтение файла как Base64 (для отображения изображений в WPF).
        /// </summary>
        public async Task<string> ReadFileAsBase64Async(string relativePath)
        {
            var absPath = GetAbsolutePath(relativePath);
            if (!File.Exists(absPath)) return null;
            var bytes = await File.ReadAllBytesAsync(absPath);
            return Convert.ToBase64String(bytes);
        }
    }
}
```

5.3. Файл `Services/WebViewBridge.cs`

Этот сервис — обёртка над `WebView2`, которая упрощает отправку сообщений в **JS** и получение сообщений из **JS**.

```csharp
using Microsoft.Web.WebView2.Wpf;
using System.Text.Json;

namespace NotaVerseBIM.WebView.Services
{
    public class WebViewBridge
    {
        private readonly WebView2 _webView;

        public WebViewBridge(WebView2 webView)
        {
            _webView = webView;
        }

        /// <summary>
        /// Отправка команды в JS (вызов window-функции).
        /// </summary>
        public async Task SendCommandAsync(string functionName, params object[] args)
        {
            var argsJson = args.Length > 0
                ? string.Join(",", args.Select(a => JsonSerializer.Serialize(a)))
                : "";

            var script = $"{functionName}({argsJson});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Отправка команды с JSON-объектом.
        /// </summary>
        public async Task SendCommandWithJsonAsync(string functionName, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var script = $"{functionName}({json});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Отправка команды без аргументов.
        /// </summary>
        public async Task SendCommandAsync(string functionName)
        {
            var script = $"{functionName}();";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
```

Шаг 6. Настройка WebView2 в MainWindow.xaml

6.1. XAML-разметка

```xml
<Window x:Class="NotaVerseBIM.WebView.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:wv2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
        Title="NotaVerseBIM — 3D Viewer"
        Height="800" Width="1200"
        Icon="Assets/logo.png">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>  <!-- Панель управления -->
            <RowDefinition Height="*"/>     <!-- 3D-сцена (WebView2) -->
        </Grid.RowDefinitions>

        <!-- ==================== ПАНЕЛЬ УПРАВЛЕНИЯ ==================== -->
        <Border Grid.Row="0" Background="#C0000000" Padding="10">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <!-- Режимы работы -->
                <StackPanel Orientation="Horizontal" Grid.Column="0">
                    <RadioButton x:Name="RadioView" Content="👁 Просмотр"
                                 IsChecked="True" Foreground="White"
                                 Checked="Mode_Changed" Margin="0,0,20,0"/>
                    <RadioButton x:Name="RadioEdit" Content="✎ Инспектор"
                                 Foreground="White"
                                 Checked="Mode_Changed" Margin="0,0,20,0"/>
                </StackPanel>

                <!-- Кнопки -->
                <StackPanel Orientation="Horizontal" Grid.Column="1">
                    <Button Content="📂 Загрузить модель"
                            Click="LoadModel_Click"
                            Padding="10,5" Margin="0,0,10,0"
                            Background="#FF8E44AD" Foreground="White"
                            BorderThickness="0" FontWeight="Bold"/>

                    <Button Content="💾 Сохранить данные"
                            Click="SaveData_Click"
                            Padding="10,5" Margin="0,0,10,0"
                            Background="#FF27AE60" Foreground="White"
                            BorderThickness="0" FontWeight="Bold"/>

                    <Button Content="📥 Загрузить данные"
                            Click="LoadData_Click"
                            Padding="10,5"
                            Background="#FF2980B9" Foreground="White"
                            BorderThickness="0" FontWeight="Bold"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- ==================== 3D-СЦЕНА (WebView2) ==================== -->
        <wv2:WebView2 x:Name="WebView"
                      Grid.Row="1"
                      AllowExternalDrop="True"
                      DefaultBackgroundColor="#1a1a2e"/>

        <!-- ==================== ПАНЕЛЬ ПРОСМОТРА ==================== -->
        <Border x:Name="ViewInfoPanel"
                Visibility="Collapsed"
                Background="#F0F4F8"
                BorderBrush="SteelBlue"
                BorderThickness="2"
                CornerRadius="5"
                Width="300"
                HorizontalAlignment="Left"
                VerticalAlignment="Center"
                Margin="20"
                Padding="15">
            <StackPanel>
                <TextBlock x:Name="LblObjectTitle"
                           Text="Информация"
                           FontSize="16" FontWeight="Bold"
                           Foreground="SteelBlue" Margin="0,0,0,10"/>
                <StackPanel x:Name="ViewFieldsStack"/>
                <Button Content="Закрыть"
                        Click="CloseViewMenu_Click"
                        Margin="0,15,0,0"/>
            </StackPanel>
        </Border>

        <!-- ==================== ПАНЕЛЬ РЕДАКТИРОВАНИЯ ==================== -->
        <Border x:Name="EditMenuPanel"
                Visibility="Collapsed"
                Background="White"
                BorderBrush="Orange"
                BorderThickness="2"
                CornerRadius="5"
                Width="400"
                HorizontalAlignment="Right"
                VerticalAlignment="Center"
                Margin="20"
                Padding="15">
            <StackPanel>
                <TextBlock Text="Настройка аннотации"
                           FontSize="16" FontWeight="Bold"
                           Margin="0,0,0,10"/>

                <TextBlock Text="Название объекта:"/>
                <TextBox x:Name="TxtObjectName" Margin="0,0,0,10"/>

                <TextBlock Text="Дополнительные поля:"
                           FontWeight="Bold" Margin="0,5,0,5"/>

                <ItemsControl x:Name="FieldsItemsControl">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Grid Margin="0,4">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="100"/>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="80"/>
                                    <ColumnDefinition Width="30"/>
                                </Grid.ColumnDefinitions>

                                <TextBox Grid.Column="0"
                                         Text="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                         GotFocus="TextBox_GetFocus"
                                         Margin="0,0,5,0"
                                         Background="#FFF5F5F5" FontWeight="Bold"/>

                                <TextBox x:Name="ValueBox"
                                         Grid.Column="1"
                                         Text="{Binding Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                         GotFocus="TextBox_GetFocus"/>

                                <ComboBox Grid.Column="2"
                                          SelectedValue="{Binding Type, Mode=TwoWay}"
                                          Margin="5,0,5,0"
                                          SelectionChanged="TypeCombo_Changed">
                                    <ComboBoxItem Content="Текст" Tag="text"/>
                                    <ComboBoxItem Content="Ссылка" Tag="link"/>
                                    <ComboBoxItem Content="Фото" Tag="image"/>
                                    <ComboBoxItem Content="Файл" Tag="file"/>
                                </ComboBox>

                                <Button Grid.Column="3"
                                        Content="×"
                                        Click="DeleteField_Click"
                                        Tag="{Binding}"
                                        Margin="5,0,0,0"
                                        Background="#FFE74C3C"
                                        Foreground="White"
                                        BorderThickness="0"
                                        FontWeight="Bold"
                                        Cursor="Hand"/>
                            </Grid>
                            <DataTemplate.Triggers>
                                <DataTrigger Binding="{Binding Type}" Value="file">
                                    <Setter TargetName="ValueBox" Property="IsReadOnly" Value="True"/>
                                    <Setter TargetName="ValueBox" Property="Background" Value="#FFF3E5F5"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding Type}" Value="image">
                                    <Setter TargetName="ValueBox" Property="IsReadOnly" Value="True"/>
                                    <Setter TargetName="ValueBox" Property="Background" Value="#FFE5F3F5"/>
                                </DataTrigger>
                            </DataTemplate.Triggers>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>

                <!-- Конструктор добавления новых полей -->
                <Border BorderBrush="Gray" BorderThickness="0,1,0,0"
                        Margin="0,10,0,0" Padding="0,5,0,0">
                    <StackPanel>
                        <TextBox x:Name="TxtNewFieldName"
                                 ToolTip="Имя нового поля (напр. Вес, ID)"/>

                        <Grid Margin="0,5,0,0">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <Button Content="+ Текст"
                                    Click="AddCustomField_Click"
                                    Grid.Column="0" Margin="0,0,2,0"/>
                            <Button Content="+ Ссылка"
                                    Click="AddLinkField_Click"
                                    Grid.Column="1" Margin="2,0,2,0"/>
                            <Button Content="+ Фото📷"
                                    Click="AddImageField_Click"
                                    Grid.Column="2" Margin="2,0,2,0"
                                    Background="#FF9B59B6"
                                    Foreground="White"
                                    BorderThickness="0"/>
                            <Button Content="+ Файл📎"
                                    Click="AddFileField_Click"
                                    Grid.Column="3" Margin="2,0,0,0"
                                    Background="#FF9B59B6"
                                    Foreground="White"
                                    BorderThickness="0"/>
                        </Grid>
                    </StackPanel>
                </Border>

                <Button Content="💾 Сохранить и Закрыть"
                        Click="CloseEditMenu_Click"
                        Background="Orange"
                        Foreground="White"
                        FontWeight="Bold"
                        Height="35"
                        Margin="0,10,0,0"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

Шаг 7. Реализация MainWindow.xaml.cs

7.1. Поля и конструктор

```csharp
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using NotaVerseBIM.WebView.Models;
using NotaVerseBIM.WebView.Services;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NotaVerseBIM.WebView
{
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

        private async void OnNavigationCompleted(
            object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Передаём в JS ссылку на C#-объект для обратных вызовов
                var dotNetHelper = new DotNetObjectReference<MainWindow>(this);
                await _bridge.SendCommandWithJsonAsync(
                    "initializeDotNetHelper", dotNetHelper);

                // Передаём данные объектов
                await _bridge.SendCommandWithJsonAsync(
                    "setObjectData", _objectsData);

                // Устанавливаем начальный режим
                await _bridge.SendCommandAsync("setMode", "view");
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
                        await _bridge.SendCommandWithJsonAsync(
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
            await _bridge.SendCommandWithJsonAsync("updateObjectData",
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
                await _bridge.SendCommandAsync("setMode", "edit");
                ViewInfoPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Режим просмотра
                await _bridge.SendCommandAsync("setMode", "view");
                EditMenuPanel.Visibility = Visibility.Collapsed;
            }
        }

        // ==================== ЗАГРУЗКА МОДЕЛИ ====================
        private async void LoadModel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D модели (*.glb;*.gltf)|*.glb;*.gltf|" +
                         "Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var filePath = openFileDialog.FileName;
                    var fileName = Path.GetFileName(filePath);
                    var fileBytes = await File.ReadAllBytesAsync(filePath);

                    // Сохраняем путь к модели в конфигурации
                    _sceneConfig.ModelFilePath = filePath;

                    // Передаём байты в JS
                    // Для больших файлов используем Base64 (WebView2 имеет лимит на размер script)
                    var base64 = Convert.ToBase64String(fileBytes);

                    // Если файл > 10 МБ, используем виртуальный хост
                    if (fileBytes.Length > 10 * 1024 * 1024)
                    {
                        // Копируем модель в wwwroot/models/
                        var modelsDir = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "wwwroot", "models");
                        if (!Directory.Exists(modelsDir))
                            Directory.CreateDirectory(modelsDir);

                        var destPath = Path.Combine(modelsDir, fileName);
                        File.Copy(filePath, destPath, true);

                        // Загружаем через URL виртуального хоста
                        await _bridge.SendCommandAsync(
                            "loadModelFromUrl",
                            $"https://app.local/models/{fileName}",
                            fileName);
                    }
                    else
                    {
                        // Для небольших файлов передаём Base64
                        await _bridge.SendCommandAsync(
                            "loadModelFromBytes", base64, fileName);
                    }

                    // Сохраняем конфигурацию
                    await _dataService.SaveSceneConfigAsync(_sceneConfig);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки модели: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==================== СОХРАНЕНИЕ/ЗАГРУЗКА ДАННЫХ ====================
        private async void SaveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем список объектов в конфигурации
                _sceneConfig.Objects = _objectsData;

                // Синхронизируем камеру из JS
                var cameraJson = await WebView.CoreWebView2.ExecuteScriptAsync(
                    "JSON.stringify({ " +
                    "positionX: camera.position.x, " +
                    "positionY: camera.position.y, " +
                    "positionZ: camera.position.z, " +
                    "targetX: controls.target.x, " +
                    "targetY: controls.target.y, " +
                    "targetZ: controls.target.z " +
                    "})");

                var cameraData = JsonSerializer.Deserialize<CameraConfig>(
                    cameraJson.Trim('"').Replace("\\\"", "\""));
                if (cameraData != null)
                {
                    _sceneConfig.Camera = cameraData;
                }

                await _dataService.SaveSceneConfigAsync(_sceneConfig);

                MessageBox.Show("Конфигурация сцены сохранена!",
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
                _sceneConfig = await _dataService.LoadSceneConfigAsync();
                _objectsData = _sceneConfig.Objects;

                // Передаём данные в JS
                await _bridge.SendCommandWithJsonAsync("setObjectData", _objectsData);

                // Восстанавливаем камеру
                if (_sceneConfig.Camera != null)
                {
                    var cam = _sceneConfig.Camera;
                    await WebView.CoreWebView2.ExecuteScriptAsync(
                        $"camera.position.set({cam.PositionX}, {cam.PositionY}, {cam.PositionZ}); " +
                        $"controls.target.set({cam.TargetX}, {cam.TargetY}, {cam.TargetZ}); " +
                        "controls.update();");
                }

                // Если есть модель — загружаем её
                if (!string.IsNullOrEmpty(_sceneConfig.ModelFilePath) &&
                    File.Exists(_sceneConfig.ModelFilePath))
                {
                    // Можно реализовать автоматическую загрузку модели
                }

                MessageBox.Show("Данные сцены загружены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}",
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
            if (_selectedObjectData != null)
            {
                _selectedObjectData.Name = TxtObjectName.Text;

                // Обновляем или добавляем объект в общий список
                var existing = _objectsData.FirstOrDefault(
                    o => o.Id == _selectedObjectData.Id);
                if (existing != null)
                {
                    _objectsData.Remove(existing);
                }
                _objectsData.Add(_selectedObjectData);

                // Сохраняем в JSON
                await _dataService.SaveObjectsAsync(_objectsData);

                // Обновляем данные в JS
                await _bridge.SendCommandWithJsonAsync("updateObjectData",
                    new { objectId = _selectedObjectData.Id, data = _selectedObjectData });
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
```

Шаг 8. Обработка drag&drop и file input в WebView2

8.1. Почему drag&drop работает «из коробки»

**WebView2** поддерживает **drag&drop** файлов из проводника **Windows** по умолчанию. Свойство `AllowExternalDrop` (по умолчанию `true`) определяет, разрешено ли перетаскивание файлов извне в **WebView2**.

В нашем коде мы явно устанавливаем `AllowExternalDrop = true` в **XAML** и в коде инициализации. Это позволяет пользователю перетаскивать *.glb-модели* прямо в окно 3D-сцены.

8.2. Обработка drop-события в JS

В `threeScene.js` добавьте обработчик для drop-зоны загрузки модели:

```js
// В конце threeScene.js
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
```

8.3. Обработка file input для параметров

Для параметров типа «Фото» и «Файл» в панели редактирования **WPF** мы используем `OpenFileDialog` — это нативный диалог **Windows**, который не требует **WebView2**. Пользователь выбирает файлы, они копируются в `AttachedFiles` через `FileService`, и пути сохраняются в **JSON**.

**Drag&drop** для параметров можно реализовать аналогично — через **JS**-обработчик, который вызывает `OnFilesDropped` в **C#**. В нашем коде уже есть метод `OnFilesDropped` в `threeScene.js` (через `dotNetHelper`).

Шаг 9. Настройка виртуального хоста для загрузки локальных файлов
9.1. Проблема с `file:///`

WebView2 **блокирует** прямую загрузку локальных **HTML**-файлов через `file:///` из соображений безопасности. Попытка навигации на `file:///C:/...` приводит к ошибке.

9.2. Решение: `SetVirtualHostNameToFolderMapping`

Используйте виртуальный хост — это рекомендованный **Microsoft** способ загрузки локального контента:

```csharp
WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "app.local",                          // Имя виртуального хоста
    wwwrootPath,                          // Путь к папке с HTML/JS/CSS
    CoreWebView2HostResourceAccessKind.Allow  // Разрешить доступ
);

WebView.CoreWebView2.Navigate("https://app.local/index.html");
```

Это создаёт защищённый виртуальный домен, который ведёт себя как обычный **HTTPS**-сайт. Все относительные пути (`js/threeScene.js`, `css/app.css`) будут работать корректно.

9.3. Загрузка моделей через виртуальный хост

Для больших **.glb**-файлов (>10 МБ) мы копируем модель в `wwwroot/models/` и загружаем её через `https://app.local/models/имя_файла.glb`. Это эффективнее, чем передача **Base64** через `ExecuteScriptAsync`, потому что избегает ограничений на размер строки.

Шаг 10. Сборка и отладка

10.1. Проверка структуры выходного каталога

После сборки проекта в папке `bin/Debug/net8.0-windows/` должны находиться:

```
NotaVerseBIM.WebView.exe
wwwroot/
  ├── index.html
  ├── css/app.css
  └── js/
      ├── threeScene.js
      └── interop.js
AttachedFiles/       (создаётся автоматически)
Data/                (создаётся автоматически)
  └── scene_config.json
WebView2_Data/       (создаётся автоматически)
```

10.2. Отладка JavaScript

Для отладки JS-кода в WebView2:

- Запустите приложение.

- Нажмите **F12** — откроются DevTools (как в Chrome/Edge).

- Используйте вкладку **Console** для просмотра логов, **Sources** для точек останова.

10.3. Типичные проблемы и решения

| Проблема	| Причина	| Решение |
|---|---|---|
| Белый экран вместо 3D	| WebView2 не может загрузить `index.html`	| Проверьте `SetVirtualHostNameToFolderMapping` и путь к `wwwroot` |
| Модель не загружается | Формат не .glb/.gltf или ошибка парсинга	| Проверьте консоль DevTools, убедитесь, что файл валидный |
| Клик по объекту не работает	| Raycaster не находит меши	| Убедитесь, что модель добавлена в `scene`, а меши имеют `isMesh = true`
| Подсветка не сбрасывается	| Не вызывается `clearHighlight()`	| Проверьте, что `hoveredObject` корректно обновляется |
| Drag&drop не работает	| `AllowExternalDrop = false`	| Установите `AllowExternalDrop = true` в XAML |
| Файлы не открываются	| Путь к `AttachedFiles` неверный	| Проверьте `AppDomain.CurrentDomain.BaseDirectory` |

Шаг 11. Итоговая структура файлов

```
NotaVerseBIM.WebView/
├── Assets/
│   └── logo.png
├── Models/
│   ├── SceneObjectData.cs
│   └── SceneConfig.cs
├── Services/
│   ├── DataService.cs
│   ├── FileService.cs
│   └── WebViewBridge.cs
├── wwwroot/
│   ├── index.html
│   ├── css/
│   │   └── app.css
│   └── js/
│       ├── threeScene.js
│       └── interop.js
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── NotaVerseBIM.WebView.csproj
```

Заключение

Вы получили полноценное WPF-приложение, которое:

1. **Использует Three.js** (через WebView2) для рендеринга .glb-моделей — современный, производительный 3D-движок с поддержкой PBR-материалов, теней и пост-обработки.

2. **Сохраняет бизнес-логику WPF** — JSON-база объектов, папка `AttachedFiles` для переносимости, нативные WPF-панели для просмотра и редактирования.

3. **Поддерживает два режима:** «Просмотр» (клик по объекту → панель с данными) и «Редактирование» (наведение → оранжевая подсветка, клик → панель редактирования).

4. **Работает с любыми типами параметров:** текст до 2000 символов, ссылки на сайты, фото (несколько), файлы (несколько).

5. **Переносим** — вся папка с `.exe` может быть скопирована на другой диск, и все ссылки на файлы останутся целыми (относительные пути).

Ключевое преимущество этой архитектуры — разделение ответственности: Three.js отвечает за 3D, WPF — за данные и UI. Это даёт вам современный 3D-рендеринг без потери надёжности нативной платформы.

## Сообщение 2

Отлично! Успешно создал проект, загрузил все Nuget-пакеты и перенёс всю расписанную тобой архитектуру (папки, классы и прочее). Скрипт `importmap` в `wwwroot/index.html` пока оставил как у тебя, но папку `node_modules` с `three` уже перенёс в корень папки проекта, просто на всякий случай. 
В остальном есть пару моментов с методами `Path.Combine()` - вызывают ошибки: ""Path" является неоднозначной ссылкой между "System.Windows.Shapes.Path" и "System.IO.Path"." И `Path.GetFileName()` - вызывают ошибки: ""Path" является неоднозначной ссылкой между "System.Windows.Shapes.Path" и "System.IO.Path"." Пробовал исправить подписанием `Path` к `System.IO` - работает, если я сделал всё правильно - так и напиши, если это не верный путь - распиши как правильно исправить эти ошибки в методах. Также ещё ошибка возникает в строке `var dotNetHelper = new DotNetObjectReference<MainWindow>(this);` Тело ошибки: "Не удалось найти тип или имя пространства имен "DotNetObjectReference<>" (возможно, отсутствует директива using или ссылка на сборку)." Попробовал добавить `using Microsoft.JSInterop;` - не помогает, ошибка меняется на: ""DotNetObjectReference<MainWindow>" не содержит конструктор, который принимает аргументы 1." Ещё возникает ошибка в строке `HidePanels();` для `case "objectDeselected"` конструкции `switch (message.Type)` в методе `private async void OnWebMessageReceived`. Тело ошибки: "Имя "HidePanels" не существует в текущем контексте." Специфических ссылок на этот метод нет, так что за что он отвечает - я не знаю, но исправить надо. И ещё одна ошибка возникает в строке `$"https://app.local/models/{fileName}",` в `await _bridge.SendCommandAsync` метода `private async void LoadModel_Click(object sender, RoutedEventArgs e)`. 
Нужно исправить все перечисленные ошибки всё также описывая подробно - как исправить, зачем и почему и т.д.

## Ответ 2

Исправление всех ошибок компиляции и логики

Отлично, что почти всё заработало! Давайте разберём каждую ошибку по порядку. Все они — следствие смешения парадигм WPF и Blazor, а также неоднозначностей, которые вносит WPF своими implicit usings. Пройдёмся по каждой.

Ошибка 1. Неоднозначность `Path` (System.IO.Path vs System.Windows.Shapes.Path)

Почему это происходит

Начиная с .NET 6, WPF-шаблоны включают функцию Implicit Usings — автоматический импорт часто используемых пространств имён. Среди них:

* `System.IO` — где живёт `Path` (работа с файловыми путями)

* `System.Windows.Shapes` — где живёт `Path` (фигура-контур в XAML/WPF)

Оба класса называются `Path`, оба видны в файле без явного `using`, и компилятор не может решить, какой из них вы имеете в виду. Это классическая ловушка WPF.

Правильные способы решения

Вариант A (рекомендуемый) — алиас в начале файла:
```csharp
using IOPath = System.IO.Path;
// или короче:
using Path = System.IO.Path;
```

Использование `using Path = System.IO.Path;` создаёт локальный алиас, который «перекрывает» оба неоднозначных имени. Далее по файлу можно спокойно писать `Path.Combine(...)`, `Path.GetFileName(...)` — компилятор будет знать, что это `System.IO.Path`.

Вариант B (правильный, но громоздкий) — полная квалификация:

Писать везде `System.IO.Path.Combine(...)`, `System.IO.Path.GetFileName(...)`. Работает, но засоряет код.

Вариант C (наименее желательный) — отключить Implicit Usings:

В `.csproj:`

```xml
<ImplicitUsings>disable</ImplicitUsings>
```

Придётся вручную прописывать все `using`, включая нужные для WPF. Не рекомендую.

Что делать вам

Вы пишете: *«Пробовал исправить подписанием `Path` к `System.IO` — работает»*. Да, это вариант B, и он абсолютно рабочий. Но чтобы не писать `System.IO.Path` в сотнях мест, я рекомендую вариант A. Добавьте в самый верх файла `MainWindow.xaml.cs` (до всех остальных `using`):

```csharp
using Path = System.IO.Path;
```

Теперь `Path.Combine`, `Path.GetFileName` и т.д. будут разрешаться в `System.IO.Path` без ошибок.

Ошибка 2. `DotNetObjectReference<>` не существует в **WPF/WebView2**

Почему это происходит

Здесь моя вина — я ошибочно перенёс паттерн из Blazor-проекта. `DotNetObjectReference<T>` — это тип из пакета `Microsoft.JSInterop`, который используется исключительно в Blazor (Server и WebAssembly) для вызова C#-методов из JavaScript. В обычном WebView2 (WPF) этот механизм не существует.

Даже если вы поставите пакет `Microsoft.JSInterop` через NuGet, тип `DotNetObjectReference<T>` требует, чтобы T реализовывал специфические интерфейсы Blazor, и его конструктор принимает не просто объект, а `JSRuntime`-контекст. В WPF-контексте это не сработает — отсюда и ошибка «не содержит конструктор, который принимает аргументы 1».

Правильный механизм для WebView2

WebView2 имеет собственный нативный механизм двусторонней связи через `chrome.webview`:

* **JS → C#**: `window.chrome.webview.postMessage(данные)`. C# ловит это в событии `CoreWebView2.WebMessageReceived`.

* **C# → JS**: `CoreWebView2.ExecuteScriptAsync(...)` (мы это уже используем) или `CoreWebView2.PostWebMessageAsJson(...)`.

Никаких `DotNetHelper`, никаких ссылок — просто сообщения. Это надёжнее и проще.

Что нужно изменить

2.1. Обновить `threeScene.js` — убрать `dotNetHelper`, использовать `chrome.webview`

Полностью замените секцию моста с C# в `threeScene.js`:

```js
// ======================== МОСТ С C# ========================
/**
 * Отправка сообщения в C# через нативный API WebView2.
 * C# получит это в событии WebMessageReceived.
 */
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

// Больше НЕ НУЖНА функция initializeDotNetHelper — удалите её.
// Вместо неё C# сам подписывается на WebMessageReceived.
```

Удалите функцию `window.initializeDotNetHelper = function(helper) {...}` целиком. Также удалите переменную `let dotNetHelper = null;` в начале файла.

2.2. Обновить `interop.js` — то же самое для drop-зон

Функция `setupDropZone` использовала `dotNetHelper.invokeMethodAsync`. Замените на `chrome.webview.postMessage`:

```js
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
```

2.3. Обновить `MainWindow.xaml.cs`

Удалите `using Microsoft.JSInterop;` — он больше не нужен.

В методе `OnNavigationCompleted` уберите строки с `DotNetObjectReference`:

```csharp
private async void OnNavigationCompleted(
    object sender, CoreWebView2NavigationCompletedEventArgs e)
{
    if (e.IsSuccess)
    {
        // БОЛЬШЕ НЕ НУЖНО создавать DotNetObjectReference!
        // Достаточно просто передать данные в JS.

        // Передаём данные объектов
        await _bridge.SendCommandWithJsonAsync("setObjectData", _objectsData);

        // Устанавливаем начальный режим
        await _bridge.SendCommandAsync("setMode", "view");
    }
}
```

C# теперь ловит сообщения из JS через уже подписанный `OnWebMessageReceived`. В этом методе вам не нужно ничего менять — он читает `e.WebMessageAsJson`, который содержит JSON, отправленный из `chrome.webview.postMessage`.

Ошибка 3. Метод `HidePanels()` не существует

Почему это происходит

Моя недоработка — я вызвал метод `HidePanels()` в обработчике `objectDeselected`, но не определил его. Он должен просто скрывать обе боковые панели (просмотра и редактирования), когда пользователь кликает по пустому месту сцены.
Что делать

Добавьте приватный метод в `MainWindow.xaml.cs` (например, после `CloseViewMenu_Click`):

```csharp
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
```

Использование `_selectedObjectData = null;` здесь важно: если пользователь кликнул по пустому месту, логично «снять» текущий выбранный объект, чтобы следующее открытие панели начиналось с чистого листа.

Ошибка 4. Проблема с `SendCommandAsync` в `LoadModel_Click`

Почему это происходит

Здесь несколько накладывающихся друг на друга проблем:

1. **Неоднозначность** `Path` в строке `Path.GetFileName(filePath)` для получения `fileName` — это вызывало каскадную ошибку в последующих строках.

2. **Параметры** `params object[]` в `SendCommandAsync` — при передаче строковых аргументов они боксируются в `object`, что технически работает, но сигнатура допускает неявные ошибки типизации.

3. **Перегрузка** `SendCommandAsync` с `params` и без него может путать компилятор при разрешении вызова.

Что делать

4.1. Сначала убедитесь, что `Path` разрешён (см. Ошибку 1)

После добавления `using Path = System.IO.Path;` эта строка компилируется:

```csharp
var fileName = Path.GetFileName(filePath);
```

4.2. Упростить `WebViewBridge` — убрать неоднозначные перегрузки

Полностью перепишите `Services/WebViewBridge.cs`:

```csharp
using Microsoft.Web.WebView2.Wpf;
using System.Text.Json;

namespace NotaVerseBIM.WebView.Services
{
    /// <summary>
    /// Обёртка над WebView2 для вызова JS-функций с корректной
    /// сериализацией аргументов.
    /// </summary>
    public class WebViewBridge
    {
        private readonly WebView2 _webView;

        public WebViewBridge(WebView2 webView)
        {
            _webView = webView;
        }

        /// <summary>
        /// Вызов JS-функции без аргументов.
        /// </summary>
        public async Task CallAsync(string functionName)
        {
            var script = $"{functionName}();";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с одним строковым аргументом.
        /// </summary>
        public async Task CallAsync(string functionName, string arg)
        {
            var json = JsonSerializer.Serialize(arg);
            var script = $"{functionName}({json});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с двумя строковыми аргументами.
        /// </summary>
        public async Task CallAsync(string functionName, string arg1, string arg2)
        {
            var json1 = JsonSerializer.Serialize(arg1);
            var json2 = JsonSerializer.Serialize(arg2);
            var script = $"{functionName}({json1}, {json2});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с произвольным объектом (сериализуется в JSON).
        /// </summary>
        public async Task CallWithJsonAsync(string functionName, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var script = $"{functionName}({json});";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Вызов JS-функции с Base64-строкой (спец. для передачи больших данных).
        /// </summary>
        public async Task CallWithBase64Async(string functionName, string base64Data, string fileName)
        {
            // Для Base64 используем Uint8Array в JS, чтобы не тянуть всю строку как JS-литерал
            var script = $@"
                (function() {{
                    const binary = atob('{base64Data}');
                    const bytes = new Uint8Array(binary.length);
                    for (let i = 0; i < binary.length; i++) {{
                        bytes[i] = binary.charCodeAt(i);
                    }}
                    {functionName}(bytes, {JsonSerializer.Serialize(fileName)});
                }})();";
            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
```

Преимущества новой реализации:

- Нет перегрузок с `params object[]`, которые могут запутать компилятор.

- Есть явные overload'ы для 0, 1 и 2 строковых аргументов — то, что нам реально нужно.

- Есть отдельные методы для JSON-объектов и Base64.

- `CallWithBase64Async` корректно передаёт большие бинарные данные, декодируя их в `Uint8Array` на стороне JS — иначе `ExecuteScriptAsync` может упереться в лимит длины скрипта (около 10 МБ).

4.3. Обновить `MainWindow.xaml.cs` — использовать новые методы

Везде, где вызывались старые методы `SendCommandAsync` и `SendCommandWithJsonAsync`, замените на новые:

| Старый вызов |	Новый вызов |
|---|---|
| _bridge.SendCommandAsync("setMode", "view")	| _bridge.CallAsync("setMode", "view") |
| _bridge.SendCommandAsync("setMode")	| _bridge.CallAsync("setMode") |
| _bridge.SendCommandWithJsonAsync("setObjectData", _objectsData)	| _bridge.CallWithJsonAsync("setObjectData", _objectsData) |
| _bridge.SendCommandWithJsonAsync("updateObjectData", new { ... })	| _bridge.CallWithJsonAsync("updateObjectData", new { ... }) |
| Передача Base64-модели	| _bridge.CallWithBase64Async("loadModelFromBytes", base64, fileName) |
| Передача URL-модели	| _bridge.CallAsync("loadModelFromUrl", url, fileName) |

Обновлённый фрагмент `LoadModel_Click`:

```csharp
private async void LoadModel_Click(object sender, RoutedEventArgs e)
{
    var openFileDialog = new Microsoft.Win32.OpenFileDialog
    {
        Filter = "3D модели (*.glb;*.gltf)|*.glb;*.gltf|Все файлы (*.*)|*.*"
    };

    if (openFileDialog.ShowDialog() == true)
    {
        try
        {
            var filePath = openFileDialog.FileName;
            var fileName = Path.GetFileName(filePath); // Path теперь System.IO.Path
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            _sceneConfig.ModelFilePath = filePath;

            // Большие файлы — через виртуальный хост (копируем в wwwroot/models)
            if (fileBytes.Length > 10 * 1024 * 1024)
            {
                var modelsDir = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "models");
                Directory.CreateDirectory(modelsDir);

                var destPath = Path.Combine(modelsDir, fileName);
                File.Copy(filePath, destPath, true);

                var url = $"https://app.local/models/{fileName}";
                await _bridge.CallAsync("loadModelFromUrl", url, fileName);
            }
            else
            {
                // Небольшие файлы — через Base64
                var base64 = Convert.ToBase64String(fileBytes);
                await _bridge.CallWithBase64Async("loadModelFromBytes", base64, fileName);
            }

            await _dataService.SaveSceneConfigAsync(_sceneConfig);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки модели: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
```

Ошибка 5 (бонус). Обновление `threeScene.js` под новый interop

Поскольку мы заменили всю систему связи, приведу финальные части `threeScene.js`, которые изменились. Остальная логика (сцена, свет, raycast, подсветка) остаётся прежней.

Заменить блок «МОСТ С C#»

```js
// ======================== МОСТ С C# ========================
/**
 * Отправка сообщения в C# через нативный API WebView2.
 * C# ловит это в событии WebMessageReceived.
 */
function notifyCSharp(type, payload) {
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.postMessage({
            type: type,
            payload: payload || {}
        });
    }
}
```

Заменить `loadModelFromBytes` — теперь принимает `Uint8Array`

```js
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
```

Заменить `addModelToScene` — присвоить стабильные ID мешам

```js
function addModelToScene(model) {
    let meshIndex = 0;
    model.traverse((child) => {
        if (child.isMesh) {
            child.castShadow = true;
            child.receiveShadow = true;
            // Стабильный ID, который переживёт перезагрузку сцены
            if (!child.userData.objectId) {
                child.userData.objectId = `mesh_${meshIndex}_${child.uuid.substring(0, 8)}`;
            }
            meshIndex++;
        }
    });
    // ... остальная логика (центрирование, камера, тени) без изменений
}
```
    Зачем стабильные ID? В исходном коде использовался uuid из Three.js — он генерируется заново при каждой загрузке, значит сохранённые в JSON id не совпадут при следующем запуске. С префиксом mesh_0_, mesh_1_ и т.д. мы получаем предсказуемые ID, которые можно сопоставлять с JSON. В идеале — использовать имена объектов из Blender (child.name), если они там заданы.

Ошибка 6 (бонус). Использование локальной `node_modules` вместо CDN

Вы упомянули, что перенесли `node_modules` с `three` в корень проекта. Это отличное решение для автономности — приложение будет работать без интернета. Чтобы переключиться с CDN на локальные файлы, отредактируйте `wwwroot/index.html`:

6.1. Скопировать `three` в `wwwroot`

Из папки `node_modules/three/` скопируйте в `wwwroot/lib/three/`:

```
node_modules/three/build/three.module.js
node_modules/three/examples/jsm/controls/OrbitControls.js
node_modules/three/examples/jsm/loaders/GLTFLoader.js
node_modules/three/examples/jsm/loaders/OBJLoader.js   (если нужен)
```

Итоговая структура:

```
wwwroot/
├── lib/
│   └── three/
│       ├── three.module.js
│       └── addons/
│           ├── controls/OrbitControls.js
│           └── loaders/GLTFLoader.js
```

6.2. Обновить importmap

```html
<script type="importmap">
{
    "imports": {
        "three": "./lib/three/three.module.js",
        "three/addons/": "./lib/three/addons/"
    }
}
</script>
```

6.3. Важный нюанс: относительные импорты внутри аддонов

`OrbitControls.js` и `GLTFLoader.js` из `examples/jsm/` импортируют `three` через `import * as THREE from 'three'`. Это работает, потому что importmap перехватывает их по имени `'three'`. Если вы копируете их в свою структуру, сохраняйте относительные пути внутри файлов как есть — не редактируйте их.

Также `GLTFLoader.js` может импортировать утилиты из `../utils/BufferGeometryUtils.js`. Убедитесь, что скопировали и их, либо оставьте структуру как в оригинале:

```
wwwroot/lib/three/addons/loaders/GLTFLoader.js
wwwroot/lib/three/addons/utils/BufferGeometryUtils.js
wwwroot/lib/three/addons/controls/OrbitControls.js
```

Проще всего скопировать всю папку `node_modules/three/examples/jsm/` в `wwwroot/lib/three/addons/` — тогда структура сохранится и всё заработает без правок.

Итоговый чек-лист правок

1. ✅ `MainWindow.xaml.cs` — добавьте в самый верх `using Path = System.IO.Path;`.

2. ✅ `MainWindow.xaml.cs` — удалите `using Microsoft.JSInterop;`.

3. ✅ `MainWindow.xaml.cs` — добавьте метод `HidePanels()`.

4. ✅ `MainWindow.xaml.cs` — замените вызовы `SendCommandAsync/SendCommandWithJsonAsync` на `CallAsync/CallWithJsonAsync/CallWithBase64Async`.

5. ✅ `MainWindow.xaml.cs` — в `OnNavigationCompleted` уберите создание `DotNetObjectReference`.

6. ✅ `WebViewBridge.cs` — замените на новую версию с явными overload'ами.

7. ✅ `threeScene.js` — удалите `dotNetHelper`, `initializeDotNetHelper`; замените `notifyCSharp` на `chrome.webview.postMessage`.

8. ✅ `threeScene.js` — обновите `loadModelFromBytes` под `Uint8Array`.

9. ✅ `interop.js` — замените `setupDropZone` на версию с `chrome.webview.postMessage`.

10. ✅ (опционально) `index.html` — переключите `importmap` на локальные файлы `Three.js`.

После этих правок проект должен собраться без ошибок. Если что-то ещё всплывёт при отладке — пишите, поможем разобраться.

## Сообщение 3

Отлично! Всё перенёс и исправил согласно твоему описанию кроме importmap (нужно его внедрить в соответствии с наполнением официального репозитория https://github.com/mrdoob/three.js/ - хочу использовать свежую версию) - приложение запустилось, но есть пару нюансов по работе.
1) При загрузке моделей возникает ошибка-уведомление, скрин скинул. Текст ошибки: "Ошибка загрузки модели: dirLight.shadow.updateProjectionMatrix не является функцией". Это происходит при загрузке любой модели .glb, при этом никак не препятствует дальнейшему использованию приложения после закрытия окна;
2) Когда модель загрузилась, при попытке покрутить модель по ЛКМ - вращение не работает. Работатет только ПКМ панорамирование и приближение;
3) Окно редактирования не сохраняет редактируемые данные объекта - после сохранения в этом же режиме и режиме просмотра и при нажатии на тот же объект данные становятся чистыми как будто я ничего не сохранял
4) Логика сохранения и загрузки данных должна работать таким образом, что при нажатии кнопки "Загрузить данные" мы подгружаем и саму модель с которой работали, и её данные. Также и с кнопкой "Сохранить данные" - мы сохраняем и модель, и её данные. Это нужно для мгновенной подгрузки модели и её данных без лишних действий
Нужно продумать как исправить все недочёты и подробно описать весь процесс исправления от и до - почему, как, зачем и т.д.

## Ответ 3

Разбор и исправление четырёх проблем

Отлично, что приложение запустилось! Все четыре проблемы — реальные баги в моём первоначальном коде, а не ваша ошибка. Разберём каждую по порядку, с объяснением причины и точным решением.

Проблема 1. `dirLight.shadow.updateProjectionMatrix is not a function`

Причина

Это моя ошибка в API Three.js. Я вызывал метод `updateProjectionMatrix()` на объекте `shadow`, но у `DirectionalLightShadow` такого метода нет. `Метод updateProjectionMatrix()` находится у камеры тени, которая является вложенным объектом: `dirLight.shadow.camera`.

Если открыть документацию Three.js:

- `DirectionalLight` → свойство `.shadow` (тип `DirectionalLightShadow`)

- `DirectionalLightShadow` → свойство `.camera` (тип `OrthographicCamera`)

- `OrthographicCamera` → метод `.updateProjectionMatrix()`

Вызов напрямую `dirLight.shadow.updateProjectionMatrix()` приводит к ошибке, потому что у `shadow` нет такого метода — он есть только у `shadow.camera`.

Дополнительно, код выполнялся внутри `try/catch` внутри `loader.parse(...)`, но ошибка не пробрасывалась, потому что она возникала синхронно после успешного парсинга модели. Поэтому модель фактически добавлялась в сцену, а исключение вылетало позже — что и объясняет, почему приложение продолжало работать после закрытия сообщения.

Что исправить

В файле `wwwroot/js/threeScene.js` в функции `addModelToScene` замените:

```js
dirLight.shadow.updateProjectionMatrix();  // ❌ неверно
```

на:

```js
dirLight.shadow.camera.updateProjectionMatrix();  // ✅ верно
```

Дополнительно рекомендую обернуть весь блок настройки тени в try/catch, чтобы в будущем ошибки в этом месте не всплывали как модальные окна:

```js
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
```

Проблема 2. Вращение модели по ЛКМ не работает

Причина

Это следствие моего первоначального решения: я отключил ЛКМ для OrbitControls, чтобы зарезервировать её под выбор объектов:

```js
controls.mouseButtons = {
    LEFT: null,                    // ← ЛКМ отключена для вращения
    MIDDLE: THREE.MOUSE.ROTATE,
    RIGHT: THREE.MOUSE.PAN
};
```

Логика была: «ЛКМ выбирает объект, СКМ вращает». Но пользователь ожидает, что ЛКМ вращает — это стандарт для большинства 3D-приложений (Blender, SketchUp, Three.js Editor).
Правильное решение: комбинация вращения ЛКМ и выбора объекта через порог движения

Нельзя просто так включить `LEFT: THREE.MOUSE.ROTATE` — тогда OrbitControls перехватит `click` и клик по объекту не сработает. Нужно различать клик и drag:

- Если мышь нажата и отпущена без значительного смещения — это клик → raycast → выбор объекта.

- Если мышь сдвинулась больше порога (например, 5 px) — это вращение → ничего не делаем.

Так делают практически все 3D-редакторы.

Что исправить в `threeScene.js`

2.1. Изменить настройки OrbitControls:

```js
const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.dampingFactor = 0.05;
controls.screenSpacePanning = true;

// ЛКМ — вращение, СКМ — приближение, ПКМ — панорама
// В Blender-стиле: СКМ — вращение, Shift+СКМ — панорама.
// Но так как пользователи WPF-приложений ожидают ЛКМ-вращение — идём им навстречу.
controls.mouseButtons = {
    LEFT: THREE.MOUSE.ROTATE,
    MIDDLE: THREE.MOUSE.DOLLY,
    RIGHT: THREE.MOUSE.PAN
};
```
    Примечание: Если хотите сохранить истинный Blender-стиль (СКМ — вращение, ЛКМ — выбор без движения), это тоже возможно, но потребует обратной логики. Ниже я покажу универсальный подход.

2.2. Заменить старый `onClick` на пару `pointerdown` + `pointerup` с порогом:

Удалите старый обработчик `renderer.domElement.addEventListener("click", onClick);` и весь метод `onClick`. Вместо него добавьте:

```js
// ======================== ОБРАБОТКА КЛИКА С ПОРОГОМ ========================
let pointerDownInfo = null;
const CLICK_THRESHOLD_PX = 5; // если мышь сместилась меньше — считаем кликом

renderer.domElement.addEventListener("pointerdown", (e) => {
    if (e.button === 0) { // только ЛКМ
        pointerDownInfo = { x: e.clientX, y: e.clientY };
    }
});

renderer.domElement.addEventListener("pointerup", (e) => {
    if (e.button !== 0 || !pointerDownInfo) return;

    const dx = e.clientX - pointerDownInfo.x;
    const dy = e.clientY - pointerDownInfo.y;
    const distance = Math.sqrt(dx * dx + dy * dy);
    pointerDownInfo = null;

    // Если мышь сместилась меньше порога — это клик, а не вращение
    if (distance < CLICK_THRESHOLD_PX) {
        handleObjectClick(e);
    }
    // Иначе — было вращение, ничего не делаем
});

function handleObjectClick(event) {
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
```

Почему это работает: OrbitControls подписан на `pointerdown`/`pointermove`/`pointerup` и реагирует только когда идёт движение. Порог в 5 px — это эмпирически проверенное значение: достаточно, чтобы отсечь дрожание руки, но не мешать интуитивному клику.

Проблема 3. Данные объекта не сохраняются

Причина — рассинхрон аргументов между C# и JS

Найдите в `threeScene.js` функцию `updateObjectData`:

```js
window.updateObjectData = function (objectId, data) {
    if (data) {
        objectDataMap.set(objectId, data);
    } else {
        objectDataMap.delete(objectId);
    }
};
```

Функция ожидает два аргумента: `objectId` и `data`. А теперь посмотрите на вызов из C# в `MainWindow.xaml.cs`:

```csharp
await _bridge.CallWithJsonAsync("updateObjectData",
    new { objectId = _selectedObjectData.Id, data = _selectedObjectData });
```

`CallWithJsonAsync` сериализует один объект и передаёт его как один аргумент:

```js
updateObjectData({"objectId":"...", "data":{...}});
```

В итоге в JS приходит `objectId = {objectId:..., data:...}` (объект), а `data = undefined`. Функция сохраняет в карту `objectDataMap` мусорный ключ, а `data` не пишется.

В результате: при следующем клике по этому же объекту JS ищет в `objectDataMap` по правильному `objectId`, находит `null`, отправляет `data: null` в C#, а C# создаёт пустой объект заново. Все ваши правки теряются.

Что исправить

3.1. Привести JS-функцию к приёму одного payload-объекта:

```js
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
```

3.2. Убедиться, что все вызовы в C# передают именно `{objectId, data}`:

Проверьте, что в `HandleObjectSelected` и `CloseEditMenu_Click` используется одна и та же форма. У вас уже есть `new { objectId, data }` — этого достаточно, менять не надо.

3.3. Проблема с нестабильными ID мешей

Дополнительный источник проблемы: в `addModelToScene` я использовал:

```js
child.userData.objectId = `mesh_${meshIndex}_${child.uuid.substring(0, 8)}`;
```

`child.uuid` — случайный UUID, генерируется заново при каждой загрузке. То есть при перезапуске приложения даже одна и та же модель получит другие ID мешей, и сохранённые данные не найдут своих адресатов.

Исправление — стабильные ID из имён Blender или по индексу:

```js
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

    // ... остальная логика
}
```
    Важно: если в Blender вы даёте объектам осмысленные имена (`Wall_01`, `Roof_Main`, `Window_03`), они станут ID в вашей JSON-базе. Это удобно для отладки и делает данные переносимыми между сессиями. Если имена не заданы (по умолчанию Blender называет всё `Cube.001`, `Object_2` и т.п.), используется индекс — тоже стабильно.

3.4. После сохранения — обязательно перечитать данные из C# в JS

В `CloseEditMenu_Click` после `CallWithJsonAsync("updateObjectData", ...)` добавьте явное перечитывание всей карты — так надёжнее:

```csharp
private async void CloseEditMenu_Click(object sender, RoutedEventArgs e)
{
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
```

`setObjectData` в JS полностью перестраивает карту — это устраняет любые рассинхроны.

Проблема 4. Загрузка/сохранение модели вместе с данными
Текущая ситуация

- `SaveData_Click` сохраняет JSON (камера + объекты), но модель — только её абсолютный путь в `_sceneConfig.ModelFilePath`.

- `LoadData_Click` загружает JSON, восстанавливает камеру, но не загружает модель — файла с абсолютным путём может уже не быть, а переносить приложение на другой диск без модели — бессмысленно.

Правильная архитектура

Нужно обращаться с моделями так же, как с прикреплёнными файлами:

1. **При импорте** — копировать `.glb` в папку `Models/` рядом с `.exe`.

2. **В JSON** — хранить относительный путь (`Models/имя_файла.glb`).

3. **При загрузке данных** — если путь указан и файл существует, загружать модель автоматически.

4. **При сохранении данных** — сохранять конфигурацию без перезаписи модели (модель уже скопирована).

Пошаговое исправление

4.1. Добавить метод в `Services/FileService.cs`:

```csharp
/// <summary>
/// Сохранение 3D-модели во внутреннюю папку Models.
/// Возвращает относительный путь (например, "Models/abc123_factory.glb").
/// </summary>
public async Task<string> SaveModelAsync(Stream stream, string originalFileName)
{
    var modelsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
    if (!Directory.Exists(modelsFolder))
        Directory.CreateDirectory(modelsFolder);

    var extension = Path.GetExtension(originalFileName);
    var baseName = Path.GetFileNameWithoutExtension(originalFileName);
    // Уникальное имя, чтобы не перезаписывать одинаковые файлы
    var uniqueName = $"{baseName}_{Guid.NewGuid().ToString()[..8]}{extension}";
    var filePath = Path.Combine(modelsFolder, uniqueName);

    using var output = File.Create(filePath);
    await stream.CopyToAsync(output);

    return Path.Combine("Models", uniqueName);
}

/// <summary>
/// Получение абсолютного пути к модели по относительному.
/// </summary>
public string GetModelAbsolutePath(string relativePath)
{
    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
}
```

4.2. Настроить второй виртуальный хост в `MainWindow.xaml.cs` для папки `Models/`

В методе `InitializeWebViewAsync` после создания виртуального хоста `app.local` добавьте:

```csharp
var modelsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
if (!Directory.Exists(modelsPath))
    Directory.CreateDirectory(modelsPath);

// Второй виртуальный хост для моделей — грузим .glb через URL
WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "models.local",
    modelsPath,
    CoreWebView2HostResourceAccessKind.Allow);
```

Теперь большие `.glb`-файлы можно загружать через `https://models.local/имя_файла.glb` — WebView2 сам отдаст их с правильным MIME-типом, и не будет ограничения на размер скрипта.

4.3. Переписать `LoadModel_Click` — копировать модель в `Models/` и грузить по URL

```csharp
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
```

4.4. Переписать `LoadData_Click` — теперь загружает и модель, и данные

```csharp
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
```

4.5. `SaveData_Click` — оставить практически как есть, но заменить Base64-логику

Так как модель уже скопирована в `Models/` при импорте, `SaveData_Click` вообще не должен работать с содержимым `.glb`. Он только сохраняет JSON:

```csharp
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
```

4.6. Обновить `threeScene.js` — `loadModelFromUrl` уже есть, но проверим

Убедитесь, что функция принимает URL и загружает модель. У вас в коде она уже есть — она работает через `GLTFLoader.load(url, ...)`. Ничего менять не надо, кроме стабильных ID (см. п. 3.3).

Итоговый чек-лист правок

| #	| Файл	| Что сделать |
|---|---|---|
| 1	| threeScene.js	| dirLight.shadow.updateProjectionMatrix() → dirLight.shadow.camera.updateProjectionMatrix() |
| 2	| threeScene.js	| controls.mouseButtons.LEFT = THREE.MOUSE.ROTATE | 
| 2	| threeScene.js	| Заменить onClick на pointerdown+pointerup с порогом 5 px |
| 3	| threeScene.js	| Заменить updateObjectData(objectId, data) на updateObjectData(payload) |
| 3	| threeScene.js	| Убрать child.uuid из ID мешей — использовать child.name или mesh_${index} |
| 4	| FileService.cs	| Добавить SaveModelAsync и GetModelAbsolutePath |
| 4	| MainWindow.xaml.cs	| Второй виртуальный хост models.local → папка Models/ |
| 4	| MainWindow.xaml.cs	| LoadModel_Click — копировать .glb в Models/, грузить по URL |
| 4	| MainWindow.xaml.cs	| LoadData_Click — автоматически грузить модель после конфига |
| 4	| MainWindow.xaml.cs	| SaveData_Click — только JSON, без модели |
| 3	| MainWindow.xaml.cs	| CloseEditMenu_Click — использовать setObjectData вместо updateObjectData |

После этих правок:

- Модели будут автоматически восстанавливаться при «Загрузить данные».

- Данные объектов будут стабильно сохраняться и находить свои меши по имени/индексу.

- Вращение по ЛКМ будет работать, не мешая выбору объекта.

- Ошибок в консоли Three.js при загрузке не будет.

Про importmap и актуальную версию Three.js

Вы хотите использовать свежую версию из официального репозитория. На момент написания актуальна ветка `r17x` (например, `r171`). Импортмапа будет выглядеть так:

```html
<script type="importmap">
{
    "imports": {
        "three": "./lib/three/build/three.module.js",
        "three/addons/": "./lib/three/examples/jsm/"
    }
}
</script>
```

Скопируйте из корня `three.js`:

```
node_modules/three/build/three.module.js        → wwwroot/lib/three/build/three.module.js
node_modules/three/examples/jsm/               → wwwroot/lib/three/examples/jsm/
```

Структуру `examples/jsm/` обязательно сохраняйте целиком — внутри `GLTFLoader.js` есть относительные импорты на соседние утилиты (`../utils/BufferGeometryUtils.js`), и если что-то не скопируете — модуль не загрузится.

Если предпочитаете CDN (быстрее в разработке, но требует интернета):

```html
<script type="importmap">
{
    "imports": {
        "three": "https://cdn.jsdelivr.net/npm/three@0.171.0/build/three.module.js",
        "three/addons/": "https://cdn.jsdelivr.net/npm/three@0.171.0/examples/jsm/"
    }
}
</script>
```

Номер версии сверяйте на github.com/mrdoob/three.js/releases — последняя стабильная на момент вашей сборки.

Проверить, что всё подключилось, можно через DevTools (F12 в WebView2): если в консоли нет ошибок вида «Failed to resolve module specifier "three"» — импортмапа работает.

## Сообщение 4

Отличная работа! Применил все исправления и дополнения - код рабочий, приложение работает исправно. 

### Немного саммари по тому, что я изменил и исправил:

1) Проблема 1: исправлена - сделал всё в точности как ты написал;
2) Вращение модели по ЛКМ: я так подумал, если у нас сейчас исправно функционирует СКМ (и это действительно так, по своей невнимательности не заметил), то новая логика с ЛКМ пока подождёт и я её не внёс в текущий код. Как это обыграть - есть идея, о ней читай ниже;
3) Данные объекта: теперь сохраняются вообще классно и работает так как надо - сделал всё в точности как ты написал;
4) Загрузка/сохранение модели вместе с данными: работает хорошо, только нужно учитывать что при загрузке другой модели старая модель и её данные сбиваются при попытке в таком случае нажать "Загрузить данные" - сделал всё в точности как ты написал;
5) importmap и актуальную версию Three.js закинул в lib и применил изменения в index.html для возможности локального запуска - сделал всё в точности как ты написал.

### Теперь хотел бы пройтись по моментам, которые я бы хотел дальше развить, внести изменения и дополнить:

1) Как я и упомянул из саммари второго пункта по поводу того, как можно обыграть новую логику с ЛКМ - помимо СКМ для вращения можно сделать метод для вращения по ЛКМ как опцию для выбора в верхнем меню - будут 2 опции вращения на выбор: вращение по ЛКМ и вращение по СКМ как сейчас, и режим СКМ будет по умолчанию;
2) Насчёт загрузки/сохранения модели вместе с данными - нужно продумать такой механизм работы, при котором может быть множество моделей, загружаемых в программу, и для каждой из них уже ранее могли быть настроены данные. Я думаю самый оптимальный способ - это модернизировать текущие механизмы загрузки новых моделей, загрузки сохранённых моделей и механизм сохранения: сделать систему проектов. Что это значит? При загрузке приложения как и сейчас - сцена будет пустая и по сути мы запустили пустой проект (это можно пометить элементом Textbox вверху панели инструментов там где режимы чтобы можно было изменять название проекта перед его сохранением). Далее на выбор 2 сценария - загрузить модель и сохранить новый проект, или загрузить уже существующий проект с настроенными окнами и сценой;
3) Для параметра "Изображение" как и для файла было бы оптимально тоже добавить ссылку на фото, чтобы можно было по ней кликнуть и в таком случае открылся бы системный просмотрщик картинок или другой, установленный по умолчанию для просмотра фото.

С учётом вышеописанного в саммари и в соответствии с теми моментами, которые я бы хотел развить, внести изменения и дополнить в текущем коде, я прошу тебя подробно описать весь процесс внесения изменений и дополнений согласно вышеописанным трём моментам от и до - почему, как, зачем и т.д. Заранее спасибо!

## Ответ 4

Развитие приложения: три новых фичи

Отлично, что база работает! Разберём все три задачи по порядку, с полным кодом и объяснениями. Начнём с самой простой (клик по фото), затем перейдём к опции вращения, и закончим самой крупной — системой проектов.
Задача 3 (самая простая). Клик по фото открывает системный просмотрщик
Причина текущего поведения

В `ShowViewPanel` вы создаёте `Image` для отображения картинки, но не вешаете на него никаких обработчиков. WPF-контрол `Image` по умолчанию не реагирует на клики — он просто рисует.

Решение

Нужно добавить на `Image`:

- `Cursor = Cursors.Hand` — визуальная подсказка, что элемент кликабелен;

- `ToolTip`— пояснение;

- `MouseDown` — обработчик, который вызовет `FileService.OpenFile` с относительным путём к файлу. `Process.Start` с `UseShellExecute = true` откроет файл через системное приложение, ассоциированное с расширением `.jpg/.png/.bmp` (обычно это «Фотографии» или «Просмотр фотографий Windows»).

Правка в `MainWindow.xaml.cs` — метод `ShowViewPanel`

Найдите блок:

```csharp
if (field.Type == "image" && field.FilePaths.Count > 0)
{
    var img = new Image { MaxWidth = 200, MaxHeight = 200, ... };
    // ...
    valueElement = img;
}
```

Замените на:

```csharp
if (field.Type == "image" && field.FilePaths.Count > 0)
{
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
}
```

Почему именно `MouseDown`, а не `MouseLeftButtonUp`?

`Image` — не `Button`, у него нет встроенного `Click`. `MouseDown` срабатывает сразу при нажатии ЛКМ по элементу и не требует Capture/release логики. Этого достаточно для нашего сценария.
Дополнительное улучшение: несколько изображений — вертикальным списком с превью

Если у параметра типа «Фото» прикреплено несколько изображений, лучше показать их все. Сейчас код отображает только `field.FilePaths[0]`. Давайте это исправим:

```csharp
if (field.Type == "image" && field.FilePaths.Count > 0)
{
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
}
```

Задача 1. Опция вращения ЛКМ / СКМ
Идея

- По умолчанию — **СКМ** (Blender-стиль): `СКМ` вращает, `ЛКМ` выбирает объекты.

- Опционально — **ЛКМ**: `ЛКМ` вращает, а выбор объекта работает через «клик без движения» (порог 5 px).

Пользователь переключает режим через `ComboBox` в верхней панели. Выбор сохраняется в JSON проекта и восстанавливается при загрузке.

Почему не сделать оба режима одновременно?

Потому что `OrbitControls` не умеет различать «клик» и «drag» — если `LEFT: THREE.MOUSE.ROTATE`, то он начнёт вращать с первого пикселя движения мыши. Значит, надо либо перехватывать клик сами (через порог), либо отключать ЛКМ для `OrbitControls`. Мы уже решали эту задачу для случая с включённым `LEFT: ROTATE` — используем тот же подход, но применяем его только в режиме ЛКМ.

1.1. Правки в `threeScene.js`

Шаг A. Заменить блок `controls.mouseButtons`:

```js
// ======================== КАМЕРА И НАВИГАЦИЯ ========================
const controls = new OrbitControls(camera, renderer.domElement);
controls.enableDamping = true;
controls.dampingFactor = 0.05;
controls.screenSpacePanning = true;

// Начальная настройка — СКМ-режим (Blender-стиль)
let rotationMode = "skm";

function applyRotationMode(mode) {
    rotationMode = mode;
    if (mode === "lkm") {
        // ЛКМ вращает, СКМ зуммирует, ПКМ панорама
        controls.mouseButtons = {
            LEFT: THREE.MOUSE.ROTATE,
            MIDDLE: THREE.MOUSE.DOLLY,
            RIGHT: THREE.MOUSE.PAN
        };
    } else {
        // СКМ вращает, ЛКМ отдана под выбор объектов, ПКМ панорама
        controls.mouseButtons = {
            LEFT: null,
            MIDDLE: THREE.MOUSE.ROTATE,
            RIGHT: THREE.MOUSE.PAN
        };
    }
    controls.update();
    console.log("Режим вращения:", mode);
}

applyRotationMode("skm"); // по умолчанию
```

Шаг B. Заменить старый `onClick` на `pointerdown`/`pointerup` с порогом:

Удалите:

```js
renderer.domElement.addEventListener("click", onClick);
function onClick(event) { /* ... */ }
```

Вставьте вместо этого:

```js
// ======================== КЛИК С ПОРОГОМ ДВИЖЕНИЯ ========================
let pointerDownInfo = null;
const CLICK_THRESHOLD_PX = 5; // если мышь сместилась меньше — это клик, а не drag

renderer.domElement.addEventListener("pointerdown", (e) => {
    if (e.button === 0) { // только ЛКМ
        pointerDownInfo = { x: e.clientX, y: e.clientY };
    }
});

renderer.domElement.addEventListener("pointerup", (e) => {
    if (e.button !== 0 || !pointerDownInfo) return;

    const dx = e.clientX - pointerDownInfo.x;
    const dy = e.clientY - pointerDownInfo.y;
    const distance = Math.sqrt(dx * dx + dy * dy);
    pointerDownInfo = null;

    // Клик = малое смещение
    if (distance < CLICK_THRESHOLD_PX) {
        handleObjectClick(e);
    }
});

function handleObjectClick(event) {
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
```

Шаг C. Добавить экспорт функции установки режима:

```js
/**
 * Установка режима вращения: "lkm" или "skm".
 */
window.setRotationMode = function (mode) {
    applyRotationMode(mode);
};
```

1.2. Правки в `MainWindow.xaml`

Найдите панель управления (`Border Grid.Row="0"`) и добавьте `ComboBox` в правую часть, до кнопок сохранения. Например:

```xml
<StackPanel Orientation="Horizontal" Grid.Column="1">
    <!-- Выбор режима вращения -->
    <TextBlock Text="Вращение:"
               Foreground="White"
               VerticalAlignment="Center"
               Margin="0,0,6,0"/>
    <ComboBox x:Name="RotationModeCombo"
              SelectedIndex="1"
              Width="80"
              Margin="0,0,15,0"
              SelectionChanged="RotationMode_Changed">
        <ComboBoxItem Content="ЛКМ" Tag="lkm"/>
        <ComboBoxItem Content="СКМ" Tag="skm"/>
    </ComboBox>

    <!-- Кнопки -->
    <Button Content="📂 Загрузить модель" .../>
    ...
</StackPanel>
```
    Почему `SelectedIndex="1"`? СКМ — режим по умолчанию, а в списке он второй (индекс 1). Это гарантирует, что при старте выбран СКМ.

1.3. Правки в `MainWindow.xaml.cs`

Добавьте обработчик:

```csharp
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
```

1.4. Сохранение предпочтения в JSON

Добавьте в `Models/SceneConfig.cs`:

```csharp
[JsonPropertyName("rotationMode")]
public string RotationMode { get; set; } = "skm";
```

Теперь при сохранении проекта (`SaveData_Click`) режим вращения уйдёт в JSON. При загрузке проекта — восстановим (об этом в задаче 2).

1.5. Восстановление режима при загрузке проекта

В `LoadData_Click` (или позже, в `OpenProject`) добавьте:

```csharp
if (!string.IsNullOrEmpty(_sceneConfig.RotationMode))
{
    var mode = _sceneConfig.RotationMode;
    await _bridge.CallAsync("setRotationMode", mode);

    // Синхронизируем UI
    foreach (ComboBoxItem item in RotationModeCombo.Items)
    {
        if (item.Tag as string == mode)
        {
            RotationModeCombo.SelectedItem = item;
            break;
        }
    }
}
```
    Внимание: при программной установке SelectedItem сработает SelectionChanged и повторно вызовет setRotationMode — это безвредно, но если хочется избежать лишнего вызова, можно временно снять обработчик через флаг.

Задача 2. Система проектов

Это самая крупная задача, но она даёт качественный скачок — приложение становится многодокументным. Разберём архитектуру.

Концепция

```
NotaVerseBIM.exe
├── Data/
│   └── current_project.txt        ← ID последнего открытого проекта
├── Models/                        ← ВСЕ .glb-модели (общие для всех проектов)
│   ├── factory_abc123.glb
│   └── house_def456.glb
├── AttachedFiles/                 ← ВСЕ прикреплённые файлы (общие)
│   ├── photo_xyz.jpg
│   └── doc_uvw.pdf
└── Projects/
    ├── aaaa-bbbb-cccc.json        ← Проект №1
    ├── dddd-eeee-ffff.json        ← Проект №2
    └── gggg-hhhh-iiii.json        ← Проект №3
```

Ключевые принципы:

- Проект = JSON-файл с именем, конфигурацией камеры, списком объектов, ссылкой на модель и режимом вращения.

- Модель и файлы физически лежат рядом с `.exe` в общих папках и referenced по относительным путям. Один и тот же `.glb` может использоваться в нескольких проектах — не дублируется.

- `current_project.txt` — помнит последний открытый проект, чтобы при следующем запуске автоматически его подхватить (опционально).

2.1. Расширение модели `SceneConfig`

`Models/SceneConfig.cs`:

```csharp
using System.Text.Json.Serialization;

namespace NotaVerseBIM.WebView.Models
{
    public class SceneConfig
    {
        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = "Новый проект";

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("rotationMode")]
        public string RotationMode { get; set; } = "skm";

        [JsonPropertyName("modelFilePath")]
        public string ModelFilePath { get; set; } = string.Empty;

        [JsonPropertyName("camera")]
        public CameraConfig Camera { get; set; } = new();

        [JsonPropertyName("objects")]
        public List<SceneObjectData> Objects { get; set; } = new();
    }

    public class CameraConfig
    {
        [JsonPropertyName("positionX")] public double PositionX { get; set; } = 5;
        [JsonPropertyName("positionY")] public double PositionY { get; set; } = 5;
        [JsonPropertyName("positionZ")] public double PositionZ { get; set; } = 5;
        [JsonPropertyName("targetX")]   public double TargetX   { get; set; } = 0;
        [JsonPropertyName("targetY")]   public double TargetY   { get; set; } = 0;
        [JsonPropertyName("targetZ")]   public double TargetZ   { get; set; } = 0;
    }
}
```

2.2. Переработка DataService

`Services/DataService.cs` — полностью заменить:

```csharp
using System.IO;
using System.Text.Json;
using NotaVerseBIM.WebView.Models;

namespace NotaVerseBIM.WebView.Services
{
    public class ProjectInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ModelFilePath { get; set; }

        public string DisplayName =>
            $"{Name} — {ModifiedAt:dd.MM.yyyy HH:mm}";

        public override string ToString() => DisplayName;
    }

    public class DataService
    {
        private readonly string _projectsFolder;
        private readonly string _currentProjectPointer;

        public DataService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _projectsFolder = Path.Combine(baseDir, "Projects");
            Directory.CreateDirectory(_projectsFolder);

            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            _currentProjectPointer = Path.Combine(dataDir, "current_project.txt");
        }

        public string ProjectsFolder => _projectsFolder;

        // ---------- Проекты ----------

        /// <summary>Загрузка проекта по ID.</summary>
        public async Task<SceneConfig> LoadProjectAsync(string projectId)
        {
            var path = Path.Combine(_projectsFolder, $"{projectId}.json");
            if (!File.Exists(path))
                return new SceneConfig { ProjectId = projectId };

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<SceneConfig>(json)
                   ?? new SceneConfig { ProjectId = projectId };
        }

        /// <summary>Сохранение проекта (создаёт или перезаписывает).</summary>
        public async Task SaveProjectAsync(SceneConfig config)
        {
            config.ModifiedAt = DateTime.Now;

            var path = Path.Combine(_projectsFolder, $"{config.ProjectId}.json");
            var json = JsonSerializer.Serialize(config,
                new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }

        /// <summary>Список всех сохранённых проектов.</summary>
        public async Task<List<ProjectInfo>> ListProjectsAsync()
        {
            var result = new List<ProjectInfo>();
            if (!Directory.Exists(_projectsFolder)) return result;

            foreach (var file in Directory.GetFiles(_projectsFolder, "*.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var config = JsonSerializer.Deserialize<SceneConfig>(json);
                    if (config != null)
                    {
                        result.Add(new ProjectInfo
                        {
                            Id = config.ProjectId,
                            Name = string.IsNullOrWhiteSpace(config.ProjectName)
                                ? "Без имени"
                                : config.ProjectName,
                            ModifiedAt = config.ModifiedAt,
                            ModelFilePath = config.ModelFilePath
                        });
                    }
                }
                catch { /* битый файл — пропускаем */ }
            }

            return result.OrderByDescending(p => p.ModifiedAt).ToList();
        }

        /// <summary>Удаление проекта по ID.</summary>
        public void DeleteProject(string projectId)
        {
            var path = Path.Combine(_projectsFolder, $"{projectId}.json");
            if (File.Exists(path)) File.Delete(path);
        }

        // ---------- Указатель на текущий проект ----------

        public async Task SaveCurrentProjectPointerAsync(string projectId)
        {
            await File.WriteAllTextAsync(_currentProjectPointer, projectId);
        }

        public async Task<string> GetCurrentProjectPointerAsync()
        {
            if (!File.Exists(_currentProjectPointer)) return null;
            var id = await File.ReadAllTextAsync(_currentProjectPointer);
            return string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        }
    }
}
```

2.3. Обновление XAML — панель проектов

Переделаем верхнюю панель. Сейчас там режимы и кнопки. Добавим слева название проекта и кнопки управления проектами.

Полностью замените блок `<Border Grid.Row="0" ...>`:

```xml
<Border Grid.Row="0" Background="#C0000000" Padding="10">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>   <!-- Имя проекта -->
            <ColumnDefinition Width="Auto"/>   <!-- Режимы -->
            <ColumnDefinition Width="*"/>      <!-- Растяжка -->
            <ColumnDefinition Width="Auto"/>   <!-- Проект-кнопки -->
            <ColumnDefinition Width="Auto"/>   <!-- Режим вращения -->
            <ColumnDefinition Width="Auto"/>   <!-- Файл-кнопки -->
        </Grid.ColumnDefinitions>

        <!-- Имя проекта -->
        <StackPanel Grid.Column="0" Orientation="Horizontal" Margin="0,0,20,0">
            <TextBlock Text="📁"
                       Foreground="White"
                       FontSize="16"
                       VerticalAlignment="Center"
                       Margin="0,0,6,0"/>
            <TextBox x:Name="TxtProjectName"
                     Width="180"
                     Text="Новый проект"
                     VerticalContentAlignment="Center"
                     Background="#40FFFFFF"
                     Foreground="White"
                     BorderThickness="0"
                     Padding="6,3"
                     FontWeight="Bold"
                     ToolTip="Название текущего проекта"/>
        </StackPanel>

        <!-- Режимы работы -->
        <StackPanel Grid.Column="1" Orientation="Horizontal">
            <RadioButton x:Name="RadioView"
                         Content="👁 Просмотр"
                         IsChecked="True"
                         Foreground="White"
                         Checked="Mode_Changed"
                         Margin="0,0,20,0"/>
            <RadioButton x:Name="RadioEdit"
                         Content="✎ Инспектор"
                         Foreground="White"
                         Checked="Mode_Changed"
                         Margin="0,0,20,0"/>
        </StackPanel>

        <!-- Кнопки проектов -->
        <StackPanel Grid.Column="3" Orientation="Horizontal" Margin="0,0,20,0">
            <Button Content="🆕 Новый"
                    Click="NewProject_Click"
                    Padding="10,5" Margin="0,0,6,0"
                    Background="#FF34495E" Foreground="White"
                    BorderThickness="0" FontWeight="Bold"
                    ToolTip="Создать новый проект"/>

            <Button Content="📂 Открыть проект"
                    Click="OpenProject_Click"
                    Padding="10,5" Margin="0,0,6,0"
                    Background="#FF2980B9" Foreground="White"
                    BorderThickness="0" FontWeight="Bold"
                    ToolTip="Открыть сохранённый проект"/>

            <Button Content="💾 Сохранить проект"
                    Click="SaveProject_Click"
                    Padding="10,5"
                    Background="#FF27AE60" Foreground="White"
                    BorderThickness="0" FontWeight="Bold"
                    ToolTip="Сохранить текущий проект"/>
        </StackPanel>

        <!-- Режим вращения -->
        <StackPanel Grid.Column="4" Orientation="Horizontal" Margin="0,0,20,0">
            <TextBlock Text="🎮 Вращение:"
                       Foreground="White"
                       VerticalAlignment="Center"
                       Margin="0,0,6,0"/>
            <ComboBox x:Name="RotationModeCombo"
                      SelectedIndex="1"
                      Width="80"
                      VerticalAlignment="Center"
                      SelectionChanged="RotationMode_Changed">
                <ComboBoxItem Content="ЛКМ" Tag="lkm"/>
                <ComboBoxItem Content="СКМ" Tag="skm"/>
            </ComboBox>
        </StackPanel>

        <!-- Кнопки работы с моделью -->
        <StackPanel Grid.Column="5" Orientation="Horizontal">
            <Button Content="📂 Загрузить модель"
                    Click="LoadModel_Click"
                    Padding="10,5"
                    Background="#FF8E44AD" Foreground="White"
                    BorderThickness="0" FontWeight="Bold"
                    ToolTip="Импортировать .glb в текущий проект"/>
        </StackPanel>
    </Grid>
</Border>
```
    Кнопки «Сохранить данные» и «Загрузить данные» убираются. Их роль теперь выполняют «Сохранить проект» и «Открыть проект». Всё, что было в JSON — теперь в проекте.

2.4. Модальное окно выбора проекта

Добавьте в конец `MainWindow.xaml`, перед закрывающим `</Grid>` главного контейнера:

```xml
<!-- ==================== ОКНО ВЫБОРА ПРОЕКТА ==================== -->
<Border x:Name="OpenProjectPanel"
        Visibility="Collapsed"
        Background="White"
        BorderBrush="#2980B9"
        BorderThickness="2"
        CornerRadius="5"
        Width="500"
        MaxHeight="500"
        HorizontalAlignment="Center"
        VerticalAlignment="Center"
        Padding="15">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0"
                   Text="📂 Открыть проект"
                   FontSize="18"
                   FontWeight="Bold"
                   Foreground="#2980B9"
                   Margin="0,0,0,10"/>

        <ListBox x:Name="ProjectsList"
                 Grid.Row="1"
                 MinHeight="200"
                 DisplayMemberPath="DisplayName"/>

        <StackPanel Grid.Row="2"
                    Orientation="Horizontal"
                    HorizontalAlignment="Right"
                    Margin="0,10,0,0">
            <Button Content="Удалить"
                    Click="DeleteProject_Click"
                    Padding="15,6" Margin="0,0,6,0"
                    Background="#FFE74C3C" Foreground="White"
                    BorderThickness="0" FontWeight="Bold"/>
            <Button Content="Отмена"
                    Click="CloseOpenProject_Click"
                    Padding="15,6" Margin="0,0,6,0"/>
            <Button Content="Открыть"
                    Click="ConfirmOpenProject_Click"
                    Padding="15,6"
                    Background="#FF2980B9" Foreground="White"
                    BorderThickness="0" FontWeight="Bold"/>
        </StackPanel>
    </Grid>
</Border>
```

2.5. Логика проектов в `MainWindow.xaml.cs`

Шаг A. Поля состояния — добавьте:

```csharp
// ==================== СОСТОЯНИЕ ПРОЕКТА ====================
private SceneConfig _currentProject = new();
private bool _isProjectDirty = false;   // есть ли несохранённые изменения
```

Шаг B. Инициализация — измените `InitializeWebViewAsync`:

```csharp
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
```

Шаг C. `OnNavigationCompleted` — загрузка проекта после готовности JS:

```csharp
private async void OnNavigationCompleted(
    object sender, CoreWebView2NavigationCompletedEventArgs e)
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
```

Шаг D. Хелперы:

```csharp
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
```

Шаг E. Обработчики кнопок:

```csharp
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
```

Шаг F. Обновлённый `LoadModel_Click` — сохраняет модель и помечает проект «грязным»:

```csharp
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
```

Шаг G. Обновлённый `CloseEditMenu_Click` — устанавливает флаг «грязный»:

```csharp
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
```

Шаг H. Отслеживание изменения названия проекта:

Подпишитесь в конструкторе на `TxtProjectName.TextChanged`:

```csharp
public MainWindow()
{
    InitializeComponent();
    _bridge = new WebViewBridge(WebView);

    TxtProjectName.TextChanged += (s, e) => _isProjectDirty = true;

    InitializeWebViewAsync();
}
```

Шаг I. Перехват закрытия приложения — предложение сохранить:

```csharp
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
```

Шаг J. Удалите старые `SaveData_Click` и `LoadData_Click` — они больше не нужны, их роль выполняют `SaveProject_Click` / `ConfirmOpenProject_Click`.

2.6. Правка `threeScene.js` — добавить `clearModel()` в window

Функция `clearModel()` уже есть, но она не экспортирована наружу. Добавьте в конец файла:

```js
window.clearModel = clearModel;
```

Иначе `ExecuteScriptAsync("clearModel();")` из C# не найдёт функцию.
2.7. Как теперь работает жизненный цикл

Сценарий 1. Первый запуск.

- Приложение стартует с пустым проектом «Новый проект».

- Пользователь загружает .glb → файл копируется в `Models/`, путь пишется в проект.

- Настраивает объекты в инспекторе.

- Нажимает «Сохранить проект» → JSON падает в `Projects/{guid}.json`, ID пишется в `Data/current_project.txt`.

Сценарий 2. Повторный запуск.

- Приложение стартует, читает `current_project.txt`.

- Загружает проект, модель, объекты, камеру, режим вращения.

- Всё как было в прошлый раз.

Сценарий 3. Несколько проектов.

- «Новый проект» → пустая сцена, работаем с другой моделью.

- «Сохранить проект» → это уже второй JSON в `Projects/`.

- «Открыть проект» → список всех проектов, клик — загружается выбранный.

Сценарий 4. Удаление.

- «Открыть проект» → выбрать → «Удалить» → JSON удаляется из `Projects/`.

- Физические файлы модели и вложений не удаляются — они могут использоваться в других проектах.

# Полный чек-лист финальных правок

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