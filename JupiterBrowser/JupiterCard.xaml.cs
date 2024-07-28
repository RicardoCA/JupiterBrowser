using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JupiterBrowser
{
    public partial class JupiterCard : Window
    {
        private static Random random = new Random();
        private const string ImagePath = "canvas_image.png";

        public JupiterCard()
        {
            InitializeComponent();
            this.KeyDown += Window_KeyDown;
            this.Loaded += JupiterCard_Loaded;
            JupiterCanvas.SizeChanged += JupiterCanvas_SizeChanged; // Adiciona o manipulador de eventos SizeChanged
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void GenerateRandomElements()
        {
            // Define o tamanho do Canvas se necessário
            JupiterCanvas.Width = 800; // Defina a largura desejada
            JupiterCanvas.Height = 600; // Defina a altura desejada

            // Limpa o canvas antes de gerar novos elementos
            JupiterCanvas.Children.Clear();

            // Gera uma cor aleatória
            var randomColor = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            var textBrush = new SolidColorBrush(randomColor);

            string[] fontFamilies = new string[]
            {
                "Arial",
                "Times New Roman",
                "Verdana",
                "Tahoma",
                "Comic Sans MS",
                "Courier New"
                // Adicione outras fontes conforme necessário
            };
            string randomFontFamily = fontFamilies[random.Next(fontFamilies.Length)];

            // Adiciona o texto "Jupiter Browser" no centro
            var textBlock = new TextBlock
            {
                Text = "Jupiter Browser",
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = textBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily(randomFontFamily), // Aplica a fonte aleatória
            };

            // Define a posição do texto no Canvas
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = textBlock.DesiredSize.Width;
            double textHeight = textBlock.DesiredSize.Height;
            double canvasWidth = JupiterCanvas.Width;
            double canvasHeight = JupiterCanvas.Height;

            Canvas.SetLeft(textBlock, (canvasWidth - textWidth) / 2);
            Canvas.SetTop(textBlock, (canvasHeight - textHeight) / 2);

            // Adiciona o texto ao Canvas
            JupiterCanvas.Children.Add(textBlock);

            // Gera elementos aleatórios
            for (int i = 0; i < 10; i++)
            {
                // Cria um novo círculo
                Ellipse ellipse = new Ellipse
                {
                    Width = random.Next(20, 50),
                    Height = random.Next(20, 50),
                    Fill = new SolidColorBrush(Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256))),
                    Stroke = Brushes.White,
                    StrokeThickness = 2
                };

                // Define uma posição aleatória no Canvas
                double x = random.NextDouble() * (JupiterCanvas.Width - ellipse.Width);
                double y = random.NextDouble() * (JupiterCanvas.Height - ellipse.Height);

                // Define a posição do círculo no Canvas
                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);

                // Adiciona o círculo ao Canvas
                JupiterCanvas.Children.Add(ellipse);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCanvasToFile();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomElements();
            
        }

        private void SaveCanvasToFile()
        {
            // Garante que o Canvas tenha uma largura e altura definidas antes de salvar
            if (JupiterCanvas.Width <= 0 || JupiterCanvas.Height <= 0)
            {
                MessageBox.Show("O Canvas não tem dimensões válidas para salvar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Cria uma caixa de diálogo para o usuário escolher onde salvar o arquivo
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png",
                DefaultExt = "png",
                FileName = "JupiterCard" // Nome padrão do arquivo
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Aguarda o Canvas ser completamente renderizado
                Dispatcher.Invoke(() =>
                {
                    double canvasWidth = JupiterCanvas.Width;
                    double canvasHeight = JupiterCanvas.Height;

                    var renderTargetBitmap = new RenderTargetBitmap(
                        (int)canvasWidth,
                        (int)canvasHeight,
                        96, 96, PixelFormats.Pbgra32);

                    renderTargetBitmap.Render(JupiterCanvas);

                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        pngEncoder.Save(fileStream);
                    }
                });
            }
        }


        private void LoadCanvas()
        {
            if (File.Exists(ImagePath))
            {
                var bitmapImage = new BitmapImage();

                using (var fileStream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = fileStream;
                    bitmapImage.EndInit();
                }

                // Define o tamanho do Canvas baseado na imagem carregada
                JupiterCanvas.Width = bitmapImage.PixelWidth;
                JupiterCanvas.Height = bitmapImage.PixelHeight;

                // Cria uma imagem do Canvas e a define como o fundo do Canvas
                var image = new Image
                {
                    Source = bitmapImage,
                    Width = JupiterCanvas.Width,
                    Height = JupiterCanvas.Height
                };
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                JupiterCanvas.Children.Clear();
                JupiterCanvas.Children.Add(image);
            }
            else
            {
                GenerateRandomElements(); // Gera elementos se não houver imagem salva
                
            }
        }

        private void JupiterCard_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCanvas(); // Carrega o Canvas quando a janela é carregada
        }

        private void JupiterCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Gera novos elementos quando o tamanho do Canvas é alterado
            GenerateRandomElements();
        }
    }
}
