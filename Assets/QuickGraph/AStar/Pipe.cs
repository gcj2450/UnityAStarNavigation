using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
namespace BrightPipe
{
    public class Pipe
    {
        public Vector2Int Location { get; private set; }
        public Grid ParentGrid { get; private set; }
        public bool Filled { get; private set; }
        public bool Leaked { get; private set; }
        public Pipe Type { get; private set; }
        public bool Replaceable { get; private set; }
        public bool CanLeak { get; private set; }

        public int Value { get; }
        public string GraphicPath { get; set; }
        public List<Direction> Directions { get; }
        public bool IsPump { get; set; }

        public Pipe Create()
        {
            var pipe = new Pipe(Value, GraphicPath, Directions, this, Replaceable);
            if (IsPump)
            {
                pipe.SetAsPump();
            }
            return pipe;
        }

        private Sprite LoadGraphic(string path)
        {
            // Load graphic from resources or asset bundle
            return Resources.Load<Sprite>(path);
        }

        public Pipe(int _value, string graphicPath, List<Direction> directions, Pipe type, bool replaceable, bool canLeak = true)
        {
            Value = _value;
            Location = Vector2Int.zero;
            ParentGrid = null;
            GraphicPath = graphicPath;
            Directions = directions;
            Filled = false;
            Leaked = false;
            IsPump = false;
            Type = type;
            Replaceable = replaceable;
            CanLeak = canLeak;
        }

        public void Attach(Grid grid, Vector2Int location)
        {
            ParentGrid = grid;
            Location = location;
        }

        public void Detach()
        {
            if (ParentGrid == null)
                return;

            var parent = ParentGrid;
            ParentGrid = null;
            parent.RemovePipe(this);
            Location = Vector2Int.zero;
        }

        public void Fill(Direction dir, List<Pipe> filledPipes = null)
        {
            if (filledPipes == null)
            {
                filledPipes = new List<Pipe>();
            }

            if (filledPipes.Contains(this))
            {
                return;
            }

            filledPipes.Add(this);

            if (Filled && Type != Pipes.Drain)
            {
                var pipeArray = GetConnections(dir);

                if (CanLeak && GetConnections(null).Count != Directions.Count)
                {
                    Leaked = true;
                }

                foreach (var pipe in pipeArray)
                {
                    var delta = Location - pipe.Location;
                    pipe.Fill(Direction.FromVector(delta), filledPipes);
                }
            }
            else
            {
                Filled = true;
            }
        }

        public void Draw(float x, float y)
        {
            //Debug.Log($"Draw:{GraphicPath}");
            GraphicPath = GraphicPath.Replace(".png", "");
            Sprite sprite = Resources.Load<Sprite>(GraphicPath);
            GameObject go = new GameObject();
            go.transform.localScale = new Vector3(78, 78, 78);
            SpriteRenderer renderer= go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.transform.position = new Vector3(x, y, 0);
        }

        public List<Pipe> GetConnections(Direction dir)
        {
            var pipeArray = new List<Pipe>();
            var pipeUp = ParentGrid.GetPipe(Location + Direction.Up.Delta);
            var pipeDown = ParentGrid.GetPipe(Location + Direction.Down.Delta);
            var pipeRight = ParentGrid.GetPipe(Location + Direction.Right.Delta);
            var pipeLeft = ParentGrid.GetPipe(Location + Direction.Left.Delta);

            if (dir == Direction.Up)
            {
                pipeUp = null;
            }
            else if (dir == Direction.Down)
            {
                pipeDown = null;
            }
            else if (dir == Direction.Right)
            {
                pipeRight = null;
            }
            else if (dir == Direction.Left)
            {
                pipeLeft = null;
            }

            foreach (var direction in Directions)
            {
                if (dir == Direction.Up)
                {
                    if (pipeUp != null && pipeUp.Directions.Contains(Direction.Down))
                    {
                        pipeArray.Add(pipeUp);
                    }
                }
                else if (dir == Direction.Down)
                {
                    if (pipeDown != null && pipeDown.Directions.Contains(Direction.Up))
                    {
                        pipeArray.Add(pipeDown);
                    }
                }
                else if (dir == Direction.Right)
                {
                    if (pipeRight != null && pipeRight.Directions.Contains(Direction.Left))
                    {
                        pipeArray.Add(pipeRight);
                    }
                }
                else if (dir == Direction.Left)
                {
                    if (pipeLeft != null && pipeLeft.Directions.Contains(Direction.Right))
                    {
                        pipeArray.Add(pipeLeft);
                    }
                }
            }

            return pipeArray;
        }

        public void Update(float deltaTime)
        {
            // Update logic if needed
        }

        public bool ConnectedToPump()
        {
            var pipeArray = GetConnections(null);
            foreach (var pipe in pipeArray)
            {
                if (pipe.IsPump)
                {
                    return true;
                }
            }
            return false;
        }

        public List<Direction> GetDirections()
        {
            return Directions;
        }

        public Vector2Int GetLocation()
        {
            return this.Location;
        }

        public void SetAsPump()
        {
            IsPump = true;
        }

        public bool IsFilled()
        {
            return Filled;
        }
        public bool IsLeaking()
        {
            return Leaked;
        }

        public bool CanReplace()
        {
            return !Filled && Replaceable;
        }
    }
}