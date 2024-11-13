using System.Collections.Generic;
using UnityEngine;
namespace BrightPipe
{
    public class Pipes
    {
        //public static readonly Pipe Vertical = new Pipe(0, "gfx/vertical.png", new List<Direction> { Direction.Up, Direction.Down }, true);
        public static readonly Pipe Vertical = new Pipe(0, "gfx/vertical.png", new List<Direction> { Direction.Up, Direction.Down }, Vertical, true);
        public static readonly Pipe Horizontal = new Pipe(1, "gfx/horizontal.png", new List<Direction> { Direction.Left, Direction.Right }, Horizontal, true);
        public static readonly Pipe RightDown = new Pipe(2, "gfx/rightDown.png", new List<Direction> { Direction.Right, Direction.Down }, RightDown, true);
        public static readonly Pipe LeftDown = new Pipe(3, "gfx/leftDown.png", new List<Direction> { Direction.Left, Direction.Down }, LeftDown, true);
        public static readonly Pipe RightUp = new Pipe(4, "gfx/rightUp.png", new List<Direction> { Direction.Right, Direction.Up }, RightUp, true);
        public static readonly Pipe LeftUp = new Pipe(5, "gfx/leftUp.png", new List<Direction> { Direction.Left, Direction.Up }, LeftUp, true);

        public static readonly Pipe FixedVertical = new Pipe(6, "gfx/vertical.png", new List<Direction> { Direction.Up, Direction.Down }, Vertical, false);
        public static readonly Pipe FixedHorizontal = new Pipe(7, "gfx/horizontal.png", new List<Direction> { Direction.Left, Direction.Right }, Horizontal, false);
        public static readonly Pipe FixedRightDown = new Pipe(8, "gfx/rightDown.png", new List<Direction> { Direction.Right, Direction.Down }, RightDown, false);
        public static readonly Pipe FixedLeftDown = new Pipe(9, "gfx/leftDown.png", new List<Direction> { Direction.Left, Direction.Down }, LeftDown, false);
        public static readonly Pipe FixedRightUp = new Pipe(10, "gfx/rightUp.png", new List<Direction> { Direction.Right, Direction.Up }, RightUp, false);
        public static readonly Pipe FixedLeftUp = new Pipe(11, "gfx/leftUp.png", new List<Direction> { Direction.Left, Direction.Up }, LeftUp, false);

        public static readonly Pipe Pump = new Pipe(12, "gfx/pump.png", new List<Direction> { Direction.Down }, Pump, false, true);
        public static readonly Pipe Drain = new Pipe(13, "gfx/drain.png", new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right }, Drain, false, false);
        public static readonly Pipe Obstacle = new Pipe(14, "gfx/demoblock.png", new List<Direction>(), Obstacle, false);
        public static readonly Pipe LeftRightDown = new Pipe(15, "gfx/leftRightDown.png", new List<Direction> { Direction.Down, Direction.Left, Direction.Right }, LeftRightDown, false);
        public static readonly Pipe LeftRightUp = new Pipe(16, "gfx/leftRightUp.png", new List<Direction> { Direction.Up, Direction.Left, Direction.Right }, LeftRightUp, false);
        public static readonly Pipe LeftUpDown = new Pipe(17, "gfx/leftUpDown.png", new List<Direction> { Direction.Down, Direction.Left, Direction.Up }, LeftUpDown, false);
        public static readonly Pipe RightUpDown = new Pipe(18, "gfx/rightUpDown.png", new List<Direction> { Direction.Down, Direction.Up, Direction.Right }, RightUpDown, false);
        public static readonly Pipe CrossPipe = new Pipe(19, "gfx/crossPipe.png", new List<Direction> { Direction.Down, Direction.Up, Direction.Right, Direction.Left }, CrossPipe, false);//这里不知道为啥是RightUpDown

        public static List<Pipe> Values(bool includeComplex)
        {
            var array = new List<Pipe>
        {
            Vertical,
            Horizontal,
            RightDown,
            LeftDown,
            RightUp,
            LeftUp
        };

            if (includeComplex)
            {
                array.AddRange(new[] { LeftRightDown, LeftRightUp, LeftUpDown, RightUpDown });
            }

            return array;
        }

        public static List<Pipe> ComplexValues()
        {
            return new List<Pipe> { LeftRightDown, LeftRightUp, LeftUpDown, RightUpDown };
        }

        public static List<Pipe> Obstacles()
        {
            return new List<Pipe>
            {
                Obstacle,
                FixedVertical,
                FixedHorizontal,
                FixedRightDown,
                FixedLeftDown,
                FixedRightUp,
                FixedLeftUp
            };
        }
    }

    //public class PipeType
    //{
    //    public int Value { get; }
    //    public string GraphicPath { get; }
    //    public List<Direction> Directions { get; }
    //    public bool Replaceable { get; }
    //    public bool IsPump { get; }

    //    public PipeType(int value, string graphicPath, List<Direction> directions, bool replaceable, bool isPump = false)
    //    {
    //        Value = value;
    //        GraphicPath = graphicPath;
    //        Directions = directions;
    //        Replaceable = replaceable;
    //        IsPump = isPump;
    //    }

    //    //public Pipe Create()
    //    //{
    //    //    var graphic = LoadGraphic(GraphicPath);
    //    //    var pipe = new Pipe(graphic, Directions, this, Replaceable);
    //    //    if (IsPump)
    //    //    {
    //    //        pipe.SetAsPump();
    //    //    }
    //    //    return pipe;
    //    //}

    //    //private Sprite LoadGraphic(string path)
    //    //{
    //    //    // Load graphic from resources or asset bundle
    //    //    return Resources.Load<Sprite>(path);
    //    //}
    //}
}