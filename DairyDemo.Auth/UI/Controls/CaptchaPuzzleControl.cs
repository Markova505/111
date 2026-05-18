using System.Drawing;

namespace DairyDemo.Auth.UI.Controls;

public class CaptchaPuzzleControl : UserControl
{
    private List<Image> _fragments = new();
    private List<int> _correctOrder = new();
    private List<int> _userOrder = new();
    private Random _random = new();
    private int _fragmentWidth;
    private int _fragmentHeight;
    private int _rows = 2;
    private int _cols = 2;

    public event EventHandler? PuzzleCompleted;

    public CaptchaPuzzleControl()
    {
        DoubleBuffered = true;
        _userOrder = new List<int>();
    }

    public void InitializeCaptcha(string captchaFolder)
    {
        _fragments.Clear();
        _correctOrder.Clear();
        _userOrder.Clear();

        // Загружаем изображения фрагментов
        for (int i = 1; i <= 4; i++)
        {
            var path = Path.Combine(captchaFolder, $"{i}.png");
            if (File.Exists(path))
            {
                _fragments.Add(Image.FromFile(path));
            }
        }

        if (_fragments.Count == 0)
        {
            // Создаем тестовые фрагменты если файлы не найдены
            CreateTestFragments();
        }

        _fragmentWidth = Width / _cols;
        _fragmentHeight = Height / _rows;

        // Генерируем правильный порядок (1, 2, 3, 4)
        for (int i = 0; i < _fragments.Count; i++)
        {
            _correctOrder.Add(i);
        }

        // Перемешиваем для отображения
        var shuffled = _correctOrder.OrderBy(x => _random.Next()).ToList();
        _userOrder = new List<int>(shuffled);

        Invalidate();
    }

    private void CreateTestFragments()
    {
        for (int i = 0; i < 4; i++)
        {
            var bitmap = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.LightGray);
                g.DrawString($"{i + 1}", new Font("Arial", 48, FontStyle.Bold), 
                    Brushes.Black, new PointF(30, 25));
            }
            _fragments.Add(bitmap);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        _fragmentWidth = Width / _cols;
        _fragmentHeight = Height / _rows;

        for (int i = 0; i < _userOrder.Count; i++)
        {
            int row = i / _cols;
            int col = i % _cols;
            int x = col * _fragmentWidth;
            int y = row * _fragmentHeight;

            if (i < _fragments.Count)
            {
                e.Graphics.DrawImage(_fragments[_userOrder[i]], x, y, _fragmentWidth, _fragmentHeight);
            }
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

        int col = e.X / _fragmentWidth;
        int row = e.Y / _fragmentHeight;
        int index = row * _cols + col;

        if (index >= 0 && index < _userOrder.Count)
        {
            // Меняем местами выбранный фрагмент с первым кликом
            if (_selectedIndices.Count == 0)
            {
                _selectedIndices.Add(index);
            }
            else if (_selectedIndices.Count == 1)
            {
                int firstIndex = _selectedIndices[0];
                if (firstIndex != index)
                {
                    // Меняем местами
                    var temp = _userOrder[firstIndex];
                    _userOrder[firstIndex] = _userOrder[index];
                    _userOrder[index] = temp;
                    _selectedIndices.Clear();
                    Invalidate();
                    CheckCompletion();
                }
                else
                {
                    _selectedIndices.Clear();
                }
            }
        }
    }

    private List<int> _selectedIndices = new();

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);

        // Рисуем рамку вокруг выбранного фрагмента
        if (_selectedIndices.Count > 0)
        {
            int index = _selectedIndices[0];
            int col = index % _cols;
            int row = index / _cols;
            int x = col * _fragmentWidth;
            int y = row * _fragmentHeight;

            using (var pen = new Pen(Color.Red, 3))
            {
                e.Graphics.DrawRectangle(pen, x, y, _fragmentWidth - 1, _fragmentHeight - 1);
            }
        }
    }

    private void CheckCompletion()
    {
        bool isCorrect = true;
        for (int i = 0; i < _userOrder.Count; i++)
        {
            if (_userOrder[i] != _correctOrder[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            PuzzleCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsSolved()
    {
        for (int i = 0; i < _userOrder.Count; i++)
        {
            if (_userOrder[i] != _correctOrder[i])
                return false;
        }
        return true;
    }

    public List<int> GetUserOrder() => new List<int>(_userOrder);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var fragment in _fragments)
            {
                fragment.Dispose();
            }
        }
        base.Dispose(disposing);
    }
}
