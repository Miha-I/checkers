using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckersGame
{
    class CheckersBoard
    {
        const byte Player = WHITE;
        delegate void OnPaintPanel();
        static OnPaintPanel Invalidate;
        private byte[] cells;
        public const byte EMPTY = 0;
        public const byte WHITE = 2;
        public const byte WHITE_KING = 3;
        public const byte BLACK = 4;
        public const byte BLACK_KING = 5;
        private const byte KING = 1;
        private const int NONE = 0;                         // первый ход
        private const int LEFT_BELOW = 1;                   // диоганаль v/
        private const int LEFT_ABOVE = 2;                   // диоганаль ^\
        private const int RIGHT_BELOW = 3;                  // диоганаль \v
        private const int RIGHT_ABOVE = 4;                  // диоганаль /^
        public List selected;
        public int whiteCell;
        public int blackCell;
        private int currentPlayer;
        Panel peraent;
        public CheckersBoard()
        {
            cells = new byte[32];
            clearBoard();
        }
        public CheckersBoard(Panel panel)
        {
            cells = new byte[32];
            Invalidate = new OnPaintPanel(panel.Invalidate);
            peraent = panel;
            selected = new List();
            
            currentPlayer = Player;
            clearBoard();
        }

        // Возврат клетки (что на ней находится)
        public byte getCell(int pos) => cells[pos];

        // Установка фишек на начальные позиции
        public void clearBoard()
        {
            int i;
            whiteCell = 12;
            blackCell = 12;
            currentPlayer = Player;

            for (i = 0; i < 12; i++)
                cells[i] = BLACK;

            for (i = 12; i < 20; i++)
                cells[i] = EMPTY;

            for (i = 20; i < 32; i++)
                cells[i] = WHITE;
        }

        // Номер клетки
        public int getСellPos(int currentX, int currentY)
        {
            for (int i = 0; i < 32; i++)
            {
                int x, y;
                y = i / 4;
                x = (i % 4) * 2 + (y % 2 == 0 ? 1 : 0);
                if (x * 50 < currentX && currentX < (x + 1) * 50 && y * 50 < currentY && currentY < (y + 1) * 50)
                    return i;
            }
            return -1;
        }

        // Цвет фишек игрока
        public int getCurrentPlayer()
        {
            return currentPlayer;
        }

        // Копирование текущей доски
        public object clone()
        {
            CheckersBoard board = new CheckersBoard();

            board.currentPlayer = currentPlayer;
            board.whiteCell = whiteCell;
            board.blackCell = blackCell;
            for (int i = 0; i < 32; i++)
                board.cells[i] = cells[i];

            return board;
        }

        // Проверка правильности хода
        public bool isValidMove(int from, int to)
        {
            if (cells[to] != EMPTY && (cells[from] & ~KING) != currentPlayer)
                return false;

            int color;
            int enemy;
            color = cells[from] & ~KING;
            if (color == WHITE)
                enemy = BLACK;
            else
                enemy = WHITE;

            int fromLine = from / 4;
            int fromCol = (from % 4) * 2 + ((from / 4) % 2 == 0 ? 1 : 0);
            int toLine = to / 4;
            int toCol = (to % 4) * 2 + ((to / 4) % 2 == 0 ? 1 : 0);

            int incX, incY;

            if (fromCol > toCol)
                incX = -1;
            else
                incX = 1;


            if (fromLine > toLine)
                incY = -1;
            else
                incY = 1;

            int x = fromCol + incX;
            int y = fromLine + incY;


            if ((cells[from] & KING) == 0)
            {
                bool goodDir;

                if ((incY == -1 && color == WHITE) || (incY == 1 && color == BLACK))
                    goodDir = true;
                else
                    goodDir = false;

                if (x == toCol && y == toLine) // Simple move
                    return goodDir && !mustAttack();

                // Если это был не простой ход, то это может быть только атакующий ход.
                return goodDir && x + incX == toCol && y + incY == toLine &&
                       (cells[colLineToPos(x, y)] & ~KING) == enemy;
            }
            else                                                        // Если это дамка
            {
                while (x != toCol && y != toLine && cells[colLineToPos(x, y)] == EMPTY)
                {
                    x += incX;
                    y += incY;
                }

                // Проверка хода дамки
                if (x == toCol && y == toLine)
                    return !mustAttack();

                if ((cells[colLineToPos(x, y)] & ~KING) == enemy)
                {
                    x += incX;
                    y += incY;

                    while (x != toCol && y != toLine && cells[colLineToPos(x, y)] == EMPTY)
                    {
                        x += incX;
                        y += incY;
                    }

                    if (x == toCol && y == toLine)
                        return true;
                }
            }
            return false;
        }

        // обработчик хода.
        public List legalMoves()
        {
            int color;
            int enemy;

            color = currentPlayer;
            if (color == WHITE)
                enemy = BLACK;
            else
                enemy = WHITE;

            if (mustAttack())
                return generateAttackMoves(color, enemy);
            else
                return generateMoves(color, enemy);
        }

        // Поиск всех возможных ходов для атаки
        private List generateAttackMoves(int color, int enemy)
        {
            List moves = new List();
            List tempMoves;
            for (int k = 0; k < 32; k++)
                if ((cells[k] & ~KING) == currentPlayer)
                {
                    if ((cells[k] & KING) == 0)
                        tempMoves = simpleAttack(k, color, enemy);
                    else
                    { // Это королевская фигура?
                        List lastPos = new List();

                        lastPos.push_back(k);

                        tempMoves = kingAttack(lastPos, k, NONE, color, enemy);
                    }

                    if (notNull(tempMoves))
                        moves.append(tempMoves);
                }
            return moves;
        }

        // Поиск всех ходов атаки для дамки
        private List kingAttack(List lastPos, int pos, int dir, int color, int enemy)
        {
            List tempMoves, moves = new List();

            if (dir != RIGHT_BELOW)
            {
                tempMoves = kingDiagAttack(lastPos, pos, color, enemy, 1, 1);

                if (notNull(tempMoves))
                    moves.append(tempMoves);
            }

            if (dir != LEFT_ABOVE)
            {
                tempMoves = kingDiagAttack(lastPos, pos, color, enemy, -1, -1);

                if (notNull(tempMoves))
                    moves.append(tempMoves);
            }


            if (dir != RIGHT_ABOVE)
            {
                tempMoves = kingDiagAttack(lastPos, pos, color, enemy, 1, -1);

                if (notNull(tempMoves))
                    moves.append(tempMoves);
            }

            if (dir != LEFT_BELOW)
            {
                tempMoves = kingDiagAttack(lastPos, pos, color, enemy, -1, 1);

                if (notNull(tempMoves))
                    moves.append(tempMoves);
            }


            return moves;
        }
        private List kingDiagAttack(List lastPos, int pos, int color, int enemy, int incX, int incY)
        {
            int x = (pos % 4) * 2 + ((pos / 4) % 2 == 0 ? 1 : 0);
            int y = pos / 4;
            int i, j;
            List moves = new List();
            List tempMoves, tempPos;


            int startPos = (int)lastPos.peek_head();

            i = x + incX;
            j = y + incY;

            // Находит врага
            while (i > 0 && i < 7 && j > 0 && j < 7 &&
                   (cells[colLineToPos(i, j)] == EMPTY || colLineToPos(i, j) == startPos))
            {
                i += incX;
                j += incY;
            }

            if (i > 0 && i < 7 && j > 0 && j < 7 && (cells[colLineToPos(i, j)] & ~KING) == enemy &&
                !lastPos.has(colLineToPos(i, j)))
            {

                lastPos.push_back(colLineToPos(i, j));

                i += incX;
                j += incY;

                int saveI = i;
                int saveJ = j;
                while (i >= 0 && i <= 7 && j >= 0 && j <= 7 &&
                     (cells[colLineToPos(i, j)] == EMPTY || colLineToPos(i, j) == startPos))
                {

                    int dir;

                    if (incX == 1 && incY == 1)
                        dir = LEFT_ABOVE;
                    else if (incX == -1 && incY == -1)
                        dir = RIGHT_BELOW;
                    else if (incX == -1 && incY == 1)
                        dir = RIGHT_ABOVE;
                    else
                        dir = LEFT_BELOW;


                    tempPos = (List)lastPos.clone();
                    tempMoves = kingAttack(tempPos, colLineToPos(i, j), dir, color, enemy);

                    if (notNull(tempMoves))
                        moves.append(addMove(new Move(pos, colLineToPos(i, j)), tempMoves));

                    i += incX;
                    j += incY;
                }

                lastPos.pop_back();

                if (moves.isEmpty())
                {
                    i = saveI;
                    j = saveJ;

                    while (i >= 0 && i <= 7 && j >= 0 && j <= 7 &&
                           (cells[colLineToPos(i, j)] == EMPTY || colLineToPos(i, j) == startPos))
                    {

                        tempMoves = new List();
                        tempMoves.push_back(new Move(pos, colLineToPos(i, j)));
                        moves.push_back(tempMoves);

                        i += incX;
                        j += incY;
                    }
                }
            }
            return moves;
        }

        private List generateMoves(int color, int enemy)
        {
            List moves = new List();
            List tempMove;


            for (int k = 0; k < 32; k++)
                if ((cells[k] & ~KING) == currentPlayer)
                {
                    int x = (k % 4) * 2 + ((k / 4) % 2 == 0 ? 1 : 0);
                    int y = k / 4;
                    int i, j;

                    if ((cells[k] & KING) == 0)
                    {
                        i = (color == WHITE) ? -1 : 1;

                        // Просмотр диоганали /^ и \v
                        if (x < 7 && y + i >= 0 && y + i <= 7 &&
                            cells[colLineToPos(x + 1, y + i)] == EMPTY)
                        {
                            tempMove = new List();
                            tempMove.push_back(new Move(k, colLineToPos(x + 1, y + i)));
                            moves.push_back(tempMove);
                        }


                        // Просмотр диоганали ^\ и v/
                        if (x > 0 && y + i >= 0 && y + i <= 7 &&
                            cells[colLineToPos(x - 1, y + i)] == EMPTY)
                        {
                            tempMove = new List();
                            tempMove.push_back(new Move(k, colLineToPos(x - 1, y + i)));
                            moves.push_back(tempMove);
                        };
                    }
                    else
                    {
                      // Просмотр диоганали \v
                        i = x + 1;
                        j = y + 1;

                        while (i <= 7 && j <= 7 && cells[colLineToPos(i, j)] == EMPTY)
                        {
                            tempMove = new List();
                            tempMove.push_back(new Move(k, colLineToPos(i, j)));
                            moves.push_back(tempMove);

                            i++;
                            j++;
                        }


                        // Просмотр диоганали ^\
                        i = x - 1;
                        j = y - 1;
                        while (i >= 0 && j >= 0 && cells[colLineToPos(i, j)] == EMPTY)
                        {
                            tempMove = new List();
                            tempMove.push_back(new Move(k, colLineToPos(i, j)));
                            moves.push_back(tempMove);

                            i--;
                            j--;
                        }

                        // Просмотр диоганали /^
                        i = x + 1;
                        j = y - 1;
                        while (i <= 7 && j >= 0 && cells[colLineToPos(i, j)] == EMPTY)
                        {
                            tempMove = new List();
                            tempMove.push_back(new Move(k, colLineToPos(i, j)));
                            moves.push_back(tempMove);

                            i++;
                            j--;
                        }

                        // Просмотр диоганали v/
                        i = x - 1;
                        j = y + 1;
                        while (i >= 0 && j <= 7 && cells[colLineToPos(i, j)] == EMPTY)
                        {
                            tempMove = new List();
                            tempMove.push_back(new Move(k, colLineToPos(i, j)));
                            moves.push_back(tempMove);

                            i--;
                            j++;
                        }
                    }
                }

            return moves;
        }

        // Добавление движения фишки
        private List addMove(Move move, List moves)
        {
            if (move == null)
                return moves;

            List current, temp = new List();
            while (!moves.isEmpty())
            {
                current = (List)moves.pop_front();
                current.push_front(move);
                temp.push_back(current);
            }

            return temp;
        }

        // проверка должен бить
        public bool mustAttack()
        {
            for (int i = 0; i < 32; i++)
                if ((cells[i] & ~KING) == currentPlayer && mayAttack(i))
                    return true;

            return false;
        }
        public bool mayAttack(int pos)
        {
            if (cells[pos] == EMPTY)
                return false;

            int color;
            int enemy;

            color = cells[pos] & ~KING;
            if (color == WHITE)
                enemy = BLACK;
            else
                enemy = WHITE;

            int x = (pos % 4) * 2 + ((pos / 4) % 2 == 0 ? 1 : 0);
            int y = pos / 4;

            if ((cells[pos] & KING) == 0)
            { // It's a simple piece
                int i;

                i = (color == WHITE) ? -1 : 1;

                // See the diagonals /^ e \v
                if (x < 6 && y + i > 0 && y + i < 7 && (cells[colLineToPos(x + 1, y + i)] & ~KING) == enemy &&
                    cells[colLineToPos(x + 2, y + 2 * i)] == EMPTY)
                    return true;

                // See the diagonals ^\ e v/
                if (x > 1 && y + i > 0 && y + i < 7 && (cells[colLineToPos(x - 1, y + i)] & ~KING) == enemy &&
                    cells[colLineToPos(x - 2, y + 2 * i)] == EMPTY)
                    return true;

            }
            else                                                        // если это дамка
            {
                int i, j;


                // Просмотр диогонали \v
                i = x + 1;
                j = y + 1;
                while (i < 6 && j < 6 && cells[colLineToPos(i, j)] == EMPTY)
                {
                    i++;
                    j++;
                }

                if (i < 7 && j < 7 && (cells[colLineToPos(i, j)] & ~KING) == enemy)
                {
                    i++;
                    j++;

                    if (i <= 7 && j <= 7 && cells[colLineToPos(i, j)] == EMPTY)
                        return true;
                }

                // Просмотр диогонали ^\
                i = x - 1;
                j = y - 1;
                while (i > 1 && j > 1 && cells[colLineToPos(i, j)] == EMPTY)
                {
                    i--;
                    j--;
                }

                if (i > 0 && j > 0 && (cells[colLineToPos(i, j)] & ~KING) == enemy)
                {
                    i--;
                    j--;

                    if (i >= 0 && j >= 0 && cells[colLineToPos(i, j)] == EMPTY)
                        return true;
                }

                // Просмотр диогонали /^
                i = x + 1;
                j = y - 1;
                while (i < 6 && j > 1 && cells[colLineToPos(i, j)] == EMPTY)
                {
                    i++;
                    j--;
                }

                if (i < 7 && j > 0 && (cells[colLineToPos(i, j)] & ~KING) == enemy)
                {
                    i++;
                    j--;

                    if (i <= 7 && j >= 0 && cells[colLineToPos(i, j)] == EMPTY)
                        return true;
                }

                // Просмотр диогонали v/
                i = x - 1;
                j = y + 1;
                while (i > 1 && j < 6 && cells[colLineToPos(i, j)] == EMPTY)
                {
                    i--;
                    j++;
                }

                if (i > 0 && j < 7 && (cells[colLineToPos(i, j)] & ~KING) == enemy)
                {
                    i--;
                    j++;

                    if (i >= 0 && j <= 7 && cells[colLineToPos(i, j)] == EMPTY)
                        return true;
                }
            }
            return false;
        }
        // Преобразование положения x и y в позицию шашки
        private int colLineToPos(int col, int line)
        {
            if (line % 2 == 0)
                return line * 4 + (col - 1) / 2;
            else
                return line * 4 + col / 2;
        }

        // Передвижение фишки
        public void move(int from, int to)
        {
            bool haveToAttack = mustAttack();

            applyMove(from, to);

            if (!haveToAttack)
                changeSide();
            else
              if (!mayAttack(to))
                changeSide();
        }

        // Изменение игрового поля
        private void applyMove(int from, int to)
        {
            clearPiece(from, to);

            // Передвижение фишки
            if (to < 4 && cells[from] == WHITE)
                cells[to] = WHITE_KING;
            else if (to > 27 && cells[from] == BLACK)
                cells[to] = BLACK_KING;
            else
                cells[to] = cells[from];

            cells[from] = EMPTY;
        }

        // Удаление фишки с доски
        private void clearPiece(int from, int to)
        {
            int fromLine = from / 4;
            int fromCol = (from % 4) * 2 + ((from / 4) % 2 == 0 ? 1 : 0);
            int toLine = to / 4;
            int toCol = (to % 4) * 2 + ((to / 4) % 2 == 0 ? 1 : 0);

            int i, j;

            if (fromCol > toCol)
                i = -1;
            else
                i = 1;


            if (fromLine > toLine)
                j = -1;
            else
                j = 1;



            fromCol += i;
            fromLine += j;

            while (fromLine != toLine && fromCol != toCol)
            {
                int pos = colLineToPos(fromCol, fromLine);
                int piece = cells[pos];

                if ((piece & ~KING) == WHITE)
                    whiteCell--;
                else if ((piece & ~KING) == BLACK)
                    blackCell--;

                cells[pos] = EMPTY;
                fromCol += i;
                fromLine += j;
            }
        }

        // Изменение хода игрока
        private void changeSide()
        {
            if (currentPlayer == WHITE)
                currentPlayer = BLACK;
            else
                currentPlayer = WHITE;
        }


        public void move(List moves)
        {
            Move move;
            Enumeration iter = moves.elements();

            while (iter.hasMoreElements())
            {
                move = (Move)iter.nextElement();
                applyMove(move.getFrom(), move.getTo());
            }

            changeSide();
        }

        // Поиск всех атак
        private List simpleAttack(int pos, int color, int enemy)
        {
            int x = (pos % 4) * 2 + ((pos / 4) % 2 == 0 ? 1 : 0);
            int y = pos / 4;
            int i;
            List moves = new List();
            List tempMoves;
            int enemyPos, nextPos;



            i = (color == WHITE) ? -1 : 1;


            // Просмотр диоганоли /^ и \v
            if (x < 6 && y + i > 0 && y + i < 7)
            {
                enemyPos = colLineToPos(x + 1, y + i);
                nextPos = colLineToPos(x + 2, y + 2 * i);

                if ((cells[enemyPos] & ~KING) == enemy && cells[nextPos] == EMPTY)
                {
                    tempMoves = simpleAttack(nextPos, color, enemy);
                    moves.append(addMove(new Move(pos, nextPos), tempMoves));
                }
            }


            // Просмотр диоганали v/ и ^\
            if (x > 1 && y + i > 0 && y + i < 7)
            {
                enemyPos = colLineToPos(x - 1, y + i);
                nextPos = colLineToPos(x - 2, y + 2 * i);

                if ((cells[enemyPos] & ~KING) == enemy && cells[nextPos] == EMPTY)
                {
                    tempMoves = simpleAttack(nextPos, color, enemy);
                    moves.append(addMove(new Move(pos, nextPos), tempMoves));
                }
            }

            if (moves.isEmpty())
                moves.push_back(new List());

            return moves;
        }

        // Проверка списка
        private bool notNull(List moves)
        {
            return !moves.isEmpty() && !((List)moves.peek_head()).isEmpty();
        }
    }
}
