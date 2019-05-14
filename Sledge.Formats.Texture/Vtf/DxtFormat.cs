namespace Sledge.Formats.Texture.Vtf
{
    public static class DxtFormat
    {
        public static void DecompressDxt1(byte[] buffer, byte[] data, int width, int height)
        {
            var pos = 0;
            var c = new byte[16];
            for (var y = 0; y < height; y += 4)
            {
                for (var x = 0; x < width; x += 4)
                {
                    int c0 = data[pos++];
                    c0 |= data[pos++] << 8;

                    int c1 = data[pos++];
                    c1 |= data[pos++] << 8;

                    c[0] = (byte)((c0 & 0xF800) >> 8);
                    c[1] = (byte)((c0 & 0x07E0) >> 3);
                    c[2] = (byte)((c0 & 0x001F) << 3);
                    c[3] = 255;

                    c[4] = (byte)((c1 & 0xF800) >> 8);
                    c[5] = (byte)((c1 & 0x07E0) >> 3);
                    c[6] = (byte)((c1 & 0x001F) << 3);
                    c[7] = 255;

                    if (c0 > c1)
                    {
                        // No alpha channel

                        c[8] = (byte)((2 * c[0] + c[4]) / 3);
                        c[9] = (byte)((2 * c[1] + c[5]) / 3);
                        c[10] = (byte)((2 * c[2] + c[6]) / 3);
                        c[11] = 255;

                        c[12] = (byte)((c[0] + 2 * c[4]) / 3);
                        c[13] = (byte)((c[1] + 2 * c[5]) / 3);
                        c[14] = (byte)((c[2] + 2 * c[6]) / 3);
                        c[15] = 255;
                    }
                    else
                    {
                        // 1-bit alpha channel

                        c[8] = (byte)((c[0] + c[4]) / 2);
                        c[9] = (byte)((c[1] + c[5]) / 2);
                        c[10] = (byte)((c[2] + c[6]) / 2);
                        c[11] = 255;
                        c[12] = 0;
                        c[13] = 0;
                        c[14] = 0;
                        c[15] = 0;
                    }

                    int bytes = data[pos++];
                    bytes |= data[pos++] << 8;
                    bytes |= data[pos++] << 16;
                    bytes |= data[pos++] << 24;

                    for (var yy = 0; yy < 4; yy++)
                    {
                        for (var xx = 0; xx < 4; xx++)
                        {
                            var xpos = x + xx;
                            var ypos = y + yy;
                            if (xpos < width && ypos < height)
                            {
                                var index = bytes & 0x0003;
                                index *= 4;
                                var pointer = ypos * width * 4 + xpos * 4;
                                buffer[pointer + 0] = c[index + 2]; // b
                                buffer[pointer + 1] = c[index + 1]; // g
                                buffer[pointer + 2] = c[index + 0]; // r
                                buffer[pointer + 3] = c[index + 3]; // a
                            }
                            bytes >>= 2;
                        }
                    }
                }
            }
        }

        public static void DecompressDxt3(byte[] buffer, byte[] data, int width, int height)
        {
            var pos = 0;
            var c = new byte[16];
            var a = new byte[8];
            for (var y = 0; y < height; y += 4)
            {
                for (var x = 0; x < width; x += 4)
                {
                    for (var i = 0; i < 8; i++) a[i] = data[pos++];

                    int c0 = data[pos++];
                    c0 |= data[pos++] << 8;

                    int c1 = data[pos++];
                    c1 |= data[pos++] << 8;

                    c[0] = (byte)((c0 & 0xF800) >> 8);
                    c[1] = (byte)((c0 & 0x07E0) >> 3);
                    c[2] = (byte)((c0 & 0x001F) << 3);
                    c[3] = 255;

                    c[4] = (byte)((c1 & 0xF800) >> 8);
                    c[5] = (byte)((c1 & 0x07E0) >> 3);
                    c[6] = (byte)((c1 & 0x001F) << 3);
                    c[7] = 255;

                    c[8] = (byte)((2 * c[0] + c[4]) / 3);
                    c[9] = (byte)((2 * c[1] + c[5]) / 3);
                    c[10] = (byte)((2 * c[2] + c[6]) / 3);
                    c[11] = 255;

                    c[12] = (byte)((c[0] + 2 * c[4]) / 3);
                    c[13] = (byte)((c[1] + 2 * c[5]) / 3);
                    c[14] = (byte)((c[2] + 2 * c[6]) / 3);
                    c[15] = 255;

                    int bytes = data[pos++];
                    bytes |= data[pos++] << 8;
                    bytes |= data[pos++] << 16;
                    bytes |= data[pos++] << 24;

                    for (var yy = 0; yy < 4; yy++)
                    {
                        for (var xx = 0; xx < 4; xx++)
                        {
                            var xpos = x + xx;
                            var ypos = y + yy;
                            var aindex = yy * 4 + xx;
                            if (xpos < width && ypos < height)
                            {
                                var index = bytes & 0x0003;
                                index *= 4;
                                var alpha = (byte)((a[aindex >> 1] >> (aindex << 2 & 0x07)) & 0x0f);
                                alpha = (byte)((alpha << 4) | alpha);
                                var pointer = ypos * width * 4 + xpos * 4;
                                buffer[pointer + 0] = c[index + 2]; // b
                                buffer[pointer + 1] = c[index + 1]; // g
                                buffer[pointer + 2] = c[index + 0]; // r
                                buffer[pointer + 3] = alpha; // a
                            }
                            bytes >>= 2;
                        }
                    }
                }
            }
        }

        public static void DecompressDxt5(byte[] buffer, byte[] data, int width, int height)
        {
            var pos = 0;
            var c = new byte[16];
            var a = new int[8];
            for (var y = 0; y < height; y += 4)
            {
                for (var x = 0; x < width; x += 4)
                {
                    var a0 = data[pos++];
                    var a1 = data[pos++];

                    a[0] = a0;
                    a[1] = a1;

                    if (a0 > a1)
                    {
                        a[2] = (6 * a[0] + 1 * a[1] + 3) / 7;
                        a[3] = (5 * a[0] + 2 * a[1] + 3) / 7;
                        a[4] = (4 * a[0] + 3 * a[1] + 3) / 7;
                        a[5] = (3 * a[0] + 4 * a[1] + 3) / 7;
                        a[6] = (2 * a[0] + 5 * a[1] + 3) / 7;
                        a[7] = (1 * a[0] + 6 * a[1] + 3) / 7;
                    }
                    else
                    {
                        a[2] = (4 * a[0] + 1 * a[1] + 2) / 5;
                        a[3] = (3 * a[0] + 2 * a[1] + 2) / 5;
                        a[4] = (2 * a[0] + 3 * a[1] + 2) / 5;
                        a[5] = (1 * a[0] + 4 * a[1] + 2) / 5;
                        a[6] = 0x00;
                        a[7] = 0xFF;
                    }

                    var aindex = 0L;
                    for (var i = 0; i < 6; i++) aindex |= ((long)data[pos++]) << (8 * i);

                    int c0 = data[pos++];
                    c0 |= data[pos++] << 8;

                    int c1 = data[pos++];
                    c1 |= data[pos++] << 8;

                    c[0] = (byte)((c0 & 0xF800) >> 8);
                    c[1] = (byte)((c0 & 0x07E0) >> 3);
                    c[2] = (byte)((c0 & 0x001F) << 3);
                    c[3] = 255;

                    c[4] = (byte)((c1 & 0xF800) >> 8);
                    c[5] = (byte)((c1 & 0x07E0) >> 3);
                    c[6] = (byte)((c1 & 0x001F) << 3);
                    c[7] = 255;

                    c[8] = (byte)((2 * c[0] + c[4]) / 3);
                    c[9] = (byte)((2 * c[1] + c[5]) / 3);
                    c[10] = (byte)((2 * c[2] + c[6]) / 3);
                    c[11] = 255;

                    c[12] = (byte)((c[0] + 2 * c[4]) / 3);
                    c[13] = (byte)((c[1] + 2 * c[5]) / 3);
                    c[14] = (byte)((c[2] + 2 * c[6]) / 3);
                    c[15] = 255;

                    int bytes = data[pos++];
                    bytes |= data[pos++] << 8;
                    bytes |= data[pos++] << 16;
                    bytes |= data[pos++] << 24;

                    for (var yy = 0; yy < 4; yy++)
                    {
                        for (var xx = 0; xx < 4; xx++)
                        {
                            var xpos = x + xx;
                            var ypos = y + yy;
                            if (xpos < width && ypos < height)
                            {
                                var index = bytes & 0x0003;
                                index *= 4;
                                var alpha = (byte)a[aindex & 0x07];
                                var pointer = ypos * width * 4 + xpos * 4;
                                buffer[pointer + 0] = c[index + 2]; // b
                                buffer[pointer + 1] = c[index + 1]; // g
                                buffer[pointer + 2] = c[index + 0]; // r
                                buffer[pointer + 3] = alpha; // a
                            }
                            bytes >>= 2;
                            aindex >>= 3;
                        }
                    }
                }
            }
        }
    }
}
