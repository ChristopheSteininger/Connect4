using Connect4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
                Grid grid = new Grid(width, height);
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
            Grid grid = new Grid(width, height);
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
            Grid grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 4, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverVerticalTest()
        {
            Grid grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(width - 1, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverPositiveDiagonalTest()
        {
            Grid grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            Assert.AreEqual(true, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverNegativeDiagonalTest()
        {
            Grid grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));

            grid = new Grid(width, height);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(true, grid.LazyIsGameOver(1));
            grid.Move(2, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(4, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(0, 1);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
            grid.Move(1, 0);
            Assert.AreEqual(1, grid.IsGameOver());
            Assert.AreEqual(false, grid.LazyIsGameOver(0));
            Assert.AreEqual(false, grid.LazyIsGameOver(1));
        }

        /// <summary>
        ///A test for GetPlayerStreaks
        ///</summary>
        [TestMethod()]
        public void GetPlayerStreaksTest()
        {
            Grid grid = new Grid(width, height);
            grid.Move(0, 0);
            Assert.AreEqual(0, grid.GetPlayerStreaks(0));
            grid.Move(1, 0);
            Assert.AreEqual(0, grid.GetPlayerStreaks(0));
            grid.Move(2, 0);
            Assert.AreEqual(1, grid.GetPlayerStreaks(0));
        }

        /// <summary>
        ///A test for UndoMove
        ///</summary>
        [TestMethod()]
        public void UndoMoveTest()
        {
            for (int player = 0; player < 2; player++)
            {
                Grid referenceGrid = new Grid(width, height);
                Grid testGrid = new Grid(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int move = 0; move < width; move++)
                    {
                        referenceGrid.Move(move, player);
                        testGrid.Move(move, player);
                        testGrid.UndoMove(move, player);
                        testGrid.Move(move, player);

                        Assert.IsTrue(testGrid.Equals(referenceGrid));
                        Assert.AreEqual(testGrid.GetTTableHash(), referenceGrid.GetTTableHash());
                    }
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
    }
}
