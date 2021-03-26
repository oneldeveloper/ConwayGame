using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace ConwayGameCore
{
    /// <summary>
    /// Rappresenta e gestisce il Conway Game, per quanto riguarda la mappa e la sua gestione
    /// </summary>
    public class ConwayBoard : IDisposable
    {
        ushort _boardWidth;
        ushort _boardHeigth;

        ushort _rowOffset;
        ushort _colOffset;

        long _cellsNumber;

        long _mapSize;

        byte[] _map;
        byte[] _mapNextGeneration;

        /// <summary>
        /// Larghezza della tabella del gioco
        /// </summary>
        public ushort BoardWidth => _boardWidth;
        /// <summary>
        /// Altezza della tabella del gioco
        /// </summary>
        public ushort BoardHeight => _boardHeigth;
        /// <summary>     
        /// Numero di celle che compongono il gioco
        /// </summary>
        public long CellsNumber => _cellsNumber;


        /// <summary>
        /// Inizializza un nuovo gioco
        /// </summary>
        /// <param name="width">larghezza in celle (solo per multipli di 8)</param>
        /// <param name="height">altezza in celle (solo per multipli di 8)</param>
        public ConwayBoard(ushort width, ushort height)
        {
            _boardWidth = width;
            _boardHeigth = height;

            _cellsNumber = width * height;

            //_rowOffset = (ushort)(length / 8);
            //if (length % 8 != 0)
            //    _rowOffset++;
            _rowOffset = width;

            _colOffset = (ushort)(height / 8);
            if (width % 8 != 0)
                _colOffset++;

            _map = GetMap(_boardWidth, _boardHeigth, ref _mapSize);
            _mapNextGeneration = GetMap(_boardWidth, _boardHeigth, ref _mapSize);
        }

        /// <summary>
        /// Inizializza una cella della mappa
        /// </summary>
        /// <param name="row">riga</param>
        /// <param name="col">colonna</param>
        /// <param name="state">stato della cella (true = viva)</param>
        public void InitializeCell(ushort row, ushort col, bool state)
        {
            SetCell(_map, row, col, state);
        }
        /// <summary>
        /// Inverte lo stato di una cella 
        /// </summary>
        /// <param name="row">riga</param>
        /// <param name="col">colonna</param>
        public void SwitchCell(ushort row, ushort col)
        {
            var cellState = GetCell(_map, row, col);
            SetCell(_map, row, col, !cellState);
        }
        /// <summary>
        /// Restituisce lo stato di una cella
        /// </summary>
        /// <param name="row">riga</param>
        /// <param name="col">colonna</param>
        /// <returns>true se la cella è viva</returns>
        public bool GetCellState(ushort row, ushort col)
        {
            return GetCell(_map, row, col);
        }
        /// <summary>
        /// Avanza il gioco di una generazione
        /// </summary>
        public void StepToNextGeneration()
        {
            bool[,] cellMatrix = new bool[3, 3];
            for (ushort bCol = 0; bCol < _boardWidth - 2; bCol++)
            {
                for (ushort bRow = 0; bRow < _boardHeigth - 2;  bRow++)
                {
                    //seleziona la matrice della cella selezionata
                    for (sbyte mCol = 0; mCol < 3; mCol++)
                    {
                        for (sbyte mRow = 0; mRow < 3; mRow++)
                        {
                            ushort r = (ushort)(mRow + bRow);
                            ushort c = (ushort)(mCol + bCol);
                            cellMatrix[mRow, mCol] = GetCell(_map, (ushort)(r), (ushort)(c));
                        }
                    }
                    bool cellState = ConwayEngine.GetNextCellState(cellMatrix);
                    SetCell(_mapNextGeneration, (ushort)(bRow + 1), (ushort)(bCol + 1), cellState);
                }
            }
            Array.Copy(_mapNextGeneration, 0, _map, 0, _mapSize);  
        }
        /// <summary>
        /// Riestituisce un array di byte che rappresenta la mappa.
        /// l'array è formato dalle righe della tabella in successione
        /// </summary>
        /// <returns></returns>
        public byte[] GetMap()
        {
            return _map;
        }
        /// <summary>
        /// Libera le risorse 
        /// </summary>
        public void Dispose()
        {
            _map = null;
        }

        private byte[] GetMap(ushort length, ushort height, ref long boardSize)
        {
            boardSize = (length * height) / 8;
            if ((length * height) % 8 != 0)
                boardSize++;
            return new byte[boardSize];
        }

        private void SetCell(byte[] map, ushort row, ushort col, bool alive)
        {
            long mappedCellByte = 0;
            byte mappedCellBit = 0;
            GetMapAddress(row, col, ref mappedCellByte, ref mappedCellBit);
            byte b = map[mappedCellByte];
            if (alive)
                b |= (byte)(1 << (7 - mappedCellBit));
            else
                b &= (byte)~(1 << (7 - mappedCellBit));
            map[mappedCellByte] = b;
        }

        private bool GetCell(byte[] map, ushort row, ushort col)
        {
            long mappedCellByte = 0;
            byte mappedCellBit = 0;
            GetMapAddress(row, col, ref mappedCellByte, ref mappedCellBit);
            byte b = map[mappedCellByte];
            return (b & (byte)(1 << 7 - mappedCellBit)) != 0;
        }

        private void GetMapAddress(ushort row, ushort col, ref long mapByte, ref byte mapBit)
        {
            long p = (row * _rowOffset) + (col);
            mapByte = p / 8;
            mapBit = (byte)(p % 8);
        }
    }
}
