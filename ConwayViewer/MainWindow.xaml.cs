using ConwayGameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace ConwayViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _locker;
        private ConwayBoard _board;
        private int _animationSpeed = 500;
        private bool _animationRunning;
        private Win32 _api32; 
        private System.Windows.Point _lastCellEdited;
        private Timer _animationTicker;
        public MainWindow()
        {
            InitializeComponent();
            _api32 = new Win32();
            _lastCellEdited = new System.Windows.Point();
            _board = new ConwayBoard(128, 64);
            _board.InitializeCell(1, 4, true);
            _board.InitializeCell(2, 4, true);
            _board.InitializeCell(3, 4, true);
            _board.InitializeCell(3, 3, true);
            _board.InitializeCell(2, 2, true);
            UpdateMap();
            AnimationSpeed.Value = 500;
            //_board.InitializeCell(4, 4, true);
            _animationTicker = new Timer(RunLife, null, Timeout.Infinite, Timeout.Infinite);
            AnimationSpeed.ValueChanged += AnimationSpeed_ValueChanged;
            ConwayBoard.MouseDown += ConwayBoard_MouseDown;
            ConwayBoard.MouseMove += ConwayBoard_MouseMove;
            StartAnimation.Click += StartAnimation_Click;
            StopAnimation.Click += StopAnimation_Click;
            Random.Click += Random_Click;
           
        }

        private void Random_Click(object sender, RoutedEventArgs e)
        {
            var rnd = new Random();
            for (ushort i = 0; i < _board.BoardWidth; i++)
            {
                for (ushort j = 0; j < _board.BoardHeight; j++)
                {
                    var r = rnd.Next(0, 2);
                    _board.InitializeCell(j, i, r != 0);
                }
            }
            UpdateMap();
        }

        private void StopAnimation_Click(object sender, RoutedEventArgs e)
        {
            _animationTicker.Change(Timeout.Infinite, Timeout.Infinite);
            _animationRunning = false;
        }

        private void StartAnimation_Click(object sender, RoutedEventArgs e)
        {
            _animationSpeed = (int)AnimationSpeed.Value;
            _animationTicker.Change(_animationSpeed, _animationSpeed);
            _animationRunning = true;
        }

        private void ConwayBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;
            var position = e.GetPosition(ConwayBoard);
            var cellWidthPixels = ConwayBoard.ActualWidth / _board.BoardWidth;
            var cellHeightPixels = ConwayBoard.ActualHeight / _board.BoardHeight;
            var cell = GetCell(position, cellWidthPixels, cellHeightPixels);
            if(cell.X != _lastCellEdited.X || cell.Y != _lastCellEdited.Y)
            {
                _board.SwitchCell((ushort)cell.Y, (ushort)cell.X);
                _lastCellEdited = cell;
                UpdateMap();
            }
        }

        private void ConwayBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ConwayBoard);
            var cellWidthPixels = ConwayBoard.ActualWidth / _board.BoardWidth;
            var cellHeightPixels = ConwayBoard.ActualHeight / _board.BoardHeight;
            var cell = GetCell(position, cellWidthPixels, cellHeightPixels);
            _board.SwitchCell((ushort)cell.Y, (ushort)cell.X);
            _lastCellEdited = cell;
            UpdateMap();
        }

        private System.Windows.Point GetCell(System.Windows.Point mousePoint, double cellWidthPixels, double cellHeightPixels)
        {

            ushort cellCol = (ushort)(mousePoint.X / cellWidthPixels);
            ushort cellRow = (ushort)(mousePoint.Y / cellHeightPixels);
            return new System.Windows.Point(cellCol, cellRow);
        }

        private void AnimationSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _animationSpeed = (int)AnimationSpeed.Value;
            if(_animationRunning)
                _animationTicker.Change(_animationSpeed, _animationSpeed);
        }

        private void RunLife(object state)
        {
            _board.StepToNextGeneration();
            UpdateMap();

        }

        //Dev'essere fornito un interblocco alle risorse per evitare condizioni di sovrapposizione di chiamata
        private void UpdateMap()
        {

            if (_locker)
                return;
            _locker = true;
            byte[] array = _board.GetMap();
            ConwayBoard.Dispatcher.Invoke(() =>
            {
                ConwayBoard.Source = GetImage(array, _board.BoardWidth);
            });
            _locker = false;
        }

        //Converte la mappa del gioco in un immagine a 1bpp.
        //Il formato dell'immagine necessita che la riga dell'immagine termini sempre con l'allineamento alla word utilizzata dal sistema
        //riempiendo i valori non usati con 0.
        private BitmapImage GetImage (byte[] array, int imageWidth)
        {
            byte[] bmpArray = ConvertToBitmapMapping(array, imageWidth);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * bmpArray.Length);
            try
            {        
                Marshal.Copy(bmpArray, 0, ptr, bmpArray.Length);
                var handle = _api32.CallCreateBitmap(_board.BoardWidth, _board.BoardHeight, 1, 1, ptr);

                var image = Image.FromHbitmap(handle);
                _api32.CallDeleteObject(handle);  //Libero le risorse

                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch
            {
                return new BitmapImage();
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            //using (var ms = new System.IO.MemoryStream(array))
            //{
            //    var image = new BitmapImage();
            //    image.BeginInit();
            //    image.CacheOption = BitmapCacheOption.OnLoad; // here
            //    image.StreamSource = ms;
            //    image.EndInit();
            //    return image;
            //}
        }

        protected override void OnClosed(EventArgs e)
        {
            _animationTicker.Dispose();
        }

        /// <summary>
        /// Importa metodi della libreria grafica delle API di windows per poter gestire le immagini con 1bpp
        /// </summary>
        private class Win32
        {
            [DllImport("Gdi32.dll", SetLastError = true)]
            public static extern IntPtr CreateBitmap(
              int nWidth,
              int nHeight,
              uint nPlanes,
              uint nBitCount,
              IntPtr lpBits);

            [DllImport("Gdi32.dll", SetLastError = true)]
            public static extern void DeleteObject(IntPtr hBitmap);

            public IntPtr CallCreateBitmap(int nWidth, int nHeight,uint nPlanes,uint nBitCount,IntPtr lpBits)
            {
                return CreateBitmap(nWidth, nHeight, nPlanes, nBitCount, lpBits);
            }
            public void CallDeleteObject(IntPtr hBitmap)
            {
                DeleteObject(hBitmap);
            }

        }

        private byte[] ConvertToBitmapMapping(byte[] inArray, int imageWidth)
        {
            int lineBytes = imageWidth / 8;

            int lineBits = imageWidth % 8;

            int wordSize = 2; // sizeof(int);
            int wordBits = wordSize * 8;
            int lineWords = imageWidth / wordBits;
            if (imageWidth % wordBits > 0)
                lineWords++;

            int lines = (inArray.Length * 8) / imageWidth;

            int outArrayLength = lines * lineWords * wordSize;
            byte[] outArray = new byte[outArrayLength];

            int fillBytes = wordSize % (imageWidth / 8);
            int inputPointer = 0;
            int outPointer = 0;
            for(int line = 0; line < lines; line++)
            {
                Array.Copy(inArray, inputPointer, outArray, outPointer, lineBytes);
                inputPointer += lineBytes;
                outPointer += (lineWords * wordSize);
            }
            return outArray;
        }

        private void SpeedChanged()
        {

        }
    }
}
