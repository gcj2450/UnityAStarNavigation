using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrightPipe
{
    //已和Js版本完全一致
    /// <summary>
    /// Grid used to store an arrangement of pipes
    /// </summary>
    public class Grid
    {
        private int gridWidth;
        private int gridHeight;
        private float cellDimensions;
        private Pipe[,] pipes;

        public Grid(float cellDimensions, int gridWidth, int gridHeight)
        {
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.cellDimensions = cellDimensions;
            pipes = new Pipe[gridWidth, gridHeight];
        }

        /// <summary>
        /// Cell dimensions 
        /// </summary>
        /// <returns></returns>
        public float GetCellDimensions()
        {
            return cellDimensions;
        }

        /// <summary>
        /// Tests if a specified location is a valid grid location.
        /// </summary>
        /// <param name="location">Location to test if valid</param>
        /// <returns>Whether the specified grid location is valid.</returns>
        public bool ValidateLocation(Vector2Int location)
        {
            return location.x >= 0 && location.y >= 0 &&
                   location.x < gridWidth && location.y < gridHeight;
        }

        /// <summary>
        /// Gets a pipe at a specified grid location.
        /// </summary>
        /// <param name="location">Location to retrieve pipe from</param>
        /// <returns>Pipe at the specified location or null if location is empty.</returns>
        public Pipe GetPipe(Vector2Int location)
        {
            if (!ValidateLocation(location))
                return null;

            return pipes[location.x, location.y];
        }

        /// <summary>
        /// Sets the pipe at a specified location.
        /// </summary>
        /// <param name="location">Location to place a pipe.</param>
        /// <param name="pipe">The pipe to place at the specified location.</param>
        public void SetPipe(Vector2Int location, Pipe pipe)
        {
            if (!ValidateLocation(location))
                return;

            pipe?.Detach();
            ClearPipe(location);

            if (pipe != null)
            {
                pipes[location.x, location.y] = pipe;
                pipe.Attach(this, location);
            }
        }

        /// <summary>
        /// Clears a pipe at a specified grid location.
        /// </summary>
        /// <param name="location">Location at which to clear the pipe/contents of.</param>
        public void ClearPipe(Vector2Int location)
        {
            pipes[location.x, location.y]?.Detach();
            pipes[location.x, location.y] = null;
        }

        /// <summary>
        /// Removes all instances of the specified pipe from the grid.
        /// </summary>
        /// <param name="pipe"></param>
        public void RemovePipe(Pipe pipe)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (pipes[x, y] == pipe)
                        ClearPipe(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// 将世界位置转为格子位置
        /// </summary>
        /// <param name="location">Location to be translated in grid space.</param>
        /// <returns>Translated location.</returns>
        /// <exception cref="Exception"></exception>
        public Vector2Int ScreenToGrid(Vector2 location)
        {
            var translation = new Vector2Int(
                Mathf.FloorToInt(location.x / cellDimensions),
                Mathf.FloorToInt(location.y / cellDimensions));

            if (!ValidateLocation(translation))
                throw new Exception("Specified coordinates are not inside screen bounds.");

            return translation;
        }

        /// <summary>
        /// 将格子位置转为世界位置
        /// </summary>
        /// <param name="location">The location to translate to screen space.</param>
        /// <returns>The translated location.</returns>
        public Vector2 GridToScreen(Vector2Int location)
        {
            return new Vector2(location.x * cellDimensions, location.y * cellDimensions);
        }

        /// <summary>
        /// Draws the grid.
        /// </summary>
        /// <param name="x">The x draw offset.</param>
        /// <param name="y">The y draw offset.</param>
        public void Draw(float x, float y)
        {
            for (int xLoc = 0; xLoc < gridWidth; xLoc++)
            {
                for (int yLoc = 0; yLoc < gridHeight; yLoc++)
                {
                    DrawGridBg(x + xLoc * cellDimensions + cellDimensions / 2, y + yLoc * cellDimensions + cellDimensions / 2, xLoc, yLoc);
                    if (pipes[xLoc, yLoc] != null)
                    {
                        pipes[xLoc, yLoc].Draw(
                            x + xLoc * cellDimensions + cellDimensions / 2,
                            y + yLoc * cellDimensions + cellDimensions / 2);
                    }
                }
            }
        }

        public Dictionary<string,GameObject> GridBgs = new Dictionary<string, GameObject>();
        /// <summary>
        /// 绘制Grid背景
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="xId"></param>
        /// <param name="yId"></param>
        void DrawGridBg(float xPos,float yPos,int xId,int yId)
        {
            Sprite sprite = Resources.Load<Sprite>("gfx/overlay");
            GameObject go = new GameObject();
            go.transform.localScale = new Vector3(78, 78, 78);
            go.name = $"{xId}_{yId}";
            GridBgs[go.name] =go;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.transform.position = new Vector3(xPos, yPos, 1);
        }

        /// <summary>
        /// Gets the screen space bounds of grid.
        /// </summary>
        /// <returns>The screen space bounds of grid.</returns>
        public Rect GetBounds()
        {
            return new Rect(Vector2.zero, new Vector2(gridWidth * cellDimensions, gridHeight * cellDimensions));
        }

        /// <summary>
        /// Gets the bounds of the grid, in terms of cells.
        /// </summary>
        /// <returns>The bounds of the grid, in terms of cells.</returns>
        public Rect GetCellBounds()
        {
            return new Rect(Vector2.zero, new Vector2(gridWidth, gridHeight));
        }

        /// <summary>
        /// Gets a list of all filled pipes in the grid.
        /// </summary>
        /// <returns>all filled pipes in the grid</returns>
        public List<Pipe> GetFilledPipes()
        {
            var filledPipes = new List<Pipe>();
            var allPipes = GetPipes();

            foreach (var pipe in allPipes)
            {
                if (pipe != null && pipe.IsFilled())
                    filledPipes.Add(pipe);
            }
            //for (var i = 0; i < allPipes.Count; i++)
            //{
            //    if (allPipes[i] != null && allPipes[i].IsFilled())
            //        filledPipes.Add(allPipes[i]);
            //}

            return filledPipes;
        }

        /// <summary>
        /// Array of pipes that were filled in this pump cycle.
        /// </summary>
        /// <returns></returns>
        public List<Pipe> Pump()
        {
            var pipesFilledBeforePump = GetFilledPipes();

            for (int xLoc = 0; xLoc < gridWidth; xLoc++)
            {
                for (int yLoc = 0; yLoc < gridHeight; yLoc++)
                {
                    var pipe = pipes[xLoc, yLoc];
                    if (pipe != null && pipe.IsPump)
                        pipe.Fill(null);
                }
            }

            var pipesFilledAfterPump = GetFilledPipes();
            var newPipesFilled = new List<Pipe>();

            foreach (var pipe in pipesFilledAfterPump)
            {
                if (!pipesFilledBeforePump.Contains(pipe))
                    newPipesFilled.Add(pipe);
            }

            return newPipesFilled;
        }

        public List<Pipe> GetPipes()
        {
            var allPipes = new List<Pipe>();

            for (int xLoc = 0; xLoc < gridWidth; xLoc++)
            {
                for (int yLoc = 0; yLoc < gridHeight; yLoc++)
                {
                    if (pipes[xLoc, yLoc] != null)
                        allPipes.Add(pipes[xLoc, yLoc]);
                }
            }

            return allPipes;
        }
    }
}