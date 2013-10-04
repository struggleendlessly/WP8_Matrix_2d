using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace SE_Matrix_2d_v_4
{
    public partial class MainPage : PhoneApplicationPage
    {
        /* ****************************** Свойства класса ****************************** */
        // Случайное число
        Random random = new Random();

        // Количество змеек после нажатия на экран в очереди
        int iteration = 5;

        // Количество одновременно появляющихся змеек после нажатия
        int countSimultaneously = 3;

        // Скорость смены символов
        int speedFrom = 20;
        int speedTo = 40;

        // Размер клетки для символа
        int addingSize = -6;

        // Итоговый размер шрифта
        int fontSize;

        // Минимальная и максимальная длина змейки
        int minLength = 10;
        int maxLength = 15;

        // Получаем расширение экрана
        double ScreenWidth = System.Windows.Application.Current.Host.Content.ActualWidth - 60;
        double ScreenHeight = System.Windows.Application.Current.Host.Content.ActualHeight - 100;
        
        // Коеффициент, отвечающий за количесвто ячеек и частично за размер шрифта
        int kolich = 30;

        // Размер шрифта задается по формуле kolich + addingFontSize.
        int addingFontSize = -2;

        // Количество смены символов в ячейке
        int countSymbol = 3;

        // Включить (false), выключить (true) матрицу
        bool flagOnOff = false;

        // Включить (false), выключить (true) "поворот экрана"
        bool turnOnOff = true;

        // Количество строк и столбцов
        int countWidth = 10;
        int countHeight = 10;

        // Словарь, в котором хранятся идентификаторы языка и соответствующие ему ASCII коды символов
        Dictionary<string, int[]> languages = new Dictionary<string, int[]>();

        // Задаю язык по-умолчанию
        string actualLanguage = "Русский";

        // Флаг, отвечающий за показывать (true) / не показывать (false) всплывающее окно (PopUp) при выборе языка
        bool flagShowLanguages = true;

        // Цвет фона матрицы, ARGB
        Dictionary<string, int> colorMatrixBackground = new Dictionary<string, int>();

        // Цвет первого символа, ARGB
        Dictionary<string, int> colorFirstSymbol = new Dictionary<string, int>();

        // Цвет градиента змейки от (второй символ) - до (последний символ), ARGB
        Dictionary<string, int> gradientFrom = new Dictionary<string, int>();
        Dictionary<string, int> gradientTo = new Dictionary<string, int>();
        
        /* ****************************** Методы класса ****************************** */
        // Конструктор
        public MainPage()
        {
            InitializeComponent();

            // Вызываем функцию настройки начальных значений цветов фона, символов и т.д.
            BeginColorSettings();

            // Инициализируем список доступных языков, а также соответствующие им ASCII коды символов
            ListLanguages();

            // Количество строк и столбцов
            this.countWidth = (int)Math.Round(ScreenWidth / (kolich + addingSize)) + Math.Abs(addingSize);
            this.countHeight = (int)Math.Round(ScreenHeight / (kolich + addingSize)) + 5 + Math.Abs(addingSize);

            // Создание сетки элементов, в которой будет сыпаться матрица
            CreateElement();

            // Подсвечиваем кнопку Вкл или Выкл,  зависит от флага
            if (this.flagOnOff)
            {
                Button_Stop.Background = new SolidColorBrush(Colors.Cyan);
                Button_Start.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                Button_Stop.Background = new SolidColorBrush(Colors.Black);
                Button_Start.Background = new SolidColorBrush(Colors.Cyan);
            }

            // Меняем цвет фона матрицы
            ChangeBackground();
        }

        // Создание сетки элементов, в которой будет сыпаться матрица
        public void CreateElement()
        {
            // Вычисляем тоговый размер шрифта для разметки / переразметки
            this.fontSize = kolich + addingFontSize + addingSize;

            // Создаем сетку ячеек
            for (int i = 0; i < countWidth; i++)
            {
                for (int j = 0; j < countHeight; j++)
                {
                    // Создаем TextBlock
                    TextBlock element = new TextBlock();

                    // Задаем имя элемента TextBlock
                    element.Name = "TB_" + i + "_" + j;

                    // Задаем начальный символ при инициализации сетки ячеек
                    // element.Text = char.ConvertFromUtf32(random.Next(0x4E00, 0x4FFF)); // Случайный символ из заданного диапазона
                    // element.Text = random.Next(0, 9).ToString(); // Случайным числом
                    element.Text = ""; // Пустота

                    // Задаем смещение каждого нового элемента TextBlock
                    // Также отвечает за разворот вертикальный / горизонтальный
                    int turnY = j * (kolich + addingSize);
                    int turnX = i * (kolich + addingSize);

                    // Включить (false), выключить (true) "поворот экрана"
                    if (turnOnOff)
                    {
                        // Вертикальное, стандартное расположение
                        element.Margin = new Thickness(turnX, turnY, 0, 0);
                    }
                    else
                    {
                        // Повернутое, горизонтальное расположение
                        element.Margin = new Thickness(turnY, turnX, 0, 0);
                    }

                    // Задаем цвет символа
                    element.Foreground = new SolidColorBrush(Colors.Green);

                    // Задаем размер шрифта
                    element.FontSize = fontSize;

                    // Добавляем созданный элемент в Grid
                    LayoutRootSecond.Children.Add(element);
                }
            }
        }

        // Событие при нажатии на эелемет Grid (на экран)
        private void Event_Grid_Tap_LayoutRoot(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Количество одновременно появляющихся змеек после нажатия
            for (int i = 0; i < countSimultaneously; i++)
            {
                // Останавливаем анимацию
                if (flagOnOff) break;

                Start();

                //Задержка между вызовами. Для красоты матрицы.
                Task.Delay(100);
            }
        }

        // Метод запуска змейки
        public async void Start()
        {
            int count;

            // Количество змеек после нажатия на экран в очереди
            for (count = 0; count < iteration; count++)
            {
                // Начало змейки по горизонтали случайным образом
                int ranX = random.Next(0, countWidth);

                // Начало змейки по вертикали случайным образом
                int ranY = random.Next(-5, countHeight - 1);

                // Длина змейки случайным образом
                int length = random.Next(minLength, maxLength);

                // Скорость смены символов в змейке случайным образом
                int time = random.Next(speedFrom, speedTo);

                await Task.Delay(1);
                
                //Обработка змейки
                await RandomElementQ_Async(ranX, ranY, length, time);
            }
        }

        // Определяю элемент, в котором нужно менять символы
        public async Task RandomElementQ_Async(int x, int y, int length, int timeOut)
        {
            // Словарь для хранения идентификаторов ячеек, которые вызывались на предыдущем этапе.
            Dictionary<int, TextBlock> dicElem = new Dictionary<int, TextBlock>();

            // Задаем цвет символов
            Dictionary<string, int> SymbolColor = new Dictionary<string, int>();

            // Счетчик, нужен для обработки случаев, когда не выполняется условие if ((y + i) < countHeight && (y + i) >= 0). Смотри на 4 строчки вниз.
            // Тоесть элемент в котором нужно менять символы выше или ниже нашей сетки (матрицы элементов).
            int count = 0;

            // Цикл формирует змейку заданной длины length
            for (int i = 0; i <= length; i++)
            {
                // Останавливаем анимацию
                if (flagOnOff) break;

                //Проверяем, что б змейка отображалась только в координатах, которые существуют в нашей сетке
                if ((y + i) < countHeight && (y + i) >= 0)
                {
                    // Формируем имя элемента, в котором будут меняться символы
                    string elementName = "TB_" + x + "_" + (y + i);

                    // Получаем элемент по его имени
                    object wantedNode = LayoutRoot.FindName(elementName);
                    TextBlock element = (TextBlock)wantedNode;

                    // Отправляем элемент в словарь, из которого он будет извлекаться для эффекта "падения" и "затухания" змейки
                    dicElem[count] = (element);

                    // Определяем коеффициент для подсчета яркости. Первый элемент(который падает) -  всега самый яркий, последний - самый темный.
                    // Отнимаем 1, потому, что последний элемент (когда к -  максимальное) в итоге получается больше 255 и становится ярким.
                    int A_Coefficient = (int)Math.Round((gradientFrom["A"] - 10) / (double)(count + 1)) - 1;
                    int R_Coefficient = (int)Math.Round((gradientFrom["R"] - gradientTo["R"]) / (double)(count + 1)) - 1;
                    int G_Coefficient = (int)Math.Round((gradientFrom["G"] - gradientTo["G"]) / (double)(count + 1)) - 1;
                    int B_Coefficient = (int)Math.Round((gradientFrom["B"] - gradientTo["B"]) / (double)(count + 1)) - 1;
                    //int greenCoefficient = (int)Math.Round(255 / (double)(count + 1)) - 1;

                    // Вызываем на прорисовку первый, самый яркий падающий элемент. Асинхронно.
                    // colorFirstSymbol["A"]. Цвет задается через настройку.                   
                    await Change(element, timeOut, colorFirstSymbol);

                    // Перебираем все  элементы, составляющие змейку на данном этапе. С каждым циклом она увеличивается, пока не достигнет нужной длины.
                    for (int k = 0; k <= i; k++)
                    {                       
                        // Останавливаем анимацию
                        if (flagOnOff) break;

                        // Если змейка начинаеися "выше" начальных координат (например, если y = -5)
                        if (dicElem.ContainsKey(k))
                        {
                            //Извлекаем элементы, которые должны следовать за самым ярким. Создаем эффект "затухания" цвета
                            TextBlock previousElement = dicElem[k];

                            // Вызываем извлеченные элементы
                            // (greenCoefficient * (k + 1)) - 20 Высчитываем яркость так, что б разница между первым и последним была на всех змейках одинаковая
                            // и равномерно распределялась независимо от ее длины(количества элементов)
                            SymbolColor["A"] = (gradientFrom["A"] - ((i - k) * A_Coefficient));
                            SymbolColor["R"] = (gradientFrom["R"] - ((i - k) * R_Coefficient));
                            SymbolColor["G"] = (gradientFrom["G"] - ((i - k) * G_Coefficient));
                            SymbolColor["B"] = (gradientFrom["B"] - ((i - k) * B_Coefficient));

                            Task dsvv = Change(previousElement, timeOut, SymbolColor);
                        }
                    }
                    count++;
                }
            }
        }

        // Метод изменения символов в заданном элеменете
        public async Task Change(TextBlock element, int timeOut, Dictionary<string, int> SymbolColor)
        {
            // Формируем нужный цвет с заданной яркостью
            SolidColorBrush NewColor = new SolidColorBrush(new Color()
            {
                A = (byte)(SymbolColor["A"]) /*Opacity*/,
                R = (byte)(SymbolColor["R"]) /*Red*/,
                G = (byte)(SymbolColor["G"]) /*Green*/,
                B = (byte)(SymbolColor["B"]) /*Blue*/
            });

            // При каждом "падении" на 1 клеточку равномерно "затухает"
            element.Foreground = NewColor;

            // Количество смены символов в каждой ячейке
            for (int i = 0; i < countSymbol; i++)
            {
                // Останавливаем анимацию
                if (flagOnOff) break;

                // Каждый раз разный символ из заданного диапазона
                element.Text = RandomActualSymbol();              

                // Размер шрифта
                element.FontSize = fontSize;

                // Скорость смены символов в ячейке
                await Task.Delay(timeOut);
            }
        }

        // Загрузка данных для элементов ViewModel
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        /* ****************************** События ****************************** */
        // Настройки. Скорость смены символов.
        private void Event_Button_Click_SpeedApplay(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_SppeedFrom в свойство класса speedFrom
            this.speedFrom = int.Parse(TextBox_SppeedFrom.Text.ToString());

            // Сохраняем значение их соответствующего TextBox TextBox_SppeedTo в свойство класса speedTo
            this.speedTo = int.Parse(TextBox_SppeedTo.Text.ToString());
        }

        // Настройки. Количество змеек в очереди
        private void Event_Button_Click_CountQueue(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_CountQueue в свойство класса iteration
            this.iteration = int.Parse(TextBox_CountQueue.Text.ToString());
        }

        // Настройки. Количество змеек за нажатие
        private void Event_Button_Click_СountSimultaneously(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_СountSimultaneously в свойство класса countSimultaneously
            this.countSimultaneously = int.Parse(TextBox_СountSimultaneously.Text.ToString());
        }

        // Настройки. Размер шрифта
        private void Event_Button_Click_FontSize(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_FontSize в свойство класса addingFontSize
            this.addingFontSize = int.Parse(TextBox_FontSize.Text.ToString());

            // Вычисляем тоговый размер шрифта для разметки / переразметки
            this.fontSize = kolich + addingFontSize + addingSize;
        }

        // Настройки. Количество смены символов в ячейке
        private void Event_Button_Click_CountSymbol(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_CountSymbol в свойство класса countSymbol
            this.countSymbol = int.Parse(TextBox_CountSymbol.Text.ToString());
        }

        // Настройки. Выключаем возможность анимирования змеек
        private void Event_Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            this.flagOnOff = true;

            // Если flagOnOff в true то подсвечиваем кнопку Stop
            if (this.flagOnOff)
            {
                Button_Stop.Background = new SolidColorBrush(Colors.Cyan);
                Button_Start.Background = new SolidColorBrush(Colors.Black);
            }
        }

        // Настройки. Включаем возможность анимирования змеек
        private void Event_Button_Click_Start(object sender, RoutedEventArgs e)
        {
            this.flagOnOff = false;

            // Если flagOnOff в false то подсвечиваем кнопку Start
            if (!this.flagOnOff)
            {
                Button_Stop.Background = new SolidColorBrush(Colors.Black);
                Button_Start.Background = new SolidColorBrush(Colors.Cyan);
            }
        }

        // Настройки. Очистка экрана
        private void Event_Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            // Перебираем сетку ячеек и устанавливаем в каждой ячейке как символ - пустоту
            for ( int i = 0; i < countWidth; i++)
            {
                for (int j = 0; j < countHeight; j++)
                {
                    // Формируем имя элемента, который будем очищать
                    string elementName = "TB_" + i + "_" + j;

                    // Получаем элемент по его имени
                    object wantedNode = LayoutRoot.FindName(elementName);
                    TextBlock element = (TextBlock)wantedNode;

                    // Очищаем значение
                    element.Text = "";
                }
            }
        }

        // Настройки. Размер клетки для символа
        private void Event_Button_Click_ElementSize(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_ElementSize в свойство класса addingSize
            this.addingSize = int.Parse(TextBox_ElementSize.Text.ToString());

            // Количество строк и столбцов. Возвращаем для вертикали
            this.countWidth = (int)Math.Round(ScreenWidth / (kolich + addingSize)) + Math.Abs(addingSize);
            this.countHeight = (int)Math.Round(ScreenHeight / (kolich + addingSize)) + 5 + Math.Abs(addingSize);

            // Останавливаем матрицу
            this.flagOnOff = true;

            // Удаляем матрицу (сами ячейки)
            LayoutRootSecond.Children.Clear();

            // Перерисовываем ячейки заново с новыми параметрами
            CreateElement();

            // Включаем матрицу
            this.flagOnOff = false;
        }

        // Настройки. Задаем максимальную и минимальную длину змейки
        private void Event_Button_Click_MaxLength(object sender, RoutedEventArgs e)
        {
            // Сохраняем значение их соответствующего TextBox TextBox_MinLength в свойство класса minLength
            this.minLength = int.Parse(TextBox_MinLength.Text.ToString());

            // Сохраняем значение их соответствующего TextBox TextBox_MaxLength в свойство класса maxLength
            this.maxLength = int.Parse(TextBox_MaxLength.Text.ToString());
        }

        // Настройки. Вериткальная / горизонтальная матрица
        private void Event_Button_Click_Turn(object sender, RoutedEventArgs e)
        {
            // Включить (true), выключить (false) "поворот экрана"
            if (turnOnOff)
            {
                // Сохраняем значение false в свойство класса turnOnOff
                this.turnOnOff = false;

                // Надпись на конопке с именем ToggleButton_Turn меняю на Горизонтально
                ToggleButton_Turn.Content = "Горизонтально";

                // Количество строк и столбцов. Инвертируем для горизонтали.
                this.countHeight = (int)Math.Round(ScreenWidth / (kolich + addingSize)) + 5 + Math.Abs(addingSize);
                this.countWidth = (int)Math.Round(ScreenHeight / (kolich + addingSize)) + 5 + Math.Abs(addingSize);
            }
            else
            {
                // Сохраняем значение true в свойство класса turnOnOff
                this.turnOnOff = true;

                // Надпись на конопке с именем ToggleButton_Turn меняю на Вертикально
                ToggleButton_Turn.Content = "Вертикально";

                // Количество строк и столбцов. Возвращаем для вертикали
                this.countWidth = (int)Math.Round(ScreenWidth / (kolich + addingSize)) + Math.Abs(addingSize);
                this.countHeight = (int)Math.Round(ScreenHeight / (kolich + addingSize)) + 5 + Math.Abs(addingSize);
            }

            // Останавливаем матрицу
            this.flagOnOff = true;

            // Удаляем матрицу (сами ячейки)
            LayoutRootSecond.Children.Clear();

            // Перерисовываем ячейки заново с новыми параметрами
            CreateElement();

            // Включаем матрицу
            this.flagOnOff = false;
        }

        // Заполняем словарь идентификаторами языка и соответствующие ему ASCII коды символов
        public void ListLanguages()
        {
            // Добавляем в словарь ключь -  название языка и значение -  массив, состоящий из ASCII кодов символов.
            languages.Add("Матрица", new int[] { 64, 127 });
            languages.Add("Китаский", new int[] { 19968, 20223 });
            languages.Add("Английский", new int[] { 64, 127 });
            languages.Add("Цифры", new int[] { 48, 57 });
            languages.Add("Случайные символы", new int[] { 0, 1000 });
            languages.Add("Русский", new int[] { 1040, 1103 });

            // Добавляем языки в всплывающую панель для возможности их выбора
            foreach (var language in languages)
            {
                // Создаю кнопку
                Button newLang = new Button();

                // Задаю надпись кнопки, соответствует языку
                newLang.Content = language.Key.ToString();

                // Горизонтальное выравнивание
                newLang.HorizontalAlignment = HorizontalAlignment.Stretch;

                // Толщина рамки
                newLang.BorderThickness = new Thickness(1);

                // Смещение
                newLang.Margin = new Thickness(0,0,0,0);

                // Событие, при нажатии на кнопку. Одно на все.
                newLang.Click += Event_Button_Click_SelectLanguageUpdate;

                // Добавляю созданую и настроенную кнопку в всплывающее окно
                StackPanel_ButtonDropDownSelectLanguage.Children.Add(newLang);
            }
        }

        // Вызываем эту функцию везде, где нужно показать случайный символ из выбранного языка.
        public string RandomActualSymbol()
        {
            // Получаем массив по ключу, содержащий ASCII коды символов языка, заданного в actualLanguage
            int[] sd = (languages[actualLanguage]);

            // Выбираем случайнфй символ в диапазоне от первого до последнего символа в заданом языке
            return char.ConvertFromUtf32(this.random.Next((int)sd.GetValue(0), (int)sd.GetValue(1)));
        }

        // Кнопка, в которой показывается текущее выбранное значение языка. при нажатии на нее всплывает меню для выбора другого языка.
        private void Event_Button_Click_SelectLanguage(object sender, RoutedEventArgs e)
        {
            // Показывать (true) / не показывать (false) всплывающее окно (PopUp) при выборе языка
            if (flagShowLanguages)
            {
                // Показать всплывающее окно
                Popup_ButtonDropDownSelectLanguage.IsOpen = true;
                flagShowLanguages = false;
            }
            else
            {
                // Скрыть всплывающее окно
                Popup_ButtonDropDownSelectLanguage.IsOpen = false;
                flagShowLanguages = true;
            }
        }

        // Всплывающее меню выбора языка.
        private void Event_Button_Click_SelectLanguageUpdate(object sender, RoutedEventArgs e)
        {
            // Если нажата кнопка выбора языка, но другой язык не выбран, то при повторном нажатии меню свернется.
            if (!flagShowLanguages)
            {
                // Скрыть всплывающее окно
                Popup_ButtonDropDownSelectLanguage.IsOpen = false;
                flagShowLanguages = true;
            }

            // Получаем название кнопки, которую нажали
            string newLanguagr = (sender as Button).Content.ToString();

            // Обновляем название кнопки, отображающей выбранный язык
            Button_SelectLanguage.Content = newLanguagr;

            // Задаем значение языка, в котором выбирать символы случайным образом
            this.actualLanguage = newLanguagr;
        }

        // Меняем цвет фона матрицы
        private void Event_Button_Click_ChangeBackground(object sender, RoutedEventArgs e)
        {
            // Передаем в свойство класса colorMatrixBackground выбранный цвет из элемента ColorPicker для фона матрицы
            colorMatrixBackground["A"] = ColorPicker.Color.A;
            colorMatrixBackground["R"] = ColorPicker.Color.R;
            colorMatrixBackground["G"] = ColorPicker.Color.G;
            colorMatrixBackground["B"] = ColorPicker.Color.B;

            // Задаем выбранный цвет фона матрицы соответствующей кнопке
            Button_BackgroundColor.Background = new SolidColorBrush(new Color()
            {
                A = (byte)(colorMatrixBackground["A"]) /*Opacity*/,
                R = (byte)(colorMatrixBackground["R"]) /*Red*/,
                G = (byte)(colorMatrixBackground["G"]) /*Green*/,
                B = (byte)(colorMatrixBackground["B"]) /*Blue*/
            });

            // Задаем выбранный цвет фона матрицы
            ChangeBackground();
        }

        // Метод изменения цвета фона матрицы. По умолчанию черный.
        private void ChangeBackground()
        {
            // Задаю цвет фона матрицы
            LayoutRootSecond.Background = new SolidColorBrush(new Color()
            {
                A = (byte)(colorMatrixBackground["A"]) /*Opacity*/,
                R = (byte)(colorMatrixBackground["R"]) /*Red*/,
                G = (byte)(colorMatrixBackground["G"]) /*Green*/,
                B = (byte)(colorMatrixBackground["B"]) /*Blue*/
            });
        }

        // Начальные настройки цветов фона, символов и т.д
        private void BeginColorSettings()
        {
            // Задаем начальный цвет фона матрицы
            colorMatrixBackground["A"] = 0;
            colorMatrixBackground["R"] = 0;
            colorMatrixBackground["G"] = 0;
            colorMatrixBackground["B"] = 0;

            // Задаем начальный цвет первого символа
            colorFirstSymbol["A"] = 255;
            colorFirstSymbol["R"] = 248;
            colorFirstSymbol["G"] = 248;
            colorFirstSymbol["B"] = 255;

            // Задаем начальный цвет градиента от (второго символа в змейке)
            gradientFrom["A"] = 255;
            gradientFrom["R"] = 1;
            gradientFrom["G"] = 255;
            gradientFrom["B"] = 1;

            // Задаем начальный цвет градиента до (последнего символа в змейке)
            gradientTo["A"] = 0;
            gradientTo["R"] = 0;
            gradientTo["G"] = 0;
            gradientTo["B"] = 0;
        }

        // Изменение цвета первого символа
        private void Event_Button_Click_FirstSymbolColor(object sender, RoutedEventArgs e)
        {
            // Передаем в свойство класса colorFirstSymbol выбранный цвет из элемента ColorPicker для фона матрицы
            colorFirstSymbol["A"] = ColorPicker.Color.A;
            colorFirstSymbol["R"] = ColorPicker.Color.R;
            colorFirstSymbol["G"] = ColorPicker.Color.G;
            colorFirstSymbol["B"] = ColorPicker.Color.B;

            // Задаем выбранный цвет фона матрицы соответствующей кнопке
            Button_FirstSymbolColor.Background = new SolidColorBrush(new Color()
            {
                A = (byte)(colorFirstSymbol["A"]) /*Opacity*/,
                R = (byte)(colorFirstSymbol["R"]) /*Red*/,
                G = (byte)(colorFirstSymbol["G"]) /*Green*/,
                B = (byte)(colorFirstSymbol["B"]) /*Blue*/
            });
        }

        // Настройки. Задаем градиент змейки. Цвет второго символа.
        private void Event_Button_Click_GradientFrom(object sender, RoutedEventArgs e)
        {
            // Передаем в свойство класса gradientFrom выбранный цвет из элемента ColorPicker для фона матрицы
            gradientFrom["A"] = ColorPicker.Color.A;
            gradientFrom["R"] = ColorPicker.Color.R;
            gradientFrom["G"] = ColorPicker.Color.G;
            gradientFrom["B"] = ColorPicker.Color.B;

            // Задаем выбранный цвет градиента от змейки соответствующей кнопке
            Button_GradientFrom.Background = new SolidColorBrush(new Color()
            {
                A = (byte)(gradientFrom["A"]) /*Opacity*/,
                R = (byte)(gradientFrom["R"]) /*Red*/,
                G = (byte)(gradientFrom["G"]) /*Green*/,
                B = (byte)(gradientFrom["B"]) /*Blue*/
            });
        }

        // Настройки. Задаем градиент змейки. Цвет последнего символа.
        private void Event_Button_Click_GradientTo (object sender, RoutedEventArgs e)
        {
            // Передаем в свойство класса gradientTo выбранный цвет из элемента ColorPicker для фона матрицы
            gradientTo["A"] = ColorPicker.Color.A;
            gradientTo["R"] = ColorPicker.Color.R;
            gradientTo["G"] = ColorPicker.Color.G;
            gradientTo["B"] = ColorPicker.Color.B;

            // Задаем выбранный цвет градиента до змейки соответствующей кнопке
            Button_GradientTo.Background = new SolidColorBrush(new Color()
            {
                A = (byte)(gradientTo["A"]) /*Opacity*/,
                R = (byte)(gradientTo["R"]) /*Red*/,
                G = (byte)(gradientTo["G"]) /*Green*/,
                B = (byte)(gradientTo["B"]) /*Blue*/
            });
        }     
    }
}