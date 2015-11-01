using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    partial class Grid
    {
        public int GuessScore(int player)
        {
            ulong p0Threats = GetThreats(0);
            ulong p1Threats = GetThreats(1);

            if ((p1Threats & (p1Threats << (width + 1))) != 0)
            {
                return 0;
            }

            ulong rowMask = (1UL << width) - 1;

            ulong takenColumns = 0UL;

            bool p0Odd = false;
            //bool p0Even = false;
            //bool p1Odd = false;
            bool p1Even = false;

            for (int row = 0; p0Threats != 0 || p1Threats != 0; row++)
            {
                // Take all threats in this row which have no threats below.
                ulong maskedPlayerThreats = (p0Threats & rowMask) & ~takenColumns;
                ulong maskedOpponentThreats = (p1Threats & rowMask) & ~takenColumns;

                bool oddRow = (row & 1) == 0;
                p0Odd = p0Odd || (maskedPlayerThreats != 0 && oddRow);
                p1Even = p1Even || (maskedOpponentThreats != 0 && !oddRow);

                if (p0Odd && p1Even)
                {
                    return 1;
                }

                // Update the taken columns;
                takenColumns |= maskedPlayerThreats | maskedOpponentThreats;

                p0Threats >>= width + 1;
                p1Threats >>= width + 1;
            }

            return 0;
        }

        private ulong GetThreats(int player)
        {
            ulong playerPosition = playerPositions[player];

            // Check for horizontal threats.
            ulong pairs = playerPosition & (playerPosition >> 1);
            ulong triples = pairs & (pairs >> 1);
            ulong threats = (((pairs >> 2) & playerPosition) << 1) // rights.
                | (((pairs << 3) & playerPosition) >> 1) // lefts.
                | (triples >> 1) | (triples << 3); // triples.

            // Check for vertical threats.
            pairs = playerPosition & (playerPosition << (width + 1));
            triples = pairs & (pairs << (width + 1));
            threats |= triples << (width + 1);

            // Check for positive diagonal threats (/).
            pairs = playerPosition & (playerPosition >> width);
            triples = pairs & (pairs >> width);
            threats |= (((pairs >> (2 * width)) & playerPosition) << width) // rights.
                | (((pairs << (3 * width)) & playerPosition) >> width) // lefts.
                | (triples >> width) | (triples << (3 * width)); // triples.

            // Check for negative diagonal threats (\).
            int shift = width + 2;
            pairs = playerPosition & (playerPosition >> shift);
            triples = pairs & (pairs >> shift);
            threats |= (((pairs >> (2 * shift)) & playerPosition) << shift) // rights.
                | (((pairs << (3 * shift)) & playerPosition) >> shift) // lefts.
                | (triples >> shift) | (triples << (3 * shift)); // triples.

            // Get rid of any threats blocked by the opponent or the edge of the board.
            const ulong borderMask = 0xFF808080808080;
            return threats & ~(playerPositions[1 - player] | borderMask);
        }
    }
}
