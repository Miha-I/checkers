using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

namespace CheckersGame
{
    class Game
    {
        ~Game()
        {
            g.Dispose();
        }
        private Graphics g = null;
        CheckersBoard board;
        Stack boards;
        Panel peraent;
        Computer computer;
        private static int KING_SIZE = 3, CHIP_SIZE = 5;
        public Game(Panel panel, CheckersBoard checkersBoard)
        {
            boards = new Stack();
            board = checkersBoard;
            peraent = panel;
            computer = new Computer(board);
        }
        public void panel_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            Size d = (sender as Panel).Size;
            int pos, x, y;
            Brush cellColor;
            Brush brushBlack = new SolidBrush(Color.Black);
            Brush brushWhite = new SolidBrush(Color.White);
            Brush brushLightGreen = new SolidBrush(Color.LightGreen);
            Brush brushBurlyWood = new SolidBrush(Color.BurlyWood);
            Brush brushChocolate = new SolidBrush(Color.Chocolate);
            Brush brushDarkOliveGreen = new SolidBrush(Color.DarkOliveGreen);

            // Поле
            for (y = 0; y < 8; y++)
                for (x = 0; x < 8; x++)
                {
                    if ((x + y) % 2 != 0)
                    {
                        pos = y * 4 + (x + ((y % 2 == 0) ? 0 : 1)) / 2;

                        if (board.selected.has(pos))
                            cellColor = brushLightGreen;       // Если фишка выбрана клетка зелёного цвета
                        else
                            cellColor = brushBlack;
                    }
                    else
                        cellColor = brushWhite;
                    g.FillRectangle(cellColor, x * 50, y * 50, 50 - 1, 50 - 1);
                }

            // Фишки
            byte cell;
            for (int i = 0; i < 32; i++)
                try
                {
                    cell = board.getCell(i);
                    if (cell != CheckersBoard.EMPTY)
                    {
                        y = i / 4;
                        x = (i % 4) * 2 + (y % 2 == 0 ? 1 : 0);
                        if (cell == CheckersBoard.BLACK || cell == CheckersBoard.BLACK_KING)
                            g.FillEllipse(brushBurlyWood, CHIP_SIZE + x * 50, CHIP_SIZE + y * 50, 50 - 1 - 2 * CHIP_SIZE, 50 - 1 - 2 * CHIP_SIZE);
                        else
                            g.FillEllipse(brushChocolate, CHIP_SIZE + x * 50, CHIP_SIZE + y * 50, 50 - 1 - 2 * CHIP_SIZE, 50 - 1 - 2 * CHIP_SIZE);


                        if (cell == CheckersBoard.WHITE_KING)
                        {
                            g.DrawEllipse(new Pen(brushDarkOliveGreen), KING_SIZE + x * 50, KING_SIZE + y * 50, 50 - KING_SIZE, 50 - KING_SIZE);
                        }
                        else if (cell == CheckersBoard.BLACK_KING)
                        {
                            g.DrawEllipse(new Pen(brushDarkOliveGreen), KING_SIZE + x * 50, KING_SIZE + y * 50, 50 - KING_SIZE, 50 - KING_SIZE);
                        }
                    }
                }
                catch
                {
                }
        }

        // Клик мышки по полю
        public void panel_MouseClick(MouseEventArgs e)
        {
            int pos;
            pos = board.getСellPos(e.X, e.Y);
            if (pos != -1)
                try
                {
                    int cell = board.getCell(pos);
                    if (cell != CheckersBoard.EMPTY &&                  // Если выбрана фишка текущего игрока
                        (((cell == CheckersBoard.WHITE || cell == CheckersBoard.WHITE_KING) && board.getCurrentPlayer() == CheckersBoard.WHITE) ||
                          ((cell == CheckersBoard.BLACK || cell == CheckersBoard.BLACK_KING) && board.getCurrentPlayer() == CheckersBoard.BLACK)))
                    {
                        if (board.selected.isEmpty())
                            board.selected.push_back(pos);
                        else
                        {
                            int temp = (int)board.selected.peek_tail();

                            if (temp == pos)
                                board.selected.pop_back();
                            else
                            {
                                MessageBox.Show("не правильный ход", "Ошибка");
                            }
                        }
                        peraent.Invalidate();
                        return;
                    }
                    else                                                // Если выбрана пустая клетка или фишка другого игрока
                    {
                        bool good = false;
                        CheckersBoard tempBoard;
                        if (!board.selected.isEmpty())                  // Если фишка выбрана
                        {
                            if (boards.Count == 0)
                            {
                                tempBoard = (CheckersBoard)board.clone();
                                boards.Push(tempBoard);
                            }
                            else
                                tempBoard = (CheckersBoard)boards.Peek();
                            int from = (int)board.selected.peek_tail();
                            if (tempBoard.isValidMove(from, pos))
                            {
                                tempBoard = (CheckersBoard)tempBoard.clone();
                                bool isAttacking = tempBoard.mustAttack();
                                tempBoard.move(from, pos);
                                if (isAttacking && tempBoard.mayAttack(pos))
                                {
                                    board.selected.push_back(pos);
                                    boards.Push(tempBoard);
                                }
                                else
                                {
                                    board.selected.push_back(pos);
                                    makeMoves(board.selected, board);
                                    peraent.Invalidate();
                                    board.selected.clear();
                                    boards = new Stack();
                                    if (!gameEnded())
                                    {
                                        Thread.Sleep(1000);
                                        computer.play();
                                        peraent.Invalidate();
                                    }
                                    //boards = new Stack();
                                }
                                good = true;
                            }
                            else if (from == pos)
                            {
                                board.selected.pop_back();
                                boards.Pop();
                                good = true;
                            }
                        }
                        if (!good)
                        {
                            MessageBox.Show("не правильный ход", "Ошибка");
                        }
                        else
                        {
                            peraent.Invalidate();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("не правильный ход", "Ошибка");
                }
        }

        // Передвижение фишек по доске
        private void makeMoves(List moves, CheckersBoard board)
        {
            List moveList = new List();
            int from, to = 0;
            from = (int)moves.pop_front();
            while (!moves.isEmpty())
            {
                to = (int)moves.pop_front();
                moveList.push_back(new Move(from, to));
                from = to;
            }
            board.move(moveList);
        }

        // Проверка окончания игры
        private bool gameEnded()
        {
            if (board.whiteCell == 0 || board.blackCell == 0)
            {
                MessageBox.Show("Черные выиграли", "Игра окончена");
                return true;
            }
            else if (board.blackCell == 0)
            {
                MessageBox.Show("Белые выиграли", "Игра окончена");
                return true;
            }
            else
                return false;
        }
    }
}
