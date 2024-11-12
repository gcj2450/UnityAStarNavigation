using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;
namespace BrightPipe
{
    public class GameScreen : MonoBehaviour
    {
        private int score;
        private int level;
        private List<Achievement> achievements;
        private const int PUMP_INTERVAL_DEFAULT = 4000;
        private const int CELL_DIMENSIONS = 50;
        private const int PIPES_PLACED_BEFORE_PLAY = 5;
        private const int PASS_LEVEL_SCORE = 200;
        private const int ACHIEVEMENT_LENGTH = 3000;

        private int achievementTimeout;
        private Achievement lastAchievement;

        private Vector2 GRID_LOCATION = new Vector2(30, 35);
        private Vector2 PIPE_SELECTION_LOCATION = new Vector2(53, 300);
        private Vector2 SETTINGS_LOCATION = new Vector2(0, 10);

        //private SettingsButton settingsButton;
        private bool playing;
        private int pipesPlaced;
        private int elapsedSinceLastPump;

        //private PipeSelectionQueue pipeSelection;
        private Grid grid;
        private List<Pipe> drains;
        private Pipe pump;

        private const int PUMP_INTERVAL_MAX = 7000;
        private const int PUMP_INTERVAL_MIN = 3000;

        //private ScreenController screenController;
        private int width;
        private int height;
        //private Control lastActiveControl;

        private Pipe draggingPipe;
        private Vector2 draggingLocation;
        private List<Pipe> newlyFilledPipes;


        private void Start()
        {
            NewGame(this.width, this.height, this.score - this.pipesPlaced * 2, this.level + 1);
        }

        public void NewGame(int width, int height, /*ScreenController screenController,*/ int score = 0, int level = 1/*, List<Achievement> achievements = null*/)
        {
            this.score = score;
            this.level = level;
            //this.achievements = achievements ?? new List<Achievement>();

            this.achievementTimeout = 0;
            this.lastAchievement = null;

            this.playing = true;
            this.pipesPlaced = 0;
            this.elapsedSinceLastPump = PUMP_INTERVAL_DEFAULT;

            //this.pipeSelection = new PipeSelectionQueue();
            this.grid = new Grid(CELL_DIMENSIONS, 6, 7);
            this.drains = new List<Pipe>();
            this.pump = Pipes.Pump.Create();

            // Center GRID_LOCATION and PIPE_SELECTION_LOCATION
            this.GRID_LOCATION.x = (width - this.grid.GetBounds().width) / 2;
            //this.PIPE_SELECTION_LOCATION.x = (width - this.pipeSelection.GetBounds().width) / 2;
            //this.PIPE_SELECTION_LOCATION.y = this.GRID_LOCATION.y + this.grid.GetBounds().height + 20;

            // Calculate numDrains and pump interval
            int numDrains = Mathf.FloorToInt(Mathf.Min(5, ((this.level - 1) / 3) + 1));
            //this.PUMP_INTERVAL_DEFAULT = PUMP_INTERVAL_MIN + Mathf.FloorToInt((PUMP_INTERVAL_MAX - PUMP_INTERVAL_MIN) * ((3 - ((this.level - 1) % 3)) / 3.0f));

            // Initialize settings button with callback to screenController
            //this.screenController = screenController;
            //this.settingsButton = new SettingsButton(() =>
            //    screenController.SetScreen(new SettingsScreen(width, height, screenController, this, true))
            //);

            // Start generating level and setup for dragging
            this.GenerateLevel(numDrains, 1.4143f);
            this.draggingPipe = null;
            this.draggingLocation = Vector2.zero;
            this.newlyFilledPipes = new List<Pipe>();

            // Fill pipe selection and set screen dimensions
            //this.fillPipeSelection();
            this.width = width;
            this.height = height;
            //this.lastActiveControl = null;

            this.SETTINGS_LOCATION.x = this.GRID_LOCATION.x + (CELL_DIMENSIONS * 6);

            // Add an achievement if on level 5
            //if (this.level == 5)
            //{
            //    this.addAchievement(Achievement.FiveRounds);
            //}
        }

        //void addAchievement(Achievement achievement)
        //{
        //    this.achievements.Add(achievement);
        //    //lowLag.play('sound/achievementUnlocked.wav');

        //    this.achievementTimeout = ACHIEVEMENT_LENGTH;
        //    this.lastAchievement = achievement;
        //}

        public bool RandomizePlacement(Pipe pipeObject, float? minDistance = null, List<Pipe> avoidSet = null, bool clearConnections = false)
        {
            pipeObject.Detach();
            var gridBounds = grid.GetCellBounds();
            Vector2Int location;

            for (int attempt = 0; attempt < 100000; attempt++)
            {
                location = new Vector2Int(
                    Mathf.FloorToInt(Random.Range(0, gridBounds.width)),
                    Mathf.FloorToInt(Random.Range(0, gridBounds.height))
                );

                if (grid.GetPipe(location) != null)
                    continue;

                var connectionDirections = pipeObject.GetDirections();

                if (clearConnections)
                {
                    bool canPlace = true;
                    foreach (var direction in connectionDirections)
                    {
                        Vector2Int dirLocation = location + direction.Delta;
                        if (!grid.GetCellBounds().Contains(dirLocation) || grid.GetPipe(dirLocation) != null)
                        {
                            canPlace = false;
                            break;
                        }
                    }
                    if (!canPlace)
                        continue;
                }

                if (minDistance.HasValue && avoidSet != null)
                {
                    var gridPipes = grid.GetPipes();
                    bool isFarEnough = true;

                    foreach (var gridPipe in gridPipes)
                    {
                        if (!avoidSet.Contains(gridPipe.Type))
                            continue;

                        if ((gridPipe.GetLocation() - location).magnitude < minDistance.Value)
                        {
                            isFarEnough = false;
                            break;
                        }
                    }
                    if (!isFarEnough)
                        continue;
                }

                grid.SetPipe(location, pipeObject);
                return true;
            }

            return false;
        }

        public void GenerateLevel(int numDrains, float minDistance)
        {
            // Seed random based on the current level
            Random.InitState(level);
            Debug.Log("GenerateLevel");
            // Get complex pipe types and initialize split pipes
            List<Pipe> complexPipes = Pipes.ComplexValues();
            List<Pipe> splits = new List<Pipe>();

            for (int i = 0; i < numDrains - 1; i++)
            {
                Pipe splitPipe = complexPipes[Random.Range(0, complexPipes.Count)].Create();
                splits.Add(splitPipe);
            }

            // Initialize drain pipes
            drains = new List<Pipe>();
            for (int i = 0; i < numDrains; i++)
            {
                drains.Add(Pipes.Drain.Create());
            }

            // Attempt to place pipes and verify solvability
            do
            {
                RandomizePlacement(pump, null, new List<Pipe> { Pipes.Pump });

                // Place drains with a minimum distance constraint
                foreach (var drain in drains)
                {
                    RandomizePlacement(drain, minDistance, new List<Pipe> { Pipes.Drain }, false);
                }

                // Place split pipes with connections cleared
                foreach (var split in splits)
                {
                    RandomizePlacement(split, 1.4143f, Pipes.ComplexValues().Concat(new List<Pipe> { Pipes.Pump }).ToList(), true);
                }

                // Ensure drains are not aligned in the same row or column as the pump
                bool invalidPlacement = false;
                foreach (var drain in drains)
                {
                    Vector2Int delta = drain.GetLocation() - pump.GetLocation();
                    if (delta.x == 0 || delta.y == 0)
                    {
                        invalidPlacement = true;
                        break;
                    }
                }

                if (invalidPlacement) continue;

            } while (!IsLevelSolvable());
        }

        bool IsLevelSolvable()
        {
            var pathFinder = new AStarPathFinder(this.grid);

            var solvable = true;
            for (var i = 0; i < this.drains.Count; i++)
            {
                solvable = solvable && pathFinder.FindPath(this.pump.GetLocation(), this.drains[i].GetLocation(), this.pump.GetDirections()[0]) != null;
            }

            return solvable;
        }

        //void refreshPipeSelection()
        //{
        //    this.pipeSelection.clear();
        //    this.fillPipeSelection();
        //}

        //void fillPipeSelection()
        //{
        //    var pipes = Pipes.Values(false);
        //    var containedPipes = new List<Pipe>();

        //    var queuePipes = this.pipeSelection.getPipes();

        //    for (var i = 0; i < queuePipes.length; i++)
        //        containedPipes.Add(queuePipes[i].type);

        //    for (var i = 0; i < pipes.Count; i++)
        //    {
        //        if (!containedPipes.Contains(pipes[i]))
        //        {
        //            this.pipeSelection.pushPipe(pipes[i].Create());
        //        }
        //    }
        //}


        //public void ShiftInPipe()
        //{
        //    List<Pipe> pipes = Pipes.Values(false); 

        //    // Count the number of usable pipes in the selection queue
        //    int containedUsablePipeCount = 0;
        //    var containedPipes = pipeSelection.GetPipes();

        //    // Iterate through the selection queue to check usability (ignoring first and last items)
        //    for (int i = 1; i < containedPipes.length - 1; i++)
        //    {
        //        if (IsPipeUsable(containedPipes[i].Type))
        //            containedUsablePipeCount++;
        //    }

        //    // Select a random pipe from available types
        //    Pipe selectedPipeType = pipes[Random.Range(0, pipes.Count)];

        //    // If no usable pipes are present, ensure we add at least one usable pipe
        //    if (containedUsablePipeCount == 0)
        //    {
        //        Pipe usefulPipe = GetUsefulPipe();
        //        selectedPipeType = usefulPipe ?? selectedPipeType;
        //    }

        //    // Add the selected pipe type to the selection queue
        //    pipeSelection.ShiftIn(selectedPipeType.Create());
        //}

        /**
         * Gets a useful pipe.
         * @returns {Pipe} A pipe that is useable.
         */
        //public Pipe GetUsefulPipe()
        //{
        //    List<Pipe> pipes = Pipes.Values(false); // Assuming Pipes.Values() returns a list of available pipe types
        //    Pipe selectedPipe = null;

        //    for (int i = 0; i < 1000; i++)
        //    {
        //        selectedPipe = pipes[Random.Range(0, pipes.Count)];
        //        if (IsPipeUsable(selectedPipe))
        //            return selectedPipe;
        //    }

        //    return null;
        //}


        //public bool IsPipeUsable(Pipe pipeType)
        //{
        //    var pipe = pipeType.Create();
        //    pipe.Fill(null);
        //    var starts = new List<Pipe>();

        //    if (newlyFilledPipes.Count > 0)
        //    {
        //        starts.AddRange(newlyFilledPipes);
        //    }
        //    else
        //    {
        //        starts.Add(pump);
        //    }

        //    var pathFinder = new AStarPathFinder(grid);

        //    foreach (var start in starts)
        //    {
        //        var drainDirections = GetDrainDirections(start);

        //        foreach (var direction in drainDirections)
        //        {
        //            var newLocation = start.GetLocation()+(direction.Delta);

        //            if (grid.GetPipe(newLocation) != null || !grid.GetCellBounds().Contains(newLocation))
        //                continue;

        //            grid.SetPipe(newLocation, pipe);

        //            bool useable = false;

        //            if (pipe.GetConnections().Contains(start))
        //            {
        //                var newDrainDirections = GetDrainDirections(pipe);

        //                foreach (var newDirection in newDrainDirections)
        //                {
        //                    var testLocation = pipe.GetLocation()+(newDirection.Delta);

        //                    if (!grid.GetCellBounds().Contains(testLocation))
        //                        continue;

        //                    foreach (var drain in drains)
        //                    {
        //                        if (drain.IsFilled())
        //                            continue;

        //                        useable = useable || pathFinder.FindPath(pipe.GetLocation(), drain.GetLocation(), newDirection) != null;
        //                    }
        //                }
        //            }

        //            grid.ClearPipe(newLocation);

        //            if (useable)
        //                return true;
        //        }
        //    }

        //    return false;
        //}

        //void loseGame(leaked)
        //{
        //    this.playing = false;
        //    this.screenController.setScreen(new GameOverScreen(this.width, this.height, this.screenController, this.score));
        //}

        ///**
        // * Updates game screen logic. Controls pumping.
        // * @param {Number} deltaTime The delta in milliseconds from last call to update.
        // */
        //void update(deltaTime)
        //{
        //    this.searchForEasterEgg();

        //    if (!this.playing)
        //        return;

        //    this.achievementTimeout = Math.max(0, this.achievementTimeout - deltaTime);

        //    var pipes = this.grid.getPipes();
        //    for (var i = 0; i < pipes.length; i++)
        //    {
        //        if (pipes[i].isLeaking() && pipes[i] !== this.drain && pipes[i] !== this.drain)
        //        {
        //            this.loseGame(true);
        //        }
        //    }

        //    if (true)
        //    {
        //        this.elapsedSinceLastPump += deltaTime;

        //        if (this.elapsedSinceLastPump > this.PUMP_INTERVAL)
        //        {
        //            this.elapsedSinceLastPump = 0;
        //            this.newlyFilledPipes = this.grid.pump();

        //            var filledDrains = 0;
        //            for (var i = 0; i < this.drains.length; i++)
        //            {
        //                if (this.drains[i].isFilled())
        //                    filledDrains++;
        //            }


        //            if (this.newlyFilledPipes.length === 0 && filledDrains > 0)
        //            {
        //                this.loseGame(false);
        //            }

        //            if (filledDrains === this.drains.length)
        //            {
        //                lowLag.play('sound/winsound.wav');

        //                this.screenController.setScreen(new GameScreen(this.width, this.height, this.screenController, this.score + this.PASS_LEVEL_SCORE - this.pipesPlaced * 2, this.level + 1, this.achievements));
        //                this.playing = false;
        //            }

        //            this.fillPipeSelection();
        //        }
        //    }
        //}

        ///**
        // * Searches for the easter egg condition
        // */

        //void searchForEasterEgg()
        //{
        //    var pipes = this.grid.getPipes();
        //    for (var i = 0; i < pipes.length; i++)
        //    {
        //        var pipe = pipes[i];

        //        if (pipe.type === Pipes.RightDown)
        //        {
        //            var right = this.grid.getPipe(pipe.getLocation().add(Direction.Right.delta));
        //            var down = this.grid.getPipe(pipe.getLocation().add(Direction.Down.delta));
        //            var accross = this.grid.getPipe(pipe.getLocation().add(Direction.Down.delta.add(Direction.Right.delta)));

        //            if (right === null || down === null || accross === null)
        //                continue;

        //            if (right.type === Pipes.LeftDown && down.type === Pipes.RightUp && accross.type === Pipes.LeftUp)
        //            {
        //                if (!pipe.isFilled())
        //                {
        //                    pipe.setAsPump();
        //                }

        //                if (this.achievements.indexOf(Achievement.Infinity) < 0)
        //                {
        //                    this.addAchievement(Achievement.Infinity);
        //                }
        //            }
        //        }
        //    }
        //}

        ///**
        // * Draws game screen.
        // * @param {Context2D} g Graphics context object through which to draw.
        // * @param {Number} x The draw offset
        // * @param {Number} y The draw offset
        // */
        //void draw(g, x, y)
        //{
        //    g.font = "15px Trade Winds";
        //    g.fillText("Number of pipes used: " + this.pipesPlaced, this.GRID_LOCATION.x, 23); // missing the function that counts the number of pipes used

        //    g.fillText("Level: " + this.level, this.GRID_LOCATION.x + 220, 23); // missing the function that counts the number of pipes used

        //    this.grid.draw(g, this.GRID_LOCATION.x + x, this.GRID_LOCATION.y + y);
        //    this.pipeSelection.draw(g, this.PIPE_SELECTION_LOCATION.x + x, this.PIPE_SELECTION_LOCATION.y + y);
        //    this.settingsButton.draw(g, this.SETTINGS_LOCATION.x + x, this.SETTINGS_LOCATION.y + y);

        //    this.drawWater(g, x, y);

        //    if (this.draggingPipe !== null)
        //        this.draggingPipe.draw(g, x + this.draggingLocation.x, y + this.draggingLocation.y);


        //    if (this.achievementTimeout > 0)
        //    {
        //        g.font = "30px Trade Winds";
        //        var str = "Achievement: " + this.lastAchievement.name;
        //        var txtDim = g.measureText(str);
        //        g.fillText(str, (this.width - txtDim.width) / 2, this.GRID_LOCATION.y + this.CELL_DIMENSIONS * 2); // missing the function that counts the number of pipes used 
        //        this.lastAchievement.graphic.draw(g, (this.width - this.lastAchievement.graphic.getBounds().width / 2) / 2, this.GRID_LOCATION.y + this.CELL_DIMENSIONS * 4);
        //    }
        //}

        ///**
        // * On mouse down event handler.
        // * @param {Vector} location Location of mouse cursor during event.
        // */
        //void onMouseDown(location)
        //{
        //    var selectionBounds = this.pipeSelection.getBounds().add(this.PIPE_SELECTION_LOCATION);
        //    this.onMouseMove(location);

        //    if (selectionBounds.contains(location))
        //    {
        //        var pipe = this.pipeSelection.popPipe(location.difference(this.PIPE_SELECTION_LOCATION));
        //        this.draggingPipe = pipe;
        //        this.draggingLocation = location;
        //        lowLag.play('sound/Sound 2.wav');
        //    }
        //    if (this.lastActiveControl !== null)
        //        this.lastActiveControl.onMouseDown(location);
        //}

        ///**
        // * On mouse up event handler.
        // * @param {Vector} location Location of mouse cursor during event.
        // */
        //void onMouseUp(location)
        //{
        //    if (this.draggingPipe !== null)
        //    {
        //        var gridBounds = this.grid.getBounds().add(this.GRID_LOCATION);

        //        if (gridBounds.contains(location))
        //        {
        //            var gridCoord = this.grid.screenToGrid(location.difference(this.GRID_LOCATION));

        //            var oldPipe = this.grid.getPipe(gridCoord);

        //            if (oldPipe !== null && !oldPipe.canReplace())
        //            {
        //                this.pipeSelection.pushPipe(this.draggingPipe);
        //                lowLag.play('sound/error.wav');
        //            }
        //            else
        //            {
        //                this.grid.setPipe(gridCoord, this.draggingPipe);
        //                this.pipesPlaced++;
        //                this.draggingPipe = null;
        //                lowLag.play('sound/Sound 3.wav');

        //                if (this.pipesPlaced == 10 && this.achievements.indexOf(Achievement.TenPipes) < 0)
        //                    this.addAchievement(Achievement.TenPipes);
        //            }
        //        }
        //        else
        //        {
        //            this.pipeSelection.pushPipe(this.draggingPipe);
        //            lowLag.play('sound/error.wav');
        //        }
        //    }

        //    this.draggingPipe = null;

        //    if (this.lastActiveControl !== null)
        //        this.lastActiveControl.onMouseUp(location);
        //}

        ///**
        // * Handles mouse move events.
        // * @param {type} location The current location of the mouse.
        // */
        //void onMouseMove(Vector2Int location)
        //{
        //    var selectedControl = null;

        //    if (this.draggingPipe !== null)
        //        this.draggingLocation = location;

        //    if (this.settingsButton.getBounds().add(this.SETTINGS_LOCATION).contains(location))
        //        selectedControl = this.settingsButton;

        //    if (selectedControl !== this.lastActiveControl)
        //    {
        //        if (this.lastActiveControl !== null)
        //            this.lastActiveControl.onMouseLeave();

        //        if (selectedControl !== null)
        //            selectedControl.onMouseEnter();
        //    }

        //    this.lastActiveControl = selectedControl;
        //}

        ///**
        // * Draws game water.
        // * @param {Context2D} g Graphics context object through which to draw.
        // * @param {Number} x The draw offset
        // * @param {Number} y The draw offset
        // */

        //void drawWater(g, x, y)
        //{

        //    var pipes = this.grid.getPipes();

        //    for (var i = 0; i < pipes.length; i++)
        //        this.drawPipeWater(g, x, y, pipes[i]);
        //}

        ///**
        // * Draws water effects for individual pipe.
        // * @param {Context2D} g Graphics context object through which to draw.
        // * @param {Number} x The draw offset
        // * @param {Number} y The draw offset
        // * @param {Pipe} pipe The pipe to draw water effects for.
        // */
        //void drawPipeWater(g, x, y, pipe)
        //{
        //    if (!pipe.isFilled() || pipe.type == Pipes.Drain)
        //        return;

        //    var inProgression = Math.min(1.0, this.elapsedSinceLastPump / (this.PUMP_INTERVAL / 2));
        //    var outProgression = Math.max(0, this.elapsedSinceLastPump / (this.PUMP_INTERVAL / 2) - inProgression);

        //    if (this.newlyFilledPipes.indexOf(pipe) === -1)
        //        inProgression = outProgression = 1.0;

        //    var pipeCentreLocation = this.GRID_LOCATION.add(this.grid.gridToScreen(pipe.getLocation().add(new Vector(0.5, 0.5))));

        //    var fillDirections = this.getFillDirections(pipe);
        //    var drainDirections = this.getDrainDirections(pipe);

        //    g.lineWidth = 8;
        //    g.strokeStyle = "#003399";

        //    for (var i = 0; i < fillDirections.Count; i++)
        //    {
        //        var start = pipeCentreLocation.add(fillDirections[i].delta.scale(this.grid.getCellDimensions() / 2));
        //        var end = pipeCentreLocation.add(fillDirections[i].delta.inverse().scale(g.lineWidth / 2));
        //        var diff = end.difference(start);
        //        var current = start.add(diff.scale(inProgression));

        //        g.beginPath();
        //        g.moveTo(start.x, start.y);
        //        g.lineTo(current.x, current.y);
        //        g.stroke();
        //    }

        //    for (var i = 0; i < drainDirections.Count; i++)
        //    {
        //        var start = pipeCentreLocation;
        //        var end = pipeCentreLocation.add(drainDirections[i].Delta.scale(this.grid.getCellDimensions() / 2));
        //        var diff = end.difference(start);

        //        var current = start.add(diff.scale(outProgression));

        //        g.beginPath();
        //        g.moveTo(start.x, start.y);
        //        g.lineTo(current.x, current.y);
        //        g.stroke();
        //    }

        //    g.beginPath();
        //}

        List<Direction> getFillDirections(Pipe pipe)
        {
            var connections = pipe.GetConnections(null);
            List<Direction> fillDirections = new List<Direction>();

            for (var i = 0; i < connections.Count; i++)
            {
                if (connections[i].IsFilled())
                {
                    var delta = connections[i].GetLocation() - pipe.GetLocation();
                    fillDirections.Add(Direction.FromVector(delta));
                }
            }

            return fillDirections;
        }

        List<Direction> GetDrainDirections(Pipe pipe)
        {
            var fillDirections = this.getFillDirections(pipe);
            var allDirections = pipe.GetDirections();
            List<Direction> drainDirections = new List<Direction>();

            for (var i = 0; i < allDirections.Count; i++)
            {
                if (fillDirections.IndexOf(allDirections[i]) == -1)
                    drainDirections.Add(allDirections[i]);
            }

            return drainDirections;
        }

    }
}