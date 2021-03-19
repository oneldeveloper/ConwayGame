using ConwayGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace ConwayBoardManager
{
    public class ConwayBoard : IDisposable
    {
        ushort _boardLength;
        ushort _boardHeigth;

        ushort _rowOffset;
        ushort _colOffset;

        long _cellsNumber;

        long _mapSize;

        public ushort BoardLength => _boardLength;

        public ushort BoardHeight => _boardHeigth;

        public long CellsNumber => _cellsNumber;

        MemoryMappedFile _map;
        MemoryMappedViewAccessor _mapAccessor;

        public ConwayBoard(ushort length, ushort height)
        {
            _boardLength = length;
            _boardHeigth = height;

            _cellsNumber = length * height;

            //_rowOffset = (ushort)(length / 8);
            //if (length % 8 != 0)
            //    _rowOffset++;
            _rowOffset = length;

            _colOffset = (ushort)(height / 8);
            if (length % 8 != 0)
                _colOffset++;

            _rowPosition = 0;
            _colPosition = 0;

            _map = GetMap("ConwayBoard", _boardLength, _boardHeigth, ref _mapSize);
            _mapAccessor = _map.CreateViewAccessor(0, _mapSize, MemoryMappedFileAccess.ReadWrite);
        }

        public void InitializeCell(ushort row, ushort col, bool state)
        {
            SetCell(_mapAccessor, row, col, state);
        }

        public bool GetCellState(ushort row, ushort col)
        {
            return GetCell(_mapAccessor, row, col);
        }

        public void StepToNextGeneration()
        {
            long mapSize = 0;
            var nextGenerationBoard = GetMap("NextBoard", _boardLength, _boardHeigth, ref mapSize);
            var accessor = nextGenerationBoard.CreateViewAccessor(0, _boardLength, MemoryMappedFileAccess.Write);
            bool[,] cellMatrix = new bool[3, 3];
            for (ushort bCol = 0; bCol < _boardLength - 2; bCol++)
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
                            cellMatrix[mRow, mCol] = GetCell(_mapAccessor, (ushort)(r), (ushort)(c));
                        }
                    }
                    bool cellState = ConwayEngine.GetNextCellState(cellMatrix);
                    SetCell(accessor, (ushort)(bRow + 1), (ushort)(bCol + 1), cellState);
                }
            }
            for (long i = 0; i < _mapSize; i++)
            {
                byte b = accessor.ReadByte(i);
                _mapAccessor.Write(i, b);
            }
        }

        public void Dispose()
        {
            _mapAccessor.Dispose();
            _map.Dispose();
        }

        private MemoryMappedFile GetMap(string mapName, ushort length, ushort height, ref long boardSize)
        {
            boardSize = (length * height) / 8;
            if ((length * height) % 8 != 0)
                boardSize++;
           return  MemoryMappedFile.CreateNew(mapName, boardSize, MemoryMappedFileAccess.ReadWrite);
        }

        private void GetMapAddress(ushort row, ushort col, ref long mapByte, ref byte mapBit)
        {
            long p = (row * _rowOffset) + (col);
            mapByte = p/8;
            mapBit = (byte)(p % 8);
        }

        private void SetCell(MemoryMappedViewAccessor accessor, ushort row, ushort col, bool alive)
        {
            long mappedCellByte = 0;
            byte mappedCellBit = 0;
            GetMapAddress(row, col, ref mappedCellByte, ref mappedCellBit);
            byte b = accessor.ReadByte(mappedCellByte);
            if (alive)
                b |= (byte)(1 << (7 - mappedCellBit));
            else
                b &= (byte)~(1 << (7 - mappedCellBit));
            accessor.Write(mappedCellByte, b);
        }

        private bool GetCell(MemoryMappedViewAccessor accessor, ushort row, ushort col)
        {
            long mappedCellByte = 0;
            byte mappedCellBit = 0;
            GetMapAddress(row, col, ref mappedCellByte, ref mappedCellBit);
            byte b = accessor.ReadByte(mappedCellByte);
            return (b & (byte)(1 << 7 - mappedCellBit)) != 0;
        }
    }
}
