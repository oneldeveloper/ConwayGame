using ConwayBoardManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestConwayBoard
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestInitializeCell()
        {
            ushort rows = 100;
            ushort cols = 100;
            ConwayBoard board = new ConwayBoard(rows, cols);
            for (ushort row = 0; row < rows; row++)
            {
                for (ushort col = 0; col < cols; col++)
                {
                    bool previousState = board.GetCellState(row, col);
                    board.InitializeCell(row, col, !previousState);
                    bool actualState = board.GetCellState(row, col);
                    Assert.AreNotEqual(actualState, previousState);
                }
            }

        }

        [TestMethod]
        public void TestMethod1()
        {
            ConwayBoard board = new ConwayBoard(100, 100);
            board.InitializeCell(2, 2, true);
            board.InitializeCell(3, 2, true);
            board.InitializeCell(4, 2, true);
            board.StepToNextGeneration();
        }
    }
}
