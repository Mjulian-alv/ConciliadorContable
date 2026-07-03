using System;
using System.Drawing;

namespace AgrupadorConceptos
{
    public static class AppIcon
    {
        private static Icon _appIcon;

        public static Icon GetIcon()
        {
            if (_appIcon == null)
            {
                // Generamos un ícono simple en memoria para la aplicación (una institución/banco)
                using (Bitmap bmp = new Bitmap(32, 32))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.Clear(Color.Transparent);

                        // Fondo circular
                        using (Brush bgBrush = new SolidBrush(Color.FromArgb(41, 128, 185))) // Azul corporativo
                        {
                            g.FillEllipse(bgBrush, 2, 2, 28, 28);
                        }
                        
                        // Dibujo de un pequeño banco / columnas (Centro)
                        using (Pen p = new Pen(Color.White, 2))
                        {
                            // Base
                            g.DrawLine(p, 8, 22, 24, 22); 
                            
                            // Columnas
                            g.DrawLine(p, 10, 22, 10, 14); 
                            g.DrawLine(p, 16, 22, 16, 14); 
                            g.DrawLine(p, 22, 22, 22, 14); 
                            
                            // Techo (Templo)
                            g.FillPolygon(Brushes.White, new Point[] {
                                new Point(16, 8),
                                new Point(8, 14),
                                new Point(24, 14)
                            });
                        }
                    }
                    
                    IntPtr hIcon = bmp.GetHicon();
                    _appIcon = Icon.FromHandle(hIcon);
                }
            }
            return _appIcon;
        }
    }
}