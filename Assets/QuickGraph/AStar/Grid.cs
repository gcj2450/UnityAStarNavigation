using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;

namespace BrightPipe
{
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

        public float GetCellDimensions()
        {
            return cellDimensions;
        }

        public bool ValidateLocation(Vector2Int location)
        {
            return location.x >= 0 && location.y >= 0 &&
                   location.x < gridWidth && location.y < gridHeight;
        }

        public Pipe GetPipe(Vector2Int location)
        {
            if (!ValidateLocation(location))
                return null;

            return pipes[location.x, location.y];
        }

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

        public void ClearPipe(Vector2Int location)
        {
            pipes[location.x, location.y]?.Detach();
            pipes[location.x, location.y] = null;
        }

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

        public Vector2Int ScreenToGrid(Vector2 location)
        {
            var translation = new Vector2Int(
                Mathf.FloorToInt(location.x / cellDimensions),
                Mathf.FloorToInt(location.y / cellDimensions));

            if (!ValidateLocation(translation))
                throw new Exception("Specified coordinates are not inside screen bounds.");

            return translation;
        }

        public Vector2 GridToScreen(Vector2Int location)
        {
            return new Vector2(location.x * cellDimensions, location.y * cellDimensions);
        }

        public void Draw(float x, float y)
        {
            for (int xLoc = 0; xLoc < gridWidth; xLoc++)
            {
                for (int yLoc = 0; yLoc < gridHeight; yLoc++)
                {
                    DrawGridBg(x + xLoc * cellDimensions + cellDimensions / 2, y + yLoc * cellDimensions + cellDimensions / 2);
                    if (pipes[xLoc, yLoc] != null)
                    {
                        pipes[xLoc, yLoc].Draw(
                            x + xLoc * cellDimensions + cellDimensions / 2,
                            y + yLoc * cellDimensions + cellDimensions / 2);
                    }
                }
            }
        }

        void DrawGridBg(float x,float y)
        {
            Sprite sprite = Resources.Load<Sprite>("gfx/overlay");
            GameObject go = new GameObject();
            go.transform.localScale = new Vector3(78, 78, 78);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.transform.position = new Vector3(x, y, 1);
        }

        public Rect GetBounds()
        {
            return new Rect(Vector2.zero, new Vector2(gridWidth * cellDimensions, gridHeight * cellDimensions));
        }

        public Rect GetCellBounds()
        {
            return new Rect(Vector2.zero, new Vector2(gridWidth, gridHeight));
        }

        public List<Pipe> GetFilledPipes()
        {
            var filledPipes = new List<Pipe>();
            var allPipes = GetPipes();

            foreach (var pipe in allPipes)
            {
                if (pipe != null && pipe.IsFilled())
                    filledPipes.Add(pipe);
            }

            return filledPipes;
        }

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