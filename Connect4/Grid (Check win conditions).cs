using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Connect4
{
    partial class Grid
    {
        /// <summary>
        /// Returns a value indicating which player, if any, won the game.
        /// </summary>
        /// <returns>-1 if the game is not over, otherwise 0 if player 1 won or 1 if player 2
        /// won.</returns>
        public int IsGameOver()
        {
            return IsGameOver(0)
                ? 0
                : (IsGameOver(1) ? 1 : -1);
        }

        /// <summary>
        /// Similar to IsGameOver() but only checks one player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>True if the last move was a winning move for the player.</returns>
        public bool IsGameOver(int player)
        {
            ulong playerPosition = playerPositions[player];

            ulong nDiagTest = playerPosition & (playerPosition >> (height + 2));
            ulong pDiagTest = playerPosition & (playerPosition >> height);
            ulong vertTest = playerPosition & (playerPosition >> 1);
            ulong horiTest = playerPosition & (playerPosition >> (height + 1));

            return ((nDiagTest & (nDiagTest >> 2 * (height + 2))) != 0)
                || ((pDiagTest & (pDiagTest >> 2 * height)) != 0)
                || ((vertTest & (vertTest >> 2)) != 0)
                || ((horiTest & (horiTest >> 2 * (height + 1))) != 0);
        }

        public bool IsValidGameOverAfterMove(int player, int move)
        {
            if (nextFreeTile[move] >= height)
            {
                return false;
            }

            int row = nextFreeTile[move];
            ulong playerPosition = playerPositions[player]
                | (ulong)1 << (move + row * (width + 1));

            ulong nDiagTest = playerPosition & (playerPosition >> (height + 2));
            ulong pDiagTest = playerPosition & (playerPosition >> height);
            ulong vertTest = playerPosition & (playerPosition >> 1);
            ulong horiTest = playerPosition & (playerPosition >> (height + 1));

            return ((nDiagTest & (nDiagTest >> 2 * (height + 2))) != 0)
                || ((pDiagTest & (pDiagTest >> 2 * height)) != 0)
                || ((vertTest & (vertTest >> 2)) != 0)
                || ((horiTest & (horiTest >> 2 * (height + 1))) != 0);
        }
    }
}
