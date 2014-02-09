﻿using Connect4;
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

        private const int size = 6;

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
                Grid grid = new Grid(size);
                TileState expectedState = (TileState)player;
                for (int row = 0; row < size; row++)
                {
                    for (int column = 0; column < size; column++)
                    {
                        grid = grid.Move(column, player);

                        // Test that only the expected tiles are filled.
                        for (int testrow = 0; testrow < size; testrow++)
                        {
                            for (int testcolumn = 0; testcolumn < size; testcolumn++)
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
            Grid grid = new Grid(size);
            int currentPlayer = 0;

            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    grid = grid.Move(column, currentPlayer);
                    currentPlayer = 1 - currentPlayer;

                    // Test that only the expected tiles are filled with the correct player.
                    for (int testrow = 0; testrow < size; testrow++)
                    {
                        for (int testcolumn = 0; testcolumn < size; testcolumn++)
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
            Grid grid = new Grid(size);
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());

            grid = new Grid(size);
            grid = grid.Move(size - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(size - 2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(size - 3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(size - 4, 1);
            Assert.AreEqual(1, grid.IsGameOver());

            grid = new Grid(size);
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(0, grid.IsGameOver());
            grid = grid.Move(4, 0);
            Assert.AreEqual(0, grid.IsGameOver());
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverVerticalTest()
        {
            Grid grid = new Grid(size);
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(0, grid.IsGameOver());

            grid = new Grid(size);
            grid = grid.Move(size - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(size - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(size - 1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(size - 1, 1);
            Assert.AreEqual(1, grid.IsGameOver());

            grid = new Grid(size);
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(1, grid.IsGameOver());
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverPositiveDiagonalTest()
        {
            Grid grid = new Grid(size);
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(0, grid.IsGameOver());

            grid = new Grid(size);
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(5, 1);
            Assert.AreEqual(1, grid.IsGameOver());
        }

        /// <summary>
        ///A test for IsGameOver
        ///</summary>
        [TestMethod()]
        public void IsGameOverNegativeDiagonalTest()
        {
            Grid grid = new Grid(size);
            grid = grid.Move(5, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(1, grid.IsGameOver());

            grid = new Grid(size);
            grid = grid.Move(5, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(4, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(3, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(2, 1);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(1, 0);
            Assert.AreEqual(-1, grid.IsGameOver());
            grid = grid.Move(0, 1);
            Assert.AreEqual(1, grid.IsGameOver());
        }

        /// <summary>
        ///A test for GetPlayerStreaks
        ///</summary>
        [TestMethod()]
        public void GetPlayerStreaksTest()
        {
            Grid grid = new Grid(size);
            grid = grid.Move(0, 0);
            AssertArraysEqual(new int[] { 0, 0, 0 }, grid.GetPlayerStreaks(0));
            grid = grid.Move(1, 0);
            AssertArraysEqual(new int[] { 1, 0, 0 }, grid.GetPlayerStreaks(0));
            grid = grid.Move(2, 0);
            AssertArraysEqual(new int[] { 0, 1, 0 }, grid.GetPlayerStreaks(0));
            grid = grid.Move(3, 0);
            AssertArraysEqual(new int[] { 0, 0, 1 }, grid.GetPlayerStreaks(0));
            grid = grid.Move(4, 0);
            AssertArraysEqual(new int[] { 0, 0, 1 }, grid.GetPlayerStreaks(0));
            grid = grid.Move(5, 0);
            AssertArraysEqual(new int[] { 0, 0, 1 }, grid.GetPlayerStreaks(0));
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
