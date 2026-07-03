using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ArcaCliente
{
    /// <summary>
    /// Proveedor centralizado del icono de ARCA para todos los formularios.
    /// El icono se genera una sola vez (singleton) usando GDI+:
    /// fondo azul redondeado con la letra "A" blanca en negrita.
    /// </summary>
    internal static class AppIcons
    {
        private static Icon _arca;

        /// <summary>Icono principal de ARCA (singleton, thread-safe por inicialización tardía).</summary>
        public static Icon Arca => _arca ??= BuildIcon();

        // ?? Construcción ??????????????????????????????????????????????????????????

        private static Icon BuildIcon()
        {
            // Crear dos tamaños para una mejor visualización en pantallas HiDPI
            using var bmp32 = RenderBitmap(32);
            using var bmp16 = RenderBitmap(16);
            return CombineIntoIcon(bmp16, bmp32);
        }

        private static Bitmap RenderBitmap(int size)
        {
            var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.Clear(Color.Transparent);

            // Fondo: rectángulo redondeado azul ARCA
            float radius = MathF.Max(2f, size / 5f);
            var rect = new RectangleF(0.5f, 0.5f, size - 1.5f, size - 1.5f);
            using var path = CreateRoundedPath(rect, radius);
            using var bg   = new SolidBrush(Color.FromArgb(0, 82, 164));
            g.FillPath(bg, path);

            // Borde blanco semitransparente
            using var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 0.8f);
            g.DrawPath(pen, path);

            // Letra "A" blanca centrada
            float fontSize = size * 0.60f;
            using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(Color.White);
            var sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString("A", font, brush, new RectangleF(0, 0, size, size), sf);

            return bmp;
        }

        /// <summary>Construye un <see cref="Icon"/> multi-resolución a partir de varios bitmaps.</summary>
        private static Icon CombineIntoIcon(params Bitmap[] bitmaps)
        {
            // Escribir el formato ICO manualmente (soporta PNG comprimido desde Vista+)
            using var ms = new System.IO.MemoryStream();
            var writer = new System.IO.BinaryWriter(ms);

            // ICONDIR header
            writer.Write((short)0);              // reserved
            writer.Write((short)1);              // type: ICO
            writer.Write((short)bitmaps.Length); // image count

            // Pre-calcular los PNG de cada bitmap
            var pngs = new byte[bitmaps.Length][];
            for (int i = 0; i < bitmaps.Length; i++)
            {
                using var png = new System.IO.MemoryStream();
                bitmaps[i].Save(png, System.Drawing.Imaging.ImageFormat.Png);
                pngs[i] = png.ToArray();
            }

            // ICONDIRENTRY por cada imagen
            int offset = 6 + bitmaps.Length * 16;
            for (int i = 0; i < bitmaps.Length; i++)
            {
                int w = bitmaps[i].Width;
                int h = bitmaps[i].Height;
                writer.Write((byte)(w >= 256 ? 0 : w));  // width  (0 = 256)
                writer.Write((byte)(h >= 256 ? 0 : h));  // height (0 = 256)
                writer.Write((byte)0);   // color count
                writer.Write((byte)0);   // reserved
                writer.Write((short)1);  // planes
                writer.Write((short)32); // bit count
                writer.Write(pngs[i].Length);
                writer.Write(offset);
                offset += pngs[i].Length;
            }

            // Datos PNG de cada imagen
            foreach (var png in pngs)
                writer.Write(png);

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            return new Icon(ms);
        }

        private static GraphicsPath CreateRoundedPath(RectangleF r, float radius)
        {
            float d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.Left,          r.Top,           d, d, 180, 90);
            p.AddArc(r.Right - d,     r.Top,           d, d, 270, 90);
            p.AddArc(r.Right - d,     r.Bottom - d,    d, d,   0, 90);
            p.AddArc(r.Left,          r.Bottom - d,    d, d,  90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
