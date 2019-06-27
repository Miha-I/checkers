using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersGame
{
    class Computer
    {
        private CheckersBoard board;
        private int color;
        private int maxDepth = 1;
        private int[] tableWeight = { 4, 4, 4, 4,
                                 4, 3, 3, 3,
                                 3, 2, 2, 4,
                                 4, 2, 1, 3,
                                 3, 1, 2, 4,
                                 4, 2, 2, 3,
                                 3, 3, 3, 4,
                                 4, 4, 4, 4};

        public Computer(CheckersBoard gameBoard)
        {
            board = gameBoard;
            color = CheckersBoard.BLACK;
        }

        public int depth
        {
            get
            {
                return maxDepth;
            }
            set
            {
                maxDepth = value;
            }
        }

        public void play()
        {
            try
            {
                List moves = minimax(board);

                if (!moves.isEmpty())
                    board.move(moves);
            }
            catch
            {
            }
        }
        public void setBoard(CheckersBoard board)
        {
            this.board = board;
        }

        private bool mayPlay(List moves)
        {
            return !moves.isEmpty() && !((List)moves.peek_head()).isEmpty();
        }

        private List minimax(CheckersBoard board)
        {
            List sucessors;
            List move, bestMove = null;
            CheckersBoard nextBoard;
            int value, maxValue = Int32.MinValue;

            sucessors = board.legalMoves();
            while (mayPlay(sucessors))
            {
                move = (List)sucessors.pop_front();
                nextBoard = (CheckersBoard)board.clone();
                nextBoard.move(move);
                value = minMove(nextBoard, 1, maxValue, Int32.MaxValue);

                if (value > maxValue)
                {
                    maxValue = value;
                    bestMove = move;
                }
            }
            return bestMove;
        }

        private int maxMove(CheckersBoard board, int depth, int alpha, int beta)
        {
            if (cutOffTest(board, depth))
                return eval(board);


            List sucessors;
            List move;
            CheckersBoard nextBoard;
            int value;
            sucessors = board.legalMoves();
            while (mayPlay(sucessors))
            {
                move = (List)sucessors.pop_front();
                nextBoard = (CheckersBoard)board.clone();
                nextBoard.move(move);
                value = minMove(nextBoard, depth + 1, alpha, beta);

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha > beta)
                {
                    return beta;
                }
            }
            return alpha;
        }

        private int minMove(CheckersBoard board, int depth, int alpha, int beta)
        {
            if (cutOffTest(board, depth))
                return eval(board);

            List sucessors;
            List move;
            CheckersBoard nextBoard;
            int value;
            sucessors = (List)board.legalMoves();
            while (mayPlay(sucessors))
            {
                move = (List)sucessors.pop_front();
                nextBoard = (CheckersBoard)board.clone();
                nextBoard.move(move);
                value = maxMove(nextBoard, depth + 1, alpha, beta);

                if (value < beta)
                {
                    beta = value;
                }

                if (beta < alpha)
                {
                    return alpha;
                }
            }
            return beta;
        }

        private int eval(CheckersBoard board)
        {
            int colorKing;
            int colorForce = 0;
            int enemyForce = 0;
            int piece;

            if (color == CheckersBoard.WHITE)
                colorKing = CheckersBoard.WHITE_KING;
            else
                colorKing = CheckersBoard.BLACK_KING;

            try
            {
                for (int i = 0; i < 32; i++)
                {
                    piece = board.getCell(i);

                    if (piece != CheckersBoard.EMPTY)
                        if (piece == color || piece == colorKing)
                            colorForce += calculateValue(piece, i);
                        else
                            enemyForce += calculateValue(piece, i);
                }
            }
            catch
            {
            }

            return colorForce - enemyForce;
        }

        private int calculateValue(int piece, int pos)
        {
            int value;

            if (piece == CheckersBoard.WHITE)
                if (pos >= 4 && pos <= 7)
                    value = 7;
                else
                    value = 5;
            else if (piece != CheckersBoard.BLACK)
                if (pos >= 24 && pos <= 27)
                    value = 7;
                else
                    value = 5;
            else
                value = 10;

            return value * tableWeight[pos];
        }
        private bool cutOffTest(CheckersBoard board, int depth)
        {
            return depth > maxDepth || board.whiteCell == 0 || board.blackCell == 0;
        }

    }
}
