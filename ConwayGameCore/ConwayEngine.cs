using System;

namespace ConwayGameCore
{
    /// <summary>
    /// Gestisce le regole del gioco
    /// </summary>
    public static class ConwayEngine
    {
        /// <summary>
        /// A partire da una matrice 3X3 in cui è valutata la cella centrale, restituisce lo stato della cella nella prossima generazione
        /// </summary>
        /// <param name="cellMatrix">matrice da valutare</param>
        /// <returns>true se la cella sarà viva</returns>
        public static bool GetNextCellState (bool[,] cellMatrix)
        {
            bool currentState = cellMatrix[1,1];
            byte neightbors = 0;
            for (byte col = 0; col < 3; col+=2)
            {
                for (byte row = 0; row < 3; row++)
                {
                    if(cellMatrix[row,col] == true)
                        neightbors++;
                }
            }
            if(cellMatrix[0,1])
                neightbors++;
            if (cellMatrix[2,1])
                neightbors++;

            if (currentState)
                return neightbors < 2 || neightbors > 3 ? false : true;
            else
                return !currentState && neightbors == 3;            
        }
    }
}
