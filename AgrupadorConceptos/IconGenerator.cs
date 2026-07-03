using System;
using System.Drawing;
using System.IO;

namespace AgrupadorConceptos
{
    public static class IconGenerator
    {
        public static void GenerateIconFile()
        {
            try
            {
                using (var icon = AppIcon.GetIcon())
                {
                    using (var fs = new FileStream("AppIcon.ico", FileMode.Create))
                    {
                        icon.Save(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating icon file: {ex.Message}");
            }
        }
    }
}