using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AR_watch_server
{
    public class Chart
    {
        const int HEIGHT = 350;
        const int WIDTH = 450;
        const float H_RATIO = 3;
        MainWindow father;
        Canvas canvas;
        class DataPoint
        {
            public float timestamp;
            public float x, y, z;
            public DataPoint(float timestamp, float x, float y, float z)
            {
                this.timestamp = timestamp;
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
        List<DataPoint> data = new List<DataPoint>();

        private SynchronizationContext mainThread;

        public Chart(MainWindow father, Canvas canvas)
        {
            this.father = father;
            this.canvas = canvas;
            mainThread = SynchronizationContext.Current;
        }

        public void clear()
        {
            data.Clear();
        }


        public void add(float timestamp, float x, float y, float z)
        {
            data.Add(new DataPoint(timestamp, x, y, z));
            while (timestamp - data[0].timestamp > 1)
            {
                data.Remove(data[0]);
            }
            mainThread.Post(new SendOrPostCallback(draw), null);
        }

        private void formLine(float x1, float y1, float x2, float y2, SolidColorBrush color)
        {
            Line line = new Line();
            line.Stroke = line.Fill = color;
            line.X1 = x1 * WIDTH;
            line.X2 = x2 * WIDTH;
            line.Y1 = HEIGHT / 2 - y1 * H_RATIO;
            line.Y2 = HEIGHT / 2 - y2 * H_RATIO;
            canvas.Children.Add(line);
        }

        public void draw(object state)
        {
            canvas.Children.Clear();
            formLine(0, 0, 1, 0, Brushes.Black);
            float t0 = data[0].timestamp;
            for (int i = 0; i + 1 < data.Count; i++)
            {
                formLine(data[i].timestamp - t0, data[i].x, data[i + 1].timestamp - t0, data[i + 1].x, Brushes.Red);
                formLine(data[i].timestamp - t0, data[i].y, data[i + 1].timestamp - t0, data[i + 1].y, Brushes.Green);
                formLine(data[i].timestamp - t0, data[i].z, data[i + 1].timestamp - t0, data[i + 1].z, Brushes.Blue);
            }
        }
    }
}
