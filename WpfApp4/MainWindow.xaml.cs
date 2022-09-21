using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string directory = @"pack://application:,,,/"; // Условный путь  на ресурс  типа контент в VS Содержимое
        string pathImage = "MyImage"; // папка для картинок 
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded; // метод  на  старте 
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cbSelectImage.ItemsSource = GetContent(pathImage); // // контент для комбобокса 
            cbSelectImage.SelectedIndex = 0; // стартовая позиция
            imageContent.Source = GetSource(cbSelectImage.SelectedItem.ToString()); // подгружаем картинку  
        }

        /// <summary>
        /// получаем картинку  по  его  имяни
        /// </summary>
        /// <param name="nameImage"></param>
        /// <returns></returns>
        private BitmapImage GetSource(string? nameImage)
        {
            Uri uri = GetUri(nameImage); // получаем  его  ури 
            try
            {
                BitmapImage image = new BitmapImage(uri); // если  файл  в  ресурсе 
                return image;
            }
            catch
            {
                try /// аааадский костыль   // если его нет  в ресурсе 
                {
                    var assembly = Assembly.GetExecutingAssembly(); //получаем  путь  откуда запустилось  приложение 

                    // путь  + папка + название файла  
                    var path = System.IO.Path.GetDirectoryName(assembly.Location) + "/" + pathImage +"/" + nameImage;

                    Uri uriEx = new Uri(path); // получаем  абсолютный  ури 
                    BitmapImage image = new BitmapImage(uriEx); // получаем  картинку  не  из  ресурса 
                    return image;
                }
                catch (Exception)
                {
                    MessageBox.Show("ошибка загрузки  изображения");
                }
            }
            return null; 
            
        }

        /// <summary>
        /// вспомогательный метод получает  ури 
        /// </summary>
        /// <param name="nameImage"></param>
        /// <returns></returns>
        private Uri GetUri(string? nameImage)
        {
            string patp = directory + pathImage +"/" + nameImage; // pack://application:,,,/ + MyImage + / название  файла
            Uri uri = new Uri(patp , UriKind.RelativeOrAbsolute); 
            return uri;
        }

        /// <summary>
        /// обработчик  события при  выборе  комбобокса 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSelectImage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            imageContent.Source = GetSource(cbSelectImage.SelectedItem.ToString()); // получить новый  контент  для картинки 
        }

        /// <summary>
        /// добавляет  новую  картинку  в  папку 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog(); // диалог
            openFile.Filter = "png|*.png|jpg|*.jpg|*.jpeg|*.jpeg"; // фильтр  
            openFile.Title = "Добавить  новую картинку";
            openFile.Multiselect = false; // можно  только  одну  картинку  выбрать 

            if (openFile.ShowDialog() == true)
            {
                var pathCopiTo = GEtDirectory(pathImage); // получаем  директорию  откуда стартовал проект 

                int   sumboNexn = 1; // будет  нцжно  если картинка с таким  именем уже есть  
                var nameNewImage = pathCopiTo + "/" + openFile.SafeFileNames[0]; // получаем  полный адрес  будующей картинки 
                //  папка  в проекте  + имя файла  

                while (File.Exists(nameNewImage)) // если такой  файл  уже есть  в  папке  
                {
                    sumboNexn ++; // придумаем новое  имя 
                    string newName = string.Empty;
                    var masiiv = openFile.SafeFileNames[0].Split('.'); // разделим название файла на массив  - последний  элемент  - png
                    for (int i = 0; i < masiiv.Length-1; i++)
                    {
                        newName += masiiv[i]; // соберем назад   без png
                    }
                    newName += $"({sumboNexn})"; // добавим  к названию  (1)
                    newName += $".{masiiv.Last()}"; // вернем назад  .png
                    nameNewImage = pathCopiTo + "/"  +  newName;  // получим  новое  имя  //
                }

                // если нашли уникальное  имя  - отправим нашу картинку  в  папку  с  проектом 

                File.Copy(openFile.FileName  , nameNewImage , false ) ; // копируем  
                MainWindow_Loaded(null, null) ; // обновим  форму  
                cbSelectImage.SelectedValue =  Path.GetFileName( nameNewImage); // выведим  в  комбобокс  только  что  добавленный файл 

            }
        }

        /// <summary>
        /// страшный метод   - получаю  список  всех ресурсов  из проект  -- вот  запихнуть  файл  в  ресурс без  страшных манипуляция 
        /// я не  придумал  ну  и  ладно 
        /// </summary>
        /// <returns> </returns>
        private string[] GetContent()
        {
            List<string> list = new List<string>();
            var t = Application.ResourceAssembly.CustomAttributes; // массив  атрибутов  

            foreach (var item in t.Where(x => x.ToString().Contains("System.Windows.Resources.AssemblyAssociatedContentFileAttribute")))
            // только  те  кто  ContentFileAttribute
            {
                foreach (var i in item.ConstructorArguments.Distinct())
                {
                    var s = i.ToString().TrimStart('"').TrimEnd('"'); // Убираем  кавычки  
                    s = s.TrimStart(pathImage.ToLower().ToCharArray());  // убираем  из названия папку 
                    s = s.TrimStart('/'); // убираем  символ  /

                    if (s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg"))// проверияем  что  это файлы
                        list.Add(s);  // складываем только  картинки  
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// список  файлов  в папке 
        /// </summary>
        /// <param name="path">под папка  с  картинками  </param>
        /// <returns></returns>
        private string[] GetContent (string path)
        {
            List<string> strings = new List<string>();

            string dir = GEtDirectory(path);  // находим  папку  с  проектом  

            var files = Directory.GetFiles(dir); // находим список  всех файлов в  папке  

            foreach (var s in files)
            {
                if (s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg"))
                {
                    strings.Add( Path.GetFileName(s)); // ищим  только  картинки  
                }
            }

            return strings.ToArray();
        }

        /// <summary>
        /// Находит  стартовую  папку проекта + 
        /// </summary>
        /// <param name="path">подкаталог  в проекте  </param>
        /// <returns></returns>
        private static string GEtDirectory(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();  // информация о  сборке  
            var dir = Path.GetDirectoryName(assembly.Location) + "\\" + path;  // старт + путь под папки  
            return dir;
        }
    }
}
