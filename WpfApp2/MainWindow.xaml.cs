using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private const char Separator = ',';
        private string[,] piezasIniciales = new string[,]
        {
            { "RookRed", "KnightRed", "BishopRed", "QueenRed", "KingRed", "BishopRed", "KnightRed", "RookRed" },
            { "PawnRed", "PawnRed", "PawnRed", "PawnRed", "PawnRed", "PawnRed", "PawnRed", "PawnRed" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "", "", "", "", "", "", "", "" },
            { "PawnBlue", "PawnBlue", "PawnBlue", "PawnBlue", "PawnBlue", "PawnBlue", "PawnBlue", "PawnBlue" },
            { "RookBlue", "KnightBlue", "BishopBlue", "QueenBlue", "KingBlue", "BishopBlue", "KnightBlue", "RookBlue" }
        };

        private bool esTurnoRojo = false; // Inicia el juego con el turno de las piezas azules
        private bool juegoTerminado = false;

        public MainWindow()
        {
            InitializeComponent();
            CrearTablero();
            ActualizarTurnoTextBlock();
        }

        private void CrearTablero()
        {
            ChessBoard.Children.Clear();

            for (int fila = 0; fila < 8; fila++)
            {
                for (int columna = 0; columna < 8; columna++)
                {
                    var casilla = new Border
                    {
                        Background = (fila + columna) % 2 == 0 ? Brushes.LightGray : Brushes.DarkGray,
                        AllowDrop = true,
                        Tag = $"{fila},{columna}"
                    };

                    casilla.Drop += Casilla_Drop;
                    casilla.DragOver += Casilla_DragOver;

                    string pieza = piezasIniciales[fila, columna];
                    if (!string.IsNullOrEmpty(pieza))
                    {
                        Path path = new Path
                        {
                            Style = (Style)FindResource(pieza),
                            Tag = $"{fila},{columna}",
                            DataContext = pieza.Contains("Red") ? "Red" : "Blue"
                        };

                        path.MouseLeftButtonDown += OnPiezaMouseLeftButtonDown;

                        casilla.Child = path;
                    }

                    ChessBoard.Children.Add(casilla);
                }
            }
        }

        private void OnPiezaMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (juegoTerminado) return;

            Path pieza = sender as Path;
            if (pieza == null) return;

            bool esPiezaRoja = pieza.DataContext.ToString() == "Red";
            if ((esTurnoRojo && !esPiezaRoja) || (!esTurnoRojo && esPiezaRoja))
            {
                return;
            }

            DataObject dragData = new DataObject("pieza", pieza);
            DragDrop.DoDragDrop(pieza, dragData, DragDropEffects.Move);
        }

        private void Casilla_Drop(object sender, DragEventArgs e)
        {
            if (juegoTerminado) return;

            if (e.Data.GetDataPresent("pieza"))
            {
                Path pieza = (Path)e.Data.GetData("pieza");
                Border casilla = (Border)sender;

                string posDestino = (string)casilla.Tag;
                var coords = posDestino.Split(Separator);

                int filaDestino = int.Parse(coords[0]);
                int columnaDestino = int.Parse(coords[1]);

                var antiguaPos = pieza.Tag.ToString().Split(Separator);
                int filaAntigua = int.Parse(antiguaPos[0]);
                int columnaAntigua = int.Parse(antiguaPos[1]);

                string tipoPieza = piezasIniciales[filaAntigua, columnaAntigua].Replace("Red", "").Replace("Blue", "");
                bool movimientoValido = false;

                switch (tipoPieza)
                {
                    case "Pawn":
                        movimientoValido = EsMovimientoValidoPeon(filaAntigua, columnaAntigua, filaDestino, columnaDestino, casilla.Child != null);
                        break;
                    case "Rook":
                        movimientoValido = EsMovimientoValidoTorre(filaAntigua, columnaAntigua, filaDestino, columnaDestino);
                        break;
                    case "Knight":
                        movimientoValido = EsMovimientoValidoCaballo(filaAntigua, columnaAntigua, filaDestino, columnaDestino);
                        break;
                    case "Bishop":
                        movimientoValido = EsMovimientoValidoAlfil(filaAntigua, columnaAntigua, filaDestino, columnaDestino);
                        break;
                    case "Queen":
                        movimientoValido = EsMovimientoValidoReina(filaAntigua, columnaAntigua, filaDestino, columnaDestino);
                        break;
                    case "King":
                        movimientoValido = EsMovimientoValidoRey(filaAntigua, columnaAntigua, filaDestino, columnaDestino);
                        break;
                }

                bool esCapturaValida = false;
                if (casilla.Child is Path piezaDestino)
                {
                    bool esPiezaDestinoRoja = piezaDestino.DataContext.ToString() == "Red";
                    esCapturaValida = (esTurnoRojo && !esPiezaDestinoRoja) || (!esTurnoRojo && esPiezaDestinoRoja);

                    if (piezaDestino.Tag.ToString().Contains("King"))
                    {
                        juegoTerminado = true;
                        GameOverTextBlock.Text = "Game Over";
                    }
                }

                if (!movimientoValido || (casilla.Child != null && !esCapturaValida)) return;

                Border antiguaCasilla = pieza.Parent as Border;
                if (antiguaCasilla != null)
                {
                    antiguaCasilla.Child = null;
                }

                if (esCapturaValida)
                {
                    casilla.Child = null;
                }

                casilla.Child = pieza;

                piezasIniciales[filaAntigua, columnaAntigua] = "";
                piezasIniciales[filaDestino, columnaDestino] = pieza.DataContext.ToString() + tipoPieza;

                pieza.Tag = $"{filaDestino},{columnaDestino}";

                esTurnoRojo = !esTurnoRojo;
                ActualizarTurnoTextBlock();
            }
        }

        private void Casilla_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void ActualizarTurnoTextBlock()
        {
            TurnoTextBlock.Text = esTurnoRojo ? "Turno de las piezas rojas" : "Turno de las piezas azules";
        }

        private bool EsMovimientoValidoPeon(int filaAntigua, int columnaAntigua, int filaDestino, int columnaDestino, bool captura)
        {
            if (!captura && columnaAntigua == columnaDestino && filaDestino == filaAntigua + (esTurnoRojo ? 1 : -1))
                return true;

            if (!captura && columnaAntigua == columnaDestino && filaDestino == filaAntigua + (esTurnoRojo ? 2 : -2) &&
                (filaAntigua == 1 || filaAntigua == 6))
                return true;

            if (captura && Math.Abs(columnaDestino - columnaAntigua) == 1 && filaDestino == filaAntigua + (esTurnoRojo ? 1 : -1))
                return true;

            return false;
        }

        private bool EsMovimientoValidoTorre(int filaAntigua, int columnaAntigua, int filaDestino, int columnaDestino)
        {
            return filaAntigua == filaDestino || columnaAntigua == columnaDestino;
        }

        private bool EsMovimientoValidoCaballo(int filaAntigua, int columnaAntigua, int filaDestino, int columnaDestino)
        {
            int filaDiff = Math.Abs(filaDestino - filaAntigua);
            int columnaDiff = Math.Abs(columnaDestino - columnaAntigua);
            return (filaDiff == 2 && columnaDiff == 1) || (filaDiff == 1 && columnaDiff == 2);
        }

        private bool EsMovimientoValidoAlfil(int filaAntigua, int columnaAntigua, int filaDestino, int columnaDestino)
        {
            return Math.Abs(filaDestino - filaAntigua) == Math.Abs(columnaDestino - columnaAntigua);
        }

        private bool EsMovimientoValidoReina(int filaAntigua, int columnaAntigua, int filaDestino, int columnaDestino)
        {
            return EsMovimientoValidoTorre(filaAntigua, columnaAntigua, filaDestino, columnaDestino) ||
                   EsMovimientoValidoAlfil(filaAntigua, columnaAntigua, filaDestino, columnaDestino);
        }

        private bool EsMovimientoValidoRey(int filaAntigua, int columnaAntigua, int filaDestino, int columnaDestino)
        {
            int filaDiff = Math.Abs(filaDestino - filaAntigua);
            int columnaDiff = Math.Abs(columnaDestino - columnaAntigua);
            return filaDiff <= 1 && columnaDiff <= 1;
        }
    }
}