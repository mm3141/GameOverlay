using System.Collections.Generic;
using System.Linq;

namespace Radar
{
    /// <summary>
    /// 
    /// </summary>
    public class MapEdgeDetector
    {
        private readonly int current, right, left, up, down;

        /// <summary>
        /// Class that helps with map edge detection.
        /// </summary>
        /// <param name="mapTextureData"></param>
        /// <param name="bytesPerRow"></param>
        /// <param name="y"></param>
        /// <param name="x"></param>
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

        /// <summary>
        /// Checks if the current tile is walkable and at least 1 other direction is walkable too.
        /// </summary>
        /// <returns></returns>
        public bool AtLeastOneDirectionIsBorder()
        {
            // we add the extra condition if current == 1 to make the border thicker.
            return (CanWalk(current) || current == 1) &&
                   (CanWalk(down) || CanWalk(up) || CanWalk(right) || CanWalk(left));
        }

        private static bool CanWalk(int direction)
        {
            // Values are [0;5], 1,2,3,4,5 = can walk. 0 = cannot walk.
            return direction != 0;
        }

        /// <summary>
        /// Checks if (ImageX,ImageY) coordinate is within the width and height of the map.
        /// </summary>
        /// <param name="totalRows"></param>
        /// <param name="imageX"></param>
        /// <param name="imageY"></param>
        /// <param name="bytesPerRow"></param>
        /// <returns>True if X,Y is within the boundary of the image. Otherwise false</returns>
        public bool IsInsideMapBoundary(int totalRows, int imageX, int imageY, int bytesPerRow)
        {
            var width = bytesPerRow * 2;
            return imageX < width && imageX >= 0 && imageY < totalRows && imageY >= 0;
        }
    }
}