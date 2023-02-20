using System;
using BrunoMikoski.Pahtfinding;
using BrunoMikoski.Pahtfinding.Grid;

namespace BrunoMikoski.Events
{
    public static class EventsDispatcher
    {
        public static class Grid
        {
            public static Action<Tile> OnTileTypeChangedEvent;


            public static void DispatchOnTileTypeChangedEvent( Tile targetTile )
            {
                if ( OnTileTypeChangedEvent != null )
                    OnTileTypeChangedEvent( targetTile );
            }
        }

    }
}
