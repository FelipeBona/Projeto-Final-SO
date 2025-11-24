using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ProcessExplorer.Controls
{
    /// Controle personalizado para exibir gráficos de performance em tempo real.
    public class PerformanceGraph : Control
    {
        private readonly List<double> _values = new();
        private readonly int _maxDataPoints = 60;
        private string _title = "Performance";
        private Color _lineColor = Color.LimeGreen;
        private Color _gridColor = Color.FromArgb(40, 40, 40);
        private Color _textColor = Color.White;
        private double _maxValue = 100.0;
        private double _currentValue = 0.0;

        public PerformanceGraph()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.Size = new Size(300, 150);
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Invalidate();
            }
        }

        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                Invalidate();
            }
        }

        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                Invalidate();
            }
        }

        /// Adiciona um novo valor ao gráfico.
        public void AddValue(double value)
        {
            _currentValue = value;
            _values.Add(value);

            if (_values.Count > _maxDataPoints)
            {
                _values.RemoveAt(0);
            }

            Invalidate();
        }

        /// Limpa todos os valores do gráfico.
        public void Clear()
        {
            _values.Clear();
            _currentValue = 0.0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawBackground(g);
            DrawGrid(g);
            DrawGraph(g);
            DrawLabels(g);
        }

        private void DrawBackground(Graphics g)
        {
            using var brush = new SolidBrush(this.BackColor);
            g.FillRectangle(brush, this.ClientRectangle);
        }

        private void DrawGrid(Graphics g)
        {
            using var pen = new Pen(_gridColor, 1);
            pen.DashStyle = DashStyle.Dot;

            // Linhas horizontais
            for (int i = 0; i <= 4; i++)
            {
                int y = (int)(this.Height * i / 4.0);
                g.DrawLine(pen, 0, y, this.Width, y);
            }

            // Linhas verticais
            for (int i = 0; i <= 6; i++)
            {
                int x = (int)(this.Width * i / 6.0);
                g.DrawLine(pen, x, 0, x, this.Height);
            }
        }

        private void DrawGraph(Graphics g)
        {
            if (_values.Count < 2) return;

            var points = new List<PointF>();
            float xStep = (float)this.Width / (_maxDataPoints - 1);

            for (int i = 0; i < _values.Count; i++)
            {
                float x = i * xStep;
                float y = this.Height - (float)(_values[i] / _maxValue * this.Height);
                y = Math.Max(0, Math.Min(this.Height, y));
                points.Add(new PointF(x, y));
            }

            if (points.Count < 2) return;

            // Desenhar área preenchida sob o gráfico
            var fillPoints = new List<PointF>(points);
            fillPoints.Add(new PointF(points.Last().X, this.Height));
            fillPoints.Add(new PointF(points.First().X, this.Height));

            using var gradientBrush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(0, this.Height),
                Color.FromArgb(80, _lineColor),
                Color.FromArgb(20, _lineColor));
            g.FillPolygon(gradientBrush, fillPoints.ToArray());

            // Desenhar linha do gráfico
            using var pen = new Pen(_lineColor, 2);
            g.DrawLines(pen, points.ToArray());
        }

        private void DrawLabels(Graphics g)
        {
            using var brush = new SolidBrush(_textColor);
            using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var smallFont = new Font("Segoe UI", 8F);

            // Título
            var titleSize = g.MeasureString(_title, font);
            g.DrawString(_title, font, brush, 5, 5);

            // Valor atual
            string currentText = $"{_currentValue:F1}%";
            var currentSize = g.MeasureString(currentText, font);
            g.DrawString(currentText, font, brush,
                this.Width - currentSize.Width - 5, 5);

            // Escala
            using var scaleBrush = new SolidBrush(Color.FromArgb(180, _textColor));
            g.DrawString($"{_maxValue:F0}", smallFont, scaleBrush, 5, 25);
            g.DrawString("0", smallFont, scaleBrush, 5, this.Height - 20);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
