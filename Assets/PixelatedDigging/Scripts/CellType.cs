using System;

namespace PixelatedDigging
{
    [Flags]
    public enum CellType
    {
        None = 0,
        DownFilled = 1,
        LeftFilled = 2,
        UpFilled = 4,
        RightFilled = 8,
    }
}