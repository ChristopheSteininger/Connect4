using Connect4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Connect4Test
{
    /// <summary>
    ///This is a test class for GridTest and is intended
    ///to contain all GridTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GridTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private const int width = 6;
        private const int height = 6;
        private const int seed = 103;

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for Move. Fills the entry grid with each player's tiles in turn and checks
        ///that the grid looks as expected after each move.
        ///</summary>
        [TestMethod()]
        public void MoveTestPerPlayer()
        {
            for (int player = 0; player < 2; player++)
            {
                Grid grid = new Grid(width, height, seed);
                TileState expectedState = (TileState)player;
                for (int row = 0; row < height; row++)
                {
                    for (int column = 0; column < width; column++)
                    {
                        grid.Move(column, player);

                        // Test that only the expected tiles are filled.
                        for (int testrow = 0; testrow < height; testrow++)
                        {
                            for (int testcolumn = 0; testcolumn < width; testcolumn++)
                            {
                                if (testrow < row || (testrow == row && testcolumn <= column))
                                {
                                    Assert.AreEqual(expectedState, grid[testrow, testcolumn]);
                                }
                                else
                                {
                                    Assert.AreEqual(TileState.Empty, grid[testrow, testcolumn]);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod()]
        public void MoveTestAlternatingPlayer()
        {
            Grid grid = new Grid(width, height, seed);
            int currentPlayer = 0;

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    grid.Move(column, currentPlayer);
                    currentPlayer = 1 - currentPlayer;

                    // Test that only the expected tiles are filled with the correct player.
                    for (int testrow = 0; testrow < height; testrow++)
                    {
                        for (int testcolumn = 0; testcolumn < width; testcolumn++)
                        {
                            if (testrow < row || (testrow == row && testcolumn <= column))
                            {
                                // Note: if size is an odd number then the expression below
                                // must be (testcolumn + testrow) % 2.
                                TileState expectedState = (TileState)(testcolumn % 2);
                                Assert.AreEqual(expectedState, grid[testrow, testcolumn]);
                            }
                            else
                            {
                                Assert.AreEqual(TileState.Empty, grid[testrow, testcolumn]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverHorizontalTest()
        {
            Grid grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 4, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverVerticalTest()
        {
            Grid grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverPositiveDiagonalTest()
        {
            Grid grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverNegativeDiagonalTest()
        {
            Grid grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));

            grid = new Grid(width, height, seed);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(false, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.IsGameOver(0));
            Assert.AreEqual(true, grid.IsGameOver(1));
        }

        /// <summary>
        ///A test for UndoMove
        ///</summary>
        [TestMethod()]
        public void UndoMoveTest()
        {
            for (int player = 0; player < 2; player++)
            {
                Grid referenceGrid = new Grid(width, height, seed);
                Grid testGrid = new Grid(width, height, seed);

                for (int y = 0; y < height; y++)
                {
                    for (int move = 0; move < width; move++)
                    {
                        referenceGrid.Move(move, player);
                        testGrid.Move(move, player);
                        testGrid.UndoMove(move, player);
                        testGrid.Move(move, player);

                        Assert.IsTrue(testGrid.Equals(referenceGrid));
                        Assert.AreEqual(testGrid.Hash, referenceGrid.Hash);
                    }
                }
            }
        }

        [TestMethod()]
        public void Perft()
        {
            Grid grid = new Grid(7, 6, 0);

            const int maxDepth = 10;

            int[] results = new int[maxDepth];
            int[] expected = new int[] { 1, 7, 49, 238, 1120, 4263, 16422,
            54859, 184275, 558186, 1662623, 4568683 };

            HashSet<ulong> visitedStates = new HashSet<ulong>();

            PerftHelper(grid, 0, maxDepth, results, visitedStates);

            for (int i = 0; i < maxDepth; i++)
            {
                Assert.AreEqual(expected[i], results[i]);
            }
        }

        private void PerftHelper(Grid grid, int depth, int maxDepth, int[] results,
            HashSet<ulong> visitedStates)
        {
            int player = depth & 1;

            if (depth >= maxDepth || grid.IsGameOver(player)
                || visitedStates.Contains(grid.Hash))
            {
                return;
            }

            results[depth]++;
            visitedStates.Add(grid.Hash);

            for (int i = 0; i < grid.Width; i++)
            {
                if (grid.IsValidMove(i))
                {
                    grid.Move(i, player);
                    PerftHelper(grid, depth + 1, maxDepth, results, visitedStates);
                    grid.UndoMove(i, player);
                }
            }
        }

        private void AssertArraysEqual(int[] expected, int[] actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        /// <summary>
        ///A test for GetThreats
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void GetThreatsReturns0OnEmptyBoard()
        {
            Grid_Accessor grid = new Grid_Accessor(new PrivateObject(new Grid(7, 6, 0)));

            ValidateThreats(grid, 0);
            ValidateThreats(grid, 1);
        }

        /// <summary>
        ///A test for GetThreats
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void GetThreatsFindsSimpleHorizontalThreat()
        {
            Grid_Accessor grid = new Grid_Accessor(new PrivateObject(new Grid(7, 6, 0)));

            grid.Move(1, 0);
            grid.Move(1, 1);
            grid.Move(2, 0);
            grid.Move(2, 1);
            grid.Move(3, 0);
            grid.Move(3, 1);

            ValidateThreats(grid, 0);
            ValidateThreats(grid, 1);
        }

        /// <summary>
        ///A test for GetThreats
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void GetThreatsIgnoresBlockedHorizontalThreats()
        {
            Grid_Accessor grid = new Grid_Accessor(new PrivateObject(new Grid(7, 6, 0)));

            grid.Move(1, 0);
            grid.Move(1, 1);
            grid.Move(2, 0);
            grid.Move(2, 1);
            grid.Move(3, 0);
            grid.Move(3, 1);
            grid.Move(6, 0);
            grid.Move(0, 1);
            grid.Move(0, 0);

            ValidateThreats(grid, 0);
            ValidateThreats(grid, 1);
        }

        /// <summary>
        ///A test for GetThreats
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void GetThreatsIgnoresBlockedByEdge()
        {
            Grid_Accessor grid = new Grid_Accessor(new PrivateObject(new Grid(7, 6, 0)));

            grid.Move(6, 0);
            grid.Move(6, 1);
            grid.Move(5, 0);
            grid.Move(5, 1);
            grid.Move(4, 0);
            grid.Move(4, 1);

            ValidateThreats(grid, 0);
            ValidateThreats(grid, 1);
        }

        /// <summary>
        ///A test for GetThreats
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Connect4.exe")]
        public void GetThreatsFuzzTest()
        {
            Random random = new Random(0);

            const int gameLimit = 200;

            for (int game = 1; game < gameLimit; game++)
            {
                Grid_Accessor grid = new Grid_Accessor(new PrivateObject(new Grid(7, 6, 0)));

                // Play a full random, game, and verify threats after each move.
                for (int i = 0; i < 7 * 6 && grid.IsGameOver() == -1; i++)
                {
                    ValidateThreats(grid, 0);
                    ValidateThreats(grid, 1);

                    int move;
                    do
                    {
                        move = random.Next(grid.Width);
                    } while (!grid.IsValidMove(move));

                    int player = i & 1;
                    grid.Move(move, player);
                }
            }
        }

        private void ValidateThreats(Grid_Accessor grid, int player)
        {
            ulong threats = grid.GetThreats(player);

            for (int i = 0; i < (grid.Height + 1) * grid.Width; i++)
            {
                ulong mask = 1UL << i;
                bool isThreat = (threats & mask) == mask;
                bool isInBuffer = ((i + 1) % (grid.Height + 1)) == 0;

                if (isThreat)
                {
                    Assert.AreEqual(0UL, grid.playerPositions[0] & mask,
                        "Player cannot have a threat in a nonempty location");
                    Assert.AreEqual(0UL, grid.playerPositions[1] & mask,
                        "Player cannot have a threat in a nonempty location");

                    Assert.IsTrue(!isInBuffer,
                        "Cannot have a threat in the column buffer");
                }

                if (!isInBuffer)
                {
                    ulong originalPosition = grid.playerPositions[player];
                    grid.playerPositions[player] |= mask & ~grid.playerPositions[1 - player];
                    Assert.AreEqual(isThreat, grid.IsGameOver(player));
                    grid.playerPositions[player] = originalPosition;
                }
            }

            Assert.AreEqual(0UL, threats >> ((grid.Height + 1) * grid.Width),
                "Threats must be on the board");
        }
    }
}
