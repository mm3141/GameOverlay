using System.Collections.Generic;
using System.Linq;

namespace Radar
{
    public class MapEdgeDetector
    {
        private readonly int current, right, left, up, down;

        public MapEdgeDetector(byte[] mapTextureData, int bytesPerRow, int y, int x)
        {
            var index = y * bytesPerRow + x / 2; // since there are 2 data points in 1 index.
            var evenRemainder = x % 2;
            var oneIfEvenZeroIfOdd = evenRemainder ^ 1;
            var zeroIfEvenOneIfOdd = evenRemainder ^ 0;
            var shiftIfEven = oneIfEvenZeroIfOdd * 0x4;
            var shiftIfOdd = zeroIfEvenOneIfOdd * 0x4;
            
            current = (GetByIndex(mapTextureData, index) >> shiftIfOdd) & 0xF;
            up = (GetByIndex(mapTextureData, index + bytesPerRow) >> shiftIfOdd) & 0xF;
            down = (GetByIndex(mapTextureData, index - bytesPerRow) >> shiftIfOdd) & 0xF;
            left = (GetByIndex(mapTextureData, index - oneIfEvenZeroIfOdd) >> shiftIfEven) & 0xF;
            right = (GetByIndex(mapTextureData, index + zeroIfEvenOneIfOdd) >> shiftIfEven) & 0xF;
        }

        private static byte GetByIndex(IEnumerable<byte> mapTextureData, int index)
        {
            return mapTextureData.ElementAtOrDefault(index);
        }

        public bool AtleastOneDirectionIsBorder()
        {
            return current is 0 or 1 && (down > 0 || up > 0 || right > 0 || left > 0);
        }

        public bool ShouldDrawBorderEdge(int totalRows, int imageX, int imageY, int bytesPerRow)
        {
            return imageX < bytesPerRow * 2 && imageX >= 0 && imageY < totalRows && imageY >= 0;
        }
    }
}