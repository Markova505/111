using System;
using System.Drawing;
using System.Windows.Forms;

namespace AuthSystem
{
    public partial class PuzzleCaptchaForm : Form
    {
        private Bitmap originalImage;
        private Bitmap[] puzzlePieces;
        private int gridSize = 3; // 3x3 сетка
        private Point[] correctPositions;
        private Point[] currentPositions;
        private PictureBox[] piecePictureBoxes;
        private Panel dropZonePanel;
        private int puzzleAttempts = 0;
        private const int MaxAttempts = 3;

        public bool IsPuzzleSolved { get; private set; } = false;

        public PuzzleCaptchaForm()
        {
            InitializeComponent();
            InitializePuzzle();
        }

        private void InitializeComponent()
        {
            this.Text = "Капча - Пазл";
            this.Size = new Size(500, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            var instructionLabel = new Label
            {
                Text = "Соберите изображение из фрагментов. Перетащите фрагменты на правильные места.",
                AutoSize = true,
                Location = new Point(20, 15),
                MaximumSize = new Size(450, 0)
            };
            this.Controls.Add(instructionLabel);

            dropZonePanel = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(300, 300),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };
            this.Controls.Add(dropZonePanel);

            var piecesPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 370),
                Size = new Size(450, 180),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true
            };
            this.Controls.Add(piecesPanel);

            var solveButton = new Button
            {
                Text = "Проверить",
                Location = new Point(20, 520),
                Size = new Size(100, 30)
            };
            solveButton.Click += SolveButton_Click;
            this.Controls.Add(solveButton);

            var refreshButton = new Button
            {
                Text = "Обновить пазл",
                Location = new Point(140, 520),
                Size = new Size(120, 30)
            };
            refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(refreshButton);

            this.FormClosing += PuzzleCaptchaForm_FormClosing;
        }

        private void PuzzleCaptchaForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!IsPuzzleSolved && puzzleAttempts >= MaxAttempts)
            {
                // Пазл не решен и попытки исчерпаны
            }
        }

        private void InitializePuzzle()
        {
            IsPuzzleSolved = false;
            puzzleAttempts = 0;

            // Создаем простое изображение с текстом или градиентом
            originalImage = new Bitmap(300, 300);
            using (Graphics g = Graphics.FromImage(originalImage))
            {
                g.Clear(Color.White);
                
                // Рисуем градиент
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(0, 0), new Point(300, 300),
                    Color.LightBlue, Color.DarkBlue))
                {
                    g.FillRectangle(brush, 0, 0, 300, 300);
                }

                // Рисуем текст
                using (var font = new Font("Arial", 24, FontStyle.Bold))
                {
                    g.DrawString("CAPTCHA", font, Brushes.White, new PointF(60, 120));
                }

                // Добавляем несколько фигур для сложности
                g.DrawEllipse(Pens.Orange, 50, 50, 80, 80);
                g.DrawRectangle(Pens.Red, 180, 180, 70, 70);
                g.DrawLine(Pens.Green, 20, 250, 280, 50);
            }

            int pieceWidth = originalImage.Width / gridSize;
            int pieceHeight = originalImage.Height / gridSize;

            puzzlePieces = new Bitmap[gridSize * gridSize];
            correctPositions = new Point[gridSize * gridSize];
            currentPositions = new Point[gridSize * gridSize];
            piecePictureBoxes = new PictureBox[gridSize * gridSize];

            // Разрезаем изображение на части
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    int index = i * gridSize + j;
                    
                    var piece = new Bitmap(pieceWidth, pieceHeight);
                    using (Graphics g = Graphics.FromImage(piece))
                    {
                        g.DrawImage(originalImage, 
                            new Rectangle(0, 0, pieceWidth, pieceHeight),
                            new Rectangle(j * pieceWidth, i * pieceHeight, pieceWidth, pieceHeight),
                            GraphicsUnit.Pixel);
                    }
                    
                    puzzlePieces[index] = piece;
                    correctPositions[index] = new Point(j, i);
                    
                    // Случайная позиция для начального размещения
                    Random rand = new Random(Guid.NewGuid().GetHashCode());
                    int randomIndex = rand.Next(gridSize * gridSize);
                    while (currentPositions[randomIndex].X != 0 || currentPositions[randomIndex].Y != 0)
                    {
                        randomIndex = (randomIndex + 1) % (gridSize * gridSize);
                    }
                    currentPositions[randomIndex] = new Point(j, i);
                }
            }

            // Перемешиваем позиции
            ShufflePositions();

            // Создаем зоны для сброса
            CreateDropZones();

            // Создаем pictureBox для каждого фрагмента
            CreatePiecePictureBoxes();
        }

        private void ShufflePositions()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < gridSize * gridSize; i++)
            {
                int j = rand.Next(i, gridSize * gridSize);
                var temp = currentPositions[i];
                currentPositions[i] = currentPositions[j];
                currentPositions[j] = temp;
            }
        }

        private void CreateDropZones()
        {
            dropZonePanel.Controls.Clear();
            int pieceWidth = dropZonePanel.Width / gridSize;
            int pieceHeight = dropZonePanel.Height / gridSize;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    var zone = new Panel
                    {
                        Location = new Point(j * pieceWidth, i * pieceHeight),
                        Size = new Size(pieceWidth, pieceHeight),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.WhiteSmoke,
                        Tag = $"{i},{j}" // Координаты зоны
                    };
                    
                    // Разрешаем Drop
                    zone.AllowDrop = true;
                    zone.DragEnter += Zone_DragEnter;
                    zone.DragDrop += Zone_DragDrop;
                    
                    dropZonePanel.Controls.Add(zone);
                }
            }
        }

        private void CreatePiecePictureBoxes()
        {
            var piecesPanel = this.Controls.Find("", true)[0] as FlowLayoutPanel;
            if (piecesPanel == null)
            {
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is FlowLayoutPanel)
                    {
                        piecesPanel = ctrl;
                        break;
                    }
                }
            }

            if (piecesPanel != null)
            {
                piecesPanel.Controls.Clear();
                int pieceWidth = dropZonePanel.Width / gridSize;
                int pieceHeight = dropZonePanel.Height / gridSize;

                for (int i = 0; i < puzzlePieces.Length; i++)
                {
                    var pictureBox = new PictureBox
                    {
                        Image = puzzlePieces[i],
                        Size = new Size(pieceWidth, pieceHeight),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Cursor = Cursors.Hand,
                        Tag = i // Индекс фрагмента
                    };

                    pictureBox.MouseDown += PictureBox_MouseDown;
                    piecesPanel.Controls.Add(pictureBox);
                    piecePictureBoxes[i] = pictureBox;
                }
            }
        }

        private PictureBox? draggedPiece = null;
        private Panel? sourceZone = null;

        private void PictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is PictureBox pictureBox && e.Button == MouseButtons.Left)
            {
                draggedPiece = pictureBox;
                pictureBox.DoDragDrop(pictureBox, DragDropEffects.Move);
            }
        }

        private void Zone_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PictureBox)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Zone_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(PictureBox)) is PictureBox pictureBox && sender is Panel targetZone)
            {
                // Удаляем из предыдущего контейнера
                pictureBox.Parent?.Controls.Remove(pictureBox);
                
                // Устанавливаем новые размеры и позицию
                pictureBox.Location = new Point(0, 0);
                pictureBox.Size = targetZone.Size;
                
                // Добавляем в новую зону
                targetZone.Controls.Add(pictureBox);
            }
        }

        private void SolveButton_Click(object? sender, EventArgs e)
        {
            puzzleAttempts++;
            
            bool isCorrect = CheckPuzzle();
            
            if (isCorrect)
            {
                IsPuzzleSolved = true;
                MessageBox.Show("Пазл собран верно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                if (puzzleAttempts >= MaxAttempts)
                {
                    MessageBox.Show($"Вы превысили количество попыток ({MaxAttempts}). Капча не пройдена.", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Неверно! Осталось попыток: {MaxAttempts - puzzleAttempts}", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private bool CheckPuzzle()
        {
            int pieceWidth = dropZonePanel.Width / gridSize;
            int pieceHeight = dropZonePanel.Height / gridSize;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    var zone = GetZoneAtPosition(i, j);
                    if (zone == null || zone.Controls.Count == 0)
                    {
                        return false; // Зона пуста
                    }

                    if (zone.Controls[0] is PictureBox pictureBox)
                    {
                        int pieceIndex = (int)pictureBox.Tag;
                        Point correctPos = correctPositions[pieceIndex];
                        
                        // Проверяем, соответствует ли фрагмент своей позиции
                        if (correctPos.X != j || correctPos.Y != i)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private Panel? GetZoneAtPosition(int row, int col)
        {
            foreach (Control ctrl in dropZonePanel.Controls)
            {
                if (ctrl is Panel zone && zone.Tag?.ToString() == $"{row},{col}")
                {
                    return zone;
                }
            }
            return null;
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            // Очищаем ресурсы
            foreach (var piece in puzzlePieces)
            {
                piece?.Dispose();
            }
            originalImage?.Dispose();

            InitializePuzzle();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            
            // Освобождаем ресурсы
            foreach (var piece in puzzlePieces)
            {
                piece?.Dispose();
            }
            originalImage?.Dispose();
        }
    }
}
