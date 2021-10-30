using System.Collections.Generic;
using System.Linq;

namespace Radar
{
    /// <summary>
    /// Detects if the current map location is map edge or not.
    /// </summary>
    public class MapEdgeDetector
    {
        /// <summary>
        /// Map (current location) walkable data is stored in these variables.
        /// </summary>
        private readonly int current, right, left, up, down;

        /// <summary>
        /// Class that helps with map edge detection.
        /// </summary>
        /// <param name="mapTextureData">map walkable data.</param>
        /// <param name="bytesPerRow">number of bytes in a single row of walkable map data.</param>
        /// <param name="y">map y location whos edge caller wants to detect.</param>
        /// <param name="x">map x location whos edge caller wants to detect.</param>
        public MapEdgeDetector(byte[] mapTextureData, int bytesPerRow, int y, int x)
        {
            var index = (y * bytesPerRow) + (x / 2); // (x / 2) => since there are 2 data points in 1 byte.
            var wantsFirstNibble = x % 2;
            var oneIfFirstNibbleZeroIfNot = wantsFirstNibble ^ 1;
            var zeroIfFirstNibbleOneIfNot = wantsFirstNibble ^ 0;
            var shiftIfFirstNibble = oneIfFirstNibbleZeroIfNot * 0x4;
            var shiftIfSecondNibble = zeroIfFirstNibbleOneIfNot * 0x4;

            current = (GetByIndex(mapTextureData, index) >> shiftIfSecondNibble) & 0xF;
            up = (GetByIndex(mapTextureData, index + bytesPerRow) >> shiftIfSecondNibble) & 0xF;
            down = (GetByIndex(mapTextureData, index - bytesPerRow) >> shiftIfSecondNibble) & 0xF;
            left = (GetByIndex(mapTextureData, index - oneIfFirstNibbleZeroIfNot) >> shiftIfFirstNibble) & 0xF;
            right = (GetByIndex(mapTextureData, index + zeroIfFirstNibbleOneIfNot) >> shiftIfFirstNibble) & 0xF;
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

        /// <summary>
        /// 0 = not walkable 1,2,3,4,5 means potentially walkable.
        /// It's potentially walkable because it also depends on entity size
        /// (e.g. if entity size is 1 then 1 or above is walkable and
        /// if entity size is 3 than 3 or above is walkable). For the purpose
        /// of generating map we will assume everything above 0 is walkable.
        /// </summary>
        /// <param name="direction">direction from the current walkable map position.</param>
        /// <returns></returns>
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