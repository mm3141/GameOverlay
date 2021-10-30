using System.Linq;

namespace Radar
{
    public class WalkSize
    {
        public readonly int C, N, P, U, D;

        public WalkSize(byte[] mapTextureData, int bytesPerRow, int y, int x)
        {
            int index = y * bytesPerRow + x / 2; // since there are 2 data points in 1 index.
            C = mapTextureData[index];
            N = mapTextureData.ElementAtOrDefault(index + 1); // right
            P = mapTextureData.ElementAtOrDefault(index - 1); // Left
            U = mapTextureData.ElementAtOrDefault(index - bytesPerRow); // Top
            D = mapTextureData.ElementAtOrDefault(index + bytesPerRow); // Bottom

            // https://en.wikipedia.org/wiki/Arithmetic_shift
            if (x % 2 == 0)
            {
                U = (U >> (0x04 * 0)) & 0x0F;
                D = (D >> (0x04 * 0)) & 0x0F;

                P = (P >> (0x04 * 1)) & 0x0F;
                N = (C >> (0x04 * 1)) & 0x0F;

                C = (C >> (0x04 * 0)) & 0x0F;
            }
            else
            {
                U = (U >> (0x04 * 1)) & 0x0F;
                D = (D >> (0x04 * 1)) & 0x0F;

                P = (C >> (0x04 * 0)) & 0x0F;
                N = (N >> (0x04 * 0)) & 0x0F;

                C = (C >> (0x04 * 1)) & 0x0F;
            }
        }

        public bool IsWalkable()
        {
            return C is 0 or 1 && (D > 0 || U > 0 || N > 0 || P > 0);
        }
    }
}