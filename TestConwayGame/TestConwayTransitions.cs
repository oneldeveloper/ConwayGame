using ConwayGame;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestConwayGame
{
    [TestClass]
    public class TestConwayTransitions
    {
        bool[,] conwayMatrix1Neightbor = new bool[,] { { false, false, false }, { false, true, false }, { true, false, false } };
        bool[,] conwayMatrix2Neightbor = new bool[,] { { true, false, false }, { false, true, false }, { true, false, false } };
        bool[,] conwayMatrix3Neightbor = new bool[,] { { false, true, true }, { true, true, false }, { false, false, false } };
        bool[,] conwayMatrix4Neightbor = new bool[,] { { false, false, false }, { false, true, true }, { true, true, true } };


        [TestMethod]
        public void TestSurvive()
        {
            bool res;
            res = ConwayEngine.GetNextCellState(conwayMatrix1Neightbor);
            Assert.IsFalse(res, "1 neightbor");
            res = ConwayEngine.GetNextCellState(conwayMatrix2Neightbor);
            Assert.IsTrue(res, "2 neightbors");
            res = ConwayEngine.GetNextCellState(conwayMatrix3Neightbor);
            Assert.IsTrue(res, "3 neightbors");
            res = ConwayEngine.GetNextCellState(conwayMatrix4Neightbor);
            Assert.IsFalse(res, "4 neightbors");
        }
    }
}
