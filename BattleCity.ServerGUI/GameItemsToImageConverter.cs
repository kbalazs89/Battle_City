using BattleCity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BattleCity.ServerGUI
{
    class GameItemsToImageConverter : IValueConverter
    {
        static Pen redPen = new Pen(Brushes.Red, 2);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<GameItem> items = value as List<GameItem>;
            if (items == null) return null;

            DrawingGroup group = new DrawingGroup();
            foreach (GameItem akt in items)
            {
                DrawingGroup rotatedDrawing = new DrawingGroup();
                GeometryDrawing gd = new GeometryDrawing(akt.ItemBrush, null, new RectangleGeometry(akt.RealRect));
                rotatedDrawing.Children.Add(gd);
                if (akt.Rotation != 0)
                {
                    double CX = (akt.RealRect.Left + akt.RealRect.Right) / 2;
                    double CY = (akt.RealRect.Top + akt.RealRect.Bottom) / 2;
                    rotatedDrawing.Transform = new RotateTransform(akt.Rotation, CX, CY);
                }
                group.Children.Add(rotatedDrawing);

                //group.Children.Add(new GeometryDrawing(null, redPen, new RectangleGeometry(akt.RealRect)));
            }

            DrawingImage img = new DrawingImage();
            img.Drawing = group;
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
