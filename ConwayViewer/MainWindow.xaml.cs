using ConwayBoardManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConwayViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConwayBoard _board;
        public MainWindow()
        {
            InitializeComponent();

            _board = new ConwayBoard(100, 100);
            _board.InitializeCell(49, 49, true);
            _board.InitializeCell(50, 50, true);
            _board.InitializeCell(51, 51, true);
            Timer ticker = new Timer(OnTimer, null, 500, 500);
        }

        private void OnTimer(object state)
        {
            _board.StepToNextGeneration();
        }
    }
}
