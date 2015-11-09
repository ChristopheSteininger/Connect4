using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    partial class Grid
    {
        public ulong GetThreats(int player)
        {
            ulong playerPosition = playerPositions[player];

            // Check for horizontal threats.
            int shift = height + 1;
            ulong pairs = playerPosition & (playerPosition >> shift);
            ulong triples = pairs & (pairs >> shift);
            ulong threats = (((pairs >> (2 * shift)) & playerPosition) << shift) // rights.
                | (((pairs << (3 * shift)) & playerPosition) >> shift) // lefts.
                | (triples >> shift) | (triples << (3 * shift)); // triples.

            // Check for vertical threats.
            pairs = playerPosition & (playerPosition << 1);
            triples = pairs & (pairs << 1);
            threats |= triples << 1;

            // Check for positive diagonal threats (/).
            pairs = playerPosition & (playerPosition >> height);
            triples = pairs & (pairs >> height);
            threats |= (((pairs >> (2 * height)) & playerPosition) << height) // rights.
                | (((pairs << (3 * height)) & playerPosition) >> height) // lefts.
                | (triples >> height) | (triples << (3 * height)); // triples.

            // Check for negative diagonal threats (\).
            shift = height + 2;
            pairs = playerPosition & (playerPosition >> shift);
            triples = pairs & (pairs >> shift);
            threats |= (((pairs >> (2 * shift)) & playerPosition) << shift) // rights.
                | (((pairs << (3 * shift)) & playerPosition) >> shift) // lefts.
                | (triples >> shift) | (triples << (3 * shift)); // triples.

            // Get rid of any threats blocked by the opponent or the edge of the board.
            const ulong borderMask = 0xFDFBF7EFDFBF; // bottomRow * 2^6.
            return threats & borderMask & ~playerPositions[1 - player];
        }
    }
}
